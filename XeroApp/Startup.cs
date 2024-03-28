using Microsoft.AspNetCore.Authentication.Cookies;
using XeroApp.Services;


namespace XeroApp
{

    public class Startup
    {
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // You can set your session timeout
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true; // Make the session cookie essential
            });
            services.AddScoped<IXeroService, XeroService>();
            // Configure cookie authentication
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Authentication/Login"; // Adjust this as necessary
                    options.LogoutPath = "/Authentication/Disconnect";
                });
            
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseRouting();

            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllers(); // For attribute routing
            });
        }
    }
}