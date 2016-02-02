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
    using EzBob3dParties.Amazon.Src.Common;
    using EzBob3dParties.Amazon.Src.CustomerApi.Model;

    /// <summary>
    /// MWSCustomerServiceClient is an implementation of MWSCustomerService
    /// </summary>
    public class MWSCustomerServiceClient : IMwsCustomerService 
    {

        private const string libraryVersion = "2015-06-18";

        private string servicePath;

        private MwsConnection connection;

        /// <summary>
        /// Used in by ezbob DI container
        /// </summary>
        public MWSCustomerServiceClient() {}

        /// <summary>
        /// Create client.
        /// </summary>
        /// <param name="accessKey">Access Key</param>
        /// <param name="secretKey">Secret Key</param>
        /// <param name="applicationName">Application Name</param>
        /// <param name="applicationVersion">Application Version</param>
        /// <param name="config">configuration</param>
        public MWSCustomerServiceClient(
            string accessKey,
            string secretKey,
            string applicationName,
            string applicationVersion,
            MWSCustomerServiceConfig config)
        {
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
        public MWSCustomerServiceClient(String accessKey, String secretKey, MWSCustomerServiceConfig config)
        {
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
        public MWSCustomerServiceClient(String accessKey, String secretKey)
            : this(accessKey, secretKey, new MWSCustomerServiceConfig())
        {
        }

        /// <summary>
        /// Create client.
        /// </summary>
        /// <param name="accessKey">Access Key</param>
        /// <param name="secretKey">Secret Key</param>
        /// <param name="applicationName">Application Name</param>
        /// <param name="applicationVersion">Application Version</param>
        public MWSCustomerServiceClient(
            String accessKey, 
            String secretKey,
            String applicationName,
            String applicationVersion ) 
            : this(accessKey, secretKey, applicationName,
                applicationVersion, new MWSCustomerServiceConfig())
        {
        }

        /// <summary>
        /// Added to initialize service without too much changes in original code .
        /// </summary>
        /// <param name="accessKey">The access key.</param>
        /// <param name="secretKey">The secret key.</param>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="applicationVersion">The application version.</param>
        public void Init(string accessKey,
            string secretKey,
            string applicationName,
            string applicationVersion) {

            var config = new MWSCustomerServiceConfig();
            this.connection = config.CopyConnection();
            this.connection.AwsAccessKeyId = accessKey;
            this.connection.AwsSecretKeyId = secretKey;
            this.connection.ApplicationName = applicationName;
            this.connection.ApplicationVersion = applicationVersion;
            this.connection.LibraryVersion = libraryVersion;
            this.servicePath = config.ServicePath;
        }

        public GetCustomersByCustomerIdResponse GetCustomersByCustomerId(GetCustomersByCustomerIdRequest request)
        {
            return this.connection.Call(
                new MWSCustomerServiceClient.Request<GetCustomersByCustomerIdResponse>("GetCustomersByCustomerId", typeof(GetCustomersByCustomerIdResponse), this.servicePath),
                request);
        }

        public GetCustomersForCustomerIdResponse GetCustomersForCustomerId(GetCustomersForCustomerIdRequest request)
        {
            return this.connection.Call(
                new MWSCustomerServiceClient.Request<GetCustomersForCustomerIdResponse>("GetCustomersForCustomerId", typeof(GetCustomersForCustomerIdResponse), this.servicePath),
                request);
        }

        public ListCustomersResponse ListCustomers(ListCustomersRequest request)
        {
            return this.connection.Call(
                new MWSCustomerServiceClient.Request<ListCustomersResponse>("ListCustomers", typeof(ListCustomersResponse), this.servicePath),
                request);
        }

        public ListCustomersByNextTokenResponse ListCustomersByNextToken(ListCustomersByNextTokenRequest request)
        {
            return this.connection.Call(
                new MWSCustomerServiceClient.Request<ListCustomersByNextTokenResponse>("ListCustomersByNextToken", typeof(ListCustomersByNextTokenResponse), this.servicePath),
                request);
        }

        public GetServiceStatusResponse GetServiceStatus(GetServiceStatusRequest request)
        {
            return this.connection.Call(
                new MWSCustomerServiceClient.Request<GetServiceStatusResponse>("GetServiceStatus", typeof(GetServiceStatusResponse), this.servicePath),
                request);
        }

        private class Request<R> : IMwsRequestType<R> where R : IMwsObject
        {
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
                return new MWSCustomerServiceException(cause);
            }

            public void SetResponseHeaderMetadata(IMwsObject response, MwsResponseHeaderMetadata rhmd) {
                ((IMWSResponse)response).ResponseHeaderMetadata = new ResponseHeaderMetadata(rhmd);
            }

        }
    }
}
