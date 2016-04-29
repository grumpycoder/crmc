using crmc.data;
using crmc.domain;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(web.Startup))]

namespace web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var hubConfiguration = new HubConfiguration
            {
                EnableDetailedErrors = true,
                EnableJSONP = true
            };
            app.MapSignalR(hubConfiguration);
        }

        public static async void Seed()
        {
            var context = new DataContext();
            var userManager = new ApplicationUserManager(new ApplicationUserStore(context));
            var roleManager = new ApplicationRoleManager(new ApplicationRoleStore(context));

            if (roleManager.FindByName("admin") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("admin"));
            }

            if (userManager.FindByName("admin") == null)
            {
                var user = new ApplicationUser()
                {
                    UserName = "admin",
                    FullName = "Administrator",
                    Email = "mark.lawrence@splcenter.org"
                };
                var result = userManager.Create(user, "password");
                //                await userManager.AddToRoleAsync(user.Id, "admin");
            }
        }
    }
}