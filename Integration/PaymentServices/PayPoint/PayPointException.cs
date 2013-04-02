﻿using System;
using System.Runtime.Serialization;

namespace PaymentServices.PayPoint
{
    [Serializable]
    public class PayPointException : Exception
    {
        private readonly PayPointReturnData _paypointData;
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public PayPointException()
        {
        }

        public PayPointException(string message, PayPointReturnData paypointData) : base(message)
        {
            _paypointData = paypointData;
        }

        public PayPointException(string message, Exception inner) : base(message, inner)
        {
        }

        protected PayPointException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        public PayPointReturnData PaypointData
        {
            get { return _paypointData; }
        }
    }
}