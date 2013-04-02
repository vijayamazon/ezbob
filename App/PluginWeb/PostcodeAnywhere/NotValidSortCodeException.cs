using System;
using System.Runtime.Serialization;

namespace PostcodeAnywhere
{
    [Serializable]
    public class NotValidSortCodeException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public NotValidSortCodeException()
        {
        }

        public NotValidSortCodeException(string message) : base(message)
        {
        }

        public NotValidSortCodeException(string message, Exception inner) : base(message, inner)
        {
        }

        protected NotValidSortCodeException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}