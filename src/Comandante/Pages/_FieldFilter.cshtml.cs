using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Comandante.Pages;
using Comandante.PagesRenderer;
using Comandante.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comandante.Pages
{
    public class _FieldFilter : EmbeddedViewModel
    {
        public AppDbContextEntityFieldInfo Model { get; set; }
        public string Value { get; set; }

        public async override Task<EmbededViewResult> InitView()
        {
            if (string.Equals(this.HttpContext.Request.Method, "GET", StringComparison.CurrentCultureIgnoreCase))
            {
                this.Value = this.HttpContext.Request.Query.FirstOrDefault(x => x.Key == Model.Name).Value.FirstOrDefault();
            }
            if(string.Equals(this.HttpContext.Request.Method, "POST", StringComparison.CurrentCultureIgnoreCase))
            {
                this.Value = this.HttpContext.Request.Form.FirstOrDefault(x => x.Key == Model.Name).Value.FirstOrDefault();
            }
            return await View();
        }
    }

}
