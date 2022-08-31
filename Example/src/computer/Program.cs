using System;

public class DeviceProgram
{
    public const string DefaultHeader = "void __init(){__cinit();main();} ";
    public const string DefaultEntry = "__init";
    private string code;
    private string header;
    private string entry;
    public DeviceProgram(string code, string header = DefaultHeader, string entry = DefaultEntry)
    {
        this.code = code;
        this.header = header;
        this.entry = entry;
    }

    public string GetEntryPoint() => entry;
    public string GetCode() => code;
    public string GetHeader() => header;

    public string GetFullSource() => header + code;

    public string SetCode(string source) => code = source;
    public string SetHeader(string source) => header = source;
}
