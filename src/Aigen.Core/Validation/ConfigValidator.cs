using Aigen.Core.Config.Enums;

namespace Aigen.Core.Validation;

public class ConfigValidator
{
    private readonly List<ValidationError> _errors   = new();
    private readonly List<string>          _warnings = new();

    public ValidationResult Validate(Config.GeneratorConfig config)
    {
        _errors.Clear();
        _warnings.Clear();

        ValidateProject(config.Project);
        ValidateDatabase(config.Database);
        ValidateArchitecture(config.Architecture);
        ValidateSecurity(config.Security);
        ValidateAI(config.AI);
        ValidateOutput(config.Output);

        return new ValidationResult(_errors.ToList(), _warnings.ToList());
    }

    private void ValidateProject(Config.ProjectConfig p)
    {
        if (string.IsNullOrWhiteSpace(p.ProjectName))
            AddError("project.projectName", "El nombre del proyecto es obligatorio.");
        else if (p.ProjectName.Length < 3)
            AddError("project.projectName", "El nombre debe tener al menos 3 caracteres.");
        else if (!System.Text.RegularExpressions.Regex.IsMatch(p.ProjectName, @"^[a-zA-Z][a-zA-Z0-9_]*$"))
            AddError("project.projectName", "Solo puede contener letras, numeros y guiones bajos.");

        if (string.IsNullOrWhiteSpace(p.RootNamespace))
            AddError("project.rootNamespace", "El namespace raiz es obligatorio.");
        else if (!p.RootNamespace.Contains('.'))
            AddWarning("project.rootNamespace", "Se recomienda formato: Com.Empresa.Proyecto");
    }

    private void ValidateDatabase(Config.DatabaseConfig db)
    {
        if (string.IsNullOrWhiteSpace(db.ConnectionString))
            AddError("database.connectionString", "El connection string es obligatorio.");
        else if (db.ConnectionString.Contains("TU_SERVIDOR") ||
                 db.ConnectionString.Contains("TU_BD") ||
                 db.ConnectionString.Contains("TU_PASS"))
            AddError("database.connectionString",
                "El connection string contiene valores de plantilla sin reemplazar.");

        if (string.IsNullOrWhiteSpace(db.Schema))
            AddWarning("database.schema", "Schema no definido, se usara 'dbo'.");

        var validSelections = new[] { "All", "Include", "Exclude" };
        if (!validSelections.Contains(db.TableSelection))
            AddError("database.tableSelection",
                $"Valor invalido '{db.TableSelection}'. Validos: All, Include, Exclude");

        if (db.TableSelection == "Include" && db.IncludedTables.Count == 0)
            AddError("database.includedTables",
                "TableSelection es 'Include' pero IncludedTables esta vacio.");
    }

    private void ValidateArchitecture(Config.ArchitectureConfig arch)
    {
        if (arch.Style == OutputStyle.Microservices && arch.Microservices.Count == 0)
            AddWarning("architecture.microservices",
                "Estilo Microservices sin microservicios definidos. Se generara uno por tabla.");

        foreach (var ms in arch.Microservices)
        {
            if (string.IsNullOrWhiteSpace(ms.Name))
                AddError("architecture.microservices", "Cada microservicio debe tener un nombre.");
            if (ms.Port < 1024 || ms.Port > 65535)
                AddError($"architecture.microservices[{ms.Name}].port",
                    $"Puerto {ms.Port} invalido. Rango valido: 1024-65535.");
            if (ms.Tables.Count == 0)
                AddWarning($"architecture.microservices[{ms.Name}]",
                    $"El microservicio '{ms.Name}' no tiene tablas asignadas.");
        }

        var ports = arch.Microservices.Select(m => m.Port).ToList();
        if (arch.Gateway.GenerateGateway) ports.Add(arch.Gateway.Port);
        if (ports.Distinct().Count() != ports.Count)
            AddError("architecture", "Hay puertos duplicados entre microservicios y/o gateway.");
    }

    private void ValidateSecurity(Config.SecurityConfig sec)
    {
        if (sec.Authentication == AuthenticationType.Keycloak)
        {
            if (string.IsNullOrWhiteSpace(sec.KeycloakUrl))
                AddError("security.keycloakUrl", "Keycloak requiere KeycloakUrl.");
            if (string.IsNullOrWhiteSpace(sec.KeycloakRealm))
                AddError("security.keycloakRealm", "Keycloak requiere KeycloakRealm.");
        }
        if (sec.EnableCors && string.IsNullOrWhiteSpace(sec.CorsOrigins))
            AddWarning("security.corsOrigins", "CORS habilitado pero CorsOrigins esta vacio.");
    }

    private void ValidateAI(Config.AIConfig ai)
    {
        if (ai.Provider != AIProviderType.None && ai.Provider != AIProviderType.Ollama)
            if (string.IsNullOrWhiteSpace(ai.ApiKey))
                AddError("ai.apiKey", $"Provider '{ai.Provider}' requiere ApiKey.");

        if (ai.Provider == AIProviderType.Ollama && string.IsNullOrWhiteSpace(ai.OllamaUrl))
            AddError("ai.ollamaUrl", "Ollama requiere OllamaUrl.");
    }

    private void ValidateOutput(Config.OutputConfig output)
    {
        if (string.IsNullOrWhiteSpace(output.Path))
            AddError("output.path", "La ruta de salida es obligatoria.");
        if (output.PushToRemote && string.IsNullOrWhiteSpace(output.RemoteUrl))
            AddError("output.remoteUrl", "PushToRemote=true requiere RemoteUrl.");
    }

    private void AddError(string field, string message) =>
        _errors.Add(new ValidationError(field, message));

    private void AddWarning(string field, string message) =>
        _warnings.Add($"{field}: {message}");
}

public record ValidationError(string Field, string Message);

public class ValidationResult
{
    public List<ValidationError> Errors   { get; }
    public List<string>          Warnings { get; }
    public bool IsValid => Errors.Count == 0;

    public ValidationResult(List<ValidationError> errors, List<string> warnings)
    {
        Errors   = errors;
        Warnings = warnings;
    }
}
