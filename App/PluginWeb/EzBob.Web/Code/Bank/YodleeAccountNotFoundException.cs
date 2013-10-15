namespace EzBob.Web.Code.Bank
{
	using System;
	using System.Runtime.Serialization;

	[Serializable]
    public class YodleeAccountNotFoundException : Exception
    {
        public YodleeAccountNotFoundException()
        {
        }

        public YodleeAccountNotFoundException(string message) : base(message)
        {
        }

        public YodleeAccountNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }

		protected YodleeAccountNotFoundException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}