using System;
using System.Runtime.Serialization;

namespace PostcodeAnywhere
{
    [Serializable]
    public class SortCodeNotFoundException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public SortCodeNotFoundException()
        {
        }

        public SortCodeNotFoundException(string message) : base(message)
        {
        }

        public SortCodeNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }

        protected SortCodeNotFoundException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}