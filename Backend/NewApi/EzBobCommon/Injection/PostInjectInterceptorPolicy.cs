using System;
using System.Collections.Generic;
using System.Linq;

namespace EzBobCommon.Injection {
    using System.Reflection;
    using StructureMap.Building.Interception;
    using StructureMap.Pipeline;

    /// <summary>
    /// Determines whether the provided pluginType contains method decorated with <see cref="PostInject"/>
    /// </summary>
    internal class PostInjectInterceptorPolicy : IInterceptorPolicy {
        public string Description
        {
            get { return "post inject policy"; }
        }

        /// <summary>
        /// Determines the interceptors.
        /// </summary>
        /// <param name="pluginType">Type of the plugin.</param>
        /// <param name="instance">The instance.</param>
        /// <returns></returns>
        public IEnumerable<IInterceptor> DetermineInterceptors(Type pluginType, Instance instance) {

            var methodInfo = instance.ReturnedType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(m => m.GetCustomAttributes(typeof(PostInject), false)
                    .Length > 0);

            if (methodInfo != null) {
                var genericType = typeof(PostInjectInterceptor<>).MakeGenericType(pluginType);
                var interceptor = Activator.CreateInstance(genericType, methodInfo) as IInterceptor;

                yield return interceptor;
            }
        }
    }
}
