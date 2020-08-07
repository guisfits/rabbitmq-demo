using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;

namespace Publisher.Controllers
{
  [Route("api/messages")]
    [ApiController]
    public class MessagesController
    {
        private static Contador _contador = new Contador();

        [HttpGet]
        public object Get()
        {
            return new
            {
                QtdMensagensEnviadas = _contador.ValorAtual
            };
        }

        [HttpPost]
        public object Post([FromServices] RabbitMQConfigurations configurations, Conteudo conteudo)
        {
            lock(_contador)
            {
                _contador.Incrementar();

                var factory = new ConnectionFactory()
                {
                    HostName = configurations.HostName,
                    Port = configurations.Port,
                    UserName = configurations.UserName,
                    Password = configurations.Password
                };

                using(var connection = factory.CreateConnection())
                using(var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "TestesASPNETCore", durable : false, exclusive : false, autoDelete : false, arguments : null);
                    string message = $"{DateTime.Now.ToLongDateString()} - Conte'udo da mensgem: {conteudo.Mensagem}";

                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish(exchange: "", routingKey: "TestesASPNETCore", basicProperties : null, body : body);
                }
            }

            return new
            {
                Resultado = "Mensagem encaminhada com sucesso"
            };
        }
    }
}
