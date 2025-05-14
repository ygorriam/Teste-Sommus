using Sommus.DengueApi.Models;

namespace Sommus.DengueApi.Services;

public interface IDengueService {
    Task<(bool Success, string Message)> ImportarDadosUltimos6MesesAsync();
    Task<DengueSemanaResponse?> ObterDadosPorSemanaAsync(int semana, int ano);
    Task<List<DengueSemanaResponse>> ObterUltimasSemanasAsync(int quantidade);
    Task<List<DengueSemanaResponse>> ObterSemanasComAlertaAsync(int quantidade, int nivelMinimo);
    Task<(bool Success, string Message)> LimparDadosAsync();
}