using Sharpener.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharpener
{
    public class SeperatorToken : BaseToken
    {
        public int StartColumn { get; set; }
        public int EndColumn { get; set; }
        public SeperatorToken(string text, int line, int tokenIndex, int startColumn, int endColumn, TokenType tokenType)
            :base(line, tokenIndex)
        {
            StringRepresentation = text;
            TokenType = tokenType;
            StartColumn = startColumn;
            EndColumn = endColumn;
        }

        public string Description
        {
            get => getDescription();
        }

        private string getDescription()
        {
            return String.Format("String: {0} SeperatorTokenType: {1}, Line: {2}, Token Index {3}",
                StringRepresentation, TokenType.ToString(), LineNumber, TokenIndex);
        }
    }
}
