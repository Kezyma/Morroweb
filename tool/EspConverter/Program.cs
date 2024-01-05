using System;
using System.Net;
using System.IO.Compression;
using System.Diagnostics;
using EspConverter.Models.Tes3;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using EspConverter.Models.Web;

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
                
                ExportCells(finalOrder, loadedData, outputDir);
                ExportDialogue(finalOrder, loadedData, outputDir);

                Console.WriteLine("Processing complete.");
            }
            Console.ReadLine();
        }

        static void ExportCells(List<string> loadOrder, Dictionary<string, Tes3Object[]> loadedData, string outputDir)
        {

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
