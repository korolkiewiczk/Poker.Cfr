using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using CommandLine;
using Newtonsoft.Json;
using Poker.Datalayer;
using Poker.Graphgen.Cfr;
using Poker.Graphgen.Utils;

namespace Poker.Graphgen.App
{
    internal class Program
    {
        private const int MaxHandResolution = 16;

        static void Main(string[] args)
        {
            NodeGen nodeGen = null;
            bool onlyXml = false;
            string xmlFileName = "";

            Options options = null;

            NodeGenConfig defaultConfig = DefaultConfig;

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o =>
                {
                    if (o.ConfigFileName != null)
                    {
                        var nodeGenConfig = JsonConvert.DeserializeObject<NodeGenConfig>(File.ReadAllText(o.ConfigFileName));
                        nodeGen = new NodeGen(nodeGenConfig);
                    }
                    else
                    {
                        nodeGen = new NodeGen(defaultConfig);
                    }

                    if (o.XmlOnlyFile != null)
                    {
                        onlyXml = true;
                        xmlFileName = o.XmlOnlyFile;
                    }

                    options = o;
                }).WithNotParsed(err => Environment.Exit(-1));

            if (options.GenConfig)
            {
                File.WriteAllText("default.json", JsonConvert.SerializeObject(defaultConfig, Formatting.Indented));
                return;
            }
            if (onlyXml)
            {
                GenerateXml(nodeGen, xmlFileName, options);
                return;
            }

            TrainAndWriteToDb(nodeGen, options);
        }

        private static void TrainAndWriteToDb(NodeGen nodeGen, Options options)
        {
            Stopwatch sw = new Stopwatch();

            sw.Start();
            var trainer = new Trainer(nodeGen, options.Iterations, new HandGenerator(MaxHandResolution), new CfrPlusFactory());
            float eq;
            HashSet<int> possibleHands;

            if (!options.Silent)
            {
                Console.WriteLine("Generating game tree...");
            }

            var rootNode = trainer.Train(out eq, out possibleHands, options.Silent ? (Action<int>)null : x =>
             {
                 if (x * 100 / options.Iterations != (x - 1) * 100 / options.Iterations)
                 {
                     Console.Write($"Training progress {x * 100 / options.Iterations}%\r");
                 }
             });
            sw.Stop();

            if (!options.Silent)
            {
                Console.Write($"Training progress {100}%\r");
            }

            if (!options.Silent)
            {
                Console.WriteLine($"\nElapsed seconds on training: {(double)sw.ElapsedMilliseconds / 1000:0.##}");
                Console.WriteLine($"Equity: {eq}");
            }

            var dbWriter = new DbWriter(options.TableName, PromptForDbDrop);
            if (!options.Silent)
            {
                Console.WriteLine("Writing to " + dbWriter.DbName);
            }

            foreach (var hand in possibleHands)
            {
                Console.WriteLine($"Hand {hand:X4}");
                dbWriter.WriteToDb(hand, rootNode, options.Silent ? (Action<int>)null : x => Console.Write($"Written {x} entires\r"));
                Console.WriteLine();
            }
        }

        private static bool PromptForDbDrop()
        {
            Console.WriteLine("Remove existing DB? (Y/N). If No, new table with random name will be generated.");
            if (Console.ReadLine()?.ToLower() == "y")
            {
                return true;
            }

            return false;
        }

        private static void GenerateXml(NodeGen nodeGen, string xmlFileName, Options options)
        {
            if (!options.Silent)
            {
                Console.WriteLine("Generating game tree...");
            }

            var rootNode0 = nodeGen.Generate();
            XElement xElement = new XElement("Node");

            NodeTraverser.TraverseToXml(xElement, rootNode0);

            XDocument doc = new XDocument(xElement);
            doc.Save(xmlFileName);
        }

        private static NodeGenConfig DefaultConfig => new NodeGenConfig
        {
            SbValue = 1,
            BbValue = 2,
            PossibleRaises = new[]
            {
                new[] {4, 6},
                new[] {6, 12},
                new[] {10, 20},
                new[] {15, 25}
            },
            ReraiseAmount = 1,
            NumPlayers = 2,
            Bankroll = 100
        };
    }
}
