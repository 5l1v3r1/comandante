using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Debug4MvcNetCore.Pages;
using Debug4MvcNetCore.PagesRenderer;
using Microsoft.AspNetCore.Mvc;

namespace Debug4MvcNetCore.Pages
{
    public class EntityFrameworkCore : EmbeddedViewModel
    {
        public EntityFrameworkCoreModel Model { get; set; }

        public override async Task InitView()
        {
            Model = new EntityFrameworkCoreModel();
            await Task.CompletedTask;
        }
    }

    public class EntityFrameworkCoreModel
    {
    }

}
