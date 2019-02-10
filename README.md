# csv_loader_lib.dll - .NET Framework dll for working with SQL Server
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

### Prerequisites

* Windows OS
* .NET Framework 4.5.1 or newer. How do I check it: [link](https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed/)

### Basic methods - description

**push_data**
* Pushing data into SQL Server.
* Table on SQL Server is automatically created if doesn't exist. 
* Data types are automatically estimated (functionality is able to recognize scientific format and convert to appropriate sql data type - int, float, decimal, ... It is also able to distinguish date, datetime format and datetime in ISO format).
```
push_data(connString, df, sqltabname, append = FALSE, showprogress = FALSE)
# If append == TRUE then appending new rows into existing SQL table. If append == FALSE then deletes rows in existing SQL table and appends new records.
```
