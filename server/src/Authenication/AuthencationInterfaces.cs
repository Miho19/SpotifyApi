namespace AuthRoute;


public interface IAuthenicatedPublicInformation
{
    string DisplayName {get;}
    string ImageURL {get;}
    string ProviderUserID {get;}
}

public interface IAuthenicatedUser
{
    IAuth0ManagementUserProfileResponse? UserProfile {get; set;}
    string? AccessToken {get;}
    Task AuthenicateUser();
}


public interface IAuthenicationManagementSystem
{
    static readonly bool IsInitialised;
    static IAuthenicationManagementSystem? instance;
    static readonly object? GetLock;
    static IAuthenicationManagementSystem? Instance {get;}
    Task Initialise();
    Task<IAuth0ManagementUserProfileResponse> FetchUser(string UserID);
}

public interface IAuth0ManagementUserProfileResponse
{
    string? display_name {get; set;}
    string? user_id {get; set;}

    
    List<Identities>? identities {get; set;}
    List<Images>? images {get; set;}

}








