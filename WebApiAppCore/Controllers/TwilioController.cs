using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using WebApiAppCore.Models;

namespace WebApiAppCore.Controllers
{
    [Route("api/twilio")]
    public class TwilioController : ControllerBase
    {
        private readonly ILoggerManager _logger;
        private readonly IConfigurationRoot _configuration;
        public TwilioController(ILoggerManager logger)
        {
            _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("config.json", false)
            .Build();

            _logger = logger;
        }
        
        //maybe to include DTO message
        [HttpPost]
        public async Task SaveResponse([FromBody] Message message)
        {
            try
            {
                _logger.LogInfo($"Enter SaveResponse method by AccountId {message.AccountId}. ");
                if (message == null)
                {
                    _logger.LogInfo($"Message object sent by client is null.");
                    return;
                }   

                //maybe it should ne configured differently
                var builder = new SqlConnectionStringBuilder();
                builder.ConnectionString = _configuration.GetSection("ConnectionStrings").Value;

                using (var connection = new SqlConnection(builder.ConnectionString))
                {
                    _logger.LogInfo("Connection made to azure sql database.");

                    string query = MakeQuery(message);

                    using (var command = new SqlCommand(query, connection))
                    {
                        _logger.LogInfo("Query database.");
                        connection.Open();
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception was thrown. Exception stack: {ex}");
            }
        }

        private string MakeQuery(Message message)
        {
            _logger.LogInfo($"Making a query. {message.ToString()}");
            return "INSERT INTO [dbo].[TwilioResponse]([AccountId], [Response], [DateCreated], [Active])" +
                $"VALUES ('{message.AccountId}', '{message.Response}' ,'{DateTime.Now}', 1)";
        }
    }
}