// Treasure Chest Pack - Version 0.99I
// By Nerun

using Server.Engines.Craft;
using Server.Engines.Points;
using Server.Mobiles;
using Server.Ziden.Kills;

namespace Server.Items
{
    // ---------- [Level 1] ----------
    // Large, Medium and Small Crate
    [FlipableAttribute(0xe3e, 0xe3f)]
    public class TreasureLevel1 : BaseTreasureChestMod
    {
        public override int DefaultGumpID { get { return 0x49; } }

        public override int GetLevel()
        {
            return 1;
        }

        [Constructable]
        public TreasureLevel1() : base(Utility.RandomList(0xE3C, 0xE3E, 0x9a9))
        {
            if (Utility.RandomDouble() < 0.1)
                this.Visible = false;
            RequiredSkill = 52;
            LockLevel = this.RequiredSkill - Utility.Random(1, 10);
            MaxLockLevel = this.RequiredSkill + 25;
            TrapType = TrapType.MagicTrap;
            TrapPower = 1 * Utility.Random(1, 25);

            AddLoot(Loot.RandomProvision());
            AddLoot(Loot.RandomProvision());
            AddLoot(Loot.RandomProvision());
            AddLoot(new Charcoal());
            if (Utility.Random(3) == 1)
                AddLoot(new BagOfReagents(15));
            if (Utility.Random(5) == 1)
                AddLoot(new BagOfNecroReagents());
            if (Utility.Random(3) == 1)
                AddLoot(new Bandage(6));
            DropItem(new Gold(30, 100));

            var t = Utility.Random(100);
            var amt = Utility.Random(50, 60);
            if (t <= 1)
            {
                DropItem(new CopperIngot(amt));
                return;
            }
            else if (t == 3)
            {
                var cloth = new Cloth(amt);
                cloth.Hue = Utility.RandomDyedHue();
                DropItem(cloth);
                return;
            }
            else if (t == 4)
            {
                DropItem(new Bolt(amt));
                DropItem(new Arrow(amt));
                return;
            }
            else if (t == 5)
            {
                DropItem(new OakBoard(amt));
                return;
            }

            DropItem(new Bolt(10));

            AddLoot(Loot.RandomWeapon());
            AddLoot(Loot.RandomArmorOrShield());
            AddLoot(Loot.RandomJewelry());
            AddLoot(Loot.RandomSeed());

            for (int i = Utility.Random(2) + 1; i > 0; i--) // random 1 to 3
                DropItem(Loot.RandomGem());
        }

        public TreasureLevel1(Serial serial) : base(serial)
        {
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
    }

    [FlipableAttribute(0xe3e, 0xe3f)]
    public class TreasureLevel0 : BaseTreasureChestMod
    {
        public override int DefaultGumpID { get { return 0x49; } }

        public override int GetLevel()
        {
            return 0;
        }


        [Constructable]
        public TreasureLevel0() : base(Utility.RandomList(0xE3C, 0xE3E, 0x9a9))
        {
            if (Utility.RandomDouble() < 0.1)
                this.Visible = false;
            RequiredSkill = 0;
            LockLevel = 40 - Utility.Random(1, 10);
            MaxLockLevel = 40 + 25;
            TrapType = TrapType.MagicTrap;
            TrapPower = 2 * Utility.Random(1, 25);
            AddLoot(new Charcoal());
            if (Utility.Random(2) == 1)
                AddLoot(new BagOfReagents(20));
            if (Utility.Random(5) == 1)
                AddLoot(new BagOfNecroReagents(20));
            DropItem(new Gold(25, 60));
            if (Utility.Random(3) == 1)
                AddLoot(new Bandage(6));
            var t = Utility.Random(100);
            var amt = 30;
            if (t <= 1)
            {
                DropItem(new CopperIngot(amt));
                return;
            }
            else if (t == 3)
            {
                var cloth = new Cloth(amt);
                cloth.Hue = Utility.RandomDyedHue();
                DropItem(cloth);
                return;
            }
            else if (t == 4)
            {
                DropItem(new Bolt(amt));
                DropItem(new Arrow(amt));
                return;
            }
            else if (t == 5)
            {
                DropItem(new OakBoard(amt));
                return;
            }



            AddLoot(Loot.RandomSeed());
            AddLoot(Loot.RandomProvision());
            AddLoot(Loot.RandomProvision());
        }

