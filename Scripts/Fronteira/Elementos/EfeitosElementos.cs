using Server.Items;
using System.Collections.Generic;

namespace Server.Fronteira.Elementos
{
    public class EfeitosElementos
    {
        private static Dictionary<ElementoPvM, string[]> _efeitos = new Dictionary<ElementoPvM, string[]>();
        private static Dictionary<ElementoPvM, string[]> _efeitosColar = new Dictionary<ElementoPvM, string[]>();

        public static void Effect(Mobile m, ElementoPvM e)
        {
            m.FixedParticles(0x3779, 8, 10, 5002, BaseArmor.HueElemento(e), 0, EffectLayer.Head, 1);
        }

        public static string [] GetEfeitos(ElementoPvM elemento)
        {
            string[] efeitos;
            if (_efeitos.TryGetValue(elemento, out efeitos))
                return efeitos;

            switch(elemento)
            {
                case ElementoPvM.Fogo:
                    efeitos = new string[] {
                        "1: Dano de Fogo",
                        "1: Esquiva",
                        "1: Fogo Queima"
                    };
                    break;
                case ElementoPvM.Agua:
                    efeitos = new string[] {
                        "1: Dano Pocoes",
                        "1: Magic Resist",
                        "1: Dano Eletrico",
                    };
                    break;
                case ElementoPvM.Terra:
                    efeitos = new string[] {
                        "1: Dano & Resist de Venenos",
                        "1: Armadura",
                        "1: Dano Fisico"
                    };
                    break;
                case ElementoPvM.Raio:
                    efeitos = new string[] {
                        "1: Dano Eletrico",
                        "1: Dano Fisico",
                        "1: Esquiva"
                    };
                    break;
                case ElementoPvM.Luz:
                    efeitos = new string[] {
                        "0.5: Cura ao Atacar",
                        "1: Resistencia Magica",
                        "1: Armadura"
                    };
                    break;
                case ElementoPvM.Escuridao:
                    efeitos = new string[] {
                        "1: Penetr. Magica",
                        //"Dano Magias Proibidas",
                        "1: Resistencia Magica",
                        "1: LifeSteal Magico"
                    };
                    break;
                case ElementoPvM.Gelo:
                    efeitos = new string[] {
                        "1: Esquiva",
                        "1: Resistencia Magica",
                        "1: Bonus Coleta Recursos"
                    };
                    break;
                case ElementoPvM.Vento:
                    efeitos = new string[] {
                        "1: Velocidade Ataque",
                        "1: Penetr. Armadura",
                        "0.5: Esquiva"
                    };
                    break;
                default:
                    efeitos = new string[] { };
                    break;
            }
            _efeitos[elemento] = efeitos;
            return efeitos;
        }

        public static string[] GetEfeitosColar(ElementoPvM elemento)
        {
            string[] efeitos;
            if (_efeitosColar.TryGetValue(elemento, out efeitos))
                return efeitos;

            switch (elemento)
            {
                case ElementoPvM.Fogo:
                    efeitos = new string[] {
                        "Bonus Flamestrike", //
                        "Bonus Fire Field",  //
                    };
                    break;
                case ElementoPvM.Agua:
                    efeitos = new string[] {
                        "Bonus Pots de Dano", //
                        "Magic Resist",       //
                    };
                    break;
                case ElementoPvM.Terra:
                    efeitos = new string[] {
                        "Armor Pets",       //
                        "Dano Fisico Pets", //
                    };
                    break;
                case ElementoPvM.Raio:
                    efeitos = new string[] {
                        "Bonus Energy Bolt", //
                        "Bonus Lightning",   //
                    };
                    break;
                case ElementoPvM.Luz:
                    efeitos = new string[] {
                        "Chance Resist a Morte", //
                        "Parry Bloqueia Magias", //
                    };
                    break;
                case ElementoPvM.Escuridao:
                    efeitos = new string[] {
                        "Bonus Resist Magias Negras", //
                        "Bonus Magias Negras",        //
                    };
                    break;
                case ElementoPvM.Gelo:
                    efeitos = new string[] {
                        "Bonus Magias de Varinhas",   //
                        "Chance Congelar Monstros Atacantes"      // 
                    };
                    break;
                case ElementoPvM.Vento:
                    efeitos = new string[] {
                        "Chance Critico",       //
                        "Chance Golpe Duplo"           //
                    };
                    break;
                default:
                    efeitos = new string[] { };
                    break;
            }
            _efeitosColar[elemento] = efeitos;
            return efeitos;
        }
    }
}
