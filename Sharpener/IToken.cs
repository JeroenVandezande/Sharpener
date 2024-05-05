using Sharpener.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharpener
{
    public interface IToken
    {
        /// <summary>
        /// Line number the token resides on
        /// </summary>
        public int LineNumber { get; }

        /// <summary>
        /// Index amoung the other Tokens
        /// </summary>
        public int TokenIndex { get; }

        /// <summary>
        /// Type of the token
        /// </summary>
        public TokenType TokenType { get; }
    }
    
    public interface ITokenWithText
    {
        /// <summary>
        /// Text of the token
        /// </summary>
        public string TokenText { get; }
    }
}
