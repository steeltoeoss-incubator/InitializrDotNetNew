#if (MessagingRabbitMqOption || MessagingRabbitMqClientOption)
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Steeltoe.Messaging.RabbitMQ.Core;

namespace Company.WebApplication.CS
{
    [ApiController]
    [Route("[controller]")]
    public class WriteMessageController : ControllerBase
    {
        private const string ReceiveAndConvertQueue = "steeltoe_message_queue";

        private readonly ILogger<WriteMessageController> _logger;
        private readonly RabbitTemplate _rabbitTemplate;
        private readonly RabbitAdmin _rabbitAdmin;

        public WriteMessageController(ILogger<WriteMessageController> logger, RabbitTemplate rabbitTemplate, RabbitAdmin rabbitAdmin)
        {
            _logger = logger;
            _rabbitTemplate = rabbitTemplate;
            _rabbitAdmin = rabbitAdmin;
        }

        [HttpGet()]
        public ActionResult<string> Index()
        {
            var msg = "Hi there from over here.";

            _rabbitTemplate.ConvertAndSend(ReceiveAndConvertQueue, msg);

            _logger.LogInformation($"Sending message '{msg}' to queue '{ReceiveAndConvertQueue}'");

            return "Message sent to queue.";
        }
    }
}
#endif