using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EmployeeContactApi.Presentation.Swagger;

/// <summary>
/// Swagger document filter that merges multiple POST operations with different content types
/// into a single operation with multiple requestBody content types.
/// </summary>
public class MergePostOperationsFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Find all API descriptions for POST /api/employee
        var postDescriptions = context.ApiDescriptions
            .Where(api => api.HttpMethod?.ToUpper() == "POST"
                          && api.RelativePath == "api/employee")
            .ToList();

        if (postDescriptions.Count <= 1)
            return;

        // Get the path item for /api/employee
        if (!swaggerDoc.Paths.TryGetValue("/api/employee", out var pathItem))
            return;

        if (pathItem.Operations?.TryGetValue(HttpMethod.Post, out var postOperation) == true && postOperation is not null)
        {
            // Merge all content types from all POST operations
            var mergedRequestBody = new OpenApiRequestBody
            {
                Description = "Employee data in JSON, CSV, or file upload format",
                Required = true,
                Content = new Dictionary<string, OpenApiMediaType>()
            };

            // Add application/json content type
            mergedRequestBody.Content["application/json"] = new OpenApiMediaType
            {
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.Array,
                    Items = new OpenApiSchema
                    {
                        Type = JsonSchemaType.Object,
                        Properties = new Dictionary<string, IOpenApiSchema>
                        {
                            ["name"] = new OpenApiSchema { Type = JsonSchemaType.String, Default = JsonValue.Create("김철수") },
                            ["email"] = new OpenApiSchema { Type = JsonSchemaType.String, Default = JsonValue.Create("charles@example.com") },
                            ["tel"] = new OpenApiSchema { Type = JsonSchemaType.String, Default = JsonValue.Create("01075312468") },
                            ["joined"] = new OpenApiSchema { Type = JsonSchemaType.String, Default = JsonValue.Create("2018-03-07") }
                        },
                        Required = new HashSet<string> { "name", "email", "tel", "joined" }
                    }
                }
            };

            // Add text/csv content type
            mergedRequestBody.Content["text/csv"] = new OpenApiMediaType
            {
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Default = JsonValue.Create("김철수, charles@example.com, 01075312468, 2018.03.07")
                }
            };

            // Add multipart/form-data content type for file upload
            mergedRequestBody.Content["multipart/form-data"] = new OpenApiMediaType
            {
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.Object,
                    Properties = new Dictionary<string, IOpenApiSchema>
                    {
                        ["file"] = new OpenApiSchema
                        {
                            Type = JsonSchemaType.String,
                            Format = "binary",
                            Description = "CSV or JSON file to upload"
                        }
                    },
                    Required = new HashSet<string> { "file" }
                }
            };

            postOperation.RequestBody = mergedRequestBody;
            postOperation.Summary = "Create employees";
            postOperation.Description = "Create employees from JSON body, CSV body, or file upload (CSV/JSON).\n\n" +
                "**Supported Content Types:**\n" +
                "- `application/json`: JSON array of employee objects\n" +
                "- `text/csv`: CSV formatted data\n" +
                "- `multipart/form-data`: File upload (CSV or JSON file)";
        }
    }
}
