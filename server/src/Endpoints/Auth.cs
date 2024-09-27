using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;



namespace AuthRoute
{
    public static class AuthEndpoints 
    {
        public static void Map(this RouteGroupBuilder group)
        {
            group.MapPost("/", AuthRouteHandlers.AuthenicateUser);
        }

    }


    public static class AuthRouteHandlers
    {
        public static async Task<Results< Ok<IAuthenicatedPublicInformation>, BadRequest<string>>> AuthenicateUser(HttpContext context, [FromBody] AuthEndpointJSONRequestBody body)
        {   // oauth2|spotify|spotify:user:1253470477
            try
            {

                

                await context.Session.LoadAsync();
                

                var authenicatedUser = await AuthenicateRetrieveUser(context.Session.Id);

                if (authenicatedUser is null)
                {
                    authenicatedUser ??= await AuthenicateCreateUser(body);
                    await AuthenicateStoreUser(authenicatedUser);
                }

                var publicInformation = AuthenicatedUserRetrievePublicInformation(authenicatedUser);
                
                return await Task.FromResult(TypedResults.Ok(publicInformation));
            } catch (Exception ex)
            {
                return await Task.FromResult(TypedResults.BadRequest("" + ex.Message));
            }
        }

        public static async Task<IAuthenicatedUser> AuthenicateCreateUser(AuthEndpointJSONRequestBody body)
        {
            if(body.auth0ID is null) throw new Exception("Request missing auth0ID");
            var newUser = new AuthenicatedAuth0User(body.auth0ID, Auth0ManagementSystem.Instance);
            await newUser.AuthenicateUser();
            return newUser;
        }
        public static async Task AuthenicateStoreUser(IAuthenicatedUser user)
        {
            if(user is null) throw new Exception("User Object is null");
            return;
        }

        public static async Task<IAuthenicatedUser?> AuthenicateRetrieveUser(string sessionID)
        {
            return null;
        }

        public static IAuthenicatedPublicInformation? AuthenicatedUserRetrievePublicInformation(IAuthenicatedUser authenicatedUser)
        {
            var spotifyUserProfile = new Auth0MangementSpotifyUserProfile(authenicatedUser);
            return spotifyUserProfile.PublicProfile;
        }


    }

    
}





