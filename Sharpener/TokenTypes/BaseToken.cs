using Sharpener.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharpener
{
    public abstract class BaseToken : IToken
    {
        public BaseToken(int lineNumber, int tokenIndex)
        {
            LineNumber = lineNumber;
            TokenIndex = tokenIndex;
        }

        /// <summary>
        /// Line number the token resides on
        /// </summary>
        public int LineNumber { get; private set; }

        /// <summary>
        /// Index amoung the other Tokens
        /// </summary>
        public int TokenIndex { get; private set; }

        /// <summary>
        /// Type of the token
        /// </summary>
        public TokenType TokenType { get; set; }

        public override string ToString()
        {
            return String.Format("TokenType: {0}, Line: {1}, Token Index {2}",
                    TokenType.ToString(), LineNumber, TokenIndex.ToString());
        }

    }
}
