/*
 * Created by SharpDevelop.
 * User: Shazzy
 * Date: 4/12/2006
 * Time: 6:26 AM
 * 
 * 
 */

using System;
using Server;
using Server.Items;

namespace Server.Mobiles
{
	[CorpseName( "an evil spellbook's corpse" )]
	public class EvilSpellbook : BaseCreature
	{
		public override bool IsScaredOfScaryThings => false; 
		public override bool IsScaryToPets => true; 
		[Constructable]
		public EvilSpellbook() : base( AIType.AI_NecroMage, FightMode.Weakest, 10, 1, 0.1, 0.2 )
		{
			Name = "an evil spellbook";
			Body = 985; // 0x3D9
			BaseSoundID = 466;

			SetStr( 400, 500 );
			SetDex( 300, 350 );
			SetInt( 900, 950 );

			SetHits( 800, 1000 );

			SetDamage( 25, 30 );

			SetDamageType( ResistanceType.Cold, 60 );
			SetDamageType( ResistanceType.Physical, 40 );

			SetResistance( ResistanceType.Physical, 60, 70 );
			SetResistance( ResistanceType.Fire, 60, 70 );
			SetResistance( ResistanceType.Poison, 60, 70 );
			SetResistance( ResistanceType.Energy, 60, 70 );
			SetResistance( ResistanceType.Cold, 60, 70 );

			SetSkill( SkillName.EvalInt, 120.0, 130.0 );
			SetSkill( SkillName.Magery, 115.0, 125.0 );
			SetSkill( SkillName.Meditation, 105.0, 115.0 );
			SetSkill(SkillName.SpiritSpeak, 200.0);
            SetSkill(SkillName.Necromancy, 112.6, 117.5);
			SetSkill( SkillName.MagicResist, 150.0, 175.0 );
			SetSkill( SkillName.Tactics, 90.0, 95.0 );
			SetSkill( SkillName.Wrestling, 120.0, 130.0 );

			Fame = 22000;
			Karma = -22000;
			
			SetWeaponAbility(WeaponAbility.ArmorPierce);
		}


        public override bool CanRummageCorpses => true;
        public override bool Unprovokable => true;
        public override bool BleedImmune => true;
        public override Poison PoisonImmune => Poison.Lethal;
        public override int TreasureMapLevel => 5;
        public override void GenerateLoot()
        {
            //AddLoot(LootPack.FilthyRich, 3);
            //AddLoot(LootPack.MedScrolls, 2);
            //AddLoot(LootPack.NecroRegs, 17, 24);
            //AddLoot(LootPack.RandomLootItem(new[] { typeof(LichFormScroll), typeof(PoisonStrikeScroll), typeof(StrangleScroll), typeof(VengefulSpiritScroll), typeof(WitherScroll) }, false, true));
        }
		
		public EvilSpellbook( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}
