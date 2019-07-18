using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Debug4MvcNetCore.Pages;
using Microsoft.AspNetCore.Mvc;

namespace Debug4MvcNetCore.Pages
{
    public class ServerInfo : EmbeddedPageModel
    {
        public ServerInfoModel Model { get; set; }

        public override async Task InitPage()
        {
            Model = new ServerInfoModel();
            Model.Environment = new EnvironmentInfoService().Create();
        }
    }

    public class ServerInfoModel
    {
        public EnvironmentInfo Environment;
    }

}
