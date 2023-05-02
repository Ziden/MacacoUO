using System;
using System.Collections.Generic;
using Server.Engines.CannedEvil;
using Server.Gumps;
using Server.Items;
using Server.Services.Virtues;

namespace Server.Mobiles
{
    public abstract class BaseChampion : BaseCreature
    {
        public BaseChampion(AIType aiType)
            : this(aiType, FightMode.Closest)
        {

        }

        public BaseChampion(AIType aiType, FightMode mode)
            : base(aiType, mode, 18, 1, 0.1, 0.2)
        {
            AddItem(Decos.RandomDeco(this));
            //AddItem(Decos.RandomDeco(this));
            //AddItem(Decos.RandomDeco(this));

            AddItem(new Item(0x12D9)); // estatua
        }

        public BaseChampion(Serial serial)
            : base(serial)
        {
        }
        public override bool CanBeParagon { get { return false; } }
        public abstract ChampionSkullType SkullType { get; }
        public abstract Type[] UniqueList { get; }
        public abstract Type[] SharedList { get; }
        public abstract Type[] DecorativeList { get; }
        public abstract MonsterStatuetteType[] StatueTypes { get; }
        public virtual bool NoGoodies
        {
            get
            {
                return false;
            }
        }

        public virtual bool CanGivePowerscrolls { get { return false; } }

