using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpener.Enums;

namespace Sharpener.SyntaxTree.Scopes;

public class WriteLnSyntaxElement: SyntaxElement, ISyntaxElementWithScope, IGenerateStatementSyntax
{
    private String _Text;
    
    public override void AddParameter(string param, TokenType tokenType)
    {
        if (String.IsNullOrEmpty(_Text))
        {
            _Text = param;
        }
        else
        {
            _Text += param;
        }
    }

    public StatementSyntax GenerateCodeNode()
    {
        return SyntaxFactory.ExpressionStatement(
            SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName("Console"),
                        SyntaxFactory.IdentifierName("WriteLine")))
                .WithArgumentList(
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                            SyntaxFactory.Argument(
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    SyntaxFactory.Literal(_Text)))))));
    }
}