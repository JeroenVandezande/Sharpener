using Sharpener.Enums;

namespace Sharpener.SyntaxTree.Scopes;

public class NewObjectSyntaxElement: SyntaxElement, ISyntaxElementAutoReturnsFromScope
{
    public string ClassName { get; set; }
    
    public override void AddParameter(string param, TokenType tokenType)
    {
        if (String.IsNullOrEmpty(ClassName))
        {
            ClassName = param;
        }
    }
}