using Server.Ziden.Traducao;
using System;

namespace Server.Engines.Craft
{
    public class CraftRes
    {
        private readonly Type m_Type;
        private readonly int m_Amount;
        private readonly string m_MessageString;
        private readonly int m_MessageNumber;
        private readonly string m_NameString;
        private readonly int m_NameNumber;
        public CraftRes(Type type, int amount)
        {
            this.m_Type = type;
            this.m_Amount = amount;
        }

        public CraftRes(Type type, TextDefinition name, int amount, TextDefinition message)
            : this(type, amount)
        {
            this.m_NameNumber = name;
            this.m_MessageNumber = message;

            this.m_NameString = name;
            this.m_MessageString = message;

      
            try
            {
                // Gambiarra porca... mas eh oq tem pra hj
                Item.BypassInitialization = true;
                var i = (Item)Activator.CreateInstance(type, true);
                Item.BypassInitialization = false;
                if (i.Name != null)
                {
                    m_NameNumber = 0;
                    m_NameString = i.Name;
                }
                else if (i.DefaultName != null)
                {
                    m_NameNumber = 0;
                    m_NameString = i.DefaultName;
                }
                i = null;

            }
            catch (Exception e)
            {
                Item.BypassInitialization = false;
                Console.WriteLine("[Erro] Instanciando craft item " + type.Name);
            }

            if (this.m_NameString != null)
                return;

            this.m_NameString = Trads.GetNome(type);
            if (this.m_NameString != null)
            {
                return;
            }

            if (m_NameString != null)
                return;

            var trans = Lang.Trans(m_NameNumber);

            if (trans != null)
            {
                m_NameNumber = 0;
                m_NameString = trans;
            }

            if (Trads.Items.ContainsKey(type.Name))
            {
                m_NameNumber = 0;
                m_NameString = Trads.Items[type.Name];
            }
            else
            {
                var trans2 = Lang.Trans(m_NameNumber);
                if (trans2 != null)
                {
                    m_NameNumber = 0;
                    m_NameString = trans2;
                }
            }
        }

        public Type ItemType
        {
            get
            {
                return this.m_Type;
            }
        }
        public string MessageString
        {
            get
            {
                return this.m_MessageString;
            }
        }
        public int MessageNumber
        {
            get
            {
                return this.m_MessageNumber;
            }
        }
        public string NameString
        {
            get
            {
                return this.m_NameString;
            }
        }
        public int NameNumber
        {
            get
            {
                return this.m_NameNumber;
            }
        }
        public int Amount
        {
            get
            {
                return this.m_Amount;
            }
        }
        public void SendMessage(Mobile from)
        {
            if (this.m_MessageNumber > 0)
                from.SendLocalizedMessage(this.m_MessageNumber);
            else if (!String.IsNullOrEmpty(this.m_MessageString))
                from.SendMessage(this.m_MessageString);
            else
                from.SendLocalizedMessage(502925); // You don't have the resources required to make that item.
        }
    }
}
