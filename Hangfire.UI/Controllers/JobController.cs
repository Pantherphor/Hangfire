using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire.Jobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hangfire.UI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class JobController : ControllerBase
    {
        private readonly IBackgroundJobClient _client;

        private readonly ILogger<JobController> _logger;

        public JobController(ILogger<JobController> logger, IBackgroundJobClient client)
        {
            _logger = logger;
            _client = client;
        }

        [HttpGet]
        public Task<bool> Get()
        {

            Random rnd = new Random((int)DateTime.Now.Ticks);
            _client.Enqueue<InsideJob>(i => i.DoJob(rnd.Next(10000, 20000)));

            return Task.FromResult(true);
        }
    }
}
