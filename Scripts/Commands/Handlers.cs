using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Server.Commands.Generic;
using Server.Gumps;
using Server.Items;
using Server.Menus.ItemLists;
using Server.Menus.Questions;
using Server.Mobiles;
using Server.Network;
using Server.Spells;
using Server.Targeting;
using Server.Targets;

namespace Server.Commands
{
    public class CommandHandlers
    {
        public static void Initialize()
        {
            CommandSystem.Prefix = ".";

            Register("Go", AccessLevel.Counselor, new CommandEventHandler(Go_OnCommand));

            Register("DropHolding", AccessLevel.Counselor, new CommandEventHandler(DropHolding_OnCommand));

            Register("GetFollowers", AccessLevel.GameMaster, new CommandEventHandler(GetFollowers_OnCommand));

            Register("ClearFacet", AccessLevel.Administrator, new CommandEventHandler(ClearFacet_OnCommand));

            Register("Where", AccessLevel.Player, new CommandEventHandler(Where_OnCommand));

            Register("AutoPageNotify", AccessLevel.Counselor, new CommandEventHandler(APN_OnCommand));
            Register("APN", AccessLevel.Counselor, new CommandEventHandler(APN_OnCommand));

            Register("Animate", AccessLevel.GameMaster, new CommandEventHandler(Animate_OnCommand));

            Register("Cast", AccessLevel.Counselor, new CommandEventHandler(Cast_OnCommand));

            Register("Stuck", AccessLevel.Counselor, new CommandEventHandler(Stuck_OnCommand));

            Register("Help", AccessLevel.Player, new CommandEventHandler(Help_OnCommand));

            Register("Save", AccessLevel.Administrator, new CommandEventHandler(Save_OnCommand));
            Register("BackgroundSave", AccessLevel.Administrator, new CommandEventHandler(BackgroundSave_OnCommand));
            Register("BGSave", AccessLevel.Administrator, new CommandEventHandler(BackgroundSave_OnCommand));
            Register("SaveBG", AccessLevel.Administrator, new CommandEventHandler(BackgroundSave_OnCommand));

            Register("Move", AccessLevel.GameMaster, new CommandEventHandler(Move_OnCommand));
            Register("Client", AccessLevel.Counselor, new CommandEventHandler(Client_OnCommand));

            Register("SMsg", AccessLevel.Counselor, new CommandEventHandler(StaffMessage_OnCommand));
            Register("SM", AccessLevel.Counselor, new CommandEventHandler(StaffMessage_OnCommand));
            Register("S", AccessLevel.Counselor, new CommandEventHandler(StaffMessage_OnCommand));

            Register("BCast", AccessLevel.GameMaster, new CommandEventHandler(BroadcastMessage_OnCommand));
            Register("BC", AccessLevel.GameMaster, new CommandEventHandler(BroadcastMessage_OnCommand));
            Register("B", AccessLevel.GameMaster, new CommandEventHandler(BroadcastMessage_OnCommand));

            Register("Bank", AccessLevel.GameMaster, new CommandEventHandler(Bank_OnCommand));

            Register("Echo", AccessLevel.Counselor, new CommandEventHandler(Echo_OnCommand));

            Register("Sound", AccessLevel.GameMaster, new CommandEventHandler(Sound_OnCommand));

            Register("ViewEquip", AccessLevel.GameMaster, new CommandEventHandler(ViewEquip_OnCommand));

            Register("Light", AccessLevel.Counselor, new CommandEventHandler(Light_OnCommand));
            Register("Stats", AccessLevel.Counselor, new CommandEventHandler(Stats_OnCommand));

            Register("ReplaceBankers", AccessLevel.Administrator, new CommandEventHandler(ReplaceBankers_OnCommand));

            Register("SpeedBoost", AccessLevel.Counselor, new CommandEventHandler(SpeedBoost_OnCommand));
            
            Register("PvMTag", AccessLevel.Player, new CommandEventHandler(PvMTag_OnCommand));
            Register("AddPvMTag", AccessLevel.GameMaster, new CommandEventHandler(AddPvMTag_OnCommand));
        }

        public static void Register(string command, AccessLevel access, CommandEventHandler handler)
        {
            CommandSystem.Register(command, access, handler);
        }

        public static Dictionary<int, DateTime> PvMTagCooldown = new Dictionary<int, DateTime>();
        public static Dictionary<int, int> PvMTagUseCount = new Dictionary<int, int>(); // nova variável criada para a contagem

