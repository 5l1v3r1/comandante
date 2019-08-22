using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Comandante.Pages;
using Comandante.PagesRenderer;
using Comandante.Services;
using Microsoft.AspNetCore.Mvc;

namespace Comandante.Pages
{
    public class EFContext : EmbeddedViewModel
    {
        public EFContextModel Model { get; set; }

        public override async Task<EmbededViewResult> InitView()
        {
            Model = new EFContextModel();

            var dbContext = this.HttpContext.Request.Query.FirstOrDefault(x => x.Key == "_dbContext").Value.ToString().Trim();

            Model.DbContext = dbContext;
            Model.AppDbContext = new EntityFrameworkService()
                .GetDbContexts(this.HttpContext)
                .FirstOrDefault(x => x.Name == dbContext);
            return await View();
        }
    }

    public class EFContextModel
    {
        public AppDbContextInfo AppDbContext;
        public string DbContext;
    }

}
