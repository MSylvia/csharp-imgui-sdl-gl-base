
using System;
using CLanguage;
using CLanguage.Interpreter;
using CLanguage.Compiler;
using System.Linq;
using System.Collections.Generic;

public class Computer
{
    CInterpreter? interpreter;
    CCompiler? compiler;
    Executable? exe;
    public DeviceProgram? program;

    bool running = false;
    public bool IsRunning => running;
    // Called when the node enters the scene tree for the first time.
    public Computer()
    {
        string code = @"void main() {
            hello();
  for(int y=0; y < 128; y++) {
    for(int x=0; x < 128; x++) {
        pixel(x,y, x + y % 2);
    }
  }
}";
        program = new DeviceProgram(code);
        LoadProgram(program);

        SetupUI();
    }

    private void SetupUI()
    {
        // var edit = (GetNode("ComputerUI/TextEdit") as TextEdit);
        // if(edit == null)
        //     GD.Print("EDIT NULL");
        // edit.Text = program.GetCode();

        Console.WriteLine(program.GetHeader());
        Console.WriteLine(program.GetCode());
        Console.WriteLine(program.GetEntryPoint());
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public void Update()
    {
        if(running)
            Execute();
    }

    public void Execute()
    {
        interpreter?.Step(10_000_000);
        // interpreter?.Run();
    }

    public void Halt()
    {
        running = false;
    }

    public void Reset()
    {
        // var log = (GetNode("ComputerUI/TextEdit2") as TextEdit);
        // log.Text = "";

        interpreter?.Reset(program?.GetEntryPoint() ?? DeviceProgram.DefaultEntry);
        running = true;
    }

    public void LoadProgram(DeviceProgram program)
    {
        running = false;
        this.program = program;
        compiler = new CLanguage.Compiler.CCompiler(new SimpleMachineInfo(/*this*/), new Report( new SimpleComputerPrinter()));
        compiler.AddCode("main.c", program.GetFullSource());

        Compile();
    }

    public void Compile()
    {
        exe = compiler?.Compile();
        if (compiler?.Options.Report.Errors.Any (x => x.IsError) ?? false)
        {
            var m = string.Join ("\n", compiler?.Options.Report.Errors.Where (x => x.IsError));
            Console.WriteLine(m);
        }
        interpreter = new CLanguage.Interpreter.CInterpreter(exe);
    }
}

public class SimpleMachineInfo : MachineInfo
{
    // Node _node;
    public SimpleMachineInfo (/*Node node*/)
    {
        // _node = node;
        CharSize = 1;
        ShortIntSize = 2;
        IntSize = 2;
        LongIntSize = 4;
        LongLongIntSize = 8;
        FloatSize = 4;
        DoubleSize = 8;
        LongDoubleSize = 8;
        PointerSize = 2;
        HeaderCode = "";

        AddInternalFunction ("void hello ()", Hello);
        AddInternalFunction ("void print (const char *value)", Print);
        AddInternalFunction ("void pixel (int x, int y, unsigned char on)", Pixel);
    }

    void Hello (CLanguage.Interpreter.CInterpreter state)
    {
        Console.WriteLine("hello");
    }

    void Print (CLanguage.Interpreter.CInterpreter state)
    {
        var p = state.ReadArg (0).PointerValue;
        var s = state.ReadString (p);
        Console.WriteLine(s);

        // var log = (_node.GetNode("ComputerUI/TextEdit2") as TextEdit);
        // log.Text += s;
    }
    void Pixel (CLanguage.Interpreter.CInterpreter state)
    {
        var x = state.ReadArg (0).UInt8Value;
        var y = state.ReadArg (1).UInt8Value;
        var o = state.ReadArg (2).UInt8Value;

        // var screen = _node.GetNode<ComputerScreen>("Screen");
        // screen?.SetPixel(x,y,o);
    }
}

class SimpleComputerPrinter : Report.Printer
{
    int[] expectedErrors;
    List<int> encounteredErrors;

    public SimpleComputerPrinter (params int[] expectedErrors)
    {
        this.expectedErrors = expectedErrors;
        encounteredErrors = new List<int> ();
    }

    public override void Print (Report.AbstractMessage msg)
    {
        if (msg.MessageType != "Info")
        {
            encounteredErrors.Add (msg.Code);
            if (!expectedErrors.Contains (msg.Code))
            {
                Console.WriteLine(msg.ToString ());
            }
        }
    }

    public void CheckForErrors ()
    {
        foreach (var e in expectedErrors)
        {
            if (!encounteredErrors.Contains (e))
            {
                Console.WriteLine("Expected error " + e + " but never got it.");
            }
        }
    }
}