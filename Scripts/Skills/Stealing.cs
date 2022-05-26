#region References
using System;
using System.Collections;

using Server.Factions;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Spells;
using Server.Spells.Fifth;
using Server.Spells.Ninjitsu;
using Server.Spells.Seventh;
using Server.Targeting;
using Server.Engines.VvV;
using Server.Regions;
using Server.Scripts.Custom.Items;
#endregion

namespace Server.SkillHandlers
{
    public delegate void ItemStolenEventHandler(ItemStolenEventArgs e);

	public class Stealing
	{
		public static void Initialize()
		{
			SkillInfo.Table[33].Callback = OnUse;
		}

        public static event ItemStolenEventHandler ItemStolen;

		public static readonly bool ClassicMode = false;
		public static readonly bool SuspendOnMurder = false;

		public static bool IsInGuild(Mobile m)
		{
            return true; // (m is PlayerMobile && ((PlayerMobile)m).NpcGuild == NpcGuild.ThievesGuild);
		}

		public static bool IsInnocentTo(Mobile from, Mobile to)
		{
			return (Notoriety.Compute(from, to) == Notoriety.Innocent);
		}

		private class StealingTarget : Target
		{
			private readonly Mobile m_Thief;

			public StealingTarget(Mobile thief)
				: base(1, false, TargetFlags.None)
			{
				m_Thief = thief;
				AllowNonlocal = true;
			}

