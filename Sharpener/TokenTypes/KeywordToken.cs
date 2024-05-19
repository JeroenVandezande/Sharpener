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
        public KeywordToken(int lineNumber, int tokenIndex, int startColumn, int endColumn, TokenType tokenType)
            : base(lineNumber, tokenIndex)
        {
            TokenType = tokenType;
            StartColumn = startColumn;
            EndColumn = endColumn;
        }

        public override string ToString()
        {
            return String.Format("KeywordTokenType: {0}, Line: {1}, Token Index {2}",
                    TokenType.ToString(), LineNumber, TokenIndex);
        }
    }
}
