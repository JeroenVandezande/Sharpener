using Sharpener.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sharpener.TokenTypes;

namespace Sharpener
{
    public class Tokenizer
    {
        public List<IToken> Tokens = new List<IToken>();

        public String Output
        {
            get
            {
                return GetOutput();
            }
        }


        public string GetOutput()
        {
            var sb = new StringBuilder();
            foreach(var tok in Tokens)
            {
                sb.AppendLine(tok.ToString());
            }

            return sb.ToString();
        }

        public IEnumerable<string> Lines { get; set; }

        public void ParseAndConvert(IEnumerable<string> lines)
        {
            Parse(lines);
        }

        public void Parse(IEnumerable<string> lines)
        {
            Lines = lines;
            var index = 1;

            // Run Tokenizer
            foreach (var line in lines)
            {
                if (line.ToLower().Contains("$region") || line.ToLower().Contains("$endregion")) //TODO fix regions is a proper way
                {
                    index++;
                    continue;
                }
                ParseLine(line, index);
                index++;
            }
        }

        public void ParseLine(string line, int lineIndex)
        {
            //check if line has a line comment on it
            
            foreach (var token in KeywordSearch(line, lineIndex))
            {
                Tokens.Add(token);
            }
        }

        private List<IToken> KeywordSearch(string parsedLine, int lineIndex)
        {

            var buf = new StringBuilder();
            var tokens = new List<IToken>();

            // Find all string literals in the line and use them later
            var stringLiterals = new List<string>();
            var stringLiteralIndex = 0;
            foreach (Match match in Regex.Matches(parsedLine, TokenizerConstants.MatchStringLiteralRegex))
            {
                stringLiterals.Add(match.Value);
            }

            foreach (Match match in Regex.Matches(parsedLine, TokenizerConstants.MatchSingleQuoteStringLiteralRegex))
            {
                stringLiterals.Add(match.Value);
            }

            // Loop through all characters in a line
            for (int i=0; i < parsedLine.Length; i++)
            {
                var character = parsedLine[i];

                // Match string literals
                if ((character.ToString() == "'") || (character.ToString() == "\""))
                {
                    var tokenAmount = tokens.Count();
                    tokens.Add(new IdentifierToken(stringLiterals[stringLiteralIndex], lineIndex, tokenAmount));
                    i += stringLiterals[stringLiteralIndex].Count() - 1;
                    stringLiteralIndex++;
                    continue;
                }

                if (Char.IsWhiteSpace(character))
                {
                    AddMultipleCharacterToken(i, lineIndex, buf, ref tokens);
                }
                else
                {
                    buf.Append(character);
                }

                // Match seperator tokens
                if (TokenizerConstants.SeperatorSyntax.TryGetValue(character.ToString(), out var sepType))
                {
                    // Add the multiple character token of the buffer seen before this seperator
                    AddMultipleCharacterToken(i, lineIndex, buf.Remove(buf.Length - 1, 1), ref tokens);
                    var tokenAmount = tokens.Count();

                    // Forward Lookahead one character. Matches edge cases like ":" and ":="
                    if (SingleForwardLookAhead(TokenizerConstants.OperatorSyntax, parsedLine, character, i, out var opType))
                    {
                        tokens.Add(new OperatorToken(lineIndex, tokenAmount, opType));
                        i++; // Add to i since we matched a forward character
                        continue;
                    }
                    
                    tokens.Add(new SeperatorToken(lineIndex, tokenAmount, i, i - 1, sepType));
                    continue;
                }

                // Match single character Operators "+, -, /, *, etc"
                if (TokenizerConstants.OperatorSyntax.TryGetValue(character.ToString(), out var op2Type))
                {
                    // Add the multiple character token of the buffer seen before this seperator
                    AddMultipleCharacterToken(i, lineIndex, buf.Remove(buf.Length - 1, 1), ref tokens);
                    var tokenAmount = tokens.Count();

                    // Forward Lookahead for a comment character
                    // Match Comments and Doc Comments (They start as divide operators)
                    if (i + 1 < parsedLine.Length)
                    {
                        var restOfString = character + parsedLine.Substring(i + 1);
                        
                        if (CheckCharactersForLineComment(restOfString))
                        {
                            if (CheckCharactersForDocComment(restOfString))
                            {
                                tokens.Add(new CommentToken(restOfString, lineIndex, tokens.Count) { CommentType = CommentType.SingleLineComment });
                                return tokens;
                            }

                            tokens.Add(new CommentToken(restOfString, lineIndex, tokens.Count) { CommentType = CommentType.SingleLineDocComment });
                            return tokens;
                        }
                    }

                    tokens.Add(new OperatorToken(lineIndex, tokenAmount, op2Type));
                    continue;
                }
            }

            if (buf.Length > 0)
            {
                AddMultipleCharacterToken(parsedLine.Length, lineIndex, buf, ref tokens);
            }

            return tokens;
        }

        /// <summary>
        /// Adds the applicable whitespace seperated token to the token list. Usually these are tokens with multiple characters
        /// </summary>
        /// <param name="index">Character Index where the token was parsed at. End of the token string</param>
        /// <param name="lineIndex">Index of the line in the document</param>
        /// <param name="buffer">String Buffer to hold characters</param>
        /// <param name="tokens">list of tokens to add to</param>
        private void AddMultipleCharacterToken(int index, int lineIndex, StringBuilder buffer, ref List<IToken> tokens)
        {
            if (buffer.Length == 0) return;
            var tokenAmount = tokens.Count();

            TokenType tokenType;
            if (TokenizerConstants.KeywordSyntax.TryGetValue(buffer.ToString().ToLower(), out tokenType))
            {
                tokens.Add(new KeywordToken(lineIndex, tokenAmount, index - buffer.Length, index, tokenType));
                buffer.Clear();
                return;
            }
            
            if (TokenizerConstants.OperatorSyntax.TryGetValue(buffer.ToString().ToLower(), out tokenType))
            {
                tokens.Add(new OperatorToken(lineIndex, tokenAmount, tokenType));
                buffer.Clear();
                return;
            }
            
            //else add as Identifier Token
            {
                tokens.Add(new IdentifierToken(buffer.ToString(), lineIndex, tokenAmount));
                buffer.Clear();
            }
        }

        private bool SingleForwardLookAhead(Dictionary<string, TokenType> searchDictionary, string parsedLine, char character, int index, out TokenType opType)
        {
            if (index + 1 < parsedLine.Length) // Make sure we aren't at the end of the line
            {
                var op = character.ToString() + parsedLine[index + 1].ToString();
                if (searchDictionary.TryGetValue(op, out opType))
                {
                    return true;
                }
            }
            opType = TokenType.UNKNOWN;

            return false;
        }

        private bool CheckCharactersForDocComment(string input)
        {
            var m2 = Regex.Match(input, TokenizerConstants.MatchSingleLineDocCommentRegex);
            if (m2.Success)
            {
                return true;
            }

            return false;
        }

        private bool CheckCharactersForLineComment(string input)
        {
            var m = Regex.Match(input, TokenizerConstants.MatchSingleLineCommentRegex);
            if (m.Success)
            {
                return true;
            }

            return false;
        }
    }
}
