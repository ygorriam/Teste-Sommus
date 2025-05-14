using System;
using System.Text.Json.Serialization;

namespace Sommus.DengueApi.Models;

public class DengueApiResponse {
    [JsonPropertyName("SE")]
    public int SemanaEpidemiologica { get; set; }

    [JsonPropertyName("data_iniSE")]
    public long DataInicioSemanaUnix { get; set; }

    [JsonPropertyName("casos_est")]
    public double CasosEstimados { get; set; }

    [JsonPropertyName("casos")]
    public int CasosNotificados { get; set; }

    [JsonPropertyName("nivel")]
    public int NivelAlerta { get; set; }

    [JsonIgnore]
    public DateTime DataInicioSemana =>
        DateTimeOffset.FromUnixTimeMilliseconds(DataInicioSemanaUnix).DateTime;

    // Adicione este método para facilitar a formatação
    public string SemanaFormatada =>
        $"{DataInicioSemana.Year}-{SemanaEpidemiologica.ToString().Substring(4)}";
}