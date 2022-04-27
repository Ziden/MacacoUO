using System;
using System.Collections.Generic;
using Server.Engines.Harvest;
using Server.Items;
using Server.Mobiles;
using Server.Ziden.Traducao;

namespace Server.Engines.BulkOrders
{
    [TypeAlias("Scripts.Engines.BulkOrders.SmallBOD")]
    public abstract class SmallBOD : BaseDecayingItem, IBOD
    {
        public abstract BODType BODType { get; }

        private int m_AmountCur, m_AmountMax;
        private Type m_Type;
        private int m_Number;
        private int m_Graphic;
        private int m_GraphicHue;
        private bool m_RequireExceptional;
        private BulkMaterialType m_Material;

        public override int Lifespan { get { return 60*60*24*3; } }
        public override bool UseSeconds { get { return false; } }

        [Constructable]
        public SmallBOD(int hue, int amountMax, Type type, int number, int graphic, bool requireExeptional, BulkMaterialType material, int graphichue = 0)
            : base(0x2258)
        {
            Name = "Ordem de trabalho Pequena";
            Weight = 1.0;
            Hue = hue; // Blacksmith: 0x44E; Tailoring: 0x483
            LootType = LootType.Blessed;

            m_AmountMax = amountMax;
            m_Type = type;
            m_Number = number;
            m_Graphic = graphic;
            m_GraphicHue = graphichue;
            m_RequireExceptional = requireExeptional;
            m_Material = material;
        }

        public SmallBOD()
            : base(0x2258)
        {
            Weight = 1.0;
            LootType = LootType.Blessed;
        }

