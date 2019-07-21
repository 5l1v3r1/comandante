using Microsoft.AspNetCore.Razor.Language;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace Debug4MvcNetCore.PagesRenderer
{
    public class EmbeddedViewRazorProjectItem : RazorProjectItem
    {
        private string _basePath;
        private string _fileName;
        private string _filePath;
        private string _layoutPath;
        public EmbeddedViewRazorProjectItem(string fileName)
        {
            _basePath = "Debug4MvcNetCore.Pages";
            _fileName = fileName;
            _filePath = _basePath + "." + fileName;
            _layoutPath = _basePath + "." + "_Layout.cshtml";
        }

        public override string BasePath => _basePath;

        public override string FilePath => _fileName;

        public override string PhysicalPath => _filePath;

        public override bool Exists => true;

        public override Stream Read()
        {
            var assembly = Assembly.GetExecutingAssembly();
            if (_fileName.StartsWith("_")) // if partial view
            {
                return assembly.GetManifestResourceStream(_filePath);
            }
            // else page
            using (Stream layoutStream = assembly.GetManifestResourceStream(_layoutPath))
            using (Stream pageBodyStream = assembly.GetManifestResourceStream(_filePath))
            {
                string layout = new StreamReader(layoutStream).ReadToEnd();
                string pageBody = new StreamReader(pageBodyStream).ReadToEnd();
                string page = layout.Replace("@RenderBody()", pageBody);
                byte[] pageBytes = Encoding.UTF8.GetBytes(page);
                return new MemoryStream(pageBytes);
            }
            throw new Exception("View not found: " + _fileName);
        }
    }
}
