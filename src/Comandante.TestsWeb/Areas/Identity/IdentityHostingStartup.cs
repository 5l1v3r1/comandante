using System;
using System.Collections.Generic;
using System.Data.Common;
using Comandante.TestsWeb.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(Comandante.TestsWeb.Areas.Identity.IdentityHostingStartup))]
namespace Comandante.TestsWeb.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<ComandanteTestsWebContext>(options =>
            {
                
                options.UseSqlite(
                    context.Configuration.GetConnectionString("ComandanteTestsWebContextConnection"));
                options.EnableSensitiveDataLogging();

              }          
            );

                

                services.AddDefaultIdentity<IdentityUser>()
                    .AddEntityFrameworkStores<ComandanteTestsWebContext>();
            });



            //ComandanteTestsWebContext a;
            //a.Set<IdentityUser>().ToList
            //a
            //EntityFrameworkQueryableExtensions.AllAsync()
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

            // var e = new Debug4MvcNetCoreTestsWebContext().Model.GetEntityTypes().ToLIst();
            //Database.ProviderName
            //IEntityType a;
            //a.Relational();
            //a.Relational
            //IRelationalEntityTypeAnnotations b;


            //a.navi
            //a.Name;
            //a.ClrType.ToString();
        }
    }

}