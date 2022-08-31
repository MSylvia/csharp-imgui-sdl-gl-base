using System;

public class ComputerUI
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    Computer? computer;
    // TextEdit? edit;
    // TextEdit? log;
    // Called when the node enters the scene tree for the first time.
    public void _Ready()
    {
        // computer = GetParent<Computer>();
        // edit = (GetNode("TextEdit") as TextEdit);
        // log = (GetNode("TextEdit2") as TextEdit);
    }

    public void _Process(float delta)
    {
        // (GetNode("Status") as CheckBox).Pressed = computer.IsRunning;
    }

    public void _on_TextEdit_focus_exited()
    {
        // compiler.c
        // computer.program.SetCode(edit.Text);
        Console.WriteLine(computer.program.GetHeader());
        Console.WriteLine(computer.program.GetCode());
        Console.WriteLine(computer.program.GetEntryPoint());
    }

    public void _on_Reset_pressed()
    {
        computer.Reset();
        // interpreter?.Run();
    }

    public void _on_Load_pressed()
    {
        // computer.program.SetCode(edit.Text);
        computer.LoadProgram(computer.program);
    }

    public void _on_Halt_pressed()
    {
        computer.Halt();
    }

    // int maxLines = 20;
    // public void AddLine(string msg)
    // {
    //     log.Text = msg;
    // }
}
