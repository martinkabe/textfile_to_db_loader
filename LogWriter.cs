using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

public class LogWriter
{
    private string m_exePath = string.Empty;
    private string logName = string.Empty;
    public LogWriter(string logMessage, string logName)
    {
        this.logName = logName;
        LogWrite(logMessage);
    }
    public void LogWrite(string logMessage)
    {
        m_exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string lgname = string.Empty;
        if (!string.IsNullOrEmpty(this.logName))
        {
            lgname = logName;
        }
        else
        {
            lgname = "log";
        }
        try
        {
            using (StreamWriter w = File.AppendText(m_exePath + "\\" + lgname + ".txt"))
            {
                Log(logMessage, w);
            }
        }
        catch (Exception ex)
        {
            throw new Exception("LogWriter class: " + ex.Message.ToString());
        }
    }

    public void Log(string logMessage, TextWriter txtWriter)
    {
        try
        {
            txtWriter.Write("\r\nLog Entry : ");
            txtWriter.Write("{0}", DateTime.Now.ToString("HH:mm:ss  MM/dd/yyyy"));
            txtWriter.Write(": [{0}]", logMessage);
        }
        catch (Exception ex)
        {
            throw new Exception("LogWriter class: " + ex.Message.ToString());
        }
    }
}
