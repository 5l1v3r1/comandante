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
    public class EFEntityDesc : EmbeddedViewModel
    {
        public EFEntityDescModel Model { get; set; }

        public override async Task<EmbededViewResult> Execute()
        {
            Model = new EFEntityDescModel();

            var dbContextName = this.HttpContext.Request.Query.FirstOrDefault(x => x.Key == "_dbContext").Value.ToString().Trim();
            var entityName = this.HttpContext.Request.Query.FirstOrDefault(x => x.Key == "_entity").Value.ToString().Trim();
            Model.DbContext = dbContextName;
            Model.Entity = new EntityFrameworkService()
                .GetDbContexts(this.HttpContext)
                .FirstOrDefault(x => x.Name == dbContextName)
                .Entities
                .FirstOrDefault(x => x.ClrTypeName == entityName);
            Model.EntityName = entityName;
            Model.EntityNamePart1 = string.Join(".", entityName?.Split(".").Reverse().Skip(1).Reverse());
            Model.EntityNamePart2 = entityName?.Split(".").Last();
            return await View();
        }
    }

    public class EFEntityDescModel
    {
        public string DbContext;
        public DbContextEntityInfo Entity;
        public string EntityName;
        public string EntityNamePart1;
        public string EntityNamePart2;
    }
}
