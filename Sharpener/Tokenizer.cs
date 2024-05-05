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
        private bool _InCommentBlock = false; 
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

        public void ParseAndConvert(IEnumerable<string> lines)
        {
            Parse(lines);
        }

        public void Parse(IEnumerable<string> lines)
        {
            var index = 1;

            // Run Tokenizer
            foreach (var line in lines)
            {
                ParseLine(line, index);
                index++;
            }
        }

        public void ParseLine(string line, int lineIndex)
        {
            //check if line has a line comment on it
            
            foreach (var token in KeywordSearch(line.Trim(), lineIndex))
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
                    
                    tokens.Add(new SeperatorToken(lineIndex, tokenAmount, sepType));
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

                // TODO: Match string literals ("string here!")
                // TODO: Match numberical literals ( 1234 )
                // TODO: Match boolean literals ( true, false )
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
                tokens.Add(new KeywordToken(lineIndex, tokenAmount, tokenType));
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

    public class TokenizerConstants
    {
        public static readonly Dictionary<string, TokenType> KeywordSyntax = new Dictionary<string, TokenType>
        {
            { "abstract", TokenType.AbstractKeyword },
            { "add", TokenType.AddEventSubscriptionHandler },
            { "array", TokenType.ArrayDeclaration },
            { "as", TokenType.AsCast },
            { "asc", TokenType.UNKNOWN },
            { "aspect", TokenType.AspectPrefix },
            { "assembly", TokenType.AssemblyKeyword },
            { "async", TokenType.AsyncDelclaration },
            { "autoreleasepool", TokenType.UNKNOWN },
            { "await", TokenType.AwaitKeyword },
            { "begin", TokenType.CodeBlockBegin },
            { "block", TokenType.BlockDelegateType },
            { "break", TokenType.BreakLoopKeyword },
            { "by", TokenType.ByLinq },
            { "case", TokenType.CaseKeyword },
            { "class", TokenType.ClassKeyword },
            { "concat", TokenType.ConcatLinq },
            { "const", TokenType.ConstKeyword },
            { "constructor", TokenType.ConstructorKeyword },
            { "continue", TokenType.ContinueKeyword },
            { "copy", TokenType.UNKNOWN },
            { "default", TokenType.UNKNOWN },
            { "delegate", TokenType.DelegateType },
            { "deprecated", TokenType.DeprecatedKeyword },
            { "desc", TokenType.DescLinq },
            { "distint", TokenType.DistinctLinq },
            { "div", TokenType.DivideOperator },
            { "do", TokenType.DoKeyword },
            { "downto", TokenType.DownToForLoopKeyword },
            { "dynamic", TokenType.DynamicType },
            { "each", TokenType.EachKeyword },
            { "else", TokenType.ElseKeyword },
            { "empty", TokenType.EmptyKeyword },
            { "end", TokenType.CodeBlockEnd },
            { "end.", TokenType.EndOfFile },
            { "ensure", TokenType.EnsureMethodCondition },
            { "enum", TokenType.EnumKeyword },
            { "equals", TokenType.EqualsLinq },
            { "event", TokenType.EventKeyword },
            { "except", TokenType.ExceptCatchKeyword },
            { "exit", TokenType.ExitReturnKeyword },
            { "extension", TokenType.ExtensionKeyword },
            { "external", TokenType.ExternalKeyword },
            { "final", TokenType.FinalOverrideKeyword },
            { "finalizer", TokenType.FinalizerDeconstructorKeyword },
            { "finally", TokenType.FinallyTryKeyword },
            { "flags", TokenType.FlagsKeyword },
            { "for", TokenType.ForKeyword },
            { "from", TokenType.FromLinq },
            { "future", TokenType.FutureType },
            { "global", TokenType.GlobalAspect },
            { "group", TokenType.GroupLinq },
            { "has", TokenType.HasRequirementClassConstraint },
            { "if", TokenType.IfKeyword },
            { "implementation", TokenType.ImplementationSection },
            { "implements", TokenType.ImplementsInheritanceKeyword },
            { "implies", TokenType.UNKNOWN },
            { "in", TokenType.InForLoop },
            { "index", TokenType.IndexForLoop },
            { "inherited", TokenType.InheritedCall },
            { "inline", TokenType.InlineCompilationCall },
            { "interface", TokenType.InterfaceKeyword },
            { "into", TokenType.IntoLinq },
            { "invariants", TokenType.UNKNOWN },
            { "is", TokenType.IsTestCast },
            { "iterator", TokenType.IteratorMethod },
            { "join", TokenType.JoinLinq },
            { "lazy", TokenType.LazyKeyword },
            { "lifetimestrategy", TokenType.UNKNOWN },
            { "locked", TokenType.LockedMethodKeyword },
            { "locking", TokenType.LockingOnTypeKeyword },
            { "loop", TokenType.LoopInfiniteKeyword },
            { "mapped", TokenType.UNKNOWN },
            { "matching", TokenType.LoopMatchingTypeConstaint },
            { "method", TokenType.MethodKeyword },
            { "module", TokenType.UNKNOWN },
            { "namespace", TokenType.NamespaceKeyword },
            { "nested", TokenType.NestedTypeKeyword },
            { "new", TokenType.NewKeyword },
            { "notify", TokenType.NotifyPropertyChangedImplementation },
            { "nullable", TokenType.NullableTypeDefinition },
            { "of", TokenType.OfTypeKeyword },
            { "old", TokenType.UNKNOWN },
            { "on", TokenType.OnCatchExceptionType },
            { "operator", TokenType.OperatorCustom },
            { "optional", TokenType.UNKNOWN },
            { "or", TokenType.OrBoolean },
            { "order", TokenType.OrderLinq },
            { "out", TokenType.OutKeyword },
            { "override", TokenType.OverrideKeyword },
            { "parallel", TokenType.ParallelForLoop },
            { "param", TokenType.ParamAspectKeyword },
            { "params", TokenType.ParamsCaptureArgsMethod },
            { "partial", TokenType.PartialClassKeyword },
            { "pinned", TokenType.PinnedTypeGCKeyword },
            { "private", TokenType.PrivateKeyword },
            { "property", TokenType.PropertyKeyword },
            { "protected", TokenType.ProtectedKeyword },
            { "public", TokenType.PublicKeyword },
            { "published", TokenType.UNKNOWN },
            { "queryable", TokenType.UNKNOWN },
            { "raise", TokenType.RaiseThrowKeyword },
            { "read", TokenType.ReadGetterKeyword },
            { "readonly", TokenType.ReadonlyKeyword },
            { "record", TokenType.RecordKeyword },
            { "reintroduce", TokenType.UNKNOWN },
            { "remove", TokenType.RemoveEventUnsubscribe },
            { "repeat", TokenType.RepeatDoUntilKeyword },
            { "require", TokenType.RequireMethodPrecondition },
            { "result", TokenType.ResultVariableKeyword },
            { "reverse", TokenType.ReverseLinq },
            { "sealed", TokenType.SealedKeyword },
            { "select", TokenType.SelectLinq },
            { "self", TokenType.SelfThisKeyword },
            { "sequence", TokenType.SequenceCollectionKeyword },
            { "set", TokenType.SetOrdinalValuesType },
            { "skip", TokenType.SkipLinq },
            { "soft", TokenType.UNKNOWN },
            { "static", TokenType.StaticKeyword },
            { "step", TokenType.StepForLoopIncrement },
            { "strong", TokenType.UNKNOWN },
            { "take", TokenType.TakeLinq },
            { "then", TokenType.ThenKeyword },
            { "to", TokenType.ForLoopIterationConstraint },
            { "try", TokenType.TryCatchKeyword },
            { "tuple", TokenType.TupleKeyword },
            { "type", TokenType.TypeDeclarationKeyword },
            { "unit", TokenType.UNKNOWN },
            { "unretained", TokenType.UNKNOWN },
            { "unsafe", TokenType.UnsafeKeyword },
            { "until", TokenType.UntilDoUntilKeyword },
            { "uses", TokenType.UsesKeyword },
            { "using", TokenType.UsingStatementKeyword },
            { "var", TokenType.VarKeyword },
            { "virtual", TokenType.VirtualKeyword },
            { "volatile", TokenType.VolatileKeyword },
            { "weak", TokenType.UNKNOWN },
            { "where", TokenType.WhereConstaintAndLinq },
            { "while", TokenType.WhileKeyword },
            { "with", TokenType.WithLocalVariableScopeKeyword },
            { "write", TokenType.WriteSetterKeyword },
            { "writeln", TokenType.WriteLnMagicFunction },
            { "yield", TokenType.YieldIteratorReturnCollectionKeyword },
        };

        public static readonly Dictionary<string, TokenType> OperatorSyntax = new Dictionary<string, TokenType>
        {
            { ":=",  TokenType.ClassDefinitionAndPropertyNullAccessor },
            { "=",  TokenType.BooleanEquals },
            { "<>",  TokenType.BooleanNotEquals },
            { "+",  TokenType.AddOperator },
            { "-",  TokenType.SubtractOperator },
            { "/",  TokenType.DivideOperator },
            { "*",  TokenType.MultiplyOperator },
            { "xor", TokenType.XorBoolean },
            { "not", TokenType.NotBoolean },
            { "and", TokenType.AndBoolean },
            { "mod", TokenType.ModuloOperator },
            { "shr", TokenType.ShiftBitwiseRight },
            { "shl", TokenType.ShiftBitwiseLeft },
        };

        public static readonly Dictionary<string, TokenType> LiteralSyntax = new Dictionary<string, TokenType>
        {
            { "true", TokenType.TrueBoolean },
            { "false", TokenType.FalseBoolean },
            { "nil", TokenType.NullKeyword },
        };

        public static readonly Dictionary<string, TokenType> SeperatorSyntax = new Dictionary<string, TokenType>
        {
            { ",",  TokenType.Comma },
            { ";",  TokenType.SemiColon },
            { ":",  TokenType.Colon },
            { "(",  TokenType.OpenParathesis },
            { ")",  TokenType.ClosedParathesis },
            { "[",  TokenType.OpenBracket },
            { "]",  TokenType.ClosedBracket },
        };

        public static readonly string MatchStringLiteralRegex = "\"[^\\\"]*\"";
        public static readonly string MatchSingleQuoteStringLiteralRegex = @"'[^\']*'";
        public static readonly string MatchSingleLineCommentRegex = "(\\/{2})([^\n\r]+)";
        public static readonly string MatchSingleLineDocCommentRegex = "(\\/{3})([^\n\r]+)";
    }
}
