using System.Globalization;
using Microsoft.CodeAnalysis;
using Sharpener.Enums;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sharpener.SyntaxTree;

public static class Tools
{
    public static SyntaxKind VisibilityToSyntaxKind(VisibilityLevel visibilityLevel)
    {
        SyntaxKind vis = SyntaxKind.None;
        switch (visibilityLevel)
        {
            case VisibilityLevel.Public:
                vis = SyntaxKind.PublicKeyword;
                break;
            case VisibilityLevel.Private:
                vis = SyntaxKind.PrivateKeyword;
                break;
            case VisibilityLevel.Assembly:
                vis = SyntaxKind.AssemblyKeyword;
                break;
            case VisibilityLevel.Protected:
                vis = SyntaxKind.ProtectedKeyword;
                break;
        }

        return vis;
    }
    
    public static SyntaxToken VisibilityToSyntaxToken(VisibilityLevel visibilityLevel)
    {
        return SyntaxFactory.Token(VisibilityToSyntaxKind(visibilityLevel));
    }
    
    public static string ConvertOxygeneSpecialTypeToCS(string oxygeneTypeName)
    {
        return oxygeneTypeName.ToLower() switch
        {
            "integer" => "int",
            "boolean" => "bool",
            _ => oxygeneTypeName
        };
    }

    public static string ConvertOxygeneSpecialValueToCS(string constantValue)
    {
        var value = constantValue.Trim();
        if (value.StartsWith("$")) //Hex Value
        {
            return Int64.Parse(value.ToLower().Substring(1),
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture).ToString();
        }

        // Return same value if no special values were found
        return constantValue;
    }
}