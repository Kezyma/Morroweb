using EspConverter.Models.TES3;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Runtime.CompilerServices;
using System.Web;

namespace EspConverter
{
    internal class Program
    {
        private const string tes3convUrl = "https://github.com/Greatness7/tes3conv/releases/download/v0.1.0/windows-latest.zip";
        static void Main(string[] args)
        {
            var espDir = args[0];
            var outputDir = args[1];

            var jsonDir = Path.GetFullPath("json");
            if (Directory.Exists(jsonDir)) Directory.Delete(jsonDir, true);
            Directory.CreateDirectory(jsonDir);

            if (string.IsNullOrEmpty(espDir))
            {
                Console.WriteLine("No esp/esm directory specified.");
            }
            else if (string.IsNullOrEmpty(outputDir))
            {
                Console.WriteLine("No output directory specified.");
            }
            else
            {
                ExtractEsp(espDir, jsonDir);
                GenerateJson(jsonDir, outputDir);
            }
            Console.ReadLine();
        }

        public static string PurgeString(string val)
        {
            return HttpUtility.UrlEncode(val
                .Replace(" ", "_")
                .Replace("-", "_")
                .Replace("'", "")
                .Replace(",", "")
                .Replace("`", "")
                .Replace("(", "_")
                .Replace(")", "_")
                .Replace("[", "_")
                .Replace("]", "_")
                .Replace("{", "_")
                .Replace("}", "_")
                .ToLower());
        }

        public static void ExtractEsp(string espDir, string jsonDir)
        {
            Console.WriteLine("Extracting Esps");
            // Get tes3conv.
            var tes3convDir = Path.GetFullPath("tes3conv");
            if (!Directory.Exists(tes3convDir)) Directory.CreateDirectory(tes3convDir);
            var tes3convExe = Path.Combine(tes3convDir, "tes3conv.exe");
            var tes3convZip = Path.Combine(tes3convDir, "tes3conv.zip");
            if (!File.Exists(tes3convExe))
            {
                Console.WriteLine("tes3conv not present, downloading.");
                var client = new WebClient();
                client.DownloadFile(tes3convUrl, tes3convZip);
                ZipFile.ExtractToDirectory(tes3convZip, tes3convDir, true);
                Console.WriteLine("tes3conv downloaded.");
            }

            // Find all the esp/esm files and convert to json.
            Console.WriteLine("Converting esp/esm files to json.");
            var dataFiles = Directory.GetFiles(espDir);
            var espFiles = dataFiles.Where(x => x.EndsWith(".esp") || x.EndsWith(".esm")).ToList();
            var fileNames = new List<string>();
            foreach (var file in espFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var jsonDest = Path.Combine(jsonDir, $"{fileName}.json");
                var command = $"\"{tes3convExe}\" \"{file}\" \"{jsonDest}\"";
                Process.Start(command).WaitForExit();
                Console.WriteLine($"Converted {fileName} to json.");
                fileNames.Add(fileName);
            }
            Console.WriteLine("Esp extraction complete.");
        }

