namespace EzBobCommon.NSB {
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Caching;
    using System.Text;
    using Common.Logging;
    using EzBobCommon.Utils.Encryption;
    using Newtonsoft.Json;
    using NServiceBus;
    using NServiceBus.Faults;

    /// <summary>
    /// Implements internal mechanism of error notifications
    /// <para>Provides automatic models validation (by reflection)</para>
    /// <para>Provides method to Send Reply</para>
    /// <para>Provides method to Register Error</para> 
    /// <para>Provides Log</para> 
    /// <para>Provides Bus</para>
    /// (When there is an error handler should register error and should NOT REPLY anything)
    /// </summary>
    public abstract class HandlerBase<T> : IHandleErrors
        where T : CommandResponseBase, new() {

        [Injected]
        public ILog Log { get; set; }

        [Injected]
        public IBus Bus { get; set; }

        [Injected]
        public ErrorCache ErrorCache { get; set; }

        /// <summary>
        /// USED BY ERROR NOTIFICATION MECHANISM, DON'T CALL THIS METHOD BY YOURSELF.
        /// Sends the error response.
        /// </summary>
        /// <param name="failedMessage">The failed message.</param>
        public void SendErrorResponse(FailedMessage failedMessage) {
            CommandBase originalCommand = this.ExtractOriginalMessage(failedMessage);

            string replyTo = failedMessage.Headers[Headers.ReplyToAddress];
            string correlationId = failedMessage.Headers[Headers.CorrelationId];

            string busMessageId = failedMessage.Headers[Headers.MessageId];

            T response = this.PrepareErrorResponse(originalCommand, busMessageId);

            Bus.Send(replyTo, correlationId, response);
        }

        /// <summary>
        /// Extracts the original message.
        /// </summary>
        /// <param name="failedMessage">The failed message.</param>
        /// <returns></returns>
        private CommandBase ExtractOriginalMessage(FailedMessage failedMessage) {
            string jsonStr = Encoding.UTF8.GetString(failedMessage.Body);
            CommandBase originalCommand = JsonConvert.DeserializeObject(jsonStr) as CommandBase;
            if (originalCommand == null) {
                Log.Error("all commands should inherit from CommandBase");
            }

            return originalCommand;
        }

        /// <summary>
        /// Prepares the error response.
        /// </summary>
        /// <param name="originalCommand">The original command.</param>
        /// <param name="busMessageId">The bus message identifier.</param>
        /// <returns></returns>
        private T PrepareErrorResponse(CommandBase originalCommand, string busMessageId) {
            T response;

            var cacheItem = ErrorCache.Get(busMessageId) as Tuple<Guid, InfoAccumulator>;
            if (cacheItem == null) {
                Log.Error("Could not find an error description for failed message");
                response = CreateResponse<T>(info: new InfoAccumulator(), messageId: originalCommand.MessageId);
            } else {
                response = CreateResponse<T>(info: cacheItem.Item2, messageId: cacheItem.Item1);
            }

            response.EzBobHeaders = originalCommand.EzBobHeaders;
            response.IsFailed = true;

            return response;
        }

        /// <summary>
        /// Validates the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="errors">The errors.</param>
        /// <param name="isToValidateProperty">The is to validate property.</param>
        /// <returns></returns>
        protected bool ValidateModel(object model, InfoAccumulator errors, Func<string, bool> isToValidateProperty = null) {
            bool res = true;

            if (model == null) {
                errors.AddError("got empty model");
                return false;
            }

            if (isToValidateProperty == null) {
                isToValidateProperty = s => true;
            }

            PropertyInfo[] propertyInfos = model.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var propertyInfo in propertyInfos.Where(prop => prop.CanWrite && prop.CanRead)) {

                if (!isToValidateProperty(propertyInfo.Name)) {
                    continue;
                }

                object propertyValue = propertyInfo.GetValue(model);

                if (propertyValue == null) {
                    string error = "could not obtain " + model.GetType()
                        .Name + "'s " + propertyInfo.Name;
                    errors.AddError(error);
                    Log.Error(error);

                    res = false;
                } else if (propertyInfo.PropertyType == typeof(string) && string.IsNullOrEmpty((string)propertyValue)) {
                    string error = "could not obtain " + model.GetType()
                        .Name + "'s " + propertyInfo.Name;
                    errors.AddError(error);
                    Log.Error(error);

                    res = false;
                }
            }

            return res;
        }

