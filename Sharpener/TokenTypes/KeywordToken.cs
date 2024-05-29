using Sharpener.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharpener
{
    public class KeywordToken: BaseToken
    {
        public int StartColumn { get; set; }
        public int EndColumn { get; set; }
        public KeywordToken(string text, int lineNumber, int tokenIndex, int startColumn, int endColumn, TokenType tokenType)
            : base(lineNumber, tokenIndex)
        {
            TokenType = tokenType;
            StartColumn = startColumn;
            EndColumn = endColumn;
            StringRepresentation = text;
        }

        public string Description
        {
            get => getDescription();
        }

        private string getDescription()
        {
            return String.Format("String: {0} KeywordTokenType: {1}, Line: {2}, Token Index {3}",
                StringRepresentation, TokenType.ToString(), LineNumber, TokenIndex);
        }
    }
}
