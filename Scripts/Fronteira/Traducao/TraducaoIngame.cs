using Server.Commands;
using Server.Mobiles;
using Server.Targeting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Ziden.Traducao
{
    class Trads
    {
        public static void Initialize()
        {
            CommandSystem.Register("traduzir", AccessLevel.Counselor, OnAction);
            CommandSystem.Register("atualizarTraducoes", AccessLevel.Administrator, OnUpdateTranslations);
        }

        public static void Configure()
        {
            Console.WriteLine("Inicializando listener traducoes");
            EventSink.ItemCreated += ItemCriado;
            EventSink.MobileCreated += MobileCriado;
        }

        public static void MobileCriado(MobileCreatedEventArgs e)
        {
            if (Mobiles.ContainsKey(e.Mobile.GetType().Name))
            {
                e.Mobile.Name = Mobiles[e.Mobile.GetType().Name].ToLower();
            }
        }

        private static void OnUpdateTranslations(CommandEventArgs e)
        {
            UpdateAllItems();
            UpdateAllMobiles();
        }

        private static void UpdateAllItems()
        {
            foreach (Item item in World.Items.Values)
            {
                UpdateNomeItem(item);
            }
        }

        private static void UpdateAllMobiles()
        {
            foreach (Mobile mob in World.Mobiles.Values)
            {
                if (mob is BaseCreature && Mobiles.ContainsKey(mob.GetType().Name))
                {
                    mob.Name = Mobiles[mob.GetType().Name].ToLower();
                }
            }
        }

        public static void UpdateNomeItem(Item i)
        {
            string nome = null;

            // Sistema novo galera traduz ingame
            if (Items.TryGetValue(i.GetType().Name, out nome))
            {
                i.Name = nome;
            }

            // Sistema velho, traduz na unha
            var itemName = Lang.ItemTrans(i.GetType().Name);
            if (itemName != null)
            {
                i.Name = itemName.ToLower();
            }

            if (i.Name == null && i.DefaultName != null)
            {
                i.Name = i.DefaultName;
            }
        }

        public static void ItemCriado(ItemCreatedEventArgs e)
        {
            if (Shard.DebugGritando)
            {
                Shard.Debug("Item Criado " + e.Item.GetType().Name);
            }

            UpdateNomeItem(e.Item);
            /*
            var nome = GetNome(e.Item.GetType());
            if (nome != null)
            {
                e.Item.Name = nome;
            }
            */
        }

        public static Dictionary<Type, string> MobTrans = new Dictionary<Type, string>();
        public static Dictionary<Type, string> ItemTrans = new Dictionary<Type, string>();
        public static HashSet<Type> Ignore = new HashSet<Type>();
        public static HashSet<Type> IgnoreMobs = new HashSet<Type>();

        public static string GetNomeMonstro(Type t)
        {
            if (MobTrans.ContainsKey(t))
                return MobTrans[t];

            if (IgnoreMobs.Contains(t))
                return t.Name;

            try
            {
                BaseCreature.BypassConstructor = true;
                Mobile.BypassInit = true;
         
                var bc = (Mobile)Activator.CreateInstance(t);
                Shard.Debug("Criando instancia fake" + t.Name+" name "+bc.Name);
                BaseCreature.BypassConstructor = false;
                Mobile.BypassInit = false;
                bc.m_Deleted = true;
                if (bc.Name != null)
                {
                    MobTrans[t] = bc.Name;
                    return bc.Name;
                }
                EventSink.InvokeMobileCreated(new MobileCreatedEventArgs(bc));
                if (bc.Name != null)
                {
                    MobTrans[t] = bc.Name;
                    return bc.Name;
                }
                Shard.Debug("Nome final " + bc.Name);
                IgnoreMobs.Add(t);
                return t.Name;
            }
            catch (Exception e)
            {
                return t.Name;
            }
            finally
            {
                BaseCreature.BypassConstructor = false;
                Mobile.BypassInit = false;
            }
        }

        public static String GetNome(Type type)
        {
            if (Ignore.Contains(type))
                return null;

            string nome = null;
            if (Items.TryGetValue(type.Name, out nome))
            {
                if (Shard.DebugEnabled) Shard.Debug("Traduzi pelo sistema novo " + nome);
                return nome;
            }
            
            if (ItemTrans.TryGetValue(type, out nome))
            {
                if (Shard.DebugEnabled) Shard.Debug("Traduzi pelo sistema antigo " + nome);
                return nome;
            }

            try
            {
                Item.BypassInitialization = true;
                var i = (Item)Activator.CreateInstance(type);
                Item.BypassInitialization = false;
                if (i != null)
                {
                    UpdateNomeItem(i);
                    if (i.Name != null)
                    {
                        ItemTrans.Add(type, i.Name);
                        return i.Name;
                    }
                    else
                    {
                        Ignore.Add(type);
                    }

                }
            }
            catch
            {
                Shard.Erro("Erro ao traduzir item " + type.Name);
                Item.BypassInitialization = false;
            }
            finally
            {
                Item.BypassInitialization = false;
            }
            return null;
        }


        public static Dictionary<Serial, int> Tradutores = new Dictionary<Serial, int>();
        public static Dictionary<string, string> Items = new Dictionary<string, string>();
        public static Dictionary<string, string> Mobiles = new Dictionary<string, string>();
        public static Dictionary<int, string> Clilocs = new Dictionary<int, string>();

        public static void Score(Mobile m)
        {
            if (!Tradutores.ContainsKey(m.Serial))
            {
                Tradutores[m.Serial] = 1;
            }
            else
            {
                Tradutores[m.Serial]++;
            }
        }

        [Usage("traduzir")]
        private static void OnAction(CommandEventArgs e)
        {
            if (e.Arguments.Count() < 1)
            {
                e.Mobile.SendMessage("Use /traduzir 'nome da coisa' ou /traduzir <numero cliloc> 'texto' para traduzir");
                return;
            }
            else if (e.Arguments.Count() == 1)
            {
                var texto = e.Arguments[0];
                e.Mobile.SendMessage("Selecione o monstro/item que deseja traduzir");
                e.Mobile.Target = new IT(texto);
            }
            else if (e.Arguments.Count() == 2)
            {
                try
                {
                    var cliloc = Int32.Parse(e.Arguments[0]);
                    var texto = e.Arguments[1];

                    var trans = Lang.Trans(cliloc);
                    var temloc = Clilocs.ContainsKey(cliloc);
                    var trad = trans;
                    if (trad == null && temloc)
                        trad = Clilocs[cliloc];
                    if (trad != null && e.Mobile.LastTraducao != "CL_" + cliloc)
                    {
                        e.Mobile.SendMessage("Este cliloc ja esta traduzido para: " + trad);
                        e.Mobile.SendMessage("Use o comando novamente para sobre-escrever");
                        e.Mobile.LastTraducao = "CL_" + cliloc;
                    }
                    else
                    {
                        Clilocs[cliloc] = texto;
                        e.Mobile.SendMessage("Cliloc " + cliloc + " traduzido para: " + texto);
                        Score(e.Mobile);
                    }
                }
                catch (Exception ex)
                {
                    e.Mobile.SendMessage("Formatacao invalida, use /traduzir <numero> ''texto''");
                }

            }


        }

        public class IT : Target
        {

            private string texto;

            public IT(string texto) : base(10, true, TargetFlags.None)
            {
                this.texto = texto;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is Item)
                {
                    var item = (Item)targeted;
                    var tipo = item.GetType().Name;
                    if (Items.ContainsKey(tipo))
                    {
                        if (from.LastTraducao == tipo)
                        {
                            from.SendMessage("Item traduzido para " + texto);
                            Items[tipo] = texto;
                            item.Name = texto;
                            Score(from);
                        }
                        else
                        {
                            var trad = Items[tipo];
                            from.SendMessage("Este item ja tem uma traducao: " + trad);
                            from.SendMessage("Use o comando novamente no item para sobreescrever");
                            from.LastTraducao = tipo;
                        }

                    }
                    else
                    {
                        from.SendMessage("Item traduzido para " + texto);
                        if (texto == "")
                        {
                            Items[tipo] = texto;
                            item.Name = texto;
                        }
                        else
                        {
                            Items.Remove(tipo);
                        }

                        Score(from);
                    }
                }
                else if (targeted is BaseCreature)
                {
                    var bc = (BaseCreature)targeted;
                    var tipo = bc.GetType().Name;
                    if (Mobiles.ContainsKey(tipo))
                    {
                        if (texto == "")
                        {
                            Mobiles.Remove(tipo);
                            bc.Name = null;
                            from.SendMessage("Mobile removido");
                            return;
                        }
                        if (from.LastTraducao == tipo)
                        {
                            from.SendMessage("Monstro traduzido para " + texto);
                            Mobiles[tipo] = texto;
                            bc.Name = texto;
                            Score(from);
                        }
                        else
                        {
                            var trad = Mobiles[tipo];
                            from.SendMessage("Este monstro ja tem uma traducao: " + trad);
                            from.SendMessage("Use o comando novamente no monstro para sobreescrever");
                            from.LastTraducao = tipo;
                        }
                    }
                    else
                    {
                        from.SendMessage("Monstro traduzido para " + texto);
                        if (texto == "")
                        {
                            Mobiles.Remove(tipo);
                        }
                        else
                        {
                            Mobiles[tipo] = texto;
                            bc.Name = texto;
                            Score(from);
                        }
                    }
                }
                else
                {
                    from.SendMessage("Por enquanto so pode-se traduzir items/monstros ou clilocs");
                }
            }
        }
    }
}
