using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

public class DebugWritelinePrintMethod : IPrintMethod
{
    public void PrintMethod(string message)
    {
        Debug.WriteLine(message);
    }
}
