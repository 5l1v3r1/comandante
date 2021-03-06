﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Comandante.Pages;
using Comandante.PagesRenderer;
using Comandante.Services;
using Microsoft.AspNetCore.Mvc;

namespace Comandante.Pages
{
    public class EFEntityEditor : EmbeddedViewModel
    {
        public EFEntityEditorModel Model { get; set; }

        public override async Task<EmbededViewResult> Execute()
        {
            Model = new EFEntityEditorModel();

            var contextName = this.HttpContext.Request.Query.FirstOrDefault(x => x.Key == "_dbContext").Value.ToString().Trim();
            var entityName = this.HttpContext.Request.Query.FirstOrDefault(x => x.Key == "_entity").Value.ToString().Trim();
            var entityInfo = new EntityFrameworkService()
                .GetDbContexts(this.HttpContext)
                .FirstOrDefault(x => x.Name == contextName)
                .Entities
                .FirstOrDefault(x => x.ClrTypeName == entityName);

            Model.EntityName = entityName;
            Model.EntityNamePart1 = string.Join(".", entityName?.Split(".").Reverse().Skip(1).Reverse());
            Model.EntityNamePart2 = entityName?.Split(".").Last();

            var fieldsValues = new Dictionary<string, string>();

            var pkValues = this.HttpContext.Request.Query
                .Where(x => x.Key.StartsWith("_") == false && string.IsNullOrEmpty(x.Value.FirstOrDefault()) == false)
                .ToDictionary(x => x.Key.Trim(), x => x.Value.FirstOrDefault()?.ToString()?.Trim());
            var isUpdate = pkValues.Count > 0;

            if (IsPost())
            {
                fieldsValues = this.HttpContext.Request.Form
                    .Where(x => x.Key.StartsWith("_") == false && string.IsNullOrEmpty(x.Value.FirstOrDefault()) == false)
                    .ToDictionary(x => x.Key, x => x.Value.FirstOrDefault()?.ToString());
            }
            
            if (IsGet())
            {
                var entity = new EntityFrameworkService().GetEntityByPrimaryKey(this.HttpContext, contextName, entityInfo, pkValues);
                if (entity != null)
                    fieldsValues =  entity.FieldsValues.ToDictionary(x => x.Key, x => Binder.ConvertToString(x.Value));
            }

            Model.DbContext = contextName;
            Model.Entity = entityInfo;
            Model.FieldsWithValues = entityInfo.Fields.Select(x => (x, fieldsValues.FirstOrDefault(v => v.Key == x.Name).Value)).ToList();
            Model.IsUpdate = isUpdate;
            if (IsSubmit())
            {
                DbContextEntityResult result = null;
                if (isUpdate)
                    result = new EntityFrameworkService().Update(this.HttpContext, contextName, entityInfo, pkValues, fieldsValues);
                else
                    result = new EntityFrameworkService().Add(this.HttpContext, contextName, entityInfo, fieldsValues);
                if ((result?.IsSuccess).GetValueOrDefault(false))
                {
                    var routeValues = entityInfo.Fields.Where(x => x.IsPrimaryKey).ToDictionary(x => x.Name, x => Reflector.GetPropertyOrFieldValue(result.Entity, x.Name));
                    routeValues.TryAdd("_dbContext", contextName);
                    routeValues.TryAdd("_entity", entityInfo.ClrTypeName);
                    var url = Url.Link("/EFEntityEditor", routeValues);
                    return new EmbededViewRedirectResult(url);
                }
                Model.Errors = result.Errors;
                Model.Warnings = result.Warnings;
            }

            return await View();
        }
    }

    public class EFEntityEditorModel
    {
        public string DbContext;
        public DbContextEntityInfo Entity;
        public List<(DbContextEntityFieldInfo Field, string Value)> FieldsWithValues;
        public List<string> Errors;
        public List<string> Warnings;
        public bool IsUpdate;
        public string EntityName;
        public string EntityNamePart1;
        public string EntityNamePart2;
    }

   
}
