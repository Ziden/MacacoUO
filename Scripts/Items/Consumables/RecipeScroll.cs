using System;
using Server.Engines.Craft;
using Server.Mobiles;
using Server.Network;

namespace Server.Items
{
    public class RecipeScroll : Item
    {

        public override bool NaoPodeBancoRP => false;


        private int m_RecipeID;
        public RecipeScroll(Recipe r)
            : this(r.ID)
        {
            var skill = RecipeScrollDefinition.RecipeSkillNameConvert(r.CraftSystem.MainSkill);
            switch(skill)
            {
                case RecipeSkillName.Alchemy: this.Hue = 110; break;
                case RecipeSkillName.Blacksmith: this.Hue = 220; break;
                case RecipeSkillName.Carpentry: this.Hue = 330; break;
                case RecipeSkillName.Cartography: this.Hue = 440; break;
                case RecipeSkillName.Cooking: this.Hue = 550; break;
                case RecipeSkillName.Jewelcrafting: this.Hue = 660; break;
                case RecipeSkillName.Tailoring: this.Hue = 770; break;
                case RecipeSkillName.Tinkering: this.Hue = 880; break;
            }
            Name = "pergaminho de receita de "+skill.ToString();
        }

        [Constructable]
        public RecipeScroll(int recipeID)
            : base(0x2831)
        {
            if(Recipe.Recipes.ContainsKey(recipeID))
            {
                var recipe = Recipe.Recipes[recipeID];
                var skill = RecipeScrollDefinition.RecipeSkillNameConvert(recipe.CraftSystem.MainSkill);
                Name = "pergaminho de receita de " + skill.ToString();
                switch (skill)
                {
                    case RecipeSkillName.Alchemy: this.Hue = 110; break;
                    case RecipeSkillName.Blacksmith: this.Hue = 220; break;
                    case RecipeSkillName.Carpentry: this.Hue = 330; break;
                    case RecipeSkillName.Cartography: this.Hue = 440; break;
                    case RecipeSkillName.Cooking: this.Hue = 550; break;
                    case RecipeSkillName.Jewelcrafting: this.Hue = 660; break;
                    case RecipeSkillName.Tailoring: this.Hue = 770; break;
                    case RecipeSkillName.Tinkering: this.Hue = 880; break;
                }
                this.m_RecipeID = recipeID;
            } else
            {
                this.m_RecipeID = recipeID;
                Name = "Receita Desconhecida " + recipeID;
            }
        }

        public RecipeScroll(Serial serial)
            : base(serial)
        {
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int RecipeID
        {
            get
            {
                return this.m_RecipeID;
            }
            set
            {
                this.m_RecipeID = value;
                this.InvalidateProperties();
            }
        }

        public Recipe Recipe
        {
            get
            {
                if (Recipe.Recipes.ContainsKey(this.m_RecipeID))
                    return Recipe.Recipes[this.m_RecipeID];

                return null;
            }
        }
        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            Recipe r = this.Recipe;

            if (r != null)
                list.Add(1049644, r.TextDefinition.ToString()); // [~1_stuff~]
            else
                list.Add("[ Receita Desconhecida ]");
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(this.GetWorldLocation(), 2))
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
                return;
            }
            Recipe r = this.Recipe;
            if (r != null && from is PlayerMobile)
            {
                PlayerMobile pm = from as PlayerMobile;
                if (!pm.HasRecipe(r))
                {
                    pm.SendMessage(78, "Voce aprendeu a receita. Digite .receitas para ver as receitas"); // You have learned a new recipe: ~1_RECIPE~
                    pm.AcquireRecipe(r);
                    this.Delete();
                }
                else
                {
                    pm.SendMessage("Voce ja conhece esta receita, digite .receitas para ver suas receitas"); // You already know this recipe.
                }
            } else if(r == null)
            {
        
                if(Name.Contains("Receita Desconhecida"))
                {
                    var s = Name.Split(' ');
                    if(s.Length == 3)
                    {
                        try
                        {
                            var i = Int32.Parse(s[2]);
                            m_RecipeID = i;
                            var recipe = Recipe.Recipes[m_RecipeID];
                            var skill = RecipeScrollDefinition.RecipeSkillNameConvert(recipe.CraftSystem.MainSkill);
                            Name = "pergaminho de receita de " + skill.ToString();
                            InvalidateProperties();
                            from.SendMessage("Voce conseguiu entender esta receita...");
                            return;
                        } catch(Exception e)
                        {

                        }
                    }
                }
                from.SendMessage("Esta receita parece estar perdida no tempo...");
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            writer.Write((int)this.m_RecipeID);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        this.m_RecipeID = reader.ReadInt();

                        break;
                    }
            }
        }
    }

    public class DoomRecipeScroll : RecipeScroll
    {
        [Constructable]
        public DoomRecipeScroll()
            : base(Utility.RandomList(355, 356, 456, 585))
        {
        }

        public DoomRecipeScroll(Serial serial)
            : base(serial)
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
