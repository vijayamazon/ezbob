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
    using EzBob3dParties.Amazon.Src.CustomerApi.Model;

    /// <summary>
    /// 
    /// </summary>
    public interface IMwsCustomerService {
        /// <summary>
        /// Added to initialize service without too much changes in original code .
        /// </summary>
        /// <param name="accessKey">The access key.</param>
        /// <param name="secretKey">The secret key.</param>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="applicationVersion">The application version.</param>
        void Init(string accessKey, string secretKey, string applicationName,string applicationVersion);


        /// <summary>
        /// Get Customers By Customer Id
        /// </summary>
        /// <param name="request">GetCustomersByCustomerIdRequest request.</param>
        /// <returns>GetCustomersByCustomerIdResponse response</returns>
        /// <remarks>
        /// Returns customer information for
        ///         one or more customers, given a seller-directed customer ID.
        ///         There is a maximum number of customer IDs supported per request.
        ///         See service documentation for that limit.
        /// </remarks>
        GetCustomersByCustomerIdResponse GetCustomersByCustomerId(GetCustomersByCustomerIdRequest request);

        /// <summary>
        /// Get Customers For Customer Id
        /// </summary>
        /// <param name="request">GetCustomersForCustomerIdRequest request.</param>
        /// <returns>GetCustomersForCustomerIdResponse response</returns>
        /// <remarks>
        /// Returns customer information for
        ///         one or more customers, given a seller-directed customer ID.
        ///         There is a maximum number of customer IDs supported per request.
        ///         See service documentation for that limit.
        /// </remarks>
        GetCustomersForCustomerIdResponse GetCustomersForCustomerId(GetCustomersForCustomerIdRequest request);

        /// <summary>
        /// List Customers
        /// </summary>
        /// <param name="request">ListCustomersRequest request.</param>
        /// <returns>ListCustomersResponse response</returns>
        /// <remarks>
        /// Returns a (potentially
        ///         paginated) list of customer records based on the criteria in the
        ///         request. The result will be at most a single page of results
        ///         plus a next token that can be used to obtain the next page of
        ///         results, if any.
        /// </remarks>
        ListCustomersResponse ListCustomers(ListCustomersRequest request);

        /// <summary>
        /// List Customers By Next Token
        /// </summary>
        /// <param name="request">ListCustomersByNextTokenRequest request.</param>
        /// <returns>ListCustomersByNextTokenResponse response</returns>
        /// <remarks>
        /// Returns the next page of results
        ///         based on the next token given from a previous call to
        ///         ListCustomers or ListCustomersByNextToken, plus a next token
        ///         that can be used to obtain the next page of results, if any.
        /// </remarks>
        ListCustomersByNextTokenResponse ListCustomersByNextToken(ListCustomersByNextTokenRequest request);

        /// <summary>
        /// Get Service Status
        /// </summary>
        /// <param name="request">GetServiceStatusRequest request.</param>
        /// <returns>GetServiceStatusResponse response</returns>
        /// <remarks>
        /// 
        /// </remarks>
        GetServiceStatusResponse GetServiceStatus(GetServiceStatusRequest request);

    }
}
