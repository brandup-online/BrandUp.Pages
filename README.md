# BrandUp.Pages

[![Build status](https://dev.azure.com/brandup/BrandUp%20Core/_apis/build/status/BrandUp.Pages)](https://dev.azure.com/brandup/BrandUp%20Core/_build/latest?definitionId=8)

## Setup

```

services.AddPages()
    .AddRazorContentPage()
    .AddContentTypesFromAssemblies(typeof(Startup).Assembly)
    .AddMongoDb<Models.AppDbContext>()
    .AddImageResizer<Infrastructure.ImageResizer>()
    .AddUserProvider<Identity.PageEditorProvider>(ServiceLifetime.Scoped)
    .AddUserAccessProvider<Identity.RoleBasedAccessProvider>(ServiceLifetime.Scoped);

```