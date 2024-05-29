namespace ConsoleApp;

interface

uses
  System;

type
  // Define custom EventArgs to pass greeting message
  GreetingChangedEventArgs = public class(EventArgs)
  public
    property GreetingMessage: String;
    constructor(aGreetingMessage: String);
  end;

  // Define an interface with a property, event, and method
  IGreet = public interface
    property Name: String read write;
    event GreetingChanged: EventHandler<GreetingChangedEventArgs>;
    method Greet: String;
  end;

  // Implement the interface in a class
  Greeter = public class(IGreet)
  private
    fName: String;
  protected
    method OnGreetingChanged;
  public
    property Name: String read fName write SetName;
    event GreetingChanged: EventHandler<GreetingChangedEventArgs>;
    method Greet: String;
  private
    method SetName(aName: String);
  end;

  // Main program class
  Program = public static class
  public
    class method Main(args: array of String);
  end;

implementation

constructor GreetingChangedEventArgs(aGreetingMessage: String);
begin
  GreetingMessage := aGreetingMessage;
end;

method Greeter.SetName(aName: String);
begin
  if fName <> aName then begin
    fName := aName;
    OnGreetingChanged;
  end;
end;

method Greeter.OnGreetingChanged;
begin
  if assigned(GreetingChanged) then
    GreetingChanged(self, new GreetingChangedEventArgs(Greet));
end;

method Greeter.Greet: String;
begin
  exit 'Hello, ' + Name + '!';
end;

class method Program.Main(args: array of String);
begin
  var greeter: IGreet := new Greeter;
  greeter.Name := 'World';
  greeter.GreetingChanged += method(sender: Object; e: GreetingChangedEventArgs) begin
    Console.WriteLine('Greeting changed to: ' + e.GreetingMessage);
  end;

  Console.WriteLine(greeter.Greet);
  greeter.Name := 'Sharpener';
  Console.WriteLine(greeter.Greet);
  Console.ReadLine;
end;

end.
