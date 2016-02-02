/*******************************************************************************
 * Copyright 2009-2015 Amazon Services. All Rights Reserved.
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 *
 * You may not use this file except in compliance with the License. 
 * You may obtain a copy of the License at: http://aws.amazon.com/apache2.0
 * This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
 * CONDITIONS OF ANY KIND, either express or implied. See the License for the 
 * specific language governing permissions and limitations under the License.
 *******************************************************************************
 * MWS Customer Service
 * API Version: 2014-03-01
 * Library Version: 2015-06-18
 * Generated: Thu Jun 18 19:32:10 GMT 2015
 */

namespace EzBob3dParties.Amazon.Src.CustomerApi
{
    using System;
    using System.Net;
    using EzBob3dParties.Amazon.Src.Common;
    using EzBob3dParties.Amazon.Src.CustomerApi.Model;

    /// <summary>
    /// Exception thrown by MWSCustomerService operations
    /// </summary>
    public class MWSCustomerServiceException : MwsException
    {

        public MWSCustomerServiceException(string message, HttpStatusCode statusCode)
            : base((int)statusCode, message, null, null, null, null) { }

        public MWSCustomerServiceException(string message)
            : base(0, message, null) { }

        public MWSCustomerServiceException(string message, HttpStatusCode statusCode, ResponseHeaderMetadata rhmd)
            : base((int)statusCode, message, null, null, null, rhmd) { }

        public MWSCustomerServiceException(Exception ex)
            : base(ex) { }

        public MWSCustomerServiceException(string message, Exception ex)
            : base(0, message, ex) { }

        public MWSCustomerServiceException(string message, HttpStatusCode statusCode, string errorCode,
            string errorType, string requestId, string xml, ResponseHeaderMetadata rhmd)
            : base((int)statusCode, message, errorCode, errorType, xml, rhmd) { }

        public new ResponseHeaderMetadata ResponseHeaderMetadata
        {
            get 
            { 
                MwsResponseHeaderMetadata baseRHMD = base.ResponseHeaderMetadata;
                if(baseRHMD != null)
                {
                    return new ResponseHeaderMetadata(baseRHMD); 
                }
                else
                {
                    return null;
                }
            }
        }

    }

}

