﻿using BrandUp.Pages.Builder;
using BrandUp.Pages.Content.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BrandUp.Pages
{
    public static class IPagesBuilderExtensions
    {
        public static IPagesBuilder AddContentTypesFromAssemblies(this IPagesBuilder builder, params Assembly[] assemblies)
        {
            builder.Services.AddSingleton<IContentTypeLocator>(new AssemblyContentTypeLocator(assemblies));
            return builder;
        }

        public static IPagesBuilder AddUserAccessProvider<T>(this IPagesBuilder builder, ServiceLifetime serviceLifetime)
            where T : Identity.IAccessProvider
        {
            builder.Services.Add(new ServiceDescriptor(typeof(Identity.IAccessProvider), typeof(T), serviceLifetime));

            return builder;
        }
    }
}