using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Comandante.PagesRenderer;
using Comandante.Services;

namespace Comandante.Pages
{
    public class Index : EmbeddedViewModel
    {
        public IndexModel Model { get; set; }

        public override async Task<EmbededViewResult> Execute()
        {
            Model = new IndexModel();
            Model.DbContexts = new EntityFrameworkService().GetDbContexts(this.HttpContext).Select(x => x.Name).ToList();

            return await View();
        }
    }

    public class IndexModel
    {
        public List<string> DbContexts;
    }
}
