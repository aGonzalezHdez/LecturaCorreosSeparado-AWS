using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;

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
                folder.Open(FolderAccess.ReadOnly);

                var correos = folder.Search(SearchQuery.NotSeen);

                foreach (var correo in correos)
                {
                    var message = folder.GetMessage(correo);
                    
                    var fechaCompleta = message.Date;
                    var fecha = fechaCompleta.ToString("dd/MM/yyyy"); // Fecha separada
                    var hora = fechaCompleta.ToString("HH:mm:ss"); // Hora separada
                    var remitente = message.From.Mailboxes.First().Address;
                    var contenido = message.TextBody ?? "Correo sin contenido.";

                    context.Logger.LogLine($"📩 Nuevo correo detectado de {remitente}");
                    context.Logger.LogLine($"📅 Fecha: {fecha} - ⏰ Hora: {hora}");
                    context.Logger.LogLine($"📝 Contenido: {contenido}");

                    await EnviarEventoAWS(remitente, fecha, hora, contenido);
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

    private async Task EnviarEventoAWS(string remitente, string fecha, string hora, string contenido)
    {
        var snsClient = new AmazonSimpleNotificationServiceClient();
        var publishRequest = new PublishRequest
        {
            TopicArn = "arn:aws:sns:us-east-1:970547342167:CorreoEventos",
            Message = JsonSerializer.Serialize(new { Remitente = remitente, Fecha = fecha, Hora = hora, Contenido = contenido })
        };
        await snsClient.PublishAsync(publishRequest);
    }

}