        public SmallBOD(Serial serial)
            : base(serial)
        {
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int AmountCur
        {
            get
            {
                return m_AmountCur;
            }
            set
            {
                m_AmountCur = value;
                InvalidateProperties();
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int AmountMax
        {
            get
            {
                return m_AmountMax;
            }
            set
            {
                m_AmountMax = value;
                InvalidateProperties();
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public virtual Type Type
        {
            get
            {
                return m_Type;
            }
            set
            {
                m_Type = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int Number
        {
            get
            {
                return m_Number;
            }
            set
            {
                m_Number = value;
                InvalidateProperties();
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int Graphic
        {
            get
            {
                return m_Graphic;
            }
            set
            {
                m_Graphic = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int GraphicHue
        {
            get
            {
                return m_GraphicHue;
            }
            set
            {
                m_GraphicHue = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public bool RequireExceptional
        {
            get
            {
                return m_RequireExceptional;
            }
            set
            {
                m_RequireExceptional = value;
                InvalidateProperties();
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public BulkMaterialType Material
        {
            get
            {
                return m_Material;
            }
            set
            {
                m_Material = value;
                InvalidateProperties();
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public bool Complete
        {
            get
            {
                return (m_AmountCur == m_AmountMax);
            }
        }
        public override string DefaultName
        {
            get
            {
                return "Ordem de trabalho Pequena";
            }
        }// a bulk order deed
        public static BulkMaterialType GetRandomMaterial(BulkMaterialType start, double[] chances)
        {
            double random = Utility.RandomDouble();

            for (int i = 0; i < chances.Length; ++i)
            {
                if (random < chances[i])
                    return (i == 0 ? BulkMaterialType.None : start + (i - 1));

                random -= chances[i];
            }

            return BulkMaterialType.None;
        }

        public static BulkMaterialType GetMaterial(CraftResource resource)
        {
            switch ( resource )
            {
                case CraftResource.Cobre:
                    return BulkMaterialType.Cobre;
                case CraftResource.Bronze:
                    return BulkMaterialType.Bronze;
                case CraftResource.Dourado:
                    return BulkMaterialType.Dourado;
                case CraftResource.Niobio:
                    return BulkMaterialType.Niobio;
                case CraftResource.Lazurita:
                    return BulkMaterialType.Lazurita;
                case CraftResource.Quartzo:
                    return BulkMaterialType.Quartzo;
                case CraftResource.Berilo:
                    return BulkMaterialType.Berilo;
                case CraftResource.Vibranium:
                    return BulkMaterialType.Vibranium;
                //case CraftResource.Adamantium:
                //    return BulkMaterialType.Adamantium;

                case CraftResource.CouroSpinned:
                    return BulkMaterialType.Spined;
                case CraftResource.CouroHorned:
                    return BulkMaterialType.Horned;
                case CraftResource.CouroBarbed:
                    return BulkMaterialType.Barbed;

                case CraftResource.Carvalho:
                    return BulkMaterialType.Carvalho;
                case CraftResource.Mogno:
                    return BulkMaterialType.Mogno;
                case CraftResource.Pinho:
                    return BulkMaterialType.Pinho;
                case CraftResource.Eucalipto:
                    return BulkMaterialType.Eucalipto;
                case CraftResource.Carmesim:
                    return BulkMaterialType.Carmesin;
                case CraftResource.Gelo:
                    return BulkMaterialType.Gelo;
            }

            return BulkMaterialType.None;
        }

        public static CraftResource GetResource(BulkMaterialType resource)
        {
            switch (resource)
            {
                case BulkMaterialType.Cobre:
                    return CraftResource.Cobre;
                case BulkMaterialType.Bronze:
                    return CraftResource.Bronze;
                case BulkMaterialType.Dourado:
                    return CraftResource.Dourado;
                case BulkMaterialType.Niobio:
                    return CraftResource.Niobio;
                case BulkMaterialType.Lazurita:
                    return CraftResource.Lazurita;
                case BulkMaterialType.Quartzo:
                    return CraftResource.Quartzo;
                case BulkMaterialType.Berilo:
                    return CraftResource.Berilo;
                case BulkMaterialType.Vibranium:
                    return CraftResource.Vibranium;
                //case CraftResource.Adamantium:
                //    return BulkMaterialType.Adamantium;

                case BulkMaterialType.Spined:
                    return CraftResource.CouroSpinned;
                case BulkMaterialType.Horned:
                    return CraftResource.CouroHorned;
                case BulkMaterialType.Barbed:
                    return CraftResource.CouroBarbed;

                case BulkMaterialType.Carvalho:
                    return CraftResource.Carvalho;
                case BulkMaterialType.Mogno:
                    return CraftResource.Mogno;
                case BulkMaterialType.Pinho:
                    return CraftResource.Pinho;
                case BulkMaterialType.Eucalipto:
                    return CraftResource.Eucalipto;
                case BulkMaterialType.Carmesin:
                    return CraftResource.Carmesim;
                case BulkMaterialType.Gelo:
                    return CraftResource.Gelo;
            }

            return CraftResource.None;
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);
           
            if (m_RequireExceptional)
                list.Add("Apenas Excepcionais"); // All items must be exceptional.

            if (m_Material != BulkMaterialType.None)
                list.Add("Feito de " + m_Material); // All items must be made with x material.

            var nomeItem = Trads.GetNome(m_Type);
            if(nomeItem != null)
            {
                list.Add("Fazer " + m_AmountMax.ToString()+" "+nomeItem); // amount to make: ~1_val~
                list.AddTwoValues("Progresso", m_AmountCur+"/"+ m_AmountMax); // ~1_val~: ~2_val~
            } else
            {
                list.Add("Fazer " + m_AmountMax.ToString()); // amount to make: ~1_val~
                list.Add(1060658, "#{0}\t{1}", m_Number, m_AmountCur); // ~1_val~: ~2_val~
            }
        

        }

        public override void OnDoubleClick(Mobile from)
        {

            if (IsChildOf(from.Backpack) || InSecureTrade || RootParent is PlayerVendor)
			{
				EventSink.InvokeBODUsed(new BODUsedEventArgs(from, this));
				from.SendGump(new SmallBODGump(from, this));
                if(this.Complete)
                {
                    from.SendMessage(78, "Esta ordem esta completa. Arresta ela para o NPC que pediu a ordem para completa-la !");
                }
			}
            else
			{
				from.SendMessage("Precisa estar na mochila"); // You must have the deed in your backpack to use it.
			}
		}

        public override void OnDoubleClickNotAccessible(Mobile from)
        {
            OnDoubleClick(from);
        }

        public override void OnDoubleClickSecureTrade(Mobile from)
        {
            OnDoubleClick(from);
        }

        public void BeginCombine(Mobile from)
        {
            if (m_AmountCur < m_AmountMax)
                from.Target = new SmallBODTarget(this);
            else
                from.SendMessage("Ja foram combinado items demais"); // The maximum amount of requested items have already been combined to this deed.
        }

        public abstract List<Item> ComputeRewards(bool full);

        public abstract int ComputeGold();

        public abstract int ComputeFame();

        public virtual void GetRewards(out Item reward, out int gold, out int fame)
        {
            reward = null;
            gold = ComputeGold();
            fame = ComputeFame();

            if (!BulkOrderSystem.NewSystemEnabled)
            {
                List<Item> rewards = ComputeRewards(false);

                if (rewards.Count > 0)
                {
                    reward = rewards[Utility.Random(rewards.Count)];

                    for (int i = 0; i < rewards.Count; ++i)
                    {
                        if (rewards[i] != reward)
                            rewards[i].Delete();
                    }
                }
            }
        }

        public virtual bool CheckType(Item item)
        {
            return CheckType(item.GetType());
        }

        public virtual bool CheckType(Type itemType)
        {
            return m_Type != null && (itemType == m_Type || itemType.IsSubclassOf(m_Type));
        }


        public void EndCombine(Mobile from, object o)
        {
            if (o is Item && ((Item)o).IsChildOf(from.Backpack))
            {
                Type objectType = o.GetType();
                Item item = o as Item;
                var arma = item as BaseWeapon;
                var ar = item as BaseArmor;
                var roupa = item as BaseClothing;
                var f = item as CraftableFurniture;
                var tool = item as BaseTool;

                if (arma != null && !arma.PlayerConstructed)
                {
                    from.SendMessage("Este item nao tem qualidade suficiente, parece ate que foi comprado de uma loja barata...");
                }
                else if (ar != null && !ar.PlayerConstructed)
                {
                    from.SendMessage("Este item nao tem qualidade suficiente, parece ate que foi comprado de uma loja barata...");
                } else if(ar != null && ar.Crafter != null && ar.Crafter != from)
                {
                    from.SendMessage("Este item nao e fruto do seu arduo trabalho...");
                }
                else if (arma != null && arma.Crafter != null && arma.Crafter != from)
                {
                    from.SendMessage("Este item nao e fruto do seu arduo trabalho...");
                }
                else if (roupa != null && roupa.Crafter != null && roupa.Crafter != from)
                {
                    from.SendMessage("Este item nao e fruto do seu arduo trabalho...");
                }
                else if (tool != null && tool.Crafter != null && tool.Crafter != from)
                {
                    from.SendMessage("Este item nao e fruto do seu arduo trabalho...");
                }
                else if (f != null && f.Crafter != null && f.Crafter != from)
                {
                    from.SendMessage("Este item nao e fruto do seu arduo trabalho...");
                }

                else if (m_AmountCur >= m_AmountMax)
                {
                    from.SendMessage("A quantidade pedida ja foi combinada"); // The maximum amount of requested items have already been combined to this deed.
                }
                else if (!CheckType((Item)o))
                {
                    from.SendMessage("Esta ordem nao quer este item"); // The item is not in the request.
                }
                else
                {
                    BulkMaterialType material = BulkMaterialType.None;

                    if (o is IResource)
                        material = GetMaterial(((IResource)o).Resource);

                    if (material != m_Material && m_Material != BulkMaterialType.None)
                    {
                        from.SendMessage("Item do material errado"); // The item is not made from the requested resource.
                    }
                    else
                    {
                        bool isExceptional = false;

                        if (o is IQuality)
                            isExceptional = (((IQuality)o).Quality == ItemQuality.Exceptional);

                        if (m_RequireExceptional && !isExceptional)
                        {
                            from.SendMessage("Precisa ser excepcional"); // The item must be exceptional.
                        }
                        else
                        {
                            if (item.Amount > 1)
                            {
                                if (AmountCur + item.Amount > AmountMax)
                                {
                                    from.SendMessage("Voce ta botando mais do que precisa"); // You have provided more than which has been requested by this deed.
                                    return;
                                }
                                else
                                {
                                    AmountCur += item.Amount;
                                    item.Delete();
                                }
                            }
                            else
                            {
                                item.Delete();
                                ++AmountCur;
                            }

                            from.SendMessage("Item combinado com a ordem"); // The item has been combined with the deed.

                            from.SendGump(new SmallBODGump(from, this));

                            if (m_AmountCur < m_AmountMax)
                                BeginCombine(from);
                        }
                    }
                }
            }
            else
            {
                from.SendMessage("Voce precisa ter o item na mochila"); // You must have the item in your backpack to target it.
            }
        }

        public static double GetRequiredSkill(BulkMaterialType type)
        {
            double skillReq = 0.0;

            switch (type)
            {
                case BulkMaterialType.Cobre:
                    skillReq = 70;
                    break;
                case BulkMaterialType.Bronze:
                    skillReq = 75;
                    break;
                case BulkMaterialType.Dourado:
                    skillReq = 80;
                    break;
                case BulkMaterialType.Niobio:
                    skillReq = 95;
                    break;
                case BulkMaterialType.Lazurita:
                    skillReq = 99;
                    break;
                case BulkMaterialType.Quartzo:
                    skillReq = 102;
                    break;
                case BulkMaterialType.Berilo:
                    skillReq = 110;
                    break;
                case BulkMaterialType.Vibranium:
                    skillReq = 115;
                    break;
                case BulkMaterialType.Spined:
                    skillReq = 65.0;
                    break;
                case BulkMaterialType.Horned:
                    skillReq = 80.0;
                    break;
                case BulkMaterialType.Barbed:
                    skillReq = 99.0;
                    break;


                case BulkMaterialType.Carvalho:
                    skillReq = 70.0;
                    break;
                case BulkMaterialType.Pinho:
                    skillReq = 75.0;
                    break;
                case BulkMaterialType.Mogno:
                    skillReq = 85.0;
                    break;
                case BulkMaterialType.Eucalipto:
                    skillReq = 100;
                    break;
                case BulkMaterialType.Carmesin:
                    skillReq = 110;
                    break;
                case BulkMaterialType.Gelo:
                    skillReq = 115;
                    break;
            }

            return skillReq;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)2); // version

            writer.Write(m_GraphicHue);

            writer.Write(m_AmountCur);
            writer.Write(m_AmountMax);
            writer.Write(m_Type == null ? null : m_Type.FullName);
            writer.Write(m_Number);
            writer.Write(m_Graphic);
            writer.Write(m_RequireExceptional);
            writer.Write((int)m_Material);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch ( version )
            {
                case 2:
                case 1:
                    {
                        m_GraphicHue = reader.ReadInt();
                        goto case 0;
                    }
                case 0:
                    {
                        m_AmountCur = reader.ReadInt();
                        m_AmountMax = reader.ReadInt();

                        string type = reader.ReadString();

                        if (type != null)
                            m_Type = ScriptCompiler.FindTypeByFullName(type);

                        m_Number = reader.ReadInt();
                        m_Graphic = reader.ReadInt();
                        m_RequireExceptional = reader.ReadBool();
                        m_Material = (BulkMaterialType)reader.ReadInt();

                        break;
                    }
            }

            if (Weight == 0.0)
                Weight = 1.0;

            if (Core.AOS && ItemID == 0x14EF)
                ItemID = 0x2258;

            if (Parent == null && Map == Map.Internal && Location == Point3D.Zero)
                Delete();

        }
    }
}
