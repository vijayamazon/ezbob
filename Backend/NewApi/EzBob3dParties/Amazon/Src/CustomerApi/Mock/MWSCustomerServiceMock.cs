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

namespace EzBob3dParties.Amazon.Src.CustomerApi.Mock
{
    using System;
    using System.IO;
    using System.Reflection;
    using EzBob3dParties.Amazon.Src.Common;
    using EzBob3dParties.Amazon.Src.CustomerApi.Model;

    /// <summary>
    /// MWSCustomerServiceMock is the implementation of MWSCustomerService based
    /// on the pre-populated set of XML files that serve local data. It simulates
    /// responses from MWS.
    /// </summary>
    /// <remarks>
    /// Use this to test your application without making a call to MWS
    ///
    /// Note, current Mock Service implementation does not validate requests
    /// </remarks>
    public class MWSCustomerServiceMock : IMwsCustomerService
    {
        /// <summary>
        /// Added to initialize service without too much changes in original code .
        /// </summary>
        /// <param name="accessKey">The access key.</param>
        /// <param name="secretKey">The secret key.</param>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="applicationVersion">The application version.</param>
        public void Init(string accessKey, string secretKey, string applicationName, string applicationVersion) {
        }

        public GetCustomersByCustomerIdResponse GetCustomersByCustomerId(GetCustomersByCustomerIdRequest request)
        {
            return newResponse<GetCustomersByCustomerIdResponse>();
        }

        public GetCustomersForCustomerIdResponse GetCustomersForCustomerId(GetCustomersForCustomerIdRequest request)
        {
            return newResponse<GetCustomersForCustomerIdResponse>();
        }

        public ListCustomersResponse ListCustomers(ListCustomersRequest request)
        {
            return newResponse<ListCustomersResponse>();
        }

        public ListCustomersByNextTokenResponse ListCustomersByNextToken(ListCustomersByNextTokenRequest request)
        {
            return newResponse<ListCustomersByNextTokenResponse>();
        }

        public GetServiceStatusResponse GetServiceStatus(GetServiceStatusRequest request)
        {
            return newResponse<GetServiceStatusResponse>();
        }

        private T newResponse<T>() where T : IMWSResponse {
            Stream xmlIn = null;
            try {
                xmlIn = Assembly.GetAssembly(this.GetType()).GetManifestResourceStream(typeof(T).FullName + ".xml");
                StreamReader xmlInReader = new StreamReader(xmlIn);
                string xmlStr = xmlInReader.ReadToEnd();

                MwsXmlReader reader = new MwsXmlReader(xmlStr);
                T obj = (T) Activator.CreateInstance(typeof(T));
                obj.ReadFragmentFrom(reader);
                obj.ResponseHeaderMetadata = new ResponseHeaderMetadata("mockRequestId", "A,B,C", "mockTimestamp", 0d, 0d, new DateTime());
                return obj;
            }
            catch (Exception e)
            {
                throw MwsUtil.Wrap(e);
            }
            finally
            {
                if (xmlIn != null) { xmlIn.Close(); }
            }
        }
    }
}
