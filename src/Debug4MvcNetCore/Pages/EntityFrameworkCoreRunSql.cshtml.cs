using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Debug4MvcNetCore.Pages;
using Debug4MvcNetCore.PagesRenderer;
using Debug4MvcNetCore.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Debug4MvcNetCore.Pages
{
    public class EntityFrameworkCoreRunSql : EmbeddedViewModel
    {
        public EntityFrameworkCoreRunSqlModel Model { get; set; }

        public override async Task<EmbededViewResult> InitView()
        {
            Model = new EntityFrameworkCoreRunSqlModel();
            
            string appDbContext = null;
            if (this.HttpContext.Request.Query.ContainsKey("AppDbContext"))
                appDbContext = this.HttpContext.Request.Query["AppDbContext"].ToString().Trim();
            Model.AppDbContext = appDbContext;

            if (string.Equals(this.HttpContext.Request.Method, "POST", StringComparison.CurrentCultureIgnoreCase))
            {
                try
                {
                    string sql = this.HttpContext.Request.Form["Sql"];
                    var results = new EntityFrameworkCoreService().RunSql(sql, appDbContext, this.HttpContext);
                    return await Json(JsonConvert.SerializeObject(results));
                } catch(Exception ex)
                {
                    return await Json(JsonConvert.SerializeObject(new { Error = ex.Message }));
                }

            }

            return await View();
        }
    }

    public class EntityFrameworkCoreRunSqlModel
    {
        public string AppDbContext;
    }

}
