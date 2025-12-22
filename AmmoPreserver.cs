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
    public class AmmoPreserver : GlobalItem
    {
        public override void OnConsumeAmmo(Item weapon, Item ammo, Player player)
        {
            if (ModContent.GetInstance<ShimmerNStuffConfig>().EndlessAmmoEnabled)
            {

                foreach (Item item in player.inventory)
                {
                    if (item.stack >= ModContent.GetInstance<ShimmerNStuffConfig>().DefaultEndlessAmmoCount && item.type == ammo.type)
                    {
                        ammo.stack += 1;
                        break;
                    }
                }
            }
        }
        // public override void OnConsumeItem(Item item, Player player)
        // {
        //     foreach (Item i in player.inventory)
        //         {
        //             if (item.stack >= ModContent.GetInstance<ShimmerNStuffConfig>().DefaultEndlessAmmoCount && i.type == item.type)
        //             {
        //                 item.stack += 1;
        //                 break;
        //             }
        //         }
        // }
    }
}