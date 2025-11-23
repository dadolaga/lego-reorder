using System.Reflection;

namespace WebApplication.Controllers {
    public static class ControllerUtilis {
        public static IOrderedEnumerable<TSource> OrderBy<TSource>(this IEnumerable<TSource> source, string filter) {
            IOrderedEnumerable<TSource>? newSource = null;

            if(filter.Length == 0) {
                throw new ArgumentException("Filter must be filled");
            }

            foreach (var item in filter.Split(", ")) {
                bool isDescent = item.StartsWith("-");
                string proprietyName = (item.StartsWith("+") || item.StartsWith("-")) ? item.Substring(1) : item;

                if (newSource == null) {
                    if (!isDescent) {
                        newSource = source.OrderBy(v => GetProprieties(v, proprietyName));
                    } else {
                        newSource = source.OrderByDescending(v => GetProprieties(v, proprietyName));
                    }
                } else {
                    if (!isDescent) {
                        newSource = newSource.ThenBy(v => GetProprieties(v, proprietyName));
                    } else {
                        newSource = newSource.ThenByDescending(v => GetProprieties(v, proprietyName));
                    }
                }
            }

            return newSource!;

            static object? GetProprieties(TSource v, string proprietyName) {
                Type type = v.GetType();
                PropertyInfo? propertyInfo = type.GetProperty(proprietyName);

                if (propertyInfo == null)
                    throw new ArgumentException($"Property '{proprietyName}' not found on type {type.Name}.");

                return propertyInfo.GetValue(v);
            }
        }
    }
}
