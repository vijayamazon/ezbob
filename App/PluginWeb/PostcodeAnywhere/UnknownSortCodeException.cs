using System;
using System.Runtime.Serialization;

namespace PostcodeAnywhere
{
    [Serializable]
    public class UnknownSortCodeException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public UnknownSortCodeException()
        {
        }

        public UnknownSortCodeException(string message) : base(message)
        {
        }

        public UnknownSortCodeException(string message, Exception inner) : base(message, inner)
        {
        }

        protected UnknownSortCodeException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}