using System.Data;

namespace Data;

public interface IDatabase
{
    public IDbConnection? Connection {get; set;}

    public bool IsConnected();
    public void Close();

}