        [Usage("PvMTag")]
        [Description("Adiciona tag PvM por 150k por 2 horas.")]
        public static void PvMTag_OnCommand(CommandEventArgs e)
        {
            PlayerMobile player = e.Mobile as PlayerMobile;

            if (player == null || player.Deleted || !player.Alive)
                return;

            if (player.HasPvMTag)
            {
                player.SendMessage("Você já tem uma tag PvM ativa.");
                return;
            }

            if (SpellHelper.CheckCombat(player))
            {
                player.SendMessage("Voce não pode usar esse comando em combate!");
                return;
            }

            if (player.AccessLevel >= AccessLevel.VIP)
            {
                // VIPs podem usar o comando 3x por dia
                DateTime lastUse;
                PvMTagCooldown.TryGetValue(player.Serial, out lastUse);
                var now = DateTime.UtcNow;

                if (lastUse > now.AddHours(-24))
                {          
                    int count;
                    PvMTagUseCount.TryGetValue(player.Serial, out count); // obtém a contagem do jogador

                    if (count >= 3) // verifica se o jogador já usou o comando 3 vezes hoje
                    {
                        TimeSpan cooldownRemaining = lastUse.AddHours(24) - now;
                        player.SendMessage("Você já usou esse comando recentemente. Por favor, espere até {0} ({1} horas e {2}  minutos) antes de usar novamente.", lastUse.AddHours(24), (int)cooldownRemaining.TotalHours, (int)cooldownRemaining.Minutes);
                        return;
                    }
                }
            }
            else
            {
                // PLayers normais apenas 1 vez
                DateTime lastUse;
                PvMTagCooldown.TryGetValue(player.Serial, out lastUse);

                var now = DateTime.UtcNow;
                if (lastUse > now.AddHours(-24))        
                {            
                    TimeSpan cooldownRemaining = lastUse.AddHours(24) - now;
                    player.SendMessage("Você já usou esse comando recentemente. Por favor, espere até {0} ({1} horas e {2}  minutos) antes de usar novamente.", lastUse.AddHours(24), (int)cooldownRemaining.TotalHours, (int)cooldownRemaining.Minutes);
                    return;
                }
            }

            var gump = new PvMTagConfirmGump(player);
            player.SendGump(gump);
        }

        private static void ActivatePvMTag(PlayerMobile player)
        {
            if (!Banker.Withdraw(player, 150000))
            {
                player.SendMessage("Voce precisa de 150.000 moedas de ouro no banco para isto, saldo insuficiente.");
                return;
            }

            player.HasPvMTag = true;
            player.SendMessage("Tag PvM ativada por 2h.");

            Timer.DelayCall(TimeSpan.FromHours(2), () =>
            {
                player.HasPvMTag = false;
                player.SendMessage("Sua Tag PvM acabou.");
            });

            player.PlaySound(0x5C3);
            PvMTagCooldown[player.Serial] = DateTime.UtcNow;

            int useCount;
            PvMTagUseCount.TryGetValue(player.Serial, out useCount); // obtém a contagem do jogador
            PvMTagUseCount[player.Serial] = useCount + 1; // incrementa a contagem do jogador
        }
        private class PvMTagConfirmGump : Gump
        {
            private readonly PlayerMobile _player;

            public PvMTagConfirmGump(PlayerMobile player) : base(40, 40)
            {
                _player = player;

                Closable = false;
                Disposable = true;
                Dragable = true;
                Resizable = false;

                AddPage(0);

                AddBackground(0, 0, 304, 126, 9270);

                AddLabel(50, 30, 0x34, "Tag PvM será ativada por 2h ao custo de 150k, deseja ativar?");
                AddButton(73, 70, 4005, 4006, 1, GumpButtonType.Reply, 0);
                AddButton(177, 70, 4005, 4006, 0, GumpButtonType.Reply, 0);
                AddLabel(100, 70, 0x34, "Sim");
                AddLabel(204, 70, 0x34, "Não");
            }

            public override void OnResponse(NetState sender, RelayInfo info)
            {
                if (info.ButtonID == 1)
                {
                    // Activate PvMTag if player clicked "Yes"
                    ActivatePvMTag(_player);
                }
                else
                {
                    _player.SendMessage("Comando cancelado.");
                }
            }
        }

        [Usage("AddPvMTag")]
        [Description("Adiciona tag PvM a um jogador.")]
        public static void AddPvMTag_OnCommand(CommandEventArgs e)
        {
            e.Mobile.BeginTarget(-1, false, TargetFlags.None, new TargetCallback(AddPvMTag_OnTarget));
            e.Mobile.SendMessage("Selecione o jogador que deseja adicionar a tag PvM");
        }

        public static void AddPvMTag_OnTarget(Mobile from, object obj)
        {
            if (obj is Mobile && ((Mobile)obj).Player)
            {
                PlayerMobile targ = obj as PlayerMobile;
                targ.HasPvMTag = true;
                from.SendMessage("Ativada tag PvM por 2h no jogador {0}.", targ);
                Timer.DelayCall(TimeSpan.FromHours(2), () =>
                {
                    targ.HasPvMTag = false;
                    targ.SendMessage("Sua Tag PvM acabou.");
                });
                targ.PlaySound(0x5C3);
            }
            else
            {
                from.SendMessage("Jogador não encontrado.");
                return;
            }
        }


