using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ConsoleWritelinePrintMethod : IPrintMethod
{
    public void PrintMethod(string message)
    {
        Console.WriteLine(message);
    }
}
