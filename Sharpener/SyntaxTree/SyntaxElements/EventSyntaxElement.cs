using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpener.Enums;

namespace Sharpener.SyntaxTree.SyntaxElements;

public class EventSyntaxElement: SyntaxElement, ISyntaxElementWithScope, IGenerateMemberSyntax
{
    private bool _nextTokenIsEventType;
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
        if (token.TokenType == TokenType.Colon)
        {
            _nextTokenIsEventType = true;
            return true;
        }

        if (token is ITokenWithText tokenWithText)
        {
            if (_nextTokenIsEventType)
            {
                EventType = tokenWithText.TokenText;
            }
            else
            {
                EventName = tokenWithText.TokenText;
            }
        }
        
        return false;
    }
    
    public List<MemberDeclarationSyntax> GenerateCodeNodes()
    {
        var result = new List<MemberDeclarationSyntax>();

        EventFieldDeclarationSyntax eventField = SyntaxFactory.EventFieldDeclaration(
            SyntaxFactory.VariableDeclaration(
                    SyntaxFactory.ParseTypeName(EventType))
                .WithVariables(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(EventName))));
            /*.WithModifiers(SyntaxFactory.TokenList(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword)));*/
        result.Add(eventField);
        return result;
    }
}