			private Item TryStealItem(Item toSteal, ref bool caught)
			{

                if(m_Thief.Region.IsPartOf("New Haven"))
                {
                    m_Thief.SendMessage("Voce nao pode roubar em Haven");
                    return null;
                }

				Item stolen = null;

				object root = toSteal.RootParent;

				StealableArtifactsSpawner.StealableInstance stealable = null;
				if (toSteal.Parent == null || !toSteal.Movable)
				{
					stealable = toSteal is AddonComponent ? StealableArtifactsSpawner.GetStealableInstance(((AddonComponent)toSteal).Addon) : StealableArtifactsSpawner.GetStealableInstance(toSteal);
				}

                var canSteal = toSteal is BaseDecorationArtifact || toSteal is DecoRelPor || ItemFlags.GetStealable(toSteal);

                if(Shard.DebugEnabled)
                {
                    Shard.Debug("Flag ? " + ItemFlags.GetStealable(toSteal) + " Dungeon ? " + ((toSteal.GetRegion() is DungeonRegion)+" Movable ? "+ toSteal.Movable));
                }

                if(!toSteal.Movable && canSteal && toSteal.GetRegion() is DungeonRegion)
                {
                    if(m_Thief.Skills[SkillName.Stealing].Value < 100)
                    {
                        m_Thief.SendMessage("Voce nao tem habilidade suficiente para isto");
                    } else
                    {
                        m_Thief.SendMessage("Voce roubou o artefato");
                        m_Thief.RevealingAction();
                        m_Thief.OverheadMessage("* roubou *");

                        var bixos = m_Thief.FindCreaturesInRange(m_Thief.Map, 12);
                        foreach (var bixo in bixos)
                        {
                            Shard.Debug("Combat " + (bixo.Combatant == null));
                            if(bixo is BaseCreature && bixo.Combatant == null && !(((BaseCreature)bixo).ControlMaster is PlayerMobile))
                            {
                                bixo.OverheadMessage("!");
                                bixo.Combatant = m_Thief;
                            }
                        }
                        toSteal.Movable = true;
                        return toSteal;
                    }
                }

                if(root == null)
                {
                    m_Thief.SendMessage("Voce estranhamente nao pode roubar isto");
                    return null;
                }

				if (!IsEmptyHanded(m_Thief))
				{
					m_Thief.SendLocalizedMessage("Suas maos precisam estar vazias para roubar"); // Both hands must be free to steal.
				}
				else if (root is Mobile && ((Mobile)root).Player && !IsInGuild(m_Thief))
				{
					m_Thief.SendLocalizedMessage("Voce precisa fazer parte da confraria dos ladinos para roubar"); // You must be in the thieves guild to steal from other players.
				}
				else if (SuspendOnMurder && root is Mobile && ((Mobile)root).Player && IsInGuild(m_Thief) && m_Thief.Kills > 0)
				{
					m_Thief.SendLocalizedMessage("Voce esta suspendido da confraria dos ladinos"); // You are currently suspended from the thieves guild.
				}
				else if (root is BaseVendor && ((BaseVendor)root).IsInvulnerable)
				{
					m_Thief.SendLocalizedMessage("Nao pode roubar vendedores... (ainda)..."); // You can't steal from shopkeepers.
				}
				else if (root is PlayerVendor)
				{
					m_Thief.SendLocalizedMessage("Voce nao pode roubar isto"); // You can't steal from vendors.
				}
				else if (!m_Thief.CanSee(toSteal))
				{
					m_Thief.SendLocalizedMessage("Voce nao pode ver isto"); // Target can not be seen.
				}
				else if (m_Thief.Backpack == null || !m_Thief.Backpack.CheckHold(m_Thief, toSteal, false, true))
				{
					m_Thief.SendLocalizedMessage("Sua mochila esta muito cheia"); // Your backpack can't hold anything else.
				}
				#region Sigils
				else if (toSteal is Sigil)
				{
					PlayerState pl = PlayerState.Find(m_Thief);
					Faction faction = (pl == null ? null : pl.Faction);

					Sigil sig = (Sigil)toSteal;

					if (!m_Thief.InRange(toSteal.GetWorldLocation(), 1))
					{
						m_Thief.SendLocalizedMessage("Voce precisa estar proximo do item"); // You must be standing next to an item to steal it.
					}
					else if (root != null) // not on the ground
					{
						m_Thief.SendLocalizedMessage("Isto nao"); // You can't steal that!
					}
					else if (faction != null)
					{
						if (!m_Thief.CanBeginAction(typeof(IncognitoSpell)))
						{
							m_Thief.SendLocalizedMessage("Voce nao pode fazer isso com incognito"); //	You cannot steal the sigil when you are incognito
						}
						else if (DisguiseTimers.IsDisguised(m_Thief))
						{
							m_Thief.SendLocalizedMessage("Nao pode estar disfarcado"); //	You cannot steal the sigil while disguised
						}
						else if (!m_Thief.CanBeginAction(typeof(PolymorphSpell)))
						{
							m_Thief.SendLocalizedMessage("Nao pode estar polimorfizado"); //	You cannot steal the sigil while polymorphed				
						}
						else if (TransformationSpellHelper.UnderTransformation(m_Thief))
						{
							m_Thief.SendLocalizedMessage("Nao pode estar transformado"); // You cannot steal the sigil while in that form.
						}
						else if (AnimalForm.UnderTransformation(m_Thief))
						{
							m_Thief.SendLocalizedMessage("Nao pode estar transformado"); // You cannot steal the sigil while mimicking an animal.
						}
						else if (pl.IsLeaving)
						{
							m_Thief.SendLocalizedMessage("Voce esta saindo de uma faction nao pode fazer isto"); // You are currently quitting a faction and cannot steal the town sigil
						}
						else if (sig.IsBeingCorrupted && sig.LastMonolith.Faction == faction)
						{
							m_Thief.SendLocalizedMessage("Vai roubar seu proprio?"); //	You cannot steal your own sigil
						}
						else if (sig.IsPurifying)
						{
							m_Thief.SendLocalizedMessage("Sigilo purificado"); // You cannot steal this sigil until it has been purified
						}
						else if (m_Thief.CheckTargetSkillMinMax(SkillName.Stealing, toSteal, 0, 0))
						{
							if (Sigil.ExistsOn(m_Thief))
							{
								m_Thief.SendLocalizedMessage("Voce ja tem um sigilo");
									//	The sigil has gone back to its home location because you already have a sigil.
							}
							else if (m_Thief.Backpack == null || !m_Thief.Backpack.CheckHold(m_Thief, sig, false, true))
							{
								m_Thief.SendLocalizedMessage("Sua mochila esta cheia"); //	The sigil has gone home because your backpack is full
							}
							else
							{
								if (sig.IsBeingCorrupted)
								{
									sig.GraceStart = DateTime.UtcNow; // begin grace period
								}

								m_Thief.SendLocalizedMessage("VOCE ROUBOU O SIGILO !!! (eita, muita calma nessa hora)"); // YOU STOLE THE SIGIL!!!   (woah, calm down now)

								if (sig.LastMonolith != null && sig.LastMonolith.Sigil != null)
								{
									sig.LastMonolith.Sigil = null;
									sig.LastStolen = DateTime.UtcNow;
								}

								return sig;
							}
						}
						else
						{
							m_Thief.SendLocalizedMessage(1005594); //	You do not have enough skill to steal the sigil
						}
					}
					else
					{
						m_Thief.SendLocalizedMessage(1005588); //	You must join a faction to do that
					}
				}
				#endregion
                #region VvV Sigils
                else if (toSteal is VvVSigil && ViceVsVirtueSystem.Instance != null)
                {
                    VvVPlayerEntry entry = ViceVsVirtueSystem.Instance.GetPlayerEntry<VvVPlayerEntry>(m_Thief);

                    VvVSigil sig = (VvVSigil)toSteal;

                    if (!m_Thief.InRange(toSteal.GetWorldLocation(), 1))
                    {
                        m_Thief.SendLocalizedMessage("Precisa estar proximo"); // You must be standing next to an item to steal it.
                    }
                    else if (root != null) // not on the ground
                    {
                        m_Thief.SendLocalizedMessage("Nao pode roubar isto"); // You can't steal that!
                    }
                    else if (entry != null)
                    {
                        if (!m_Thief.CanBeginAction(typeof(IncognitoSpell)))
                        {
                            m_Thief.SendLocalizedMessage("Nao pode roubar o sigilo neste estado"); //	You cannot steal the sigil when you are incognito
                        }
                        else if (DisguiseTimers.IsDisguised(m_Thief))
                        {
                            m_Thief.SendLocalizedMessage("Nao pode roubar o sigilo neste estado"); //	You cannot steal the sigil while disguised
                        }
                        else if (!m_Thief.CanBeginAction(typeof(PolymorphSpell)))
                        {
                            m_Thief.SendLocalizedMessage("Nao pode roubar o sigilo neste estado"); //	You cannot steal the sigil while polymorphed				
                        }
                        else if (TransformationSpellHelper.UnderTransformation(m_Thief))
                        {
                            m_Thief.SendLocalizedMessage("Nao pode roubar o sigilo neste estado"); // You cannot steal the sigil while in that form.
                        }
                        else if (AnimalForm.UnderTransformation(m_Thief))
                        {
                            m_Thief.SendLocalizedMessage("Nao pode roubar o sigilo neste estado"); // You cannot steal the sigil while mimicking an animal.
                        }
                        else if (m_Thief.CheckTargetSkillMinMax(SkillName.Stealing, toSteal, 100.0, 120.0))
                        {
                            if (m_Thief.Backpack == null || !m_Thief.Backpack.CheckHold(m_Thief, sig, false, true))
                            {
                                m_Thief.SendLocalizedMessage("Mochila cheia"); //	The sigil has gone home because your backpack is full
                            }
                            else
                            {
                                m_Thief.SendLocalizedMessage("VOCE ROUBOU O SIGILO !! Muita calma agora !"); // YOU STOLE THE SIGIL!!!   (woah, calm down now)

                                sig.OnStolen(entry);

                                return sig;
                            }
                        }
                        else
                        {
                            m_Thief.SendLocalizedMessage(1005594); //	You do not have enough skill to steal the sigil
                        }
                    }
                    else
                    {
                        m_Thief.SendLocalizedMessage(1155415); //	Only participants in Vice vs Virtue may use this item.
                    }
                }
                #endregion

                else if (stealable == null && (toSteal.Parent == null || !toSteal.Movable) && !ItemFlags.GetStealable(toSteal))
				{
					m_Thief.SendLocalizedMessage("Nao pode roubar isto"); // You can't steal that!
				}
				else if ((toSteal.LootType == LootType.Blessed || toSteal.CheckBlessed(root)) && !ItemFlags.GetStealable(toSteal))
				{
					m_Thief.SendLocalizedMessage("Nao pode roubar isto"); // You can't steal that!
				}
				else if (Core.AOS && stealable == null && toSteal is Container && !ItemFlags.GetStealable(toSteal))
				{
					m_Thief.SendLocalizedMessage("Nao pode roubar isto"); // You can't steal that!
				}
				else if (!m_Thief.InRange(toSteal.GetWorldLocation(), 1))
				{
					m_Thief.SendLocalizedMessage("Muito longe"); // You must be standing next to an item to steal it.
				}
				else if (stealable != null && m_Thief.Skills[SkillName.Stealing].Value < 100.0)
				{
					m_Thief.SendLocalizedMessage("Precisa de mais skill"); // You're not skilled enough to attempt the theft of this item.
				}
				else if (toSteal.Parent is Mobile)
				{
					m_Thief.SendLocalizedMessage("Nao pode usar items equipados, ainda..."); // You cannot steal items which are equiped.
				}
				else if (root == m_Thief)
				{
					m_Thief.SendLocalizedMessage("Voce pegou voce mesmo tentando roubar voce mesmo. Mas hein ?"); // You catch yourself red-handed.
				}
				else if (root is Mobile && ((Mobile)root).IsStaff())
				{
					m_Thief.SendLocalizedMessage("Nao pode roubar isto"); // You can't steal that!
				}
				else if (root is Mobile && !(root is PlayerMobile) && !m_Thief.CanBeHarmful((Mobile)root))
				{ }
                else if(root is PlayerMobile && ((PlayerMobile)root).Young)
                {
                    m_Thief.SendMessage("Voce nao pode roubar novatos");
                }
                else if (root is PlayerMobile && ((PlayerMobile)root).RP)
                {
                    m_Thief.SendMessage("Voce nao pode roubar jogadores RP");
                }
                else if (root is Corpse)
				{
					m_Thief.SendLocalizedMessage("Nao pode roubar isto"); // You can't steal that!
				}
				else
				{
					double w = toSteal.Weight + toSteal.TotalWeight;

					if (w > 10)
					{
						m_Thief.SendMessage("Isto e muito pesado para roubar.");
					}
					else
					{
						if (toSteal.Stackable && toSteal.Amount > 1)
						{
							int maxAmount = (int)((m_Thief.Skills[SkillName.Stealing].Value / 10.0) / toSteal.Weight);

							if (maxAmount < 1)
							{
								maxAmount = 1;
							}
							else if (maxAmount > toSteal.Amount)
							{
								maxAmount = toSteal.Amount;
							}

							int amount = Utility.RandomMinMax(1, maxAmount);

							if (amount >= toSteal.Amount)
							{
								int pileWeight = (int)Math.Ceiling(toSteal.Weight * toSteal.Amount);
								pileWeight *= 10;

								if (m_Thief.CheckTargetSkillMinMax(SkillName.Stealing, toSteal, pileWeight - 22.5, pileWeight + 27.5))
								{
									stolen = toSteal;
								}
							}
							else
							{
								int pileWeight = (int)Math.Ceiling(toSteal.Weight * amount);
								pileWeight *= 10;

								if (m_Thief.CheckTargetSkillMinMax(SkillName.Stealing, toSteal, pileWeight - 22.5, pileWeight + 27.5))
								{
									stolen = Mobile.LiftItemDupe(toSteal, toSteal.Amount - amount);

									if (stolen == null)
									{
										stolen = toSteal;
									}
								}
							}
						}
						else
						{
							int iw = (int)Math.Ceiling(w);
							iw *= 10;

							if (m_Thief.CheckTargetSkillMinMax(SkillName.Stealing, toSteal, iw - 22.5, iw + 27.5))
							{
								stolen = toSteal;
							}
						}

                        // Non-movable stealable (not in fillable container) items cannot result in the stealer getting caught
                        if (stolen != null && (root is FillableContainer || stolen.Movable))
                        {
                            double skillValue = m_Thief.Skills[SkillName.Stealing].Value;

                            if (root is FillableContainer)
                            {
                                caught = (Utility.Random((int)(skillValue / 5)) == 0); // 1 of 48 chance at 120
                            }
                            else
                            {
                                caught = (skillValue < Utility.Random(200));
                            }
                        }
                        else
                        {
                            caught = false;
                        }

                        if (stolen != null)
						{
							m_Thief.SendLocalizedMessage("Voce roubou o item"); // You succesfully steal the item.

							ItemFlags.SetTaken(stolen, true);
							ItemFlags.SetStealable(stolen, false);
							stolen.Movable = true;

                            InvokeItemStoken(new ItemStolenEventArgs(stolen, m_Thief));

							if (stealable != null)
							{
								toSteal.Movable = true;
								//stealable.Item = null;
							}
						}
						else
						{
							m_Thief.SendLocalizedMessage("Voce falhou em roubar o item"); // You fail to steal the item.
						}
					}
				}

				return stolen;
			}

