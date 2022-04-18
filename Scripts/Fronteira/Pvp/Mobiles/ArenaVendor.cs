using System;
using Server;
using Server.Items;
using System.Collections.Generic;
using Server.TournamentSystem;

namespace Server.Mobiles
{
    public class ArenaVendor : BaseVendor
    {
        private List<SBInfo> m_SBInfos = new List<SBInfo>();
		protected override List<SBInfo> SBInfos{ get { return m_SBInfos; } }

        public override void InitSBInfo()
		{
            m_SBInfos.Add(new SBAlchemist(this));
            m_SBInfos.Add(new SBHerbalist());
            m_SBInfos.Add(new SBBlacksmith());
		}

        [Constructable]
        public ArenaVendor() : base ("the arena supplier")
        {
            SetSkill(SkillName.Tailoring, 85.0, 100.0);
            SetSkill(SkillName.Blacksmith, 85.0, 100.0);
            SetSkill(SkillName.Tinkering, 85.0, 100.0);
            SetSkill(SkillName.Carpentry, 85.0, 100.0);
        }

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            if (from == null || dropped == null)
                return false;

            if (dropped is Container)
            {
                Container cont = (Container)dropped;
                bool canFix = false;
                foreach (ArenaTeam team in ArenaTeam.GetTeams(from))
                {
                    if (team != null && team.Active && team.Points > 5000)
                    {
                        canFix = true;
                        break;
                    }
                }

                if (canFix)
                {
                    if (cont.Items.Count == 0)
                        SayTo(from, "An empty pack?");
                    else
                    {
                        Timer.DelayCall(TimeSpan.FromSeconds(2.0), new TimerStateCallback(TryRepair), new object[] { cont, from });
                        SayTo(from, "Let me see if I can work with anything here...");
                        return true;
                    }
                }
                else
                    SayTo(from, "You haven't the privledge to use my services.");
            }
            return base.OnDragDrop(from, dropped);    
        }

        public void TryRepair(object o)
        {
            object[] obj = (object[])o;
            Container cont = obj[0] as Container;
            Mobile from = obj[1] as Mobile;

            int repaired = 0;
            int failed = 0;
            int soundID = 0;
            double chance = 1.0;

            foreach (Item item in cont.Items)
            {
                chance = 1.0;

                if (item is BaseWeapon)
                {
                    BaseWeapon wep = (BaseWeapon)item;

                    if (wep.HitPoints >= wep.MaxHitPoints)
                        continue;

                    if (wep.HitPoints == 0)
                        chance /= 2;

                    if (Utility.RandomDouble() < chance)
                    {
                        if (CheckWeaken(wep.HitPoints, wep.MaxHitPoints))
                            wep.MaxHitPoints--;

                        soundID = 0x2A;
                        wep.HitPoints = wep.MaxHitPoints;
                        repaired++;
                    }
                    else
                        failed++;
                }
                else if (item is BaseArmor)
                {
                    BaseArmor arm = (BaseArmor)item;

                    if (arm.HitPoints >= arm.MaxHitPoints)
                        continue;

                    if (arm.HitPoints == 0)
                        chance /= 2;

                    if (Utility.RandomDouble() < chance)
                    {
                        if (CheckWeaken(arm.HitPoints, arm.MaxHitPoints))
                            arm.MaxHitPoints--;
                        
                        if (item is BaseClothing || arm.Resource >= CraftResource.RegularLeather)
                            soundID = 0x248;
                        else
                            soundID = 0x2A;
                        
                        arm.HitPoints = arm.MaxHitPoints;
                        repaired++;
                    }
                    else
                        failed++;
                }
            }

            from.Backpack.DropItem(cont);

            if (soundID > 0)
                PlaySound(soundID);

            if (repaired == 0 && failed == 0)
                SayTo(from, "I cannot repair any of those items!");
            else if (failed > 0)
            {
                SayTo(from, String.Format("I have repaired {0}, while failing to repair {1} of your items.", repaired, failed));
            }
            else
            {
                SayTo(from, String.Format("I have repaired {0} of your items.", repaired));
            }
        }

        //Taken from Repair
        private int GetWeakenChance(int curHits, int maxHits)
        {
            // 40% - (1% per hp lost) - (1% per 10 craft skill)
            return (40 + (maxHits - curHits) - 10);
        }

        private bool CheckWeaken(int curHits, int maxHits)
        {
            return (GetWeakenChance(curHits, maxHits) > Utility.Random(100));
        }

        public ArenaVendor( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
        	writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
    }
}