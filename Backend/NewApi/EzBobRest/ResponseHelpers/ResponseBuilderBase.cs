using System;

namespace EzBobRest.ResponseHelpers {
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using EzBobCommon;
    using EzBobCommon.Utils;
    using Newtonsoft.Json.Linq;

    public abstract class ResponseBuilderBase<T>
        where T : ResponseBuilderBase<T> {

        private readonly JObject response = new JObject();

        private static readonly string CustomerId = "CustomerId";
        private static readonly string CompanyId = "CompanyId";
        private static readonly string AuthorityId = "AuthorityId";
        private static readonly string RedirectUrl = "RedirectUrl";
        private static readonly string SessionId = "SessionId";

        /// <summary>
        /// Sets the customer identifier.
        /// <remarks>Nancy default values are ignored.</remarks>
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        public T WithCustomerId(string customerId) {
            return WithKeyValue(CustomerId, customerId);
        }

        /// <summary>
        /// Sets the company identifier.
        /// <remarks>Nancy default values are ignored.</remarks>
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public T WithCompanyId(string companyId) {
            return WithKeyValue(CompanyId, companyId);
        }

        /// <summary>
        /// Sets the authority identifier.
        /// <remarks>Nancy default values are ignored.</remarks>
        /// </summary>
        /// <param name="authorityId">The authority identifier.</param>
        /// <returns></returns>
        public T WithAuthorityId(string authorityId) {
            return WithKeyValue(AuthorityId, authorityId);
        }

        /// <summary>
        /// Sets the redirect URL.
        /// <remarks>Nancy default values are ignored.</remarks>
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public T WithRedirectUrl(string url) {
            return WithKeyValue(RedirectUrl, url);
        }

        /// <summary>
        /// Sets the session identifier.
        /// <remarks>Nancy default values are ignored.</remarks>
        /// </summary>
        /// <param name="sessionId">The session identifier.</param>
        /// <returns></returns>
        public T WithSessionId(string sessionId) {
            return WithKeyValue(SessionId, sessionId);
        }

        /// <summary>
        /// Adds the key value, deduced from provided property access expression.
        /// </summary>
        /// <remarks>Nancy default values are ignored<br/></remarks>
        /// <remarks>
        /// For example: () => o.Age will deduce key to be 'Age' and value will be result of expression's invocation converted to <see cref="JToken"/>.
        /// </remarks>
        /// <param name="propertyAccessExpression">The property access expression like: '() => o.Age'.</param>
        /// <returns></returns>
        public T WithKeyValue(Expression<Func<object>> propertyAccessExpression) {

            var optionalkeyValue = this.ExtractMemberNameAndValue(propertyAccessExpression);
            if (optionalkeyValue.HasValue) {
                KeyValuePair<string, JToken> keyValue = optionalkeyValue.Value;
                this.response[keyValue.Key] = keyValue.Value;
            }

            return (T)this;
        }

        /// <summary>
        /// Adds the key value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="val">The value.</param>
        /// <remarks>Nancy default values are ignored.</remarks>
        /// <returns></returns>
        public T WithKeyValue(string key, string val) {
            if (key.IsNotEmpty() && val.IsNotEmpty() && !IsNancyDefaultValue(key, val)) {
                this.response[key] = val;
            }

            return (T)this;
        }

        /// <summary>
        /// Adds the key with array value.
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        public T WithKeyArray<V>(string key, IEnumerable<V> values) where V : class {
            if (!string.IsNullOrEmpty(key) && CollectionUtils.IsEmpty(values)) {
                this.response[key] = new JArray(values);
            }

            return (T)this;
        }

        /// <summary>
        /// Sets the JObject.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public T WithJObject(string key, JObject obj)
        {
            if (obj != null) {
                this.response[key] = obj;
            }

            return (T)this;
        }

        /// <summary>
        /// Builds the response.
        /// </summary>
        /// <returns></returns>
        public virtual JObject BuildResponse() {
            return this.response;
        }

        /// <summary>
        /// Sets the j-token.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        protected void SetJToken(string key, JToken value) {
            this.response[key] = value;
        }

        /// <summary>
        /// Extracts the name of the member.
        /// </summary>
        /// <param name="propertyAccessExpression">The property access expression.</param>
        /// <returns></returns>
        protected Optional<KeyValuePair<string, JToken>> ExtractMemberNameAndValue(Expression<Func<object>> propertyAccessExpression) {
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
                return Optional<KeyValuePair<string, JToken>>.Empty();
            }

            if (IsSimpleType(val.GetType())) {
                return new KeyValuePair<string, JToken>(key, val.ToString());
            }

            JObject json = JObject.FromObject(val);
            return new KeyValuePair<string, JToken>(key, json);
        }

        /// <summary>
        /// Determines whether we got default Nancy value.
        /// <remarks>
        /// (for example: if REST parameter 'customerId' was not specified in request, nancy will map its value to '{customerId}'.
        /// </remarks> 
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        protected bool IsNancyDefaultValue(string key, string value) {
            bool res = value.ToLowerInvariant()
                .Equals(string.Format("{{{0}}}", key.ToLowerInvariant()));

            return res;
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
