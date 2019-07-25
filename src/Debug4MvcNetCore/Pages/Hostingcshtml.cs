using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Debug4MvcNetCore.Pages;
using Debug4MvcNetCore.PagesRenderer;
using Microsoft.AspNetCore.Mvc;

namespace Debug4MvcNetCore.Pages
{
    public class Hosting : EmbeddedViewModel
    {
        public HostingModel Model { get; set; }

        public override async Task InitView()
        {
            Model = new HostingModel();
            Model.Environment = new EnvironmentService().Create();
            Model.Process = new ProcessService().GetProcessInfo();
            await Task.CompletedTask;
        }
    }

    public class HostingModel
    {
        public Environments Environment;
        public ProcessInfo Process;
    }

}
