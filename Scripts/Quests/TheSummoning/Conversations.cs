using System;
using Server.Mobiles;
namespace Server.Engines.Quests.Doom

    
        
          
    

        
        Expand All
    
    @@ -23,7 +23,7 @@ public override object Message
  
{
    public class AcceptConversation : QuestConversation
    {
        public AcceptConversation()
        {
        }
        public override object Message
        {
            get
            {
                /* You have accepted Victoria's help.  She requires 1000 Daemon
                * bones to summon the devourer.<BR><BR>
                * 
                * You may hand Victoria the bones as you collect them and she
                * will keep count of how many you have brought her.<BR><BR>
                * 
                * Daemon bones can be collected via various means throughout
                * Dungeon Doom.<BR><BR>
                * 
                * Good luck.
                */
                return 1050027;
            }
        }

    
        
          
    

        
        Expand All
    
    @@ -46,44 +46,13 @@ public override object Message
  
        public override void OnRead()
        {
            this.System.AddObjective(new CollectBonesObjective());
        }
    }
    public class VanquishDaemonConversation : QuestConversation
    {
        public VanquishDaemonConversation()
        {
        }
        public override object Message
        {
            get
            {
                /* Well done brave soul.   I shall summon the beast to the circle
                * of stones just South-East of here.  Take great care - the beast
                * takes many forms.  Now hurry...
                */
                return 1050021;
            }
        }
        public override void OnRead()
        {
            Victoria victoria = ((TheSummoningQuest)this.System).Victoria;

            if (victoria == null)
            {
                this.System.From.SendMessage("Internal error: unable to find Victoria. Quest unable to continue.");
                this.System.Cancel();
            }
            else
            {
                SummoningAltar altar = victoria.Altar;

                if (altar == null)
                {
                    this.System.From.SendMessage("Internal error: unable to find summoning altar. Quest unable to continue.");
                    this.System.Cancel();
                }
                else if (altar.Daemon == null || !altar.Daemon.Alive)
                {
                    BoneDemon daemon = new BoneDemon();

                    daemon.MoveToWorld(altar.Location, altar.Map);
                    altar.Daemon = daemon;

                    this.System.AddObjective(new VanquishDaemonObjective(daemon));
                }
                else
                {
                    victoria.SayTo(this.System.From, "The devourer has already been summoned.");

                    ((TheSummoningQuest)this.System).WaitForSummon = true;
                }
            }
        }
    }
}