        public TreasureLevel0(Serial serial) : base(serial)
        {
        }

        public override void LockPick(Mobile from)
        {
            int exp = 20;
            base.LockPick(from);
            var pl = from as PlayerMobile;
            if (pl == null)
                return;
            from.SendMessage("Ganhou " + exp + " exp");
            PointsSystem.Exp.AwardPoints(from, exp);
            PontosPvm.DaXpElementos(pl, exp);
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
    }

    // ---------- [Level 1 Hybrid] ----------
    // Large, Medium and Small Crate
    [FlipableAttribute(0xe3e, 0xe3f)]
    public class TreasureLevel1h : BaseTreasureChestMod
    {

        public override int GetLevel()
        {
            return 1;
        }

        public override int DefaultGumpID { get { return 0x49; } }

        [Constructable]
        public TreasureLevel1h() : base(Utility.RandomList(0xE3C, 0xE3E, 0x9a9))
        {
            if (Utility.RandomDouble() < 0.1)
                this.Visible = false;
            RequiredSkill = 56;
            LockLevel = this.RequiredSkill - Utility.Random(1, 10);
            MaxLockLevel = this.RequiredSkill + 25;
            TrapType = TrapType.MagicTrap;
            TrapPower = 1 * Utility.Random(1, 25);
            if (Utility.Random(3) == 1)
                AddLoot(new Bandage(6));
            AddLoot(new Charcoal(2));
            if (Utility.Random(3) == 1)
                AddLoot(new BagOfReagents(25));
            if (Utility.Random(5) == 1)
                AddLoot(new BagOfNecroReagents());
            DropItem(new Gold(25, 70));

            var t = Utility.Random(100);
            var amt = 100;
            if (t <= 1)
            {
                DropItem(new CopperIngot(amt));
                return;
            }
            else if (t == 3)
            {
                var cloth = new Cloth(amt * 2);
                cloth.Hue = Utility.RandomDyedHue();
                DropItem(cloth);
                return;
            }
            else if (t == 4)
            {
                DropItem(new Bolt(amt));
                DropItem(new Arrow(amt));
                return;
            }
            else if (t == 5)
            {
                DropItem(new OakBoard(amt));
                return;
            }

            DropItem(Loot.RandomSeed());
            DropItem(Loot.RandomSeed());
            AddLoot(Loot.RandomProvision());
            AddLoot(Loot.RandomProvision());
            AddLoot(Loot.RandomProvision());

            DropItem(new Bolt(5));
            switch (Utility.Random(10))
            {
                case 0: DropItem(new Shoes(Utility.Random(1, 2))); break;
                case 1: DropItem(new Sandals(Utility.Random(1, 2))); break;
            }

            switch (Utility.Random(3))
            {
                case 0: DropItem(new BeverageBottle(BeverageType.Ale)); break;
                case 1: DropItem(new BeverageBottle(BeverageType.Liquor)); break;
                case 2: DropItem(new Jug(BeverageType.Cider)); break;
            }
            AddLoot(Loot.RandomSeed());
        }

        public TreasureLevel1h(Serial serial) : base(serial)
        {
        }

        public override void LockPick(Mobile from)
        {
            int exp = 50;
            base.LockPick(from);
            from.SendMessage("Ganhou " + exp + " exp");
            PointsSystem.Exp.AwardPoints(from, exp);
            PontosPvm.DaXpElementos(from as PlayerMobile, exp);
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
    }

    // ---------- [Level 2] ----------
    // Large, Medium and Small Crate
    // Wooden, Metal and Metal Golden Chest
    // Keg and Barrel
    [FlipableAttribute(0xe43, 0xe42)]
    public class TreasureLevel2 : BaseTreasureChestMod
    {
        public override int GetLevel()
        {
            return 2;
        }

