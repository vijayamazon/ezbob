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

namespace EzBob3dParties.Amazon.Src.ProductsApi.Mock
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using EzBob3dParties.Amazon.Src.Common;
    using EzBob3dParties.Amazon.Src.ProductsApi.Model;

    /// <summary>
    /// ImwsProductsServiceMock is the implementation of IMwsProductsService based
    /// on the pre-populated set of XML files that serve local data. It simulates
    /// responses from MWS.
    /// </summary>
    /// <remarks>
    /// Use this to test your application without making a call to MWS
    ///
    /// Note, current Mock Service implementation does not validate requests
    /// </remarks>
    public class ImwsProductsServiceMock : IMwsProductsService
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

        public GetCompetitivePricingForASINResponse GetCompetitivePricingForASIN(GetCompetitivePricingForASINRequest request)
        {
            return newResponse<GetCompetitivePricingForASINResponse>();
        }

        public GetCompetitivePricingForSKUResponse GetCompetitivePricingForSKU(GetCompetitivePricingForSKURequest request)
        {
            return newResponse<GetCompetitivePricingForSKUResponse>();
        }

        public GetLowestOfferListingsForASINResponse GetLowestOfferListingsForASIN(GetLowestOfferListingsForASINRequest request)
        {
            return newResponse<GetLowestOfferListingsForASINResponse>();
        }

        public GetLowestOfferListingsForSKUResponse GetLowestOfferListingsForSKU(GetLowestOfferListingsForSKURequest request)
        {
            return newResponse<GetLowestOfferListingsForSKUResponse>();
        }

        public GetLowestPricedOffersForASINResponse GetLowestPricedOffersForASIN(GetLowestPricedOffersForASINRequest request)
        {
            return newResponse<GetLowestPricedOffersForASINResponse>();
        }

        public GetLowestPricedOffersForSKUResponse GetLowestPricedOffersForSKU(GetLowestPricedOffersForSKURequest request)
        {
            return newResponse<GetLowestPricedOffersForSKUResponse>();
        }

        public GetMatchingProductResponse GetMatchingProduct(GetMatchingProductRequest request)
        {
            return newResponse<GetMatchingProductResponse>();
        }

        public GetMatchingProductForIdResponse GetMatchingProductForId(GetMatchingProductForIdRequest request)
        {
            return newResponse<GetMatchingProductForIdResponse>();
        }

        public GetMyPriceForASINResponse GetMyPriceForASIN(GetMyPriceForASINRequest request)
        {
            return newResponse<GetMyPriceForASINResponse>();
        }

        public GetMyPriceForSKUResponse GetMyPriceForSKU(GetMyPriceForSKURequest request)
        {
            return newResponse<GetMyPriceForSKUResponse>();
        }

        public GetProductCategoriesForASINResponse GetProductCategoriesForASIN(GetProductCategoriesForASINRequest request)
        {
            return newResponse<GetProductCategoriesForASINResponse>();
        }

        public Task<GetProductCategoriesForSKUResponse> GetProductCategoriesForSKU(GetProductCategoriesForSKURequest request) {
            return Task.Run(() => newResponse<GetProductCategoriesForSKUResponse>());
        }

        public GetServiceStatusResponse GetServiceStatus(GetServiceStatusRequest request)
        {
            return newResponse<GetServiceStatusResponse>();
        }

        public ListMatchingProductsResponse ListMatchingProducts(ListMatchingProductsRequest request)
        {
            return newResponse<ListMatchingProductsResponse>();
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
