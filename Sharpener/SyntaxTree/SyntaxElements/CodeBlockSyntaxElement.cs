using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sharpener.SyntaxTree.Scopes;


public class CodeBlockSyntaxElement: SyntaxElement, ISyntaxElementWithScope, IGenerateCodeBlock
{
    public int StartLine { get; set; }
    public int EndLine { get; set; }
    public string CodeBlock { get; set; }
    public BlockSyntax GenerateCodeNode()
    {
        if (String.IsNullOrEmpty(CodeBlock))
        {
            return SyntaxFactory.Block(SyntaxFactory.ParseStatement(String.Empty));
        }
        return SyntaxFactory.Block(SyntaxFactory.ParseStatement(CodeBlock));
    }
}