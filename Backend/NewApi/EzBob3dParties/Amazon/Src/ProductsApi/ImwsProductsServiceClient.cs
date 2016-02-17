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
 * Marketplace Web Service Products
 * API Version: 2011-10-01
 * Library Version: 2015-09-01
 * Generated: Thu Sep 10 06:52:19 PDT 2015
 */

namespace EzBob3dParties.Amazon.Src.ProductsApi {
    using System;
    using System.Threading.Tasks;
    using EzBob3dParties.Amazon.Src.Common;
    using EzBob3dParties.Amazon.Src.ProductsApi.Model;

    /// <summary>
    /// ImwsProductsServiceClient is an implementation of IMwsProductsService
    /// </summary>
    public class ImwsProductsServiceClient : IMwsProductsService {

        private const string libraryVersion = "2015-09-01";

        private string servicePath;

        private MwsConnection connection;


        public ImwsProductsServiceClient() { }
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

            var config = new MarketplaceWebServiceProductsConfig();
            config.ServiceURL = "https://mws.amazonservices.co.uk/Products/2013-09-01";
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
        /// <param name="applicationName">Application Name</param>
        /// <param name="applicationVersion">Application Version</param>
        /// <param name="config">configuration</param>
        public ImwsProductsServiceClient(
            String applicationName, String applicationVersion, String accessKey, String secretKey,
            MarketplaceWebServiceProductsConfig config) {
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
        public ImwsProductsServiceClient(String accessKey, String secretKey, MarketplaceWebServiceProductsConfig config) {
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
        public ImwsProductsServiceClient(String accessKey, String secretKey)
            : this(accessKey, secretKey, new MarketplaceWebServiceProductsConfig()) {}

        /// <summary>
        /// Create client.
        /// </summary>
        /// <param name="accessKey">Access Key</param>
        /// <param name="secretKey">Secret Key</param>
        /// <param name="applicationName">Application Name</param>
        /// <param name="applicationVersion">Application Version</param>
        public ImwsProductsServiceClient(
            String accessKey,
            String secretKey,
            String applicationName,
            String applicationVersion)
            : this(accessKey, secretKey, applicationName,
                applicationVersion, new MarketplaceWebServiceProductsConfig()) {}

        public GetCompetitivePricingForASINResponse GetCompetitivePricingForASIN(GetCompetitivePricingForASINRequest request) {
            return this.connection.Call(
                new ImwsProductsServiceClient.Request<GetCompetitivePricingForASINResponse>("GetCompetitivePricingForASIN", typeof(GetCompetitivePricingForASINResponse), this.servicePath),
                request);
        }

        public GetCompetitivePricingForSKUResponse GetCompetitivePricingForSKU(GetCompetitivePricingForSKURequest request) {
            return this.connection.Call(
                new ImwsProductsServiceClient.Request<GetCompetitivePricingForSKUResponse>("GetCompetitivePricingForSKU", typeof(GetCompetitivePricingForSKUResponse), this.servicePath),
                request);
        }

        public GetLowestOfferListingsForASINResponse GetLowestOfferListingsForASIN(GetLowestOfferListingsForASINRequest request) {
            return this.connection.Call(
                new ImwsProductsServiceClient.Request<GetLowestOfferListingsForASINResponse>("GetLowestOfferListingsForASIN", typeof(GetLowestOfferListingsForASINResponse), this.servicePath),
                request);
        }

        public GetLowestOfferListingsForSKUResponse GetLowestOfferListingsForSKU(GetLowestOfferListingsForSKURequest request) {
            return this.connection.Call(
                new ImwsProductsServiceClient.Request<GetLowestOfferListingsForSKUResponse>("GetLowestOfferListingsForSKU", typeof(GetLowestOfferListingsForSKUResponse), this.servicePath),
                request);
        }

        public GetLowestPricedOffersForASINResponse GetLowestPricedOffersForASIN(GetLowestPricedOffersForASINRequest request) {
            return this.connection.Call(
                new ImwsProductsServiceClient.Request<GetLowestPricedOffersForASINResponse>("GetLowestPricedOffersForASIN", typeof(GetLowestPricedOffersForASINResponse), this.servicePath),
                request);
        }

        public GetLowestPricedOffersForSKUResponse GetLowestPricedOffersForSKU(GetLowestPricedOffersForSKURequest request) {
            return this.connection.Call(
                new ImwsProductsServiceClient.Request<GetLowestPricedOffersForSKUResponse>("GetLowestPricedOffersForSKU", typeof(GetLowestPricedOffersForSKUResponse), this.servicePath),
                request);
        }

        public GetMatchingProductResponse GetMatchingProduct(GetMatchingProductRequest request) {
            return this.connection.Call(
                new ImwsProductsServiceClient.Request<GetMatchingProductResponse>("GetMatchingProduct", typeof(GetMatchingProductResponse), this.servicePath),
                request);
        }

        public GetMatchingProductForIdResponse GetMatchingProductForId(GetMatchingProductForIdRequest request) {
            return this.connection.Call(
                new ImwsProductsServiceClient.Request<GetMatchingProductForIdResponse>("GetMatchingProductForId", typeof(GetMatchingProductForIdResponse), this.servicePath),
                request);
        }

        public GetMyPriceForASINResponse GetMyPriceForASIN(GetMyPriceForASINRequest request) {
            return this.connection.Call(
                new ImwsProductsServiceClient.Request<GetMyPriceForASINResponse>("GetMyPriceForASIN", typeof(GetMyPriceForASINResponse), this.servicePath),
                request);
        }

        public GetMyPriceForSKUResponse GetMyPriceForSKU(GetMyPriceForSKURequest request) {
            return this.connection.Call(
                new ImwsProductsServiceClient.Request<GetMyPriceForSKUResponse>("GetMyPriceForSKU", typeof(GetMyPriceForSKUResponse), this.servicePath),
                request);
        }

        public GetProductCategoriesForASINResponse GetProductCategoriesForASIN(GetProductCategoriesForASINRequest request) {
            return this.connection.Call(
                new ImwsProductsServiceClient.Request<GetProductCategoriesForASINResponse>("GetProductCategoriesForASIN", typeof(GetProductCategoriesForASINResponse), this.servicePath),
                request);
        }

        public Task<GetProductCategoriesForSKUResponse> GetProductCategoriesForSKU(GetProductCategoriesForSKURequest request) {
            return Task.Run(() => this.connection.Call(
                new ImwsProductsServiceClient.Request<GetProductCategoriesForSKUResponse>("GetProductCategoriesForSKU", typeof(GetProductCategoriesForSKUResponse), this.servicePath),
                request));
        }

        public GetServiceStatusResponse GetServiceStatus(GetServiceStatusRequest request) {
            return this.connection.Call(
                new ImwsProductsServiceClient.Request<GetServiceStatusResponse>("GetServiceStatus", typeof(GetServiceStatusResponse), this.servicePath),
                request);
        }

        public ListMatchingProductsResponse ListMatchingProducts(ListMatchingProductsRequest request) {
            return this.connection.Call(
                new ImwsProductsServiceClient.Request<ListMatchingProductsResponse>("ListMatchingProducts", typeof(ListMatchingProductsResponse), this.servicePath),
                request);
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
                return new MarketplaceWebServiceProductsException(cause);
            }

            public void SetResponseHeaderMetadata(IMwsObject response, MwsResponseHeaderMetadata rhmd) {
                ((IMWSResponse)response).ResponseHeaderMetadata = new ResponseHeaderMetadata(rhmd);
            }

        }
    }
}
