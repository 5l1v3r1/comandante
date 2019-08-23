using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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

        //private Expression<Func<Goods, bool>> LambdaConstructor(string propertyName, string inputText, Condition condition)
        //{

        //    var item = Expression.Parameter(typeof(Goods), "item");
        //    var prop = Expression.Property(item, propertyName);
        //    var propertyInfo = typeof(Goods).GetProperty(propertyName);
        //    var value = Expression.Constant(Convert.ChangeType(inputText, propertyInfo.PropertyType));
        //    BinaryExpression equal;
        //    switch (condition)
        //    {
        //        case Condition.eq:
        //            equal = Expression.Equal(prop, value);
        //            break;
        //        case Condition.gt:
        //            equal = Expression.GreaterThan(prop, value);
        //            break;
        //        case Condition.gte:
        //            equal = Expression.GreaterThanOrEqual(prop, value);
        //            break;
        //        case Condition.lt:
        //            equal = Expression.LessThan(prop, value);
        //            break;
        //        case Condition.lte:
        //            equal = Expression.LessThanOrEqual(prop, value);
        //            break;
        //        default:
        //            equal = Expression.Equal(prop, value);
        //            break;
        //    }
        //    var lambda = Expression.Lambda<Func<Goods, bool>>(equal, item);
        //    return lambda;
        //}

        public AppDbContextSqlResult RunSql(HttpContext httpContext, string contextName, string sql, int maxRowsReads = 1000)
        {
            try {
                Type dbContextType = GetDbContextTypes().FirstOrDefault(x => x.Name == contextName);
                object dbContext = httpContext?.RequestServices?.GetService(dbContextType);
                if (dbContext == null)
                    return AppDbContextSqlResult.Error("Cannot find DbContext: " + contextName);

                object database = dbContext.GetPropertyValue("Database");
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
                return new AppDbContextSqlResult
                {
                    Columns = columns,
                    Rows = rows,
                    AffectedRecords = recordsAffected
                };
            }
            catch (Exception ex)
            {
                return AppDbContextSqlResult.Error(ex.GetDetails());
            }
        }

        public AppDbContextEntitiesResult GetAll(HttpContext httpContext, string contextName, AppDbContextEntityInfo entityInfo)
        {
            return GetAll(httpContext, contextName, entityInfo, new Dictionary<string, string>());
        }
        public AppDbContextEntitiesResult GetAll(HttpContext httpContext, string contextName, AppDbContextEntityInfo entityInfo, Dictionary<string, string> where)
        {
            AppDbContextEntitiesResult results = new AppDbContextEntitiesResult();
            results.Fields = entityInfo.Fields;
            try
            {
                Type dbContextType = GetDbContextTypes().FirstOrDefault(y => y.Name == contextName);
                object dbContext = httpContext?.RequestServices?.GetService(dbContextType);
                if (dbContext == null)
                    return AppDbContextEntitiesResult.Error("Cannot find DbContext: " + contextName);

                object dbSet = dbContext.InvokeGenericMethod("Set", new[] { entityInfo.ClrType });

                ParameterExpression x = Expression.Parameter(entityInfo.ClrType, "x");
                BinaryExpression lastOperation = null;
                foreach (var whereCondition in where)
                {
                    MemberExpression leftSide = Expression.Property(x, whereCondition.Key);
                    ConstantExpression rightSide = Expression.Constant(whereCondition.Value);
                    BinaryExpression operation = Expression.Equal(leftSide, rightSide);
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

                var toListMethod = typeof(System.Linq.Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                        .FirstOrDefault(m => m.Name == "ToList" && m.GetParameters().Count() == 1);
                MethodInfo genericToListMethod = toListMethod.MakeGenericMethod(new[] { entityInfo.ClrType });
                dbSet = genericToListMethod.Invoke(null, new object[] { dbSet });

                foreach (var entity in dbSet as System.Collections.IEnumerable)
                {
                    object[] row = new object[entityInfo.Fields.Count];
                    for (int i = 0; i < entityInfo.Fields.Count; i++)
                    {
                        row[i] = dbContext
                            .InvokeMethod("Entry", entity)
                            .InvokeMethod("Property", entityInfo.Fields[i].Name)
                            .GetPropertyValue("CurrentValue");
                    }
                    results.Rows.Add(row);
                }
                return results;
            }
            catch (Exception ex)
            {
                results.Errors.Add(ex.GetDetails());
            }
            return results;
        }

        public AppDbContextEntityResult GetEntityByPrimaryKey(HttpContext httpContext, string contextName, AppDbContextEntityInfo entityInfo, Dictionary<string, string> pkValues)
        {
            Type dbContextType = GetDbContextTypes().FirstOrDefault(y => y.Name == contextName);
            object dbContext = httpContext?.RequestServices?.GetService(dbContextType);
            if (dbContext == null)
                return AppDbContextEntityResult.Error("Cannot find DbContext: " + contextName);

            try
            {
                var primaryKeys = entityInfo.Fields
                    .Where(x => x.IsPrimaryKey)
                    .Select(x => pkValues.FirstOrDefault(v => v.Key == x.Name).Value.ConvertToType(x.FieldInfo.FieldType))
                    .ToArray();

                var entity = dbContext.InvokeMethod("Find", entityInfo.ClrType, primaryKeys);
                if (entity == null)
                {
                    var pkString = string.Join(";", pkValues.Select(x => x.Key + "=" + x.Value).ToArray());
                    return AppDbContextEntityResult.Error($"Cannot find entity {entityInfo.ClrTypeName} with primary key: {pkString}");
                }
                var fieldsValues = entityInfo.Fields.ToDictionary(
                    x => x.Name,
                    x =>
                    {
                        return dbContext
                            .InvokeMethod("Entry", entity)
                            .InvokeMethod("Property", x.Name)
                            .GetPropertyValue("CurrentValue");
                    });
                return new AppDbContextEntityResult
                {
                    Entity = entity,
                    FieldsValues = fieldsValues
                };
            } catch (Exception ex)
            {
                var pkString = string.Join(";", pkValues.Select(x => x.Key + "=" + x.Value).ToArray());
                return AppDbContextEntityResult.Error($"Cannot find entity {entityInfo.ClrTypeName} with primary key: {pkString}. ex.GetDetails()");
            }
        }

        public AppDbContextEntityResult Add(HttpContext httpContext, string contextName, AppDbContextEntityInfo entityInfo, Dictionary<string, string> fieldsValues)
        {
            Type dbContextType = GetDbContextTypes().FirstOrDefault(y => y.Name == contextName);
            object dbContext = httpContext?.RequestServices?.GetService(dbContextType);
            if (dbContext == null)
                return AppDbContextEntityResult.Error("Cannot find DbContext: " + contextName);

            var entity = Activator.CreateInstance(entityInfo.ClrType);
            AppDbContextEntityResult result = new AppDbContextEntityResult();
            result.Entity = entity;
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
                    var value = field.Value.ConvertToType(entityField.ClrType);
                    dbContext
                        .InvokeMethod("Entry", entity)
                        .InvokeMethod("Property", field.Key)
                        .SetPropertyValue("CurrentValue", value);
                }catch(Exception ex)
                {
                    result.Errors.Add($"Cannot set entity property {field.Key} to {field.Value}. Error: {ex.GetDetails()}");
                }
            }

            if (result.Errors.Count > 0)
                return result;

            try
            {
                dbContext.InvokeMethod("Add", entity);
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Cannot add entity to the context. Error: {ex.GetDetails()}");
                return result;
            }

            try
            {
                dbContext.InvokeMethod("SaveChanges");
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Cannot save entity. Error: {ex.GetDetails()}");
                return result;
            }

            return result;

        }

        public AppDbContextEntityResult Update(HttpContext httpContext, string contextName, AppDbContextEntityInfo entityInfo, Dictionary<string, string> pkValues , Dictionary<string, string> fieldsValues)
        {
            Type dbContextType = GetDbContextTypes().FirstOrDefault(y => y.Name == contextName);
            object dbContext = httpContext?.RequestServices?.GetService(dbContextType);
            if (dbContext == null)
                return AppDbContextEntityResult.Error("Cannot find DbContext: " + contextName);

            var entityResult = GetEntityByPrimaryKey(httpContext, contextName, entityInfo, pkValues);
            if (entityResult.IsSuccess == false)
                return AppDbContextEntityResult.Error(entityResult.Errors);
            object entity = entityResult.Entity;

            AppDbContextEntityResult result = new AppDbContextEntityResult();
            result.Entity = entity;
            foreach (var field in entityInfo.Fields)
            {
                object value = fieldsValues.FirstOrDefault(x => x.Key == field.Name).Value;
                try
                {
                    value = value.ConvertToType(field.ClrType);
                    dbContext
                        .InvokeMethod("Entry", entity)
                        .InvokeMethod("Property", field.Name)
                        .SetPropertyValue("CurrentValue", value);
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Cannot set entity property {field.Name} to {value}. Error: {ex.GetDetails()}");
                }
            }

            if (result.Errors.Count > 0)
                return result;

            try {
                dbContext.InvokeMethod("Update", entity);
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Cannot update entity in the context. Error: {ex.GetDetails()}");
                return result;
            }

            try
            {
                dbContext.InvokeMethod("SaveChanges");
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Cannot save entity. Error: {ex.GetDetails()}");
                return result;
            }

            return result;
        }

        public List<AppDbContextInfo> GetDbContexts(HttpContext httpContext)
        {
            List<AppDbContextInfo> appDbContexts = new List<AppDbContextInfo>();
            foreach (var addDbContextType in GetDbContextTypes())
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
                        var entitiesTypes = model.InvokeMethod("GetEntityTypes") as System.Collections.IEnumerable;
                        foreach (var entityType in entitiesTypes)
                        {
                            var relationalEntity = relationalMetadataExtensions.InvokeStaticMethod("Relational", entityType);
                            var schema = relationalEntity.GetPropertyValue("Schema")?.ToString();
                            var tableName = relationalEntity.GetPropertyValue("TableName")?.ToString();

                            var debugView = entityType.GetPropertyValue("DebugView")?.GetPropertyValue("View"); ;

                            var properties = entityType.GetFieldValue("_properties") as System.Collections.IEnumerable;
                            var entityFields = new List<AppDbContextEntityFieldInfo>();
                            foreach (var property in properties)
                            {
                                entityFields.Add(new AppDbContextEntityFieldInfo
                                {
                                    Name = property.GetPropertyValue("Value")?.GetPropertyValue("Name")?.ToString(),
                                    ClrType = property.GetPropertyValue("Value")?.GetPropertyValue("ClrType") as Type,
                                    FieldInfo = property.GetPropertyValue("Value")?.GetPropertyValue("FieldInfo") as FieldInfo,
                                    IsPrimaryKey = property.GetPropertyValue("Value")?.GetPropertyValue("PrimaryKey") != null,
                                });
                            }

                            var schemaAndTableName = string.IsNullOrEmpty(schema) == false ? schema + "." + tableName : tableName;
                            appDbContextInfo.Entities.Add(new AppDbContextEntityInfo
                            {
                                NavigationName = entityType.GetPropertyValue("DefiningNavigationName")?.ToString(),
                                ClrTypeName = entityType.GetPropertyValue("ClrType")?.ToString(),
                                ClrType = entityType.GetPropertyValue("ClrType") as Type,
                                Schema = schema,
                                TableName = tableName,
                                SchemaAndTableName = schemaAndTableName,
                                DebugView = debugView?.ToString(),
                                Fields = entityFields,
                            });

                        }

                        var modelDebugView = model.GetPropertyValue("DebugView")?.GetPropertyValue("View"); ;
                        appDbContextInfo.DebugView = modelDebugView?.ToString();
                    }
                }

            }
            return appDbContexts;
        }

        public List<Type> GetDbContextTypes()
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
        public string DebugView;

    }

    public class AppDbContextEntityInfo
    {
        public string NavigationName;
        public Type ClrType;
        public string ClrTypeName;
        public string Schema;
        public string TableName;
        public string SchemaAndTableName;
        public List<AppDbContextEntityFieldInfo> Fields;
        public string DebugView;
    }

    public class AppDbContextEntityFieldInfo
    {
        public string Name;
        public FieldInfo FieldInfo;
        public bool IsPrimaryKey;
        public Type ClrType;
    }

    public class AppDbContextSqlResult
    {
        public int AffectedRecords;
        public List<object[]> Rows = new List<object[]>();
        public List<string> Columns = new List<string>();
        public List<string> Errors = new List<string>();

        public bool IsSuccess => Errors.Count == 0;

        public static AppDbContextSqlResult Error(string error)
        {
            AppDbContextSqlResult result = new AppDbContextSqlResult();
            result.Errors.Add(error);
            return result;
        }
    }

    public class AppDbContextEntitiesResult
    {
        public int AffectedRecords;
        public List<object[]> Rows = new List<object[]>();
        public List<AppDbContextEntityFieldInfo> Fields = new List<AppDbContextEntityFieldInfo>();
        public List<string> Errors = new List<string>();

        public bool IsSuccess => Errors.Count == 0;

        public static AppDbContextEntitiesResult Error(string error)
        {
            AppDbContextEntitiesResult result = new AppDbContextEntitiesResult();
            result.Errors.Add(error);
            return result;
        }
    }

    public class AppDbContextEntityResult
    {
        public object Entity;
        public Dictionary<string, object> FieldsValues = new Dictionary<string, object>();
        public List<string> Errors = new List<string>();

        public bool IsSuccess => Entity != null && Errors.Count == 0;

        public static AppDbContextEntityResult Error(string error)
        {
            AppDbContextEntityResult result = new AppDbContextEntityResult();
            result.Errors.Add(error);
            return result;
        }

        public static AppDbContextEntityResult Error(List<string> errors)
        {
            AppDbContextEntityResult result = new AppDbContextEntityResult();
            result.Errors = errors;
            return result;
        }

    }
    
}