        [Usage("Where")]
        [Description("Diz ao jogador que comanda suas coordenadas, região e faceta.")]
        public static void Where_OnCommand(CommandEventArgs e)
        {
            Mobile from = e.Mobile;
            Map map = from.Map;

            from.SendMessage("Voce esta em {0} {1} {2} in {3}.", from.X, from.Y, from.Z, map);

            if (map != null)
            {
                Region reg = from.Region;

                if (!reg.IsDefault)
                {
                    StringBuilder builder = new StringBuilder();

                    builder.Append(reg.ToString());
                    reg = reg.Parent;

                    while (reg != null)
                    {
                        builder.Append(" <- " + reg.ToString());
                        reg = reg.Parent;
                    }

                    from.SendMessage("Sua regiao e {0}.", builder.ToString());
                }
            }
        }

        [Usage("DropHolding")]
        [Description("Descarta o item, se houver, que o jogador alvo está segurando. O item é colocado em sua mochila ou, se estiver cheia, a seus pés.")]
        public static void DropHolding_OnCommand(CommandEventArgs e)
        {
            e.Mobile.BeginTarget(-1, false, TargetFlags.None, new TargetCallback(DropHolding_OnTarget));
            e.Mobile.SendMessage("Mire no jogador para largar o que está segurando");
        }

        public static void DropHolding_OnTarget(Mobile from, object obj)
        {
            if (obj is Mobile && ((Mobile)obj).Player)
            {
                Mobile targ = (Mobile)obj;
                Item held = targ.Holding;

                if (held == null)
                {
                    from.SendMessage("Eles não estão segurando nada.");
                }
                else
                {
                    if (from.AccessLevel == AccessLevel.Counselor)
                    {
                        Engines.Help.PageEntry pe = Engines.Help.PageQueue.GetEntry(targ);

                        if (pe == null || pe.Handler != from)
                        {
                            if (pe == null)
                                from.SendMessage("Você só pode usar este comando em alguém que o chamou.");
                            else
                                from.SendMessage("Você só pode usar este comando se estiver lidando com sua página de ajuda.");

                            return;
                        }
                    }

                    if (targ.AddToBackpack(held))
                        from.SendMessage("O item que eles seguravam foi colocado em sua mochila.");
                    else
                        from.SendMessage("O item que seguravam foi colocado a seus pés.");

                    held.ClearBounce();

                    targ.Holding = null;
                }
            }
            else
            {
                from.BeginTarget(-1, false, TargetFlags.None, new TargetCallback(DropHolding_OnTarget));
                from.SendMessage("Isso não é um jogador. Tente novamente.");
            }
        }

        public static void DeleteList_Callback(Mobile from, bool okay, object state)
        {
            if (okay)
            {
                List<IEntity> list = (List<IEntity>)state;

                CommandLogging.WriteLine(from, "{0} {1} deleting {2} object{3}", from.AccessLevel, CommandLogging.Format(from), list.Count, list.Count == 1 ? "" : "s");

                NetState.Pause();

                for (int i = 0; i < list.Count; ++i)
                    list[i].Delete();

                NetState.Resume();

                from.SendMessage("Você excluiu {0} objeto{1}.", list.Count, list.Count == 1 ? "" : "s");
            }
            else
            {
                from.SendMessage("Você optou por não excluir esses objetos.");
            }
        }

        [Usage("ClearFacet")]
        [Description("Exclui todos os itens e celulares em sua faceta. Jogadores e seus inventários não serão excluídos.")]
        public static void ClearFacet_OnCommand(CommandEventArgs e)
        {
            Map map = e.Mobile.Map;

            if (map == null || map == Map.Internal)
            {
                e.Mobile.SendMessage("Você não pode executar esse comando aqui.");
                return;
            }

            List<IEntity> list = new List<IEntity>();

            foreach (Item item in World.Items.Values)
                if (item.Map == map && item.Parent == null)
                    list.Add(item);

            foreach (Mobile m in World.Mobiles.Values)
                if (m.Map == map && !m.Player)
                    list.Add(m);

            if (list.Count > 0)
            {
                CommandLogging.WriteLine(e.Mobile, "{0} {1} starting facet clear of {2} ({3} object{4})", e.Mobile.AccessLevel, CommandLogging.Format(e.Mobile), map, list.Count, list.Count == 1 ? "" : "s");

                e.Mobile.SendGump(
                    new WarningGump(1060635, 30720,
                        String.Format("Você está prestes a deletar {0} object{1} desta faceta.  Você realmente deseja continuar?",
                            list.Count, list.Count == 1 ? "" : "s"),
                        0xFFC000, 360, 260, new WarningGumpCallback(DeleteList_Callback), list));
            }
            else
            {
                e.Mobile.SendMessage("Não foram encontrados objetos para excluir.");
            }
        }

