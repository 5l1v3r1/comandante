using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Debug4MvcNetCore.Pages;
using Microsoft.AspNetCore.Mvc;

namespace Debug4MvcNetCore.Pages
{
    public class EntityFrameworkCore : EmbeddedPageModel
    {
        public EntityFrameworkCoreModel Model { get; set; }

        public override async Task InitPage()
        {
            Model = new EntityFrameworkCoreModel();
            this.HttpContext.RequestServices.g
        }
    }

    public class EntityFrameworkCoreModel
    {
    }

}
