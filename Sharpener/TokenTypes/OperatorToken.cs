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
        public OperatorToken(string text, int lineNumber, int tokenIndex, TokenType tokenType)
            : base(lineNumber, tokenIndex)
        {
            StringRepresentation = text;
            TokenType = tokenType;
        }
        
        public string Description
        {
            get => getDescription();
        }

        private string getDescription()
        {
            return String.Format("String: {0} OperatorTokenType: {1}, Line: {2}, Token Index {3}",
                StringRepresentation, TokenType.ToString(), LineNumber, TokenIndex);
        }
    }
}
