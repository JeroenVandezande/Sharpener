using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpener.Enums;
using System.Reflection;

namespace Sharpener.SyntaxTree.Scopes;

public class AttributeSyntaxElement : SyntaxElement, IAttributeElement
{
    public String AttributeText { get; set; }
    public bool AttributeRequiresCodeConversion { get; set; }

    public override bool WithToken(Document document, IToken token)
    {
        if (token is ITokenWithText param)
        {
            if (token.TokenType == TokenType.Variable)
            {
                if (String.IsNullOrEmpty(AttributeText))
                {
                    AttributeText = param.TokenText;
                    return true;
                }
            }
            else
            {
                AttributeRequiresCodeConversion = false;
            }
        }

        return false;
    }

    public override void FinishSyntaxElement(Document document)
    {
        base.FinishSyntaxElement(document);
        if (AttributeRequiresCodeConversion)
        {
            AttributeText = this.OriginalSourceCode;
        }
    }

    public AttributeSyntax GenerateCodeNode()
    {
        AttributeSyntax result;
        result = SyntaxFactory.Attribute(SyntaxFactory.ParseName(AttributeText));
        return result;
    }
}