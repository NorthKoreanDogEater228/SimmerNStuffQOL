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
	public class WorldLogic : ModSystem
	{
		int counter = 0;
		public bool needToPlay = true;
		public static List<int> bags = new List<int>();
		List<int> bagContainments = new List<int>();
		Item i = new Item();
		List<int> banned = new List<int>();
		List<int> ids = new List<int>();
		Dictionary<int, Dictionary<IItemDropRule, NPC>> checkRules = new Dictionary<int, Dictionary<IItemDropRule, NPC>>();
		public static Dictionary<int, int> bagValues = new Dictionary<int, int>();
		List<Recipe> bannedRecipes = new List<Recipe>();
		public override void PostUpdateWorld()
		{
			//my testing space
			if (/* needToPlay */ false)
			{
				ids = new List<int>();
				
				for (int ID = -65; ID < NPCLoader.NPCCount; ID++)
				{
					
					Main.NewText("NPC NAME : " + Lang.GetNPCNameValue(ID));
					List<IItemDropRule> rules = Main.ItemDropsDB.GetRulesForNPCID(ID);
					DropRateInfoChainFeed chainFeed = new DropRateInfoChainFeed(1f);
					List<DropRateInfo> dropRates = new List<DropRateInfo>();
					Dictionary<float, List<int>> drops = new Dictionary<float, List<int>>();
					Dictionary<int, Dictionary<float, List<int>>> conditionDictionary = new Dictionary<int, Dictionary<float, List<int>>>();
					Dictionary<int, List<IItemDropRuleCondition>> conditionList = new Dictionary<int, List<IItemDropRuleCondition>>();
					foreach (IItemDropRule rule in rules)
					{
						rule.ReportDroprates(dropRates, chainFeed);
					}
					if (dropRates == null) { continue; }
					foreach (DropRateInfo dropRate in dropRates)
					{

						if (!(banned.Contains(dropRate.itemId)) && !(ids.Contains(dropRate.itemId)) && !(bags.Contains(dropRate.itemId)) /* && !(bagContainments.Contains(dropRate.itemId)) */)
						{
							float round = (float)Math.Round(dropRate.dropRate, 4);
							if (dropRate.conditions != null)
							{

								int hash = 0;
								foreach (IItemDropRuleCondition cond in dropRate.conditions)
								{
									 
									Main.NewText(cond.GetConditionDescription());
									Main.NewText(Lang.GetItemNameValue(dropRate.itemId));
									Main.NewText(dropRate.dropRate);
									hash += cond.GetHashCode();
								}
								if (conditionDictionary.ContainsKey(hash))
								{
									if (conditionDictionary[hash].ContainsKey(/* dropRate.dropRate */round))
									{
										conditionDictionary[hash][/* dropRate.dropRate */round].Add(dropRate.itemId);
										if (!conditionList.ContainsKey(dropRate.itemId))
										{
											conditionList.Add(dropRate.itemId, dropRate.conditions);
										}
										// Main.NewText("added " + Lang.GetItemNameValue(dropRate.itemId));
										// Main.NewText(hash);
									}
									else
									{
										float key = /* dropRate.dropRate */round;
										List<int> ints = new List<int>();
										ints.Add(dropRate.itemId);
										conditionDictionary[hash].Add(key, ints);
										if (!conditionList.ContainsKey(dropRate.itemId))
										{
											conditionList.Add(dropRate.itemId, dropRate.conditions);
										}

										// Main.NewText(dropRate.itemId);
										// Main.NewText("added new droprate " + Lang.GetItemNameValue(dropRate.itemId));
									}

								}
								else
								{
									Dictionary<float, List<int>> dict = new Dictionary<float, List<int>>();
									float key = dropRate.dropRate;
									List<int> ints = new List<int>();
									ints.Add(dropRate.itemId);
									dict.Add(key, ints);
									conditionDictionary.Add(hash, dict);
									if (!conditionList.ContainsKey(dropRate.itemId))
									{
										conditionList.Add(dropRate.itemId, dropRate.conditions);
									}
									//Main.NewText("added new conditions " + Lang.GetItemNameValue(dropRate.itemId));
								}
							}
							else
							{
								Main.NewText(Lang.GetItemNameValue(dropRate.itemId) + " NO CONDITIONS");
								if (drops.ContainsKey(dropRate.dropRate))
								{
									drops[dropRate.dropRate].Add(dropRate.itemId);
								}
								else
								{
									float key = dropRate.dropRate;
									List<int> ints = new List<int>();
									ints.Add(dropRate.itemId);
									drops.Add(key, ints);
								}
							}
						}
					}
					List<float> keys = new List<float>();
					List<List<int>> allItems = new List<List<int>>();
					foreach (KeyValuePair<float, List<int>> entry1 in drops)
					{
						if (!keys.Contains(entry1.Key))
						{
							List<int> values = new List<int>();
							if (entry1.Value.Count > 1)
							{
								foreach (KeyValuePair<float, List<int>> entry2 in drops)
								{
									if (entry1.Value.Intersect(entry2.Value).ToList().Count > 0 && !keys.Contains(entry2.Key))
									{
										values = values.Union(entry1.Value).ToList();
										values = values.Union(entry2.Value).ToList();
										keys.Add(entry1.Key);
										keys.Add(entry2.Key);
									}
								}
							}
							values.RemoveAll(x => ids.Contains(x));
							if (values.Count > 1)
							{
								allItems.Add(values);
								foreach (int value in values)
								{
									ids.Add(value);
								}
							}
						}
					}
					allItems = Utils.FindIntersection(allItems);
					// Main.NewText(allItems.Count);
					foreach (List<int> ints in allItems)
					{
						Main.NewText("CYCLE");
						foreach (int value in ints)
						{
							if (ints.IndexOf(value) < ints.Count - 1)
							{
								Main.NewText(Lang.GetItemNameValue(value) + " TO " + Lang.GetItemNameValue(ints[ints.IndexOf(value) + 1]));
							}
							else
							{
								Main.NewText(Lang.GetItemNameValue(value) + " TO " + Lang.GetItemNameValue(ints[0]));
							}
						}
						Main.NewText("CYCLE");
					}
					keys = new List<float>();
					allItems = new List<List<int>>();
					foreach (KeyValuePair<int, Dictionary<float, List<int>>> entry in conditionDictionary)
					{
						foreach (KeyValuePair<float, List<int>> entry1 in entry.Value)
						{
							foreach (int item in entry1.Value)
							{
								//Main.NewText(Lang.GetItemNameValue(item) + " 1");
							}
							// Main.NewText(entry.Key);
							// Main.NewText(entry1.Key);
							if (!keys.Contains(entry1.Key))
							{
								List<int> values = new List<int>();
								if (entry1.Value.Count > 1)
								{
									foreach (KeyValuePair<float, List<int>> entry2 in entry.Value)
									{
										if (entry1.Value.Intersect(entry2.Value).ToList().Count > 0 && !keys.Contains(entry2.Key))
										{
											values = values.Union(entry1.Value).ToList();
											values = values.Union(entry2.Value).ToList();
											keys.Add(entry1.Key);
											keys.Add(entry2.Key);
											foreach (int item in values)
											{
												//Main.NewText(Lang.GetItemNameValue(item) + " 2");
											}
										}
									}
								}
								values.RemoveAll(x => ids.Contains(x));
								if (values.Count > 1)
								{
									allItems.Add(values);
									foreach (int value in values)
									{
										
										ids.Add(value);
									}
								}
							}
						}

					}
					allItems = Utils.FindIntersection(allItems);
					//Main.NewText(allItems.Count + "allitems");
					foreach (List<int> ints in allItems)
					{

						Main.NewText("CYCLE NEW");
						foreach (int value in ints)
						{
							if (ints.IndexOf(value) < ints.Count - 1)
							{
								Main.NewText(Lang.GetItemNameValue(value) + " TO " + Lang.GetItemNameValue(ints[ints.IndexOf(value) + 1]));

								foreach (IItemDropRuleCondition condition in conditionList[value])
								{
									Main.NewText(condition.GetConditionDescription());
								}
							}
							else
							{
								Main.NewText(Lang.GetItemNameValue(value) + " TO " + Lang.GetItemNameValue(ints[0]));
								foreach (IItemDropRuleCondition condition in conditionList[value])
								{
									Main.NewText(condition.GetConditionDescription());
								}
							}
						}
						Main.NewText("CYCLE NEW");
					}
				}

			}
			needToPlay = false;
		}
		public override void OnWorldUnload()
		{
			needToPlay = true;
		}
		public override void PostAddRecipes()
		{
			foreach (Recipe rec in Main.recipe)
			{
				banned.Add(rec.createItem.type);
			}
			for (int ID = 1; ID < ItemLoader.ItemCount; ID++)
			{

				List<IItemDropRule> rules = Main.ItemDropsDB.GetRulesForItemID(ID);
				if (rules.Count != 0)
				{
					bags.Add(ID);
				}
				i.SetDefaults(ID);
				if (i.maxStack > 1 || ItemID.Sets.ShimmerTransformToItem[ID] > 0)
				{
					banned.Add(ID);
				}
			}
			foreach (int ID in bags)
			{
				List<IItemDropRule> rules = Main.ItemDropsDB.GetRulesForItemID(ID);
				Item item = new Item(ID);
				DropRateInfoChainFeed chainFeed = new DropRateInfoChainFeed(1f);
				List<DropRateInfo> dropRates = new List<DropRateInfo>();
				List<List<int>> cycles = new List<List<int>>();
				Dictionary<float, List<int>> drops = new Dictionary<float, List<int>>();
				Dictionary<int, Dictionary<float, List<int>>> conditionDictionary = new Dictionary<int, Dictionary<float, List<int>>>();
				Dictionary<int, List<IItemDropRuleCondition>> condDictionary = new Dictionary<int, List<IItemDropRuleCondition>>();
				Dictionary<int, List<IItemDropRuleCondition>> conditionList = new Dictionary<int, List<IItemDropRuleCondition>>();
				Dictionary<int, List<List<int>>> duplicates = new Dictionary<int, List<List<int>>>();
				int itemsValue = 0;
				List<int> bannerBan = new List<int>();
				foreach (IItemDropRule rule in rules)
				{
					rule.ReportDroprates(dropRates, chainFeed);
				}
				foreach (DropRateInfo dropRate in dropRates)
				{
					if (dropRate.conditions == null)
					{
						bannerBan.Add(dropRate.itemId);

					}
				}
				foreach (DropRateInfo dropRate in dropRates)
				{
					Item b = new Item();
					b.SetDefaults(dropRate.itemId);
					itemsValue += (int)Math.Floor((dropRate.stackMin * b.value * dropRate.dropRate) / 5);

					if (!b.IsACoin && ModContent.GetInstance<ShimmerNStuffConfig>().GenerateBossBagOrCrateToItemRecipes)
					{
						int banner = ID;
						Recipe recipe = Recipe.Create(dropRate.itemId, dropRate.stackMin);
						List<int> con = new List<int>();
						recipe.AddIngredient(banner, 1);
						recipe.DisableDecraft();
						

						List<Recipe> bannedRec = new List<Recipe>();
						if (!bannedRec.Contains(recipe))
						{
							recipe.Register();
							bannedRec.Add(recipe);
						}
						//}
						// if (dropRate.conditions != null && !bannerBan.Contains(dropRate.itemId))
						// {
						// 	List<IItemDropRuleCondition> bannerConds = new List<IItemDropRuleCondition>();
						// 	bannerConds = dropRate.conditions;

						// 	List<int> l = new List<int>();


						// 	foreach (IItemDropRuleCondition cond in bannerConds)
						// 	{




						// 		recipe.AddCondition(Utils.FromDropCondition(cond, null));
						// 		l.Add(cond.GetHashCode());


						// 	}
						// 	if (!duplicates.ContainsKey(dropRate.itemId))
						// 	{

						// 		List<Recipe> bannedRec = new List<Recipe>();
						// 		if (!bannedRec.Contains(recipe))
						// 		{
						// 			recipe.Register();
						// 			bannedRec.Add(recipe);
						// 		}
						// 		duplicates.Add(dropRate.itemId, [l]);
						// 	}
						// 	else if (!(duplicates[dropRate.itemId].Contains(l)))
						// 	{
						// 		List<Recipe> bannedRec = new List<Recipe>();
						// 		if (!bannedRec.Contains(recipe))
						// 		{
						// 			recipe.Register();
						// 			bannedRec.Add(recipe);
						// 		}
						// 		duplicates[dropRate.itemId].Add(l);
						// 	}
						// }



					}
					if (!ModContent.GetInstance<ShimmerNStuffConfig>().GenerateBossBagsAndCratesDropsTransmutations)
					{
						continue;
					}
					//float round = (float)Math.Round(dropRate.dropRate, 4);
					bagContainments.Add(dropRate.itemId);
					if (!(banned.Contains(dropRate.itemId)) && !(ids.Contains(dropRate.itemId)))
					{
						if (dropRate.conditions == null)
						{
							if (drops.ContainsKey(dropRate.dropRate))
							{
								drops[dropRate.dropRate].Add(dropRate.itemId);
							}
							else
							{
								float key = dropRate.dropRate;
								List<int> ints = new List<int>();
								ints.Add(dropRate.itemId);
								drops.Add(key, ints);
							}
						}
						else
						{
							int hash = 0;
							foreach (IItemDropRuleCondition cond in dropRate.conditions)
							{
								hash += cond.GetHashCode();
							}
							if (!condDictionary.ContainsKey(hash))
							{
								condDictionary.Add(hash, dropRate.conditions);
							}
							if (conditionDictionary.ContainsKey(hash))
							{
								if (conditionDictionary[hash].ContainsKey(dropRate.dropRate/* round */))
								{
									conditionDictionary[hash][dropRate.dropRate/* round */].Add(dropRate.itemId);
									// if (!conditionList.ContainsKey(dropRate.itemId))
									// {
									// 	conditionList.Add(dropRate.itemId, dropRate.conditions);
									// }
									//condDictionary[hash][round].Add(dropRate.conditions);
								}
								else
								{
									float key = dropRate.dropRate/* round */;
									List<int> ints = new List<int>();
									ints.Add(dropRate.itemId);
									conditionDictionary[hash].Add(key, ints);
									// if (!conditionList.ContainsKey(dropRate.itemId))
									// {
									// 	conditionList.Add(dropRate.itemId, dropRate.conditions);
									// }
									//condDictionary[hash][round].Add(dropRate.conditions);
								}

							}
							else
							{
								Dictionary<float, List<int>> dict = new Dictionary<float, List<int>>();
								float key = dropRate.dropRate;
								List<int> ints = new List<int>();
								ints.Add(dropRate.itemId);
								dict.Add(key, ints);
								conditionDictionary.Add(hash, dict);
								// if (!conditionList.ContainsKey(dropRate.itemId))
								// {
								// 	conditionList.Add(dropRate.itemId, dropRate.conditions);
								// }
								// Dictionary<float, List<List<IItemDropRuleCondition>>> d = new Dictionary<float, List<List<IItemDropRuleCondition>>>();
								// List<List<IItemDropRuleCondition>> l = new List<List<IItemDropRuleCondition>>();
								// l.Add(dropRate.conditions);
								// d.Add(dropRate.dropRate, l);
								// condDictionary.Add(hash, d);
							}
						}
					}
				}
				bagValues.Add(ID, itemsValue);
				List<float> keys = new List<float>();
				List<List<int>> allItems = new List<List<int>>();
				foreach (KeyValuePair<float, List<int>> entry1 in drops)
				{
					if (!keys.Contains(entry1.Key))
					{
						List<int> values = new List<int>();
						if (entry1.Value.Count > 1)
						{
							foreach (KeyValuePair<float, List<int>> entry2 in drops)
							{
								if (entry1.Value.Intersect(entry2.Value).ToList().Count > 0 && !keys.Contains(entry2.Key))
								{
									values = values.Union(entry1.Value).ToList();
									values = values.Union(entry2.Value).ToList();
									keys.Add(entry1.Key);
									keys.Add(entry2.Key);
								}
							}
						}
						values.RemoveAll(x => ids.Contains(x));
						if (values.Count > 1)
						{
							allItems.Add(values);
							foreach (int value in values)
							{
								ids.Add(value);
							}
						}
					}
				}
				allItems = Utils.FindIntersection(allItems);
				Main.NewText(allItems.Count);
				foreach (List<int> ints in allItems)
				{
					
					foreach (int value in ints)
					{
						if (ints.IndexOf(value) < ints.Count - 1)
						{
							Main.NewText(Lang.GetItemNameValue(value) + " TO " + Lang.GetItemNameValue(ints[ints.IndexOf(value) + 1]));
							Recipe recipe = Recipe.Create(value);
							recipe.AddCustomShimmerResult(ints[ints.IndexOf(value) + 1], 1);
							recipe.AddCondition(Condition.InEvilBiome);
							recipe.AddCondition(Condition.NotInEvilBiome);
							List<Recipe> bannedRec = new List<Recipe>();
							if (!bannedRec.Contains(recipe))
							{
								recipe.Register();
								bannedRec.Add(recipe);
							}
						}
						else
						{
							Main.NewText(Lang.GetItemNameValue(value) + " TO " + Lang.GetItemNameValue(ints[0]));
							Recipe recipe = Recipe.Create(value);
							recipe.AddCustomShimmerResult(ints[0], 1);
							recipe.AddCondition(Condition.InEvilBiome);
							recipe.AddCondition(Condition.NotInEvilBiome);
							List<Recipe> bannedRec = new List<Recipe>();
							if (!bannedRec.Contains(recipe))
							{
								recipe.Register();
								bannedRec.Add(recipe);
							}
						}
					}
				
				}
				keys = new List<float>();
				allItems = new List<List<int>>();
				foreach (KeyValuePair<int, Dictionary<float, List<int>>> entry in conditionDictionary)
				{
					foreach (KeyValuePair<float, List<int>> entry1 in entry.Value)
					{
						
						if (!keys.Contains(entry1.Key))
						{
							List<int> values = new List<int>();
							if (entry1.Value.Count > 1)
							{
								foreach (KeyValuePair<float, List<int>> entry2 in entry.Value)
								{
									if (entry1.Value.Intersect(entry2.Value).ToList().Count > 0 && !keys.Contains(entry2.Key))
									{
										values = values.Union(entry1.Value).ToList();
										values = values.Union(entry2.Value).ToList();
										keys.Add(entry1.Key);
										keys.Add(entry2.Key);
										
									}
								}
							}
							values.RemoveAll(x => ids.Contains(x));
							if (values.Count > 1)
							{

								allItems.Add(values);
								foreach (int value in values)
								{
									conditionList.Add(value, condDictionary[entry.Key]);
									ids.Add(value);
								}
							}
						}
					}

				}
				allItems = Utils.FindIntersection(allItems);
				foreach (List<int> ints in allItems)
				{

					foreach (int value in ints)
					{
						if (ints.IndexOf(value) < ints.Count - 1)
						{
							Recipe recipe = Recipe.Create(value);
							recipe.AddCustomShimmerResult(ints[ints.IndexOf(value) + 1], 1);
							recipe.AddCondition(Condition.InEvilBiome);
							recipe.AddCondition(Condition.NotInEvilBiome);


							// foreach (IItemDropRuleCondition condition in conditionList[value])
							// {
							// 	// NPC npc = new NPC();
							// 	// npc.SetDefaults(ID);

							// 	recipe.AddDecraftCondition(Utils.FromDropCondition(condition, null));
							// }
							recipe.Register();

						}
						else
						{

							Recipe recipe = Recipe.Create(value);
							recipe.AddCustomShimmerResult(ints[0], 1);
							recipe.AddCondition(Condition.InEvilBiome);
							recipe.AddCondition(Condition.NotInEvilBiome);
							// foreach (IItemDropRuleCondition condition in conditionList[value])
							// {
							// 	// NPC npc = new NPC();
							// 	// npc.SetDefaults(ID);
							// 	recipe.AddDecraftCondition(Utils.FromDropCondition(condition, null));
							// }
							recipe.Register();
						}
					}
				}
				// List<float> keys = new List<float>();
				// foreach (KeyValuePair<float, List<int>> entry1 in drops)
				// {
				// 	List<int> values = new List<int>();
				// 	if (entry1.Value.Count > 1)
				// 	{
				// 		foreach (KeyValuePair<float, List<int>> entry2 in drops)
				//     	{
				// 			if (entry1.Value.Intersect(entry2.Value).ToList().Count > 0 && !(keys.Contains(entry2.Key)))
				// 			{
				// 				values = values.Union(entry1.Value).ToList();
				// 				values = values.Union(entry2.Value).ToList();
				// 				keys.Add(entry1.Key);
				// 				keys.Add(entry2.Key);
				// 			}	
				//     	}
				// 	}
				// 	if (values.Count > 1)
				// 	{
				// 	if (values.Count > 1)
				// 	{ Main.NewText("CYCLE");
				// 	foreach (int value in values)
				// 	{
				// 		if (values.IndexOf(value) < values.Count - 1)
				// 		{
				// 			//Main.NewText(Lang.GetItemNameValue(value) + " TO " + Lang.GetItemNameValue(values[values.IndexOf(value) + 1]));
				// 			Recipe recipe = Recipe.Create(value);
				// 			recipe.AddCustomShimmerResult(values[values.IndexOf(value) + 1], 1);
				// 			recipe.AddCondition(Condition.InEvilBiome);
				// 			recipe.AddCondition(Condition.NotInEvilBiome);
				// 			recipe.Register();
				// 		}
				// 		else
				// 		{
				// 			//Main.NewText(Lang.GetItemNameValue(value) + " TO " + Lang.GetItemNameValue(values[0]));
				// 			Recipe recipe = Recipe.Create(value);
				// 			recipe.AddCustomShimmerResult(values[0], 1);
				// 			recipe.AddCondition(Condition.InEvilBiome);
				// 			recipe.AddCondition(Condition.NotInEvilBiome);
				// 			recipe.Register();
				// 		}
				// 	}
				// 	Main.NewText("CYCLE");
				// 	foreach (int value in values)
				// 	{
				// 		ids.Add(value);
				// 	}
				// 	}
				// 	}     
				// }
			}
			Dictionary<int, List<int>> perBannerBan = new Dictionary<int, List<int>>();
			for (int ID = -65; ID < NPCLoader.NPCCount; ID++)
			{
				List<IItemDropRule> rules = Main.ItemDropsDB.GetRulesForNPCID(ID);
				DropRateInfoChainFeed chainFeed = new DropRateInfoChainFeed(1f);
				List<DropRateInfo> dropRates = new List<DropRateInfo>();
				Dictionary<float, List<int>> drops = new Dictionary<float, List<int>>();
				Dictionary<int, Dictionary<float, List<int>>> conditionDictionary = new Dictionary<int, Dictionary<float, List<int>>>();
				Dictionary<int, List<IItemDropRuleCondition>> condDictionary = new Dictionary<int, List<IItemDropRuleCondition>>();
				Dictionary<int, List<IItemDropRuleCondition>> conditionList = new Dictionary<int, List<IItemDropRuleCondition>>();
				Dictionary<int, List<List<int>>> duplicates = new Dictionary<int, List<List<int>>>();
				NPC npc = new NPC();
				npc.SetDefaults(ID);
				List<int> bannerBan = new List<int>();
				List<int> idBan = new List<int>();
				foreach (IItemDropRule rule in rules)
				{
					rule.ReportDroprates(dropRates, chainFeed);
				}
				//if (dropRates == null) { continue; }
				foreach (DropRateInfo dropRate in dropRates)
				{
					if (dropRate.conditions == null)
					{
						bannerBan.Add(dropRate.itemId);

					}
				}
				foreach (DropRateInfo dropRate in dropRates)
				{
					Item b = new Item();
					b.SetDefaults(dropRate.itemId);
					if (Item.NPCtoBanner(ID) != 0 && !b.IsACoin /* && b.maxStack < 1 */ && ModContent.GetInstance<ShimmerNStuffConfig>().GenerateBannerToItemRecipes)
					{
						int banner = Item.BannerToItem(Item.NPCtoBanner(ID));
						Recipe recipe = Recipe.Create(dropRate.itemId, dropRate.stackMax);
						List<int> con = new List<int>();
						recipe.AddIngredient(banner, 1);
						recipe.DisableDecraft();

						if (dropRate.conditions == null)
						{

							List<Recipe> bannedRec = new List<Recipe>();
							if (!bannedRec.Contains(recipe))
							{
								if (perBannerBan.ContainsKey(banner))
								{
									if (perBannerBan[banner].Contains(dropRate.itemId))
									{
										continue;
									}
									else
									{
										
										perBannerBan[banner].Add(dropRate.itemId);
									}
								}
								else
								{
									List<int> l = new List<int>();
									l.Add(dropRate.itemId);
									perBannerBan.Add(banner, l);
									
								}

								recipe.Register();
								bannedRec.Add(recipe);
								idBan.Add(dropRate.itemId);

							}
						}
						// if (dropRate.conditions != null && !bannerBan.Contains(dropRate.itemId))
						// {
						// 	List<IItemDropRuleCondition> bannerConds = new List<IItemDropRuleCondition>();
						// 	bannerConds = dropRate.conditions;

						// 	List<int> l = new List<int>();


						// 	foreach (IItemDropRuleCondition cond in bannerConds)
						// 	{




						// 		recipe.AddCondition(Utils.FromDropCondition(cond, npc));
						// 		l.Add(cond.GetHashCode());


						// 	}
						// 	if (!duplicates.ContainsKey(dropRate.itemId))
						// 	{

						// 		recipe.Register();
						// 		duplicates.Add(dropRate.itemId, [l]);
						// 	}
						// 	else if (!(duplicates[dropRate.itemId].Contains(l)))
						// 	{
						// 		recipe.Register();
						// 		duplicates[dropRate.itemId].Add(l);
						// 	}
						// }



					}
					if (!ModContent.GetInstance<ShimmerNStuffConfig>().GenerateMobDropsTransmutations)
					{
						continue;
					}
					if (!(banned.Contains(dropRate.itemId)) && !(ids.Contains(dropRate.itemId)) && !(bags.Contains(dropRate.itemId)) && !(bagContainments.Contains(dropRate.itemId)))
					{

						//float round = (float)Math.Round(dropRate.dropRate, 4);
						if (dropRate.conditions != null)
						{
							int hash = 0;
							foreach (IItemDropRuleCondition cond in dropRate.conditions)
							{
								hash += cond.GetHashCode();
							}
							if (!condDictionary.ContainsKey(hash))
							{
								condDictionary.Add(hash, dropRate.conditions);
							}
							if (conditionDictionary.ContainsKey(hash))
							{
								if (conditionDictionary[hash].ContainsKey(dropRate.dropRate/* round */))
								{
									conditionDictionary[hash][dropRate.dropRate/* round */].Add(dropRate.itemId);
									// if (!conditionList.ContainsKey(dropRate.itemId))
									// {
									// 	conditionList.Add(dropRate.itemId, dropRate.conditions);
									// }
									//condDictionary[hash][round].Add(dropRate.conditions);
								}
								else
								{
									float key = dropRate.dropRate/* round */;
									List<int> ints = new List<int>();
									ints.Add(dropRate.itemId);
									conditionDictionary[hash].Add(key, ints);
									// if (!conditionList.ContainsKey(dropRate.itemId))
									// {
									// 	conditionList.Add(dropRate.itemId, dropRate.conditions);
									// }
									//condDictionary[hash][round].Add(dropRate.conditions);
								}

							}
							else
							{
								Dictionary<float, List<int>> dict = new Dictionary<float, List<int>>();
								float key = dropRate.dropRate;
								List<int> ints = new List<int>();
								ints.Add(dropRate.itemId);
								dict.Add(key, ints);
								conditionDictionary.Add(hash, dict);
								// if (!conditionList.ContainsKey(dropRate.itemId))
								// {
								// 	conditionList.Add(dropRate.itemId, dropRate.conditions);
								// }
								// Dictionary<float, List<List<IItemDropRuleCondition>>> d = new Dictionary<float, List<List<IItemDropRuleCondition>>>();
								// List<List<IItemDropRuleCondition>> l = new List<List<IItemDropRuleCondition>>();
								// l.Add(dropRate.conditions);
								// d.Add(dropRate.dropRate, l);
								// condDictionary.Add(hash, d);
							}
						}
						else
						{
							if (drops.ContainsKey(dropRate.dropRate))
							{
								drops[dropRate.dropRate].Add(dropRate.itemId);
							}
							else
							{
								float key = dropRate.dropRate;
								List<int> ints = new List<int>();
								ints.Add(dropRate.itemId);
								drops.Add(key, ints);
							}
						}
					}
				}
				List<float> keys = new List<float>();
				List<List<int>> allItems = new List<List<int>>();
				foreach (KeyValuePair<float, List<int>> entry1 in drops)
				{
					if (!keys.Contains(entry1.Key))
					{
						List<int> values = new List<int>();
						if (entry1.Value.Count > 1)
						{
							foreach (KeyValuePair<float, List<int>> entry2 in drops)
							{
								if (entry1.Value.Intersect(entry2.Value).ToList().Count > 0 && !keys.Contains(entry2.Key))
								{
									values = values.Union(entry1.Value).ToList();
									values = values.Union(entry2.Value).ToList();
									keys.Add(entry1.Key);
									keys.Add(entry2.Key);
								}
							}
						}
						values.RemoveAll(x => ids.Contains(x));
						if (values.Count > 1)
						{
							allItems.Add(values);
							foreach (int value in values)
							{
								ids.Add(value);
							}
						}
					}
				}
				allItems = Utils.FindIntersection(allItems);
				foreach (List<int> ints in allItems)
				{
					foreach (int value in ints)
					{
						if (ints.IndexOf(value) < ints.Count - 1)
						{
							Main.NewText(Lang.GetItemNameValue(value) + " TO " + Lang.GetItemNameValue(ints[ints.IndexOf(value) + 1]));
							Recipe recipe = Recipe.Create(value);
							recipe.AddCustomShimmerResult(ints[ints.IndexOf(value) + 1], 1);
							recipe.AddCondition(Condition.InEvilBiome);
							recipe.AddCondition(Condition.NotInEvilBiome);
							List<Recipe> bannedRec = new List<Recipe>();
							if (!bannedRec.Contains(recipe))
							{
								recipe.Register();
								bannedRec.Add(recipe);
							}
						}
						else
						{
							Main.NewText(Lang.GetItemNameValue(value) + " TO " + Lang.GetItemNameValue(ints[0]));
							Recipe recipe = Recipe.Create(value);
							recipe.AddCustomShimmerResult(ints[0], 1);
							recipe.AddCondition(Condition.InEvilBiome);
							recipe.AddCondition(Condition.NotInEvilBiome);
							List<Recipe> bannedRec = new List<Recipe>();
							if (!bannedRec.Contains(recipe))
							{
								recipe.Register();
								bannedRec.Add(recipe);
							}
						}
					}
				}
				keys = new List<float>();
				allItems = new List<List<int>>();
				foreach (KeyValuePair<int, Dictionary<float, List<int>>> entry in conditionDictionary)
				{
					foreach (KeyValuePair<float, List<int>> entry1 in entry.Value)
					{
						foreach (int item in entry1.Value)
						{
						}
						if (!keys.Contains(entry1.Key))
						{
							List<int> values = new List<int>();
							if (entry1.Value.Count > 1)
							{
								foreach (KeyValuePair<float, List<int>> entry2 in entry.Value)
								{
									if (entry1.Value.Intersect(entry2.Value).ToList().Count > 0 && !keys.Contains(entry2.Key))
									{
										values = values.Union(entry1.Value).ToList();
										values = values.Union(entry2.Value).ToList();
										keys.Add(entry1.Key);
										keys.Add(entry2.Key);
										foreach (int item in values)
										{
										}
									}
								}
							}
							values.RemoveAll(x => ids.Contains(x));
							if (values.Count > 1)
							{

								allItems.Add(values);
								foreach (int value in values)
								{
									conditionList.Add(value, condDictionary[entry.Key]);
									ids.Add(value);
								}
							}
						}
					}

				}
				allItems = Utils.FindIntersection(allItems);
				foreach (List<int> ints in allItems)
				{

					foreach (int value in ints)
					{
						if (ints.IndexOf(value) < ints.Count - 1)
						{
							Recipe recipe = Recipe.Create(value);
							recipe.AddCustomShimmerResult(ints[ints.IndexOf(value) + 1], 1);
							recipe.AddCondition(Condition.InEvilBiome);
							recipe.AddCondition(Condition.NotInEvilBiome);


							// foreach (IItemDropRuleCondition condition in conditionList[value])
							// {
							// 	NPC npc = new NPC();
							// 	npc.SetDefaults(ID);

							// 	recipe.AddDecraftCondition(Utils.FromDropCondition(condition, npc));
							// }

							List<Recipe> bannedRec = new List<Recipe>();
							if (!bannedRec.Contains(recipe))
							{
								recipe.Register();
								bannedRec.Add(recipe);
							}
						}
						else
						{

							Recipe recipe = Recipe.Create(value);
							recipe.AddCustomShimmerResult(ints[0], 1);
							recipe.AddCondition(Condition.InEvilBiome);
							recipe.AddCondition(Condition.NotInEvilBiome);
							//foreach (IItemDropRuleCondition condition in conditionList[value])
							// {
							// 	NPC npc = new NPC();
							// 	npc.SetDefaults(ID);
							// 	recipe.AddDecraftCondition(Utils.FromDropCondition(condition, npc));
							// }
							List<Recipe> bannedRec = new List<Recipe>();
							if (!bannedRec.Contains(recipe))
							{
								recipe.Register();
								bannedRec.Add(recipe);
							}
						}
					}
				}
			}
		}
	}
}
// oldest analysing method
// if (ids.Contains(dropRate.itemId))
// {
// 	continue;
// }

// int craftTopica = dropRate.itemId;
// DropRateInfo topicaRate = dropRate;

// foreach (DropRateInfo rate in dropRates)
// {
// 	if (ids.Contains(rate.itemId))
// 	{
// 		continue;
// 	}

// 	if (rate.dropRate == dropRate.dropRate && !(ids.Contains(craftTopica)) && !(ids.Contains(rate.itemId)) && dropRates.IndexOf(rate) > dropRates.IndexOf(topicaRate) && !(banned.Contains(craftTopica)) && !(banned.Contains(rate.itemId)))
// 	{
// 		Main.NewText(Lang.GetItemNameValue(craftTopica) + " TO " + Lang.GetItemNameValue(rate.itemId));
// 		ids.Add(craftTopica);
// 		topicaRate = rate;
// 		craftTopica = rate.itemId;
// 	}
// 	if (dropRates.IndexOf(rate) == dropRates.Count - 1 && !(ids.Contains(craftTopica)) && dropRate.itemId != craftTopica && !(banned.Contains(craftTopica)))
// 	{
// 		Main.NewText(Lang.GetItemNameValue(craftTopica) + " TO " + Lang.GetItemNameValue(dropRate.itemId));
// 		ids.Add(craftTopica);
// 	}
// }