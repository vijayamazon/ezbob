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

namespace EzBob3dParties.Amazon.Src.OrdersApi {
    using System;
    using System.Threading.Tasks;
    using EzBob3dParties.Amazon.Src.Common;
    using EzBob3dParties.Amazon.Src.OrdersApi.Model;

    /// <summary>
    /// ImwssOrdersServiceClient is an implementation of MarketplaceWebServiceOrders
    /// </summary>
    public class ImwssOrdersServiceClient : IMwsOrdersService {

//        private const string libraryVersion = "2015-09-24";
        private const string libraryVersion = "2013-09-01";

        private string servicePath;

        private MwsConnection connection;

        public ImwssOrdersServiceClient() { }

        /// <summary>
        /// Create client.
        /// </summary>
        /// <param name="accessKey">Access Key</param>
        /// <param name="secretKey">Secret Key</param>
        /// <param name="applicationName">Application Name</param>
        /// <param name="applicationVersion">Application Version</param>
        /// <param name="config">configuration</param>
        public ImwssOrdersServiceClient(
            string accessKey,
            string secretKey,
            string applicationName,
            string applicationVersion,
            MarketplaceWebServiceOrdersConfig config) {
            this.connection = config.CopyConnection();
            this.connection.AwsAccessKeyId = accessKey;
            this.connection.AwsSecretKeyId = secretKey;
            this.connection.ApplicationName = applicationName;
            this.connection.ApplicationVersion = applicationVersion;
            this.connection.LibraryVersion = libraryVersion;
            this.servicePath = config.ServicePath;
        }

        /// <summary>
        /// Create client.
        /// </summary>
        /// <param name="accessKey">Access Key</param>
        /// <param name="secretKey">Secret Key</param>
        /// <param name="config">configuration</param>
        public ImwssOrdersServiceClient(String accessKey, String secretKey, MarketplaceWebServiceOrdersConfig config) {
            this.connection = config.CopyConnection();
            this.connection.AwsAccessKeyId = accessKey;
            this.connection.AwsSecretKeyId = secretKey;
            this.connection.LibraryVersion = libraryVersion;
            this.servicePath = config.ServicePath;
        }

        /// <summary>
        /// Create client.
        /// </summary>
        /// <param name="accessKey">Access Key</param>
        /// <param name="secretKey">Secret Key</param>
        public ImwssOrdersServiceClient(String accessKey, String secretKey)
            : this(accessKey, secretKey, new MarketplaceWebServiceOrdersConfig()) {}

        /// <summary>
        /// Create client.
        /// </summary>
        /// <param name="accessKey">Access Key</param>
        /// <param name="secretKey">Secret Key</param>
        /// <param name="applicationName">Application Name</param>
        /// <param name="applicationVersion">Application Version</param>
        public ImwssOrdersServiceClient(
            String accessKey,
            String secretKey,
            String applicationName,
            String applicationVersion)
            : this(accessKey, secretKey, applicationName,
                applicationVersion, new MarketplaceWebServiceOrdersConfig()) {}

        /// <summary>
        /// Added to initialize service without too much changes in original code .
        /// </summary>
        /// <param name="accessKey">The access key.</param>
        /// <param name="secretKey">The secret key.</param>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="applicationVersion">The application version.</param>
        public void Init(string accessKey, string secretKey, string applicationName, string applicationVersion) {
            var config = new MarketplaceWebServiceOrdersConfig();
            config.ServiceURL = "https://mws.amazonservices.co.uk/Orders/2013-09-01";
            this.connection = config.CopyConnection();
            this.connection.AwsAccessKeyId = accessKey;
            this.connection.AwsSecretKeyId = secretKey;
            this.connection.ApplicationName = applicationName;
            this.connection.ApplicationVersion = applicationVersion;
            this.connection.LibraryVersion = libraryVersion;
            this.servicePath = config.ServicePath;
        }

        public Task<GetOrderResponse> GetOrder(GetOrderRequest request) {
            return Task.Run(() => this.connection.Call(
                new ImwssOrdersServiceClient.Request<GetOrderResponse>("GetOrder", typeof(GetOrderResponse), this.servicePath),
                request));
        }

        public Task<GetServiceStatusResponse> GetServiceStatus(GetServiceStatusRequest request) {
            return Task.Run(() => this.connection.Call(
                new ImwssOrdersServiceClient.Request<GetServiceStatusResponse>("GetServiceStatus", typeof(GetServiceStatusResponse), this.servicePath),
                request));
        }

        public Task<ListOrderItemsResponse> ListOrderItems(ListOrderItemsRequest request) {
            return Task.Run(() => this.connection.Call(
                new ImwssOrdersServiceClient.Request<ListOrderItemsResponse>("ListOrderItems", typeof(ListOrderItemsResponse), this.servicePath),
                request));
        }

        public Task<ListOrderItemsByNextTokenResponse> ListOrderItemsByNextToken(ListOrderItemsByNextTokenRequest request) {
            return Task.Run(() => this.connection.Call(
                new ImwssOrdersServiceClient.Request<ListOrderItemsByNextTokenResponse>("ListOrderItemsByNextToken", typeof(ListOrderItemsByNextTokenResponse), this.servicePath),
                request));
        }

        public Task<ListOrdersResponse> ListOrders(ListOrdersRequest request) {
            return Task.Run(() => this.connection.Call(
                new ImwssOrdersServiceClient.Request<ListOrdersResponse>("ListOrders", typeof(ListOrdersResponse), this.servicePath),
                request));
        }

        public Task<ListOrdersByNextTokenResponse> ListOrdersByNextToken(ListOrdersByNextTokenRequest request) {
            return Task.Run(() => this.connection.Call(
                new ImwssOrdersServiceClient.Request<ListOrdersByNextTokenResponse>("ListOrdersByNextToken", typeof(ListOrdersByNextTokenResponse), this.servicePath),
                request));
        }

        private class Request<R> : IMwsRequestType<R>
            where R : IMwsObject {
            private string operationName;
            private Type responseClass;
            private string servicePath;

            public Request(string operationName, Type responseClass, string servicePath) {
                this.operationName = operationName;
                this.responseClass = responseClass;
                this.servicePath = servicePath;
            }

            public string ServicePath
            {
                get { return this.servicePath; }
            }

            public string OperationName
            {
                get { return this.operationName; }
            }

            public Type ResponseClass
            {
                get { return this.responseClass; }
            }

            public MwsException WrapException(Exception cause) {
                return new MarketplaceWebServiceOrdersException(cause);
            }

            public void SetResponseHeaderMetadata(IMwsObject response, MwsResponseHeaderMetadata rhmd) {
                ((IMWSResponse)response).ResponseHeaderMetadata = new ResponseHeaderMetadata(rhmd);
            }

        }
    }
}
