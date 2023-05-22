using CSHlibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CSHlibrary;

public class DBO : IDisposable, IDBO
{
    private OdbcConnection odbcConnection;
    private readonly string dbo;
    private readonly string user;
    private readonly string password;
    const string driver = "DRIVER={MySQL ODBC 8.0 ANSI Driver};";
    const string server = "SERVER=localhost;PORT=3306;";
    private bool disposed;
    private bool isBusy;
    public bool Connected => odbcConnection.State == ConnectionState.Open;
    private string connectionString => $"{driver}{server}{dbo}{user}{password}";
    /// <summary>
    /// Request <paramref name="dbo"/> , <paramref name="password"/> and <paramref name="user"/> to instantiate a new connection
    /// <br></br>
    /// Additionally you may pass the parameter <paramref name="timeout"/> in order to set a connectiont timeout
    /// </summary>
    /// <param name="dbo"></param>
    /// <param name="user"></param>
    /// <param name="password"></param>
    /// <param name="timeout"></param>
    public DBO(string dbo, string user, string password, int timeout = 2500)
    {
        this.dbo = $"DATABASE={dbo};";
        this.user = $"UID={user};";
        this.password = $"PWD={password};";
        odbcConnection = new OdbcConnection(connectionString);
        odbcConnection.ConnectionTimeout = timeout;
    }
    public async Task<(List<string>, List<string>)> ExecuteQuery(string query)
    {
        if (isBusy) return (null!, null!);
        try
        {
            isBusy = true;
            await InitializeConnection();
            if (!Connected) return (null!, null!);
            if (query.ToUpper().StartsWith("SELECT"))
            {
                using OdbcCommand command = new(query, odbcConnection);
                using var reader = await command.ExecuteReaderAsync();
                List<string> result = new();
                List<string> names = new();
                int maxFields = reader.FieldCount;
                for (int i = 0; i < maxFields; i++)
                {
                    names.Add(reader.GetName(i));
                }
                while (reader.Read())
                {
                    string value = string.Empty;
                    for (int i = 0; i < maxFields; i++)
                    {
                        value += $" {reader.GetValue(i)}";
                    }
                    result.Add(value);
                }
                return (names, result);
            }
            else
            {
                return (null!, null!);
            }

        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return (null!, null!);
        }
        finally
        {

            if (Connected)
                await odbcConnection.CloseAsync();
            isBusy = false;
        }

    }
    public async Task<DataSet> ExecuteQueryDataSetASync(string query)
    {
        DataSet ds = new();
        try
        {
            await odbcConnection.OpenAsync();
            OdbcDataAdapter adapter = new(query, odbcConnection);
            adapter.Fill(ds);
            return ds;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return ds;
        }
        finally
        {
            await odbcConnection.CloseAsync();
        }
    }
    public async Task<bool> InsertInto(string query, Dictionary<string, object> values)
    {
        try
        {
            OdbcCommand command = new(query, odbcConnection);
            foreach (var item in values)
            {
                command.Parameters.AddWithValue(item.Key, item.Value);
            }
            await odbcConnection.OpenAsync();
            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return false;
        }
        finally
        {
            await odbcConnection.CloseAsync();
        }
    }
    public async Task<bool> DeleteFrom(string query, Dictionary<string, int> values)
    {
        try
        {
            OdbcCommand command = new OdbcCommand(query, odbcConnection);
            foreach (var item in values)
            {
                command.Parameters.AddWithValue(item.Key, item.Value);
            }
            await odbcConnection.OpenAsync();
            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch(Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return false;
        }
        finally
        {
            await odbcConnection.CloseAsync();
        }
    }
    private async Task InitializeConnection()
    {
        try
        {
            await odbcConnection.OpenAsync();
            Debug.WriteLine("Connection success");
        }
        catch (Exception ex)
        {
            await Console.Out.WriteLineAsync(ex.Message);
            throw new Exception(ex.Message);
        }
    }
    public void Dispose()
    {
        Task.Run(async () => await Dispose(true));
        GC.SuppressFinalize(this);
    }
    protected virtual async Task Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                if (odbcConnection.State != ConnectionState.Closed)
                {
                    await odbcConnection.CloseAsync();
                }
                await odbcConnection.DisposeAsync();
                Debug.WriteLine("Connection closed successfully");
                odbcConnection = null!;
            }
            disposed = true;
        }
    }
    ~DBO()
    {
        Dispose(false);
        Console.WriteLine("Dbo disposed successfully");
    }
}
