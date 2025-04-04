using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatabaseInitializer.Models;

public class Consultas
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string FechaLlegada { get; set; } = "";
    public string HoraLlegada { get; set; } = "";
    public string CorreoCliente { get; set; } = "";
    public string TituloCorreo { get; set; } = "";
    public int Partidas { get; set; }
    public string FechaCierre { get; set; } = "";
    public string HoraCierre { get; set; } = "";
    public int DiasRespuesta { get; set; }
    public string Referencia { get; set; } = "";
    public string RazonSocial { get; set; } = "";

    // Relación con Cliente
    [ForeignKey("Cliente")]
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; }

    // Relación con Comentarios
    [ForeignKey("Comentarios")]
    public int ComentariosId { get; set; }
    public Comentarios Comentarios { get; set; }

    // Relación con Ejecutivo (AtendidoPor)
    [ForeignKey("Ejecutivo")]
    public int AtendidoPorId { get; set; }
    public Ejecutivo AtendidoPor { get; set; }
}