using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Debug4MvcNetCore.Pages;
using Debug4MvcNetCore.PagesRenderer;
using Debug4MvcNetCore.Services;
using Microsoft.AspNetCore.Mvc;

namespace Debug4MvcNetCore.Pages
{
    public class EntityFrameworkCoreDbContext : EmbeddedViewModel
    {
        public EntityFrameworkCoreDbContextModel Model { get; set; }

        public override async Task<EmbededViewResult> InitView()
        {
            Model = new EntityFrameworkCoreDbContextModel();

            var appDbContext = this.HttpContext.Request.Query.FirstOrDefault(x => x.Key == "AppDbContext").Value.ToString().Trim();

            Model.AppDbContext = new EntityFrameworkCoreService()
                .GetAppDbContexts(this.HttpContext)
                .FirstOrDefault(x => x.Name == appDbContext);
            return await View();
        }
    }

    public class EntityFrameworkCoreDbContextModel
    {
        public AppDbContextInfo AppDbContext;
    }

}