        [Constructable]
        public TreasureLevel2() : base(Utility.RandomList(0xe3c, 0xE3E, 0x9a9, 0xe42, 0x9ab, 0xe40, 0xe7f, 0xe77))
        {
            if (Utility.RandomDouble() < 0.1)
                this.Visible = false;
            RequiredSkill = 72;
            LockLevel = this.RequiredSkill - Utility.Random(1, 10);
            MaxLockLevel = this.RequiredSkill + 25;
            TrapType = TrapType.MagicTrap;
            TrapPower = 2 * Utility.Random(1, 25);
            AddLoot(new Charcoal(5));
            DropItem(Loot.RandomSeed());
            DropItem(Loot.RandomSeed());
            AddLoot(Loot.RandomProvision());
            AddLoot(Loot.RandomProvision());
            AddLoot(Loot.RandomProvision());
            AddLoot(Loot.RandomProvision());
            DropItem(Loot.RandomSeed());
            if (Utility.Random(3) == 1)
                AddLoot(new Bandage(10));

            AddLoot(new BagOfReagents(30));
            if (Utility.Random(3) == 1)
                AddLoot(new BagOfNecroReagents());


            var t = Utility.Random(100);
            var amt = Utility.Random(200, 100);
            if (t <= 1)
            {
                DropItem(new CopperIngot(amt));
                //return;
            }
            else if (t == 3)
            {
                var cloth = new Cloth(amt * 2);
                cloth.Hue = Utility.RandomDyedHue();
                DropItem(cloth);
                //return;
            }
            else if (t == 4)
            {
                DropItem(new Bolt(amt));
                DropItem(new Arrow(amt));
                //return;
            }
            else if (t == 5)
            {
                DropItem(new OakBoard(amt));
                //return;
            }

            DropItem(Loot.RandomSeed());
            if (Utility.Random(10) == 1)
            {
                DropItem(DefAlchemy.GetRandomRecipe());
            }
            if (Utility.Random(5) == 1)
                DropItem(DefCookingExp.GetReceitaRandom());
            DropItem(Loot.RandomSeed());
            DropItem(new Gold(300, 400));
            DropItem(new Arrow(10));
            DropItem(Loot.RandomPotion());
            for (int i = Utility.Random(1, 2); i > 1; i--)
            {
                Item ReagentLoot = Loot.RandomReagent();
                ReagentLoot.Amount = Utility.Random(1, 2);
                DropItem(ReagentLoot);
            }
            if (Utility.RandomBool()) //50% chance
                for (int i = Utility.Random(8) + 1; i > 0; i--)
                    DropItem(Loot.RandomScroll(0, 39, SpellbookType.Regular));


        }

        public TreasureLevel2(Serial serial) : base(serial)
        {
        }

        public override void LockPick(Mobile from)
        {
            int exp = 100;
            base.LockPick(from);
            from.SendMessage("Ganhou " + exp + " exp");
            PointsSystem.Exp.AwardPoints(from, exp);
            PontosPvm.DaXpElementos(from as PlayerMobile, exp);
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
    }

    // ---------- [Level 3] ----------
    // Wooden, Metal and Metal Golden Chest
    [FlipableAttribute(0x9ab, 0xe7c)]
    public class TreasureLevel3 : BaseTreasureChestMod
    {
        public override int DefaultGumpID { get { return 0x4A; } }

        public override int GetLevel()
        {
            return 3;
        }


