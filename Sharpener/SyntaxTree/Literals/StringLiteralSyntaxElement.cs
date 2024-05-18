using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpener.Enums;
using Sharpener.SyntaxTree.SyntaxElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharpener.SyntaxTree.Literals
{
    /*public class LiteralSyntaxElement : SyntaxElement, IGenerateExpressionSyntax
    {
        public string Literal { get; set; }
        public override void AddParameter(string param, TokenType tokenType)
        {
            if (tokenType == TokenType.Variable)
            {
                Literal = param;
            }
        }

        public ExpressionSyntax GenerateCode()
        {
            return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(Literal));
        }
    }*/

}
