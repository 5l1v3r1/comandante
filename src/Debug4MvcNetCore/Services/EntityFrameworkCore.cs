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

        public AppDbContextSqlResults RunSql(string sql, string appDbContextName, HttpContext httpContext, int maxRowsReads = 1000)
        {
            try {
                Type addDbContextType = GetAppDbContextTypes().FirstOrDefault(x => x.Name == appDbContextName);
                object addDbContext = httpContext?.RequestServices?.GetService(addDbContextType);
                if (addDbContext != null)
                {
                    object database = addDbContext.GetPropertyValue("Database");
                    List<string> columns = new List<string>();
                    List<object[]> rows = new List<object[]>();
                    int recordsAffected = -1;
                    var relatoinDatabaseExtensionType = GetRelationalDatabaseExtensionTypes();
                    using (var dbConnection = relatoinDatabaseExtensionType.InvokeStaticMethod("GetDbConnection", database) as IDisposable)
                    {
                        dbConnection.InvokeMethod("Open");
                        object dbCommand = dbConnection.InvokeMethod("CreateCommand");
                        dbCommand.SetPropertyValue("CommandText", sql);
                        object reader = dbCommand.InvokeMethod("ExecuteReader");
                        while ((bool)reader.InvokeMethod("Read"))
                        {
                            if (columns.Count == 0)
                            {
                                int fieldCount = (int)reader.GetPropertyValue("FieldCount");
                                for (int i = 0; i < fieldCount; i++)
                                {
                                    columns.Add(reader.InvokeMethod("GetName", i)?.ToString());
                                }
                            }
                            object[] row = new object[columns.Count];
                            for (int i = 0; i < columns.Count; i++)
                            {
                                row[i] = reader.InvokeMethod("GetValue", i);
                            }
                            rows.Add(row);
                            if (rows.Count >= maxRowsReads)
                                break;
                        }
                        recordsAffected = (int)reader.GetPropertyValue("RecordsAffected");
                    }
                    return new AppDbContextSqlResults
                    {
                        Columns = columns,
                        Rows = rows,
                        AffectedRecords = recordsAffected
                    };

                }
                return new AppDbContextSqlResults { Error = "Cannot find DbContext: " + appDbContextName };
            }
            catch (TargetInvocationException ex)
            {
                var exception = ex.InnerException;
                var error = "";
                while (exception != null)
                {
                    error = error + " " + exception.Message; ;
                    exception = exception.InnerException;
                }
                return new AppDbContextSqlResults { Error = error };
            }
            catch (Exception ex)
            {
                var exception = ex;
                var error = "";
                while (exception != null)
                {
                    error = error + " " + exception.Message; ;
                    exception = exception.InnerException;
                }
                return new AppDbContextSqlResults { Error = error };
            }
        }

        public List<AppDbContextInfo> GetAppDbContexts(HttpContext httpContext)
        {
            List<AppDbContextInfo> appDbContexts = new List<AppDbContextInfo>();
            foreach (var addDbContextType in GetAppDbContextTypes())
            {
                AppDbContextInfo appDbContextInfo = new AppDbContextInfo();
                appDbContexts.Add(appDbContextInfo);
                appDbContextInfo.Type = addDbContextType;
                appDbContextInfo.Name = addDbContextType.Name;
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
        public List<string> Columns = new List<string>();
        public string Error;
    }

    public static class ObjectExtensions
    {
        public static object GetPropertyValue(this object obj, string propertyName)
        {
            if (obj == null)
                return null;
            return obj.GetType().GetProperty(propertyName)?.GetValue(obj);
        }

        public static void SetPropertyValue(this object obj, string propertyName, object propertyValue)
        {
            if (obj == null)
                return;
            obj.GetType().GetProperty(propertyName)?.SetValue(obj, propertyValue);
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
