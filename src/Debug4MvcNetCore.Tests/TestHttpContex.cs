using Microsoft.AspNetCore.Http;
using System.IO;

namespace Debug4MvcNetCore.Tests
{
    public class TestHttpContex : DefaultHttpContext
    {
        public TestHttpContex()
        {
            Response.Body = new MemoryStream();
        }

        public string ResponseHtml
        {
            get
            {
                Response.Body.Seek(0, SeekOrigin.Begin);
                return new StreamReader(Response.Body).ReadToEnd();
            }
        }
    }
}