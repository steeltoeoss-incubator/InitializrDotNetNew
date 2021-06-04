using System.Collections.Generic;
using Steeltoe.NetCoreTool.Template.WebApi.Test.Utils;
using Xunit.Abstractions;

namespace Steeltoe.NetCoreTool.Template.WebApi.Test
{
    public class RandomValueProjectOptionTest : ProjectOptionTest
    {
        public RandomValueProjectOptionTest(ITestOutputHelper logger) : base("random-value",
            "Add a random value configuration source", logger)
        {
        }

        protected override void AssertCsprojPackagesHook(SteeltoeVersion steeltoeVersion, Framework framework,
            List<(string, string)> packages)
        {
            packages.Add(("Steeltoe.Extensions.Configuration.RandomValueBase", "$(SteeltoeVersion)"));
        }

        protected override void AssertValuesControllerCsSnippetsHook(SteeltoeVersion steeltoeVersion, Framework framework,
            List<string> snippets)
        {
            snippets.Add("using Microsoft.Extensions.Configuration;");
            snippets.Add("private readonly IConfiguration _configuration;");
            snippets.Add(@"
[HttpGet]
public ActionResult<IEnumerable<string>> Get()
{
    var val1 = _configuration[""random:int""];
    var val2 = _configuration[""random:uuid""];
    var val3 = _configuration[""random:string""];
    return new[] { val1, val2, val3 };
}
");
        }
    }
}