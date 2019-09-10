using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Comandante.Services
{
    public class EntityFrameworkService
    {
        public EntityFrameworkService()
        {

        }

        public DbContextSqlResult RunSql(HttpContext httpContext, string contextName, string sql, int maxRowsReads = 1000)
        {
            try {
                Type dbContextType = GetDbContextTypes().FirstOrDefault(x => x.Name == contextName);
                object dbContext = httpContext?.RequestServices?.GetService(dbContextType);
                if (dbContext == null)
                    return DbContextSqlResult.Error("Cannot find DbContext: " + contextName);

                object database = Reflector.GetPropertyValue(dbContext, "Database");
                List<string> columns = new List<string>();
                List<object[]> rows = new List<object[]>();
                int recordsAffected = -1;
                var relatoinDatabaseExtensionType = GetRelationalDatabaseExtensionType();
                using (var dbConnection = Reflector.InvokeStaticMethod(relatoinDatabaseExtensionType, "GetDbConnection", database) as IDisposable)
                {
                    Reflector.InvokeMethod(dbConnection, "Open");
                    using (var dbCommand = Reflector.InvokeMethod(dbConnection, "CreateCommand") as IDisposable)
                    {
                        Reflector.SetPropertyValue(dbCommand, "CommandText", sql);
                        using (var reader = Reflector.InvokeMethod(dbCommand, "ExecuteReader") as IDisposable)
                        {
                            while ((bool)Reflector.InvokeMethod(reader, "Read"))
                            {
                                if (columns.Count == 0)
                                {
                                    int fieldCount = (int)Reflector.GetPropertyValue(reader, "FieldCount");
                                    for (int i = 0; i < fieldCount; i++)
                                    {
                                        columns.Add(Reflector.InvokeMethod(reader, "GetName", i)?.ToString());
                                    }
                                }
                                object[] row = new object[columns.Count];
                                for (int i = 0; i < columns.Count; i++)
                                {
                                    row[i] = Reflector.InvokeMethod(reader, "GetValue", i);
                                }
                                rows.Add(row);
                                if (rows.Count >= maxRowsReads)
                                    break;
                            }
                            recordsAffected = (int)Reflector.GetPropertyValue(reader, "RecordsAffected");
                        }
                    }
                }
                return new DbContextSqlResult
                {
                    Columns = columns,
                    Rows = rows,
                    AffectedRecords = recordsAffected
                };
            }
            catch (Exception ex)
            {
                return DbContextSqlResult.Error(ex.GetAllMessages());
            }
        }

        public DbContextEntitiesResult GetAll(HttpContext httpContext, string contextName, DbContextEntityInfo entityInfo)
        {
            return GetAll(httpContext, contextName, entityInfo, new Dictionary<string, string>(), null);
        }

        public DbContextEntitiesResult GetAll(HttpContext httpContext, string contextName, DbContextEntityInfo entityInfo, Dictionary<string, string> where, int? page)
        {
            DbContextEntitiesResult results = new DbContextEntitiesResult();
            results.Fields = entityInfo.Fields;
            try
            {
                Type dbContextType = GetDbContextTypes().FirstOrDefault(y => y.Name == contextName);
                DbContext dbContext = httpContext?.RequestServices?.GetService(dbContextType) as DbContext;
                if (dbContext == null)
                    return DbContextEntitiesResult.Error("Cannot find DbContext: " + contextName);

                object dbSet = Reflector.InvokeGenericMethod(dbContext, "Set", new[] { entityInfo.ClrType });

                ParameterExpression x = Expression.Parameter(entityInfo.ClrType, "x");
                BinaryExpression lastOperation = null;
                foreach (var whereCondition in where)
                {
                    var fieldInfo = entityInfo.Fields.FirstOrDefault(f => f.Name == whereCondition.Key);
                    var operation = Expression.Equal(
                        Expression.Call(typeof(EF), nameof(EF.Property), new[] { fieldInfo.ClrType }, x, Expression.Constant(whereCondition.Key)),
                        Expression.Constant(Binder.ConvertToType(whereCondition.Value, fieldInfo.ClrType), fieldInfo.ClrType));

                    if (lastOperation == null)
                        lastOperation = operation;
                    else
                        lastOperation = Expression.And(lastOperation, operation);
                }

                if (lastOperation != null)
                {
                    Type delegateType = typeof(Func<,>).MakeGenericType(entityInfo.ClrType, typeof(bool));
                    LambdaExpression predicate = Expression.Lambda(delegateType, lastOperation, x);

                    var whereMethod = typeof(System.Linq.Queryable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                        .FirstOrDefault(m => m.Name == "Where" && m.GetParameters().Count() == 2);

                    MethodInfo genericWhereMethod = whereMethod.MakeGenericMethod(new[] { entityInfo.ClrType });
                    dbSet = genericWhereMethod.Invoke(null, new object[] { dbSet, predicate });
                }

                int take = 200;
                int skip = (page.GetValueOrDefault(1) - 1) * 200;

                var skipMethod = typeof(System.Linq.Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                        .FirstOrDefault(m => m.Name == "Skip" && m.GetParameters().Count() == 2);
                MethodInfo genericSkipMethod = skipMethod.MakeGenericMethod(new[] { entityInfo.ClrType });
                dbSet = genericSkipMethod.Invoke(null, new object[] { dbSet, skip });
                //dbSet = typeof(System.Linq.Enumerable).InvokeStaticGenericMethod("Skip", new[] { entityInfo.ClrType }, dbSet, take);

                var takeMethod = typeof(System.Linq.Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                        .FirstOrDefault(m => m.Name == "Take" && m.GetParameters().Count() == 2);
                MethodInfo genericTakeMethod = takeMethod.MakeGenericMethod(new[] { entityInfo.ClrType });
                dbSet = genericTakeMethod.Invoke(null, new object[] { dbSet, take });
                //dbSet = typeof(System.Linq.Enumerable).InvokeStaticGenericMethod("Take", new[] { entityInfo.ClrType }, dbSet, take);

                var toListMethod = typeof(System.Linq.Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                        .FirstOrDefault(m => m.Name == "ToList" && m.GetParameters().Count() == 1);
                MethodInfo genericToListMethod = toListMethod.MakeGenericMethod(new[] { entityInfo.ClrType });
                dbSet = genericToListMethod.Invoke(null, new object[] { dbSet });

                foreach (var entity in dbSet as System.Collections.IEnumerable)
                {
                    var entityEntry = dbContext.Entry(entity);
                    object[] row = new object[entityInfo.Fields.Count];
                    for (int i = 0; i < entityInfo.Fields.Count; i++)
                        row[i] = entityEntry.Property(entityInfo.Fields[i].Name).CurrentValue;
                    results.Rows.Add(row);
                }
                return results;
            }
            catch (Exception ex)
            {
                results.Errors.Add(ex.GetAllMessages());
            }
            return results;
        }

        public DbContextEntityResult GetEntityByPrimaryKey(HttpContext httpContext, string contextName, DbContextEntityInfo entityInfo, Dictionary<string, string> pkValues)
        {
            Type dbContextType = GetDbContextTypes().FirstOrDefault(y => y.Name == contextName);
            DbContext dbContext = httpContext?.RequestServices?.GetService(dbContextType) as DbContext;
            if (dbContext == null)
                return DbContextEntityResult.Error("Cannot find DbContext: " + contextName);

            try
            {
                var primaryKeys = entityInfo.Fields
                    .Where(x => x.IsPrimaryKey)
                    .Select(x => Binder.ConvertToType(pkValues.FirstOrDefault(v => v.Key == x.Name).Value, x.FieldInfo.FieldType))
                    .ToArray();

                var entity = dbContext.Find(entityInfo.ClrType, primaryKeys);
                if (entity == null)
                {
                    var pkString = string.Join(";", pkValues.Select(x => x.Key + "=" + x.Value).ToArray());
                    return DbContextEntityResult.Error($"Cannot find entity {entityInfo.ClrTypeName} with primary key: {pkString}");
                }
                var entityEntry = dbContext.Entry(entity);
                var fieldsValues = entityInfo.Fields.ToDictionary(
                    x => x.Name,
                    x => entityEntry.Property(x.Name).CurrentValue);
                return new DbContextEntityResult
                {
                    Entity = entity,
                    FieldsValues = fieldsValues
                };
            } catch (Exception ex)
            {
                var pkString = string.Join(";", pkValues.Select(x => x.Key + "=" + x.Value).ToArray());
                return DbContextEntityResult.Error($"Cannot find entity {entityInfo.ClrTypeName} with primary key: {pkString}. ex.GetDetails()");
            }
        }

        public DbContextEntityResult Add(HttpContext httpContext, string contextName, DbContextEntityInfo entityInfo, Dictionary<string, string> fieldsValues)
        {
            Type dbContextType = GetDbContextTypes().FirstOrDefault(y => y.Name == contextName);
            DbContext dbContext = httpContext?.RequestServices?.GetService(dbContextType) as DbContext;
            if (dbContext == null)
                return DbContextEntityResult.Error("Cannot find DbContext: " + contextName);

            var entity = Activator.CreateInstance(entityInfo.ClrType);
            DbContextEntityResult result = new DbContextEntityResult();
            result.Entity = entity;
            var entityEntry = dbContext.Entry(entity);
            foreach (var field in fieldsValues)
            {
                try
                {
                    var entityField = entityInfo.Fields.FirstOrDefault(x => x.Name == field.Key);
                    if (entityField == null)
                    {
                        result.Errors.Add($"Property {field.Key} does not exist in the entity {entityInfo.ClrTypeName}");
                        continue;
                    }
                    var value = Binder.ConvertToType(field.Value, entityField.ClrType);
                    entityEntry.Property(field.Key).CurrentValue = value;
                }catch(Exception ex)
                {
                    result.Errors.Add($"Cannot set entity property {field.Key} to {field.Value}. Error: {ex.GetAllMessages()}");
                }
            }

            if (result.Errors.Count > 0)
                return result;

            try
            {
                dbContext.Add(entity);
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Cannot add entity to the context. Error: {ex.GetAllMessages()}");
                return result;
            }

            try
            {
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Cannot save entity. Error: {ex.GetAllMessages()}");
                return result;
            }

            return result;
        }

        public DbContextEntityResult Update(HttpContext httpContext, string contextName, DbContextEntityInfo entityInfo, Dictionary<string, string> pkValues , Dictionary<string, string> fieldsValues)
        {
            Type dbContextType = GetDbContextTypes().FirstOrDefault(y => y.Name == contextName);
            DbContext dbContext = httpContext?.RequestServices?.GetService(dbContextType) as DbContext;
            if (dbContext == null)
                return DbContextEntityResult.Error("Cannot find DbContext: " + contextName);

            var entityResult = GetEntityByPrimaryKey(httpContext, contextName, entityInfo, pkValues);
            if (entityResult.IsSuccess == false)
                return DbContextEntityResult.Error(entityResult.Errors);
            object entity = entityResult.Entity;

            DbContextEntityResult result = new DbContextEntityResult();
            result.Entity = entity;
            var entityEntry = dbContext.Entry(entity);
            foreach (var field in entityInfo.Fields)
            {
                object value = fieldsValues.FirstOrDefault(x => x.Key == field.Name).Value;
                try
                {
                    if (Binder.IsConvertionSupported(field.ClrType) == false)
                    {
                        result.Warnings.Add($"Type of property {field.Name} is not supported.");
                        continue;
                    }
                    value = Binder.ConvertToType(value, field.ClrType);
                    entityEntry.Property(field.Name).CurrentValue = value;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Cannot set entity property {field.Name} to {value}. Error: {ex.GetAllMessages()}");
                }
            }

            if (result.Errors.Count > 0)
                return result;

            try {
                dbContext.Update(entity);
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Cannot update entity in the context. Error: {ex.GetAllMessages()}");
                return result;
            }

            try
            {
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Cannot save entity. Error: {ex.GetAllMessages()}");
                return result;
            }

            return result;
        }

        public List<DbContextInfo> GetDbContexts(HttpContext httpContext)
        {
            List<DbContextInfo> appDbContexts = new List<DbContextInfo>();
            foreach (var dbContextType in GetDbContextTypes())
            {
                DbContextInfo appDbContextInfo = new DbContextInfo();
                appDbContexts.Add(appDbContextInfo);
                appDbContextInfo.Type = dbContextType;
                appDbContextInfo.Name = dbContextType.Name;
                DbContext dbContext = httpContext?.RequestServices?.GetService(dbContextType) as DbContext;
                if (dbContext != null)
                {
                    var database = dbContext.Database;
                    var relationalDatabaseExtensionType = GetRelationalDatabaseExtensionType();
                    var relationalMetadataExtensions = GetRelationalMetadataExtensionsType();
                    if (database != null)
                    {
                        appDbContextInfo.ProviderName = database.ProviderName;
                        if (relationalDatabaseExtensionType != null)
                        {
                            appDbContextInfo.Migrations = (Reflector.InvokeStaticMethod(relationalDatabaseExtensionType, "GetMigrations", database) as IEnumerable<string>)?.ToList() ?? new List<string>();
                            appDbContextInfo.PendingMigrations = (Reflector.InvokeStaticMethod(relationalDatabaseExtensionType, "GetPendingMigrations", database) as IEnumerable<string>)?.ToList() ?? new List<string>();
                            appDbContextInfo.AppliedMigrations = (Reflector.InvokeStaticMethod(relationalDatabaseExtensionType, "GetAppliedMigrations", database) as IEnumerable<string>)?.ToList() ?? new List<string>();
                        }
                    }
                    var model = dbContext.Model;
                    if (model != null)
                    {
                        foreach (var entityType in model.GetEntityTypes())
                        {
                            var relationalEntity = Reflector.InvokeStaticMethod(relationalMetadataExtensions, "Relational", entityType);
                            var schema = Reflector.GetPropertyValue(relationalEntity, "Schema")?.ToString();
                            var tableName = Reflector.GetPropertyValue(relationalEntity, "TableName")?.ToString();

                            var debugView = Reflector.GetPropertyValue(Reflector.GetPropertyValue(entityType, "DebugView"), "View");

                            var clrProperties = entityType.ClrType.GetProperties();
                            var clrPropertiesOrder = clrProperties.ToDictionary(x => x.Name, x => clrProperties.IndexOf(x));

                            var entityProperties = entityType.GetProperties().OrderBy(x => x.IsShadowProperty ? clrPropertiesOrder.GetValueOrDefault(x.Name.Substring(0, x.Name.Length - 2), 0) : clrPropertiesOrder.GetValueOrDefault(x.Name, 0));
                            
                            var entityFields = new List<DbContextEntityFieldInfo>();
                            foreach (Microsoft.EntityFrameworkCore.Metadata.Internal.Property property in entityProperties)
                            {
                                entityFields.Add(new DbContextEntityFieldInfo
                                {
                                    Name = property.Name,
                                    ClrType = property.ClrType,
                                    FieldInfo = property.FieldInfo,
                                    IsPrimaryKey = property.IsPrimaryKey(),
                                    IsForeignKey = property.IsForeignKey(),
                                    ForeignEntityName = property.ForeignKeys?.FirstOrDefault()?.PrincipalEntityType?.Name,
                                    ForeignEntityClrType = property.ForeignKeys?.FirstOrDefault()?.PrincipalEntityType?.ClrType,
                                    ForeignEntityFieldName = property.ForeignKeys?.FirstOrDefault().PrincipalKey.Properties.FirstOrDefault().Name
                                });
                            }

                            var schemaAndTableName = string.IsNullOrEmpty(schema) == false ? schema + "." + tableName : tableName;
                            appDbContextInfo.Entities.Add(new DbContextEntityInfo
                            {
                                NavigationName = Reflector.GetPropertyValue(entityType, "DefiningNavigationName")?.ToString(),
                                ClrTypeName = Reflector.GetPropertyValue(entityType, "ClrType")?.ToString(),
                                ClrType = Reflector.GetPropertyValue(entityType, "ClrType") as Type,
                                Schema = schema,
                                TableName = tableName,
                                SchemaAndTableName = schemaAndTableName,
                                DebugView = debugView?.ToString(),
                                Fields = entityFields,
                            });

                        }

                        var modelDebugView = Reflector.GetPropertyValue(Reflector.GetPropertyValue(model, "DebugView"), "View");
                        appDbContextInfo.DebugView = modelDebugView?.ToString();
                    }
                }

            }
            return appDbContexts;
        }

        public List<Type> GetDbContextTypes()
        {
            List<Type> appDbCotexts = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName.StartsWith("Microsoft.EntityFrameworkCore"))
                    continue;
                if (assembly.FullName.StartsWith("Microsoft.AspNetCore"))
                    continue;
                foreach (TypeInfo type in assembly.DefinedTypes)
                {
                    if (type.IsSubclassOf(typeof(DbContext)))
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
            //loggingOptions.SetPropertyValue("IsSensitiveDataLoggingEnabled", true);
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
            //loggingOptions.SetPropertyValue("IsSensitiveDataLoggingEnabled", true);
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

    public class DbContextInfo
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
        public List<DbContextEntityInfo> Entities = new List<DbContextEntityInfo>();
        public string DebugView;

    }

    public class DbContextEntityInfo
    {
        public string NavigationName;
        public Type ClrType;
        public string ClrTypeName;
        public string Schema;
        public string TableName;
        public string SchemaAndTableName;
        public List<DbContextEntityFieldInfo> Fields;
        public string DebugView;
    }

    public class DbContextEntityFieldInfo
    {
        public string Name;
        public FieldInfo FieldInfo;
        public bool IsPrimaryKey;
        public bool IsForeignKey;
        public Type ClrType;
        public string ForeignEntityName;
        public Type ForeignEntityClrType;
        public string ForeignEntityFieldName;
    }

    public class DbContextSqlResult
    {
        public int AffectedRecords;
        public List<object[]> Rows = new List<object[]>();
        public List<string> Columns = new List<string>();
        public List<string> Errors = new List<string>();

        public bool IsSuccess => Errors.Count == 0;

        public static DbContextSqlResult Error(string error)
        {
            DbContextSqlResult result = new DbContextSqlResult();
            result.Errors.Add(error);
            return result;
        }
    }

    public class DbContextEntitiesResult
    {
        public int AffectedRecords;
        public List<object[]> Rows = new List<object[]>();
        public List<DbContextEntityFieldInfo> Fields = new List<DbContextEntityFieldInfo>();
        public List<string> Errors = new List<string>();

        public bool IsSuccess => Errors.Count == 0;

        public static DbContextEntitiesResult Error(string error)
        {
            DbContextEntitiesResult result = new DbContextEntitiesResult();
            result.Errors.Add(error);
            return result;
        }
    }

    public class DbContextEntityResult
    {
        public object Entity;
        public Dictionary<string, object> FieldsValues = new Dictionary<string, object>();
        public List<string> Errors = new List<string>();
        public List<string> Warnings = new List<string>();
        
        public bool IsSuccess => Entity != null && Errors.Count == 0;

        public static DbContextEntityResult Error(string error)
        {
            DbContextEntityResult result = new DbContextEntityResult();
            result.Errors.Add(error);
            return result;
        }

        public static DbContextEntityResult Error(List<string> errors)
        {
            DbContextEntityResult result = new DbContextEntityResult();
            result.Errors = errors;
            return result;
        }

    }
    
}
