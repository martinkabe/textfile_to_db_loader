using System.Diagnostics;

public class DebugWritelinePrintMethod : IPrintMethod
{
    public void PrintMethod(string message)
    {
        Debug.WriteLine(message);
    }
}
