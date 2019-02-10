using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;

class TextFileToDataTable
{
    /// <summary>
    /// Store csv file into data table
    /// </summary>
    /// <param name="pathToCSV">Determine path to csv file stored on disk.</param>
    /// <param name="countRowsToBeStored">How many rows should be stored into data table from csv file to estimate separator.</param>
    /// <param name="sep">Separator for columns delimited.</param>
    /// <returns></returns>
    public static DataTable CsvToDataTableToEstimateSepAndDataTypes(string pathToCSV, Int32 countRowsToBeStored, char sep)
    {
        DataTable dtCsv = new DataTable();
        Int32 counter = 0;

        try
        {
            using (StreamReader sr = new StreamReader(pathToCSV))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    string[] values = line.Split(sep);

                    DataRow dr = dtCsv.NewRow();

                    // first add columns
                    if (counter < 1)
                    {
                        for (int i = 0; i < values.Count(); i++)
                        {
                            dtCsv.Columns.Add(values[i]); //add headers  
                        }
                    }
                    else
                    {
                        for (int i = 0; i < values.Count(); i++)
                        {
                            values[i] = Regex.Replace(values[i], @"\s\s+", "");
                            if (values[i] == "NA" || string.IsNullOrWhiteSpace(values[i]))
                            {
                                values[i] = null;
                            }
                            dr[i] = values[i];
                        }
                        dtCsv.Rows.Add(dr); //add other rows
                    }

                    counter++;

                    if (counter == countRowsToBeStored)
                    {
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
        return dtCsv;
    }

    public static DataTable CsvToDataTableAll(string pathToCSV, char sep)
    {
        DataTable dtCsv = new DataTable();
        Int32 counter = 0;
        try
        {
            using (StreamReader sr = new StreamReader(pathToCSV))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    string[] values = line.Split(sep);

                    DataRow dr = dtCsv.NewRow();

                    // first add columns
                    if (counter < 1)
                    {
                        for (int i = 0; i < values.Count(); i++)
                        {
                            dtCsv.Columns.Add(values[i]); //add headers  
                        }
                    }
                    else
                    {
                        for (int i = 0; i < values.Count(); i++)
                        {
                            values[i] = Regex.Replace(values[i], @"\s\s+", "");
                            if (values[i] == "NA" || string.IsNullOrWhiteSpace(values[i]))
                            {
                                values[i] = null;
                            }
                            dr[i] = values[i];
                        }
                        dtCsv.Rows.Add(dr); //add other rows
                    }
                    counter++;
                }
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
        return dtCsv;
    }
}
