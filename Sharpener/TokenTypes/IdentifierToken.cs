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
            StringRepresentation = TokenText;
            TokenType = TokenType.Variable;
        }

        /// <summary>
        /// Text of the variable type token
        /// </summary>
        public string TokenText { get; set; }

        public string Description
        {
            get => getDescription();
        }

        private string getDescription()
        {
            return String.Format("String: {0} IdentifierTokenType: {1}, Line: {2}, Token Index {3}",
                TokenText, TokenType.ToString(), LineNumber, TokenIndex);
        }
    }
}