        [Constructable]
        public TreasureLevel3() : base(Utility.RandomList(0x9ab, 0xe40, 0xe42))
        {
            if (Utility.RandomDouble() < 0.1)
                this.Visible = false;
            AddLoot(new BagOfReagents(30));
            AddLoot(new BagOfNecroReagents());
            AddLoot(new Charcoal(10));
            var t = Utility.Random(100);
            var amt = Utility.Random(300, 200);
            if (t <= 1)
            {
                DropItem(new CopperIngot(amt));

            }
            else if (t == 3)
            {
                var cloth = new Cloth(amt * 2);
                cloth.Hue = Utility.RandomDyedHue();
                DropItem(cloth);

            }
            else if (t == 4)
            {
                DropItem(new Bolt(amt));
                DropItem(new Arrow(amt));

            }
            else if (t == 5)
            {
                DropItem(new OakBoard(amt));

            }

            DropItem(Loot.RandomSeed());
            DropItem(Loot.RandomSeed());
            DropItem(Loot.RandomSeed());
            DropItem(Loot.RandomSeed());
            DropItem(Loot.RandomSeed());
            DropItem(Loot.RandomSeed());
            DropItem(Loot.RandomSeed());
            DropItem(Loot.RandomSeed());
            DropItem(Loot.RandomSeed());
            DropItem(Loot.RandomSeed());
            DropItem(DefAlchemy.GetRandomRecipe());
            DropItem(DefCookingExp.GetReceitaRandom());

            RequiredSkill = 72;
            LockLevel = this.RequiredSkill - Utility.Random(1, 20);
            MaxLockLevel = this.RequiredSkill + 25;
            TrapType = TrapType.MagicTrap;
            TrapPower = 2 * Utility.Random(1, 35);

            DropItem(new Gold(700, 900));
            DropItem(new Arrow(10));

            for (int i = Utility.Random(1, 3); i > 1; i--)
            {
                Item ReagentLoot = Loot.RandomReagent();
                ReagentLoot.Amount = Utility.Random(1, 9);
                DropItem(ReagentLoot);
            }

            for (int i = Utility.Random(1, 3); i > 1; i--)
                DropItem(Loot.RandomPotion());

            if (0.67 > Utility.RandomDouble()) //67% chance = 2/3
                for (int i = Utility.Random(12) + 1; i > 0; i--)
                    DropItem(Loot.RandomScroll(0, 47, SpellbookType.Regular));


            for (int i = Utility.Random(1, 3); i > 1; i--)
                DropItem(Loot.RandomWand());


            // Magical ArmorOrWeapon
            for (int i = Utility.Random(1, 3); i > 1; i--)
            {
                Item item = Loot.RandomArmorOrShieldOrWeapon();

                if (!Core.AOS)
                {
                    if (item is BaseWeapon)
                    {
                        BaseWeapon weapon = (BaseWeapon)item;
                        weapon.DamageLevel = (WeaponDamageLevel)Utility.Random(3);
                        weapon.AccuracyLevel = (WeaponAccuracyLevel)Utility.Random(3);
                        weapon.DurabilityLevel = (WeaponDurabilityLevel)Utility.Random(3);
                        weapon.Quality = ItemQuality.Normal;
                    }
                    else if (item is BaseArmor)
                    {
                        BaseArmor armor = (BaseArmor)item;
                        armor.ProtectionLevel = (ArmorProtectionLevel)Utility.Random(3);
                        armor.Durability = (ArmorDurabilityLevel)Utility.Random(3);
                        armor.Quality = ItemQuality.Normal;
                    }
                }
                else
                    AddLoot(item);
            }

            for (int i = Utility.Random(1, 2); i > 1; i--)
                AddLoot(Loot.RandomClothing());

            for (int i = Utility.Random(1, 2); i > 1; i--)
                AddLoot(Loot.RandomJewelry());

            // Magic clothing (not implemented)

            // Magic jewelry (not implemented)
        }

        public override void LockPick(Mobile from)
        {
            int exp = 300;
            base.LockPick(from);
            from.SendMessage("Ganhou " + exp + " exp");
            PointsSystem.Exp.AwardPoints(from, exp);
            PontosPvm.DaXpElementos(from as PlayerMobile, exp);
        }

        public TreasureLevel3(Serial serial) : base(serial)
        {
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
    }

    // ---------- [Level 4] ----------
    // Wooden, Metal and Metal Golden Chest
    [FlipableAttribute(0xe41, 0xe40)]
    public class TreasureLevel4 : BaseTreasureChestMod
    {
        public override int GetLevel()
        {
            return 4;
        }

        public override void LockPick(Mobile from)
        {
            int exp = 500;
            base.LockPick(from);
            from.SendMessage("Ganhou " + exp + " exp");
            PointsSystem.Exp.AwardPoints(from, exp);
            PontosPvm.DaXpElementos(from as PlayerMobile, exp);
        }


