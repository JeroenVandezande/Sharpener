using Sharpener.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharpener
{
    public class IdentifierToken : BaseToken, ITokenWithText
    {
        public IdentifierToken(string variableText, int line, int tokenIndex)
            :base(line, tokenIndex)
        {
            TokenText = variableText;
            TokenType = TokenType.Variable;
        }

        /// <summary>
        /// Text of the variable type token
        /// </summary>
        public string TokenText { get; set; }

        public override string ToString()
        {
            return String.Format("IdentifierTokenText: {0}, Line: {1}, Token Index {2}",
                    TokenText, LineNumber, TokenIndex);
        }
    }
}
