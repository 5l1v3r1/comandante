using Microsoft.AspNetCore.Razor.Language;
using System;
using System.Collections.Generic;

namespace Debug4MvcNetCore
{
    public class EmbeddedPagesRazorProject : RazorProjectFileSystem
    {
        public override IEnumerable<RazorProjectItem> EnumerateItems(string basePath)
        {
            throw new NotImplementedException();
        }

        public override RazorProjectItem GetItem(string path)
        {
            return new EmbeddedPagesRazorProjectItem(path);
        }
    }

}
