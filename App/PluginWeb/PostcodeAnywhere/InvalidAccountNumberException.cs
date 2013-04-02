using System;
using System.Runtime.Serialization;

namespace PostcodeAnywhere
{
    [Serializable]
    public class InvalidAccountNumberException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public InvalidAccountNumberException()
        {
        }

        public InvalidAccountNumberException(string message) : base(message)
        {
        }

        public InvalidAccountNumberException(string message, Exception inner) : base(message, inner)
        {
        }

        protected InvalidAccountNumberException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}