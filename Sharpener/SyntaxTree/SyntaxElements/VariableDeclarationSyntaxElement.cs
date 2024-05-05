namespace Sharpener.SyntaxTree.Scopes;

public class VariableDeclarationSyntaxElement: SyntaxElement, ISyntaxElementAutoReturnsFromScope
{
    public string VariableName { get; set; }
}