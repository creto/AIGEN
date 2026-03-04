namespace Aigen.Core.Config.Enums;
public enum LoggingProvider    { Serilog, NLog, NativeILogger }
public enum ValidationProvider { FluentValidation, DataAnnotations, Both }
public enum MappingProvider    { AutoMapper, Mapster, Manual }
public enum CacheProvider      { None, MemoryCache, Redis }
public enum ApiDocProvider     { Swagger, Scalar, None }
public enum CICDProvider       { GitHubActions, AzureDevOps, None }
public enum OutputType         { LocalPath, GitHub, AzureDevOps }
public enum AIProviderType     { Claude, OpenAI, Ollama, None }
