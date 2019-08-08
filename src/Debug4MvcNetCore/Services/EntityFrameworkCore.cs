using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

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
                    var relatoinDatabaseExtensionType = GetRelationalDatabaseExtensionType();
                    using (var dbConnection = relatoinDatabaseExtensionType.InvokeStaticMethod("GetDbConnection", database) as IDisposable)
                    {
                        dbConnection.InvokeMethod("Open");
                        using (var dbCommand = dbConnection.InvokeMethod("CreateCommand") as IDisposable)
                        {
                            dbCommand.SetPropertyValue("CommandText", sql);
                            using (var reader = dbCommand.InvokeMethod("ExecuteReader") as IDisposable)
                            {
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
                        }
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
                    var relationalDatabaseExtensionType = GetRelationalDatabaseExtensionType();
                    var relationalMetadataExtensions = GetRelationalMetadataExtensionsType();
                    if (database != null)
                    {
                        appDbContextInfo.ProviderName = database.GetPropertyValue("ProviderName")?.ToString();
                        if (relationalDatabaseExtensionType != null)
                        {
                            appDbContextInfo.Migrations = (relationalDatabaseExtensionType.InvokeStaticMethod("GetMigrations", database) as IEnumerable<string>)?.ToList() ?? new List<string>();
                            appDbContextInfo.PendingMigrations = (relationalDatabaseExtensionType.InvokeStaticMethod("GetPendingMigrations", database) as IEnumerable<string>)?.ToList() ?? new List<string>();
                            appDbContextInfo.AppliedMigrations = (relationalDatabaseExtensionType.InvokeStaticMethod("GetAppliedMigrations", database) as IEnumerable<string>)?.ToList() ?? new List<string>();
                        }
                    }
                    var model = addDbContextType.GetProperty("Model")?.GetValue(addDbContext);
                    if (model != null)
                    {
                        var entitiesTypes = model.InvokeMethod("GetEntityTypes") as  System.Collections.IEnumerable;
                        foreach(var entityType in entitiesTypes)
                        {
                            var relationalEntity = relationalMetadataExtensions.InvokeStaticMethod("Relational", entityType);
                            var schema = relationalEntity.GetPropertyValue("Schema")?.ToString();
                            var tableName = relationalEntity.GetPropertyValue("TableName")?.ToString();
                            var schemaAndTableName = string.IsNullOrEmpty(schema) == false ? schema + "." + tableName : tableName;
                            appDbContextInfo.Entities.Add(new AppDbContextEntityInfo
                            {
                                NavigationName = entityType.GetPropertyValue("DefiningNavigationName")?.ToString(),
                                ClrTypeName = entityType.GetPropertyValue("ClrType")?.ToString(),
                                Schema = schema,
                                TableName = tableName,
                                SchemaAndTableName = schemaAndTableName,
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

        public Type GetRelationalDatabaseExtensionType()
        {
            var entityFrameworkCoreAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.FullName.StartsWith("Microsoft.EntityFrameworkCore.Relational,"));
            if (entityFrameworkCoreAssembly == null)
                return null;
            Type relationalDatabaseFacadeExtensionsType = entityFrameworkCoreAssembly.GetType("Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions");
            return relationalDatabaseFacadeExtensionsType;
        }

        public Type GetRelationalMetadataExtensionsType()
        {
            var entityFrameworkCoreAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.FullName.StartsWith("Microsoft.EntityFrameworkCore.Relational,"));
            if (entityFrameworkCoreAssembly == null)
                return null;
            Type relationalDatabaseFacadeExtensionsType = entityFrameworkCoreAssembly.GetType("Microsoft.EntityFrameworkCore.RelationalMetadataExtensions");
            return relationalDatabaseFacadeExtensionsType;
        }

        public string DecodeSqlFromLogEntry(string logDetails)
        {
            Regex paramRegex = new Regex(@"(@p\d+|@__\w*?_\d+)='(.*?)'(\s\(\w*?\s=\s\w*\))*(?:,\s|\]).*?");
            string parameterStart = "[Parameters=[";

            var messageLines = logDetails.Split('\n').Select(x => x.Trim()).ToArray();

            if (messageLines[0].IndexOf("[Parameters=[]") > 0) // if list of parameters is empty
                return string.Join("\r\n", messageLines.Skip(1));

            var parametersIndex = messageLines[0].IndexOf(parameterStart);
            if (parametersIndex <= 0)  // if does not contain parameter info
                return logDetails;

            List<(string ParamName, string ParamValue, string[] ParamTypes, string ValueToInsert)> decodedMatches = new List<(string ParamName, string ParamValue, string[] ParamTypes, string ValueToInsert)>();
            foreach (var match in paramRegex.Matches(messageLines[0].Substring(parametersIndex + parameterStart.Length)).Cast<Match>())
            {
                string paramName = match.Groups[1].Value;
                string paramValue = match.Groups[2].Value;
                string[] paramTypes = match.Groups[3].Captures.Cast<Capture>().Select(y => y.Value).ToArray();
                string valueToInsert = null;
                if (paramValue == string.Empty)
                    //If no value we assume NULL 
                    //NOTE: this fails with empty string
                    valueToInsert = "NULL";

                if (paramValue.Any())
                    //We assume its something that needs to be a string
                    //NOTE: This will get byte[] wrong
                    valueToInsert = $"'{paramValue.Replace("'", "''")}'";

                if (paramValue == "True" || paramValue == "False")
                    valueToInsert = paramValue == "True" ? "1" : "0";

                //NOTE: numbers are presented as a string, but SQL Server handles that OK.
                valueToInsert = $"'{paramValue}'";
                decodedMatches.Add((paramName, paramValue, paramTypes, valueToInsert));
            }

            //is sensitive logging isn't enabled then all the param values will '?', so we just return the message
            if (decodedMatches.All(x => x.ParamValue == "?"))
                return logDetails;

            decodedMatches.Reverse();   //Need to reverse so that @p10 comes before @p1

            for (int i = 1; i < messageLines.Length; i++)
            {
                var lineToUpdate = messageLines[i];
                foreach (var param in decodedMatches)
                {
                    lineToUpdate = lineToUpdate.Replace(param.ParamName, param.ValueToInsert);
                }
                messageLines[i] = lineToUpdate;
            }

            return string.Join("\r\n", messageLines.Skip(1));
        }

        public void EnableSensitiveDataLogging(HttpContext httpContext)
        {
            var loggingOptions = GetLoggingOptions(httpContext);
            loggingOptions.SetPropertyValue("IsSensitiveDataLoggingEnabled", true);
        }

        public object GetLoggingOptions(HttpContext httpContext)
        {
            var entityFrameworkCoreAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.FullName.StartsWith("Microsoft.EntityFrameworkCore,"));
            Type loggingOptionsType = entityFrameworkCoreAssembly.GetType("Microsoft.EntityFrameworkCore.Diagnostics.ILoggingOptions");
            if (loggingOptionsType == null)
                return null;
            var loggingOptions = httpContext.RequestServices.GetService(loggingOptionsType);
            return loggingOptions;

            //var efOptions = (Microsoft.EntityFrameworkCore.Internal.LoggingOptions)scope.ServiceProvider.GetService(typeof(Microsoft.EntityFrameworkCore.Diagnostics.ILoggingOptions));
            ////efOptions.IsSensitiveDataLoggingEnabled = true;
            //efOptions.GetType().GetProperty("IsSensitiveDataLoggingEnabled").SetValue(efOptions, true);
        }

        public void EnableSensitiveDataLogging(IServiceProvider serviceProvider)
        {
            var loggingOptions = GetLoggingOptions(serviceProvider);
            loggingOptions.SetPropertyValue("IsSensitiveDataLoggingEnabled", true);
        }

        public object GetLoggingOptions(IServiceProvider serviceProvider)
        {
            var entityFrameworkCoreAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.FullName.StartsWith("Microsoft.EntityFrameworkCore,"));
            Type loggingOptionsType = entityFrameworkCoreAssembly.GetType("Microsoft.EntityFrameworkCore.Diagnostics.ILoggingOptions");
            if (loggingOptionsType == null)
                return null;
            var loggingOptions = serviceProvider.GetService(loggingOptionsType);
            return loggingOptions;

            //var efOptions = (Microsoft.EntityFrameworkCore.Internal.LoggingOptions)scope.ServiceProvider.GetService(typeof(Microsoft.EntityFrameworkCore.Diagnostics.ILoggingOptions));
            ////efOptions.IsSensitiveDataLoggingEnabled = true;
            //efOptions.GetType().GetProperty("IsSensitiveDataLoggingEnabled").SetValue(efOptions, true);
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
        public string Schema;
        public string TableName;

        public string SchemaAndTableName { get; internal set; }
    }

    public class AppDbContextSqlResults
    {
        public int AffectedRecords;
        public List<object[]> Rows = new List<object[]>();
        public List<string> Columns = new List<string>();
        public string Error;
    }
}
