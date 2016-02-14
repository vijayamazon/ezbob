using System.Linq;

namespace EzBobRest.Modules {
    using System;
    using System.Net;
    using Common.Logging;
    using EzBobCommon;
    using EzBobRest.Init;
    using EzBobRest.ResponseHelpers;
    using FluentValidation;
    using FluentValidation.Results;
    using Nancy;
    using Nancy.Security;

    /// <summary>
    /// Provides basic functionality:
    /// Automatic module security (not functional in debug mode).
    /// Methods to create responses, Log, RestServerConfig.
    /// Automatically treats the 'CommandOriginator' and 'CommandOriginatorIp'
    /// </summary>
    public abstract class NancyModuleBase : NancyModule {

        [Injected]
        public ILog Log { get; set; }

        [Injected]
        public RestServerConfig Config { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Nancy.NancyModule"/> class.
        /// </summary>
        protected NancyModuleBase() {
            if (!System.Diagnostics.Debugger.IsAttached) 
            {
//                this.RequiresHttps();
                this.RequiresMSOwinAuthentication();
            }
            Before += ctx => {
                //adds 'CommandOriginator' to context parameters in order to get automatically correct value with Binding. 
                ctx.Parameters.CommandOriginator = GetOrigin();
                ctx.Parameters.CommandOriginatorIP = this.Request.UserHostAddress;
                ctx.Parameters.CommandOriginatorUrl = Dns.GetHostEntry(this.Request.UserHostAddress).HostName;
                return null;//continue to route
            };
        }

        /// <summary>
        /// Gets the origin.<see cref="EzBobOAuth2AuthorizationProvider"/>.
        /// </summary>
        /// <returns></returns>
        private string GetOrigin() {
            string origin;

            if (!System.Diagnostics.Debugger.IsAttached) {
                var user = this.Context.GetMSOwinUser();
                var claim = user.Claims.First(c => "Origin".Equals(c.Type, StringComparison.InvariantCultureIgnoreCase));
                origin = claim.Value;
            } else {
                origin = "testOrigin";
            }

            return origin;
        }

        /// <summary>
        /// Validates the specified command.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command">The command.</param>
        /// <param name="validator">The validator.</param>
        /// <returns></returns>
        protected InfoAccumulator Validate<T>(T command, AbstractValidator<T> validator) where T : class {
            var validationResult = validator.Validate(command);
            if (validationResult.IsValid) {
                return new InfoAccumulator();
            }

            return CreateInfoAccumulator(validationResult);
        }

        /// <summary>
        /// Creates the information accumulator.
        /// </summary>
        /// <param name="validationResult">The validation result.</param>
        /// <returns></returns>
        private static InfoAccumulator CreateInfoAccumulator(ValidationResult validationResult) {
            var res = validationResult.Errors.Aggregate(new InfoAccumulator(), (info, f) => info.AddError(f.ErrorMessage));
            return res;
        }

        /// <summary>
        /// Creates the error response.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        /// <returns></returns>
        protected Response CreateErrorResponse(Nancy.HttpStatusCode statusCode) {
            return Response.AsJson("{}")
                .WithStatusCode(statusCode);
        }

        /// <summary>
        /// Creates the error response.
        /// </summary>
        /// <param name="setParameters">The set parameters action.</param>
        /// <param name="statusCode">The status code.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">setParameters could not be null</exception>
        protected Response CreateErrorResponse(Action<ErrorResponseBuilder> setParameters, Nancy.HttpStatusCode statusCode = Nancy.HttpStatusCode.BadRequest) {
            if (setParameters == null) {
                throw new ArgumentException("setParameters could not be null");
            }

            var builder = new ErrorResponseBuilder();
            setParameters(builder);

            return Response.AsJson(builder.BuildResponse())
                .WithStatusCode(statusCode);
        }

        /// <summary>
        /// Creates the ok response.
        /// </summary>
        /// <param name="setParameters">The set parameters.</param>
        /// <returns></returns>
        protected Response CreateOkResponse(Action<OkResponseBuilder> setParameters) {
            OkResponseBuilder builder = new OkResponseBuilder();
            if (setParameters != null) {
                setParameters(builder);
            }
            return Response.AsJson(builder.BuildResponse())
                .WithStatusCode(Nancy.HttpStatusCode.OK);
        }
    }
}
