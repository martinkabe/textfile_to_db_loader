using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class RegularConnString
{
    private readonly string cs;

    /// <summary>
    /// Constructor of RegularConnString class simplifies creation of SQL Server connection string. 
    /// </summary>
    /// <param name="datasource">E.g. myServerAddress</param>
    /// <param name="initialcatalogue">E.g. myDataBase</param>
    /// <param name="customparam">Could be e.g. Integrated Security=SSPI or Trusted_Connection=True or Asynchronous Processing=True</param>
    /// <param name="userid">E.g. myDomain\myUsername</param>
    /// <param name="password">E.g. myPassword</param>
    public RegularConnString(string datasource = "",
                                    string initialcatalogue = "",
                                    string customparam = "",
                                    string userid = "",
                                    string password = "")
    {
        this.cs = PrepareConnString(datasource, initialcatalogue, customparam, userid, password);
    }

    public string GetConnString() => cs;
    private string PrepareConnString(string datasource = "",
                                    string initialcatalogue = "",
                                    string customparam = "",
                                    string userid = "",
                                    string password = "")
    {
        if (!String.IsNullOrEmpty(datasource))
            datasource = "Data Source=" + datasource;
        if (!String.IsNullOrEmpty(initialcatalogue))
            initialcatalogue = "Initial Catalog=" + initialcatalogue;
        if (!String.IsNullOrEmpty(userid))
            userid = "User ID=" + userid;
        if (!String.IsNullOrEmpty(password))
            password = "Password" + password;

        var array = new[] { datasource, initialcatalogue, customparam, userid, password };
        string paramsconn = string.Join(";", array.Where(s => !string.IsNullOrEmpty(s)));
        return paramsconn;
    }
}
