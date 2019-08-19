using Microsoft.AspNetCore.Razor.Language;
using System;
using System.Collections.Generic;

namespace Comandante.PagesRenderer
{
    public class EmbeddedViewRazorProject : RazorProjectFileSystem
    {
        public override IEnumerable<RazorProjectItem> EnumerateItems(string basePath)
        {
            throw new NotImplementedException();
        }

        public override RazorProjectItem GetItem(string path)
        {
            return new EmbeddedViewRazorProjectItem(path);
        }
    }

}