        [Constructable]
        public TreasureLevel4() : base(Utility.RandomList(0xe40, 0xe42, 0x9ab))
        {
            if (Utility.RandomDouble() < 0.1)
                this.Visible = false;
            RequiredSkill = 100;
            LockLevel = this.RequiredSkill - Utility.Random(1, 10);
            MaxLockLevel = this.RequiredSkill + 25;
            TrapType = TrapType.MagicTrap;
            TrapPower = 4 * Utility.Random(10, 17);
            AddLoot(new Charcoal(25));
            var t = Utility.Random(100);
            var amt = Utility.Random(25, 25);
            if (t <= 5)
            {
                DropItem(new LazuritaIngot(amt * 2));
            }
            else if (t <= 7)
            {
                var cloth = new Cloth(amt * 4);
                cloth.Hue = Utility.RandomDyedHue();
                DropItem(cloth);
            }
            else if (t <= 15)
            {
                DropItem(new Bolt(amt));
                DropItem(new Arrow(amt));
            }
            else if (t <= 25)
            {
                DropItem(new OakBoard(amt));
            }
            else if (t <= 27)
            {
                DropItem(new YewBoard(amt));
            }
            else if (t <= 30)
            {
                DropItem(new BeriloIngot(amt));
            }

            if (Utility.RandomDouble() < 0.2)
                DropItem(new PotionKeg());

            if (Utility.RandomDouble() < 0.1)
                DropItem(Loot.RandomTalisman());

            AddLoot(new BagOfReagents());
            AddLoot(new BagOfNecroReagents());

            DropItem(Loot.RandomSeed());
            DropItem(Loot.RandomSeed());
            DropItem(Loot.RandomSeed());
            DropItem(Loot.RandomSeed());
            DropItem(Loot.RandomSeed());
            DropItem(Loot.RandomSeed());
            DropItem(Loot.RandomSeed());
            DropItem(Loot.RandomSeed());
            DropItem(Loot.RandomSeed());
            DropItem(Loot.RandomSeed());
            DropItem(Loot.RandomSeed());
            DropItem(DefAlchemy.GetRandomRecipe());
            DropItem(DefCookingExp.GetReceitaRandom());


            DropItem(new Gold(1000, 1210));
            DropItem(new BlankScroll(Utility.Random(1, 4)));

            for (int i = Utility.Random(1, 4); i > 1; i--)
            {
                Item ReagentLoot = Loot.RandomReagent();
                ReagentLoot.Amount = Utility.Random(6, 12);
                DropItem(ReagentLoot);
            }

            for (int i = Utility.Random(1, 4); i > 1; i--)
                DropItem(Loot.RandomPotion());

            if (0.75 > Utility.RandomDouble()) //75% chance = 3/4
                for (int i = Utility.RandomMinMax(8, 16); i > 0; i--)
                    DropItem(Loot.RandomScroll(0, 47, SpellbookType.Regular));

            if (0.75 > Utility.RandomDouble()) //75% chance = 3/4
                for (int i = Utility.RandomMinMax(1, 3) + 1; i > 0; i--)
                    DropItem(Loot.RandomGem());

            for (int i = Utility.Random(1, 4); i > 1; i--)
                DropItem(Loot.RandomWand());

            // Magical ArmorOrWeapon
            for (int i = Utility.Random(1, 4); i > 1; i--)
            {
                Item item = Loot.RandomArmorOrShieldOrWeapon();

                if (!Core.AOS)
                {
                    if (item is BaseWeapon)
                    {
                        BaseWeapon weapon = (BaseWeapon)item;
                        weapon.DamageLevel = (WeaponDamageLevel)Utility.Random(4);
                        weapon.AccuracyLevel = (WeaponAccuracyLevel)Utility.Random(4);
                        weapon.DurabilityLevel = (WeaponDurabilityLevel)Utility.Random(4);
                        weapon.Quality = ItemQuality.Normal;
                    }
                    else if (item is BaseArmor)
                    {
                        BaseArmor armor = (BaseArmor)item;
                        armor.ProtectionLevel = (ArmorProtectionLevel)Utility.Random(4);
                        armor.Durability = (ArmorDurabilityLevel)Utility.Random(4);
                        armor.Quality = ItemQuality.Normal;
                    }
                }
                else
                    AddLoot(item);
            }

            for (int i = Utility.Random(1, 2); i > 1; i--)
                AddLoot(Loot.RandomClothing());

            for (int i = Utility.Random(1, 2); i > 1; i--)
                AddLoot(Loot.RandomJewelry());
        }

        public TreasureLevel4(Serial serial) : base(serial)
        {
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
    }

}
