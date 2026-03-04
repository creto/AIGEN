namespace Aigen.Core.Config;
public class ProjectConfig
{
    public string ProjectName   { get; set; } = "MiProyecto";
    public string RootNamespace { get; set; } = "Com.MiEmpresa.MiProyecto";
    public string Version       { get; set; } = "1.0.0";
    public string Author        { get; set; } = "AIGEN";
    public string Year          { get; set; } = DateTime.Now.Year.ToString();
    public string Description   { get; set; } = string.Empty;
    public string Language      { get; set; } = "Spanish";
}