        [Usage("GetFollowers")]
        [Description("Teleporta todos os animais de estimação de um jogador selecionado para sua localização.")]
        public static void GetFollowers_OnCommand(CommandEventArgs e)
        {
            e.Mobile.BeginTarget(-1, false, TargetFlags.None, new TargetCallback(GetFollowers_OnTarget));
            e.Mobile.SendMessage("Escolha um jogador para pegar seus animais de estimação.");
        }

        public static void GetFollowers_OnTarget(Mobile from, object obj)
        {
            if (obj is PlayerMobile)
            {
                PlayerMobile master = (PlayerMobile)obj;
                List<Mobile> pets = master.AllFollowers;

                if (pets.Count > 0)
                {
                    CommandLogging.WriteLine(from, "{0} {1} obtendo todos os seguidores de {2}", from.AccessLevel, CommandLogging.Format(from), CommandLogging.Format(master));

                    from.SendMessage("Esse jogador tem {0} pet{1}.", pets.Count, pets.Count != 1 ? "s" : "");

                    for (int i = 0; i < pets.Count; ++i)
                    {
                        Mobile pet = (Mobile)pets[i];

                        if (pet is IMount)
                            ((IMount)pet).Rider = null; // make sure it's dismounted

                        pet.MoveToWorld(from.Location, from.Map);
                    }
                }
                else
                {
                    from.SendMessage("Nenhum animal de estimação foi encontrado para aquele jogador.");
                }
            }
            else if (obj is Mobile && ((Mobile)obj).Player)
            {
                Mobile master = (Mobile)obj;
                ArrayList pets = new ArrayList();

                foreach (Mobile m in World.Mobiles.Values)
                {
                    if (m is BaseCreature)
                    {
                        BaseCreature bc = (BaseCreature)m;

                        if ((bc.Controlled && bc.ControlMaster == master) || (bc.Summoned && bc.SummonMaster == master))
                            pets.Add(bc);
                    }
                }

                if (pets.Count > 0)
                {
                    CommandLogging.WriteLine(from, "{0} {1} getting all followers of {2}", from.AccessLevel, CommandLogging.Format(from), CommandLogging.Format(master));

                    from.SendMessage("Esse jogador tem {0} pet{1}.", pets.Count, pets.Count != 1 ? "s" : "");

                    for (int i = 0; i < pets.Count; ++i)
                    {
                        Mobile pet = (Mobile)pets[i];

                        if (pet is IMount)
                            ((IMount)pet).Rider = null; // make sure it's dismounted

                        pet.MoveToWorld(from.Location, from.Map);
                    }
                }
                else
                {
                    from.SendMessage("Nenhum animal de estimação foi encontrado para aquele jogador.");
                }
            }
            else
            {
                from.BeginTarget(-1, false, TargetFlags.None, new TargetCallback(GetFollowers_OnTarget));
                from.SendMessage("Isso não é um jogador. Tente novamente.");
            }
        }

        public static void ReplaceBankers_OnCommand(CommandEventArgs e)
        {
            List<Mobile> list = new List<Mobile>();

            foreach (Mobile m in World.Mobiles.Values)
                if ((m is Banker) && !(m is BaseCreature))
                    list.Add(m);

            foreach (Mobile m in list)
            {
                Map map = m.Map;

                if (map != null)
                {
                    bool hasBankerSpawner = false;

                    foreach (Item item in m.GetItemsInRange(0))
                    {
                        if (item is Spawner)
                        {
                            Spawner spawner = (Spawner)item;

                            for (int i = 0; !hasBankerSpawner && i < spawner.SpawnObjects.Count; ++i)
                                hasBankerSpawner = Insensitive.Equals((string)spawner.SpawnObjects[i].SpawnName, "banker");

                            if (hasBankerSpawner)
                                break;
                        }
                    }

                    if (!hasBankerSpawner)
                    {
                        Spawner spawner = new Spawner(1, 1, 5, 0, 4, "banker");

                        spawner.MoveToWorld(m.Location, map);
                    }
                }
            }
        }

        [Usage("ViewEquip")]
        [Description("Lista o equipamento de um celular-alvo. Da lista, você pode mover, excluir ou abrir adereços.")]
        public static void ViewEquip_OnCommand(CommandEventArgs e)
        {
            e.Mobile.Target = new ViewEqTarget();
        }

        [Usage("Sound <index> [toAll=true]")]
        [Description("Toca um som para jogadores a até 12 peças de você. O argumento (toAll) especifica para todos ou apenas para aqueles que podem ver você.")]
        public static void Sound_OnCommand(CommandEventArgs e)
        {
            if (e.Length == 1)
                PlaySound(e.Mobile, e.GetInt32(0), true);
            else if (e.Length == 2)
                PlaySound(e.Mobile, e.GetInt32(0), e.GetBoolean(1));
            else
                e.Mobile.SendMessage("Formato: Som <index> [toAll]");
        }

