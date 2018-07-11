using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;
using IEPprojekat.Models;
using System.Linq;

[assembly: OwinStartupAttribute(typeof(IEPprojekat.Startup))]
namespace IEPprojekat
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            createRolesAndUsers();
            app.MapSignalR();
        }

        private void createRolesAndUsers()
        {
            ApplicationDbContext context = new ApplicationDbContext();

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            if (!roleManager.RoleExists("Administrator"))
            {
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "Administrator";
                roleManager.Create(role);

                var user = new ApplicationUser();
                user.UserName = "tjovanovic510@gmail.com";
                user.Email = "tjovanovic510@gmail.com";
                user.FirstName = "Tijana";
                user.LastName = "Jovanovic";
                user.Role = 1;
                user.NumberOfTokens = 0;

                string adminPassword = "123456";

                var admin = UserManager.Create(user, adminPassword);

                if (admin.Succeeded)
                {
                    var adminAdded = UserManager.AddToRole(user.Id, "Administrator");
                }
            }
            Auctions db = new Auctions();
            if (!db.InformationsForAdministrator.Any())
            {

                var info = new InformationsForAdministrator();
                info.Currency = "RSD";
                info.GoldPack = 50;
                info.SilverPack = 30;
                info.PlatinumPack = 100;
                info.ItemsPerPage = 10;
                info.ValueToken = 10;

                db.InformationsForAdministrator.Add(info);
                db.SaveChanges();
            }


            if (!roleManager.RoleExists("RegularUser"))
            {
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "RegularUser";
                roleManager.Create(role);
            }

        }
    }
}

