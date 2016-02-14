namespace EzBobRest {
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using FluentValidation;

    /// <summary>
    /// Provides common functionality
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ValidatorBase<T> : AbstractValidator<T> {

        public ValidatorBase() {
            string id = string.Empty;

            //general rule for customerId
            RuleFor(o => GetCustomerId(o, out id))
                .NotEmpty()
                .WithMessage("Empty customer id.")
                .DependentRules(d => d.RuleFor(c => id.ToLowerInvariant())
                    .NotEqual("{customerid}") //when url parameter is not provided Nancy puts default string
                    .WithMessage("Customer id is mandatory."));
        }

        /// <summary>
        /// Automatically defines rule for rest parameters
        /// <remarks>
        /// RuleForRestParameter(o => o.SessionId) assumes that SessionId is a property corresponding to REST parameter /api/v1/something/{sessionId}
        /// </remarks>
        /// </summary>
        /// <param name="propertyExpression">The property expression.</param>
        protected void RuleForRestParameter(Expression<Func<T, string>> propertyExpression) {
            PropertyInfo prop = GetPropertyInfo(propertyExpression);//in example above corresponds to SessionId
            if (prop != null) {
                string propValue = string.Empty;
                RuleFor(o => GetPropertyValue(o, prop, out propValue))
                    .NotEmpty()
                    .WithMessage("empty " + prop.Name)
                    .DependentRules(d => d.RuleFor(c => propValue.ToLowerInvariant())
                        .NotEqual(string.Format("{{{0}}}", prop.Name.ToLowerInvariant())) //when url parameter is not provided Nancy puts default string: {parametername}
                        .WithMessage(prop.Name + " is mandatory"));
            }
        }

        /// <summary>
        /// Gets the property value.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <param name="prop">The property.</param>
        /// <param name="propValue">The property value.</param>
        /// <returns></returns>
        private string GetPropertyValue(object o, PropertyInfo prop, out string propValue) {
            propValue = prop.GetValue(o) as string;
            return propValue;
        }

        /// <summary>
        /// Gets the customer identifier.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <returns></returns>
        private string GetCustomerId(object o, out string id) {
            var property = typeof(T).GetProperty("CustomerId");
            if (property == null) {
                property = typeof(T).GetProperty("customerId");
            }

            if (property == null) {
                id = "string_to_pass_validation";
                return id;
            }

            id = property.GetValue(o) as string;
            return id;
        }

        /// <summary>
        /// Gets the property information.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="propertyLambda">The property lambda.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">
        /// </exception>
        private PropertyInfo GetPropertyInfo<TProperty>(Expression<Func<T, TProperty>> propertyLambda) {
            Type type = typeof(T);

            MemberExpression member = propertyLambda.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    propertyLambda.ToString()));

            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.",
                    propertyLambda.ToString()));

            if (type != propInfo.ReflectedType && !type.IsSubclassOf(propInfo.ReflectedType)) {
                //refers to a property that is not from type"
                return null;
            }

            return propInfo;
        }
    }
}
