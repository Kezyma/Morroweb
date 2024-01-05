using System;
using System.Net;
using System.IO.Compression;
using System.Diagnostics;
using EspConverter.Models.Tes3;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using EspConverter.Models.Web;
using System.ComponentModel;
using Newtonsoft.Json.Linq;

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

                // Read all the new json data.
                Console.WriteLine("Loading all json data.");
                var loadedData = new Dictionary<string, Tes3Object[]>();
                foreach (var file in fileNames)
                {
                    var jsonPath = Path.Combine(jsonDir, $"{file}.json");
                    var text = File.ReadAllText(jsonPath);
                    var json = JsonConvert.DeserializeObject<Tes3Object[]>(text);
                    loadedData[file] = json;
                    Console.WriteLine($"Loaded {file} json data.");
                }

                // Calculate a load order.
                var forceOrder = new[] { "Morrowind", "Tribunal", "Bloodmoon" };
                var finalOrder = forceOrder.ToList();
                finalOrder.AddRange(fileNames.Where(x => !forceOrder.Contains(x)));
                
                ExportGMSTs(finalOrder, loadedData, outputDir);
                ExportGlobals(finalOrder, loadedData, outputDir);
                ExportClasses(finalOrder, loadedData, outputDir);
                ExportFactions(finalOrder, loadedData, outputDir);
                ExportRaces(finalOrder, loadedData, outputDir);
                ExportSkills(finalOrder, loadedData, outputDir);
                ExportMagicEffects(finalOrder, loadedData, outputDir);
                ExportScripts(finalOrder, loadedData, outputDir);

                Console.WriteLine("Processing complete.");
            }
            Console.ReadLine();
        }

        static void ExportGMSTs(List<string> loadOrder, Dictionary<string, Tes3Object[]> loadedData, string outputDir)
        {
            Console.WriteLine("Exporting GMST records.");
            var gmstDict = new Dictionary<string, object>();
            foreach (var file in loadOrder)
            {
                foreach (var obj in loadedData[file])
                {
                    if (obj.type == "GameSetting")
                    {
                        var prop = obj.value as JObject;
                        gmstDict[obj.id] = prop.GetValue("data");
                    }
                }
            }
            var dataFile = Path.Combine(outputDir, $"morroweb.gmst.json");
            File.WriteAllText(dataFile, JsonConvert.SerializeObject(gmstDict, Formatting.Indented));
            Console.WriteLine("GMST records exported.");
        }
        static void ExportGlobals(List<string> loadOrder, Dictionary<string, Tes3Object[]> loadedData, string outputDir)
        {
            Console.WriteLine("Exporting Global records.");
            var gmstDict = new Dictionary<string, object>();
            foreach (var file in loadOrder)
            {
                foreach (var obj in loadedData[file])
                {
                    if (obj.type == "GlobalVariable")
                    {
                        gmstDict[obj.id] = obj.value;
                    }
                }
            }
            var dataFile = Path.Combine(outputDir, $"morroweb.global.json");
            File.WriteAllText(dataFile, JsonConvert.SerializeObject(gmstDict, Formatting.Indented));
            Console.WriteLine("Global records exported.");
        }
        static void ExportClasses(List<string> loadOrder, Dictionary<string, Tes3Object[]> loadedData, string outputDir)
        {
            Console.WriteLine("Exporting Class records.");
            var gmstDict = new Dictionary<string, WebClass>();
            foreach (var file in loadOrder)
            {
                foreach (var obj in loadedData[file])
                {
                    if (obj.type == "Class")
                    {
                        var classObj = new WebClass
                        {
                            Id = obj.id,
                            Name = obj.name,
                            Description = obj.description,
                            Specialisation = obj.data.specialization.ToString(),
                            Attributes = new[] { obj.data.attribute1, obj.data.attribute2 },
                            Major = new[] { obj.data.major1, obj.data.major2, obj.data.major3, obj.data.major4, obj.data.major5 },
                            Minor = new[] { obj.data.minor1, obj.data.minor2, obj.data.minor3, obj.data.minor4, obj.data.minor5 },
                            Playable = obj.data.flags == "PLAYABLE"
                        };
                        gmstDict[obj.id] = classObj;
                    }
                }
            }
            var dataFile = Path.Combine(outputDir, $"morroweb.class.json");
            File.WriteAllText(dataFile, JsonConvert.SerializeObject(gmstDict, Formatting.Indented));
            Console.WriteLine("Class records exported.");
        }
        static void ExportFactions(List<string> loadOrder, Dictionary<string, Tes3Object[]> loadedData, string outputDir)
        {
            Console.WriteLine("Exporting Faction records.");
            var gmstDict = new Dictionary<string, WebFaction>();
            foreach (var file in loadOrder)
            {
                foreach (var obj in loadedData[file])
                {
                    if (obj.type == "Faction")
                    {
                        var factionObj = new WebFaction
                        {
                            Id = obj.id,
                            Name = obj.name,
                            Ranks = obj.rank_names,
                            Attributes = obj.data.favored_attributes,
                            Skills = obj.data.favored_skills,
                            Reactions = obj.reactions.Select(x => x.faction).Distinct().ToDictionary(x =>  x, x => obj.reactions.LastOrDefault(y => y.faction == x).reaction),
                            Requirements = obj.data.requirements.Select(y => new WebFactionRequirement
                            {
                                Attributes = y.attributes,
                                FavouredSkill = y.favored_skill,
                                PrimarySkill = y.primary_skill,
                                Reputation = y.reputation
                            }).ToArray()
                        };
                        gmstDict[obj.id] = factionObj;
                    }
                }
            }
            var dataFile = Path.Combine(outputDir, $"morroweb.faction.json");
            File.WriteAllText(dataFile, JsonConvert.SerializeObject(gmstDict, Formatting.Indented));
            Console.WriteLine("Faction records exported.");
        }
        static void ExportRaces(List<string> loadOrder, Dictionary<string, Tes3Object[]> loadedData, string outputDir)
        {
            Console.WriteLine("Exporting Race records.");
            var gmstDict = new Dictionary<string, WebRace>();
            foreach (var file in loadOrder)
            {
                foreach (var obj in loadedData[file])
                {
                    if (obj.type == "Race")
                    {
                        var factionObj = new WebRace
                        {
                            Id = obj.id,
                            Name = obj.name,
                            Description = obj.description,
                            Beast = obj.data.flags.Contains("BEAST_RACE"),
                            Playable = obj.data.flags.Contains("PLAYABLE"),
                            Height = obj.data.height,
                            Weight = (obj.data.weight as JArray).Values().Select(x => x.Value<float>()).ToArray(),
                            Spells = obj.spells
                        };
                        factionObj.SkillBonuses = new Dictionary<string, int>();
                        if (obj.data.skill_bonuses.skill_0 != "None") factionObj.SkillBonuses[obj.data.skill_bonuses.skill_0] = obj.data.skill_bonuses.bonus_0;
                        if (obj.data.skill_bonuses.skill_1 != "None") factionObj.SkillBonuses[obj.data.skill_bonuses.skill_1] = obj.data.skill_bonuses.bonus_1;
                        if (obj.data.skill_bonuses.skill_2 != "None") factionObj.SkillBonuses[obj.data.skill_bonuses.skill_2] = obj.data.skill_bonuses.bonus_2;
                        if (obj.data.skill_bonuses.skill_3 != "None") factionObj.SkillBonuses[obj.data.skill_bonuses.skill_3] = obj.data.skill_bonuses.bonus_3;
                        if (obj.data.skill_bonuses.skill_4 != "None") factionObj.SkillBonuses[obj.data.skill_bonuses.skill_4] = obj.data.skill_bonuses.bonus_4;
                        if (obj.data.skill_bonuses.skill_5 != "None") factionObj.SkillBonuses[obj.data.skill_bonuses.skill_5] = obj.data.skill_bonuses.bonus_5;
                        if (obj.data.skill_bonuses.skill_6 != "None") factionObj.SkillBonuses[obj.data.skill_bonuses.skill_6] = obj.data.skill_bonuses.bonus_6;
                        factionObj.Attributes = new Dictionary<string, int[]>
                        {
                            { "strength", (obj.data.strength as JToken).Select(x => x.Value<int>()).ToArray() },
                            { "intelligence", (obj.data.intelligence as JToken).Select(x => x.Value<int>()).ToArray() },
                            { "willpower", (obj.data.willpower as JToken).Select(x => x.Value<int>()).ToArray() },
                            { "agility", (obj.data.agility as JToken).Select(x => x.Value<int>()).ToArray() },
                            { "speed", (obj.data.speed as JToken).Select(x => x.Value<int>()).ToArray() },
                            { "endurance", (obj.data.endurance as JToken).Select(x => x.Value<int>()).ToArray() },
                            { "personality", (obj.data.personality as JToken).Select(x => x.Value<int>()).ToArray() },
                            { "luck", (obj.data.luck as JToken).Select(x => x.Value<int>()).ToArray() }
                        };
                        gmstDict[obj.id] = factionObj;
                    }
                }
            }
            var dataFile = Path.Combine(outputDir, $"morroweb.race.json");
            File.WriteAllText(dataFile, JsonConvert.SerializeObject(gmstDict, Formatting.Indented));
            Console.WriteLine("Race records exported.");
        }
        static void ExportSkills(List<string> loadOrder, Dictionary<string, Tes3Object[]> loadedData, string outputDir)
        {
            Console.WriteLine("Exporting Skill records.");
            var gmstDict = new Dictionary<string, WebSkill>();
            foreach (var file in loadOrder)
            {
                foreach (var obj in loadedData[file])
                {
                    if (obj.type == "Skill")
                    {
                        var skillObj = new WebSkill
                        {
                            Id = obj.skill_id,
                            Description = obj.description,
                            Actions = obj.data.actions,
                            Attribute = obj.data.governing_attribute,
                            Specialisation = (int)(long)obj.data.specialization
                        };
                        gmstDict[obj.skill_id] = skillObj;
                    }
                }
            }
            var dataFile = Path.Combine(outputDir, $"morroweb.skill.json");
            File.WriteAllText(dataFile, JsonConvert.SerializeObject(gmstDict, Formatting.Indented));
            Console.WriteLine("Skill records exported.");
        }
        static void ExportMagicEffects(List<string> loadOrder, Dictionary<string, Tes3Object[]> loadedData, string outputDir)
        {
            Console.WriteLine("Exporting Magic Effect records.");
            var gmstDict = new Dictionary<string, WebMagicEffect>();
            foreach (var file in loadOrder)
            {
                foreach (var obj in loadedData[file])
                {
                    if (obj.type == "MagicEffect")
                    {
                        var magicObj = new WebMagicEffect
                        {
                            Id = obj.effect_id,
                            Description = obj.description,
                            School = obj.data.school,
                            BaseCost = obj.data.base_cost,
                            Speed = (float)(double)obj.data.speed,
                            Enchanting = obj.data.flags.Contains("ALLOW_ENCHANTING"),
                            Spellmaking = obj.data.flags.Contains("ALLOW_SPELLMAKING"),
                            Size = obj.data.size,
                            SizeCap = obj.data.size_cap,
                        };
                        gmstDict[obj.effect_id] = magicObj;
                    }
                }
            }
            var dataFile = Path.Combine(outputDir, $"morroweb.magiceffect.json");
            File.WriteAllText(dataFile, JsonConvert.SerializeObject(gmstDict, Formatting.Indented));
            Console.WriteLine("Skill Magic Effect exported.");
        }
        static void ExportScripts(List<string> loadOrder, Dictionary<string, Tes3Object[]> loadedData, string outputDir)
        {
            Console.WriteLine("Exporting Script records.");
            var gmstDict = new Dictionary<string, string[]>();
            foreach (var file in loadOrder)
            {
                foreach (var obj in loadedData[file])
                {
                    if (obj.type == "Script")
                    {
                        var scriptText = obj.text.Split("\r\n").Where(x => !x.Trim().StartsWith(";") && !string.IsNullOrWhiteSpace(x.Trim())).Select(x => x.Replace("\t", "")).ToArray();
                        gmstDict[obj.id] = scriptText;
                    }
                }
            }
            var dataFile = Path.Combine(outputDir, $"morroweb.script.json");
            File.WriteAllText(dataFile, JsonConvert.SerializeObject(gmstDict, Formatting.Indented));
            Console.WriteLine("Script records exported.");
        }
        static void ExportRegions(List<string> loadOrder, Dictionary<string, Tes3Object[]> loadedData, string outputDir)
        {
            Console.WriteLine("Exporting Region records.");
            var gmstDict = new Dictionary<string, WebRegion>();
            foreach (var file in loadOrder)
            {
                foreach (var obj in loadedData[file])
                {
                    if (obj.type == "Region")
                    {
                        var regionObj = new WebRegion
                        {
                            Id = obj.id,
                            Name = obj.name,
                            SleepCreatures = obj.sleep_creature
                        };
                        gmstDict[obj.id] = regionObj;
                    }
                }
            }
            var dataFile = Path.Combine(outputDir, $"morroweb.region.json");
            File.WriteAllText(dataFile, JsonConvert.SerializeObject(gmstDict, Formatting.Indented));
            Console.WriteLine("Region records exported.");
        }
        static void ExportBirthsigns(List<string> loadOrder, Dictionary<string, Tes3Object[]> loadedData, string outputDir)
        {
            Console.WriteLine("Exporting Birthsign records.");
            var gmstDict = new Dictionary<string, WebBirthsign>();
            foreach (var file in loadOrder)
            {
                foreach (var obj in loadedData[file])
                {
                    if (obj.type == "Birthsign")
                    {
                        var regionObj = new WebBirthsign
                        {
                            Id = obj.id,
                            Name = obj.name,
                            Description = obj.description,
                            Spells = obj.spells,
                        };
                        gmstDict[obj.id] = regionObj;
                    }
                }
            }
            var dataFile = Path.Combine(outputDir, $"morroweb.birthsign.json");
            File.WriteAllText(dataFile, JsonConvert.SerializeObject(gmstDict, Formatting.Indented));
            Console.WriteLine("Birthsign records exported.");
        }

        static void ExportCells(List<string> loadOrder, Dictionary<string, Tes3Object[]> loadedData, string outputDir)
        {
            Console.WriteLine("Exporting cell records.");
            var cellDict = new Dictionary<int, Dictionary<int, Tes3Object>>();
            foreach (var file in loadOrder)
            {
                foreach (var obj in loadedData[file])
                {
                    
                }
            }
            Console.WriteLine("Cell records exported.");
        }

        static void ExportDialogue(List<string> loadOrder, Dictionary<string, Tes3Object[]> loadedData, string outputDir)
        {
            Console.WriteLine("Exporting dialogue records.");
            var dialogueDict = new Dictionary<string, Dictionary<string, Dictionary<string, Tes3Object>>>();
            foreach (var file in loadOrder)
            {
                var jsonData = loadedData[file];
                var types = jsonData.Select(x => x.type).Distinct().ToList();

                var currentTopic = string.Empty;
                foreach (var obj in jsonData)
                {
                    if (obj.type == "Dialogue")
                    {
                        currentTopic = obj.id;
                        if (!dialogueDict.ContainsKey(obj.dialogue_type)) dialogueDict.Add(obj.dialogue_type, []);
                        if (!dialogueDict[obj.dialogue_type].ContainsKey(currentTopic)) dialogueDict[obj.dialogue_type].Add(currentTopic, []);
                    }
                    if (obj.type == "DialogueInfo")
                    {
                        if (!dialogueDict.ContainsKey(obj.data.dialogue_type)) dialogueDict.Add(obj.data.dialogue_type, []);
                        dialogueDict[obj.data.dialogue_type][currentTopic][obj.id] = obj;
                    }
                }
            }

            foreach (var dialogueType in dialogueDict.Keys)
            {
                var dialogueItem = dialogueDict[dialogueType];
                var dialogeData = new WebDialogueType
                {
                    Name = dialogueType,
                    Topics = dialogueItem.Select(y => new WebDialogueTopic
                    {
                        Text = y.Key,
                        Lines = y.Value.Select(z => new WebDialogueLine
                        {
                            Id = z.Value.id,
                            NextId = z.Value.next_id,
                            PrevId = z.Value.prev_id,
                            NPCId = z.Value.speaker_id,
                            NPCSex = z.Value.data.speaker_sex,
                            NPCRace = z.Value.speaker_race,
                            NPCFaction = z.Value.speaker_faction,
                            NPCClass = z.Value.speaker_class,
                            NPCRank = z.Value.data.speaker_rank,
                            NPCCell = z.Value.speaker_cell,
                            PCFaction = z.Value.player_faction,
                            PCRank = z.Value.data.player_rank,
                            Scripts = z.Value.script_text.Split("\r\n").Where(a => !string.IsNullOrWhiteSpace(a) && !a.Trim().StartsWith(";")).ToArray(),
                            Filters = z.Value.filters.Select(a => new WebDialogueFilter
                            {
                                Type = a.comparison,
                                Func = a.function,
                                Id = a.id,
                                Value = a.value.data.ToString() ?? ""
                            }).ToArray(),
                            Disposition = z.Value.data.disposition,
                            Text = z.Value.text
                        }).ToList()
                    }).ToArray()
                };
                var dataFile = Path.Combine(outputDir, $"morroweb.dialogue.{dialogueType.ToLower()}.json");
                File.WriteAllText(dataFile, JsonConvert.SerializeObject(dialogeData, Formatting.Indented));

            }
            Console.WriteLine("Dialogue records exported.");
        }
    }
}
