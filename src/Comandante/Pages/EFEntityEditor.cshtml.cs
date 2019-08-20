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
    public class EFEntityEditor : EmbeddedViewModel
    {
        public EFEntityEditorModel Model { get; set; }

        public override async Task<EmbededViewResult> InitView()
        {
            Model = new EFEntityEditorModel();

            var appDbContext = this.HttpContext.Request.Query.FirstOrDefault(x => x.Key == "_AppDbContext").Value.ToString().Trim();
            var entityName = this.HttpContext.Request.Query.FirstOrDefault(x => x.Key == "_Entity").Value.ToString().Trim();
            var entity = new EntityFrameworkService()
                .GetAppDbContexts(this.HttpContext)
                .FirstOrDefault(x => x.Name == appDbContext)
                .Entities
                .FirstOrDefault(x => x.ClrTypeName == entityName);

            Model.AppDbContext = appDbContext;
            Model.Entity = entity;

            return await View();
        }
    }

    public class EFEntityEditorModel
    {
        public string AppDbContext;
        public AppDbContextEntityInfo Entity;
    }

   
}
