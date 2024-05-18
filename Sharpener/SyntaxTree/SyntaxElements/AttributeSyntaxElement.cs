using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpener.Enums;
using System.Reflection;

namespace Sharpener.SyntaxTree.Scopes;

public class AttributeSyntaxElement : SyntaxElement, IAttributeElement, ISyntaxElementWithScope
{
    public String AttributeText { get; set; } = "["; // Default attribute starting character
    public bool AttributeRequiresCodeConversion { get; set; }

    public override void AddParameter(string param, TokenType tokenType)
    {
        if (tokenType == TokenType.Variable)
        {
            AttributeText += param;
            return;
        }
        else
        {
            AttributeRequiresCodeConversion = false;
        }
    }

    public override void FinishSyntaxElement(Document document)
    {
        if (!AttributeRequiresCodeConversion)
        {
            AttributeText += "]";
        }
        base.FinishSyntaxElement(document);
        if (AttributeRequiresCodeConversion)
        {
            AttributeText = this.OriginalSourceCode;
        }
    }
}