			protected override void OnTarget(Mobile from, object target)
			{
				from.RevealingAction();

                if (m_Thief.Region.IsPartOf("New Haven"))
                {
                    m_Thief.SendMessage("Voce nao pode roubar em Haven");
                    return;
                }



                Item stolen = null;
				object donoItem = null;
				bool caught = false;

				if (target is Item)
				{
					donoItem = ((Item)target).RootParent;
					stolen = TryStealItem((Item)target, ref caught);
				}
				else if (target is Mobile)
				{
					Container pack = ((Mobile)target).Backpack;

					if (pack != null && pack.Items.Count > 0)
					{
						int randomIndex = Utility.Random(pack.Items.Count);

						donoItem = target;
						stolen = TryStealItem(pack.Items[randomIndex], ref caught);
					}

                    #region Monster Stealables
                    if (target is BaseCreature && from is PlayerMobile)
                    {
                        Server.Engines.CreatureStealing.StealingHandler.HandleSteal(target as BaseCreature, from as PlayerMobile);
                    }
                    #endregion
				}
				else
				{
					m_Thief.SendLocalizedMessage("Voce nao pode roubar isto"); // You can't steal that!
				}

                if (donoItem is BaseCreature)
                {
                    ((BaseCreature)donoItem).Combatant = from;
                    ((BaseCreature)donoItem).OverheadMessage("!!");
                }       

				if (stolen != null)
				{
                    if (stolen is AddonComponent)
                    {
                        BaseAddon addon = ((AddonComponent)stolen).Addon as BaseAddon;
                        if(addon != null)
                        {
                            if(addon.Deed != null)
                                from.AddToBackpack(addon.Deed);
                            else
                            {
                                addon.Movable = true;
                                from.AddToBackpack(addon);
                            }
                            addon.Delete();
                        } else
                        {
                            from.SendMessage(38, "Erro - reportar como reproduzir pra staff");
                        }
                    }
                    else
                    {
                        from.AddToBackpack(stolen);
                    }

					if (!(stolen is Container || stolen.Stackable))
					{
						// do not return stolen containers or stackable items
						StolenItem.Add(stolen, m_Thief, donoItem as Mobile);
					}
				}

				if (caught)
				{
					if (donoItem == null)
					{
						m_Thief.CriminalAction(false);
					}
					else if (donoItem is Corpse && ((Corpse)donoItem).IsCriminalAction(m_Thief))
					{
						m_Thief.CriminalAction(false);
					}
					else if (donoItem is Mobile)
					{
						Mobile mobRoot = (Mobile)donoItem;

						if (IsInnocentTo(m_Thief, mobRoot))
						{
							m_Thief.CriminalAction(false);
						}

						string message = String.Format("Voce reparou {0} tentando roubar {1}.", m_Thief.Name, mobRoot.Name);

						foreach (NetState ns in m_Thief.GetClientsInRange(8))
						{
							if (ns.Mobile != m_Thief)
							{
								ns.Mobile.SendMessage(message);
							}
						}
					}
				}
				else if (donoItem is Corpse && ((Corpse)donoItem).IsCriminalAction(m_Thief))
				{
					m_Thief.CriminalAction(false);
				}

				if (donoItem is Mobile && ((Mobile)donoItem).Player && m_Thief is PlayerMobile && IsInnocentTo(m_Thief, (Mobile)donoItem))
				{
                    PlayerMobile pm = (PlayerMobile)m_Thief;
                    if(!((Mobile)donoItem).Criminal && !(donoItem is BaseCreature))
                        pm.CriminalAction(false);
					pm.Delta(MobileDelta.Noto);
				}
			}
		}

