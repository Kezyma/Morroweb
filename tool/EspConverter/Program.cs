using EspConverter.Models.TES3;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;

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

                // Calculate a load order.
                var forceOrder = new[] { "Morrowind", "Tribunal", "Bloodmoon" };
                var finalOrder = forceOrder.ToList();
                finalOrder.AddRange(fileNames.Where(x => !forceOrder.Contains(x)));

                // Overwrite records and export.
                var outputDict = new Dictionary<string, Dictionary<string, object>>();
                foreach (var key in finalOrder)
                {
                    dynamic jsonData = JsonConvert.DeserializeObject(File.ReadAllText(Path.Combine(jsonDir, $"{key}.json")));
                    foreach (var record in (JArray)jsonData)
                    {
                        var objectType = record.Value<string>("type");
                        var className = $"TES3_{objectType}";
                        var classType = System.Reflection.Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(x => x.Name == className);
                        var converted = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(record), classType);

                        switch (objectType)
                        {
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
                            case "Class":
                                if (!outputDict.ContainsKey("class")) outputDict["class"] = [];
                                var classObj = (converted as TES3_Class);
                                outputDict["class"][classObj.id] = new Dictionary<string, object>
                                {
                                    { "Name", classObj.name },
                                    { "Description", classObj.description },
                                    { "Attributes", new [] { classObj.data.attribute1, classObj.data.attribute2 } },
                                    { "Major", new [] { classObj.data.major1, classObj.data.major2, classObj.data.major3, classObj.data.major4, classObj.data.major5 } },
                                    { "Minor", new [] { classObj.data.minor1, classObj.data.minor2, classObj.data.minor3, classObj.data.minor4, classObj.data.minor5 } },
                                    { "Playable", classObj.data.flags.Contains("PLAYABLE") },
                                    { "Services", classObj.data.services },
                                    { "Flags", classObj.data.flags },
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
                            case "Script":
                                if (!outputDict.ContainsKey("script")) outputDict["script"] = [];
                                var script = (converted as TES3_Script);
                                outputDict["script"][script.id] = script.text.Split("\r\n").Select(x => x.Replace("\t", "")).Where(x => !x.Trim().StartsWith(";") && !string.IsNullOrWhiteSpace(x)).ToArray();
                                break;
                            case "Spell":
                                if (!outputDict.ContainsKey("spell")) outputDict["spell"] = [];
                                var spell = (converted as TES3_Spell);
                                outputDict["spell"][spell.id] = new Dictionary<string, object>
                                {
                                    { "Name", spell.name },
                                    { "Type", spell.data.spell_type },
                                    { "Cost", spell.data.cost },
                                    { "Flags", spell.data.flags },
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
                            default: break;
                        }
                    }
                }

                foreach (var key in outputDict.Keys)
                {
                    var dataFile = Path.Combine(outputDir, $"morroweb.{key}.json");
                    File.WriteAllText(dataFile, JsonConvert.SerializeObject(outputDict[key], Formatting.Indented));
                }

                Console.WriteLine("Processing complete.");
            }
            Console.ReadLine();
        }
    }
}
