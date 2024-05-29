using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpener.Enums;

namespace Sharpener.SyntaxTree.Scopes;

public class InterfaceSyntaxElement : SyntaxElement, ISyntaxElementWithScope, IGenerateMemberSyntax
{
    public String InterfaceName { get; set; }
    public List<String> InheritsFrom { get; set; } = new List<string>();
    public VisibilityLevel Visibility { get; set; }
    public override void FinishSyntaxElement(Document document)
    {
        document.CurrentContainingTypeElement = ContainingTypeElement.None;
        base.FinishSyntaxElement(document);
    }
    
    public InterfaceSyntaxElement WithVisibility(VisibilityLevel visibilityLevel)
    {
        Visibility = visibilityLevel;
        return this;
    }
    
    public InterfaceSyntaxElement WithInterfaceName(String name)
    {
        InterfaceName = name;
        return this;
    }
    
    public override bool WithToken(Document document, IToken token)
    {
        if (token is ITokenWithText param)
        {
            if (!ElementIsFinished)
            {
                if (!InheritsFrom.Contains(param.TokenText))
                {
                    InheritsFrom.Add(param.TokenText);
                }
                return true;
            }
        }

        return false;
    }

    public List<MemberDeclarationSyntax> GenerateCodeNodes()
    {
        var result = new List<MemberDeclarationSyntax>();
        var interfaceDeclaration = SyntaxFactory.InterfaceDeclaration(InterfaceName);

        SyntaxKind vis = Tools.VisibilityToSyntaxKind(Visibility);
        
        interfaceDeclaration = interfaceDeclaration.AddModifiers(SyntaxFactory.Token(vis));

         foreach (var inh in InheritsFrom)
         {
             interfaceDeclaration =
                 interfaceDeclaration.AddBaseListTypes(
                     SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(inh)));
         }
         
         foreach (var child in Children)
         {
             if (child is IGenerateMemberSyntax)
             {
                 var c = ((IGenerateMemberSyntax)child).GenerateCodeNodes();
                 if(c == null) continue;
                 foreach (var member in c)
                 {
                    interfaceDeclaration = interfaceDeclaration.AddMembers(member);
                 }
             }
         }
         result.Add(interfaceDeclaration);
         return result;
    }
}