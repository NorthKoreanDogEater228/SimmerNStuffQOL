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
namespace SimmerNStuffQOL.NPCs
{
    public class MerchantShopHandler : GlobalNPC
    {
        static Dictionary<int, int> bagValues = WorldLogic.bagValues;
        static int defaultValue = 100;
        static int multiplier = 3;
        public override void ModifyActiveShop(NPC npc, string shopName, Item[] items)
        {
            
            if (npc.type == NPCID.Merchant && ModContent.GetInstance<ShimmerNStuffConfig>().WillMerchantSellItems)
            {
                Player player = Main.player[Main.myPlayer];
                for (int slot = 0; slot < 10; slot++)
                {
                    if (player.inventory[slot].type == 0)
                    {
                        continue;
                    }
                    
                    for (int i = 0; i < items.Length; i++)
                    {
                        if (items[i] == null)
                        {
                            
                            int slotId = 0;
                            slotId = player.inventory[i].type;
                            Item it = new Item();
                            items[i] = it;
                            items[i].SetDefaults(player.inventory[slot].type);
                            if (player.inventory[slot].value > 0)
                            {
                                items[i].shopCustomPrice = Item.buyPrice(copper: (player.inventory[slot].value * ModContent.GetInstance<ShimmerNStuffConfig>().DefaultMerchantPriceMultiplier) / 5);
                            }
                            else if (bagValues.ContainsKey(items[i].type))
                            {
                                items[i].shopCustomPrice = Item.buyPrice(copper: bagValues[items[i].type] * ModContent.GetInstance<ShimmerNStuffConfig>().DefaultBossBagOrCrateMerchantPriceMultiplier);
                            }
                            else
                            {
                                items[i].shopCustomPrice = Item.buyPrice(copper: ModContent.GetInstance<ShimmerNStuffConfig>().DefaultPriceForPricelessItemsInCopperCoins);
                            }
                            
                            break;
                        }
                        

                    }
                }


            }
        }

    }
}