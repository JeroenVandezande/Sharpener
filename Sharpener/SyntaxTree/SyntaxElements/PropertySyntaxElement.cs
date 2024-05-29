using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpener.Enums;
using Sharpener.OpenAI;

namespace Sharpener.SyntaxTree.Scopes;

public class PropertySyntaxElement: PropertySyntaxElementBase
{
    private bool _useNotifyInFile;
    private int _getterStart;
    private int _getterEnd;
    private int _setterStart;
    private int _setterEnd;
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

    private AccessorDeclarationSyntax CreateExplicitGetter(string explicitInterfaceName, string explicitPropertyName)
    {
        return SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
            .WithBody(
                SyntaxFactory.Block(
                    SyntaxFactory.SingletonList<StatementSyntax>(
                        SyntaxFactory.ReturnStatement(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.CastExpression(
                                    SyntaxFactory.ParseTypeName(explicitInterfaceName),
                                    SyntaxFactory.ThisExpression()
                                ),
                                SyntaxFactory.IdentifierName(explicitPropertyName)
                            )
                        )
                    )
                )
            );
    }

    private AccessorDeclarationSyntax CreateExplicitSetter(string explicitInterfaceName, string explicitPropertyName)
    {
        return SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
            .WithBody(
                SyntaxFactory.Block(
                    SyntaxFactory.SingletonList<StatementSyntax>(
                        SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.CastExpression(
                                        SyntaxFactory.ParseTypeName(explicitInterfaceName),
                                        SyntaxFactory.ThisExpression()
                                    ),
                                    SyntaxFactory.IdentifierName(explicitPropertyName)
                                ),
                                SyntaxFactory.IdentifierName("value")
                            )
                        )
                    )
                )
            );
    }

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
            SpecialTokenState = SpecialSyntaxToken.Implements;
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
        
        if (base.WithToken(document, token))
        {
            return true;
        }
        
        if (token.TokenType == TokenType.ReadGetterKeyword)
        {
            _getterStart = ((KeywordToken)token).EndColumn + 1;
            SpecialTokenState = SpecialSyntaxToken.Getter;
            return true;
        }
        
        if (token.TokenType == TokenType.WriteSetterKeyword)
        {
            if (SpecialTokenState == SpecialSyntaxToken.Getter)
            {
                _getterEnd = ((KeywordToken)token).StartColumn;
                GetterCode = document.OriginalOxygeneCode[token.LineNumber - 1]
                    .Substring(_getterStart, (_getterEnd - _getterStart));
            }
            _setterStart = ((KeywordToken)token).EndColumn + 1;
            // _getterIsNext = false;
            // _setterIsNext = true;
            SpecialTokenState = SpecialSyntaxToken.Setter;
            return true;
        }
        
        if (token.TokenType == TokenType.SemiColon)
        {
            switch (SpecialTokenState)
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
                    // Return from here, so we don't leave the scope. We already left it in the previous element
                    SpecialTokenState = SpecialSyntaxToken.None;
                    return true;
                    break;
                case SpecialSyntaxToken.None:
                    break;
            }

            SpecialTokenState = SpecialSyntaxToken.None;
            ElementIsFinished = true;
            document.returnFromCurrentScope();
            return true;
        }
        
        if (token is ITokenWithText param)
        {
            switch (SpecialTokenState)
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
                    return true;
                }
                    break;
            }
        }

        return false;
    }

    public override List<MemberDeclarationSyntax> GenerateCodeNodes()
    {
        var result = base.GenerateCodeNodes();
        var vis = Tools.VisibilityToSyntaxKind(VisibilityLevel);
        
        // Create a Property
        if (String.IsNullOrEmpty(Propertytype)) return null;
        var propType = OxygeneTypeTranslator.ConvertOxygeneTypeToCS(Propertytype);
        propType = propType + (IsNullable ? "?" : "");
        
        var propertyDeclaration = SyntaxFactory
            .PropertyDeclaration(SyntaxFactory.ParseTypeName(propType), PropertyName)
            .AddModifiers(SyntaxFactory.Token(vis));
        
        // Handle creating secondary type that references the first explicit one.
        /*
         * For example. The original Oxygene code:
         * property UsedWorklist: IWorklist; implements ITest.Worklist
         *
         * Converts to
         *
         * public IWorklist UsedWorklist
         * {
         *      get { return ((ITest)this).Worklist; }
         *      set { ((ITest)this).Worklist = value; }
         * }
         * IWorklist ITest.Worklist { get; set; }
         */
        if (!String.IsNullOrEmpty(ExplicitPropertyName) && (!String.IsNullOrEmpty(ExplicitInterfaceName)))
        {
            // Add getter and setter code for referencing explicit reference
            propertyDeclaration = propertyDeclaration.AddAccessorListAccessors(
                CreateExplicitGetter(ExplicitInterfaceName, ExplicitPropertyName),
                CreateExplicitSetter(ExplicitInterfaceName, ExplicitPropertyName));
            
            // Create base explicit property
            var getter = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

            // Create the setter accessor
            var setter = SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

            // Create the explicit interface specifier
            var explicitInterfaceSpecifier = SyntaxFactory.ExplicitInterfaceSpecifier(
                SyntaxFactory.IdentifierName(ExplicitInterfaceName));
            
            // Create the property
            var propertyDeclarationOriginalName = SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName(propType), ExplicitPropertyName)
                .WithExplicitInterfaceSpecifier(explicitInterfaceSpecifier)
                .AddAccessorListAccessors(getter, setter);
            
            result.Add(propertyDeclarationOriginalName);
        }
        else if (String.IsNullOrEmpty(GetterCode) && String.IsNullOrEmpty(SetterCode)) //implied getter/setter
        {
            // Add "{ get; set; }" code
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