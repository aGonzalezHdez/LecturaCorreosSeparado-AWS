
using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.SQS;
using Amazon.SQS.Model;
using DatabaseInitializer;
using DatabaseInitializer.Models;
using EmailProcessor.Model;
using MySqlConnector;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EmailProcessor;

public class Function
{
    public async Task Handler(SQSEvent sqsEvent, ILambdaContext context)
    {
        var queueUrl = "https://sqs.us-east-1.amazonaws.com/970547342167/LecturaCorreos-Recibidos";
        var sqsClient = new AmazonSQSClient();

        using var db = new MySQLDBContext(); // **Usamos DbContext para interactuar con RDS**

        foreach (var record in sqsEvent.Records)
        {
            try
            {
                var correoDTO = JsonSerializer.Deserialize<CorreoDTO>(record.Body);
                if (correoDTO == null)
                {
                    context.Logger.LogLine("❌ Error al deserializar el mensaje SQS.");
                    continue;
                }

                // ✅ **Crear nueva instancia de Consultas y llenar datos**
                var consulta = new Consultas
                {
                    FechaLlegada = correoDTO.Fecha,
                    HoraLlegada = correoDTO.Hora,
                    CorreoCliente = correoDTO.Remitente,
                    TituloCorreo = correoDTO.Asunto,
                    Partidas = 0, // Puede modificarse según la lógica de negocio
                    FechaCierre = "",
                    HoraCierre = "",
                    DiasRespuesta = 0,
                    Referencia = "",
                    RazonSocial = "",
                    ClienteId = 1, // **Asigna IDs reales según tu lógica**
                    ComentariosId = 1,
                    AtendidoPorId = 1
                };

                // ✅ **Guardar en la base de datos con EF**
                db.Consultas.Add(consulta);
                await db.SaveChangesAsync();

                context.Logger.LogLine($"✅ Correo guardado en la base (Consultas): {consulta.TituloCorreo}");

                // ✅ **Eliminar el mensaje de SQS después de procesarlo**
                var deleteRequest = new DeleteMessageRequest
                {
                    QueueUrl = queueUrl,
                    ReceiptHandle = record.ReceiptHandle
                };
                await sqsClient.DeleteMessageAsync(deleteRequest);
                context.Logger.LogLine($"✅ Mensaje eliminado de SQS: {record.MessageId}");
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"❌ Error procesando mensaje de SQS: {ex.Message}");
            }
        }
    }


}