		public static bool IsEmptyHanded(Mobile from)
		{
			if (from.FindItemOnLayer(Layer.OneHanded) != null)
			{
				return false;
			}

			if (from.FindItemOnLayer(Layer.TwoHanded) != null)
			{
				return false;
			}

			return true;
		}

		public static TimeSpan OnUse(Mobile m)
		{
            if(m.IsYoung())
            {
                m.SendMessage("Novato ladrao, mil anos sem perdao");
                return TimeSpan.FromSeconds(1);
            }

            if (m.Region.IsPartOf("New Haven"))
            {
                m.SendMessage("Voce nao pode roubar em Haven");
                return TimeSpan.FromSeconds(1);
            }

            if (!IsEmptyHanded(m))
			{
				m.SendLocalizedMessage("Suas maos precisam estar livres"); // Both hands must be free to steal.
			}
			else
			{
				m.Target = new StealingTarget(m);
				m.RevealingAction();

				m.SendLocalizedMessage("Que item deseja roubar?"); // Which item do you want to steal?
			}

			return TimeSpan.FromSeconds(10.0);
		}

        public static void InvokeItemStoken(ItemStolenEventArgs e)
        {
            if (ItemStolen != null)
            {
                ItemStolen(e);
            }
        }
	}

	public class StolenItem
	{
		public static readonly TimeSpan StealTime = TimeSpan.FromMinutes(2.0);

