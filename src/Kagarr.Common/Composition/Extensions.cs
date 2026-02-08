using System.Collections.Generic;
using System.Linq;
using DryIoc;
using Kagarr.Common.EnvironmentInfo;

namespace Kagarr.Common.Composition.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static Rules WithKagarrRules(this Rules rules)
        {
            return rules.WithMicrosoftDependencyInjectionRules()
                .WithAutoConcreteTypeResolution()
                .WithDefaultReuse(Reuse.Singleton);
        }

        public static IContainer AddStartupContext(this IContainer container, StartupContext context)
        {
            container.RegisterInstance<IStartupContext>(context, ifAlreadyRegistered: IfAlreadyRegistered.Replace);
            return container;
        }

        public static IContainer AutoAddServices(this IContainer container, List<string> assemblyNames)
        {
            var assemblies = assemblyNames
                .Select(name => System.Reflection.Assembly.Load(name))
                .ToList();

            container.RegisterMany(assemblies,
                serviceTypeCondition: type => type.IsInterface && !string.IsNullOrWhiteSpace(type.FullName) && !type.FullName.StartsWith("System"),
                reuse: Reuse.Singleton);

            container.RegisterMany(assemblies,
                serviceTypeCondition: type => !type.IsInterface && !string.IsNullOrWhiteSpace(type.FullName) && !type.FullName.StartsWith("System"),
                reuse: Reuse.Transient);

            return container;
        }
    }
}
