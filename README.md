# WebQuark

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Nuget](https://img.shields.io/nuget/v/WebQuark.Core?style=plastic)](https://www.nuget.org/packages/WebQuark.Core)
![NuGet Downloads](https://img.shields.io/nuget/dt/WebQuark.Core?cacheSeconds=300)
[![issues - webquark](https://img.shields.io/github/issues/engineering87/WebQuark)](https://github.com/engineering87/WebQuark/issues)
[![stars - webquark](https://img.shields.io/github/stars/engineering87/WebQuark?style=social)](https://github.com/engineering87/WebQuark)
[![Sponsor me](https://img.shields.io/badge/Sponsor-❤-pink)](https://github.com/sponsors/engineering87)

<img src="https://github.com/engineering87/WebQuark/blob/main/img/WebQuark_logo.jpg" width="300">

**WebQuark** is a lightweight, modular .NET utility library that streamlines web request, session handling, and routing across both legacy **.NET Framework** and modern **.NET / ASP.NET Core** environments.

## 📑 Table of Contents
- [Packages](#-packages)
- [Integrating WebQuark in Your Project](#integrating-webquark-in-your-project)
  - [.NET Core Integration](#net-core-integration)
  - [Legacy .NET Framework Integration](#legacy-net-framework-integration)
- [Contributing](#contributing)
- [License](#license)
- [Contact](#contact)

## 📦 Packages

WebQuark is split into small, focused projects:

| Project                    | Description                                                                 | NuGet Stable |
|----------------------------|-----------------------------------------------------------------------------|--------------|
| `WebQuark.Core`            | Core interfaces and shared abstractions used across other modules           | [![WebQuark.Core](https://img.shields.io/nuget/v/WebQuark.Core?style=plastic)](https://www.nuget.org/packages/WebQuark.Core) |
| `WebQuark.HttpRequest`     | Unified request handling and query utilities for Framework and Core         | [![WebQuark.HttpRequest](https://img.shields.io/nuget/v/WebQuark.HttpRequest?style=plastic)](https://www.nuget.org/packages/WebQuark.HttpRequest) |
| `WebQuark.HttpResponse`    | Response helpers: headers, status codes, content writing, and redirection   | [![WebQuark.HttpResponse](https://img.shields.io/nuget/v/WebQuark.HttpResponse?style=plastic)](https://www.nuget.org/packages/WebQuark.HttpResponse) |
| `WebQuark.Session`         | Cross-platform session manager with support for encrypted object storage    | [![WebQuark.Session](https://img.shields.io/nuget/v/WebQuark.Session?style=plastic)](https://www.nuget.org/packages/WebQuark.Session) |
| `WebQuark.QueryString`     | Strongly-typed query string parser and encoder utilities                    | [![WebQuark.QueryString](https://img.shields.io/nuget/v/WebQuark.QueryString?style=plastic)](https://www.nuget.org/packages/WebQuark.QueryString) |
| `WebQuark.Extensions`      | Extension methods to simplify integration and enhance core WebQuark modules | [![WebQuark.Extensions](https://img.shields.io/nuget/v/WebQuark.Extensions?style=plastic)](https://www.nuget.org/packages/WebQuark.Extensions) |

## Integrating WebQuark in Your Project
This guide shows how to integrate the WebQuark library for unified abstraction across different .NET platforms, including .NET Core and legacy .NET Framework.

### .NET Core Integration
For .NET Core applications, WebQuark provides extension methods to register all core services using the built-in dependency injection (DI) container.

#### Register WebQuark Services in `Program.cs` or `Startup.cs`

```csharp
using WebQuark.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add required services
builder.Services.AddRazorPages();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor(); // Register IHttpContextAccessor
builder.Services.AddWebQuark();             // Register all WebQuark services

var app = builder.Build();

// Configure WebQuark's QueryStringHandler with IHttpContextAccessor
var accessor = app.Services.GetRequiredService<IHttpContextAccessor>();
WebQuark.QueryString.QueryStringHandler.ConfigureHttpContextAccessor(accessor);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseSession();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();
```

You can also register services individually if needed:

```csharp
builder.Services.AddWebQuarkHttpRequestInspector();
builder.Services.AddWebQuarkHttpResponseHandler();
builder.Services.AddWebQuarkRequestQueryHandler();
builder.Services.AddWebQuarkSessionHandler();
```

### Legacy .NET Framework Integration
For legacy ASP.NET Framework projects (e.g., targeting .NET Framework 4.7.2), WebQuark services can be used directly by instantiating their classes:

```csharp
var responseHandler = new HttpResponseHandler();
var queryHandler = new QueryStringHandler();
var sessionHandler = new SessionHandler();
var requestInspector = new HttpRequestInspector();
```

## Contributing
Thank you for considering to help out with the source code!
If you'd like to contribute, please fork, fix, commit and send a pull request for the maintainers to review and merge into the main code base.

 * [Setting up Git](https://docs.github.com/en/get-started/getting-started-with-git/set-up-git)
 * [Fork the repository](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/working-with-forks/fork-a-repo)
 * [Open an issue](https://github.com/engineering87/WebQuark/issues) if you encounter a bug or have a suggestion for improvements/features

### License
WebQuark source code is available under MIT License, see license in the source.

### Contact
Please contact at francesco.delre[at]protonmail.com for any details.
