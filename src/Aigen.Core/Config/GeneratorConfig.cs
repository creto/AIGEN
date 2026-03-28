namespace Aigen.Core.Config;
public class GeneratorConfig
{
    public string AigenVersion    { get; set; } = "1.0";
    public ProjectConfig      Project      { get; set; } = new();
    public DatabaseConfig     Database     { get; set; } = new();
    public ArchitectureConfig Architecture { get; set; } = new();
    public BackendConfig      Backend      { get; set; } = new();
    public FrontendConfig     Frontend     { get; set; } = new();
    public SecurityConfig     Security     { get; set; } = new();
    public FeaturesConfig     Features     { get; set; } = new();
    public AuditConfig        Audit        { get; set; } = new();
    public AIConfig           AI           { get; set; } = new();
    public OutputConfig       Output       { get; set; } = new();
    public string ResolveOutputPath() => Output.Path.Replace("{ProjectName}", Project.ProjectName);
}
