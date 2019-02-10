# csv_loader_lib.dll - .NET Framework dll for working with MS SQL Server
SQL Server operations from C#

**Usage is:**
* pulling data from SQL Server
* pushing data into SQL Server
* calling stored procedures on SQL Server
* extracting data types from table on SQL Server
* creating, deleting table on SQL Server

## Getting Started
*Adding library to C# Visual Studio project:*
* go to [csv_loader_lib.dll](https://github.com/martinkabe/textfile_to_db_loader/blob/master/csv_loader_lib.dll) -> Download -> right click on "References" in your C# Visual Studio project -> Add Reference... -> Browse -> select newly downloaded csv_loader_lib.dll -> OK.
* add **using PullPushDB.BasicOperations;** to C# Visual Studio project.

### Prerequisites
* Windows OS
* .NET Framework 4.5.1 or newer. How do I check it: [link](https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed/)

### Basic methods - description
*Create instance of BasicFunctions class:*
```
BasicFunctions bf = new BasicFunctions("Data Source=LAPTOP-SERVERNAME\\SQLEXPRESS;Initial Catalog=DatabaseName;Integrated Security=True;",
                                                    new WriteToLogPrintMethod());
```
First parameter in constructor is connection string, second parameter specifies output write line statements:
* new WriteToLogPrintMethod() writes into log file
```
Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
```
* new ConsoleWritelinePrintMethod() is standard Console.Writeline()
* new DebugWritelinePrintMethod() is standard Debug.Writeline() from System.Diagnostics class

**push_data**
* Pushing data from text file or DataTable object into SQL Server.
```
// Push the data from text file into table on SQL Server
bf.PushFlatFileToDB("c:\\Users\\Username\\Downloads\\test.csv", "dbo.TestTable")

// Push the data from DataTable object into table on SQL Server
DataTable dt = new DataTable();
dt.Clear();
dt.Columns.Add("Name");
dt.Columns.Add("Marks");
dt.Rows.Add(new object[] { "Ravi", 500 });
dt.Rows.Add(new object[] { "Ilhan", 1000 });
dt.Rows.Add(new object[] { "Francesco", 100 });
dt.Rows.Add(new object[] { "Oskar", 900 });
dt.Rows.Add(new object[] { "Peter", 300.87 });

// if dbo.TestTable table doesn't exist, create it. Data types are automatically estimated based on the data in DataTable object
if (bf.IfSQLTableExists("dbo.TestTableRavi"))
{
    bf.InsertDataIntoSQLServerUsingSQLBulkCopy(dt, "dbo.TestTable");
}
else
{
    bf.CreateSQLTableBasedOnDataTable(dt, "dbo.TestTable");
    bf.InsertDataIntoSQLServerUsingSQLBulkCopy(dt, "dbo.TestTable");
}
```
