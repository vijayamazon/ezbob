namespace EzBobCommon {
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using StructureMap.Building.Interception;

    /// <summary>
    ///Accumulates errors
    /// </summary>
    [Serializable]
    [JsonConverter(typeof(InfoAccumulatorSerializer))]
    public class InfoAccumulator {

        private readonly IDictionary<string, string> errors = new ConcurrentDictionary<string, string>();
        private readonly IDictionary<string, string> warnings = new ConcurrentDictionary<string, string>();
        private readonly ConcurrentQueue<Exception> exceptions = new ConcurrentQueue<Exception>();
        private readonly ConcurrentQueue<String> infoQueue = new ConcurrentQueue<string>();

        /// <summary>
        /// Initializes a new instance of info accumulator
        /// </summary>
        public InfoAccumulator() {
            IsRetry = false;
        }

        /// <summary>
        /// Adds the error.
        /// </summary>
        /// <param name="error">The error.</param>
        public InfoAccumulator AddError(string error)
        {
            if (!string.IsNullOrEmpty(error)) {
                this.errors.Add(error, null);
            }
            return this;
        }

        /// <summary>
        /// Adds the warning.
        /// </summary>
        /// <param name="warning">The warning.</param>
        public InfoAccumulator AddWarning(string warning)
        {
            if (!string.IsNullOrEmpty(warning)) {
                this.warnings.Add(warning, null);
            }

            return this;
        }

        /// <summary>
        /// Adds the information.
        /// </summary>
        /// <param name="info">The information.</param>
        public InfoAccumulator AddInfo(String info)
        {
            if (!string.IsNullOrEmpty(info)) {
                this.infoQueue.Enqueue(info);
            }

            return this;
        }

        /// <summary>
        /// Adds the exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public InfoAccumulator AddException(Exception exception)
        {
            this.exceptions.Enqueue(exception);
            return this;
        }

        /// <summary>
        /// Gets the information.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetInfo() {
            return this.infoQueue;
        }

        /// <summary>
        /// Gets the warning.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetWarning() {
            return this.warnings.Keys;
        }

        /// <summary>
        /// Gets the errors.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetErrors() {
            return this.errors.Keys;
        }

        /// <summary>
        /// Gets the exceptions.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Exception> GetExceptions() {
            return this.exceptions;
        }

        /// <summary>
        /// Gets a value indicating whether this instance has errors.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has errors; otherwise, <c>false</c>.
        /// </value>
        public bool HasErrors
        {
            get { return this.errors.Any() || this.exceptions.Any(); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to send command to retry.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is retry; otherwise, <c>false</c>.
        /// </value>
        public bool IsRetry { get; set; }
    }
}
