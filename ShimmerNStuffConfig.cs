using System.ComponentModel;
using Terraria.ModLoader.Config;
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
    public class ShimmerNStuffConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;
        [ReloadRequired]
        [DefaultValue(true)]
        public bool GenerateBossBagsAndCratesDropsTransmutations;
        [ReloadRequired]
        [DefaultValue(true)]
        public bool GenerateMobDropsTransmutations;
        [ReloadRequired]
        [DefaultValue(true)]
        public bool GenerateBossBagOrCrateToItemRecipes;
        [ReloadRequired]
        [DefaultValue(true)]
        public bool GenerateBannerToItemRecipes;
        [DefaultValue(true)]
        public bool WillMerchantSellItems;
        
        [Range(0, int.MaxValue)]
        [DefaultValue(3)]
        public int DefaultMerchantPriceMultiplier;
        [Range(0, int.MaxValue)]
        [DefaultValue(1)]
        public int DefaultBossBagOrCrateMerchantPriceMultiplier;
        [Range(0, int.MaxValue)]
        [DefaultValue(100)]
        public int DefaultPriceForPricelessItemsInCopperCoins;
         [DefaultValue(true)]
        public bool EndlessAmmoEnabled;
        [Range(0, int.MaxValue)]
        [DefaultValue(100)]
        public int DefaultEndlessAmmoCount;
    }
}