        public static void GenerateJson(string jsonDir, string outputDir)
        {
            Console.WriteLine("Generating Json");

            var dataFiles = Directory.GetFiles(jsonDir);
            var jsonFiles = dataFiles.Where(x => x.EndsWith(".json")).Select(x => Path.GetFileNameWithoutExtension(x)).ToList();

            // Calculate a load order.
            var forceOrder = new[] { "Morrowind", "Tribunal", "Bloodmoon" };
            var finalOrder = forceOrder.ToList();
            finalOrder.AddRange(jsonFiles.Where(x => !forceOrder.Contains(x)));

            // Overwrite records and export.
            var outputDict = new Dictionary<string, Dictionary<string, object>>();
            var dialogueDict = new Dictionary<string, Dictionary<string, object>>();
            foreach (var key in finalOrder)
            {
                dynamic jsonData = JsonConvert.DeserializeObject(File.ReadAllText(Path.Combine(jsonDir, $"{key}.json")));
                string currentDialogueTopic = string.Empty, currentDialogueType = string.Empty;
                foreach (var record in (JArray)jsonData)
                {
                    var objectType = record.Value<string>("type");
                    var className = $"TES3_{objectType}";
                    var classType = System.Reflection.Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(x => x.Name == className);
                    var converted = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(record), classType);

                    switch (objectType)
                    {
                        #region System
                        case "GameSetting":
                            if (!outputDict.ContainsKey("gmst")) outputDict["gmst"] = [];
                            var gmst = (converted as TES3_GameSetting);
                            outputDict["gmst"][gmst.id] = gmst.value.data;
                            break;
                        case "GlobalVariable":
                            if (!outputDict.ContainsKey("global")) outputDict["global"] = [];
                            var global = (converted as TES3_GlobalVariable);
                            outputDict["global"][global.id] = global.value;
                            break;
                        case "Script":
                            if (!outputDict.ContainsKey("script")) outputDict["script"] = [];
                            var script = (converted as TES3_Script);
                            outputDict["script"][script.id] = script.text.Split("\r\n").Select(x => x.Replace("\t", "")).Where(x => !x.Trim().StartsWith(";") && !string.IsNullOrWhiteSpace(x)).ToArray();
                            break;
                        #endregion

                        #region Stats
                        case "Skill":
                            if (!outputDict.ContainsKey("skill")) outputDict["skill"] = [];
                            var skill = (converted as TES3_Skill);
                            outputDict["skill"][skill.skill_id] = new Dictionary<string, object>
                                {
                                    { "Description", skill.description },
                                    { "Attribute", skill.data.governing_attribute },
                                    { "Specialisation", skill.data.specialization },
                                    { "Actions", skill.data.actions }
                                };
                            break;
                        case "Class":
                            if (!outputDict.ContainsKey("class")) outputDict["class"] = [];
                            var classObj = (converted as TES3_Class);
                            outputDict["class"][classObj.id] = new Dictionary<string, object>
                                {
                                    { "Name", classObj.name },
                                    { "Description", classObj.description },
                                    { "Specialisation", classObj.data.specialization },
                                    { "Attributes", new [] { classObj.data.attribute1, classObj.data.attribute2 } },
                                    { "Major", new [] { classObj.data.major1, classObj.data.major2, classObj.data.major3, classObj.data.major4, classObj.data.major5 } },
                                    { "Minor", new [] { classObj.data.minor1, classObj.data.minor2, classObj.data.minor3, classObj.data.minor4, classObj.data.minor5 } },
                                    { "Playable", classObj.data.flags.Contains("PLAYABLE") },
                                    { "Services", classObj.data.services.Split("|").Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray() },
                                    { "Flags", classObj.data.flags.Split("|").Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray() },
                                };
                            break;
                        case "Race":
                            if (!outputDict.ContainsKey("race")) outputDict["race"] = [];
                            var race = (converted as TES3_Race);
                            var skillBonuses = new Dictionary<string, object>();
                            if (race.data.skill_bonuses.skill_0 != "None") skillBonuses[race.data.skill_bonuses.skill_0] = race.data.skill_bonuses.bonus_0;
                            if (race.data.skill_bonuses.skill_1 != "None") skillBonuses[race.data.skill_bonuses.skill_1] = race.data.skill_bonuses.bonus_1;
                            if (race.data.skill_bonuses.skill_2 != "None") skillBonuses[race.data.skill_bonuses.skill_2] = race.data.skill_bonuses.bonus_2;
                            if (race.data.skill_bonuses.skill_3 != "None") skillBonuses[race.data.skill_bonuses.skill_3] = race.data.skill_bonuses.bonus_3;
                            if (race.data.skill_bonuses.skill_4 != "None") skillBonuses[race.data.skill_bonuses.skill_4] = race.data.skill_bonuses.bonus_4;
                            if (race.data.skill_bonuses.skill_5 != "None") skillBonuses[race.data.skill_bonuses.skill_5] = race.data.skill_bonuses.bonus_5;
                            if (race.data.skill_bonuses.skill_6 != "None") skillBonuses[race.data.skill_bonuses.skill_6] = race.data.skill_bonuses.bonus_6;
                            outputDict["race"][race.id] = new Dictionary<string, object>
                                {
                                    { "Name", race.name },
                                    { "Description", race.description },
                                    { "Spells", race.spells },
                                    { "Attributes", new Dictionary<string, object>
                                        {
                                            { "Strength", race.data.strength },
                                            { "Intelligence", race.data.intelligence },
                                            { "Willpower", race.data.willpower },
                                            { "Agility", race.data.agility },
                                            { "Speed", race.data.speed },
                                            { "Endurance", race.data.endurance },
                                            { "Personality", race.data.personality },
                                            { "Luck", race.data.luck }
                                        }
                                    },
                                    { "SkillBonuses", skillBonuses },
                                    { "Height", race.data.height },
                                    { "Weight", race.data.weight },
                                    { "Playable", race.data.flags.Contains("PLAYABLE") },
                                    { "Beast", race.data.flags.Contains("BEAST_RACE") }
                                };
                            break;
                        case "Birthsign":
                            if (!outputDict.ContainsKey("birthsign")) outputDict["birthsign"] = [];
                            var birthsign = (converted as TES3_Birthsign);
                            outputDict["birthsign"][birthsign.id] = new Dictionary<string, object>
                                {
                                    { "Name", birthsign.name },
                                    { "Description", birthsign.description },
                                    { "Spells", birthsign.spells },
                                    { "Image", birthsign.texture.Split("\\").Last().Split(".").First() }
                                };
                            break;
                        case "Faction":
                            if (!outputDict.ContainsKey("faction")) outputDict["faction"] = [];
                            var faction = (converted as TES3_Faction);
                            outputDict["faction"][faction.id] = new Dictionary<string, object>
                                {
                                    { "Name", faction.name },
                                    { "Ranks", faction.rank_names },
                                    { "Reactions", faction.reactions.Select(x => x.faction).Distinct().ToDictionary(x => x, x => faction.reactions.Last(f => f.faction == x).reaction) },
                                    { "Attributes", faction.data.favored_attributes },
                                    { "Skills", faction.data.favored_skills },
                                    { "Requirements", faction.data.requirements.Select(x => new Dictionary<string, object>
                                        {
                                            { "Attributes", x.attributes },
                                            { "PrimarySkill", x.primary_skill },
                                            { "FavouredSkill", x.favored_skill },
                                            { "Reputation", x.reputation }
                                        })
                                    }
                                };
                            break;
                        #endregion

                        #region Magic
                        case "MagicEffect":
                            if (!outputDict.ContainsKey("magiceffect")) outputDict["magiceffect"] = [];
                            var magicEffect = (converted as TES3_MagicEffect);
                            outputDict["magiceffect"][magicEffect.effect_id] = new Dictionary<string, object>
                                {
                                    { "Icon", magicEffect.icon.Split("\\").Last().Split(".").First() },
                                    { "Description", magicEffect.description },
                                    { "School", magicEffect.data.school },
                                    { "Cost", magicEffect.data.base_cost },
                                    { "Speed", magicEffect.data.speed },
                                    { "Size", magicEffect.data.size },
                                    { "SizeCap", magicEffect.data.size_cap },
                                    { "Spellmaking", magicEffect.data.flags.Contains("ALLOW_SPELLMAKING") },
                                    { "Enchanting", magicEffect.data.flags.Contains("ALLOW_ENCHANTING") }
                                };
                            break;
                        case "Spell":
                            if (!outputDict.ContainsKey("spell")) outputDict["spell"] = [];
                            var spell = (converted as TES3_Spell);
                            outputDict["spell"][spell.id] = new Dictionary<string, object>
                                {
                                    { "Name", spell.name },
                                    { "Type", spell.data.spell_type },
                                    { "Cost", spell.data.cost },
                                    { "Flags", spell.data.flags.Split("|").Select(x => x.Trim()).ToArray() },
                                    { "Effects", spell.effects.Select(x => new Dictionary<string, object>
                                        {
                                            { "Effect", x.magic_effect },
                                            { "Skill", x.skill },
                                            { "Attribute", x.attribute },
                                            { "Range", x.range },
                                            { "Area", x.area },
                                            { "Duration", x.duration },
                                            { "Magnitude", new [] { x.min_magnitude, x.max_magnitude } }
                                        })
                                    }
                                };
                            break;
                        case "Enchanting":
                            if (!outputDict.ContainsKey("enchanting")) outputDict["enchanting"] = [];
                            var enchanting = (converted as TES3_Enchanting);
                            outputDict["enchanting"][enchanting.id] = new Dictionary<string, object>
                                {
                                    { "Type", enchanting.data.enchant_type },
                                    { "Cost", enchanting.data.cost },
                                    { "Charge", enchanting.data.max_charge },
                                    { "Flags", enchanting.data.flags.Split("|").Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray() },
                                    { "Effects", enchanting.effects.Select(x => new Dictionary<string, object> {
                                        { "Effect", x.magic_effect },
                                        { "Skill", x.skill },
                                        { "Attribute", x.attribute },
                                        { "Range", x.range },
                                        { "Area", x.area },
                                        { "Duration", x.duration },
                                        { "Magnitude", new[] {x.min_magnitude, x.max_magnitude} }
                                    })}
                                };
                            break;
                        #endregion

                        #region Items
                        case "MiscItem":
                            if (!outputDict.ContainsKey("miscitem")) outputDict["miscitem"] = [];
                            var miscItem = (converted as TES3_MiscItem);
                            outputDict["miscitem"][miscItem.id] = new Dictionary<string, object>
                                {
                                    { "Name", miscItem.name },
                                    { "Icon", miscItem.icon.Split("\\").Last().Split(".").First() },
                                    { "Weight", miscItem.data.weight },
                                    { "Value", miscItem.data.value },
                                    { "Flags", miscItem.data.flags }
                                };
                            break;
                        case "Weapon":
                            if (!outputDict.ContainsKey("weapon")) outputDict["weapon"] = [];
                            var weapon = (converted as TES3_Weapon);
                            outputDict["weapon"][weapon.id] = new Dictionary<string, object>
                                {
                                    { "Name", weapon.name },
                                    { "Icon", weapon.icon.Split("\\").Last().Split(".").First() },
                                    { "Enchanting", weapon.enchanting },
                                    { "Weight", weapon.data.weight },
                                    { "Value", weapon.data.value },
                                    { "Type", weapon.data.weapon_type },
                                    { "Health", weapon.data.health },
                                    { "Speed", weapon.data.speed },
                                    { "Reach", weapon.data.reach },
                                    { "Enchantment", weapon.data.enchantment },
                                    { "Chop", new [] { weapon.data.chop_min, weapon.data.chop_max } },
                                    { "Slash", new [] { weapon.data.slash_min, weapon.data.slash_max } },
                                    { "Thrust", new [] { weapon.data.thrust_min, weapon.data.thrust_max } },
                                    { "Flags", weapon.data.flags }
                                };
                            break;
                        case "Book":
                            if (!outputDict.ContainsKey("book")) outputDict["book"] = [];
                            var book = (converted as TES3_Book);
                            outputDict["book"][book.id] = new Dictionary<string, object>
                                {
                                    { "Name", book.name },
                                    { "Icon", book.icon.Split("\\").Last().Split(".").First() },
                                    { "Enchanting", book.enchanting },
                                    { "Text", book.text },
                                    { "Type", book.data.book_type },
                                    { "Skill", book.data.skill },
                                    { "Enchantment", book.data.enchantment },
                                    { "Weight", book.data.weight },
                                    { "Value", book.data.value },
                                };
                            break;
                        case "Alchemy":
                            if (!outputDict.ContainsKey("alchemy")) outputDict["alchemy"] = [];
                            var alchemy = (converted as TES3_Alchemy);
                            outputDict["alchemy"][alchemy.id] = new Dictionary<string, object>
                                {
                                    { "Name", alchemy.name },
                                    { "Icon", alchemy.icon.Split("\\").Last().Split(".").First() },
                                    { "Weight", alchemy.data.weight },
                                    { "Value", alchemy.data.value },
                                    { "Effects", alchemy.effects.Select(x => new Dictionary<string, object>
                                    {
                                        { "Effect", x.magic_effect },
                                        { "Skill", x.skill },
                                        { "Attribute", x.attribute },
                                        { "Range", x.range },
                                        { "Area", x.area },
                                        { "Duration", x.duration },
                                        { "Magnitude", new [] { x.min_magnitude, x.max_magnitude } }
                                    }) }
                                };
                            break;
                        case "Apparatus":
                            if (!outputDict.ContainsKey("apparatus")) outputDict["apparatus"] = [];
                            var apparatus = (converted as TES3_Apparatus);
                            outputDict["apparatus"][apparatus.id] = new Dictionary<string, object>
                                {
                                    { "Name", apparatus.name },
                                    { "Icon", apparatus.icon.Split("\\").Last().Split(".").First() },
                                    { "Weight", apparatus.data.weight },
                                    { "Value", apparatus.data.value },
                                    { "Type", apparatus.data.apparatus_type },
                                    { "Quality", apparatus.data.quality }
                                };
                            break;
                        case "Armor":
                            if (!outputDict.ContainsKey("armour")) outputDict["armour"] = [];
                            var armour = (converted as TES3_Armor);
                            outputDict["armour"][armour.id] = new Dictionary<string, object>
                                {
                                    { "Name", armour.name },
                                    { "Icon", armour.icon.Split("\\").Last().Split(".").First() },
                                    { "Weight", armour.data.weight },
                                    { "Value", armour.data.value },
                                    { "Enchanting", armour.enchanting },
                                    { "Type", armour.data.armor_type },
                                    { "Health", armour.data.health },
                                    { "Enchantment", armour.data.enchantment },
                                    { "Rating", armour.data.armor_rating }
                                };
                            break;
                        case "Clothing":
                            if (!outputDict.ContainsKey("clothing")) outputDict["clothing"] = [];
                            var clothing = (converted as TES3_Clothing);
                            outputDict["clothing"][clothing.id] = new Dictionary<string, object>
                                {
                                    { "Name", clothing.name },
                                    { "Icon", clothing.icon.Split("\\").Last().Split(".").First() },
                                    { "Weight", clothing.data.weight },
                                    { "Value", clothing.data.value },
                                    { "Enchanting", clothing.enchanting },
                                    { "Type", clothing.data.clothing_type },
                                    { "Enchantment", clothing.data.enchantment }
                                };
                            break;
                        case "Ingredient":
                            if (!outputDict.ContainsKey("ingredient")) outputDict["ingredient"] = [];
                            var ingredient = (converted as TES3_Ingredient);
                            outputDict["ingredient"][ingredient.id] = new Dictionary<string, object>
                                {
                                    { "Name", ingredient.name },
                                    { "Icon", ingredient.icon.Split("\\").Last().Split(".").First() },
                                    { "Weight", ingredient.data.weight },
                                    { "Value", ingredient.data.value },
                                    { "Effects", ingredient.data.effects },
                                    { "Skills", ingredient.data.skills },
                                    { "Attributes", ingredient.data.attributes }
                                };
                            break;
                        case "Lockpick":
                            if (!outputDict.ContainsKey("lockpick")) outputDict["lockpick"] = [];
                            var lockpick = (converted as TES3_Lockpick);
                            outputDict["lockpick"][lockpick.id] = new Dictionary<string, object>
                                {
                                    { "Name", lockpick.name },
                                    { "Icon", lockpick.icon.Split("\\").Last().Split(".").First() },
                                    { "Weight", lockpick.data.weight },
                                    { "Value", lockpick.data.value },
                                    { "Quality", lockpick.data.quality },
                                    { "Uses", lockpick.data.uses }
                                };
                            break;
                        case "Probe":
                            if (!outputDict.ContainsKey("probe")) outputDict["probe"] = [];
                            var probe = (converted as TES3_Probe);
                            outputDict["probe"][probe.id] = new Dictionary<string, object>
                                {
                                    { "Name", probe.name },
                                    { "Icon", probe.icon.Split("\\").Last().Split(".").First() },
                                    { "Weight", probe.data.weight },
                                    { "Value", probe.data.value },
                                    { "Quality", probe.data.quality },
                                    { "Uses", probe.data.uses }
                                };
                            break;
                        case "RepairItem":
                            if (!outputDict.ContainsKey("repairitem")) outputDict["repairitem"] = [];
                            var repairItem = (converted as TES3_RepairItem);
                            outputDict["repairitem"][repairItem.id] = new Dictionary<string, object>
                                {
                                    { "Name", repairItem.name },
                                    { "Icon", repairItem.icon.Split("\\").Last().Split(".").First() },
                                    { "Weight", repairItem.data.weight },
                                    { "Value", repairItem.data.value },
                                    { "Quality", repairItem.data.quality },
                                    { "Uses", repairItem.data.uses }
                                };
                            break;
                        #endregion

                        #region Activators
                        case "Container":
                            if (!outputDict.ContainsKey("container")) outputDict["container"] = [];
                            var container = (converted as TES3_Container);
                            var containerFlags = container.flags;
                            if (!string.IsNullOrWhiteSpace(container.container_flags))
                            {
                                if (!string.IsNullOrWhiteSpace(containerFlags)) containerFlags += " | ";
                                containerFlags += container.container_flags;
                            }
                            outputDict["container"][container.id] = new Dictionary<string, object>
                                {
                                    { "Name", container.name },
                                    { "Flags", containerFlags.Split("|").Select(x => x.Trim()).ToArray() },
                                    { "Encumberance", container.encumbrance },
                                    { "Inventory", container.inventory.ToDictionary(x => x[1], x => x[0]) }
                                };
                            break;
                        #endregion

                        #region NPCs
                        case "Creature":
                            if (!outputDict.ContainsKey("creature")) outputDict["creature"] = [];
                            var creature = (converted as TES3_Creature);
                            var creatureFlags = creature.flags;
                            if (!string.IsNullOrWhiteSpace(creature.creature_flags))
                            {
                                if (!string.IsNullOrWhiteSpace(creatureFlags)) creatureFlags += " | ";
                                creatureFlags += creature.creature_flags;
                            }
                            outputDict["creature"][creature.id] = new Dictionary<string, object>
                                {
                                    { "Name", creature.name },
                                    { "Spells", creature.spells },
                                    { "Inventory", creature.inventory.ToDictionary(x => x[1], x => x[0]) },
                                    { "Flags", creatureFlags.Split("|").Select(x => x.Trim()).ToArray() },
                                    { "BloodType", creature.blood_type },
                                    { "AI", new Dictionary<string, object> {
                                        { "Hello", creature.ai_data.hello },
                                        { "Fight", creature.ai_data.fight },
                                        { "Flee", creature.ai_data.flee },
                                        { "Alarm", creature.ai_data.alarm },
                                        { "Services", creature.ai_data.services.Split("|").Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray() }
                                    }},
                                    { "Type", creature.data.creature_type },
                                    { "Level", creature.data.level },
                                    { "Attributes", new Dictionary<string, object> {
                                        { "Strength", creature.data.strength },
                                        { "Intelligence", creature.data.intelligence },
                                        { "Willpower", creature.data.willpower },
                                        { "Agility", creature.data.agility },
                                        { "Speed", creature.data.speed },
                                        { "Endurance", creature.data.endurance },
                                        { "Personality", creature.data.personality },
                                        { "Luck", creature.data.luck }
                                    } },
                                    { "Stats", new Dictionary<string, object> {
                                        { "Health", creature.data.health },
                                        { "Magicka", creature.data.magicka },
                                        { "Fatigue", creature.data.fatigue },
                                    } },
                                    { "Soul", creature.data.soul },
                                    { "Specialisations", new Dictionary<string, object> {
                                        { "Combat", creature.data.combat },
                                        { "Magic", creature.data.magic },
                                        { "Stealth", creature.data.steath }
                                    } },
                                    { "Attacks", new [] { creature.data.attack1, creature.data.attack2, creature.data.attack3 } },
                                    { "Gold", creature.data.gold }
                                };
                            break;
                        case "Npc":
                            if (!outputDict.ContainsKey("npc")) outputDict["npc"] = [];
                            var npc = (converted as TES3_Npc);
                            var npcFlags = npc.flags;
                            if (!string.IsNullOrWhiteSpace(npc.npc_flags))
                            {
                                if (!string.IsNullOrWhiteSpace(npcFlags)) npcFlags += " | ";
                                npcFlags += npc.npc_flags;
                            }
                            outputDict["npc"][npc.id] = new Dictionary<string, object>
                                {
                                    { "Name", npc.name },
                                    { "Race", npc.race },
                                    { "Class", npc._class },
                                    { "Head", npc.head },
                                    { "Hair", npc.hair },
                                    { "Spells", npc.spells },
                                    { "Inventory", npc.inventory.ToDictionary(x => x[1], x => x[0]) },
                                    { "Flags", npcFlags.Split("|").Select(x => x.Trim()).ToArray() },
                                    { "BloodType", npc.blood_type },
                                    { "AI", new Dictionary<string, object> {
                                        { "Hello", npc.ai_data.hello },
                                        { "Fight", npc.ai_data.fight },
                                        { "Flee", npc.ai_data.flee },
                                        { "Alarm", npc.ai_data.alarm },
                                        { "Services", npc.ai_data.services.Split("|").Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray() }
                                    }},
                                    { "Level", npc.data.level },
                                    { "Attributes", npc.data.stats != null ? npc.data.stats.attributes : [] },
                                    { "Skills", npc.data.stats != null ? npc.data.stats.skills :[] },
                                    { "Stats", npc.data.stats != null ? new Dictionary<string, object> {
                                        { "Health", npc.data.stats.health },
                                        { "Magicka", npc.data.stats.magicka },
                                        { "Fatigue", npc.data.stats.fatigue },
                                    } : [] },
                                    { "Gold", npc.data.gold },
                                    { "Disposition", npc.data.disposition },
                                    { "Reputation", npc.data.reputation },
                                    { "Rank", npc.data.rank },
                                    { "Faction", npc.faction },
                                };
                            break;
                        #endregion

                        #region Leveled Lists
                        case "LeveledCreature":
                            if (!outputDict.ContainsKey("leveledcreature")) outputDict["leveledcreature"] = [];
                            var leveledCreature = (converted as TES3_LeveledCreature);
                            var leveledCreatureFlags = leveledCreature.flags;
                            if (!string.IsNullOrWhiteSpace(leveledCreature.leveled_creature_flags))
                            {
                                if (!string.IsNullOrWhiteSpace(leveledCreatureFlags)) leveledCreatureFlags += " | ";
                                leveledCreatureFlags += leveledCreature.leveled_creature_flags;
                            }
                            outputDict["leveledcreature"][leveledCreature.id] = new Dictionary<string, object>
                                {
                                    { "Flags", leveledCreatureFlags.Split("|").Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray() },
                                    { "ChanceNone", leveledCreature.chance_none },
                                    { "Creatures", leveledCreature.creatures.Select(x => x[0]).Distinct().ToDictionary(x => x, x => leveledCreature.creatures.Where(i => i[0] == x).Last()[1]) }
                                };
                            break;
                        case "LeveledItem":
                            if (!outputDict.ContainsKey("leveleditem")) outputDict["leveleditem"] = [];
                            var leveledItem = (converted as TES3_LeveledItem);
                            var leveledItemFlags = leveledItem.flags;
                            if (!string.IsNullOrWhiteSpace(leveledItem.leveled_item_flags))
                            {
                                if (!string.IsNullOrWhiteSpace(leveledItemFlags)) leveledItemFlags += " | ";
                                leveledItemFlags += leveledItem.leveled_item_flags;
                            }
                            outputDict["leveleditem"][leveledItem.id] = new Dictionary<string, object>
                                {
                                    { "Flags", leveledItemFlags.Split("|").Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray() },
                                    { "ChanceNone", leveledItem.chance_none },
                                    { "Items", leveledItem.items.Select(x => x[0]).Distinct().ToDictionary(x => x, x => leveledItem.items.Where(i => i[0] == x).Last()[1]) }
                                };
                            break;
                        #endregion

                        #region Cells
                        case "Region":
                            if (!outputDict.ContainsKey("region")) outputDict["region"] = [];
                            var region = (converted as TES3_Region);
                            outputDict["region"][region.id] = new Dictionary<string, object>
                                {
                                    { "Name", region.name },
                                    { "SleepCreature", region.sleep_creature },
                                    { "Weather", new Dictionary<string, object>
                                        {
                                            { "Clear", region.weather_chances.clear },
                                            { "Cloudy", region.weather_chances.cloudy },
                                            { "Foggy", region.weather_chances.foggy },
                                            { "Overcast", region.weather_chances.overcast },
                                            { "Rain", region.weather_chances.rain },
                                            { "Thunder", region.weather_chances.thunder },
                                            { "Ash", region.weather_chances.ash },
                                            { "Blight", region.weather_chances.blight },
                                            { "Snow", region.weather_chances.snow },
                                            { "Blizzard", region.weather_chances.blizzard }
                                        }
                                    }
                                };
                            break;
                        case "Cell":
                            if (!outputDict.ContainsKey("cell")) outputDict["cell"] = [];
                            var cell = (converted as TES3_Cell);

                            var cellId = "";
                            if (!string.IsNullOrWhiteSpace(cell.region)) cellId += $"{cell.region}_{cell.data.grid[0]}_{cell.data.grid[1]}";
                            else cellId = $"{cell.name}";

                            var cellFlags = cell.flags;
                            if (!string.IsNullOrWhiteSpace(cell.data.flags))
                            {
                                if (!string.IsNullOrWhiteSpace(cellFlags)) cellFlags += " | ";
                                cellFlags += cell.data.flags;
                            }
                            var cellReferenceFile = PurgeString(cellId);
                            outputDict["cell"][cellId] = new Dictionary<string, object>
                                {
                                    { "Name", cell.name },
                                    { "Flags", cellFlags.Split("|").Select(x => x.Trim()).ToArray() },
                                    { "Grid", cell.data.grid },
                                    { "Region", cell.region },
                                    { "References", cellReferenceFile }
                                };
                            var cellReferences = cell.references.Select(x => new Dictionary<string, object> {
                                    { "Id", x.id },
                                    { "Translation", x.translation },
                                    { "Rotation", x.rotation },
                                    { "Owner", x.owner },
                                    { "Faction", x.owner_faction },
                                    { "FactionRank", x.owner_faction_rank },
                                    { "OwnerGlobal", x.owner_global },
                                    { "Health", x.health_left },
                                    { "Count", x.object_count },
                                    { "Lock", x.lock_level },
                                    { "Trap", x.trap },
                                    { "Key", x.key },
                                    { "Soul", x.soul },
                                    { "Charge", x.charge_left },
                                    { "Blocked", x.blocked }
                                });
                            var referencesDir = Path.Combine(outputDir, "morroweb.cell.references");
                            if (!Directory.Exists(referencesDir)) Directory.CreateDirectory(referencesDir);
                            var referencesPath = Path.Combine(referencesDir, $"{HttpUtility.UrlEncode(cellReferenceFile)}.json");
                            File.WriteAllText(referencesPath, JsonConvert.SerializeObject(cellReferences, Formatting.Indented));
                            break;
                        #endregion

                        #region Dialogue
                        case "Dialogue":
                            if (!outputDict.ContainsKey("dialogue")) outputDict["dialogue"] = [];
                            var dialogue = (converted as TES3_Dialogue);
                            outputDict["dialogue"][dialogue.id] = new Dictionary<string, object>
                                {
                                    { "Type", dialogue.dialogue_type },
                                    { "Info", PurgeString(dialogue.id) }
                                };
                            currentDialogueTopic = dialogue.id;
                            currentDialogueType = dialogue.dialogue_type;
                            if (!dialogueDict.ContainsKey(currentDialogueType)) dialogueDict[currentDialogueType] = new Dictionary<string, object>();
                            if (!dialogueDict[currentDialogueType].ContainsKey(currentDialogueTopic)) dialogueDict[currentDialogueType][currentDialogueTopic] = new Dictionary<string, object>();
                            break;
                        case "DialogueInfo":
                            var dialoguInfo = (converted as TES3_DialogueInfo);
                            ((Dictionary<string, object>)dialogueDict[currentDialogueType][currentDialogueTopic])[dialoguInfo.id] = new Dictionary<string, object>
                                {
                                    { "Prev", dialoguInfo.prev_id },
                                    { "Next", dialoguInfo.next_id },
                                    { "Type", dialoguInfo.data.dialogue_type },
                                    { "Disposition", dialoguInfo.data.disposition },
                                    { "SpeakerRank", dialoguInfo.data.speaker_rank },
                                    { "SpeakerSex", dialoguInfo.data.speaker_sex },
                                    { "PlayerRank", dialoguInfo.data.player_rank },
                                    { "SpeakerId", dialoguInfo.speaker_id },
                                    { "SpeakerRace", dialoguInfo.speaker_race },
                                    { "SpeakerClass", dialoguInfo.speaker_class },
                                    { "SpeakerFaction", dialoguInfo.speaker_faction },
                                    { "SpeakerCell", dialoguInfo.speaker_cell },
                                    { "PlayerFaction", dialoguInfo.player_faction },
                                    { "Text", dialoguInfo.text },
                                    { "Filters", dialoguInfo.filters.Select(x => new Dictionary<string, object>
                                    {
                                        { "Type", x.filter_type },
                                        { "Func", x.function },
                                        { "Compare", x.comparison },
                                        { "Id", x.id },
                                        { "Value", x.value.data },
                                    }) },
                                    { "Script", dialoguInfo.script_text.Split("\r\n").Select(x => x.Replace("\t", "")).Where(x => !x.Trim().StartsWith(";") && !string.IsNullOrWhiteSpace(x)).ToArray() }
                                };
                            break;
                        #endregion

                        default: break;
                    }
                }
            }

            var dialogueDir = Path.Combine(outputDir, "morroweb.dialogueinfo");
            foreach (var key in dialogueDict.Keys)
            {
                var typeDir = Path.Combine(dialogueDir, $"{key.ToLower()}");
                if (!Directory.Exists(typeDir)) Directory.CreateDirectory(typeDir);
                foreach (var topic in dialogueDict[key].Keys)
                {
                    var topicFile = Path.Combine(typeDir, $"{PurgeString(topic)}.json");
                    File.WriteAllText(topicFile, JsonConvert.SerializeObject(dialogueDict[key][topic], Formatting.Indented));
                }
            }

            foreach (var key in outputDict.Keys)
            {
                var dataFile = Path.Combine(outputDir, $"morroweb.{key}.json");
                File.WriteAllText(dataFile, JsonConvert.SerializeObject(outputDict[key], Formatting.Indented));
            }

            Console.WriteLine("Json generation complete.");
        }
    }
}