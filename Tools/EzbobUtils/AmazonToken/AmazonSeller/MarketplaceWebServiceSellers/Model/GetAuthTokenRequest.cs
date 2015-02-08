/*******************************************************************************
 * Copyright 2009-2014 Amazon Services. All Rights Reserved.
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 *
 * You may not use this file except in compliance with the License. 
 * You may obtain a copy of the License at: http://aws.amazon.com/apache2.0
 * This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
 * CONDITIONS OF ANY KIND, either express or implied. See the License for the 
 * specific language governing permissions and limitations under the License.
 *******************************************************************************
 * List Marketplace Participations Request
 * API Version: 2011-07-01
 * Library Version: 2014-09-30
 * Generated: Mon Sep 15 19:38:40 GMT 2014
 */


using System;
using System.Xml;
using System.Xml.Serialization;
using MWSClientCsRuntime;

namespace MarketplaceWebServiceSellers.Model
{
    [XmlTypeAttribute(Namespace = "https://mws.amazonservices.com/Sellers/2011-07-01")]
    [XmlRootAttribute(Namespace = "https://mws.amazonservices.com/Sellers/2011-07-01", IsNullable = false)]
    public class GetAuthTokenRequest  : AbstractMwsObject
    {

        private string _sellerId;

        /// <summary>
        /// Gets and sets the SellerId property.
        /// </summary>
        [XmlElementAttribute(ElementName = "SellerId")]
        public string SellerId
        {
            get { return this._sellerId; }
            set { this._sellerId = value; }
        }

        /// <summary>
        /// Sets the SellerId property.
        /// </summary>
        /// <param name="sellerId">SellerId property.</param>
        /// <returns>this instance.</returns>
        public GetAuthTokenRequest WithSellerId(string sellerId)
        {
            this._sellerId = sellerId;
            return this;
        }

        /// <summary>
        /// Checks if SellerId property is set.
        /// </summary>
        /// <returns>true if SellerId property is set.</returns>
        public bool IsSetSellerId()
        {
            return this._sellerId != null;
        }

        public override void ReadFragmentFrom(IMwsReader reader)
        {
            _sellerId = reader.Read<string>("SellerId");
        }

        public override void WriteFragmentTo(IMwsWriter writer)
        {
            writer.Write("SellerId", _sellerId);
        }

        public override void WriteTo(IMwsWriter writer)
        {
            writer.Write("https://mws.amazonservices.com/Sellers/2011-07-01", "GetAuthTokenRequest ", this);
        }

        public GetAuthTokenRequest(): base()
        {
        }
    }
}
