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
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Comandante.Pages
{
    public class Option : EmbeddedViewModel
    {
        public OptionModel Model { get; set; }

        public override async Task<EmbededViewResult> Execute()
        {
            Model = new OptionModel();
            if (this.HttpContext.Request.Query.ContainsKey("_o"))
            {
                var o = this.HttpContext.Request.Query["_o"].ToString().Trim();
                Model.Option = new OptionsService().GetOption(o);
                Model.OptionInstance = new OptionsService().GetOptionInstance(Model.Option)?.ToJson();
            }
            return await View();
        }
    }

    public class OptionModel
    {
        public OptionInfo Option { get; set; }
        public string OptionInstance { get; set; }
    }
}