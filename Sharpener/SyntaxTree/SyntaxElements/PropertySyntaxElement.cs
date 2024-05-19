using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpener.Enums;

namespace Sharpener.SyntaxTree.Scopes;

public class PropertySyntaxElement: SyntaxElement, ISyntaxElementWithScope, IGenerateMemberSyntax
{
    private bool _getterIsNext = false;
    private bool _setterIsNext = false;
    public string PropertyName { get; set; }
    public string Propertytype { get; set; }
    public string GetterCode { get; set; } = String.Empty;
    public string SetterCode { get; set; } = String.Empty;
    public bool IsNullable { get; set; }
    public bool HasNotifyPatternApplied { get; set; }
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

    public override bool WithToken(Document document, IToken token)
    {
        if (token.TokenType == TokenType.NotifyPropertyChangedImplementation)
        {
            HasNotifyPatternApplied = true;
            return true;
        }
        
        if (token.TokenType == TokenType.ReadGetterKeyword)
        {
            _getterIsNext = true;
            _setterIsNext = false;
            return true;
        }
        
        if (token.TokenType == TokenType.WriteSetterKeyword)
        {
            _getterIsNext = false;
            _setterIsNext = true;
            return true;
        }
        
        if (token.TokenType == TokenType.SemiColon)
        {
            _getterIsNext = false;
            _setterIsNext = false;
            ElementIsFinished = true;
            return true;
        }

        if (_getterIsNext)
        {
            // TODO get the getter Oxygene code
            //GetterCode += 
            return true;
        }
        
        if (_setterIsNext)
        {
            // TODO get the setter Oxygene code
            //SetterCode += 
            return true;
        }
        
        if (token is ITokenWithText param)
        {
            if (String.IsNullOrEmpty(PropertyName))
            {
                PropertyName = param.TokenText;
                return true;
            }

            if (String.IsNullOrEmpty(Propertytype))
            {
                Propertytype = param.TokenText;
                return true;
            }
        }

        return false;
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