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

namespace EzBob3dParties.Amazon.Src.CustomerApi {
    using System;
    using System.Collections.Generic;
    using EzBob3dParties.Amazon.Src.CustomerApi.Model;

    /// <summary>
    /// Runnable sample code to demonstrate usage of the C# client.
    ///
    /// To use, import the client source as a console application,
    /// and mark this class as the startup object. Then, replace
    /// parameters below with sensible values and run.
    /// </summary>
    public class MWSCustomerServiceSample {

        public static void Main2(string[] args)
        {
            // TODO: Set the below configuration variables before attempting to run

            // Developer AWS access key
            string accessKey = "replaceWithAccessKey";

            // Developer AWS secret key
            string secretKey = "replaceWithSecretKey";

            // The client application name
            string appName = "CSharpSampleCode";

            // The client application version
            string appVersion = "1.0";

            // The endpoint for region service and version (see developer guide)
            // ex: https://mws.amazonservices.com
            string serviceURL = "replaceWithServiceURL";

            // Create a configuration object
            MWSCustomerServiceConfig config = new MWSCustomerServiceConfig();
            config.ServiceURL = serviceURL;
            // Set other client connection configurations here if needed
            // Create the client itself
            IMwsCustomerService client = new MWSCustomerServiceClient(accessKey, secretKey, appName, appVersion, config);

            MWSCustomerServiceSample sample = new MWSCustomerServiceSample(client);

            // Uncomment the operation you'd like to test here
            // TODO: Modify the request created in the Invoke method to be valid

            try 
            {
                IMWSResponse response = null;
                // response = sample.InvokeGetCustomersByCustomerId();
                // response = sample.InvokeGetCustomersForCustomerId();
                // response = sample.InvokeListCustomers();
                // response = sample.InvokeListCustomersByNextToken();
                // response = sample.InvokeGetServiceStatus();
                Console.WriteLine("Response:");
                ResponseHeaderMetadata rhmd = response.ResponseHeaderMetadata;
                // We recommend logging the request id and timestamp of every call.
                Console.WriteLine("RequestId: " + rhmd.RequestId);
                Console.WriteLine("Timestamp: " + rhmd.Timestamp);
                string responseXml = response.ToXML();
                Console.WriteLine(responseXml);
            }
            catch (MWSCustomerServiceException ex)
            {
                // Exception properties are important for diagnostics.
                ResponseHeaderMetadata rhmd = ex.ResponseHeaderMetadata;
                Console.WriteLine("Service Exception:");
                if(rhmd != null)
                {
                    Console.WriteLine("RequestId: " + rhmd.RequestId);
                    Console.WriteLine("Timestamp: " + rhmd.Timestamp);
                }
                Console.WriteLine("Message: " + ex.Message);
                Console.WriteLine("StatusCode: " + ex.StatusCode);
                Console.WriteLine("ErrorCode: " + ex.ErrorCode);
                Console.WriteLine("ErrorType: " + ex.ErrorType);
                throw ex;
            }
        }

        private readonly IMwsCustomerService client;

        public MWSCustomerServiceSample(IMwsCustomerService client)
        {
            this.client = client;
        }

        public GetCustomersByCustomerIdResponse InvokeGetCustomersByCustomerId()
        {
            // Create a request.
            GetCustomersByCustomerIdRequest request = new GetCustomersByCustomerIdRequest();
            string sellerId = "example";
            request.SellerId = sellerId;
            string mwsAuthToken = "example";
            request.MWSAuthToken = mwsAuthToken;
            string marketplaceId = "example";
            request.MarketplaceId = marketplaceId;
            List<string> customerIdList = new List<string>();
            request.CustomerIdList = customerIdList;
            return this.client.GetCustomersByCustomerId(request);
        }

        public GetCustomersForCustomerIdResponse InvokeGetCustomersForCustomerId()
        {
            // Create a request.
            GetCustomersForCustomerIdRequest request = new GetCustomersForCustomerIdRequest();
            string sellerId = "example";
            request.SellerId = sellerId;
            string mwsAuthToken = "example";
            request.MWSAuthToken = mwsAuthToken;
            string marketplaceId = "example";
            request.MarketplaceId = marketplaceId;
            List<string> customerIdList = new List<string>();
            request.CustomerIdList = customerIdList;
            return this.client.GetCustomersForCustomerId(request);
        }

        public ListCustomersResponse InvokeListCustomers()
        {
            // Create a request.
            ListCustomersRequest request = new ListCustomersRequest();
            string sellerId = "example";
            request.SellerId = sellerId;
            string mwsAuthToken = "example";
            request.MWSAuthToken = mwsAuthToken;
            string marketplaceId = "example";
            request.MarketplaceId = marketplaceId;
            string dateRangeType = "example";
            request.DateRangeType = dateRangeType;
            DateTime dateRangeStart = new DateTime();
            request.DateRangeStart = dateRangeStart;
            DateTime dateRangeEnd = new DateTime();
            request.DateRangeEnd = dateRangeEnd;
            return this.client.ListCustomers(request);
        }

        public ListCustomersByNextTokenResponse InvokeListCustomersByNextToken()
        {
            // Create a request.
            ListCustomersByNextTokenRequest request = new ListCustomersByNextTokenRequest();
            string sellerId = "example";
            request.SellerId = sellerId;
            string mwsAuthToken = "example";
            request.MWSAuthToken = mwsAuthToken;
            string nextToken = "example";
            request.NextToken = nextToken;
            return this.client.ListCustomersByNextToken(request);
        }

        public GetServiceStatusResponse InvokeGetServiceStatus()
        {
            // Create a request.
            GetServiceStatusRequest request = new GetServiceStatusRequest();
            string sellerId = "example";
            request.SellerId = sellerId;
            string mwsAuthToken = "example";
            request.MWSAuthToken = mwsAuthToken;
            return this.client.GetServiceStatus(request);
        }


    }
}
