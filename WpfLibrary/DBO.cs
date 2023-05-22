using CSHlibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSH07;

public class DBO : IDisposable, IDBO
{
    private OdbcConnection odbcConnection;
    private readonly string dbo;
    private readonly string user;
    private readonly string password;
    const string driver = "DRIVER={MySQL ODBC 8.0 ANSI Driver};";
    const string server = "SERVER=localhost;PORT=3306;";
    private bool disposed;
    private string url => $"{driver}{server}{dbo}{user}{password}";
    public DBO(string dbo, string user, string password)
    {
        this.dbo = $"DATABASE={dbo};";
        this.user = $"UID={user};";
        this.password = $"PWD={password};";
        odbcConnection = new OdbcConnection(url);
    }
    public async Task InitializeConnection()
    {
        try
        {
            await odbcConnection.OpenAsync();
            await Console.Out.WriteLineAsync("Connection success");
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
                await Console.Out.WriteLineAsync("Connection closed successfully");
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
