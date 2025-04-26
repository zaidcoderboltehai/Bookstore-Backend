using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bookstore.API.Swagger
{
    /// <summary>
    /// Swagger operation filter to handle file upload endpoints
    /// </summary>
    public class FileUploadOperationFilter : IOperationFilter
    {
        /// <summary>
        /// Applies filter settings to Swagger documentation
        /// </summary>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Check if method has IFormFile parameter
            var fileParams = context.MethodInfo.GetParameters()
                .Where(p => p.ParameterType == typeof(IFormFile))
                .ToList();

            if (!fileParams.Any())
                return;

            // Configure multipart/form-data schema
            var uploadFileMediaType = new OpenApiMediaType
            {
                Schema = new OpenApiSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, OpenApiSchema>
                    {
                        ["file"] = new OpenApiSchema
                        {
                            Type = "string",
                            Format = "binary",
                            Description = "Select CSV file for import",
                            Nullable = false
                        }
                    },
                    Required = new HashSet<string> { "file" }
                }
            };

            // Override request body configuration
            operation.RequestBody = new OpenApiRequestBody
            {
                Content = { ["multipart/form-data"] = uploadFileMediaType },
                Description = "CSV file upload for bulk import",
                Required = true
            };

            // ✨ Remove the auto-generated “Content-Type” parameter so Swagger UI will supply its own boundary
            operation.Parameters = operation.Parameters
                .Where(p => !p.Name.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
}
