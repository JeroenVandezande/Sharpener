using Sharpener.Enums;

namespace Sharpener.SyntaxTree.Scopes;

public class ConstructorSyntaxElement: SyntaxElement, ISyntaxElementWithScope, ISyntaxElementAutoReturnsFromScope
{
    private bool _ParamType;
    private String _ParamName;
    public List<KeyValuePair<string, string>> Parameters { get; } = new List<KeyValuePair<string, string>>();
    public VisibilityLevel VisibilityLevel { get; set; }
    public bool IsStatic { get; set; }
    
    public override void FinishSyntaxElement(Document document)
    {
        base.FinishSyntaxElement(document);
        document.LastKnownInCodeBlock = false;
    }
    
    public ConstructorSyntaxElement WithVisibility(VisibilityLevel visibilityLevel)
    {
        visibilityLevel = visibilityLevel;
        return this;
    }

    public ConstructorSyntaxElement WithStaticApplied(bool isStaticApplied)
    {
        IsStatic = isStaticApplied;
        return this;
    }
    public override void AddParameter(string param, TokenType tokenType)
    {
        if (!_ParamType)
        {
            _ParamName = param;
            _ParamType = true;
        }
        else
        {
            var kvp = new KeyValuePair<string, string>(_ParamName, param);
            Parameters.Add(kvp);
            _ParamType = false;
        }
    }
}