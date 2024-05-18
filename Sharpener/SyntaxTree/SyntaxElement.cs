using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Sharpener.Enums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sharpener.SyntaxTree;

public interface ISyntaxElement
{
    public List<ISyntaxElement> Children { get; set; }
    public ISyntaxElement Parent { get; set; }
    public string OriginalSourceCode { get; set; }
    public int OriginalSourceCodeStartLineNumber { get; set; }
    public int OriginalSourceCodeStartColumnNumber { get; set; }
    public int OriginalSourceCodeStopLineNumber { get; set; }
    public int OriginalSourceCodeStopColumnNumber { get; set; }
    public void AddParameter(string param, TokenType tokenType);
    public void FinishSyntaxElement(Document document);
    void SemicolonWasDetected();
}

public interface IGenerateMemberSyntax : ISyntaxElement
{
    public MemberDeclarationSyntax GenerateCodeNode();
}

public interface IGenerateCodeBlock : ISyntaxElement
{
    public BlockSyntax GenerateCodeNode();
}

public interface IGenerateExpressionSyntax : ISyntaxElement
{
    public ExpressionSyntax GenerateCode();
}

public interface IGenerateStatementSyntax : ISyntaxElement
{
    public StatementSyntax GenerateCodeNode();
}

public interface ISyntaxElementWithScope
{
}

public interface ISyntaxElementAutoReturnsFromScope
{
}

public interface ISyntaxElementNotInTree
{
}

public abstract class SyntaxElement: ISyntaxElement
{
    public List<ISyntaxElement> Children { get; set; } = new List<ISyntaxElement>();
    [JsonIgnore] 
    [IgnoreDataMember] 
    public ISyntaxElement Parent { get; set; }

    public string OriginalSourceCode { get; set; }
    public int OriginalSourceCodeStartLineNumber { get; set; }
    public int OriginalSourceCodeStartColumnNumber { get; set; }
    public int OriginalSourceCodeStopLineNumber { get; set; }
    public int OriginalSourceCodeStopColumnNumber { get; set; }

    public virtual void AddParameter(string param, TokenType tokenType)
    {
    }

    public virtual void FinishSyntaxElement(Document document)
    {
        var subset = document.OriginalOxygeneCode.Skip(OriginalSourceCodeStartLineNumber - 1).Take(OriginalSourceCodeStopLineNumber - OriginalSourceCodeStartLineNumber + 1);
        OriginalSourceCode = String.Join(Environment.NewLine, subset);
    }

    public virtual void SemicolonWasDetected()
    {
    }

    public SyntaxElement WithStartSourceCodePosition(int startLineNumber, int startColumnNumber)
    {
        OriginalSourceCodeStartLineNumber = startLineNumber;
        OriginalSourceCodeStartColumnNumber = startColumnNumber;
        return this;
    }
}

public class NameSpaceElement : SyntaxElement, ISyntaxElementWithScope
{
    public List<String> Usings { get; set; } = new List<string>(){"System"};
    public String NameSpace { get; set; }
    
    public bool ElementIsFinished { get; set; }
    public override void AddParameter(string param, TokenType tokenType)
    {
        if (String.IsNullOrEmpty(NameSpace))
        {
            NameSpace = param;
        }
        else
        {
            if (ElementIsFinished)
            {
                return;
            }
            if (!Usings.Contains(param))
            {
                Usings.Add(param);
            }
        }
    }
}