        public static void GivePowerScrollTo(Mobile m, Item item, BaseChampion champ)
        {
            if (m == null)	//sanity
                return;

            if (!Core.SE || m.Alive)
                m.AddToBackpack(item);
            else
            {
                if (m.Corpse != null && !m.Corpse.Deleted)
                    m.Corpse.DropItem(item);
                else
                    m.AddToBackpack(item);
            }

            if (item is PowerScrollNovo && m is PlayerMobile)
            {
                PlayerMobile pm = (PlayerMobile)m;

                for (int j = 0; j < pm.JusticeProtectors.Count; ++j)
                {
                    Mobile prot = pm.JusticeProtectors[j];

                    if (prot.Map != m.Map || prot.Murderer || prot.Criminal || !JusticeVirtue.CheckMapRegion(m, prot) || !prot.InRange(champ, 100))
                        continue;

                    int chance = 0;

                    switch (VirtueHelper.GetLevel(prot, VirtueName.Justice))
                    {
                        case VirtueLevel.Seeker:
                            chance = 5;
                            break;
                        case VirtueLevel.Follower:
                            chance = 10;
                            break;
                        case VirtueLevel.Knight:
                            chance = 20;
                            break;
                    }

                    if (chance > Utility.Random(100))
                    {
                        PowerScrollNovo powerScroll = CreateRandomPowerScroll();

                        prot.SendLocalizedMessage(1049368); // You have been rewarded for your dedication to Justice!

                        if (!Core.SE || prot.Alive)
                            prot.AddToBackpack(powerScroll);
                        else
                        {
                            if (prot.Corpse != null && !prot.Corpse.Deleted)
                                prot.Corpse.DropItem(powerScroll);
                            else
                                prot.AddToBackpack(powerScroll);
                        }
                    }
                }
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }

        public virtual Item GetArtifact()
        {
            if (!Core.AOS)
            {
                return this.CreateArtifact(this.DecorativeList);
            }
            double random = Utility.RandomDouble();
            if (0.05 >= random)
                return this.CreateArtifact(this.UniqueList);
            else if (0.15 >= random)
                return this.CreateArtifact(this.SharedList);
            else if (0.30 >= random)
                return this.CreateArtifact(this.DecorativeList);
            return null;
        }

        public Item CreateArtifact(Type[] list)
        {
            if (list.Length == 0)
                return null;

            int random = Utility.Random(list.Length);

            Type type = list[random];

            Item artifact = Loot.Construct(type);

            if (artifact is MonsterStatuette && this.StatueTypes.Length > 0)
            {
                ((MonsterStatuette)artifact).Type = this.StatueTypes[Utility.Random(this.StatueTypes.Length)];
                ((MonsterStatuette)artifact).LootType = LootType.Regular;
            }

            return artifact;
        }

        public virtual void GivePowerScrolls()
        {
            if (this.Map != Map.Felucca)
                return;

            Shard.Debug("Dropando Powerscrolls");

            List<Mobile> toGive = new List<Mobile>();
            List<DamageStore> rights = GetLootingRights();

            for (int i = rights.Count - 1; i >= 0; --i)
            {
                DamageStore ds = rights[i];

                if (ds.m_HasRight && InRange(ds.m_Mobile, 100) && ds.m_Mobile.Map == this.Map)
                    toGive.Add(ds.m_Mobile);
            }

            Shard.Debug("Pessoas pra ganhar PS: " + toGive);

            if (toGive.Count == 0)
                return;

            for (int i = 0; i < toGive.Count; i++)
            {
                Mobile m = toGive[i];

                if (!(m is PlayerMobile))
                    continue;

                bool gainedPath = false;

                int pointsToGain = 800;

                if (VirtueHelper.Award(m, VirtueName.Valor, pointsToGain, ref gainedPath))
                {
                    if (gainedPath)
                        m.SendLocalizedMessage(1054032); // You have gained a path in Valor!
                    else
                        m.SendLocalizedMessage(1054030); // You have gained in Valor!
                    //No delay on Valor gains
                }
            }

            Shard.Debug("QTD Powerscrolls: " + ChampionSystem.PowerScrollAmount);

            // Randomize - PowerScrolls
            for (int i = 0; i < toGive.Count; ++i)
            {
                int rand = Utility.Random(toGive.Count);
                Mobile hold = toGive[i];
                toGive[i] = toGive[rand];
                toGive[rand] = hold;
            }

            for (int i = 0; i < ChampionSystem.PowerScrollAmount; ++i)
            {
                Mobile m = toGive[i % toGive.Count];

                PowerScrollNovo ps = CreateRandomPowerScroll();
                m.SendLocalizedMessage(1049524); // You have received a scroll of power!

                GivePowerScrollTo(m, ps, this);
            }

            if (Core.TOL)
            {
                // Randomize - Primers
                for (int i = 0; i < toGive.Count; ++i)
                {
                    int rand = Utility.Random(toGive.Count);
                    Mobile hold = toGive[i];
                    toGive[i] = toGive[rand];
                    toGive[rand] = hold;
                }

                for (int i = 0; i < ChampionSystem.PowerScrollAmount; ++i)
                {
                    Mobile m = toGive[i % toGive.Count];

                    SkillMasteryPrimer p = CreateRandomPrimer();
                    m.SendLocalizedMessage(1156209); // You have received a mastery primer!

                    GivePowerScrollTo(m, p, this);
                }
            }

            ColUtility.Free(toGive);
        }

        public virtual void OnChampPopped(ChampionSpawn spawn)
        {
        }

        public override bool OnBeforeDeath()
        {
            if (CanGivePowerscrolls && !NoKillAwards)
            {
                Shard.Debug("Dando Power Scrols !!!");
                this.GivePowerScrolls();

                if (this.NoGoodies)
                    return base.OnBeforeDeath();
            }

            GoldShower.DoForChamp(Location, Map);

            return base.OnBeforeDeath();
        }

        public override void OnDeath(Container c)
        {
            AnuncioGump.Texto("O champion " + Name + " foi derrotado");

            if (this.Map == Map.Felucca)
            {
                //TODO: Confirm SE change or AoS one too?
                List<DamageStore> rights = GetLootingRights();
                List<Mobile> toGive = new List<Mobile>();

                for (int i = rights.Count - 1; i >= 0; --i)
                {
                    DamageStore ds = rights[i];

                    if (ds.m_HasRight)
                        toGive.Add(ds.m_Mobile);
                }

                if (SkullType != ChampionSkullType.None)
                {
                    if (toGive.Count > 0)
                        toGive[Utility.Random(toGive.Count)].AddToBackpack(new ChampionSkull(this.SkullType));
                    else
                        c.DropItem(new ChampionSkull(this.SkullType));
                }

                if (Core.SA)
                    RefinementComponent.Roll(c, 3, 0.10);
            }

            base.OnDeath(c);
        }

        private static PowerScrollNovo CreateRandomPowerScroll()
        {
            int level;
            double random = Utility.RandomDouble();

            if (0.03 >= random)
                level = 115;
            else if (0.2 >= random)
                level = 110;
            else if (0.3 >= random)
                level = 105;
            else
                level = 100;

            return PowerScroll.CreateRandomNoCraft(level, level);
        }

        private static SkillMasteryPrimer CreateRandomPrimer()
        {
            return SkillMasteryPrimer.GetRandom();
        }
    }
}
