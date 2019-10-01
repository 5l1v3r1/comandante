using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Comandante.Services
{
    public class OptionsService
    {
        public List<OptionInfo> GetOptions()
        {
            var serviceCollection = ComandanteServiceCollectionExtensions.Services;
            return serviceCollection
                .Where(x => x.ServiceType.IsGenericType && x.ServiceType.GetGenericTypeDefinition() == typeof(IConfigureOptions<>))
                .Select(x =>
                    new OptionInfo
                    {
                        OptionType = x.ServiceType.GetGenericArguments()[0],
                        OptionFullName = x.ServiceType.GetGenericArguments()[0].GetFullFriendlyName(),
                        OptionFriendlyName = x.ServiceType.GetGenericArguments()[0].GetFriendlyName(),
                    })
                .Distinct(new OptionInfoComparer()).OrderBy(x => x.OptionType.Namespace != null && (x.OptionType.Namespace.StartsWith("Microsoft.") || x.OptionType.Namespace.StartsWith("System.")) ? 1 : 0).ToList(); ;
        }

        public OptionInfo GetOption(string fullName)
        {
            return GetOptions().FirstOrDefault(x => x.OptionFullName == fullName);
        }

        public object GetOptionInstance(OptionInfo option)
        {
            if (option == null)
                return null;
            var optionsType = typeof(IOptions<>).MakeGenericType(option.OptionType);
            return new HttpContextHelper().HttpContext.RequestServices.GetService(optionsType);
        }

    }

    public class OptionInfo
    {
        public Type OptionType;
        public string OptionFullName;
        public string OptionFriendlyName;
    }

    public class OptionInfoComparer : IEqualityComparer<OptionInfo>
    {
        public bool Equals(OptionInfo x, OptionInfo y)
        {
            return x.OptionFullName == y.OptionFullName;
        }

        public int GetHashCode(OptionInfo obj)
        {
            return obj.OptionFullName.GetHashCode();
        }
    }
}
