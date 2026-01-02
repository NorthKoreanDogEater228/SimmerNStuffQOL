using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using Terraria.UI;
using Terraria.UI.Chat;
using static Terraria.ModLoader.ModContent;

namespace SimmerNStuffQOL
{
    public class Utils : ModSystem
    {
        public static Dictionary<IItemDropRuleCondition, bool> itemDropConds = new Dictionary<IItemDropRuleCondition, bool>();
        public static IItemDropRuleCondition condition;
        public static Dictionary<int, bool> genericRules = new Dictionary<int, bool>();
        
        public static List<List<int>> FindIntersection(List<List<int>> list)
        {
            bool needToRepeat = false;
            List<int> values = new List<int>();
            List<List<int>> newList = new List<List<int>>();
            foreach (List<int> idList1 in list)
            {

                List<int> union = new List<int>();
                if (!values.Contains(list.IndexOf(idList1)))
                {
                    foreach (List<int> idList2 in list)
                    {
                        if (idList1.Intersect(idList2).ToList().Count > 0 && !(values.Contains(list.IndexOf(idList2))) && list.IndexOf(idList1) != list.IndexOf(idList2))
                        {
                            needToRepeat = true;
                            union = union.Union(idList1).ToList();
                            union = union.Union(idList2).ToList();
                            values.Add(list.IndexOf(idList1));
                            values.Add(list.IndexOf(idList2));
                        }
                    }
                    newList.Add(union);
                }
            }
            if (needToRepeat)
            {
                newList = FindIntersection(newList);
                return newList;
            }
            else
            {
                return list;
            }
        }
        public static bool isDropping()
        {
            var dropInfo = new DropAttemptInfo
            {
                player = Main.LocalPlayer
            };
            return condition.CanDrop(dropInfo);
        }
        public static Condition FromDropCondition(IItemDropRuleCondition dropCondition, NPC npc)
        {
            if (!itemDropConds.ContainsKey(dropCondition))
            {
                itemDropConds.Add(dropCondition, false);
            }
            
            var dropInfo = new DropAttemptInfo
            {
                npc = npc,
                player = Main.LocalPlayer
            };
            
            bool canDrop = dropCondition.CanDrop(dropInfo);
            //return new Condition(LocalizedText.Empty, () => dropCondition.CanDrop(dropInfo));
            return new Condition(LocalizedText.Empty, () => itemDropConds[dropCondition]);
        }
        public static bool CanDropByCond(IItemDropRuleCondition dropCondition, NPC npc)
        {
            var dropInfo = new DropAttemptInfo
            {
                npc = npc,
                player = Main.LocalPlayer
                
            };
            return dropCondition.CanDrop(dropInfo);
        }
    }
}