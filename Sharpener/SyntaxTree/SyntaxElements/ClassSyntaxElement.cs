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
    public override void FinishSyntaxElement(Document document)
    {
        document.IsInClassPartOfFile = false;
        base.FinishSyntaxElement(document);
    }

    public ClassSyntaxElement WithVisibility(VisibilityLevel visibilityLevel)
    {
        Visibility = visibilityLevel;
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
    
    public override bool WithToken(Document document, IToken token)
    {
        if (document.IsNotifyKeywordUsedInFile)
        {
            if (!InheritsFrom.Contains("INotifyPropertyChanged"))
            {
                InheritsFrom.Add("INotifyPropertyChanged");
            }
        }
        
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
                 var c = ((IGenerateMemberSyntax)child).GenerateCodeNode();
                 if(c == null) continue;
                 classDeclaration = classDeclaration.AddMembers(c);
             }
         }
         
         var attributeSyntaxList = new List<AttributeSyntax>();
         if (Attributes != null)
         {
             foreach (var attr in Attributes)
             {
                 attributeSyntaxList.Add(attr.GenerateCodeNode());
             }
         }
         
         var attributeListSyntax = SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(attributeSyntaxList));

         if (attributeSyntaxList.Count > 0)
         {
             classDeclaration = classDeclaration.WithAttributeLists(SyntaxFactory.SingletonList(attributeListSyntax));
         }
         
         return classDeclaration;
    }
}