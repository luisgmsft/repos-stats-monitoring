using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

namespace Pnp.VSTSSync
{
    public static class WorkItems
    {
        private static JsonPatchDocument BuildDocument(IDictionary<string, object> fields, Operation operation)
        {
            var operations = fields
                .Select(f => new JsonPatchOperation() {
                    Path = f.Key,
                    Operation = operation,
                    Value = f.Value
                }).ToList();
            var document = new Microsoft.VisualStudio.Services.WebApi.Patch.Json.JsonPatchDocument();
            document.AddRange(operations);
            return document;
        }

        public static JsonPatchDocument BuildCreateDocument(IDictionary<string, object> fields)
        {
            return BuildDocument(fields, Operation.Add);
        }

        public static JsonPatchDocument BuildUpdateDocument(IDictionary<string, object> fields)
        {
            return BuildDocument(fields, Operation.Replace);
        }
    }
}
