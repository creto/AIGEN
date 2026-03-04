namespace Aigen.Core.Config.Enums;
public enum AuthenticationType { Jwt, AspNetIdentity, Keycloak, AzureAD, None }
public enum AuthorizationType  { Roles, Claims, Policies, None }
public enum SocialProvider     { Google, Microsoft, Facebook, GitHub }
