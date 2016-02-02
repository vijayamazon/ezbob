using System.Collections.Generic;

namespace EzBobRest.ResponseHelpers {
    using System;
    using System.Linq.Expressions;
    using EzBobCommon.Utils;
    using Newtonsoft.Json.Linq;

    public class OkResponseBuilder {
        private readonly JObject response = new JObject();
        private static readonly string CustomerId = "CustomerId";
        private static readonly string CompanyId = "CompanyId";
        private static readonly string RedirectUrl = "RedirectUrl";
        private static readonly string SessionId = "SessionId";

        /// <summary>
        /// Adds the customer identifier.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        public OkResponseBuilder WithCustomerId(string customerId) {
            if (!string.IsNullOrEmpty(customerId)) {
                this.response[CustomerId] = customerId;
            }

            return this;
        }

        /// <summary>
        /// Adds the company identifier.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public OkResponseBuilder WithCompanyId(string companyId) {
            if (!string.IsNullOrEmpty(companyId)) {
                this.response[CompanyId] = companyId;
            }

            return this;
        }

        /// <summary>
        /// Adds the redirect URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public OkResponseBuilder WithRedirectUrl(string url) {
            if (!string.IsNullOrEmpty(url)) {
                this.response[RedirectUrl] = url;
            }

            return this;
        }

        /// <summary>
        /// Adds the session identifier.
        /// </summary>
        /// <param name="sessionId">The session identifier.</param>
        /// <returns></returns>
        public OkResponseBuilder WithSessionId(string sessionId) {
            if (!string.IsNullOrEmpty(sessionId)) {
                this.response[SessionId] = sessionId;
            }

            return this;
        }

        /// <summary>
        /// Adds the key with array value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        public OkResponseBuilder WithKeyArray<T>(string key, IEnumerable<T> values) where T : class {
            if (!string.IsNullOrEmpty(key) && CollectionUtils.IsEmpty(values)) {
                this.response[key] = new JArray(values);
            }

            return this;
        }

        /// <summary>
        /// Adds the key value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        public OkResponseBuilder WithKeyValue(string key, string val) {
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(val)) {
                this.response[key] = val;
            }

            return this;
        }

        /// <summary>
        /// Adds the key value, deduced from provided property access expression.
        /// </summary>
        /// <remarks>
        /// For example: () => o.Age will deduce key to be 'Age' and value will be result of expression's invocation converted to <see cref="JToken"/>.
        /// </remarks>
        /// <param name="propertyAccessExpression">The property access expression like: '() => o.Age'.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">
        /// expected lambda like o => o.Age
        /// or
        /// got null value when invoked expression
        /// </exception>
        public OkResponseBuilder WithKeyValue(Expression<Func<object>> propertyAccessExpression) {

            var keyValue = this.ExtractMemberNameAndValue(propertyAccessExpression);
            this.response[keyValue.Key] = keyValue.Value;

            return this;
        }

        public JObject BuildResponse() {
            return this.response;
        }

        /// <summary>
        /// Extracts the name of the member.
        /// </summary>
        /// <param name="propertyAccessExpression">The property access expression.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">
        /// expected lambda like o => o.Age
        /// or
        /// got null value when invoked expression
        /// </exception>
        private KeyValuePair<string, JToken> ExtractMemberNameAndValue(Expression<Func<object>> propertyAccessExpression) {
            MemberExpression memberExpression = propertyAccessExpression.Body as MemberExpression;

            if (memberExpression == null) {
                //it could be a Convert expression, for example, if property type is struct
                UnaryExpression u = propertyAccessExpression.Body as UnaryExpression;
                if (u != null) {
                    memberExpression = u.Operand as MemberExpression;
                }
            }

            if (memberExpression == null) {
                throw new ArgumentException("expected lambda like o => o.Age");
            }

            string key = memberExpression.Member.Name;
            object val = propertyAccessExpression
                .Compile()
                .Invoke();

            if (val == null) {
                throw new ArgumentException(string.Format("invocation of {0} got null", key));
            }

            if (IsSimpleType(val.GetType())) {
                return new KeyValuePair<string, JToken>(key, val.ToString());
            }

            JObject json = JObject.FromObject(val);
            return new KeyValuePair<string, JToken>(key, json);
        }

        /// <summary>
        /// Determines whether specified type is simple type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private bool IsSimpleType(Type type) {
            return Type.GetTypeCode(type) != TypeCode.Object;
        }
    }
}
