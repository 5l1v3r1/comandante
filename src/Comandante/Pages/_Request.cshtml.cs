using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Comandante.Pages;
using Comandante.PagesRenderer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comandante.Pages
{
    public class _Request : EmbeddedViewModel
    {
        public RequestResponseInfo Model { get; set; }

        public async override Task<EmbededViewResult> InitView()
        {
            return await View();
        }
    }
}
