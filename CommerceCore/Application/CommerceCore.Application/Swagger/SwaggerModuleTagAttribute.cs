using System;

namespace CommerceCore.Application.Swagger;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class SwaggerModuleTagAttribute : Attribute
{
    public SwaggerModuleTagAttribute(string moduleName)
    {
        ModuleName = moduleName;
    }

    public string ModuleName { get; }
}
