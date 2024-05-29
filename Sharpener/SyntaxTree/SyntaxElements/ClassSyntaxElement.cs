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
        document.CurrentContainingTypeElement = ContainingTypeElement.None;
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

    public List<MemberDeclarationSyntax> GenerateCodeNodes()
    {
        var result = new List<MemberDeclarationSyntax>();
        var classDeclaration = SyntaxFactory.ClassDeclaration(ClassName);

        SyntaxKind vis = Tools.VisibilityToSyntaxKind(Visibility);
        
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
                 var c = ((IGenerateMemberSyntax)child).GenerateCodeNodes();
                 if(c == null) continue;
                 classDeclaration = classDeclaration.AddMembers(c.ToArray());
             }
         }
         
         var attributeSyntaxList = new List<AttributeSyntax>();
         if (Attributes != null)
         {
             foreach (var attr in Attributes)
             {
                 foreach (var member in attr.GenerateCodeNodes())
                 {
                    attributeSyntaxList.Add(member);
                 }
             }
         }
         
         var attributeListSyntax = SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(attributeSyntaxList));

         if (attributeSyntaxList.Count > 0)
         {
             classDeclaration = classDeclaration.WithAttributeLists(SyntaxFactory.SingletonList(attributeListSyntax));
         }
         
         result.Add(classDeclaration);
         return result;
    }
}