using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Debug4MvcNetCore.Services
{
    public class EntityFrameworkCoreService
    {
        public EntityFrameworkCoreService()
        {

        }

        public string RunSql(string appDbContext)
        {
            //AppDbContextSqlResults;
            //DbConnection dbConnection = new Debug4MvcNetCoreTestsWebContext().Database.GetDbConnection();
            //dbConnection.Open();
            //DbCommand command = dbConnection.CreateCommand();
            //command.CommandText = "ssss a";
            //command.CommandType = System.Data.CommandType.Text;
            //DbDataReader reader = command.ExecuteReader();
            //while(reader.Read())
            //{
            //    for (int i = 0; i < reader.FieldCount; i++)
            //    {
            //        columns.Add(reader.GetName(i));
            //    }
            //}
            return null;
        }

        public List<AppDbContextInfo> GetAppDbContexts(HttpContext httpContext)
        {
            List<AppDbContextInfo> appDbContexts = new List<AppDbContextInfo>();
            foreach (var addDbContextType in GetAppDbContextTypes())
            {
                AppDbContextInfo appDbContextInfo = new AppDbContextInfo();
                appDbContexts.Add(appDbContextInfo);
                appDbContextInfo.Type = addDbContextType;
                appDbContextInfo.Name = addDbContextType.ToString();
                var addDbContext = httpContext?.RequestServices?.GetService(addDbContextType);
                if (addDbContext != null)
                {
                    var database = addDbContext.GetPropertyValue("Database");
                    if (database != null)
                    {
                        appDbContextInfo.ProviderName = database.GetPropertyValue("ProviderName")?.ToString();
                        var relatoinDatabaseExtensionType =  GetRelationalDatabaseExtensionTypes();
                        if (relatoinDatabaseExtensionType != null)
                        {
                            appDbContextInfo.Migrations = (relatoinDatabaseExtensionType.InvokeStaticMethod("GetMigrations", database) as IEnumerable<string>)?.ToList() ?? new List<string>();
                            appDbContextInfo.PendingMigrations = (relatoinDatabaseExtensionType.InvokeStaticMethod("GetPendingMigrations", database) as IEnumerable<string>)?.ToList() ?? new List<string>();
                            appDbContextInfo.AppliedMigrations = (relatoinDatabaseExtensionType.InvokeStaticMethod("GetAppliedMigrations", database) as IEnumerable<string>)?.ToList() ?? new List<string>();
                        }
                    }
                    var model = addDbContextType.GetProperty("Model")?.GetValue(addDbContext);
                    if (model != null)
                    {
                        var entitiesTypes = model.InvokeMethod("GetEntityTypes") as  System.Collections.IEnumerable;
                        foreach(var entityType in entitiesTypes)
                        {
                            appDbContextInfo.Entities.Add(new AppDbContextEntityInfo
                            {
                                NavigationName = entityType.GetPropertyValue("DefiningNavigationName")?.ToString(),
                                ClrTypeName = entityType.GetPropertyValue("ClrType")?.ToString(),
                            });

                        }
                    }
                }

            }
            return appDbContexts;
        }

        public List<Type> GetAppDbContextTypes()
        {
            var entityFrameworkCoreAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.FullName.StartsWith("Microsoft.EntityFrameworkCore,"));
            Type dbContexType = entityFrameworkCoreAssembly.GetType("Microsoft.EntityFrameworkCore.DbContext");
            if (dbContexType == null)
                return new List<Type>();

            List<Type> appDbCotexts = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName.StartsWith("Microsoft.EntityFrameworkCore"))
                    continue;
                if (assembly.FullName.StartsWith("Microsoft.AspNetCore"))
                    continue;
                foreach (TypeInfo type in assembly.DefinedTypes)
                {
                    if (type.IsSubclassOf(dbContexType))
                        appDbCotexts.Add(type);
                }
            }
            return appDbCotexts;
        }

        public Type GetRelationalDatabaseExtensionTypes()
        {
            var entityFrameworkCoreAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.FullName.StartsWith("Microsoft.EntityFrameworkCore.Relational,"));
            if (entityFrameworkCoreAssembly == null)
                return null;
            Type relationalDatabaseFacadeExtensionsType = entityFrameworkCoreAssembly.GetType("Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions");
            return relationalDatabaseFacadeExtensionsType;
        }
    }

    public class AppDbContextInfo
    {
        public string Name;

        public Type Type;
        //.Database.ProviderName
        public string ProviderName;
        //.Database.GetMigrations()
        public List<string> Migrations = new List<string>();
        //.Database.GetPendingMigrations()
        public List<string> PendingMigrations = new List<string>();
        //.Database.GetAppliedMigrations()
        public List<string> AppliedMigrations = new List<string>();
        //.Model.GetEntityTypes() : IEnumerable<IEntityType>
        public List<AppDbContextEntityInfo> Entities = new List<AppDbContextEntityInfo>();
        
    }

    public class AppDbContextEntityInfo
    {
        public string NavigationName;
        public string ClrTypeName;
    }

    public class AppDbContextSqlResults
    {
        public int AffectedRecords;
        public List<object[]> Rows = new List<object[]>();
        public List<string> FieldsName = new List<string>();
    }

    public static class ObjectExtensions
    {
        public static object GetPropertyValue(this object obj, string propertyName)
        {
            if (obj == null)
                return null;
            return obj.GetType().GetProperty(propertyName)?.GetValue(obj);
        }

        public static object InvokeMethod(this object obj, string methodName, params object[] methodParameters)
        {
            if (obj == null)
                return null;
            int parametersCount = (methodParameters ?? new object[0]).Length;
            return obj.GetType().GetMethods().FirstOrDefault(x => x.Name == methodName && x.GetParameters().Length == parametersCount).Invoke(obj, methodParameters);
        }

        public static object InvokeStaticMethod(this Type type, string methodName, params object[] methodParameters)
        {
            if (type == null)
                return null;
            int parametersCount = (methodParameters ?? new object[0]).Length;
            return type.GetMethods().FirstOrDefault(x => x.Name == methodName && x.GetParameters().Length == parametersCount).Invoke(null, methodParameters);
        }
    }
}
