using System;
using ApplicationMng;
using ApplicationMng.Signal.Base;
using Scorto.Strategy;

namespace EzBob.Signals
{
    public abstract class SignalHandlerBase<T> : ISignalHandler where T : BaseMessage
    {

        protected T Message;

        public virtual object Clone()
        {
            return Activator.CreateInstance(this.GetType());
        }

        public virtual void Init(string key)
        {
        }

        public virtual bool Validate(object messageBody)
        {
            return messageBody.GetType().IsAssignableFrom(typeof(T));
        }

        public virtual void ParseMessage(Signal signal)
        {
            Signal = signal;
            Message = (T)signal.Body;
        }

        protected Signal Signal { get; set; }

        public virtual Type[] SignalTypes
        {
            get { return new Type[] { typeof(T) }; }
        }

        public abstract void Execute();
    }
}