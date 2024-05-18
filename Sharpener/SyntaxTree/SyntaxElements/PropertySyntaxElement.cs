using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpener.Enums;

namespace Sharpener.SyntaxTree.Scopes;

public class PropertySyntaxElement: SyntaxElement, ISyntaxElementWithScope, IGenerateMemberSyntax
{
    public string PropertyName { get; set; }
    public string Propertytype { get; set; }
    public string GetterCode { get; set; }
    public string SetterCode { get; set; }
    public bool IsNullable { get; set; }
    public VisibilityLevel VisibilityLevel { get; set; }
    public bool IsStatic { get; set; }

    public PropertySyntaxElement WithVisibility(VisibilityLevel visibilityLevel)
    {
        VisibilityLevel = visibilityLevel;
        return this;
    }

    public PropertySyntaxElement WithStaticApplied(bool isStaticApplied)
    {
        IsStatic = isStaticApplied;
        return this;
    }

    public override void AddParameter(string param, TokenType tokenType)
    {
        if (String.IsNullOrEmpty(PropertyName))
        {
            PropertyName = param;
            return;
        }
        if (String.IsNullOrEmpty(Propertytype))
        {
            Propertytype = param;
            return;
        }
    }

    public MemberDeclarationSyntax GenerateCodeNode()
    {
        var vis = Tools.VisibilityToSyntaxKind(VisibilityLevel);
       
        // Create a Property
        if (String.IsNullOrEmpty(Propertytype)) return null;
        var propType = OxygeneTypeTranslator.ConvertOxygeneTypeToCS(Propertytype);
        propType = propType + (IsNullable ? "?" : "");
        var propertyDeclaration = SyntaxFactory
            .PropertyDeclaration(SyntaxFactory.ParseTypeName(propType), PropertyName)
            .AddModifiers(SyntaxFactory.Token(vis))
            .AddAccessorListAccessors(
               SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
               SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
        
        if (IsStatic)
        {
            propertyDeclaration = propertyDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword));
        }
        
        return propertyDeclaration;
    }
}