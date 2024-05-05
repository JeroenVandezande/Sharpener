using Sharpener.Enums;

namespace Sharpener.SyntaxTree.Scopes;

public class CommentSyntaxElement: SyntaxElement
{
    public List<String> CommentLines { get; set; } = new List<string>();
    
    public CommentType CommentType { get; set; }
}