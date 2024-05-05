using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpener.Enums;

namespace Sharpener.SyntaxTree.Scopes;

public class ClassSyntaxElement : SyntaxElement, ISyntaxElementWithScope, IGenerateMemberSyntax
{
    public String ClassName { get; set; }
    public List<String> InheritsFrom { get; set; } = new List<string>();
    public VisibilityLevel Visibility { get; set; }
    public bool IsStatic { get; set; }
    
    public ClassSyntaxElement WithVisibility(VisibilityLevel visibilityLevel)
    {
        visibilityLevel = visibilityLevel;
        return this;
    }

    public ClassSyntaxElement WithStaticApplied(bool isStaticApplied)
    {
        IsStatic = isStaticApplied;
        return this;
    }
    
    public ClassSyntaxElement WithClassName(String name)
    {
        ClassName = name;
        return this;
    }
    
    public override void AddParameter(string param, TokenType tokenType)
    {
        InheritsFrom.Add(param);
    }

    public MemberDeclarationSyntax GenerateCodeNode()
    {
        var classDeclaration = SyntaxFactory.ClassDeclaration(ClassName);

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
                vis = SyntaxKind.AssemblyKeyword;
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
         return classDeclaration;
    }
}