using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpener.Enums;

namespace Sharpener.SyntaxTree.Scopes;

public class ConstantSyntaxElement: SyntaxElement, ISyntaxElementWithScope, IGenerateMemberSyntax
{
    private bool _staticReadonlyType;
    private bool _inTypeSection;
    private bool _inValueSection;
    public string ConstantName { get; set; }
    public string ConstantValue { get; set; }
    public string? ConstantType { get; set; }
    
    public VisibilityLevel VisibilityLevel { get; set; }

    public ConstantSyntaxElement WithVisibility(VisibilityLevel visibilityLevel)
    {
        VisibilityLevel = visibilityLevel;
        return this;
    }

    public override bool WithToken(Document document, IToken token)
    {
        if (token.TokenType == TokenType.Colon)
        {
            _inTypeSection = true;
            return true;
        }
        
        if (token.TokenType == TokenType.BooleanEquals)
        {
            _inValueSection = true;
            _inTypeSection = false;
            return true;
        }
        
        if (_inTypeSection)
        {
            ConstantType += token.StringRepresentation;
            return true;
        }
        
        if (token is ITokenWithText param)
        {
            if (String.IsNullOrEmpty(ConstantName))
            {
                ConstantName = param.TokenText;
                return true;
            }

            if (String.IsNullOrEmpty(ConstantValue))
            {
                ConstantValue = param.TokenText;
                return true;
            }
            
            if (_inTypeSection)
            {
                ConstantType = param.TokenText;
                _inTypeSection = false;
                return true;
            }
        }

        if (token.TokenType == TokenType.BooleanEquals)
        {
            _inValueSection = true;
            return true;
        }

        if (token.TokenType == TokenType.SemiColon)
        {
            ConstantValue = Tools.ConvertOxygeneSpecialValueToCS(ConstantValue);
            
            _inValueSection = false;
            
            if (String.IsNullOrEmpty(ConstantType))
            {
                ConstantType = GetTypeFromValue(ConstantValue);
            }
            else
            {
                // Convert oxygene special built-in types to regular C# ones (i.e. Integer => int)
                ConstantType = Tools.ConvertOxygeneSpecialTypeToCS(ConstantType);
            }

            // Oxygene allows constant types that C# does not. This should be converted to static readonly types
            _staticReadonlyType = !IsTypeValidConst(ConstantType);
            
            ElementIsFinished = true;
            document.returnFromCurrentScope();
            return true;
        }

        // When implicitally determining a value we want to add ALL characters, operators, etc. Until the semicolon
        if (_inValueSection)
        {
            ConstantValue = ConstantValue + token.StringRepresentation;
            return true;
        }

        return false;
    }

    public List<MemberDeclarationSyntax> GenerateCodeNodes()
    {
        var result = new List<MemberDeclarationSyntax>();
        
        // Create the constant value expression
        var valueExpression = SyntaxFactory.ParseExpression(ConstantValue);

        // Create the variable declarator
        var variableDeclarator = SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(ConstantName))
            .WithInitializer(SyntaxFactory.EqualsValueClause(valueExpression));

        // Create the variable declaration
        var variableDeclaration = SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName(ConstantType))
            .AddVariables(variableDeclarator);

        // Determine the appropriate modifiers based on _staticReadonlyType
        SyntaxTokenList modifiers;
        if (_staticReadonlyType)
        {
            modifiers = SyntaxFactory.TokenList(Tools.VisibilityToSyntaxToken(VisibilityLevel), 
                SyntaxFactory.Token(SyntaxKind.StaticKeyword), 
                SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword));
        }
        else
        {
            modifiers = SyntaxFactory.TokenList(Tools.VisibilityToSyntaxToken(VisibilityLevel), 
                SyntaxFactory.Token(SyntaxKind.ConstKeyword));
        }

        // Create the field declaration with the determined modifiers
        var fieldDeclaration = SyntaxFactory.FieldDeclaration(variableDeclaration)
            .WithModifiers(modifiers);
        
        result.Add(fieldDeclaration);

        return result;
    }

    private bool IsTypeValidConst(string typeValue)
    {
        // List of valid constant types in C#
        var validConstTypes = new HashSet<string>
        {
            "bool",
            "byte",
            "sbyte",
            "short",
            "ushort",
            "int",
            "uint",
            "long",
            "ulong",
            "float",
            "double",
            "decimal",
            "char",
            "string"
        };

        // Check if the given string is a valid constant type
        if (validConstTypes.Contains(typeValue))
        {
            return true;
        }

        return false;
    }

    // Type inference based on value
    private static string GetTypeFromValue(string value)
    {
        if (bool.TryParse(value, out _))
            return "bool";
        
        if (byte.TryParse(value, out _))
            return "byte";
        
        if (sbyte.TryParse(value, out _))
            return "sbyte";

        if (short.TryParse(value, out _))
            return "short";
        
        if (ushort.TryParse(value, out _))
            return "ushort";

        if (int.TryParse(value, out _))
            return "int";
        
        if (uint.TryParse(value, out _))
            return "uint";

        if (long.TryParse(value, out _))
            return "long";
        
        if (ulong.TryParse(value, out _))
            return "ulong";
        
        if (float.TryParse(value, out _))
            return "float";
        
        if (double.TryParse(value, out _))
            return "double";
        
        if (decimal.TryParse(value, out _))
            return "decimal";
        
        if (char.TryParse(value, out _))
            return "char";

        if (Regex.Matches(value, TokenizerConstants.MatchStringLiteralRegex).Any() || 
            Regex.Matches(value, TokenizerConstants.MatchSingleQuoteStringLiteralRegex).Any())
            return "string";
        
        // return the string as a custom type (This could be an Enum)
        return value;
    }
}