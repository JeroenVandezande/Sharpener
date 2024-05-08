namespace TestLibrary;

// Class without interface and implementation sections
// Used as an example Oxygene language class to demonstrate as many language features as possible

uses
  System.Collections.Generic,
  System.Linq;

type
  // Internal class
  SimpleClass = assembly class 
  public
    property SimpleString: String;
    // Implied properties
    SomeInt: Int32 := 5;
    SomeDouble: Double := 5.5;
    method InternalSimpleMethod();
    begin
      Console.WriteLine(SomeDouble);
    end;
  end;

  TestClassName = public class
  private
    // Static private property
    class _staticPrivate: Int32 := 15;
    // Private properties
    property _testDoubleLiteral := 9.9;
    property _testIntLiteral := 9;
  protected
    property protectedString: String;
  public
    // Constructor
    constructor();
    begin
      _testDoubleLiteral := 9.9;
    end;
    
    // Constructor overload with optional parameter
    constructor(aInputParamOne: Double; aOptionalParam: Boolean := false);
    begin
      _testDoubleLiteral := aInputParamOne;
    end;
    
    // Empty method
    method EmptyMethod(); empty;

    // Static Main Method
    class method Main(): Int32;
    begin
      // String Literal
      Console.WriteLine('Some Classic Code');
      // Alternate String Literal
      Console.WriteLine("Some More Code");
      // Character Tab
      Console.WriteLine(#9);
      Console.WriteLine(_staticPrivate);

      // Create class
      var internalObj := new SimpleClass();

      // Create class with properties
      var internalObjInitialize := new SimpleClass(
        SimpleString := "SimpleStringTest", 
        SomeInt := 8);
    end;

    method TestMethod(aSomeParam: String);
    begin
      // Method in a method
      method DoSomethingInsideAMethod(): Boolean;
      begin
        result := false;
        result := true;
      end;

      // If Else
      if DoSomethingInsideAMethod then
      begin
        exit;
      end
      else 
      begin
        Console.WriteLine("DoSomething");
      end;
    end;

    method TestMethod2(aSomeClassParam: TestClassName);
    begin
      // Null Class
      var Myclass: TestClassName;

      // Set Property to Null
      aSomeClassParam.protectedString := nil;

      // If Else
      if String.IsNullOrEmpty(aSomeClassParam.protectedString) then
      begin
        exit;
      end
      else if assigned(Myclass) then
      begin
        Console.WriteLine("DoSomething2");
      end
      else
      begin
        Console.WriteLine("DoSomethingElse");
      end;
    end;

    // Switch Case method
    method SwitchCaseMethod(aMyEnum: MyEnum): String;
    begin
      case aMyEnum of
        MyEnum.Hello: exit "Hello";
        MyEnum.Bonjour: result := "Bonjour";
        MyEnum.Hola: exit "Hola";
        else
          result := "Huh?";
      end;
    end;

    // Private backed property
    property TestDoubleLiteral: Double read _testDoubleLiteral write _testDoubleLiteral;

  end;

  MyEnum = public enum(Hello, Bonjour, Hola);

  MyTypedEnum = public enum(Hello, Bonjour, Hola) of UInt16;

end.