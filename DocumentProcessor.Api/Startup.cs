using System;
using DocumentProcessor.Api;
using DocumentProcessor.Api.Configs;
using DocumentProcessor.Api.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly:FunctionsStartup(typeof(Startup))]
namespace DocumentProcessor.Api
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var services = builder.Services;
            services.AddSingleton(new SecureStorageConfiguration
            {
                Account = Environment.GetEnvironmentVariable("SecureStorageConfiguration.Account"),
                Container = Environment.GetEnvironmentVariable("SecureStorageConfiguration.Container")
            });

            services.AddSingleton<IBlobService, BlobService>();
        }
    }
}