		private readonly Item m_Stolen;
		private readonly Mobile m_Thief;
		private readonly Mobile m_Victim;
		private DateTime m_Expires;

		public Item Stolen { get { return m_Stolen; } }
		public Mobile Thief { get { return m_Thief; } }
		public Mobile Victim { get { return m_Victim; } }
		public DateTime Expires { get { return m_Expires; } }

		public bool IsExpired { get { return (DateTime.UtcNow >= m_Expires); } }

		public StolenItem(Item stolen, Mobile thief, Mobile victim)
		{
			m_Stolen = stolen;
			m_Thief = thief;
			m_Victim = victim;

			m_Expires = DateTime.UtcNow + StealTime;
		}

		private static readonly Queue m_Queue = new Queue();

		public static void Add(Item item, Mobile thief, Mobile victim)
		{
			Clean();

			m_Queue.Enqueue(new StolenItem(item, thief, victim));
		}

		public static bool IsStolen(Item item)
		{
			Mobile victim = null;

			return IsStolen(item, ref victim);
		}

		public static bool IsStolen(Item item, ref Mobile victim)
		{
			Clean();

			foreach (StolenItem si in m_Queue)
			{
				if (si.m_Stolen == item && !si.IsExpired)
				{
					victim = si.m_Victim;
					return true;
				}
			}

			return false;
		}

		public static void ReturnOnDeath(Mobile killed, Container corpse)
		{
			Clean();

			foreach (StolenItem si in m_Queue)
			{
				if (si.m_Stolen.RootParent == corpse && si.m_Victim != null && !si.IsExpired)
				{
					if (si.m_Victim.AddToBackpack(si.m_Stolen))
					{
						si.m_Victim.SendLocalizedMessage("O item roubado foi retornado a voce"); // the item that was stolen is returned to you.
					}
					else
					{
						si.m_Victim.SendLocalizedMessage("O item roubado cai no chao"); // the item that was stolen from you falls to the ground.
					}

					si.m_Expires = DateTime.UtcNow; // such a hack
				}
			}
		}

		public static void Clean()
		{
			while (m_Queue.Count > 0)
			{
				StolenItem si = (StolenItem)m_Queue.Peek();

				if (si.IsExpired)
				{
					m_Queue.Dequeue();
				}
				else
				{
					break;
				}
			}
		}
	}

    public class ItemStolenEventArgs : EventArgs
    {
        public Item Item { get; set; }
        public Mobile Mobile { get; set; }

        public ItemStolenEventArgs(Item item, Mobile thief)
        {
            Mobile = thief;
            Item = item;
        }
    }
}
