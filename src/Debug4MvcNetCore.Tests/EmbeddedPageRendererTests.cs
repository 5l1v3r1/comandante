using NUnit.Framework;
using System;
using Debug4MvcNetCore;

namespace Debug4MvcNetCore.Tests
{
    public class EmbeddedPageRendererTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void RenderViewToString()
        {
            EmbeddedPageRenderer sut = new EmbeddedPageRenderer();

            var httpContext = new TestHttpContex();
            sut.Render("Index", httpContext).Wait();

            Console.Write(httpContext.ResponseHtml);
        }
    }
}