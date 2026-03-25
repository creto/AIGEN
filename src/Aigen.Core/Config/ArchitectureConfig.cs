using Aigen.Core.Config.Enums;
namespace Aigen.Core.Config;
public class ArchitectureConfig
{
    public OutputStyle                  Style            { get; set; } = OutputStyle.Microservices;
    public bool                         SeparateSolutionPerService { get; set; } = false;
    public string                        TablePrefixGrouping        { get; set; } = "auto"; // auto | manual
    public ArchitecturePattern          Pattern          { get; set; } = ArchitecturePattern.CleanArchitecture;
    public GroupingStrategy             GroupingStrategy { get; set; } = GroupingStrategy.Manual;
    public List<MicroserviceDefinition> Microservices    { get; set; } = new();
    public GatewayConfig                Gateway          { get; set; } = new();
}
public class MicroserviceDefinition
{
    public string       Name   { get; set; } = string.Empty;
    public string       Prefix { get; set; } = "MS";
    public int          Port   { get; set; } = 5001;
    public List<string> Tables { get; set; } = new();
}
public class GatewayConfig
{
    public bool   GenerateGateway { get; set; } = true;
    public string Technology      { get; set; } = "YARP";
    public int    Port            { get; set; } = 5000;
}
