using Sharpener.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharpener.TokenTypes
{
    public class OperatorToken : BaseToken
    {
        public OperatorToken(int lineNumber, int tokenIndex, TokenType tokenType)
            : base(lineNumber, tokenIndex)
        {
            TokenType = tokenType;
        }

        public override string ToString()
        {
            return String.Format("OperatorTokenType: {0}, Line: {1}, Token Index {2}",
                    TokenType.ToString(), LineNumber, TokenIndex);
        }
    }
}
