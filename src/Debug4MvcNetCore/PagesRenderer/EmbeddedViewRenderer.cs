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

namespace Debug4MvcNetCore.PagesRenderer
{
    public class EmbeddedViewRenderer
    {
        public EmbeddedViewRenderer()
        {
 
        }

        public async Task RenderView(string view, HttpContext httpContext)
        {
            await RenderView(view, httpContext, null);
        }

        public async Task RenderView(string view, HttpContext httpContext, object model)
        {
            try
            {
                var fs = new EmbeddedViewRazorProject();
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

                MetadataReference[] metadataReferences =
                    typeof(EmbeddedViewRenderer)
                    .Assembly
                    .GetReferencedAssemblies()
                    .Select(x => GetMetadataReference(x.Name))
                    .Union(new[]
                    {
                    GetMetadataReference(typeof(UrlResolutionTagHelper)),
                    GetMetadataReference(typeof(RazorCompiledItemAttribute)),
                    GetMetadataReference(typeof(IModelExpressionProvider)),
                    GetMetadataReference(typeof(IUrlHelper)),
                    GetMetadataReference(typeof(object)),
                    GetMetadataReference(typeof(DynamicAttribute)),
                    GetMetadataReference(typeof(EmbeddedViewRenderer)),
                    GetMetadataReference(typeof(Microsoft.AspNetCore.Http.HttpContext)),
                    GetMetadataReference(typeof(IHeaderDictionary)),
                    GetMetadataReference(typeof(Microsoft.Extensions.Primitives.StringValues)),
                    GetMetadataReference(typeof(System.Net.IPAddress)),
                    GetMetadataReference(typeof(System.Security.Cryptography.X509Certificates.X509Certificate2)),

                    GetMetadataReference(typeof(System.Security.Claims.ClaimsPrincipal)),
                    
                    //tadataReference.CreateFromFile(typeof(object).Assembly.Location), // include corlib
                    //MetadataReference.CreateFromFile(typeof(RazorCompiledItemAttribute).Assembly.Location), // include Microsoft.AspNetCore.Razor.Runtime
                    MetadataReference.CreateFromFile(Assembly.GetExecutingAssembly().Location), // this file (that contains the MyTemplate base class)

                    // for some reason on .NET core, I need to add this... this is not needed with .NET framework
                    MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), "System.Runtime.dll")),

                    // as found out by @Isantipov, for some other reason on .NET Core for Mac and Linux, we need to add this... this is not needed with .NET framework
                    MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), "netstandard.dll")),
                    })
                    .ToArray();


                const string dllName = "Debug4MvcNetCore.Dynamic";
                var compilation = CSharpCompilation.Create(dllName, new[] { pageTree },
                    metadataReferences,
                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)); // we want a dll


                var assembly = LoadAssembly(compilation);
                await RunAsync(assembly, httpContext, view, model);
            } catch(Exception ex)
            {
                while (ex != null)
                {
                    await httpContext.Response.WriteAsync(ex.Message);
                    await httpContext.Response.WriteAsync(ex.StackTrace);
                    ex = ex.InnerException;
                }
            }
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
                            throw new Exception(formattedMessage);
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

        public async Task RunAsync(Assembly assembly, HttpContext httpContext, string viewName, object model)
        {
            RazorCompiledItemLoader loader = new RazorCompiledItemLoader();
            RazorCompiledItem item = loader.LoadItems(assembly).SingleOrDefault();
            EmbeddedViewModel view = (EmbeddedViewModel)Activator.CreateInstance(item.Type);
            view.HttpContext = httpContext;
            view.ViewName = viewName;
            if (model != null)
                item.Type.GetProperty("Model").SetValue(view, model);
            await view.InitView();
            if (httpContext.Response.StatusCode == 200)
                await view.ExecuteAsync();
        }

        private static MetadataReference GetMetadataReference(Type type) =>
            MetadataReference.CreateFromFile(type.GetTypeInfo().Assembly.Location);

        private static MetadataReference GetMetadataReference(string assemblyName) =>
            MetadataReference.CreateFromFile(Assembly.Load(assemblyName).Location);

    }
}
