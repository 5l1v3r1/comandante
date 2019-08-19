using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Comandante.Pages;
using Comandante.PagesRenderer;
using Comandante.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Comandante.Pages
{
    public class EntityFrameworkCoreRunSql : EmbeddedViewModel
    {
        private EntityFrameworkCoreService _entityFrameworkCoreService = new EntityFrameworkCoreService();
        public EntityFrameworkCoreRunSqlModel Model { get; set; }

        public override async Task<EmbededViewResult> InitView()
        {
            Model = new EntityFrameworkCoreRunSqlModel();
            Model.AppDbContexts = _entityFrameworkCoreService.GetAppDbContexts(this.HttpContext);

            if (this.HttpContext.Request.Query.ContainsKey("EFExecutedDbCommand"))
            {
                var efExecutedDbCommand = this.HttpContext.Request.Query["EFExecutedDbCommand"].ToString().Trim();
                var decodedSql = _entityFrameworkCoreService.DecodeSqlFromLogEntry(efExecutedDbCommand);
                return new EmbededViewRedirectResult("/debug/EntityFrameworkCoreRunSql?Sql=" + WebUtility.UrlEncode(decodedSql));
            }

            string appDbContext = null;
            if (this.HttpContext.Request.Query.ContainsKey("AppDbContext"))
                appDbContext = this.HttpContext.Request.Query["AppDbContext"].ToString().Trim();
            Model.AppDbContext = appDbContext;

            if (string.Equals(this.HttpContext.Request.Method, "POST", StringComparison.CurrentCultureIgnoreCase))
            {
                try
                {
                    string sql = this.HttpContext.Request.Form["Sql"];
                    appDbContext = this.HttpContext.Request.Form["AppDbContext"];
                    
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
        public List<AppDbContextInfo> AppDbContexts = new List<AppDbContextInfo>();
    }

}
