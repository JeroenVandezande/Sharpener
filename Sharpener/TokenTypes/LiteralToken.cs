using Sharpener.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharpener.TokenTypes
{
    public class LiteralToken : BaseToken
    {
        public LiteralToken(int line, int tokenIndex, TokenType tokenType)
            : base(line, tokenIndex)
        {
            TokenType = tokenType;
        }

        public override string ToString()
        {
            return String.Format("LiteralTokenType: {0}, Line: {1}, Token Index {2}",
                    TokenType.ToString(), LineNumber, TokenIndex);
        }
    }
}
