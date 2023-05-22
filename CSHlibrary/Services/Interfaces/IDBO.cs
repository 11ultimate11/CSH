using System.Data;

namespace CSHlibrary;
/// <summary>
/// Simply Interface that connect to local database
/// </summary>
public interface IDBO
{
    /// <summary>
    /// Invoke this Method to Dispose the ConnectionObject
    /// </summary>
    void Dispose();
    /// <summary>
    /// Its take the parameter <paramref name="query"/> as SQL query to be executed
    /// <br></br>
    /// Its return 2 lists of strings one for the header of the query and the other for the values.
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    Task<(List<string>, List<string>)> ExecuteQuery(string query);
    Task<DataSet> ExecuteQueryDataSetASync(string query);
    Task<bool> InsertInto(string query, Dictionary<string, object> values);
    Task<bool> DeleteFrom(string query, Dictionary<string, int> values);
    /// <summary>
    /// Its return a value false of the true representing the actual connection state
    /// </summary>
    bool Connected { get; }
}