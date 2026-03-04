namespace Aigen.Core.Config.Enums;
public enum OrmType           { EntityFrameworkCore, Dapper, EFCoreWithDapper }
public enum RepositoryPattern { RepositoryUnitOfWork, RepositoryOnly, DirectDbContext }
public enum EfStrategy        { DatabaseFirst, CodeFirst }
