using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Comandante.Pages;
using Comandante.PagesRenderer;
using Comandante.Services;
using Microsoft.AspNetCore.Mvc;

namespace Comandante.Pages
{
    public class EntityFrameworkCore : EmbeddedViewModel
    {
        public EntityFrameworkCoreModel Model { get; set; }

        public override async Task<EmbededViewResult> InitView()
        {
            Model = new EntityFrameworkCoreModel();

            Model.AppDbContexts = new EntityFrameworkCoreService().GetAppDbContexts(this.HttpContext);
            return await View();
        }
    }

    public class EntityFrameworkCoreModel
    {
        public List<AppDbContextInfo> AppDbContexts;
    }

}
