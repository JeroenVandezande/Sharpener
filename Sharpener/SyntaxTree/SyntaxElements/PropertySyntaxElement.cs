using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpener.Enums;
using Sharpener.OpenAI;

namespace Sharpener.SyntaxTree.Scopes;

public class PropertySyntaxElement: SyntaxElement, ISyntaxElementWithScope, IGenerateMemberSyntax
{
    private bool _useNotifyInFile;
    private int _getterStart;
    private int _getterEnd;
    private int _setterStart;
    private int _setterEnd;
    private enum SpecialSyntaxToken
    {
        None,
        Getter,
        Setter,
        Implements
    }
    private SpecialSyntaxToken _specialTokenState;
    public string? ExplicitInterfaceName { get; set; }
    public string? ExplicitPropertyName { get; set; }
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
        
        if (token.TokenType == TokenType.NotifyPropertyChangedImplementation)
        {
            HasNotifyPatternApplied = true;
            return true;
        }

        if (token.TokenType == TokenType.ImplementsInheritanceKeyword)
        {
            _specialTokenState = SpecialSyntaxToken.Implements;
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
            _specialTokenState = SpecialSyntaxToken.Getter;
            return true;
        }
        
        if (token.TokenType == TokenType.WriteSetterKeyword)
        {
            if (_specialTokenState == SpecialSyntaxToken.Getter)
            {
                _getterEnd = ((KeywordToken)token).StartColumn;
                GetterCode = document.OriginalOxygeneCode[token.LineNumber - 1]
                    .Substring(_getterStart, (_getterEnd - _getterStart));
            }
            _setterStart = ((KeywordToken)token).EndColumn + 1;
            // _getterIsNext = false;
            // _setterIsNext = true;
            _specialTokenState = SpecialSyntaxToken.Setter;
            return true;
        }
        
        if (token.TokenType == TokenType.SemiColon)
        {
            switch (_specialTokenState)
            {
                case SpecialSyntaxToken.Getter:
                    {
                        if (token is SeperatorToken kwt)
                        {
                            _getterEnd = kwt.StartColumn;
                            GetterCode = document.OriginalOxygeneCode[token.LineNumber - 1]
                                .Substring(_getterStart, (_getterEnd - _getterStart));
                        }
                    }
                    break;
                case SpecialSyntaxToken.Setter:
                    {
                        if (token is SeperatorToken kwt)
                        {
                            _setterEnd = kwt.StartColumn;
                            SetterCode = document.OriginalOxygeneCode[token.LineNumber - 1]
                                .Substring(_setterStart, (_setterEnd - _setterStart));
                        }
                    }
                    break;
                case SpecialSyntaxToken.Implements:
                    break;
                case SpecialSyntaxToken.None:
                    break;
            }

            _specialTokenState = SpecialSyntaxToken.None;
            ElementIsFinished = true;
            document.returnFromCurrentScope();
            return true;
        }
        
        if (token is ITokenWithText param)
        {
            switch (_specialTokenState)
            {
                case SpecialSyntaxToken.None: // Regular case handles "PropertyName: Type" syntax
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
                    break;
                case SpecialSyntaxToken.Implements: // Case handles explicit interface implementation (i.e. implements)
                {
                    var explicitParts = param.TokenText.Split(".");
                    if (explicitParts.Length != 2)
                    {
                        throw new FormatException(
                            "Explicit Implementation Missing Format: \"implements IInterface.Member\"");
                        
                    }

                    ExplicitInterfaceName = explicitParts[0];
                    ExplicitPropertyName = explicitParts[1];
                }
                    break;
            }
        }

        return false;
    }

    public List<MemberDeclarationSyntax> GenerateCodeNodes()
    {
        var result = new List<MemberDeclarationSyntax>();
        var vis = Tools.VisibilityToSyntaxKind(VisibilityLevel);

        
        
        // Create a Property
        if (String.IsNullOrEmpty(Propertytype)) return null;
        var propType = OxygeneTypeTranslator.ConvertOxygeneTypeToCS(Propertytype);
        propType = propType + (IsNullable ? "?" : "");
        
        // Handle creating secondary type that references the first implicit one.
        /*
         * For example. The original Oxygene code:
         * property UsedWorklist: IWorklist; implements ITest.Worklist
         *
         * Converts to
         *
         * public IWorklist UsedWorklist
         * { 
		        get {return Worklist;}
		        set {Worklist = value;}
		   }
		   
         */
        var actualPropertyName = String.Empty;
        if (!String.IsNullOrEmpty(ExplicitPropertyName))
        {
            actualPropertyName = ExplicitPropertyName;
            var propertyDeclarationOriginalName = SyntaxFactory
                .PropertyDeclaration(SyntaxFactory.ParseTypeName(propType), PropertyName)
                .AddModifiers(SyntaxFactory.Token(vis));
        }
        else
        {
            actualPropertyName = PropertyName;
        }
        var propertyDeclaration = SyntaxFactory
            .PropertyDeclaration(SyntaxFactory.ParseTypeName(propType), actualPropertyName)
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
                foreach (var member in attr.GenerateCodeNodes())
                {
                    attributeSyntaxList.Add(member);
                }
            }
        }
         
        var attributeListSyntax = SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(attributeSyntaxList));

        if (attributeSyntaxList.Count > 0)
        {
            propertyDeclaration = propertyDeclaration.WithAttributeLists(SyntaxFactory.SingletonList(attributeListSyntax));
        }
        result.Add(propertyDeclaration);
        return result;
    }
}