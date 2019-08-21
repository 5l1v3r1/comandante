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
    public class EFEntity : EmbeddedViewModel
    {
        public EFEntityModel Model { get; set; }

        public override async Task<EmbededViewResult> InitView()
        {
            Model = new EFEntityModel();

            var appDbContext = this.HttpContext.Request.Query.FirstOrDefault(x => x.Key == "_dbContext").Value.ToString().Trim();
            var entity = this.HttpContext.Request.Query.FirstOrDefault(x => x.Key == "_entity").Value.ToString().Trim();
            var rowsFilter = this.HttpContext.Request.Query
                .Where(x => x.Key.StartsWith("_") == false && string.IsNullOrEmpty(x.Value.FirstOrDefault()) == false)
                .ToDictionary(x => x.Key, x => x.Value.FirstOrDefault()?.ToString());

            Model.AppDbContext = appDbContext;
            Model.Entity = new EntityFrameworkService()
                .GetAppDbContexts(this.HttpContext)
                .FirstOrDefault(x => x.Name == appDbContext)
                .Entities
                .FirstOrDefault(x => x.ClrTypeName == entity);
            Model.Rows = new EntityFrameworkService().GetAll(this.HttpContext, appDbContext, Model.Entity, rowsFilter);
            return await View();
        }
    }

    public class EFEntityModel
    {
        public string AppDbContext;
        public AppDbContextEntityInfo Entity;
        public AppDbContextSqlResults Rows;
    }
}
