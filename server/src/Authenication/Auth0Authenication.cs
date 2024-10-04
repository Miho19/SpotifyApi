using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace AuthRoute;

public class Auth0MangementSpotifyUserProfile(IAuthenicatedUser AuthenicatedUser)
{
    

    public IAuthenicatedPublicInformation? PublicProfile
    {
        get 
        {
            if(AuthenicatedUser.UserProfile == null) throw new Exception("Auth0Management User Profile does not exist");
            
            var spotifyProfile = new SpotifyPublicUserInformation();
            spotifyProfile.DisplayName = AuthenicatedUser.UserProfile?.display_name ?? "";
            spotifyProfile.ImageURL = AuthenicatedUser.UserProfile?.images?[0].url ?? "";
            spotifyProfile.ProviderUserID = AuthenicatedUser.UserProfile?.user_id ?? "";
            
            return spotifyProfile;
        }
    }
}


 public class AuthenicatedAuth0User : IAuthenicatedUser
 {

    public IAuth0ManagementUserProfileResponse? UserProfile {get; set;}

    private IAuthenicationManagementSystem AuthenicationManagementSystem;

    public string Auth0UserID {get;} = string.Empty;

    public string? AccessToken
    {
        get 
        {
            return UserProfile?.identities?[0].access_token;
        }
        
    }

    public AuthenicatedAuth0User(string UserID, IAuthenicationManagementSystem AuthenicationManagementSystem)
    {
        this.Auth0UserID = UserID;
        this.AuthenicationManagementSystem = AuthenicationManagementSystem;
    }

    public async Task AuthenicateUser()
    {
        var response = await AuthenicationManagementSystem.FetchUser(this.Auth0UserID);
        this.UserProfile = response;
    }

}

public class SpotifyPublicUserInformation : IAuthenicatedPublicInformation
{
    public string DisplayName {get; set;} = string.Empty;
    public string ImageURL {get; set;} = string.Empty;
    public string ProviderUserID {get; set;} = string.Empty;
}


/** 
    https://csharpindepth.com/articles/Singleton 
    Thread safe singleton
*/
public class Auth0ManagementSystem : IAuthenicationManagementSystem
{
    public readonly bool IsInitialised;
    private static Auth0ManagementSystem? instance = null;
    private static readonly object GetLock = new object();

    private static readonly HttpClient _HttpClient = new HttpClient();

    private string? AccessToken;

    Auth0ManagementSystem()
    {
    }

    public static Auth0ManagementSystem Instance
    {
        get 
        {
            lock (GetLock) 
            {
                if (instance == null)
                {
                    instance = new Auth0ManagementSystem();
                }
                return instance;
            }
        }
    }

// https://www.dofactory.com/code-examples/csharp/content-type-header
    public  async Task Initialise()
    {
        
        var Auth0URL = System.Environment.GetEnvironmentVariable("Auth0URL");
        var Auth0ClientID = System.Environment.GetEnvironmentVariable("Auth0ClientID");
        var Auth0ClientSecret = System.Environment.GetEnvironmentVariable("Auth0ClientSecret");

        if(this.IsInitialised == true)
        {
            return;
        }

        try
        {

            _HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var headers = new {
                client_id = Auth0ClientID,
                client_secret = Auth0ClientSecret,
                audience = Auth0URL + "/api/v2/",
                grant_type = "client_credentials",
            };

            var stringHeaders = JsonConvert.SerializeObject(headers);
            var requestContent = new StringContent(stringHeaders, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, Auth0URL + "/oauth/token");
            request.Content = requestContent;
            var auth0Response = await _HttpClient.SendAsync(request);

           
            var jsonString = await auth0Response.Content.ReadAsStringAsync();
            
            var auth0ResponseObject = JsonConvert.DeserializeObject<Auth0ManagementResponse>(jsonString);
            
            this.AccessToken = auth0ResponseObject?.access_token;
            return;
        }
        catch
        {
            throw new Exception("Error initialising Auth0ManagementSystem");
        }

    }

    public async Task<IAuth0ManagementUserProfileResponse> FetchUser(string UserID)
    {

        if(string.IsNullOrEmpty(UserID)) throw new Exception("Missing User ID");
        

        if (!this.IsInitialised) await this.Initialise();

        _HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.AccessToken);
        

        var Auth0URL = System.Environment.GetEnvironmentVariable("Auth0URL");
       

        try 
        {
            var request = new HttpRequestMessage(HttpMethod.Get, Auth0URL + "/api/v2/users/" + UserID);
            var auth0Response = await _HttpClient.SendAsync(request);
            var jsonBody = await auth0Response.Content.ReadAsStringAsync();
            var userProfile = JsonConvert.DeserializeObject<Auth0MangementUserProfileResponse>(jsonBody);
            return userProfile ?? throw new Exception("Failed to deserialize response into class object");
        }
        catch
        {
            throw new Exception("Failed to get Auth0 User Profile");
        }
        
        
    }
}


public class Auth0ManagementResponse
{
    public string? access_token;
}


public class Auth0MangementUserProfileResponse : IAuth0ManagementUserProfileResponse
{
    public string? display_name {get; set;}
    public string? user_id {get; set;}
    public List<Identities>? identities {get; set;}
    public List<Images>? images {get; set;}
   
}
public class Identities
{
    public string? access_token;
}

public class Images
{
    public string? url;
}

public class AuthEndpointJSONRequestBody
{
    public string? auth0ID {get; set;}
}