        [Usage("Echo <text>")]
        [Description("Retransmite (texto) como uma mensagem do sistema.")]
        public static void Echo_OnCommand(CommandEventArgs e)
        {
            string toEcho = e.ArgString.Trim();

            if (toEcho.Length > 0)
                e.Mobile.SendMessage(toEcho);
            else
                e.Mobile.SendMessage("Format: Echo \"<text>\"");
        }

        [Usage("Bank")]
        [Description("Abre a caixa do banco de um determinado alvo.")]
        public static void Bank_OnCommand(CommandEventArgs e)
        {
            e.Mobile.Target = new BankTarget();
        }

        [Usage("Help")]
        [Description("Lista todos os comandos disponíveis.")]
        public static void Help_OnCommand(CommandEventArgs e)
        {
            Mobile m = e.Mobile;

            List<CommandEntry> list = new List<CommandEntry>();

            foreach (CommandEntry entry in CommandSystem.Entries.Values)
                if (m.AccessLevel >= entry.AccessLevel)
                    list.Add(entry);

            list.Sort();

            StringBuilder sb = new StringBuilder();

            if (list.Count > 0)
                sb.Append(list[0].Command);

            for (int i = 1; i < list.Count; ++i)
            {
                string v = list[i].Command;

                if ((sb.Length + 1 + v.Length) >= 256)
                {
                    m.SendAsciiMessage(0x482, sb.ToString());
                    sb = new StringBuilder();
                    sb.Append(v);
                }
                else
                {
                    sb.Append(' ');
                    sb.Append(v);
                }
            }

            if (sb.Length > 0)
                m.SendAsciiMessage(0x482, sb.ToString());
        }

        [Usage("SMsg <text>")]
        [Aliases("S", "SM")]
        [Description("Transmite uma mensagem para toda a equipe online.")]
        public static void StaffMessage_OnCommand(CommandEventArgs e)
        {
            BroadcastMessage(AccessLevel.Counselor, e.Mobile.SpeechHue, String.Format("[{0}] {1}", e.Mobile.Name, e.ArgString));
        }

        [Usage("BCast <text>")]
        [Aliases("B", "BC")]
        [Description("Transmite uma mensagem para todos online.")]
        public static void BroadcastMessage_OnCommand(CommandEventArgs e)
        {
            BroadcastMessage(AccessLevel.Player, 0x482, String.Format("Staff message from {0}:", e.Mobile.Name));
            BroadcastMessage(AccessLevel.Player, 0x482, e.ArgString);
        }

        public static void BroadcastMessage(AccessLevel ac, int hue, string message) 
        { 
            World.Broadcast(hue, false, ac, message);
        }

        [Usage("AutoPageNotify")]
        [Aliases("APN")]
        [Description("Alterna seu status de notificação de página automática.")]
        public static void APN_OnCommand(CommandEventArgs e)
        {
            Mobile m = e.Mobile;

            m.AutoPageNotify = !m.AutoPageNotify;

            m.SendMessage("Sua notificação de página automática foi desativada {0}.", m.AutoPageNotify ? "on" : "off");
        }

        [Usage("Animate <action> <frameCount> <repeatCount> <forward> <repeat> <delay>")]
        [Description("Faz seu personagem fazer uma animação específica.")]
        public static void Animate_OnCommand(CommandEventArgs e)
        {
            if (e.Length == 6)
            {
                e.Mobile.Animate(e.GetInt32(0), e.GetInt32(1), e.GetInt32(2), e.GetBoolean(3), e.GetBoolean(4), e.GetInt32(5));
            }
            else
            {
                e.Mobile.SendMessage("Format: Animate <action> <frameCount> <repeatCount> <forward> <repeat> <delay>");
            }
        }

        [Usage("Cast <name>")]
        [Description("Lança um feitiço pelo nome.")]
        public static void Cast_OnCommand(CommandEventArgs e)
        {
            if (e.Length == 1)
            {
                if (!Multis.DesignContext.Check(e.Mobile))
                    return; // They are customizing

                Spell spell = SpellRegistry.NewSpell(e.GetString(0), e.Mobile, null);

                if (spell != null)
                    spell.Cast();
                else
                    e.Mobile.SendMessage("Esse feitiço não foi encontrado.");
            }
            else
            {
                e.Mobile.SendMessage("Formato: Elenco <nome>");
            }
        }

        [Usage("Stuck")]
        [Description("Abre um menu de cidades, usado para teletransportar celulares presos.")]
        public static void Stuck_OnCommand(CommandEventArgs e)
        {
            e.Mobile.Target = new StuckMenuTarget();
        }

        [Usage("Light <level>")]
        [Description("Defina o seu nível de luz local.")]
        public static void Light_OnCommand(CommandEventArgs e)
        {
            e.Mobile.LightLevel = e.GetInt32(0);
        }

