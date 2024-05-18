using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpener.Enums;

namespace Sharpener.SyntaxTree.Scopes;

public class ConstantSyntaxElement: SyntaxElement, ISyntaxElementWithScope
{
    public string ConstantName { get; set; }
    public string ConstantValue { get; set; }
    
    public VisibilityLevel VisibilityLevel { get; set; }

    public ConstantSyntaxElement WithVisibility(VisibilityLevel visibilityLevel)
    {
        VisibilityLevel = visibilityLevel;
        return this;
    }

    public override bool WithToken(Document document, IToken token)
    {
        if (token is ITokenWithText param)
        {
            if (String.IsNullOrEmpty(ConstantName))
            {
                ConstantName = param.TokenText;
                return true;
            }

            if (String.IsNullOrEmpty(ConstantValue))
            {
                ConstantValue = param.TokenText;
                return true;
            }
        }

        return false;
    }
    
}