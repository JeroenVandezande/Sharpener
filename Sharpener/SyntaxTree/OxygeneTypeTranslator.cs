namespace Sharpener.SyntaxTree;

public static class OxygeneTypeTranslator
{
    public static string ConvertOxygeneTypeToCS(string oxygeneTypeName)
    {
        return oxygeneTypeName.ToLower() switch
        {
            "integer" => "int",
            "boolean" => "bool",
            "double" => "double",
            "string" => "string",
            _ => oxygeneTypeName
        };
    }
}