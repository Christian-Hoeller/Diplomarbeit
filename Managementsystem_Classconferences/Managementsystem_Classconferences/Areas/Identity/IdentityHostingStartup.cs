using System;
using Managementsystem_Classconferences.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(Managementsystem_Classconferences.Areas.Identity.IdentityHostingStartup))]
namespace Managementsystem_Classconferences.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<Managementsystem_ClassconferencesContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("Managementsystem_ClassconferencesContextConnection")));

                services.AddDefaultIdentity<IdentityUser>()
                    .AddEntityFrameworkStores<Managementsystem_ClassconferencesContext>();
            });
        }
    }
}