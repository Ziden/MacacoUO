using System;
using Server.Fronteira.Talentos;
using Server.Mobiles;

namespace Server.Items
{
    /// <summary>
    /// This devastating strike is most effective against those who are in good health and whose reserves of mana are low, or vice versa.
    /// </summary>
    public class MovingBlow : WeaponAbility
    {
        public MovingBlow()
        {
        }

        public override int BaseMana
        {
            get
            {
                return 20;
            }
        }

        public override double DamageScalar
        {
            get
            {
                return 1.1;
            }
        }

        public override Talento TalentoParaUsar { get { return Talento.Hab_MovingBlow; } }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (!this.Validate(attacker) || !this.CheckMana(attacker, true))
                return;

            ClearCurrentAbility(attacker);
            attacker.SendLocalizedMessage("Voce deu um golpe rapido"); 
            Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(defender.X, defender.Y, defender.Z + 50), defender.Map), new Entity(Serial.Zero, new Point3D(defender.X, defender.Y, defender.Z + 20), defender.Map), 0xFB4, 1, 0, false, false, 0, 3, 9501, 1, 0, EffectLayer.Head, 0x100);
        }
    }
}
