public class WriteToLogPrintMethod : IPrintMethod
{
    private string logName = string.Empty;

    /// <summary>
    /// Log file name is going to be "log.txt"
    /// </summary>
    public WriteToLogPrintMethod()
    {
    }
    /// <summary>
    /// Log file name is going to be what ever user write as logname parameter "*.txt"
    /// </summary>
    /// <param name="logname"></param>
    public WriteToLogPrintMethod(string logname)
    {
        this.logName = logname;
    }
    public void PrintMethod(string message)
    {
        new LogWriter(message, logName);
    }
}