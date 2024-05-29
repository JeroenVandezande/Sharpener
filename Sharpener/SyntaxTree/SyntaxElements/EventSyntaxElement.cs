using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpener.Enums;

namespace Sharpener.SyntaxTree.SyntaxElements;

public class EventSyntaxElement: SyntaxElement, ISyntaxElementWithScope, IGenerateMemberSyntax
{
    public VisibilityLevel VisibilityLevel { get; set; }
    public bool IsStatic { get; set; }

    public string? EventName;
    public string? EventType;

    public EventSyntaxElement WithVisibility(VisibilityLevel visibilityLevel)
    {
        VisibilityLevel = visibilityLevel;
        return this;
    }
    
    public EventSyntaxElement WithStaticApplied(bool isStaticApplied)
    {
        IsStatic = isStaticApplied;
        return this;
    }
    
    public override bool WithToken(Document document, IToken token)
    {
        if (token.TokenType == TokenType.SemiColon)
        {
            ElementIsFinished = true;
            document.returnFromCurrentScope();
            return true;
        }
        return false;
    }
    
    public List<MemberDeclarationSyntax> GenerateCodeNodes()
    {
        var result = new List<MemberDeclarationSyntax>();
        return result;
    }
}