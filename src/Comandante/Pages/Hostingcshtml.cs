using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Comandante.Pages;
using Comandante.PagesRenderer;
using Microsoft.AspNetCore.Mvc;

namespace Comandante.Pages
{
    public class Hosting : EmbeddedViewModel
    {
        public HostingModel Model { get; set; }

        public override async Task<EmbededViewResult> Execute()
        {
            Model = new HostingModel();
            Model.Environment = new EnvironmentService().GetEnvironmentInfo();
            Model.Process = new ProcessService().GetProcessInfo();
            return await View();
        }
    }

    public class HostingModel
    {
        public Environments Environment;
        public ProcessInfo Process;
    }

}
