using AuthRoute;
using Data;


namespace database;

public class DatabaseReadTests
{
    readonly MysqlDatabase database = MysqlDatabase.Instance;

    [SetUp]
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
    public void UserTableReadReturnDummyEntry()
    {
        var userTableHandler = new UserTableDatabaseHandler(database);
        List<IAuthenicatedUser> result = userTableHandler.UserRead("fake user session id");
        var firstResponse = result[0] ?? throw new Exception("Failed to get user data");
        var userProfile = firstResponse.UserProfile ?? throw new Exception("User Profile not initialised");
        Assert.Multiple(() =>
        {
            Assert.That(result.Capacity, Is.GreaterThan(0));
            Assert.That(firstResponse, Is.Not.Null);
            Assert.That(string.Equals(firstResponse.UserProfile.display_name, "Josh April"), Is.True);
        });
    }
    [Test]
    public void UserTableReadFailedRead()
    {
        var userTableHandler = new UserTableDatabaseHandler(database);
        List<IAuthenicatedUser> result = userTableHandler.UserRead("invalid session ID");
        Assert.That(result.Capacity, Is.LessThanOrEqualTo(0));
    }

}