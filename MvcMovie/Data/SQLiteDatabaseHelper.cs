using System;
using Microsoft.Data.Sqlite;

public class SQLiteDatabaseHelper
{
    private SqliteConnection connection;
    private string connectionString = "Data Source=database.db";

    public SQLiteDatabaseHelper()
    {
        connection = new SqliteConnection(connectionString);
    }

    public SqliteConnection GetConnection()
    {
        return connection;
    }

    public void OpenConnection()
    {
        try
        {
            connection.Open();
        }
        catch (SqliteException ex)
        {
            Console.WriteLine(ex.Message);
            throw ex;
        }
    }

    public void CloseConnection()
    {
        if (connection != null && connection.State != System.Data.ConnectionState.Closed)
        {
            connection.Close();
        }
    }
}
