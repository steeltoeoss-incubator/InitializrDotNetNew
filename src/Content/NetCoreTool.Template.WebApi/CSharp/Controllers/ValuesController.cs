#if (!ConnectorRabbitMqOption)
using System.Collections.Generic;
#endif
#if (AnySqlDatabase)
using System.Data;
#endif
#if (ConnectorSqlServerOption)
using System.Data.SqlClient;
#endif
#if (ConnectorRabbitMqOption)
using System.Text;
using System.Threading;
#endif
#if (ConnectorRedisOption)
using System.Threading.Tasks;
#endif
using Microsoft.AspNetCore.Mvc;
#if (ConnectorRedisOption)
using Microsoft.Extensions.Caching.Distributed;
#endif
#if (AnyConfigurator)
using Microsoft.Extensions.Configuration;
#endif
#if (ConnectorRabbitMqOption)
using Microsoft.Extensions.Logging;
#endif
#if (HostingCloudFoundryOption)
using Microsoft.Extensions.Options;
#endif
#if (ConnectorMongoDbOption)
using MongoDB.Driver;
#endif
#if (ConnectorMySqlOption)
using MySql.Data.MySqlClient;
#endif
#if (ConnectorPostgreSqlOption)
using Npgsql;
#endif
#if (ConnectorRabbitMqOption)
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
#endif
#if (HostingCloudFoundryOption)
using Steeltoe.Extensions.Configuration.CloudFoundry;
#endif

namespace Company.WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
#if (HostingCloudFoundryOption)
        private readonly CloudFoundryApplicationOptions _appOptions;

        public ValuesController(IOptions<CloudFoundryApplicationOptions> appOptions)
        {
            _appOptions = appOptions.Value;
        }
#endif
#if (AnyConfigurator)
        private readonly IConfiguration _configuration;

        public ValuesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
#endif
#if (ConnectorMongoDbOption)
        private readonly IMongoClient _mongoClient;
        private readonly MongoUrl _mongoUrl;

        public ValuesController(IMongoClient mongoClient, MongoUrl mongoUrl)
        {
            _mongoClient = mongoClient;
            _mongoUrl = mongoUrl;
        }
#endif
#if (ConnectorRabbitMqOption)
        private readonly ILogger _logger;
        private readonly ConnectionFactory _factory;
        private const string QueueName = "my-queue";

        public ValuesController(ILogger<ValuesController> logger, [FromServices] ConnectionFactory factory)
        {
            _logger = logger;
            _factory = factory;
        }
#endif
#if (ConnectorRedisOption)
        private readonly IDistributedCache _cache;

        public ValuesController(IDistributedCache cache)
        {
            _cache = cache;
        }
#endif
#if (AnySqlDatabase)
#if (ConnectorSqlServerOption)
        private readonly SqlConnection _dbConnection;

        public ValuesController([FromServices] SqlConnection dbConnection)
#endif
#if (ConnectorMySqlOption)
        private readonly MySqlConnection _dbConnection;

        public ValuesController([FromServices] MySqlConnection dbConnection)
#endif
#if (ConnectorPostgreSqlOption)
        private readonly NpgsqlConnection _dbConnection;

        public ValuesController([FromServices] NpgsqlConnection dbConnection)
#endif
        {
            _dbConnection = dbConnection;
        }
#endif

        [HttpGet]
#if (ConnectorRabbitMqOption)
        public ActionResult<string> Get()
        {
            using (var connection = _factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                // the queue
                channel.QueueDeclare(
                    queue: QueueName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
                // consumer
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    string msg = Encoding.UTF8.GetString(ea.Body);
                    _logger.LogInformation("Received message: {Message}", msg);
                };
                channel.BasicConsume(
                    queue: QueueName,
                    autoAck: true,
                    consumer: consumer);
                // publisher
                int i = 0;
                while (i < 5) // write a message every second, for 5 seconds
                {
                    var body = Encoding.UTF8.GetBytes($"Message {++i}");
                    channel.BasicPublish(
                        exchange: "",
                        routingKey: QueueName,
                        basicProperties: null,
                        body: body);
                    Thread.Sleep(1000);
                }
            }

            return "Wrote 5 message to the info log. Have a look!";
#elif (ConnectorRedisOption)
        public async Task<IEnumerable<string>> Get()
        {
            await _cache.SetStringAsync("MyValue1", "123");
            await _cache.SetStringAsync("MyValue2", "456");
            string myval1 = await _cache.GetStringAsync("MyValue1");
            string myval2 = await _cache.GetStringAsync("MyValue2");

            return new[] { myval1, myval2};
#else
        public ActionResult<IEnumerable<string>> Get()
        {
#if (ConfigurationCloudConfigOption)
            var val1 = _configuration["Value1"];
            var val2 = _configuration["Value2"];

            return new[] { val1, val2 };
#elif (HostingCloudFoundryOption)
            string appName = _appOptions.ApplicationName;
            string appInstance = _appOptions.ApplicationId;

            return new[] { appInstance, appName };
#elif (ConnectorMongoDbOption)
            List<string> listing = _mongoClient.ListDatabaseNames().ToList();
            listing.Insert(0, _mongoUrl.Url);
            return listing;
#elif (AnySqlDatabase)
            List<string> tables = new List<string>();
            _dbConnection.Open();
            DataTable dt = _dbConnection.GetSchema("Tables");
            _dbConnection.Close();
            foreach (DataRow row in dt.Rows)
            {
                string tablename = (string)row[2];
                tables.Add(tablename);
            }

            return tables;
#elif (ConfigurationPlaceholderOption)
            var val1 = _configuration["ResolvedPlaceholderFromEnvVariables"];
            var val2 = _configuration["UnresolvedPlaceholder"];
            var val3 = _configuration["ResolvedPlaceholderFromJson"];

            return new[] { val1, val2, val3 };
#elif (ConfigurationRandomValueOption)
            var val1 = _configuration["random:int"];
            var val2 = _configuration["random:uuid"];
            var val3 = _configuration["random:string"];

            return new[] { val1, val2, val3 };
#else
            return new[] { "value" };
#endif
#endif
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
