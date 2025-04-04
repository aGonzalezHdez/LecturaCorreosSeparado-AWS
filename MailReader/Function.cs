using Amazon.Lambda.Core;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailReader.Model;
using Newtonsoft.Json;

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
                var imapServer = Environment.GetEnvironmentVariable("IMAP_SERVER");
                client.Connect(imapServer, 993, true);
                
                var (email, password) = await ObtenerCredencialesGmail(); // ✅ Obtener credenciales de AWS Secrets
                client.Authenticate(email, password); // ✅ Seguridad mejorada

                var folder = client.GetFolder("Desarrollo");
                folder.Open(FolderAccess.ReadWrite);

                var correos = folder.Search(SearchQuery.NotSeen);

                foreach (var correo in correos)
                {
                    var message = folder.GetMessage(correo);
                 
                    
                    var fechaUtc = message.Date.UtcDateTime;
                    var zonaLocal = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"); // 📌 Cambia por la zona que necesitas
                    var fechaLocal = TimeZoneInfo.ConvertTimeFromUtc(fechaUtc, zonaLocal);
                    
                    if (zonaLocal.IsDaylightSavingTime(fechaLocal))
                    {
                        fechaLocal = fechaLocal.AddHours(-1);
                    }
                    
                    var correoDatos = new CorreoDTO
                    {
                        Remitente = message.From.Mailboxes.First().Address,
                        Fecha = fechaLocal.ToString("dd/MM/yyyy"),
                        Hora = fechaLocal.ToString("HH:mm:ss"),
                        Asunto = message.Subject ?? "Sin asunto",
                        Contenido = message.TextBody ?? "Correo sin contenido."
                    };

                    context.Logger.LogLine($"📩 Nuevo correo detectado de {correoDatos.Remitente}");
                    context.Logger.LogLine($"📅 Fecha: {correoDatos.Fecha} - ⏰ Hora: {correoDatos.Hora}");
                    context.Logger.LogLine($"📝 Asunto: {correoDatos.Asunto}");
                    context.Logger.LogLine($"📖 Contenido: {correoDatos.Contenido}");

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
        var snsArn = Environment.GetEnvironmentVariable("SNS_ARN"); //📖Obtenemos el valor alamacenado en AWS

        var snsClient = new AmazonSimpleNotificationServiceClient();
        var publishRequest = new PublishRequest
        {
            TopicArn = snsArn,
            Message = System.Text.Json.JsonSerializer.Serialize(correoDatos)
        };
        await snsClient.PublishAsync(publishRequest);
    }
    
    private async Task<(string Email, string Password)> ObtenerCredencialesGmail()
    {
        var client = new AmazonSecretsManagerClient();
        var secretCredential = Environment.GetEnvironmentVariable("SECRET_GMAIL_CREDENTIALS");
        var request = new GetSecretValueRequest { SecretId = secretCredential }; //📖Obtenemos el valor secreto en AWS
        var response = await client.GetSecretValueAsync(request);

        var secret = JsonConvert.DeserializeObject<dynamic>(response.SecretString);
        return (secret.Email.ToString(), secret.Password.ToString());
    }
}