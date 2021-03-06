using Jal.Router.AzureStorage.Impl;
using Jal.Router.Interface.Management;

namespace Jal.Router.AzureStorage.Extensions
{
    public static class ConfigurationExtensions
    {
        public static void UsingAzureSagaStorage(this IConfiguration configuration)
        {
            configuration.UsingSagaStorage<AzureSagaStorage>();

            configuration.AddStartupTask<AzureSagaStorageStartupTask>();
        }

        public static void UsingAzureMessageStorage(this IConfiguration configuration)
        {
            configuration.UsingMessageStorage<AzureMessageStorage>();

            configuration.AddStartupTask<AzureMessageStorageStartupTask>();
        }
    }
}