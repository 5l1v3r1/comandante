using Microsoft.AspNetCore.Razor.Language;
using System.IO;
using System.Reflection;
using System.Text;

namespace Debug4MvcNetCore
{
    public class EmbeddedPagesRazorProjectItem : RazorProjectItem
    {
        private string _basePath;
        private string _filePath;
        private string _physicalPath;
        private string _layoutPhysicalPath;
        public EmbeddedPagesRazorProjectItem(string path)
        {
            _basePath = "Debug4MvcNetCore.Pages";
            _filePath = path;
            _physicalPath = _basePath + "." + path;
            _layoutPhysicalPath = "Debug4MvcNetCore.Pages._Layout.cshtml";
        }

        public override string BasePath => _basePath;

        public override string FilePath => _filePath;

        public override string PhysicalPath => _physicalPath;

        public override bool Exists => true;

        public override Stream Read()
        {
            var assembly = Assembly.GetExecutingAssembly();
            Stream layoutStream = assembly.GetManifestResourceStream(_layoutPhysicalPath);
            string layout = new StreamReader(layoutStream).ReadToEnd();
            Stream pageBodyStream = assembly.GetManifestResourceStream(_physicalPath);
            string pageBody = new StreamReader(pageBodyStream).ReadToEnd();
            string page = layout.Replace("@RenderBody()", pageBody);
            byte[] pageBytes = Encoding.UTF8.GetBytes(page);
            return new MemoryStream(pageBytes);
        }
    }
}
