using Sharpener.Enums;

namespace Sharpener.SyntaxTree.Scopes;

public class TypeInferanceDeclarationSyntaxElement: SyntaxElement, ISyntaxElementWithScope
{
    public string VariableName { get; set; }

    public override void AddParameter(string param, TokenType tokenType)
    {
        if (String.IsNullOrEmpty(VariableName))
        {
            VariableName = param;
        }
    }
}