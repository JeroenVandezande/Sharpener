using System.Runtime.Serialization;
using Newtonsoft.Json;
using Sharpener.Enums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpener.SyntaxTree.Scopes;


namespace Sharpener.SyntaxTree;

public class Document
{
    public String[] OriginalOxygeneCode { get; set; }
    public NameSpaceElement RootElement { get; set; }
    [JsonIgnore] 
    [IgnoreDataMember] 
    public Stack<SyntaxElement> Scopes { get; } = new Stack<SyntaxElement>();

    [JsonIgnore] 
    [IgnoreDataMember] 
    public SyntaxElement CurrentElement
    {
        get;
        set;
    }
    [JsonIgnore] 
    [IgnoreDataMember] 
    public SyntaxElement CurrentScope
    {
        get
        {
            if (Scopes.Count > 0)
            {
                return Scopes.Peek();
            }
            else
            {
                return null;
            }
        }
    }
    public VisibilityLevel LastKnownVisibilityLevel { get; set; }
    
    public bool LastKnownStatic { get; set; }
    public string LastKnownVariable { get; set; }
    
    public bool LastKnownInCodeBlock { get; set; }

    public SyntaxElement returnFromCurrentScope()
    {
        SyntaxElement result;
        result = Scopes.Pop();
        while (( Scopes.Count > 0) && Scopes.Peek() is ISyntaxElementAutoReturnsFromScope)
        {
            result = Scopes.Pop();
        }
        return result;
    }

    public void AddNewElementToCurrent(SyntaxElement element)
    {
        if (CurrentElement != null)
        {
            if (!(element is ISyntaxElementNotInTree))
            {
                var ps = Scopes.Peek();
                ps.Children.Add(element);
                element.Parent = ps;
            }
        }

        if (RootElement == null)
        {
            RootElement =(NameSpaceElement) element;
        }
        
    }
    
    public void AddNewElementToCurrentAndMakeCurrent(SyntaxElement element)
    {
        AddNewElementToCurrent(element);
        CurrentElement = element;
        if (element is ISyntaxElementWithScope)
        {
            Scopes.Push(element);
        }
    }

    private NamespaceDeclarationSyntax _currentNameSpaceSyntax;
    private ClassDeclarationSyntax _currentClassDeclarationSyntax;
    private InterfaceDeclarationSyntax _currentInterfaceDelDeclarationSyntax;
    
    private void RecurseThroughChildElements(List<ISyntaxElement> childElements)
    {
        foreach (var child in childElements)
        {
            if (child is ClassSyntaxElement expression)
            {
                _currentClassDeclarationSyntax = (ClassDeclarationSyntax) expression.GenerateCodeNode();
                _currentNameSpaceSyntax = _currentNameSpaceSyntax.AddMembers(_currentClassDeclarationSyntax);
            }

            if (child.Children.Count > 0)
            {
                RecurseThroughChildElements(child.Children);
            }
        }
    }

    public string CreateCSharpCode()
    {
        var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp9);
        // Create CompilationUnitSyntax
        var syntaxFactory = SyntaxFactory.CompilationUnit();
        

        foreach (var us in RootElement.Usings)
        {
            syntaxFactory = syntaxFactory.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(us)));
        }
       
        _currentNameSpaceSyntax = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(RootElement.NameSpace)).NormalizeWhitespace();

        RecurseThroughChildElements(RootElement.Children);

        syntaxFactory = syntaxFactory.AddMembers(_currentNameSpaceSyntax);


        // Add System using statement: (using System)
        //syntaxFactory = syntaxFactory.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")));

        // Create a namespace: (namespace CodeGenerationSample)
        //var @namespace = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("CodeGenerationSample")).NormalizeWhitespace();
        
        //  Create a class: (class Order)
        //var classDeclaration = SyntaxFactory.ClassDeclaration("Order");

        // Add the public modifier: (public class Order)
       // classDeclaration = classDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

        // Inherit BaseEntity<T> and implement IHaveIdentity: (public class Order : BaseEntity<T>, IHaveIdentity)
       // classDeclaration = classDeclaration.AddBaseListTypes(
            //SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("BaseEntity<Order>")),
            //SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("IHaveIdentity")));

        // Create a string variable: (bool canceled;)
        //var variableDeclaration = SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName("bool"))
        //    .AddVariables(SyntaxFactory.VariableDeclarator("canceled"));

        // Create a field declaration: (private bool canceled;)
        //var fieldDeclaration = SyntaxFactory.FieldDeclaration(variableDeclaration)
        //    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));

        // Create a Property: (public int Quantity { get; set; })
        //var propertyDeclaration = SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("int"), "Quantity")
         //   .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
         //   .AddAccessorListAccessors(
         //       SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
         //       SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));

        // Create a stament with the body of a method.
        //var syntax = SyntaxFactory.ParseStatement("canceled = true;");

        // Create a method
        //var methodDeclaration = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "MarkAsCanceled")
        //    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
        //    .WithBody(SyntaxFactory.Block(syntax));

        // Add the field, the property and method to the class.
        //classDeclaration = classDeclaration.AddMembers(fieldDeclaration, propertyDeclaration, methodDeclaration);

        // Add the class to the namespace.
        //@namespace = @namespace.AddMembers(classDeclaration);

        // Add the namespace to the compilation unit.
        //syntaxFactory = syntaxFactory.AddMembers(@namespace);

        // Normalize and get code as string.
        var code = syntaxFactory
            .NormalizeWhitespace()
            .ToFullString();

        // Output new code.
        return code;
    }
}