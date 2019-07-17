using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Debug4MvcNetCore
{
    public class EmbeddedPageRenderer
    {
        public EmbeddedPageRenderer()
        {
 
        }

        public async Task Render(string view, HttpContext httpContext)
        {
            var fs = new EmbeddedPagesRazorProject();
            var engine = RazorProjectEngine.Create(RazorConfiguration.Default, fs, (builder) =>
            {
                InheritsDirective.Register(builder);
                builder.SetNamespace("Debug4MvcNetCore");
                builder.SetBaseType("Debug4MvcNetCore.Pages." + view);
            });

            var page = fs.GetItem(view + ".cshtml");
            var pageCodeDocument = engine.Process(page);
            var pageCs = pageCodeDocument.GetCSharpDocument();
            var pageTree = CSharpSyntaxTree.ParseText(pageCs.GeneratedCode);

            const string dllName = "Debug4MvcNetCore.Dynamic";
            var compilation = CSharpCompilation.Create(dllName, new[] { pageTree},
                new[]
                {
                    //GetMetadataReference(typeof(InputTagHelper)),
                    GetMetadataReference(typeof(UrlResolutionTagHelper)),
                    GetMetadataReference(typeof(RazorCompiledItemAttribute)),
                    GetMetadataReference(typeof(IModelExpressionProvider)),
                    GetMetadataReference(typeof(IUrlHelper)),
                    GetMetadataReference(typeof(object)),
                    GetMetadataReference(typeof(DynamicAttribute)),
                    GetMetadataReference(typeof(EmbeddedPageRenderer)),

                    //tadataReference.CreateFromFile(typeof(object).Assembly.Location), // include corlib
                    //MetadataReference.CreateFromFile(typeof(RazorCompiledItemAttribute).Assembly.Location), // include Microsoft.AspNetCore.Razor.Runtime
                    //MetadataReference.CreateFromFile(Assembly.GetExecutingAssembly().Location), // this file (that contains the MyTemplate base class)

                    // for some reason on .NET core, I need to add this... this is not needed with .NET framework
                    MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), "System.Runtime.dll")),

                    // as found out by @Isantipov, for some other reason on .NET Core for Mac and Linux, we need to add this... this is not needed with .NET framework
                    MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), "netstandard.dll")),
                    
                },
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)); // we want a dll


            var assembly = LoadAssembly(compilation);
            await RunAsync(assembly, httpContext);
        }

        public Assembly LoadAssembly(CSharpCompilation compilation)
        {
            Assembly assembly;
            EmitOptions emitOptions = new EmitOptions(debugInformationFormat: DebugInformationFormat.PortablePdb);
            using (MemoryStream assemblyStream = new MemoryStream())
            {
                using (MemoryStream pdbStream = new MemoryStream())
                {
                    EmitResult result = compilation.Emit(
                        assemblyStream,
                        pdbStream,
                        options: emitOptions);

                    if (!result.Success)
                    {
                        List<Diagnostic> errorsDiagnostics = result.Diagnostics
                            .Where(d => d.IsWarningAsError || d.Severity == DiagnosticSeverity.Error)
                            .ToList();
                        foreach (Diagnostic diagnostic in errorsDiagnostics)
                        {
                            FileLinePositionSpan lineSpan =
                                diagnostic.Location.SourceTree.GetMappedLineSpan(
                                    diagnostic.Location.SourceSpan);
                            string errorMessage = diagnostic.GetMessage();
                            string formattedMessage =
                                "("
                                + lineSpan.StartLinePosition.Line.ToString()
                                + ":"
                                + lineSpan.StartLinePosition.Character.ToString()
                                + ") "
                                + errorMessage;
                            Console.WriteLine(formattedMessage);
                        }
                        return null;
                    }

                    assemblyStream.Seek(0, SeekOrigin.Begin);
                    pdbStream.Seek(0, SeekOrigin.Begin);

                    assembly = Assembly.Load(assemblyStream.ToArray(), pdbStream.ToArray());
                }
            }
            return assembly;
        }

        public async Task RunAsync(Assembly assembly, HttpContext httpContext)
        {
            RazorCompiledItemLoader loader = new RazorCompiledItemLoader();
            RazorCompiledItem item = loader.LoadItems(assembly).SingleOrDefault();
            EmbeddedPageModel page = (EmbeddedPageModel)Activator.CreateInstance(item.Type);
            page.HttpContext = httpContext;
            await page.InitPage();
            await page.ExecuteAsync();
        }

        private static MetadataReference GetMetadataReference(Type type) =>
            MetadataReference.CreateFromFile(type.GetTypeInfo().Assembly.Location);

        private static MetadataReference GetMetadataReference(string assemblyName) =>
            MetadataReference.CreateFromFile(Assembly.Load(assemblyName).Location);

    }
}
