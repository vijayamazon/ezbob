namespace EzBobRest.ResponseHelpers {
    using System;
    using System.Collections.Generic;
    using EzBobCommon;
    using Nancy.ModelBinding;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Helps to create error response
    /// </summary>
    /// <remarks>
    /// There is no need to check anything for being null or empty<br/>
    /// Builder ignores empty and null parameters
    /// </remarks>
    public class ErrorResponseBuilder : ResponseBuilderBase<ErrorResponseBuilder>
    {
        private readonly JArray errors = new JArray();

        private ModelBindingException bindingException;


        /// <summary>
        /// Adds the error message.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public ErrorResponseBuilder WithErrorMessage(string errorMessage) {
            AddErrorMessageInternal(errorMessage);

            return this;
        }

        /// <summary>
        /// Adds the error messages.
        /// </summary>
        /// <param name="messages">The messages.</param>
        /// <returns></returns>
        public ErrorResponseBuilder WithErrorMessages(params string[] messages) {
            if (messages != null) {
                foreach (var message in messages) {
                    AddErrorMessageInternal(message);
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the error messages.
        /// </summary>
        /// <param name="messages">The messages.</param>
        /// <returns></returns>
        public ErrorResponseBuilder WithErrorMessages(IEnumerable<string> messages) {
            if (messages != null) {
                foreach (var message in messages) {
                    AddErrorMessageInternal(message);
                }
            }

            return this;
        }

        /// <summary>
        /// Adds the model binding exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public ErrorResponseBuilder WithModelBindingException(ModelBindingException exception) {
            this.bindingException = exception;
            return this;
        }

        /// <summary>
        /// Builds the response.
        /// </summary>
        /// <returns></returns>
        public override JObject BuildResponse() {
            if (this.bindingException != null) {
                string propertyName = ExtractInvalidPropertyName(this.bindingException);
                if (propertyName.IsEmpty()) {
                    string errorMsg = "Invalid " + ExtractInvalidPropertyName(this.bindingException);
                    AddErrorMessageInternal(errorMsg);
                } else if (!this.errors.HasValues) {
                    AddErrorMessageInternal("Invalid request"); //actually should not get here
                }
            }

            base.SetJToken("Errors", this.errors);
            
            return base.BuildResponse();
        }

        /// <summary>
        /// Adds the error message.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        private void AddErrorMessageInternal(string errorMessage) {
            if (!string.IsNullOrEmpty(errorMessage)) {
                this.errors.Add(errorMessage);
            }
        }

        /// <summary>
        /// Extracts the name of the invalid property.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        private string ExtractInvalidPropertyName(ModelBindingException exception) {
            if (exception.InnerException != null) {
                var items = exception.InnerException.Message.Split(',')[0].Split('.');
                return items[items.Length - 1];
            }

            return String.Empty;
        }
    }
}
