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
* add 
<br /> <font color="blue"> **using**</font> <i>**PullPushDB.BasicOperations;**</i>
<br /> to C# Visual Studio project.

### Prerequisites
* Windows OS
* .NET Framework 4.5.1 or newer. How do I check it: [link](https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed/)

### Basic methods - description
*Create instance of BasicFunctions class:*
```
BasicFunctions bf = new BasicFunctions(new RegularConnString("LAPTOP-USERPC\\SQLEXPRESS", "DatabaseName", customparam: "Integrated Security=True", userid:null, password:null),
                                       new WriteToLogPrintMethod());
// and simply check connection string
string cs = bf.GetConnectionString();
```
First parameter in constructor is instance of RegularConnString class (this simplifies creation of SQL Server connection string), second parameter specifies output write line statements:
* new WriteToLogPrintMethod() writes all print statements into log file
```
Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
```
* new ConsoleWritelinePrintMethod() is standard Console.Writeline()
* new DebugWritelinePrintMethod() is standard Debug.Writeline() from System.Diagnostics class

**Push data**
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

**Pull data**
* Pulling data from table on SQL Server into DataTable object or directly to flat file.
```
/*
    Create simple table on SQL Server:

    CREATE TABLE [dbo].[Finance](
	    [Product] [int] NULL,
	    [Timestamp] [datetime] NULL,
	    [Price] [numeric](16, 4) NULL,
	    [n_price] [numeric](16, 1) NULL)

    Insert some values:

    INSERT INTO [dbo].[Finance] ([Product],[Timestamp],[Price],[n_price])
    VALUES
        (5678, '2008-01-01 12:00:00.000', 12.3400, 12.3),
	    (5678, '2008-01-01 12:01:00.000', NULL, 12.3),
	    (5678, '2008-01-01 12:02:00.000', NULL, 12.3),
	    (5678, '2008-01-01 12:03:00.000', 23.4500, 23.5),
	    (5678, '2008-01-01 12:04:00.000', NULL, 23.5),
	    (5678, '2008-01-01 13:04:00.000', NULL, NULL),
	    (5678, '2008-01-01 13:05:00.000', 30.3500, 30.4),
	    (5678, '2008-01-01 13:06:00.000', NULL, 30.4)
*/

// From SQL Server to DataTable
DataTable dt = bf.FromSQLToDataTable("select * from [dbo].[Finance]");

// Simple LINQ query for group by
var result = from row in dt.AsEnumerable()
            group row by row.Field<int>("Product") into grp
            select new
            {
                Product = grp.Key,
                ProductCount = grp.Count(),
                PriceSum = grp.Sum(r => r.Field<decimal?>("Price"))
            };

// Create DataTable object for IEnumerable<`a> result.
DataTable new_dt = new DataTable();
new_dt.Columns.Add("Product", typeof(string));
new_dt.Columns.Add("ProductCount", typeof(decimal));
new_dt.Columns.Add("PriceSum", typeof(decimal));

foreach (var item in result)
{
    new_dt.Rows.Add(item.Product, item.ProductCount, item.PriceSum);
}

// And push the data into table on SQL Server
// if dbo.FinanceGrouped table doesn't exist, create it. Data types are automatically estimated based on the data in DataTable object
if (bf.IfSQLTableExists("dbo.FinanceGrouped"))
{
    bf.InsertDataIntoSQLServerUsingSQLBulkCopy(new_dt, "dbo.FinanceGrouped");
}
else
{
    bf.CreateSQLTableBasedOnDataTable(new_dt, "dbo.FinanceGrouped");
    bf.InsertDataIntoSQLServerUsingSQLBulkCopy(new_dt, "dbo.FinanceGrouped");
}
```

**Execute stored procedure**
* Executing stored procedure on SQL Server.

```
/*
    Create simple table:

    CREATE TABLE [dbo].[BoolData](
	[name] [varchar](9) NULL,
	[married] [bit] NULL)

    Insert some records into:

    INSERT INTO [dbo].[BoolData] (name, married)
	 VALUES
	    ('Henryk', 0),
	    ('Tiral', 1),
	    ('Frederic', 1),
	    ('Anna', 0),
	    ('Michelle', 0),
	    ('Mark', 1),
	    ('Nicolas', 1),
	    ('Praveen',0)

    Create some stored procedures:

    CREATE PROCEDURE dbo.sp_SelectZerosFromBoolData
    AS
    select * from dbo.BoolData where married = 0
    GO

    CREATE PROCEDURE dbo.sp_DeleteZerosFromBoolData
    AS
    delete from dbo.BoolData where married = 0
    GO

    CREATE PROCEDURE dbo.sp_DeleteWithParams @name varchar(10), @married bit
    AS
    delete from dbo.BoolData where name = @name and married = @married
    GO
*/

// Call stored procedure without any parameters / values and return data in DataTable object
DataTable dt_storeProc = bf.CallSQLStoredProcedure("dbo.sp_SelectZerosFromBoolData", new ArrayList { }, new ArrayList { }, true);

// Call stored procedure without any parameters / values and do not return any data
bf.CallSQLStoredProcedure("dbo.sp_DeleteZerosFromBoolData", new ArrayList { }, new ArrayList { });

// Call stored procedure with parameters / values and do not return any data
bf.CallSQLStoredProcedure("dbo.sp_DeleteWithParams", new ArrayList { "@name", "@married" }, new ArrayList { "Mark", 1 });
```

**HandyExtensions class**
* <i>ToDataTable</i> extension converts IEnumerable or List type into DataTable. Allows null values.


**Another methods**
* Another methods such as void CleanUpTable(string sqlTableName),
* void DropTable(string sqlTableName),
* void SQLQueryTask(string sqltask),
* DataTable ExtractDataTypesFromSQLTable(string tabName),
* bool IfSQLTableExists(string tabname),
* Tuple<bool, string> IsServerConnected(),
* ...

## Author

* **Martin Kovarik**
