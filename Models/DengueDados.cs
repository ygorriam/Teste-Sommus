using System;
using System.ComponentModel.DataAnnotations;

namespace Sommus.DengueApi.Models;

public class DengueDados
{
    public int Id { get; set; }

    [Required]
    [StringLength(7)]
    public string SemanaEpidemiologica { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int CasosEstimados { get; set; }

    [Range(0, int.MaxValue)]
    public int CasosNotificados { get; set; }

    [Range(1, 4)]
    public int NivelAlerta { get; set; }

    public DateTime DataImportacao { get; set; } = DateTime.Now;
}