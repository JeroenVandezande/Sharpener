using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpener.Enums;

namespace Sharpener.SyntaxTree.Scopes;

public class EnumSyntaxElement : SyntaxElement, IGenerateMemberSyntax
{
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
    
    public override void AddParameter(string param, TokenType tokenType)
    {
        EnumValues.Add(param);
    }

    public MemberDeclarationSyntax GenerateCodeNode()
    {
        var enumDeclaration = SyntaxFactory.EnumDeclaration(EnumName);
        /*var classDeclaration = SyntaxFactory.ClassDeclaration(ClassName);

        SyntaxKind vis = SyntaxKind.None;
        switch (Visibility)
        {
            case VisibilityLevel.Public:
                vis = SyntaxKind.PublicKeyword;
                break;
            case VisibilityLevel.Private:
                vis = SyntaxKind.PrivateKeyword;
                break;
            case VisibilityLevel.Protected:
                vis = SyntaxKind.ProtectedKeyword;
                break;
            case VisibilityLevel.Assembly:
                vis = SyntaxKind.InternalKeyword;
                break;
        }
        
         classDeclaration = classDeclaration.AddModifiers(SyntaxFactory.Token(vis));

         foreach (var inh in InheritsFrom)
         {
             classDeclaration =
                 classDeclaration.AddBaseListTypes(
                     SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(inh)));
         }

         foreach (var child in Children)
         {
             if (child is IGenerateMemberSyntax)
             {
                 classDeclaration = classDeclaration.AddMembers(((IGenerateMemberSyntax)child).GenerateCodeNode());
             }
         }
         return classDeclaration;*/
        
        SyntaxKind vis = SyntaxKind.None;
        switch (Visibility)
        {
            case VisibilityLevel.Public:
                vis = SyntaxKind.PublicKeyword;
                break;
            case VisibilityLevel.Private:
                vis = SyntaxKind.PrivateKeyword;
                break;
            case VisibilityLevel.Protected:
                vis = SyntaxKind.ProtectedKeyword;
                break;
            case VisibilityLevel.Assembly:
                vis = SyntaxKind.InternalKeyword;
                break;
        }
        
        enumDeclaration = enumDeclaration.AddModifiers(SyntaxFactory.Token(vis));

        List<EnumMemberDeclarationSyntax> members = new List<EnumMemberDeclarationSyntax>();
        foreach (var e in EnumValues)
        {
           members.Add(SyntaxFactory.EnumMemberDeclaration(e)); 
        }

        enumDeclaration = enumDeclaration.AddMembers(members.ToArray());
        return enumDeclaration;
    }
}