using Sharpener.Enums;

namespace Sharpener.SyntaxTree.Scopes;

public class TypeInferanceDeclarationSyntaxElement: SyntaxElement, ISyntaxElementWithScope
{
    public string VariableName { get; set; }

    public override bool WithToken(Document document, IToken token)
    {
        if (token is ITokenWithText param)
        {
            if (String.IsNullOrEmpty(VariableName))
            {
                VariableName = param.TokenText;
                return true;
            }
        }

        return false;
    }
}