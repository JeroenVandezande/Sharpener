using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpener.Enums;
using Sharpener.OpenAI;

namespace Sharpener.SyntaxTree.Scopes;


public class MethodImplementationElement : SyntaxElement, ISyntaxElementWithScope, ISyntaxElementAutoReturnsFromScope
{
    private bool _NextParamIsReturnType;
    private bool _ParamType;
    private String _ParamName;
    
    public string ClassName { get; set; }
    public string MethodName { get; set; }
    public string ReturnType { get; set; } = "void";
    public List<KeyValuePair<string, string>> Parameters { get; } = new List<KeyValuePair<string, string>>();
    public override void FinishSyntaxElement(Document document)
    {
        base.FinishSyntaxElement(document);
        document.LastKnownInCodeBlock = false;
        var cls = document.FindClassByName(ClassName);
        if (cls != null)
        {
            foreach (var e in cls.Children)
            {
                if (e is MethodElement me)
                {
                    if (me.MethodName.ToLower() == MethodName.ToLower())
                    {
                        me.OriginalSourceCode = OriginalSourceCode;
                    }
                }
            }
        }
    }
    
    public override bool WithToken(Document document, IToken token)
    {
        if (token is ITokenWithText param)
        {
            if (token.TokenType == TokenType.ClosedParathesis)
            {
                _NextParamIsReturnType = true;
                return true;
            }

            if (_NextParamIsReturnType)
            {
                ReturnType = param.TokenText;
                return true;
            }

            if (string.IsNullOrEmpty(ClassName) && string.IsNullOrEmpty(MethodName))
            {
                var classAndMethod = param.TokenText.Split(".");
                ClassName = classAndMethod[0];
                MethodName = classAndMethod[1];
                return true;
            }
            else
            {
                if (!_ParamType)
                {
                    _ParamName = param.TokenText;
                    _ParamType = true;
                    return true;
                }
                else
                {
                    var kvp = new KeyValuePair<string, string>(_ParamName, param.TokenText);
                    Parameters.Add(kvp);
                    _ParamType = false;
                    return true;
                }
            }
        }

        return false;
    }

}

public class MethodElement: SyntaxElement, ISyntaxElementWithScope, IGenerateMemberSyntax, ISyntaxElementAutoReturnsFromScope
{
    private bool _ParamType;
    private String _ParamName;
    private bool _NextParamIsReturnType;
    public string MethodName { get; set; }
    public string ReturnType { get; set; } = "void";
    public List<KeyValuePair<string, string>> Parameters { get; } = new List<KeyValuePair<string, string>>();
    public VisibilityLevel VisibilityLevel { get; set; }
    public bool IsStatic { get; set; }
    public bool IsEmpty { get; set; }

    public override void FinishSyntaxElement(Document document)
    {
        base.FinishSyntaxElement(document);
        document.LastKnownInCodeBlock = false;
    }

    public MethodElement WithVisibility(VisibilityLevel visibilityLevel)
    {
        VisibilityLevel = visibilityLevel;
        return this;
    }

    public MethodElement WithStaticApplied(bool isStaticApplied)
    {
        IsStatic = isStaticApplied;
        return this;
    }
    
    public override bool WithToken(Document document, IToken token)
    {
        if (token is ITokenWithText param)
        {
            if(token.TokenType == TokenType.EmptyKeyword)
            {
                if (document.CurrentScope is MethodElement me)
                {
                    var current = document.returnFromCurrentScope();
                    current.OriginalSourceCodeStopLineNumber = token.LineNumber;
                    current.OriginalSourceCodeStopColumnNumber = token.TokenIndex;
                    current.FinishSyntaxElement(document);
                    me.IsEmpty = true;
                    return true;
                }
            }
            
            if (token.TokenType == TokenType.ClosedParathesis)
            {
                _NextParamIsReturnType = true;
                return true;
            }

            if (token.TokenType == TokenType.Colon)
            {
                _NextParamIsReturnType = true;
                return true;
            }

            if (_NextParamIsReturnType)
            {
                ReturnType = param.TokenText;
                _NextParamIsReturnType = false;
                return true;
            }

            if (string.IsNullOrEmpty(MethodName))
            {
                MethodName = param.TokenText;
                return true;
            }
            else
            {
                if (!_ParamType)
                {
                    _ParamName = param.TokenText;
                    _ParamType = true;
                    return true;
                }
                else
                {
                    var kvp = new KeyValuePair<string, string>(_ParamName, param.TokenText);
                    Parameters.Add(kvp);
                    _ParamType = false;
                    return true;
                }
            }
        }

        return false;
    }

    public MemberDeclarationSyntax GenerateCodeNode()
    {
        //setup parameters
        var paramList = new List<ParameterSyntax>();
        foreach (var par in Parameters)
        {
            paramList.Add(SyntaxFactory.Parameter(SyntaxFactory.Identifier(par.Key))
                .WithType(SyntaxFactory.ParseTypeName(par.Value)));
        }
        var methodparams = SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(paramList));
        
        var vis = Tools.VisibilityToSyntaxKind(VisibilityLevel);
        // Create a method
        var methodDeclaration = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName(ReturnType), MethodName)
            .AddModifiers(SyntaxFactory.Token(vis))
            .WithBody(SyntaxFactory.Block())
            .WithParameterList(methodparams);
        if (IsStatic)
        {
            methodDeclaration = methodDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword));
        }
        
        if (IsEmpty)
        {
            return methodDeclaration;
        }
        
        var cscode = MethodBodyTranslation.TranslateOxygeneToCS(OriginalSourceCode);
        var cscodeLines = cscode.Split("\n").ToList();
        var methodStatements = new List<StatementSyntax>();
        SyntaxTrivia previousComment = SyntaxFactory.Comment("");
        bool hasComment = false;
        foreach (var codeline in cscodeLines)
        {
            if (codeline.Trim().StartsWith("//"))
            {
                previousComment = SyntaxFactory.Comment(codeline.Trim());
                hasComment = true;
            }
            else
            {
                var ms = SyntaxFactory.ParseStatement(codeline);
                if (hasComment)
                {
                    ms = ms.WithLeadingTrivia(previousComment);
                    hasComment = false;
                }
                methodStatements.Add(ms);
            }
        }
        methodDeclaration = methodDeclaration.WithBody(SyntaxFactory.Block(methodStatements));

        var attributeSyntaxList = new List<AttributeSyntax>();
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
            methodDeclaration = methodDeclaration.WithAttributeLists(SyntaxFactory.SingletonList(attributeListSyntax));
        }

        return methodDeclaration;
    }
}