using System.Runtime.Serialization;
using Newtonsoft.Json;
using Sharpener.Enums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpener.SyntaxTree.Scopes;


namespace Sharpener.SyntaxTree;

public enum ContainingTypeElement {None, Class, Interface, Record}

public class Document
{
    [JsonIgnore]
    [IgnoreDataMember]
    public VisibilityLevel LastKnownVisibilityLevel { get; set; }
    [JsonIgnore]
    [IgnoreDataMember]
    public bool LastKnownStatic { get; set; }
    [JsonIgnore]
    [IgnoreDataMember]
    public string? LastKnownVariable { get; set; }
    [JsonIgnore]
    [IgnoreDataMember]
    public bool LastKnownInCodeBlock { get; set; }
    [JsonIgnore]
    [IgnoreDataMember]
    public bool IsInTypePartOfFile { get; set; }
    [JsonIgnore]
    [IgnoreDataMember]
    public bool IsNotifyKeywordUsedInFile { get; set; }
    [JsonIgnore]
    [IgnoreDataMember]
    public bool IsInInterfacePartOfFile { get; set; }
    [JsonIgnore]
    [IgnoreDataMember]
    public bool IsInImplementationPartOfFile { get; set; }
    [JsonIgnore]
    [IgnoreDataMember]
    public ContainingTypeElement CurrentContainingTypeElement { get; set; }
    [JsonIgnore]
    [IgnoreDataMember]
    public string[]? OriginalOxygeneCode { get; set; }
    
    [JsonIgnore]
    [IgnoreDataMember]
    public SyntaxElement? PreviousElement { get; set; }
    public NameSpaceElement? RootElement { get; set; }

    [JsonIgnore]
    [IgnoreDataMember]
    public List<IAttributeElement> AttributeCache { get; set; } = new List<IAttributeElement>();

    [JsonIgnore]
    [IgnoreDataMember]
    public Stack<SyntaxElement> Scopes { get; } = new Stack<SyntaxElement>();

    [JsonIgnore]
    [IgnoreDataMember]
    public SyntaxElement CurrentElement { get; set; }

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

    public ClassSyntaxElement _FindClassByName(string className, ISyntaxElement parent)
    {
        if (parent is ClassSyntaxElement cse)
        {
            if (cse.ClassName.ToLower() == className.ToLower())
            {
                return cse;
            }
        }

        foreach (var c in parent.Children)
        {
            var found = _FindClassByName(className, c);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    public ClassSyntaxElement FindClassByName(string className)
    {
        if (String.IsNullOrEmpty(className))
        {
            return null;
        }

        return _FindClassByName(className, RootElement);
    }
    public SyntaxElement returnFromCurrentScope()
    {
        SyntaxElement result;
        result = Scopes.Pop();
        while ((Scopes.Count > 0) && Scopes.Peek() is ISyntaxElementAutoReturnsFromScope)
        {
            result = Scopes.Pop();
        }

        PreviousElement = CurrentElement;
        CurrentElement = Scopes.Peek();
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
            RootElement = (NameSpaceElement)element;
        }
    }

    public void AddNewElementToCurrentAndMakeCurrent(SyntaxElement element)
    {
        if (CurrentElement is MethodElement)
        {
            if (element is MethodElement)
            {
                returnFromCurrentScope();
            }
        }

        AddNewElementToCurrent(element);
        PreviousElement = CurrentElement;
        CurrentElement = element;

        if (element is AttributeSyntaxElement attributeElement)
        {
            AttributeCache.Add(attributeElement);
        }
        else
        {
            if (AttributeCache.Count > 0)
            {
                CurrentElement.Attributes = new List<IAttributeElement>(AttributeCache);
                AttributeCache.Clear();
            }
        }

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
                _currentClassDeclarationSyntax = (ClassDeclarationSyntax)expression.GenerateCodeNodes()[0];
                _currentNameSpaceSyntax = _currentNameSpaceSyntax.AddMembers(_currentClassDeclarationSyntax);
            }

            if (child is EnumSyntaxElement enumExpression)
            {
                var enumDeclarationSyntax = (EnumDeclarationSyntax)enumExpression.GenerateCodeNodes()[0];
                _currentNameSpaceSyntax = _currentNameSpaceSyntax.AddMembers(enumDeclarationSyntax);
            }

            if (child.Children.Count > 0)
            {
                RecurseThroughChildElements(child.Children);
            }
        }
    }

    public string CreateCSharpCode()
    {
        //var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp9);
        // Create CompilationUnitSyntax
        var syntaxFactory = SyntaxFactory.CompilationUnit();


        foreach (var us in RootElement.Usings)
        {
            syntaxFactory = syntaxFactory.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(us)));
        }

        _currentNameSpaceSyntax = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(RootElement.NameSpace))
            .NormalizeWhitespace();

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