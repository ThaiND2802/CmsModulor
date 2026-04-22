namespace Commerce.Host.WebApi.Configuration;

public sealed class OpenApiVisibilityOptions
{
    public const string SectionName = "OpenApi";

    public bool HideDisabledEndpoints { get; set; }

    public bool MarkDisabledEndpoints { get; set; } = true;
}
