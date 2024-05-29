using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpener.Enums;

namespace Sharpener.SyntaxTree.Scopes;

public class EnumSyntaxElement : SyntaxElement, IGenerateMemberSyntax
{
    private bool _pastLastEnumValue = false;
    public String EnumName { get; set; }
    public List<String> EnumValues { get; set; } = new List<string>();
    public VisibilityLevel Visibility { get; set; }

    public override void FinishSyntaxElement(Document document)
    {
        base.FinishSyntaxElement(document);
    }

    public EnumSyntaxElement WithVisibility(VisibilityLevel visibilityLevel)
    {
        Visibility = visibilityLevel;
        return this;
    }
    
    
    public EnumSyntaxElement WithEnumName(String name)
    {
        EnumName = name;
        return this;
    }
    
    public override bool WithToken(Document document, IToken token)
    {
        if (token.TokenType == TokenType.SemiColon)
        {
            _pastLastEnumValue = true;
            return true;
        }
        
        if (token is ITokenWithText param)
        {
            if (!_pastLastEnumValue)
            {
                EnumValues.Add(param.TokenText);
                return true;
            }
        }

        return false;
    }
    
    public List<MemberDeclarationSyntax> GenerateCodeNodes()
    {
        var result = new List<MemberDeclarationSyntax>();
        var enumDeclaration = SyntaxFactory.EnumDeclaration(EnumName);

        SyntaxKind vis = Tools.VisibilityToSyntaxKind(Visibility);
        
        enumDeclaration = enumDeclaration.AddModifiers(SyntaxFactory.Token(vis));

        List<EnumMemberDeclarationSyntax> members = new List<EnumMemberDeclarationSyntax>();
        foreach (var e in EnumValues)
        {
           members.Add(SyntaxFactory.EnumMemberDeclaration(e)); 
        }

        enumDeclaration = enumDeclaration.AddMembers(members.ToArray());
        result.Add(enumDeclaration);
        return result;
    }
}