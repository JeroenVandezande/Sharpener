using Sharpener.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharpener.TokenTypes
{
    public class CommentToken : BaseToken
    {
        public CommentToken(string variableText, int line, int tokenIndex)
            :base(line, tokenIndex)
        {
            TokenText = variableText;
            TokenType = TokenType.Comment;
        }

        /// <summary>
        /// Text of the variable type token
        /// </summary>
        public string TokenText { get; set; }
        
        public CommentType CommentType { get; set; }
        public override string ToString()
        {
            return String.Format("CommentTokenType: {0}, Line: {1}, Token Index {2}",
                    TokenType.ToString(), LineNumber, TokenIndex);
        }
    }
}
