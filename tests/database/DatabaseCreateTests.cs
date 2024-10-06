using AuthRoute;
using Data;


namespace database;

public class DatabaseCreateTests
{
    readonly MysqlDatabase database = MysqlDatabase.Instance;

    public void Setup()
    {
        DotNetEnv.Env.TraversePath().Load();
        database.IsConnected();
        var userTableHandler = new UserTableDatabaseHandler(database);
        userTableHandler.UserTableUseSource("/root/SpotifyApi/server/src/data/SpotifyServer.sql");
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        database.IsConnected();
        var userTableHandler = new UserTableDatabaseHandler(database);
        userTableHandler.UserTableUseSource("/root/SpotifyApi/server/src/data/SpotifyServer.sql");       
    }

   [Test]
    public void DatabaseIsConnected()
    {
        var result = database.IsConnected(); 
        Assert.That(result, Is.True);
    }
    [Test]
    public void UserTableCreateDummyEntry()
    {
        var userTableHandler = new UserTableDatabaseHandler(database);
        var user = new AuthenicatedAuth0User("oauth2|spotify|spotify:user:1253470477", Auth0ManagementSystem.Instance);
        user.UserProfile = new Auth0MangementUserProfileResponse();
        user.UserProfile.identities = new List<Identities>();
        user.UserProfile.images = new List<Images>();

        user.UserProfile.display_name = "fake user";
        user.UserProfile.user_id = "oauth2|spotify|spotify:user:123456789";
        user.UserProfile.identities.Add(new Identities {access_token = "Fake Access Token"});
        user.UserProfile.images.Add(new Images {url = "Fake Image URL"});

        var isUserCreated = userTableHandler.UserCreate(user, "fake session id");
        Assert.That(isUserCreated, Is.True);
    }
}