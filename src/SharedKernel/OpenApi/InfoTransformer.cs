using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace SharedKernel.OpenApi;

public class InfoTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var openApiInfo = context.ApplicationServices.GetRequiredService<IOptions<OpenApiInfo>>();

        document.Info = openApiInfo.Value;

        return Task.CompletedTask;
    }
}