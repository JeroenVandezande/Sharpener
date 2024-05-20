namespace SimpleConsoleApp;

uses
  System.Linq;

type
  [AttributeHere]
  [SecondAttribute]
  Program = class
  public
    class method Main(): Int32;
    begin
      // add your own code here
      writeLn('The magic happens here.');
    end;
  end;
end.