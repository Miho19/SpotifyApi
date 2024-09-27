namespace SpotifyApi;

using AuthRoute;



public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        
        builder.Services.AddDistributedMemoryCache();

        builder.Services.AddSession(options => 
        {
            options.IdleTimeout = TimeSpan.FromMinutes(10);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        DotNetEnv.Env.TraversePath().Load();
        
       

        var app = builder.Build();

        app.UseSession();

        var authMapGroup = app.MapGroup("/auth");

        AuthEndpoints.Map(authMapGroup);

        app.Run();
    }
}
