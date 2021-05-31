using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Steeltoe.DotNetNew.SteeltoeWebApi.Test.Utils;
using Steeltoe.DotNetNew.Test.Utilities.Assertions;
using Xunit;
using Xunit.Abstractions;

namespace Steeltoe.DotNetNew.SteeltoeWebApi.Test
{
    public class HystrixProjectOptionTest : ProjectOptionTest
    {
        public HystrixProjectOptionTest(ITestOutputHelper logger) : base("hystrix",
            "Add support for Netflix Hystrix, a latency and fault tolerance library", logger)
        {
        }

        [Fact]
        [Trait("Category", "ProjectGeneration")]
        public async void TestDefaultNotPolluted()
        {
            using var sandbox = await TemplateSandbox("false");
            sandbox.FileExists("HelloHystrixCommand.cs").Should().BeFalse();
        }

        protected override async Task AssertProjectGeneration(SteeltoeVersion steeltoeVersion, Framework framework)
        {
            await base.AssertProjectGeneration(steeltoeVersion, framework);
            Logger.WriteLine("asserting HelloHystrixCommand.cs");
            var source = await Sandbox.GetFileTextAsync("HelloHystrixCommand.cs");
            source.Should().ContainSnippet("public sealed class HelloHystrixCommand : HystrixCommand<string>");
            source.Should()
                .ContainSnippet(
                    "public HelloHystrixCommand(string name) : base(HystrixCommandGroupKeyDefault.AsKey(\"MyCircuitBreakers\"))");
        }

        protected override void AssertCsprojPackagesHook(SteeltoeVersion steeltoeVersion, Framework framework,
            List<(string, string)> packages)
        {
            packages.Add(("Steeltoe.CircuitBreaker.HystrixCore", "$(SteeltoeVersion)"));
            packages.Add(("Steeltoe.CircuitBreaker.Hystrix.MetricsStreamCore", "$(SteeltoeVersion)"));
        }

        protected override void AssertStartupCsSnippetsHook(SteeltoeVersion steeltoeVersion, Framework framework,
            List<string> snippets)
        {
            snippets.Add("services.AddHystrixCommand<HelloHystrixCommand>(\"MyCircuitBreakers\", Configuration);");
            snippets.Add("services.AddHystrixMetricsStream(Configuration);");
            snippets.Add("app.UseHystrixRequestContext();");
            snippets.Add("app.UseHystrixMetricsStream();");
        }
    }
}