        [Usage("Stats")]
        [Description("Veja algumas estatísticas sobre o servidor.")]
        public static void Stats_OnCommand(CommandEventArgs e)
        {
            e.Mobile.SendMessage("Conexões abertas: {0}", Network.NetState.Instances.Count);
            e.Mobile.SendMessage("Mobiles: {0}", World.Mobiles.Count);
            e.Mobile.SendMessage("Items: {0}", World.Items.Count);
        }

        [Usage("SpeedBoost [true|false]")]
        [Description("Ativa um aumento de velocidade para o invocador. Desative com parâmetros.")]
        private static void SpeedBoost_OnCommand(CommandEventArgs e)
        {
            Mobile from = e.Mobile;

            if (e.Length <= 1)
            {
                if (e.Length == 1 && !e.GetBoolean(0))
                {
                    from.Send(SpeedControl.Disable);
                    from.SendMessage("O aumento de velocidade foi desativado.");
                }
                else
                {
                    from.Send(SpeedControl.MountSpeed);
                    from.SendMessage("O aumento de velocidade foi ativado.");
                }
            }
            else
            {
                from.SendMessage("Formato: SpeedBoost [true | false]");
            }
        }

        private static void PlaySound(Mobile m, int index, bool toAll)
        {
            Map map = m.Map;

            if (map == null)
                return;

            CommandLogging.WriteLine(m, "{0} {1} playing sound {2} (toAll={3})", m.AccessLevel, CommandLogging.Format(m), index, toAll);

            Packet p = new PlaySound(index, m.Location);

            p.Acquire();

            foreach (NetState state in m.GetClientsInRange(12))
            {
                if (toAll || state.Mobile.CanSee(m))
                    state.Send(p);
            }

            p.Release();
        }

        [Usage("Client")]
        [Description("Abre o menu do cliente gump para um determinado jogador.")]
        private static void Client_OnCommand(CommandEventArgs e)
        {
            e.Mobile.Target = new ClientTarget();
        }

        [Usage("Move")]
        [Description("Reposiciona um item de destino ou dispositivo móvel.")]
        private static void Move_OnCommand(CommandEventArgs e)
        {
            e.Mobile.Target = new PickMoveTarget();
        }

        [Usage("Save")]
        [Description("Salva o mundo.")]
        private static void Save_OnCommand(CommandEventArgs e)
        {
            Misc.AutoSave.Save();
        }

        [Usage("BackgroundSave")]
        [Aliases("BGSave", "SaveBG")]
        [Description("Salva o mundo, gravando no disco em segundo plano")]
        private static void BackgroundSave_OnCommand(CommandEventArgs e)
        {
            Misc.AutoSave.Save(true);
        }

        private static bool FixMap(ref Map map, ref Point3D loc, Item item)
        {
            if (map == null || map == Map.Internal)
            {
                Mobile m = item.RootParent as Mobile;

                return (m != null && FixMap(ref map, ref loc, m));
            }

            return true;
        }

        private static bool FixMap(ref Map map, ref Point3D loc, Mobile m)
        {
            if (map == null || map == Map.Internal)
            {
                map = m.LogoutMap;
                loc = m.LogoutLocation;
            }

            return (map != null && map != Map.Internal);
        }

