using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpener.Enums;
using System.Reflection;

namespace Sharpener.SyntaxTree.Scopes;

public class AttributeSyntaxElement : SyntaxElement, IAttributeElement
{
    public String AttributeText { get; set; }
    public bool AttributeRequiresCodeConversion { get; set; }

    public override void AddParameter(string param, TokenType tokenType)
    {
        if (tokenType == TokenType.Variable)
        {
            if (String.IsNullOrEmpty(AttributeText))
            {
                AttributeText = param;
            }

            return;
        }
        else
        {
            AttributeRequiresCodeConversion = false;
        }
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