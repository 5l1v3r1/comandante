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
    public class _FieldEditor : EmbeddedViewModel
    {
        public (DbContextEntityFieldInfo Field, string Value) Model { get; set; }

        public async override Task<EmbededViewResult> Execute()
        {
            return await View();
        }
    }

}