        [Usage("Go [name | serial | (x y [z]) | (deg min (N | S) deg min (E | W))]")]
        [Description("With no arguments, this command brings up the go menu. With one argument, (name), you are moved to that regions \"go location.\" Or, if a numerical value is specified for one argument, (serial), you are moved to that object. Two or three arguments, (x y [z]), will move your character to that location. When six arguments are specified, (deg min (N | S) deg min (E | W)), your character will go to an approximate of those sextant coordinates.")]
        private static void Go_OnCommand(CommandEventArgs e)
        {
            Mobile from = e.Mobile;

            if (e.Length == 0)
            {
                GoGump.DisplayTo(from);
                return;
            }

            if (e.Length == 1)
            {
                try
                {
                    int ser = e.GetInt32(0);

                    IEntity ent = World.FindEntity(ser);

                    if (ent is Item)
                    {
                        Item item = (Item)ent;

                        Map map = item.Map;
                        Point3D loc = item.GetWorldLocation();

                        Mobile owner = item.RootParent as Mobile;

                        if (owner != null && (owner.Map != null && owner.Map != Map.Internal) && !BaseCommand.IsAccessible(from, owner) /* !from.CanSee( owner )*/)
                        {
                            from.SendMessage("Você não pode ir para o que você não pode ver.");
                            return;
                        }
                        else if (owner != null && (owner.Map == null || owner.Map == Map.Internal) && owner.Hidden && owner.AccessLevel >= from.AccessLevel)
                        {
                            from.SendMessage("Você não pode ir para o que você não pode ver.");
                            return;
                        }
                        else if (!FixMap(ref map, ref loc, item))
                        {
                            from.SendMessage("Esse é um item interno e você não pode acessá-lo.");
                            return;
                        }

                        from.MoveToWorld(loc, map);

                        return;
                    }
                    else if (ent is Mobile)
                    {
                        Mobile m = (Mobile)ent;

                        Map map = m.Map;
                        Point3D loc = m.Location;

                        Mobile owner = m;

                        if (owner != null && (owner.Map != null && owner.Map != Map.Internal) && !BaseCommand.IsAccessible(from, owner) /* !from.CanSee( owner )*/)
                        {
                            from.SendMessage("Você não pode ir para o que você não pode ver.");
                            return;
                        }
                        else if (owner != null && (owner.Map == null || owner.Map == Map.Internal) && owner.Hidden && owner.AccessLevel >= from.AccessLevel)
                        {
                            from.SendMessage("Você não pode ir para o que você não pode ver.");
                            return;
                        }
                        else if (!FixMap(ref map, ref loc, m))
                        {
                            from.SendMessage("Esse é um celular interno e você não pode acessá-lo.");
                            return;
                        }

                        from.MoveToWorld(loc, map);

                        return;
                    }
                    else
                    {
                        string name = e.GetString(0);
                        Map map;

                        for (int i = 0; i < Map.AllMaps.Count; ++i)
                        {
                            map = Map.AllMaps[i];

                            if (map.MapIndex == 0x7F || map.MapIndex == 0xFF)
                                continue;

                            if (Insensitive.Equals(name, map.Name))
                            {
                                from.Map = map;
                                return;
                            }
                        }

                        Dictionary<string, Region> list = from.Map.Regions;

                        foreach (KeyValuePair<string, Region> kvp in list)
                        {
                            Region r = kvp.Value;

                            if (Insensitive.Equals(r.Name, name))
                            {
                                from.Location = new Point3D(r.GoLocation);
                                return;
                            }
                        }

                        for (int i = 0; i < Map.AllMaps.Count; ++i)
                        {
                            Map m = Map.AllMaps[i];

                            if (m.MapIndex == 0x7F || m.MapIndex == 0xFF || from.Map == m)
                                continue;

                            foreach (Region r in m.Regions.Values)
                            {
                                if (Insensitive.Equals(r.Name, name))
                                {
                                    from.MoveToWorld(r.GoLocation, m);
                                    return;
                                }
                            }
                        }

                        if (ser != 0)
                            from.SendMessage("Nenhum objeto com esse serial foi encontrado.");
                        else
                            from.SendMessage("Nenhuma região com esse nome foi encontrada.");

                        return;
                    }
                }
                catch
                {
                }

                from.SendMessage("Nome da região não encontrado");
            }
            else if (e.Length == 2 || e.Length == 3)
            {
                Map map = from.Map;

                if (map != null)
                {
                    try
                    {
                        /*
                        * This to avoid being teleported to (0,0) if trying to teleport
                        * to a region with spaces in its name.
                        */
                        int x = int.Parse(e.GetString(0));
                        int y = int.Parse(e.GetString(1));
                        int z = (e.Length == 3) ? int.Parse(e.GetString(2)) : map.GetAverageZ(x, y);

                        from.Location = new Point3D(x, y, z);
                    }
                    catch
                    {
                        from.SendMessage("Nome da região não encontrado.");
                    }
                }
            }
            else if (e.Length == 6)
            {
                Map map = from.Map;

                if (map != null)
                {
                    Point3D p = Sextant.ReverseLookup(map, e.GetInt32(3), e.GetInt32(0), e.GetInt32(4), e.GetInt32(1), Insensitive.Equals(e.GetString(5), "E"), Insensitive.Equals(e.GetString(2), "S"));

                    if (p != Point3D.Zero)
                        from.Location = p;
                    else
                        from.SendMessage("A pesquisa reversa de Sextant falhou.");
                }
            }
            else
            {
                from.SendMessage("Format: Go [name | serial | (x y [z]) | (deg min (N | S) deg min (E | W)]");
            }
        }

        private class ViewEqTarget : Target
        {
            public ViewEqTarget()
                : base(-1, false, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (!BaseCommand.IsAccessible(from, targeted))
                {
                    from.SendMessage("Isso não é acessível.");
                    return;
                }

                if (targeted is Mobile)
                    from.SendMenu(new EquipMenu(from, (Mobile)targeted, GetEquip((Mobile)targeted)));
            }

            private static ItemListEntry[] GetEquip(Mobile m)
            {
                ItemListEntry[] entries = new ItemListEntry[m.Items.Count];

                for (int i = 0; i < m.Items.Count; ++i)
                {
                    Item item = m.Items[i];

                    entries[i] = new ItemListEntry(String.Format("{0}: {1}", item.Layer, item.GetType().Name), item.ItemID, item.Hue);
                }

                return entries;
            }

            private class EquipMenu : ItemListMenu
            {
                private readonly Mobile m_Mobile;
                public EquipMenu(Mobile from, Mobile m, ItemListEntry[] entries)
                    : base("Equipment", entries)
                {
                    this.m_Mobile = m;

                    CommandLogging.WriteLine(from, "{0} {1} viewing equipment of {2}", from.AccessLevel, CommandLogging.Format(from), CommandLogging.Format(m));
                }

