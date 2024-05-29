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

        private string getDescription()
        {
            return String.Format("String: {0} \t TokenType: {1} \t Line: {2} \t Token Index {3}",
                StringRepresentation, TokenType.ToString(), LineNumber, TokenIndex.ToString());
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

        /// <summary>
        /// Description of the token and where it is
        /// </summary>
        public string Description
        {
            get => getDescription();
        }
        
        public string StringRepresentation { get; set; }
    }
}
