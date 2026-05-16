using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace ServiceDefaults.OpenApi;

public static class ApiVersioningExtensions
{
    public static OpenApiOptions AddApiVersionTransformer(this OpenApiOptions options, OpenApiInfo openApiInfo)
    {
        options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                var versionedDescriptionProvider = context.ApplicationServices.GetService<IApiVersionDescriptionProvider>();
                var apiDescription = versionedDescriptionProvider?.ApiVersionDescriptions
                    .SingleOrDefault(description => description.GroupName == context.DocumentName);

                if (apiDescription is null)
                {
                    return Task.CompletedTask;
                }

                document.Info = new()
                {
                    Title = openApiInfo.Title,
                    Version = openApiInfo.Version,
                    Description = openApiInfo.Description,
                    Contact = new()
                    {
                        Name = openApiInfo.Contact.Name,
                        Email = openApiInfo.Contact.Email,
                        Url = openApiInfo.Contact.Url,
                    },
                    License = new()
                    {
                        Name = openApiInfo.License.Name,
                        Url = openApiInfo.License.Url,
                    },
                };

                return Task.CompletedTask;
            });

        return options;
    }
}