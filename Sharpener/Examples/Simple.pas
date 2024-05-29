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
  end;
end.