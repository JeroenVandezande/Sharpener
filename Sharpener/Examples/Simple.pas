namespace SimpleConsoleApp;

uses
  System.Linq;

type
  [AttributeHere]
  [SecondAttribute]
  Program = class
  public
    property WorkList: TWorklist read TWorklist(UsedWorklist) write UsedWorklist;
    property UsedWorklist: IWorklist; implements ITest.Worklist;
  
    class method Main(): Int32;
    begin
      // add your own code here
      writeLn('The magic happens here.');
    end;
	// Implicit Const
	  const STRING_CONST = "MyConstantString";
	  const INT_CONST = 80;
	  const DOUBLE_CONST = 155.0;
	  const e = 1e-6;
	  
	  // Explicit Consts
	  const EXPLICITINT_CONST: Integer = 65536;
	  const EXPLICITOBJECT_CONST = System.Drawing.Color.FromArgb(0, 0, 50, 100);
  end;
  
  
  
end.