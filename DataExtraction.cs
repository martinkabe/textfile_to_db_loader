using System;
using System.Data;

class DataExtraction
{
    public static DataTable GetDataTableFromDataReader(IDataReader dataReader)
    {
        DataTable schemaTable = dataReader.GetSchemaTable();
        DataTable resultTable = new DataTable();

        foreach (DataRow dataRow in schemaTable.Rows)
        {
            DataColumn dataColumn = new DataColumn();
            dataColumn.ColumnName = dataRow["ColumnName"].ToString();
            dataColumn.DataType = Type.GetType(dataRow["DataType"].ToString());
            dataColumn.ReadOnly = (bool)dataRow["IsReadOnly"];
            dataColumn.AutoIncrement = (bool)dataRow["IsAutoIncrement"];
            dataColumn.Unique = (bool)dataRow["IsUnique"];

            resultTable.Columns.Add(dataColumn);
        }
        while (dataReader.Read())
        {
            DataRow dataRow = resultTable.NewRow();
            for (int i = 0; i < resultTable.Columns.Count; i++)
            {
                dataRow[i] = dataReader[i];
            }
            resultTable.Rows.Add(dataRow);
        }
        return resultTable;
    }
}
