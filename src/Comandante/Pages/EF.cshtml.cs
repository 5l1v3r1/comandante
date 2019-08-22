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
    public class EF : EmbeddedViewModel
    {
        public EFModel Model { get; set; }

        public override async Task<EmbededViewResult> InitView()
        {
            Model = new EFModel();

            Model.AppDbContexts = new EntityFrameworkService().GetDbContexts(this.HttpContext);
            return await View();
        }
    }

    public class EFModel
    {
        public List<AppDbContextInfo> AppDbContexts;
    }

}
