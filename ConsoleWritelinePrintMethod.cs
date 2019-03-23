using System;

public class ConsoleWritelinePrintMethod : IPrintMethod
{
    public void PrintMethod(string message)
    {
        Console.WriteLine(message);
    }
}
