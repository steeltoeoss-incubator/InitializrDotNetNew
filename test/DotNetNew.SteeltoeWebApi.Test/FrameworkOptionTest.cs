using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using FluentAssertions;
using Steeltoe.DotNetNew.Test.Utilities.Assertions;
using Steeltoe.DotNetNew.Test.Utilities.Models;
using Xunit;
using Xunit.Abstractions;

namespace Steeltoe.DotNetNew.SteeltoeWebApi.Test
{
    public class FrameworkOptionTest : Test
    {
        public FrameworkOptionTest(ITestOutputHelper logger) : base("framework", logger)
        {
        }

        [Fact]
        public override async void TestHelp()
        {
            using var sandbox = await TemplateSandbox("--help");
            sandbox.CommandOutput.Should().ContainSnippet(@"
-f|--framework  The target framework for the project.
                  net5.0
                  netcoreapp3.1
                  netcoreapp2.1
                Default: net5.0
");
        }

        [Fact]
        public async void TestUnsupported()
        {
            using var sandbox = await TemplateSandbox("unsupported1.0");
            sandbox.CommandError.Should().Contain("'unsupported1.0' is not a valid value for --framework");
        }

        [Theory]
        [InlineData("net5.0")]
        [InlineData("netcoreapp3.1")]
        [InlineData("netcoreapp2.1")]
        public async void TestCsproj(string framework)
        {
            using var sandbox = await TemplateSandbox(framework);
            var xDoc = await sandbox.GetXmlDocumentAsync($"{sandbox.Name}.csproj");
            var expectedFrameworks = new List<string> { framework };
            var actualFrameworks =
            (
                from e in xDoc.Elements().Elements("PropertyGroup").Elements("TargetFramework")
                select e.Value
            ).ToList();
            actualFrameworks.Should().BeEquivalentTo(expectedFrameworks);
            var expectedPackageRefs = framework switch
            {
                "net5.0" => new List<string>
                {
                    "Swashbuckle.AspNetCore",
                },
                "netcoreapp3.1" => new List<string>(),
                "netcoreapp2.1" => new List<string>
                {
                    "Microsoft.AspNetCore",
                    "Microsoft.AspNetCore.CookiePolicy",
                    "Microsoft.AspNetCore.HttpsPolicy",
                    "Microsoft.AspNetCore.Mvc",
                    "Microsoft.AspNetCore.Session",
                    "Microsoft.AspNetCore.StaticFiles",
                },
                _ => throw new ArgumentOutOfRangeException(nameof(framework), framework)
            };
            var actualPackageRefs =
            (
                from e in xDoc.Elements().Elements("ItemGroup").Elements("PackageReference").Attributes("Include")
                select e
            ).ToList().Select(attr => attr.Value).ToList();
            actualPackageRefs.Should().BeEquivalentTo(expectedPackageRefs);
        }

        [Theory]
        [InlineData("net5.0")]
        [InlineData("netcoreapp3.1")]
        [InlineData("netcoreapp2.1")]
        public async void TestProgramCs(string framework)
        {
            using var sandbox = await TemplateSandbox(framework);
            var programSource = await sandbox.GetFileTextAsync("Program.cs");
            switch (framework)
            {
                case "net5.0":
                case "netcoreapp3.1":
                    programSource.Should().ContainSnippet("using Microsoft.AspNetCore.Hosting;");
                    programSource.Should().ContainSnippet("using Microsoft.Extensions.Hosting;");
                    programSource.Should().ContainSnippet("CreateHostBuilder(args).Build().Run();");
                    programSource.Should().ContainSnippet(@"
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
");
                    break;
                case "netcoreapp2.1":
                    programSource.Should().ContainSnippet("using Microsoft.AspNetCore;");
                    programSource.Should().ContainSnippet("using Microsoft.AspNetCore.Hosting;");
                    programSource.Should().ContainSnippet("CreateWebHostBuilder(args).Build().Run();");
                    programSource.Should().ContainSnippet(@"
        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var builder = WebHost.CreateDefaultBuilder(args).UseStartup<Startup>();
            return builder;
        }
");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(framework), framework);
            }
        }

        [Theory]
        [InlineData("net5.0")]
        [InlineData("netcoreapp3.1")]
        [InlineData("netcoreapp2.1")]
        public async void TestStartupCs(string framework)
        {
            using var sandbox = await TemplateSandbox(framework);
            var startupSource = await sandbox.GetFileTextAsync("Startup.cs");
            switch (framework)
            {
                case "net5.0":
                    startupSource.Should().ContainSnippet("using Microsoft.AspNetCore.Builder;");
                    startupSource.Should().ContainSnippet("using Microsoft.AspNetCore.Hosting;");
                    startupSource.Should().ContainSnippet("using Microsoft.Extensions.Configuration;");
                    startupSource.Should().ContainSnippet("using Microsoft.Extensions.DependencyInjection;");
                    startupSource.Should().ContainSnippet("using Microsoft.OpenApi.Models;");
                    startupSource.Should().ContainSnippet("services.AddControllers();");
                    startupSource.Should().ContainSnippet(@"
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc(""v1"", new OpenApiInfo { Title = ""@@NAME@@"", Version = ""v1"" });
        });
".Replace("@@NAME@@", sandbox.Name));
                    startupSource.Should().ContainSnippet(@"
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint(""/swagger/v1/swagger.json"", ""@@NAME@@ v1""));
            }
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
".Replace("@@NAME@@", sandbox.Name));
                    break;
                case "netcoreapp3.1":
                    startupSource.Should().ContainSnippet("using Microsoft.AspNetCore.Builder;");
                    startupSource.Should().ContainSnippet("using Microsoft.AspNetCore.Hosting;");
                    startupSource.Should().ContainSnippet("using Microsoft.Extensions.Configuration;");
                    startupSource.Should().ContainSnippet("using Microsoft.Extensions.DependencyInjection;");
                    startupSource.Should().ContainSnippet("services.AddControllers();");
                    startupSource.Should().ContainSnippet(@"
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) { app.UseDeveloperExceptionPage(); }
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
");
                    break;
                case "netcoreapp2.1":
                    startupSource.Should().ContainSnippet("using Microsoft.AspNetCore.Builder;");
                    startupSource.Should().ContainSnippet("using Microsoft.AspNetCore.Hosting;");
                    startupSource.Should().ContainSnippet("using Microsoft.Extensions.Configuration;");
                    startupSource.Should().ContainSnippet("using Microsoft.Extensions.DependencyInjection;");
                    startupSource.Should().NotContainSnippet("using Microsoft.Extensions.Hosting;");
                    startupSource.Should().ContainSnippet("services.AddMvc();");
                    startupSource.Should().ContainSnippet(@"
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment()) { app.UseDeveloperExceptionPage(); }
            app.UseMvc();
        }
");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(framework), framework);
            }
        }

        [Theory]
        [InlineData("net5.0")]
        [InlineData("netcoreapp3.1")]
        [InlineData("netcoreapp2.1")]
        public async void TestLaunchSettingsJson(string framework)
        {
            using var sandbox = await TemplateSandbox(framework);
            var launchSettings = await sandbox.GetJsonDocumentAsync<LaunchSettings>("Properties/launchSettings.json");
            switch (framework)
            {
                case "net5.0":
                    launchSettings.Profiles["IIS Express"].LaunchUrl.Should().Be("swagger");
                    launchSettings.Profiles[sandbox.Name].LaunchUrl.Should().Be("swagger");
                    break;
                case "netcoreapp3.1":
                case "netcoreapp2.1":
                    launchSettings.Profiles["IIS Express"].LaunchUrl.Should().Be("api/values");
                    launchSettings.Profiles[sandbox.Name].LaunchUrl.Should().Be("api/values");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(framework), framework);
            }
        }
    }
}