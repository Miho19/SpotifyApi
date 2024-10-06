using System.Data;
using MySql.Data.MySqlClient;
using AuthRoute;
using Mysqlx;
namespace Data;

public class MysqlDatabase : IDatabase
{
    public IDbConnection? Connection {get; set;}

    public void Close()
    {
        Connection?.Close();
    }

    public bool IsConnected()
    {
        if (Connection is not null)
        {
            return true;
        }
        try 
        {
            
            var connectionString = System.Environment.GetEnvironmentVariable("DatabaseConnectionString");
            Connection = new MySqlConnection(connectionString);
            
            Connection.Open();
            return true;
        } catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
        
    }

    private static MysqlDatabase? instance = null;
    private static readonly object GetLock = new object();

    MysqlDatabase()
    {
    }

    public static MysqlDatabase Instance
    {
        get 
        {
            lock(GetLock)
            {
                if(instance == null)
                {
                    instance = new MysqlDatabase();
                }
                return instance;
            }
        }
    }

}

public class UserTableDatabaseHandler(IDatabase database)
{
    private readonly IDatabase Database = database;

    public bool UserCreate(IAuthenicatedUser user, string sessionID)
    {
        try 
        {   
            if (!UserCreateIsUserObjectValid(user)) return false;
            if (String.IsNullOrEmpty(sessionID)) return false;
            var newUserTableID = UserTableAdd(sessionID);

            return SpotifyUserDataTableAdd(user, newUserTableID);

        } 
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;   
        }
        
    }

    private int UserTableAdd(string sessionID)
    {   
        var userAddCommand = GetMySqlCommand();
        userAddCommand.CommandText = @"INSERT INTO users (u_sid) VALUES (@sessionID)";
        userAddCommand.Parameters.AddWithValue("@sessionID", sessionID);
        userAddCommand.ExecuteNonQuery();

        var userGetCommand = GetMySqlCommand();
        userGetCommand.CommandText = @"SELECT * FROM users WHERE u_sid=@sessionID";
        userGetCommand.Parameters.AddWithValue("@sessionID", sessionID);
        userGetCommand.ExecuteNonQuery();

        MySqlDataReader reader = userGetCommand.ExecuteReader();
        reader.Read();
        var readerResponse = reader["u_id"].ToString();
        reader.Close();

        if (String.IsNullOrEmpty(readerResponse)) throw new Exception("Failed to read u_id from database");
        var userID = Int32.Parse(readerResponse);
        return userID;
    }

    private bool SpotifyUserDataTableAdd(IAuthenicatedUser user, int userID)
    {
        
        var accessToken = user.UserProfile?.identities?[0].access_token;
        var displayName = user.UserProfile?.display_name;
        var spotifyUserID = user.UserProfile?.user_id;
        var imageURL = user.UserProfile?.images?[0].url;

        var spotifyUserDataAddCommand = GetMySqlCommand();
        spotifyUserDataAddCommand.CommandText = @"INSERT INTO spotifyUserData
            (spotify_id, u_id, displayName, accessToken, imageURL) 
            VALUES(@spotifyUserID, @u_id, @displayName, @accessToken, @imageURL)";
        
        spotifyUserDataAddCommand.Parameters.AddWithValue("@spotifyUserID", spotifyUserID);
        spotifyUserDataAddCommand.Parameters.AddWithValue("@u_id", userID);
        spotifyUserDataAddCommand.Parameters.AddWithValue("@displayName", displayName);
        spotifyUserDataAddCommand.Parameters.AddWithValue("@accessToken", accessToken);
        spotifyUserDataAddCommand.Parameters.AddWithValue("@imageURL", imageURL);

        spotifyUserDataAddCommand.ExecuteNonQuery();

        return true;
    }
    

    private bool UserCreateIsUserObjectValid(IAuthenicatedUser user)
    {   
        if (user.UserProfile is null) return false;
        if (String.IsNullOrEmpty(user.AccessToken)) return false;
        if (String.IsNullOrEmpty(user.UserProfile.display_name)) return false;
        if (String.IsNullOrEmpty(user.UserProfile.user_id)) return false;

        if (user.UserProfile.identities is null || user.UserProfile.identities.Count <= 0) return false;
        if (user.UserProfile.images is null || user.UserProfile.images.Count <= 0) return false;

        if (String.IsNullOrEmpty(user.UserProfile.identities[0].access_token)) return false;
        if (String.IsNullOrEmpty(user.UserProfile.images[0].url)) return false;

        return true;
    }

    private MySqlCommand GetMySqlCommand()
    {
        if(!Database.IsConnected())
        {
            throw new Exception("Failed to connect to database");
        } 
        MySqlCommand command = new MySqlCommand();
        command.Connection = Database.Connection as MySqlConnection;
        return command;
    }

    public List<IAuthenicatedUser> UserRead(string sessionID)
    {
        try 
        {
            var readData = new List<IAuthenicatedUser>();
            var command = GetMySqlCommand();
            command.CommandText = @"SELECT u_sid as sessionID, spotify_id as userID, displayName, accessToken, imageUrl from users INNER JOIN spotifyUserData ON users.u_id = spotifyUserData.u_id WHERE (users.u_sid = @sessionID)";
            command.Parameters.AddWithValue("@sessionID", sessionID);
            MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                readData.Add(UserConvertSpotifyUserDataToAuthenicatedUserObject(reader));
            }
            reader.Close();
            return readData;
        }
        catch
        {
            return [];
        }
    }

    private IAuthenicatedUser UserConvertSpotifyUserDataToAuthenicatedUserObject(MySqlDataReader reader)
    {
        var userID = reader["userID"].ToString();
        var accessToken = reader["accessToken"].ToString();
        var displayName = reader["displayName"].ToString();
        var imageURL = reader["imageURL"].ToString();


        if(string.IsNullOrEmpty(userID)) throw new Exception("Failed to read userID from User Table");

        var auth0UserObject = new AuthenicatedAuth0User(userID, Auth0ManagementSystem.Instance);
        auth0UserObject.UserProfile = new Auth0MangementUserProfileResponse();
        auth0UserObject.UserProfile.identities = new List<Identities>();
        auth0UserObject.UserProfile.images = new List<Images>();
        
        var userProfile = auth0UserObject.UserProfile ?? throw new Exception("Failed to initialise AuthenicatedAuth0User Object");

        if(userProfile.identities is null) throw new Exception("Failed to initialise identities list");
        if(userProfile.images is null) throw new Exception("Failed to initialise images list");

        

        userProfile.display_name = displayName;
        userProfile.user_id = userID;
        userProfile.identities.Add(new Identities {access_token = accessToken});
        userProfile.images.Add(new Images {url = imageURL});

        return auth0UserObject;

    }

    public bool UserUpdate(IAuthenicatedUser user)
    {
        return false;
    }
    public bool UserUpdate(string sessionID)
    {
        return false;
    }


    public bool UserDelete(IAuthenicatedUser user)
    {
        return false;
    }

    public bool UserDelete(string sessionID)
    {
        return false;
    }

    public void UserTableUseSource(string filePath)
    {
        try 
        {
            FileInfo file = new FileInfo(filePath);
            if (!file.Exists)
            {
                Console.WriteLine($"File: ${filePath}\nCould not be found");
                return;
            }

            var command = GetMySqlCommand();
            command.CommandText = @"source @filePath";
            command.Parameters.AddWithValue("@filePath", file.FullName);

            command.ExecuteNonQuery();

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        } 
        
        

    }

}

