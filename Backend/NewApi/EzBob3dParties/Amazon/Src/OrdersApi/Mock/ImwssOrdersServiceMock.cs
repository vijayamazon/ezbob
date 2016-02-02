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
 * Marketplace Web Service Orders
 * API Version: 2013-09-01
 * Library Version: 2015-09-24
 * Generated: Fri Sep 25 20:06:25 GMT 2015
 */

namespace EzBob3dParties.Amazon.Src.OrdersApi.Mock
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using EzBob3dParties.Amazon.Src.Common;
    using EzBob3dParties.Amazon.Src.OrdersApi.Model;

    /// <summary>
    /// ImwssOrdersServiceMock is the implementation of MarketplaceWebServiceOrders based
    /// on the pre-populated set of XML files that serve local data. It simulates
    /// responses from MWS.
    /// </summary>
    /// <remarks>
    /// Use this to test your application without making a call to MWS
    ///
    /// Note, current Mock Service implementation does not validate requests
    /// </remarks>
    public class ImwssOrdersServiceMock : IMwsOrdersService
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

        public Task<GetOrderResponse> GetOrder(GetOrderRequest request)
        {
            return newResponse<GetOrderResponse>();
        }

        public Task<GetServiceStatusResponse> GetServiceStatus(GetServiceStatusRequest request)
        {
            return newResponse<GetServiceStatusResponse>();
        }

        public Task<ListOrderItemsResponse> ListOrderItems(ListOrderItemsRequest request)
        {
            return newResponse<ListOrderItemsResponse>();
        }

        public Task<ListOrderItemsByNextTokenResponse> ListOrderItemsByNextToken(ListOrderItemsByNextTokenRequest request)
        {
            return newResponse<ListOrderItemsByNextTokenResponse>();
        }

        public Task<ListOrdersResponse> ListOrders(ListOrdersRequest request)
        {
            return newResponse<ListOrdersResponse>();
        }

        public Task<ListOrdersByNextTokenResponse> ListOrdersByNextToken(ListOrdersByNextTokenRequest request)
        {
            return newResponse<ListOrdersByNextTokenResponse>();
        }

        private Task<T> newResponse<T>() where T : IMWSResponse {
            Stream xmlIn = null;
            try {
                xmlIn = Assembly.GetAssembly(this.GetType()).GetManifestResourceStream(typeof(T).FullName + ".xml");
                StreamReader xmlInReader = new StreamReader(xmlIn);
                string xmlStr = xmlInReader.ReadToEnd();

                MwsXmlReader reader = new MwsXmlReader(xmlStr);
                T obj = (T) Activator.CreateInstance(typeof(T));
                obj.ReadFragmentFrom(reader);
                obj.ResponseHeaderMetadata = new ResponseHeaderMetadata("mockRequestId", "A,B,C", "mockTimestamp", 0d, 0d, new DateTime());
                TaskCompletionSource<T> taskSource = new TaskCompletionSource<T>();
                taskSource.SetResult(obj);
                return taskSource.Task;
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
