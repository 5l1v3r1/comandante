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

            var dbContextName = this.HttpContext.Request.Query.FirstOrDefault(x => x.Key == "_dbContext").Value.ToString().Trim();
            var entityName = this.HttpContext.Request.Query.FirstOrDefault(x => x.Key == "_entity").Value.ToString().Trim();
            var page = this.HttpContext.Request.Query.FirstOrDefault(x => x.Key == "_p").Value.ToString().Trim().ToIntOrDefault();
            var rowsFilter = this.HttpContext.Request.Query
                .Where(x => x.Key.StartsWith("_") == false && string.IsNullOrEmpty(x.Value.FirstOrDefault()) == false)
                .ToDictionary(x => x.Key, x => x.Value.FirstOrDefault()?.ToString());

            Model.DbContext = dbContextName;
            Model.Entity = new EntityFrameworkService()
                .GetDbContexts(this.HttpContext)
                .FirstOrDefault(x => x.Name == dbContextName)
                .Entities
                .FirstOrDefault(x => x.ClrTypeName == entityName);
            Model.Rows = new EntityFrameworkService().GetAll(this.HttpContext, dbContextName, Model.Entity, rowsFilter, page);
            Model.Page = page;
            Model.EntityName = entityName;
            Model.EntityNamePart1 = string.Join(".", entityName?.Split(".").Reverse().Skip(1).Reverse());
            Model.EntityNamePart2 = entityName?.Split(".").Last();
            return await View();
        }
    }

    public class EFEntityModel
    {
        public string DbContext;
        public DbContextEntityInfo Entity;
        public DbContextEntitiesResult Rows;
        public int? Page;
        public string EntityName;
        public string EntityNamePart1;
        public string EntityNamePart2;
    }
}