                public override void OnResponse(NetState state, int index)
                {
                    if (index >= 0 && index < this.m_Mobile.Items.Count)
                    {
                        Item item = this.m_Mobile.Items[index];

                        state.Mobile.SendMenu(new EquipDetailsMenu(this.m_Mobile, item));
                    }
                }

                private class EquipDetailsMenu : QuestionMenu
                {
                    private readonly Mobile m_Mobile;
                    private readonly Item m_Item;
                    public EquipDetailsMenu(Mobile m, Item item)
                        : base(String.Format("{0}: {1}", item.Layer, item.GetType().Name), new string[] { "Move", "Delete", "Props" })
                    {
                        this.m_Mobile = m;
                        this.m_Item = item;
                    }

                    public override void OnCancel(NetState state)
                    {
                        state.Mobile.SendMenu(new EquipMenu(state.Mobile, this.m_Mobile, ViewEqTarget.GetEquip(this.m_Mobile)));
                    }

                    public override void OnResponse(NetState state, int index)
                    {
                        if (index == 0)
                        {
                            CommandLogging.WriteLine(state.Mobile, "{0} {1} moving equipment item {2} of {3}", state.Mobile.AccessLevel, CommandLogging.Format(state.Mobile), CommandLogging.Format(this.m_Item), CommandLogging.Format(this.m_Mobile));
                            state.Mobile.Target = new MoveTarget(this.m_Item);
                        }
                        else if (index == 1)
                        {
                            CommandLogging.WriteLine(state.Mobile, "{0} {1} deleting equipment item {2} of {3}", state.Mobile.AccessLevel, CommandLogging.Format(state.Mobile), CommandLogging.Format(this.m_Item), CommandLogging.Format(this.m_Mobile));
                            this.m_Item.Delete();
                        }
                        else if (index == 2)
                        {
                            CommandLogging.WriteLine(state.Mobile, "{0} {1} opening properties for equipment item {2} of {3}", state.Mobile.AccessLevel, CommandLogging.Format(state.Mobile), CommandLogging.Format(this.m_Item), CommandLogging.Format(this.m_Mobile));
                            state.Mobile.SendGump(new PropertiesGump(state.Mobile, this.m_Item));
                        }
                    }
                }
            }
        }

        private class BankTarget : Target
        {
            public BankTarget()
                : base(-1, false, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is Mobile)
                {
                    Mobile m = (Mobile)targeted;

                    BankBox box = (m.Player ? m.BankBox : m.FindBankNoCreate());

                    if (box != null)
                    {
                        CommandLogging.WriteLine(from, "{0} {1} opening bank box of {2}", from.AccessLevel, CommandLogging.Format(from), CommandLogging.Format(targeted));

                        if (from == targeted)
                            box.Open();
                        else
                            box.DisplayTo(from);
                    }
                    else
                    {
                        from.SendMessage("Eles não têm caixa bancária.");
                    }
                }
            }
        }

        private class DismountTarget : Target
        {
            public DismountTarget()
                : base(-1, false, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is Mobile)
                {
                    CommandLogging.WriteLine(from, "{0} {1} dismounting {2}", from.AccessLevel, CommandLogging.Format(from), CommandLogging.Format(targeted));

                    Mobile targ = (Mobile)targeted;

                    for (int i = 0; i < targ.Items.Count; ++i)
                    {
                        Item item = targ.Items[i];

                        if (item is IMountItem)
                        {
                            IMount mount = ((IMountItem)item).Mount;

                            if (mount != null)
                                mount.Rider = null;

                            if (targ.Items.IndexOf(item) == -1)
                                --i;
                        }
                    }

                    for (int i = 0; i < targ.Items.Count; ++i)
                    {
                        Item item = targ.Items[i];

                        if (item.Layer == Layer.Mount)
                        {
                            item.Delete();
                            --i;
                        }
                    }
                }
            }
        }

        private class ClientTarget : Target
        {
            public ClientTarget()
                : base(-1, false, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is Mobile)
                {
                    Mobile targ = (Mobile)targeted;

                    if (targ.NetState != null)
                    {
                        CommandLogging.WriteLine(from, "{0} {1} opening client menu of {2}", from.AccessLevel, CommandLogging.Format(from), CommandLogging.Format(targeted));
                        from.SendGump(new ClientGump(from, targ.NetState));
                    }
                }
            }
        }

        private class StuckMenuTarget : Target
        {
            public StuckMenuTarget()
                : base(-1, false, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is Mobile)
                {
                    if (((Mobile)targeted).AccessLevel >= from.AccessLevel && targeted != from)
                        from.SendMessage("Você não pode fazer isso com alguém com nível de acesso superior ao seu!");
                    else
                        from.SendGump(new StuckMenu(from, (Mobile)targeted, false));
                }
            }
        }
    }
}
