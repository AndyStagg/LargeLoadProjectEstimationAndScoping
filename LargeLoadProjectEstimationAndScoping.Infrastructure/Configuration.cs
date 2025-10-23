using LargeLoadProjectEstimationAndScoping.Core.Abstractions;
using LargeLoadProjectEstimationAndScoping.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LargeLoadProjectEstimationAndScoping.Infrastructure
{
    public static class Configuration
    {
        public static IServiceCollection AddDataCenterProjectDataService(this IServiceCollection services, bool dev = false)
        {
            if (dev)
            {
                services.AddSingleton<IDataCenterProjectDataService, DevDataCenterProjectDataService>();
            }
            else
            {
                throw new System.NotImplementedException("Production IDataCenterProjectDataService is not implemented yet.");
            }

            return services;
        }

        public static IServiceCollection AddProposalDataService(this IServiceCollection services, bool dev = false)
        {
            if (dev)
            {
                services.AddSingleton<IProposalDataService, DevProposalDataService>();
            }
            else
            {
                throw new System.NotImplementedException("Production IProposalDataService is not implemented yet.");
            }

            return services;
        }

        public static IServiceCollection AddProjectDataService(this IServiceCollection services, bool dev = false)
        {
            if (dev)
            {
                services.AddSingleton<IProjectDataService, DevProjectDataService>();
            }
            else
            {
                throw new System.NotImplementedException("Production IProjectDataService is not implemented yet.");
            }

            return services;
        }
    }
}
