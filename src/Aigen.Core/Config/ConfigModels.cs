using Aigen.Core.Config.Enums;
namespace Aigen.Core.Config;
public class BackendConfig
{
    public OrmType           Orm                     { get; set; } = OrmType.EFCoreWithDapper;
    public RepositoryPattern RepositoryPattern       { get; set; } = RepositoryPattern.RepositoryUnitOfWork;
    public bool              UseCQRS                 { get; set; } = false;
    public bool              GenerateStoredProcedures{ get; set; } = true;
    // direct: EF+Dapper sin SPs | storedProcedures: todo via SPs | mixed: SPs solo en tablas marcadas
    public string CrudStrategy { get; set; } = "direct";
    public string SpPrefix     { get; set; } = "PA_";  // prefijo para SPs — ej: PA_, sp_
    public string SpSchema     { get; set; } = "API";  // schema donde se crean los SPs
    public string TargetFramework                    { get; set; } = "net8.0";
}
public class FrontendConfig
{
    public FrontendFramework Framework        { get; set; } = FrontendFramework.Angular;
    public bool              GenerateFrontend { get; set; } = true;
    public string            FrameworkVersion { get; set; } = "18";
    public UiLibrary         UiLibrary        { get; set; } = UiLibrary.PrimeNG;
    public StateManagement   StateManagement  { get; set; } = StateManagement.Signals;
    public string            Theme            { get; set; } = "Material";
    public string            PrimaryColor     { get; set; } = "2563EB";
    public string            SecondaryColor   { get; set; } = "64748B";
    public string?           LogoPath         { get; set; }
    public string            ApiBaseUrl       { get; set; } = "http://localhost:5000";
    public string            ApiBaseProdUrl   { get; set; } = "__API_URL__";
}
public class SecurityConfig
{
    public AuthenticationType Authentication { get; set; } = AuthenticationType.Jwt;
    public AuthorizationType  Authorization  { get; set; } = AuthorizationType.Roles;
    public bool               TwoFactor      { get; set; } = false;
    public bool               ForceHttps     { get; set; } = true;
    public bool               EnableCors     { get; set; } = true;
    public string             CorsOrigins    { get; set; } = "http://localhost:4200";
    public string?            KeycloakUrl    { get; set; }
    public string?            KeycloakRealm  { get; set; }
    // JWT settings
    public string  JwtSource            { get; set; } = "DatabaseTable"; // DatabaseTable | Hardcoded | OIDC
    public string  UserTable            { get; set; } = "TB_Funcionario";
    public string  JwtKey               { get; set; } = "AIGEN_SECRET_KEY_CHANGE_IN_PRODUCTION_MIN32CHARS";
    public string  JwtIssuer            { get; set; } = "AigenApi";
    public string  JwtAudience          { get; set; } = "AigenClient";
    public int     JwtExpiresMinutes    { get; set; } = 60;
    public int     RefreshExpiresDays   { get; set; } = 7;
    public bool    UseRefreshToken      { get; set; } = true;
    public string  OidcProvider         { get; set; } = "None"; // None | Keycloak | AzureAD | Auth0
    public string? OidcAuthority        { get; set; }
    public string? OidcClientId         { get; set; }
    public string? OidcClientSecret     { get; set; }
}
public class FeaturesConfig
{
    public LoggingProvider    Logging               { get; set; } = LoggingProvider.Serilog;
    public ApiDocProvider     ApiDoc                { get; set; } = ApiDocProvider.Swagger;
    public CacheProvider      Cache                 { get; set; } = CacheProvider.None;
    public ValidationProvider Validation            { get; set; } = ValidationProvider.FluentValidation;
    public MappingProvider    Mapping               { get; set; } = MappingProvider.AutoMapper;
    public CICDProvider       CICD                  { get; set; } = CICDProvider.GitHubActions;
    public bool               GeneratePagination    { get; set; } = true;
    public bool               SoftDelete            { get; set; } = true;
    public bool               Auditing              { get; set; } = true;
    public bool               MultiTenancy          { get; set; } = false;
    public bool               GenerateTests         { get; set; } = true;
    public bool               GenerateDockerfile    { get; set; } = true;
    public bool               GenerateDockerCompose { get; set; } = true;
}
public class AuditConfig
{
    // EFInterceptor: captura via SaveChanges | StoredProcedure: SP por tabla | Both: ambos
    public string Provider       { get; set; } = "EFInterceptor";
    public bool   GlobalEnabled  { get; set; } = true;
    public string AuditTable     { get; set; } = "TA_AuditoriaTransacciones";
    // Configuracion por tabla: clave=nombre tabla, valor=campos a auditar (* = todos)
    public Dictionary<string, AuditTableConfig> Tables { get; set; } = new();
}
public class AuditTableConfig
{
    public bool         Enabled { get; set; } = true;
    public List<string> Fields  { get; set; } = new() { "*" }; // * = todos los campos
}

public class AIConfig
{
    public AIProviderType Provider           { get; set; } = AIProviderType.None;
    public string         Model              { get; set; } = "claude-sonnet-4-5";
    public string         ApiKey             { get; set; } = string.Empty;
    public string         OllamaUrl          { get; set; } = "http://localhost:11434";
    public bool           EnhanceEntities    { get; set; } = true;
    public bool           GenerateTests      { get; set; } = true;
    public bool           InferBusinessRules { get; set; } = true;
}
public class OutputConfig
{
    public OutputType Type          { get; set; } = OutputType.LocalPath;
    public string     Path          { get; set; } = "./output/{ProjectName}";
    public bool       CreateGitRepo { get; set; } = true;
    public bool       PushToRemote  { get; set; } = false;
    public string     RemoteUrl     { get; set; } = string.Empty;
}



