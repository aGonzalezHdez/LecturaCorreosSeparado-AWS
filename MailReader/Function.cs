using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailReader.Model;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace MailReader;

public class Function
{
    public async Task FunctionHandler(object input, ILambdaContext context)
    {
        try
        {
            context.Logger.LogLine("🔄 Iniciando lectura de correos...");
            
            using (var client = new ImapClient())
            {
                client.Connect("imap.gmail.com", 993, true);
                client.Authenticate("agonzalez.mex@grupoei.com.mx", "13wolfiinXP"); // ¡Usa AWS Secrets Manager para credenciales!

                var folder = client.GetFolder("Desarrollo");
                folder.Open(FolderAccess.ReadWrite);

                var correos = folder.Search(SearchQuery.NotSeen);

                foreach (var correo in correos)
                {
                    var message = folder.GetMessage(correo);
                    
                    
                    var correoDatos = new CorreoDTO
                    {
                        Remitente = message.From.Mailboxes.First().Address,
                        Fecha = message.Date.ToString("dd/MM/yyyy"),
                        Hora = message.Date.ToString("HH:mm:ss"),
                        Asunto = message.Subject ?? "Sin asunto",
                        Contenido = message.TextBody ?? "Correo sin contenido."
                    };

                    context.Logger.LogLine($"📩 Nuevo correo detectado de {correoDatos.Remitente}");
                    context.Logger.LogLine($"📅 Fecha: {correoDatos.Fecha} - ⏰ Hora: {correoDatos.Hora}");
                    context.Logger.LogLine($"📝 Asunto: {correoDatos.Asunto}");
                    context.Logger.LogLine($"📝 Contenido: {correoDatos.Contenido}");

                    await EnviarEventoAWS(correoDatos);
                    folder.AddFlags(correo, MessageFlags.Seen, true);
                }

                client.Disconnect(true);
            }

            context.Logger.LogLine("✅ Lectura de correos completada.");
        }
        catch (Exception ex)
        {
            context.Logger.LogLine($"❌ Error: {ex.Message}");
        }
    }

    private async Task EnviarEventoAWS(CorreoDTO correoDatos)
    {
        var snsClient = new AmazonSimpleNotificationServiceClient();
        var publishRequest = new PublishRequest
        {
            TopicArn = "arn:aws:sns:us-east-1:970547342167:CorreoEventos",
            Message = JsonSerializer.Serialize(correoDatos)
        };
        await snsClient.PublishAsync(publishRequest);
    }
}