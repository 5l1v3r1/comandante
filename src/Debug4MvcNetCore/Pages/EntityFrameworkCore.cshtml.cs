using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Debug4MvcNetCore.Pages;
using Debug4MvcNetCore.PagesRenderer;
using Debug4MvcNetCore.Services;
using Microsoft.AspNetCore.Mvc;

namespace Debug4MvcNetCore.Pages
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
