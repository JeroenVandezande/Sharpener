using System;
using System.Text;
using Newtonsoft.Json;
using Sharpener.SyntaxTree;

namespace Sharpener
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
                return; // return if no file was dragged onto exe
            string filename = args[0];
            var lines = File.ReadLines(filename).ToArray();
            var tokenizer = new Tokenizer();
            
            // Parser lines into tokens
            tokenizer.Parse(lines);
            File.WriteAllText(filename + ".tokens", tokenizer.Output);

            //Token Parser
            var tp = new TokenParser();
            var doc = new Document();
            doc.OriginalOxygeneCode = lines;
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