        /// <summary>
        /// Registers the error.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="command">The command.</param>
        protected void RegisterError(InfoAccumulator info, CommandBase command) {
            Tuple<Guid, InfoAccumulator> cacheItem = new Tuple<Guid, InfoAccumulator>(command.MessageId, info);
            ErrorCache.Set(Bus.CurrentMessageContext.Id, cacheItem, new CacheItemPolicy {
                AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(60)
            });
        }

        /// <summary>
        /// Sends the response.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="cmd">The command.</param>
        /// <param name="beforeReply">The before reply action.</param>
        protected void SendReply(InfoAccumulator info, CommandBase cmd, Action<T> beforeReply = null) {
            var response = CreateResponse<T>(info, cmd.MessageId);
            if (beforeReply != null) {
                beforeReply(response);
            }

            foreach (var header in cmd.EzBobHeaders) {
                response.EzBobHeaders.Add(header);
            }
//            ************** The line below does not work when using async handler ********** 
//            Bus.Send(Bus.CurrentMessageContext.ReplyToAddress, response);
            //look at 'AsyncHandlerSupport'
            Bus.Send(cmd.ReplyToAddress, response);
//            Bus.Reply(response);
        }

        /// <summary>
        /// Sends the command.
        /// </summary>
        /// <param name="destinationAddress">The destination address.</param>
        /// <param name="commandToSend">The command.</param>
        /// <param name="handledCommand">The handled command.</param>
        protected void SendCommand(string destinationAddress, CommandBase commandToSend, CommandBase handledCommand) {

            commandToSend.MessageId = handledCommand.MessageId;
            commandToSend.EzBobHeaders.Add(Headers.ReplyToAddress, Bus.CurrentMessageContext.Headers[Headers.ReplyToAddress]);
            commandToSend.EzBobHeaders.Add(Headers.CorrelationId, Bus.CurrentMessageContext.Headers[Headers.CorrelationId]);

            Bus.Send(destinationAddress, commandToSend);
        }


        /// <summary>
        /// Copies the headers. USE WHEN YOU NOW WHAT YOU ARE DOING
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        protected void CopyHeaders(CommandBase from, CommandBase to) {
            to.EzBobHeaders = from.EzBobHeaders;
            to.MessageId = from.MessageId;
        }

        /// <summary>
        /// Replies to origin.
        /// </summary>
        /// <param name="handledResponse">The handled response.</param>
        /// <param name="beforeReply">The before reply.</param>
        protected void ReplyToOrigin(CommandResponseBase handledResponse, Action<T> beforeReply = null) {
            T responseToSend = new T();

            string replyTo = (string)handledResponse.EzBobHeaders[Headers.ReplyToAddress];
            string correlatonId = (string)handledResponse.EzBobHeaders[Headers.CorrelationId];
            responseToSend.MessageId = handledResponse.MessageId;

            responseToSend.Errors = handledResponse.Errors;
            responseToSend.Infos = handledResponse.Infos;
            responseToSend.Warnings = handledResponse.Warnings;

            if (beforeReply != null) {
                beforeReply(responseToSend);
            }

            Bus.Send(replyTo, correlatonId, responseToSend);
        }

        /// <summary>
        /// Encrypts the specified value.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        protected byte[] Encrypt(string val) {
            var res = EncryptionUtils.SafeEncrypt(val);
            return Encoding.UTF8.GetBytes(res);
        }

        /// <summary>
        /// Creates the response.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="info">The information.</param>
        /// <param name="messageId">The message identifier.</param>
        /// <returns></returns>
        private T CreateResponse<T>(InfoAccumulator info, Guid messageId) where T : CommandResponseBase, new() {
            var response = new T {
                MessageId = messageId,
                Errors = info.GetErrors()
                    .Concat(info.GetExceptions()
                        .Select(e => e.ToString()))
                    .ToArray(),
                Warnings = info.GetWarning()
                    .ToArray(),
                Infos = info.GetInfo()
                    .ToArray()
            };

            return response;
        }
    }
}
