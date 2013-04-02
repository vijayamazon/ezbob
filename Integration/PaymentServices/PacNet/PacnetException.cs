﻿using System;
using System.Runtime.Serialization;

namespace PaymentServices.PacNet
{
    [Serializable]
    public class PacnetException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public PacnetException()
        {
        }

        public PacnetException(string message) : base(message)
        {
        }

        public PacnetException(string message, Exception inner) : base(message, inner)
        {
        }

        protected PacnetException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}