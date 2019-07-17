using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Debug4MvcNetCore.Pages;
using Microsoft.AspNetCore.Mvc;

namespace Debug4MvcNetCore.Pages
{
    public class Index : EmbeddedPageModel
    {
        public IndexModel Model { get; set; }

        public override async Task InitPage()
        {
            Model = new IndexModel();
            Model.FirstName = "Daniel";
        }
    }

    public class IndexModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

}
