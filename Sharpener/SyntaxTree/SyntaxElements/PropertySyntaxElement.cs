using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpener.Enums;
using Sharpener.OpenAI;

namespace Sharpener.SyntaxTree.Scopes;

public class PropertySyntaxElement: SyntaxElement, ISyntaxElementWithScope, IGenerateMemberSyntax
{
    private bool _useNotifyInFile;
    private bool _getterIsNext = false;
    private bool _setterIsNext = false;
    private int _getterStart;
    private int _getterEnd;
    private int _setterStart;
    private int _setterEnd;
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
        _useNotifyInFile = document.IsNotifyKeywordUsedInFile;
        if (token.TokenType == TokenType.NullableTypeDefinition)
        {
            IsNullable = true;
            return true;
        }
        
        if ((token.TokenType == TokenType.OpenBracket) || (token.TokenType == TokenType.ClosedBracket))
        {
            if (!ElementIsFinished)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        if (token.TokenType == TokenType.ReadGetterKeyword)
        {
            _getterStart = ((KeywordToken)token).EndColumn + 1;
            _getterIsNext = true;
            _setterIsNext = false;
            return true;
        }
        
        if (token.TokenType == TokenType.WriteSetterKeyword)
        {
            if (_getterIsNext)
            {
                _getterEnd = ((KeywordToken)token).StartColumn;
                GetterCode = document.OriginalOxygeneCode[token.LineNumber - 1]
                    .Substring(_getterStart, (_getterEnd - _getterStart));
            }
            _setterStart = ((KeywordToken)token).EndColumn + 1;
            _getterIsNext = false;
            _setterIsNext = true;
            return true;
        }
        
        if (token.TokenType == TokenType.SemiColon)
        {
            if (_getterIsNext)
            {
                if (token is SeperatorToken kwt)
                {
                    _getterEnd = kwt.StartColumn;
                    GetterCode = document.OriginalOxygeneCode[token.LineNumber - 1]
                        .Substring(_getterStart, (_getterEnd - _getterStart));
                }
            }
            if (_setterIsNext)
            {
                if (token is SeperatorToken kwt)
                {
                    _setterEnd = kwt.StartColumn;
                    SetterCode = document.OriginalOxygeneCode[token.LineNumber - 1]
                        .Substring(_setterStart, (_setterEnd - _setterStart));
                }
            }
            _getterIsNext = false;
            _setterIsNext = false;
            ElementIsFinished = true;
            document.returnFromCurrentScope();
            document.LastUsedProperty = this;
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
            .AddModifiers(SyntaxFactory.Token(vis));

        if (String.IsNullOrEmpty(GetterCode) && String.IsNullOrEmpty(SetterCode)) //implied getter/setter
        {
            propertyDeclaration = propertyDeclaration.AddAccessorListAccessors(
            SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
            SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))); 
        }

        if (!String.IsNullOrEmpty(GetterCode))
        {
            propertyDeclaration = propertyDeclaration.AddAccessorListAccessors(
                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.ParseExpression(PropertyGetterTranslation.TranslateOxygeneToCS(GetterCode))))
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
        }
        
        if (!String.IsNullOrEmpty(SetterCode))
        {
            propertyDeclaration = propertyDeclaration.AddAccessorListAccessors(
                SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.ParseExpression(PropertySetterTranslation.TranslateOxygeneToCS(SetterCode))))
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
        }

        var attributeSyntaxList = new List<AttributeSyntax>();
        
        if (IsStatic)
        {
            propertyDeclaration = propertyDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword));
        }
        else
        {
            if (_useNotifyInFile)
            {
                if (!HasNotifyPatternApplied)
                {
                    attributeSyntaxList.Add(SyntaxFactory.Attribute(SyntaxFactory.ParseName("DoNotNotify")));
                }
            }
        }
        
        
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
            propertyDeclaration = propertyDeclaration.WithAttributeLists(SyntaxFactory.SingletonList(attributeListSyntax));
        }
        
        return propertyDeclaration;
    }
}