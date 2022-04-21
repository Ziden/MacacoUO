#region References
using System;
using System.Collections.Generic;

using Server.Items;
using Server.Network;
using Server.Spells;
using Server.Mobiles;
#endregion

namespace Server.SkillHandlers
{
    internal class SpiritSpeak
    {
        public static void Initialize()
        {
            SkillInfo.Table[32].Callback = OnUse;
        }

        public static Dictionary<Mobile, Timer> _Table;

        public static TimeSpan OnUse(Mobile m)
        {

            if(BleedAttack.IsBleeding(m))
            {
                m.SendMessage("Voce nao pode usar isto sangrando");
                return TimeSpan.FromSeconds(5.0);
            }

            if (m.Spell != null && m.Spell.IsCasting)
            {
                m.SendMessage("Voce ja esta conjurando algo"); // You are already casting a spell.
            }
            else if (BeginSpiritSpeak(m))
            {
                return TimeSpan.FromSeconds(5.0);
            }

            return TimeSpan.Zero;
        }

        private class SpiritSpeakTimer : Timer
        {
            private readonly Mobile m_Owner;

            public SpiritSpeakTimer(Mobile m)
                : base(TimeSpan.FromMinutes(2.0))
            {
                m_Owner = m;
                Priority = TimerPriority.FiveSeconds;
            }

            protected override void OnTick()
            {
                m_Owner.CanHearGhosts = false;
                m_Owner.SendLocalizedMessage(502445); //You feel your contact with the neitherworld fading.
            }
        }

        public static bool BeginSpiritSpeak(Mobile m)
        {
            m.RevealingAction();

            if (m.Paralyzed)
            {
                m.SendMessage("Nao pode fazer isto paralizado");
                return false;
            }

            if (_Table == null || !_Table.ContainsKey(m))
            {
                m.Freeze(TimeSpan.FromSeconds(1));

                m.Animate(AnimationType.Spell, 1);
                m.PublicOverheadMessage(MessageType.Regular, 0x3B2, 1062074, "", false); // Anh Mi Sah Ko
                m.PlaySound(0x24A);

                if (_Table == null)
                    _Table = new Dictionary<Mobile, Timer>();

                _Table[m] = new SpiritSpeakTimerNew(m);
                return true;
            }

            return false;
        }

        public static bool IsInSpiritSpeak(Mobile m)
        {
            Shard.Debug("Checando ss", m);
            if (_Table == null)
                return false;
            Timer t = null;
            if (_Table.TryGetValue(m, out t))
                return t.Running;
            return false;
        }

        public static void Remove(Mobile m)
        {
            if (_Table != null && _Table.ContainsKey(m))
            {
                if (_Table[m] != null)
                    _Table[m].Stop();

                m.SendSpeedControl(SpeedControlType.Disable);
                _Table.Remove(m);

                if (_Table.Count == 0)
                    _Table = null;
            }
        }

        public static void CheckDisrupt(Mobile m)
        {

            if (_Table != null && _Table.ContainsKey(m))
            {
                if (m is PlayerMobile)
                {
                    m.SendMessage("Sua concentracao foi interrompida"); // Your concentration is disturbed, thus ruining thy spell.
                }

                m.FixedEffect(0x3735, 6, 30);
                m.PlaySound(0x5C);

                m.NextSkillTime = Core.TickCount;

                Remove(m);
            }
        }

        private class SpiritSpeakTimerNew : Timer
        {
            public Mobile Caster { get; set; }

            public SpiritSpeakTimerNew(Mobile m)
                : base(TimeSpan.FromSeconds(1))
            {
                Start();
                Caster = m;
            }

            protected override void OnTick()
            {
                Corpse toChannel = null;
                Caster.RevealingAction();

                IPooledEnumerable eable = Caster.GetObjectsInRange(3);

                foreach (object objs in eable)
                {
                    if (objs is Corpse && !((Corpse)objs).Channeled && !((Corpse)objs).Animated)
                    {
                        toChannel = (Corpse)objs;
                        break;
                    }
                    else if (objs is Server.Engines.Khaldun.SageHumbolt)
                    {
                        if (((Server.Engines.Khaldun.SageHumbolt)objs).OnSpiritSpeak(Caster))
                        {
                            eable.Free();
                            SpiritSpeak.Remove(Caster);
                            Stop();
                            return;
                        }
                    }
                }

                eable.Free();


                if (Caster.Paralyzed)
                {
                    SpiritSpeak.Remove(Caster);
                    Stop();
                    return;
                }

                int max, min, mana;
                string msg;

                if (toChannel != null)
                {
                    min = 1 + (int)(Caster.Skills[SkillName.SpiritSpeak].Value * 0.2);
                    max = min + 8;
                    mana = 0;
                    msg = "Voce canaliza suas energias na alma do corpo proximo"; // You channel energy from a nearby corpse to heal your wounds.
                }
                else
                {
                    min = 1 + (int)(Caster.Skills[SkillName.SpiritSpeak].Value * 0.05);
                    max = min + 15;
                    mana = 10;
                    msg = "Voce canaliza sua propria energia para se curar"; // You channel your own spiritual energy to heal your wounds.
                }

                if (Caster.Mana < mana)
                {
                    Caster.SendMessage("Voce nao tem mana suficiente"); // You lack the mana required to use this skill.
                }
                else
                {
                    Caster.CheckSkillMult(SkillName.SpiritSpeak, 0.0, 120.0);

                    if (Utility.RandomDouble() > (Caster.Skills[SkillName.SpiritSpeak].Value / 100.0))
                    {
                        Caster.SendMessage("Voce falhou ao canalizar sua energia"); // You fail your attempt at contacting the netherworld.
                    }
                    else
                    {
                        if (toChannel != null)
                        {
                            toChannel.Animated = true;
                            toChannel.Channeled = true;
                            toChannel.Hue = 1109;
                            var pl = Caster as PlayerMobile;
                            if (pl != null && pl.Almas < 30)
                            {

                                var qtdAlmas = 1;
                                if(toChannel.Owner != null)
                                {
                                    if (toChannel.Owner.HitsMax > 300)
                                        qtdAlmas++;
                                    if (toChannel.Owner.HitsMax > 600)
                                        qtdAlmas++;
                                    if (toChannel.Owner.HitsMax > 1000)
                                        qtdAlmas++;
                                    if (toChannel.Owner.HitsMax > 1500)
                                        qtdAlmas++;
                                }

                                pl.Almas+= qtdAlmas;
                                if (pl.Almas > 30)
                                    pl.Almas = 30;
                                pl.SendMessage($"Almas coletadas: {pl.Almas}/30");
                                pl.PrivateOverheadMessage($"* {pl.Almas}/30 *");
                            }
                        }

                        Caster.Mana -= mana;
                        Caster.SendMessage(msg);

                        if (min > max)
                        {
                            min = max;
                        }

                        Caster.Heal(Utility.RandomMinMax(min, max));

                        Caster.FixedParticles(0x375A, 1, 15, 9501, 2100, 4, EffectLayer.Waist);
                    }
                }

                SpiritSpeak.Remove(Caster);
                Stop();
            }
        }
    }
}
