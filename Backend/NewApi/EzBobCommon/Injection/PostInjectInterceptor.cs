using System;

namespace EzBobCommon.Injection {
    using System.Linq.Expressions;
    using System.Reflection;
    using StructureMap;
    using StructureMap.Building.Interception;

    /// <summary>
    /// Calls method marked with <see cref="PostInject"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class PostInjectInterceptor<T> : ActivatorInterceptor<T> {
        public PostInjectInterceptor(MethodInfo method)
            : base(GetPostInjectCallExpression(method), "post inject interceptor") {}

        /// <summary>
        /// Gets the post inject call expression.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <returns></returns>
        private static Expression<Action<IContext, T>> GetPostInjectCallExpression(MethodInfo info) {
            Expression<Action<IContext, T>> expr = (ctx, obj) => CallPostInject(ctx, obj, info);
            return expr;
        }

        /// <summary>
        /// Calls the method marked decorated with <see cref="PostInject"/>.
        /// </summary>
        /// <param name="ctx">The context.</param>
        /// <param name="instance">The instance.</param>
        /// <param name="methodInfo">The method information.</param>
        private static void CallPostInject(IContext ctx, T instance, MethodInfo methodInfo) {
            methodInfo.Invoke(instance, null);
        }
    }
}
