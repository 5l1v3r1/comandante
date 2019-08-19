using NUnit.Framework;
using System;
using Comandante;
using Comandante.PagesRenderer;

namespace Comandante.Tests
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
            EmbeddedViewRenderer sut = new EmbeddedViewRenderer();

            var httpContext = new TestHttpContex();
            sut.RenderView("Index", httpContext).Wait();

            Console.Write(httpContext.ResponseHtml);
        }
    }
}