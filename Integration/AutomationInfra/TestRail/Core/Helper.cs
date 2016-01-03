namespace TestRailCore {
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public static class Helper {
        public static Type GetTypeWithAttributeValue<TAttribute>(Assembly aAssembly, Predicate<TAttribute> pred) {
            foreach (Type type in aAssembly.GetTypes()) {
                foreach (TAttribute oTemp in type.GetCustomAttributes(typeof(TAttribute), true)) {
                    if (pred(oTemp)) {
                        return type;
                    }
                }
            }
            return typeof(string); //otherwise return a string type
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source) {
            return new HashSet<T>(source);
        }
    }

}
