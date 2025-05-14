public class DengueSemanaResponse {
    public string SemanaEpidemiologica { get; set; }
    public int CasosEstimados { get; set; }
    public int CasosNotificados { get; set; }
    public int NivelAlerta { get; set; }
    public string CorNivelAlerta { get; set; }

    public DengueSemanaResponse(string semanaEpidemiologica, int casosEstimados, int casosNotificados, int nivelAlerta) {
        SemanaEpidemiologica = semanaEpidemiologica;
        CasosEstimados = casosEstimados;
        CasosNotificados = casosNotificados;
        NivelAlerta = nivelAlerta;
        CorNivelAlerta = GetCorNivelAlerta(nivelAlerta);
    }

    private string GetCorNivelAlerta(int nivelAlerta) {
        switch (nivelAlerta) {
            case 1: return "green";
            case 2: return "yellow";
            case 3: return "orange";
            case 4: return "red";
            default: return "gray";
        }
    }
}
