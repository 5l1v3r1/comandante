using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Debug4MvcNetCore.Pages;
using Debug4MvcNetCore.PagesRenderer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Debug4MvcNetCore.Pages
{
    public class _Request : EmbeddedViewModel
    {
        public RequestInfo Model { get; set; }

        public override Task InitView()
        {
            return Task.CompletedTask;
        }
    }
}
