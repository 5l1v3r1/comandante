using System;
using System.Collections.Generic;
using System.Data.Common;
using Debug4MvcNetCore.TestsWeb.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(Debug4MvcNetCore.TestsWeb.Areas.Identity.IdentityHostingStartup))]
namespace Debug4MvcNetCore.TestsWeb.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<Debug4MvcNetCoreTestsWebContext>(options =>
                    options.UseSqlite(
                        context.Configuration.GetConnectionString("Debug4MvcNetCoreTestsWebContextConnection")));

                services.AddDefaultIdentity<IdentityUser>()
                    .AddEntityFrameworkStores<Debug4MvcNetCoreTestsWebContext>();
            });

            //DbConnection dbConnection = new Debug4MvcNetCoreTestsWebContext().Database.GetDbConnection();
            //dbConnection.Open();
            //DbCommand command = dbConnection.CreateCommand();
            //command.CommandText = "ssss a";
            //command.CommandType = System.Data.CommandType.Text;
            //DbDataReader reader = command.ExecuteReader();
            //while (reader.Read())
            //{
            //    for (int i = 0; i < reader.FieldCount; i++)
            //    {
            //        columns.Add(reader.GetName(i));
            //    }
            //}
            
            //new Debug4MvcNetCoreTestsWebContext().Model.GetEntityTypes()[0].
            //Database.ProviderName
            //IEntityType a;
            //a.navi
            //a.Name;
            //a.ClrType.ToString();
        }
    }

}