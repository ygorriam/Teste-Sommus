using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sommus.DengueApi.Data;
using Sommus.DengueApi.Models;

namespace Sommus.DengueApi.Services;

public class DengueService : IDengueService
{
    private readonly ApplicationDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly ILogger<DengueService> _logger;

    public DengueService(
        ApplicationDbContext context,
        HttpClient httpClient,
        ILogger<DengueService> logger)
    {
        _context = context;
        _httpClient = httpClient;
        _logger = logger;
        ConfigureHttpClient();
    }

    private void ConfigureHttpClient()
    {
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "SommusDengueAPI/1.0");
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<(bool Success, string Message)> ImportarDadosUltimos6MesesAsync()
    {
        try
        {
            _logger.LogInformation("Iniciando importação de dados dos últimos 6 meses");

            // 1. Calcula o período das últimas 24 semanas (6 meses)
            var (startWeek, endWeek, startYear, endYear) = CalcularPeriodo6Meses();
            _logger.LogInformation($"Período calculado: Semana {startWeek}/{startYear} a {endWeek}/{endYear}");

            // 2. Constroi a URL da API
            var url = ConstruirUrlApi(startWeek, endWeek, startYear, endYear);
            _logger.LogInformation($"URL da API: {url}");

            // 3. Faz a requisição para a API
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Erro na API: {response.StatusCode} - {errorContent}");
                return (false, $"Erro na API: {response.StatusCode}");
            }

            // 4. Processa o conteúdo da resposta
            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content) || content.Trim() == "[]")
            {
                _logger.LogWarning("API retornou dados vazios");
                return (false, "API retornou dados vazios para o período solicitado");
            }

            // 5. Desserializa o JSON
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            };

            var dadosApi = JsonSerializer.Deserialize<List<DengueApiResponse>>(content, options);

            if (dadosApi == null || !dadosApi.Any())
            {
                _logger.LogWarning("Nenhum dado válido encontrado após desserialização");
                return (false, "Nenhum dado válido retornado pela API");
            }

            _logger.LogInformation($"Recebidos {dadosApi.Count} registros da API");

            // 6. Processa e salva os dados
            var registrosProcessados = await ProcessarDadosImportados(dadosApi);

            _logger.LogInformation($"Importação concluída. {registrosProcessados} registros processados");
            return (true, $"Dados importados com sucesso. {registrosProcessados} registros processados");
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "Erro ao desserializar JSON da API");
            return (false, $"Erro no formato dos dados: {jsonEx.Message}");
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "Erro na requisição HTTP");
            return (false, $"Erro de comunicação com a API: {httpEx.Message}");
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Erro ao salvar no banco de dados");
            return (false, $"Erro de banco de dados: {dbEx.InnerException?.Message ?? dbEx.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado durante a importação");
            return (false, $"Erro inesperado: {ex.Message}");
        }
    }

    private async Task<int> ProcessarDadosImportados(List<DengueApiResponse> dadosApi)
    {
        var registrosProcessados = 0;

        // Obtém todas as semanas já existentes no banco de uma vez
        var semanasExistentes = await _context.DengueDados
            .Select(d => d.SemanaEpidemiologica)
            .ToListAsync();

        _logger.LogInformation($"Verificando {dadosApi.Count} registros contra o banco de dados");

        foreach (var item in dadosApi)
        {
            try
            {
                var semanaFormatada = item.SemanaFormatada;

                if (semanasExistentes.Contains(semanaFormatada))
                {
                    // Atualiza registro existente
                    var registroExistente = await _context.DengueDados
                        .FirstOrDefaultAsync(d => d.SemanaEpidemiologica == semanaFormatada);

                    if (registroExistente != null)
                    {
                        registroExistente.CasosEstimados = (int)Math.Round(item.CasosEstimados);
                        registroExistente.CasosNotificados = item.CasosNotificados;
                        registroExistente.NivelAlerta = item.NivelAlerta;
                        _logger.LogDebug($"Atualizado registro para semana {semanaFormatada}");
                    }
                }
                else
                {
                    // Adiciona novo registro
                    _context.DengueDados.Add(new DengueDados
                    {
                        SemanaEpidemiologica = semanaFormatada,
                        CasosEstimados = (int)Math.Round(item.CasosEstimados),
                        CasosNotificados = item.CasosNotificados,
                        NivelAlerta = item.NivelAlerta,
                        DataImportacao = DateTime.Now
                    });
                    _logger.LogDebug($"Adicionado novo registro para semana {semanaFormatada}");
                }

                registrosProcessados++;

                // Salva a cada 10 registros para melhor performance
                if (registrosProcessados % 10 == 0)
                {
                    await _context.SaveChangesAsync();
                    _logger.LogDebug($"Salvou {registrosProcessados} registros temporariamente");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao processar semana {item.SemanaEpidemiologica}");
                // Continua processando os próximos registros
            }
        }

        // Salva os registros restantes
        if (registrosProcessados > 0)
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Salvou no total {registrosProcessados} registros no banco");
        }

        return registrosProcessados;
    }

    public async Task<DengueSemanaResponse?> ObterDadosPorSemanaAsync(int semana, int ano)
    {
        if (semana < 1 || semana > 53)
            throw new ArgumentOutOfRangeException(nameof(semana), "Semana epidemiológica deve estar entre 1 e 53");

        if (ano < 2000 || ano > 2100)
            throw new ArgumentOutOfRangeException(nameof(ano), "Ano deve estar entre 2000 e 2100");

        try
        {
            var semanaFormatada = $"{ano}-{semana:D2}";

            _logger.LogInformation($"Buscando dados para semana {semanaFormatada}");

            var dado = await _context.DengueDados
                .Where(d => d.SemanaEpidemiologica == semanaFormatada)
                .Select(d => new DengueSemanaResponse(
                    d.SemanaEpidemiologica,
                    d.CasosEstimados,
                    d.CasosNotificados,
                    d.NivelAlerta))
                .FirstOrDefaultAsync();

            if (dado == null)
            {
                _logger.LogWarning($"Semana {semanaFormatada} não encontrada no banco de dados");
            }

            return dado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao buscar dados para semana {ano}-{semana}");
            throw;
        }
    }

    public async Task<List<DengueSemanaResponse>> ObterUltimasSemanasAsync(int quantidade = 3)
    {
        if (quantidade < 1 || quantidade > 52)
            throw new ArgumentOutOfRangeException(nameof(quantidade), "Quantidade deve estar entre 1 e 52");

        try
        {
            _logger.LogInformation($"Buscando últimas {quantidade} semanas");

            return await _context.DengueDados
                .OrderByDescending(d => d.SemanaEpidemiologica)
                .Take(quantidade)
                .Select(d => new DengueSemanaResponse(
                    d.SemanaEpidemiologica,
                    d.CasosEstimados,
                    d.CasosNotificados,
                    d.NivelAlerta))
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar últimas semanas");
            throw;
        }
    }

    public async Task<List<DengueSemanaResponse>> ObterSemanasComAlertaAsync(int quantidade = 3, int nivelMinimo = 2)
    {
        if (quantidade < 1 || quantidade > 52)
            throw new ArgumentOutOfRangeException(nameof(quantidade), "Quantidade deve estar entre 1 e 52");

        if (nivelMinimo < 1 || nivelMinimo > 4)
            throw new ArgumentOutOfRangeException(nameof(nivelMinimo), "Nível de alerta deve estar entre 1 e 4");

        try
        {
            _logger.LogInformation($"Buscando {quantidade} semanas com nível de alerta >= {nivelMinimo}");

            return await _context.DengueDados
                .Where(d => d.NivelAlerta >= nivelMinimo)
                .OrderByDescending(d => d.SemanaEpidemiologica)
                .Take(quantidade)
                .Select(d => new DengueSemanaResponse(
                    d.SemanaEpidemiologica,
                    d.CasosEstimados,
                    d.CasosNotificados,
                    d.NivelAlerta))
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar semanas com alerta");
            throw;
        }
    }

    public async Task<(bool Success, string Message)> LimparDadosAsync()
    {
        try
        {
            var totalRegistros = await _context.DengueDados.CountAsync();

            if (totalRegistros == 0)
            {
                return (true, "Nenhum registro para limpar");
            }

            _logger.LogInformation($"Removendo {totalRegistros} registros do banco de dados");

            _context.DengueDados.RemoveRange(_context.DengueDados);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Dados limpos com sucesso");
            return (true, $"{totalRegistros} registros removidos com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao limpar dados");
            return (false, $"Erro ao limpar dados: {ex.Message}");
        }
    }

    private (int startWeek, int endWeek, int startYear, int endYear) CalcularPeriodo6Meses()
    {
        var hoje = DateTime.Now;
        var dataInicio = hoje.AddMonths(-6);

        var startWeek = ISOWeek.GetWeekOfYear(dataInicio);
        var endWeek = ISOWeek.GetWeekOfYear(hoje);

        return (startWeek, endWeek, dataInicio.Year, hoje.Year);
    }

    private string ConstruirUrlApi(int startWeek, int endWeek, int startYear, int endYear)
    {
        return $"https://info.dengue.mat.br/api/alertcity?" +
               $"geocode=3106200&" +  // Código IBGE de Belo Horizonte
               $"disease=dengue&" +
               $"format=json&" +
               $"ew_start={startWeek}&" +
               $"ew_end={endWeek}&" +
               $"ey_start={startYear}&" +
               $"ey_end={endYear}";
    }
}