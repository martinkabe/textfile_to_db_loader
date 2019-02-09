using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PrintMethod
{
    private readonly IPrintMethod _iprintmethod;
    public PrintMethod(IPrintMethod iprintmethod)
    {
        _iprintmethod = iprintmethod;
    }
    public void ShowPrintMethod(string msg) => _iprintmethod.PrintMethod(msg);
}