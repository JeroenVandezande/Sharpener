using System;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using Sharpener.OpenAI;
using Sharpener.SyntaxTree;

namespace Sharpener
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Not enough arguments provided to run!");
                Debugger.Break();
                return; // return if not enough arguments to run program
            }
            string filename = args[0];
            KeyContainer.APIKey = args[1];
            if (args.Length > 2)
            {
                KeyContainer.SkipOpenAICalls = args[2].ToLower() == "skipapi" ? true : false;
            }

            var lines = File.ReadLines(filename).ToArray();
            var tokenizer = new Tokenizer();
            
            // Parser lines into tokens
            tokenizer.Parse(lines);
            File.WriteAllText(filename + ".tokens", tokenizer.Output);

            //Token Parser
            var tp = new TokenParser();
            var doc = new Document();
            doc.OriginalOxygeneCode = lines;
            if (lines.Any(l => l.ToLower().Contains("notify;")))
            {
                doc.IsNotifyKeywordUsedInFile = true;
            }
            tp.ParseTokens(tokenizer, ref doc);
            string json = JsonConvert.SerializeObject(doc, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
            File.WriteAllText(filename + ".json", json);
            
            File.WriteAllText(filename + ".cs", doc.CreateCSharpCode());
            
            
            // Parse token output
            var stringBuilder = new StringBuilder();
            var prevLineIndex = 1;
            foreach (var token in tokenizer.Tokens)
            {
                // Insert Newline for the next line of code to parse
                if (token.LineNumber > prevLineIndex)
                {
                    stringBuilder.AppendLine("");
                }
                stringBuilder.AppendLine(token.ToString());

                prevLineIndex = token.LineNumber;
            }

            File.WriteAllText(filename + ".txt", stringBuilder.ToString());
        }
    }
}