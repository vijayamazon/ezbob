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

namespace EzBob3dParties.Amazon.Src.OrdersApi
{
    using System.Threading.Tasks;
    using EzBob3dParties.Amazon.Src.OrdersApi.Model;

    /// <summary>
    /// This contains the Order Retrieval API section of the Marketplace Web Service.
    /// </summary>
    public interface IMwsOrdersService 
    {
        /// <summary>
        /// Added to initialize service without too much changes in original code .
        /// </summary>
        /// <param name="accessKey">The access key.</param>
        /// <param name="secretKey">The secret key.</param>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="applicationVersion">The application version.</param>
        void Init(string accessKey, string secretKey, string applicationName, string applicationVersion);
        /// <summary>
        /// Get Order
        /// </summary>
        /// <param name="request">GetOrderRequest request.</param>
        /// <returns>
        /// GetOrderResponse response
        /// </returns>
        /// <remarks>
        /// This operation takes up to 50 order ids and returns the corresponding orders.
        /// </remarks>
        Task<GetOrderResponse> GetOrder(GetOrderRequest request);

        /// <summary>
        /// Get Service Status
        /// </summary>
        /// <param name="request">GetServiceStatusRequest request.</param>
        /// <returns>GetServiceStatusResponse response</returns>
        /// <remarks>
        /// Returns the service status of a particular MWS API section. The operation
        /// 		takes no input.
        /// </remarks>
        Task<GetServiceStatusResponse> GetServiceStatus(GetServiceStatusRequest request);

        /// <summary>
        /// List Order Items
        /// </summary>
        /// <param name="request">ListOrderItemsRequest request.</param>
        /// <returns>ListOrderItemsResponse response</returns>
        /// <remarks>
        /// This operation can be used to list the items of the order indicated by the
        ///         given order id (only a single Amazon order id is allowed).
        /// </remarks>
        Task<ListOrderItemsResponse> ListOrderItems(ListOrderItemsRequest request);

        /// <summary>
        /// List Order Items By Next Token
        /// </summary>
        /// <param name="request">ListOrderItemsByNextTokenRequest request.</param>
        /// <returns>ListOrderItemsByNextTokenResponse response</returns>
        /// <remarks>
        /// If ListOrderItems cannot return all the order items in one go, it will
        ///         provide a nextToken. That nextToken can be used with this operation to
        ///         retrive the next batch of items for that order.
        /// </remarks>
        Task<ListOrderItemsByNextTokenResponse> ListOrderItemsByNextToken(ListOrderItemsByNextTokenRequest request);

        /// <summary>
        /// List Orders
        /// </summary>
        /// <param name="request">ListOrdersRequest request.</param>
        /// <returns>ListOrdersResponse response</returns>
        /// <remarks>
        /// ListOrders can be used to find orders that meet the specified criteria.
        /// </remarks>
        Task<ListOrdersResponse> ListOrders(ListOrdersRequest request);

        /// <summary>
        /// List Orders By Next Token
        /// </summary>
        /// <param name="request">ListOrdersByNextTokenRequest request.</param>
        /// <returns>ListOrdersByNextTokenResponse response</returns>
        /// <remarks>
        /// If ListOrders returns a nextToken, thus indicating that there are more orders
        ///         than returned that matched the given filter criteria, ListOrdersByNextToken
        ///         can be used to retrieve those other orders using that nextToken.
        /// </remarks>
        Task<ListOrdersByNextTokenResponse> ListOrdersByNextToken(ListOrdersByNextTokenRequest request);

    }
}
