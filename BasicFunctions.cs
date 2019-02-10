using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PullPushDB
{
    namespace BasicOperations
    {
        public class BasicFunctions
        {
            private readonly IPrintMethod _iprintmethod;
            private string _connectionString;
            /// <summary>
            /// Constructor for BasicOperations.BasicFunctions class.
            /// </summary>
            /// <param name="conString">Connection string, e.g. LAPTOP-USERNAME\\SQLEXPRESS;Initial Catalog=DBName;Integrated Security=True;</param>
            /// <param name="iprintmethod">Instance of class, e.g.: new ConsoleWritelinePrintMethod() uses Console.Writeline() method.</param>
            public BasicFunctions(string conString, IPrintMethod iprintmethod)
            {
                _connectionString = conString;
                _iprintmethod = iprintmethod;
            }

            /// <summary>
            /// Wrapper for IPrintMethod interface, e.g.: new ConsoleWritelinePrintMethod() in ctor represents Console.Writeline() method.
            /// </summary>
            /// <param name="message">String message.</param>
            public void ShowPrintMethod(string message) => _iprintmethod.PrintMethod(message);

            /// <summary>
            /// Pushes the data into SQL Server DB using SqlBulkCopy class.
            /// </summary>
            /// <param name="dtable">DataTable as input parameter.</param>
            /// <param name="sqlTableName">Name of table on SQL Server database.</param>
            /// <param name="batchSize">The size of batch in BulkCopy, default 100000 records.</param>
            /// <param name="timeout">BulkCopy timeout.</param>
            /// <param name="notifyAfter">BulkCopy timeout.</param>
            public void InsertDataIntoSQLServerUsingSQLBulkCopy(DataTable dtable,
                                                                string sqlTableName,
                                                                Int32 batchSize = 100000,
                                                                int timeout = 0,
                                                                bool notifyAfter = false)
            {
                try
                {
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(this._connectionString, System.Data.SqlClient.SqlBulkCopyOptions.TableLock))
                    {
                        bulkCopy.DestinationTableName = sqlTableName;

                        try
                        {
                            // Write from the source to the destination.
                            bulkCopy.BulkCopyTimeout = timeout;
                            bulkCopy.BatchSize = batchSize;
                            // Set up the event handler to notify after 50 rows.
                            // bulkCopy.SqlRowsCopied += new SqlRowsCopiedEventHandler(OnSqlRowsCopied);
                            if (notifyAfter)
                            {
                                bulkCopy.NotifyAfter = batchSize;

                                bulkCopy.SqlRowsCopied += (sender, args) =>
                                {
                                    Console.WriteLine("Copied {0} records so far ...", args.RowsCopied);
                                };
                            }
                            bulkCopy.WriteToServer(dtable);
                        }
                        catch (SqlException ex)
                        {
                            if (ex.Message.Contains("Received an invalid column length from the bcp client for colid"))
                            {
                                string pattern = @"\d+";
                                Match match = Regex.Match(ex.Message.ToString(), pattern);
                                var index = Convert.ToInt32(match.Value) - 1;

                                FieldInfo fi = typeof(SqlBulkCopy).GetField("_sortedColumnMappings", BindingFlags.NonPublic | BindingFlags.Instance);
                                var sortedColumns = fi.GetValue(bulkCopy);
                                var items = (Object[])sortedColumns.GetType().GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sortedColumns);

                                FieldInfo itemdata = items[index].GetType().GetField("_metadata", BindingFlags.NonPublic | BindingFlags.Instance);
                                var metadata = itemdata.GetValue(items[index]);

                                var column = metadata.GetType().GetField("column", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(metadata);
                                var length = metadata.GetType().GetField("length", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(metadata);

                                ShowPrintMethod("Error message: Column [" + column + "] contains data with a length greater than " + length);
                                ShowPrintMethod("\n");
                                ShowPrintMethod("Table " + sqlTableName + " already exists in DB, just change data type - see the tip below.");
                                ShowPrintMethod("Tip: try something like ALTER TABLE table_name ALTER COLUMN column_name datatype;");
                                // CleanUpTable(sqlTableName, connString);
                                Environment.Exit(1);
                            }
                            else
                            {
                                ShowPrintMethod(ex.Message.ToString());
                                // CleanUpTable(sqlTableName, connString);
                                Environment.Exit(1);
                            }
                        }
                        catch (Exception e)
                        {
                            ShowPrintMethod(e.Message.ToString());
                            // CleanUpTable(sqlTableName, connString);
                            Environment.Exit(1);
                        }
                    }
                }
                catch (Exception e)
                {
                    ShowPrintMethod(e.Message.ToString());
                    Environment.Exit(1);
                }
            }
            /// <summary>
            /// TRUNCATE table on SQL Server. If TRUNCATE is not allowed, catch exception uses DROP.
            /// </summary>
            /// <param name="sqlTableName">Name of table on SQL Server database.</param>
            public void CleanUpTable(string sqlTableName)
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(this._connectionString))
                    {
                        con.Open();
                        string deleteRowsInTable = @"IF OBJECT_ID(" + "'" + sqlTableName + "','U')" +
                                                      " IS NOT NULL TRUNCATE TABLE " + sqlTableName + ";";
                        using (SqlCommand command = new SqlCommand(deleteRowsInTable, con))
                        {
                            command.CommandTimeout = 0;
                            command.ExecuteNonQuery();
                        }
                        con.Close();
                    }
                }
                catch (SqlException sqlex)
                {
                    ShowPrintMethod("Truncate command cannot be used because of insufficient permissions: " + sqlex.Message.ToString());
                    ShowPrintMethod("DELETE FROM tabName is used instead.");
                    using (SqlConnection con = new SqlConnection(this._connectionString))
                    {
                        con.Open();
                        string deleteRowsInTable = @"IF OBJECT_ID(" + "'" + sqlTableName + "','U')" +
                                                      " IS NOT NULL DELETE FROM " + sqlTableName + ";";
                        using (SqlCommand command = new SqlCommand(deleteRowsInTable, con))
                        {
                            command.CommandTimeout = 0;
                            command.ExecuteNonQuery();
                        }
                        con.Close();
                    }
                }
                catch (Exception ex)
                {
                    ShowPrintMethod(ex.Message.ToString());
                    Environment.Exit(1);
                }
            }
            /// <summary>
            /// DROP table uses SQL command DROP table.
            /// </summary>
            /// <param name="sqlTableName">Name of table on SQL Server database.</param>
            public void DropTable(string sqlTableName)
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(this._connectionString))
                    {
                        con.Open();
                        string deleteRowsInTable = @"IF OBJECT_ID(" + "'" + sqlTableName + "','U')" +
                                                      " IS NOT NULL DROP TABLE " + sqlTableName + ";";
                        using (SqlCommand command = new SqlCommand(deleteRowsInTable, con))
                        {
                            command.CommandTimeout = 0;
                            command.ExecuteNonQuery();
                        }
                        con.Close();
                    }
                }
                catch (Exception ex)
                {
                    ShowPrintMethod(ex.Message.ToString());
                    Environment.Exit(1);
                }
            }
            /// <summary>
            /// POST SQL QUERY into DB.
            /// </summary>
            /// <param name="sqltask">SQL Query, e.g. UPDATE table.</param>
            public void SQLQueryTask(string sqltask)
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(this._connectionString))
                    {
                        con.Open();
                        using (SqlCommand command = new SqlCommand(sqltask, con))
                        {
                            command.CommandTimeout = 0;
                            command.ExecuteNonQuery();
                        }
                        con.Close();
                        ShowPrintMethod("Query has been completed!");
                    }
                }
                catch (Exception ex)
                {
                    ShowPrintMethod(ex.Message.ToString());
                    Environment.Exit(1);
                }
            }
            /// <summary>
            /// Method pushes text file into SQL Server DB.
            /// </summary>
            /// <param name="strFilePath">Path to file, e.g.: c:\\Users\\Username\\Downloads\\test.csv.</param>
            /// <param name="tabName">Table name on SQL Server DB.</param>
            /// <param name="sepType">The type of text separator. Default is SepType.auto (detects separator from text file automatically).</param>
            /// <param name="flushed_batch_size">Batch size defined in BulkCopy.</param>
            /// <param name="sqlBulkCopyTimeOut">Time out for sqlBulkCopy.</param>
            /// <param name="showprogress">Show how many records has already been pushed into table (based on flushed_batch_size parameter).</param>
            /// <param name="removeTabBeforePushData">Delete records in the table before pushing new one.</param>
            /// <param name="createTableIfNotExist">Create table if doesn't exist.</param>
            /// <param name="rowstoestimatedt">How many rows are used to estimate data types for newly created table on SQL Server DB.</param>
            public void PushFlatFileToDB(string strFilePath,
                                         string tabName,
                                         SepType sepType = SepType.auto,
                                         Int32 flushed_batch_size = 100000,
                                         int sqlBulkCopyTimeOut = 0,
                                         bool showprogress = false,
                                         bool removeTabBeforePushData = false,
                                         bool createTableIfNotExist = false,
                                         int rowstoestimatedt = 250000)
            {
                DataTable dt = new DataTable();
                DataTable dataTypes = new DataTable();
                Int64 rowsCount = 0;
                char sep = AutoMethods.DetectSeparator(strFilePath, sepType);

                try
                {
                    if (IfSQLTableExists(tabName))
                    {
                        dataTypes = ExtractDataTypesFromSQLTable(tabName);
                    }
                    else
                    {
                        // table doesn't exist
                        if (createTableIfNotExist)
                        {
                            CreateSQLTableBasedOnTextFile(strFilePath, tabName, rowstoestimatedt, sepType);
                            dataTypes = ExtractDataTypesFromSQLTable(tabName);
                        }
                        else
                        {
                            ShowPrintMethod("Table " + tabName + " doesn't exist");
                            Environment.Exit(1);
                        }   
                    }
                    
                    int dt_rows_count = dataTypes.Rows.Count;

                    using (StreamReader sr = new StreamReader(strFilePath))
                    {
                        string[] headers = sr.ReadLine().Split(sep);

                        if (headers.Length != dt_rows_count)
                        {
                            ShowPrintMethod("CSV file has different count of columns than table " + tabName + "!");
                            Environment.Exit(1);
                        }

                        // Compare header - CSV vs DataTable
                        for (int i = 0; i < dt_rows_count; i++)
                        {
                            DataRow drh = dataTypes.Rows[i];
                            //if (!headers[i].ToString().Contains(drh.ItemArray[0].ToString()))
                            if (headers[i].ToString().Replace("\"", "").ToLower() != drh.ItemArray[0].ToString().ToLower())
                            {
                                ShowPrintMethod("You need to reorder columns in your csv based to columns in " + tabName + " table!");
                                ShowPrintMethod("Column " + headers[i].ToString().Replace("\"", "") + " in your data.table or data.frame\ndoesn't correspond with column " + drh.ItemArray[0].ToString() + " defined in " + tabName + " table");
                                Environment.Exit(1);
                            }
                        }

                        if (removeTabBeforePushData)
                        {
                            ShowPrintMethod("Cleaning table " + tabName);
                            CleanUpTable(tabName);
                            ShowPrintMethod("Table " + tabName + " has been cleaned");
                        }

                        for (int i = 0; i < dt_rows_count; i++)
                        {
                            DataRow dr = dataTypes.Rows[i];
                            // entire logic should goes here:
                            if (dr.ItemArray[1].ToString() == "float") { dt.Columns.Add(dr.ItemArray[0].ToString(), typeof(double)); }
                            else if (dr.ItemArray[1].ToString() == "real") { dt.Columns.Add(dr.ItemArray[0].ToString(), typeof(Single)); }
                            else if (dr.ItemArray[1].ToString() == "smallint") { dt.Columns.Add(dr.ItemArray[0].ToString(), typeof(Int16)); }
                            else if (dr.ItemArray[1].ToString() == "int") { dt.Columns.Add(dr.ItemArray[0].ToString(), typeof(Int32)); }
                            else if (dr.ItemArray[1].ToString() == "bigint") { dt.Columns.Add(dr.ItemArray[0].ToString(), typeof(Int64)); }
                            else if (dr.ItemArray[1].ToString() == "bit") { dt.Columns.Add(dr.ItemArray[0].ToString(), typeof(Boolean)); }
                            else if (dr.ItemArray[1].ToString() == "decimal" || dr.ItemArray[1].ToString() == "numeric") { dt.Columns.Add(dr.ItemArray[0].ToString(), typeof(decimal)); }
                            else if (dr.ItemArray[1].ToString() == "uniqueidentifier") { dt.Columns.Add(dr.ItemArray[0].ToString(), typeof(Guid)); }
                            else { dt.Columns.Add(dr.ItemArray[0].ToString(), typeof(string)); }
                        }

                        Int64 batchsize = 0;

                        while (!sr.EndOfStream)
                        {
                            string[] rows = sr.ReadLine().Split(sep);

                            for (int i = 0; i < rows.Length; i++)
                            {
                                DataRow dtr = dataTypes.Rows[i];

                                if (rows[i] == "NA" || string.IsNullOrWhiteSpace(rows[i]))
                                {
                                    rows[i] = null;
                                }
                                else
                                {
                                    if (dtr.ItemArray[1].ToString() == "bigint") { rows[i] = Int64.Parse(rows[i], NumberStyles.Any).ToString(); }
                                    else if (dtr.ItemArray[1].ToString() == "smallint") { rows[i] = Int16.Parse(rows[i], NumberStyles.Any).ToString(); }
                                    else if (dtr.ItemArray[1].ToString() == "int") { rows[i] = Int32.Parse(rows[i], NumberStyles.Any).ToString(); }
                                    else if (dtr.ItemArray[1].ToString() == "datetime") { rows[i] = DateTime.Parse(rows[i], null, DateTimeStyles.RoundtripKind).ToString(); }
                                    else if (dtr.ItemArray[1].ToString() == "float") { rows[i] = double.Parse(rows[i], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture).ToString(); }
                                    else if (dtr.ItemArray[1].ToString() == "bit")
                                    {
                                        Boolean.TryParse(StringExtensions.ToBoolean(rows[i]).ToString(), out bool parsedValue);
                                        rows[i] = parsedValue.ToString();
                                    }
                                    else if (dtr.ItemArray[1].ToString() == "real") { rows[i] = Single.Parse(rows[i], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture).ToString(); }
                                    else if (dtr.ItemArray[1].ToString() == "decimal" || dtr.ItemArray[1].ToString() == "numeric") { rows[i] = Decimal.Parse(rows[i], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture).ToString(); }
                                    else { rows[i] = rows[i].ToString().Replace("\"", ""); }
                                }
                            }

                            dt.Rows.Add(rows);
                            batchsize += 1;

                            if (batchsize == flushed_batch_size)
                            {
                                InsertDataIntoSQLServerUsingSQLBulkCopy(dt, tabName, flushed_batch_size, sqlBulkCopyTimeOut);
                                dt.Rows.Clear();
                                batchsize = 0;
                                if (showprogress) { ShowPrintMethod("Flushing " + flushed_batch_size + " rows (" + (rowsCount + 1) + " records already imported)"); }
                            }
                            rowsCount += 1;
                            // rowCounter++;
                        }
                        InsertDataIntoSQLServerUsingSQLBulkCopy(dt, tabName, flushed_batch_size, sqlBulkCopyTimeOut);
                        dt.Rows.Clear();
                    }
                    ShowPrintMethod(rowsCount + " records imported");
                }
                catch (FormatException fex)
                {
                    ShowPrintMethod(fex.Message.ToString());
                    ShowPrintMethod("Tip: there might be string between numeric data or the most likely escape character in string.\r\nCheck also scientific notation considered as string.");
                    Environment.Exit(1);
                }
                catch (Exception ex)
                {
                    ShowPrintMethod(ex.Message.ToString());
                }
            }
            /// <summary>
            /// Method extracts columns' data types from table stored on SQL Server.
            /// </summary>
            /// <param name="tabName">Table name on SQL Server DB.</param>
            /// <returns>Returns DataTable object.</returns>
            public DataTable ExtractDataTypesFromSQLTable(string tabName)
            {
                DataTable table = new DataTable();
                try
                {
                    using (SqlConnection con = new SqlConnection(this._connectionString))
                    {
                        string sqlQuery = @"SELECT [Column Name],[Data type],[Max Length],[precision],[scale],[is_nullable],[Primary Key]
                                        FROM
                                        (
                                        SELECT [Column Name],[Data type],[Max Length],[precision],[scale],[is_nullable],[Primary Key],
                                        r_number, ROW_NUMBER() OVER(PARTITION BY [Column Name] ORDER BY [Primary Key] DESC) rn
                                        FROM
                                        (
                                        SELECT
                                            c.name 'Column Name',
                                            t.Name 'Data type',
                                            c.max_length 'Max Length',
                                            c.[precision],
                                            c.[scale],
                                            c.is_nullable,
                                            ISNULL(i.is_primary_key, 0) 'Primary Key',
	                                        ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) r_number
                                        FROM    
                                            sys.columns c
                                        INNER JOIN 
                                            sys.types t ON c.user_type_id = t.user_type_id
                                        LEFT OUTER JOIN 
                                            sys.index_columns ic ON ic.object_id = c.object_id AND ic.column_id = c.column_id
                                        LEFT OUTER JOIN 
                                            sys.indexes i ON ic.object_id = i.object_id AND ic.index_id = i.index_id
                                        WHERE
                                            c.object_id = OBJECT_ID('" + tabName + "')) a ) b WHERE b.rn = 1 ORDER BY b.r_number";

                        using (SqlCommand cmd = new SqlCommand(sqlQuery, con))
                        {
                            SqlDataAdapter ds = new SqlDataAdapter(cmd);
                            ds.Fill(table);
                        }
                        con.Close();
                    }
                }
                catch (Exception ex)
                {
                    ShowPrintMethod(ex.Message.ToString());
                    Environment.Exit(1);
                }
                return table;
            }
            /// <summary>
            /// Method tests whether table exists on SQL Server DB.
            /// </summary>
            /// <param name="tabname">Table name on SQL Server DB.</param>
            /// <returns>Returns bool type.</returns>
            public bool IfSQLTableExists(string tabname)
            {
                bool exists = false;
                tabname = tabname.Replace("[", string.Empty).Replace("]", string.Empty);
                try
                {
                    using (SqlConnection con = new SqlConnection(this._connectionString))
                    {
                        con.Open();
                        using (SqlCommand command = new SqlCommand("SELECT CASE WHEN EXISTS((SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA + '.' + TABLE_NAME = '" + tabname + "' OR TABLE_NAME = '" + tabname + "')) THEN 1 ELSE 0 END", con))
                        {
                            command.CommandTimeout = 0;
                            exists = (int)command.ExecuteScalar() == 1;
                        }
                        con.Close();
                    }
                }
                catch (InvalidOperationException ioe)
                {
                    ShowPrintMethod("Invalid Operation Exception: " + ioe.Message.ToString() + "\nSomething is wrong with your connection string! You might check back slashes!");
                    Environment.Exit(1);
                }
                catch (Exception ex)
                {
                    ShowPrintMethod(ex.Message.ToString());
                    Environment.Exit(1);
                }
                return (exists);
            }
            /// <summary>
            /// Method stores table on SQL Server into DataTable object
            /// </summary>
            /// <param name="sqlQuery">SQL query to download data from SQL Server</param>
            /// <returns></returns>
            public DataTable FromSQLToDataTable(string sqlQuery)
            {
                DataTable table = new DataTable();
                try
                {
                    using (SqlConnection con = new SqlConnection(this._connectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand(sqlQuery, con))
                        {
                            SqlDataAdapter ds = new SqlDataAdapter(cmd);
                            ds.Fill(table);
                        }
                        con.Close();
                    }
                }
                catch (Exception ex)
                {
                    ShowPrintMethod(ex.Message.ToString());
                    Environment.Exit(1);
                }
                return table;
            }

            /// <summary>
            /// Write table on SQL Server into text file using StringBuilder. Writing all the data into StringBuilder and then into text file.
            /// </summary>
            /// <param name="sql_query">SQL SELECT statement.</param>
            /// <param name="csvpath">Patch to *.csv file.</param>
            /// <param name="sepType">Separator, default SepType.auto identifies separator automatically.</param>
            /// <param name="showprogress">Show already flushed data into csv file.</param>
            /// <param name="batchSize">Batch size shows already flushed data into *.csv file. Default 100000.</param>
            public void WriteFromDBToTextFile(string sql_query,
                                         string csvpath,
                                         SepType sepType = SepType.auto,
                                         bool showprogress = false,
                                         int batchSize = 100000)
            {
                // DataTable dataTable = new DataTable();
                StringBuilder sb = new StringBuilder();
                DataTable dataTable = new DataTable();
                // define separator
                char sep = AutoMethods.SetSeparator(sepType);

                try
                {
                    using (SqlConnection con = new SqlConnection(this._connectionString))
                    {
                        con.Open();
                        using (SqlCommand command = new SqlCommand(sql_query, con))
                        {
                            command.CommandTimeout = 0;

                            if (showprogress) { ShowPrintMethod("Pushing data from SQL Server into DataTable"); }

                            using (IDataReader rdr = command.ExecuteReader())
                            {
                                dataTable = DataExtraction.GetDataTableFromDataReader(rdr);
                            }
                            //IDataReader rdr = new SqlCommand(sql_query, con).ExecuteReader(CommandBehavior.CloseConnection);
                            //dataTable = GetDataTableFromDataReader(rdr);
                            //rdr = null;

                            //SqlDataAdapter da = new SqlDataAdapter(command);
                            //if (showprogress) { ShowPrintMethod("Downloading data from sql server and pushing into DataTable object"); }
                            //da.Fill(dataTable);

                            if (showprogress) { ShowPrintMethod("Pushing data from DataTable object into StringBuilder"); }

                            for (int i = 0; i < dataTable.Columns.Count; i++)
                            {
                                sb.Append(dataTable.Columns[i].ColumnName);
                                sb.Append(i == dataTable.Columns.Count - 1 ? "\n" : sep.ToString());
                            }

                            string day_s = string.Empty;
                            string month_s = string.Empty;
                            string value = string.Empty;
                            Int32 counter = 0;
                            Int32 c_ounter = 0;
                            // Writing data into csv file
                            foreach (DataRow row in dataTable.Rows)
                            {
                                for (int i = 0; i < dataTable.Columns.Count; i++)
                                {
                                    if (row[i].GetType().Name == "DateTime")
                                    {
                                        DateTime dt_val = (DateTime)row[i];
                                        if (dt_val.Month.ToString().Length == 1)
                                        {
                                            month_s = "0" + dt_val.Month.ToString();
                                        }
                                        else
                                        {
                                            month_s = dt_val.Month.ToString();
                                        }
                                        if (dt_val.Day.ToString().Length == 1)
                                        {
                                            day_s = "0" + dt_val.Day.ToString();
                                        }
                                        else
                                        {
                                            day_s = dt_val.Day.ToString();
                                        }
                                        if (dt_val.Hour == 0 & dt_val.Minute == 0 & dt_val.Second == 0 & dt_val.Millisecond == 0)
                                        {
                                            value = dt_val.Year.ToString() + "-" + month_s + "-" + day_s;
                                        }
                                        else
                                        {
                                            value = dt_val.Year.ToString() + "-" + month_s + "-" + day_s + " " + dt_val.TimeOfDay.ToString();
                                        }
                                        sb.Append(value);
                                        sb.Append(i == dataTable.Columns.Count - 1 ? "\n" : sep.ToString());
                                    }
                                    else if (row[i].GetType().Name == "Decimal" |
                                            row[i].GetType().Name == "Numeric" |
                                            row[i].GetType().Name == "Float" |
                                            row[i].GetType().Name == "Double" |
                                            row[i].GetType().Name == "Single")
                                    {
                                        Double val;
                                        if (double.TryParse(row[i].ToString(), out val))
                                        {
                                            sb.Append(val.ToString(CultureInfo.InvariantCulture));
                                            sb.Append(i == dataTable.Columns.Count - 1 ? "\n" : sep.ToString());
                                        }
                                    }
                                    else
                                    {
                                        sb.Append(row[i].ToString());
                                        sb.Append(i == dataTable.Columns.Count - 1 ? "\n" : sep.ToString());
                                    }
                                }
                                counter++;
                                c_ounter++;
                                if (c_ounter == batchSize & showprogress)
                                {
                                    ShowPrintMethod(counter + " rows inserted from StringBuilder --> csv.");
                                    File.AppendAllText(csvpath, sb.ToString());
                                    sb.Clear();
                                    c_ounter = 0;
                                }
                            }
                            if (sb.Length != 0)
                            {
                                File.AppendAllText(csvpath, sb.ToString());
                            }
                            ShowPrintMethod(counter + " records written into DataFrame/DataTable.");
                            // if (showprogress) { ShowPrintMethod("Writing from StringBuilder into csv file."); }
                            // File.WriteAllText(csvpath, sb.ToString());
                        }
                        con.Close();
                    }
                }
                catch (Exception ex)
                {
                    ShowPrintMethod(ex.Message.ToString());
                    Environment.Exit(1);
                }
            }

            /// <summary>
            /// Writing the data into text file via StreamWriter. Writing directly into text file.
            /// </summary>
            /// <param name="sql_query">SQL SELECT statement.</param>
            /// <param name="csvpath">Patch to *.csv file.</param>
            /// <param name="sepType">Separator, default SepType.auto identifies separator automatically.</param>
            /// <param name="showprogress">Show already flushed data into csv file.</param>
            /// <param name="batchSize">Batch size shows already flushed data into *.csv file. Default 100000.</param>
            public void WriteToTextFileFromDB(string sql_query,
                                          string csvpath,
                                          SepType sepType = SepType.auto,
                                          bool showprogress = false,
                                          int batchSize = 10000)
            {
                try
                {
                    // define separator
                    char sep = AutoMethods.SetSeparator(sepType);
                    // define counter for writing into flat file
                    Int32 cntr = 0;
                    Int32 cntr_overall = 0;
                    //create connection
                    SqlCommand comm = new SqlCommand
                    {
                        Connection = new SqlConnection(this._connectionString)
                    };
                    string sql = sql_query;

                    comm.CommandText = sql;
                    comm.CommandTimeout = 0;
                    comm.CommandType = CommandType.Text;
                    comm.Connection.Open();

                    SqlDataReader sqlReader = comm.ExecuteReader();

                    // Open the file for write operations.  If exists, it will overwrite due to the "false" parameter
                    using (StreamWriter file = new StreamWriter(csvpath, false))
                    {
                        object[] output = new object[sqlReader.FieldCount];

                        for (int i = 0; i < sqlReader.FieldCount; i++)
                            output[i] = sqlReader.GetName(i);

                        file.WriteLine(string.Join(sep.ToString(), output));

                        while (sqlReader.Read())
                        {
                            sqlReader.GetValues(output);

                            string day_s = string.Empty;
                            string month_s = string.Empty;
                            Int32 counter = 0;

                            foreach (var val in output)
                            {
                                if (val.GetType().Name == "DateTime")
                                {
                                    DateTime dt_val = (DateTime)val;
                                    if (dt_val.Month.ToString().Length == 1)
                                    {
                                        month_s = "0" + dt_val.Month.ToString();
                                    }
                                    else
                                    {
                                        month_s = dt_val.Month.ToString();
                                    }
                                    if (dt_val.Day.ToString().Length == 1)
                                    {
                                        day_s = "0" + dt_val.Day.ToString();
                                    }
                                    else
                                    {
                                        day_s = dt_val.Day.ToString();
                                    }
                                    if (dt_val.Hour == 0 & dt_val.Minute == 0 & dt_val.Second == 0 & dt_val.Millisecond == 0)
                                    {
                                        output[counter] = dt_val.Year.ToString() + "-" + month_s + "-" + day_s;
                                    }
                                    else
                                    {
                                        output[counter] = dt_val.Year.ToString() + "-" + month_s + "-" + day_s + " " + dt_val.TimeOfDay.ToString();
                                    }
                                }
                                else if (val.GetType().Name == "Decimal" |
                                        val.GetType().Name == "Numeric" |
                                        val.GetType().Name == "Float" |
                                        val.GetType().Name == "Double" |
                                        val.GetType().Name == "Single")
                                {
                                    Double numval;
                                    if (double.TryParse(val.ToString(), out numval))
                                    {
                                        output[counter] = numval.ToString(CultureInfo.InvariantCulture);
                                    }
                                }
                                else
                                {
                                    output[counter] = val.ToString();
                                }
                                counter += 1;
                            }
                            if (cntr == batchSize && showprogress)
                            {
                                ShowPrintMethod("Flushed " + batchSize + " records into flat file, " + cntr_overall + " records are already there.");
                                cntr = 0;
                            }
                            file.WriteLine(string.Join(sep.ToString(), output));
                            cntr_overall += 1;
                            cntr += 1;
                        }
                    }
                    comm.Connection.Close();
                }
                catch (Exception ex)
                {
                    ShowPrintMethod(ex.Message.ToString());
                    Environment.Exit(1);
                }
            }
            /// <summary>
            /// Create SQL table based on the data in text file.
            /// </summary>
            /// <param name="pathtocsv">Patch to *.csv file.</param>
            /// <param name="tablename">Name of the table on SQL Server needs to be created.</param>
            /// <param name="rowstoestimatedatatype">How many records are supposed to be used to data type identification, default is 250000.</param>
            /// <param name="sepType">Separator, default SepType.auto identifies separator automatically.</param>
            private void CreateSQLTableBasedOnTextFile(string pathtocsv,
                                       string tablename,
                                       Int32 rowstoestimatedatatype = 250000,
                                       SepType sepType = SepType.auto)
            {
                char sep = AutoMethods.DetectSeparator(pathtocsv, sepType);
                
                string[,] sqldts = DataTypeIdentifier.SQLDataTypesBasedOnTextFile(pathtocsv, rowstoestimatedatatype, sep);

                string createTable_string = string.Empty;

                for (int i = 0; i < sqldts.GetLength(1); i++)
                {
                    if (i == sqldts.GetLength(1) - 1)
                    {
                        createTable_string = createTable_string + "[" + sqldts[1, i] + "]" + " " + sqldts[0, i];
                    }
                    else
                    {
                        createTable_string = createTable_string + "[" + sqldts[1, i] + "]" + " " + sqldts[0, i] + ", ";
                    }
                }
                try
                {
                    using (SqlConnection con = new SqlConnection(this._connectionString))
                    {
                        con.Open();
                        string createTable = @"CREATE TABLE " + tablename + " (" + createTable_string + ");";
                        using (SqlCommand command = new SqlCommand(createTable, con))
                        {
                            command.CommandTimeout = 0;
                            command.ExecuteNonQuery();
                        }
                        con.Close();
                    }
                }
                catch (Exception ex)
                {
                    ShowPrintMethod(ex.Message.ToString());
                    Environment.Exit(1);
                }
            }
            /// <summary>
            /// Create SQL table based on the data in DataTable.
            /// </summary>
            /// <param name="dt">DataTable object.</param>
            /// <param name="tablename">Name of the table on SQL Server needs to be created.</param>
            public void CreateSQLTableBasedOnDataTable(DataTable dt,
                                                       string tablename)
            {
                string[,] sqldts = DataTypeIdentifier.SQLDataTypesBasedOnDataTable(dt);

                string createTable_string = string.Empty;

                for (int i = 0; i < sqldts.GetLength(1); i++)
                {
                    if (i == sqldts.GetLength(1) - 1)
                    {
                        createTable_string = createTable_string + "[" + sqldts[1, i] + "]" + " " + sqldts[0, i];
                    }
                    else
                    {
                        createTable_string = createTable_string + "[" + sqldts[1, i] + "]" + " " + sqldts[0, i] + ", ";
                    }
                }
                try
                {
                    using (SqlConnection con = new SqlConnection(this._connectionString))
                    {
                        con.Open();
                        string createTable = @"CREATE TABLE " + tablename + " (" + createTable_string + ");";
                        using (SqlCommand command = new SqlCommand(createTable, con))
                        {
                            command.CommandTimeout = 0;
                            command.ExecuteNonQuery();
                        }
                        con.Close();
                    }
                }
                catch (Exception ex)
                {
                    ShowPrintMethod(ex.Message.ToString());
                    Environment.Exit(1);
                }
            }

            /// <summary>
            /// Test if connection to the SQL Server is valid.
            /// </summary>
            /// <returns>Returns Tuple<bool, string>(true/false, string.Empty/SqlException error message).</returns>
            public Tuple<bool, string> IsServerConnected()
            {
                using (SqlConnection connection = new SqlConnection(this._connectionString))
                {
                    try
                    {
                        connection.Open();
                        var tpl = new Tuple<bool, string>(true, string.Empty);
                        ShowPrintMethod("Connection is valid");
                        return tpl;
                    }
                    catch (SqlException ex)
                    {
                        string msg = "SqlException message: " + ex.Message.ToString();
                        var tpl = new Tuple<bool, string>(false, msg);
                        return tpl;
                    }
                }
            }
            /// <summary>
            /// Converts data from text file into DataTable object.
            /// </summary>
            /// <param name="pathtocsv">Path to *.csv file.</param>
            /// <param name="sepType">Separator, default SepType.auto identifies separator automatically.</param>
            /// <returns>Returns DataTable object.</returns>
            public DataTable TextFileToDataTableAll(string pathtocsv,
                                                    SepType sepType = SepType.auto)
            {
                char sep = AutoMethods.DetectSeparator(pathtocsv, sepType);
                return TextFileToDataTable.CsvToDataTableAll(pathtocsv, sep);
            }
            /// <summary>
            /// Calling SQL stored procedure.
            /// </summary>
            /// <param name="spName">Name of stored procedure on SQL Server. E.g. dbo.sp_SelectZerosFromBoolData.</param>
            /// <param name="arrayListParams">ArrayList of parameters.</param>
            /// <param name="arrayListValues">ArrayList of values.</param>
            /// <param name="returningData">True: returns data into DataTable (result of SELECT statement), False: calls stored procedure without returning any data (INSERT, UPDATE, DELETE).</param>
            /// <returns>Returns DataTable object.</returns>
            public DataTable CallSQLStoredProcedure(string spName,
                                                ArrayList arrayListParams,
                                                ArrayList arrayListValues,
                                                bool returningData = false)
            {
                DataTable dataTable = new DataTable();
                try
                {
                    using (SqlConnection con = new SqlConnection(this._connectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand(spName, con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            if (arrayListParams.Count != 0)
                            {
                                if (arrayListParams.Count == arrayListValues.Count)
                                {
                                    for (int i = 0; i < arrayListParams.Count; ++i)
                                    {
                                        cmd.Parameters.Add(new SqlParameter(arrayListParams[i].ToString(), arrayListValues[i]));
                                    }
                                }
                                else
                                {
                                    ShowPrintMethod("List of parameters is not equal to list of values");
                                    Environment.Exit(1);
                                }
                            }
                            con.Open();
                            if (returningData)
                            {
                                using (SqlDataReader rdr = cmd.ExecuteReader())
                                {
                                    dataTable.Load(rdr);
                                    ShowPrintMethod("DataTable with " + dataTable.Columns.Count + " columns and " + dataTable.Rows.Count + " returned.");
                                }
                            }
                            else
                            {
                                var results = cmd.ExecuteNonQuery();
                                ShowPrintMethod(results + " records affected and empty DataTable returned.");
                            }       
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowPrintMethod(ex.Message.ToString());
                }
                return dataTable;
            }
        }
    }
}
