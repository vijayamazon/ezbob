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
 * List Marketplace Participations Result
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
    public class GetAuthTokenResult : AbstractMwsObject
    {

        private string _MWSAuthToken;
        private string _sellerId;

        /// <summary>
        /// Gets and sets the NextToken property.
        /// </summary>
        [XmlElementAttribute(ElementName = "MWSAuthToken")]
        public string MWSAuthToken
        {
            get { return this._MWSAuthToken; }
            set { this._MWSAuthToken = value; }
        }

        /// <summary>
        /// Sets the NextToken property.
        /// </summary>
        /// <param name="nextToken">NextToken property.</param>
        /// <returns>this instance.</returns>
        public GetAuthTokenResult WithMWSAuthToken(string MWSAuthToken)
        {
            this._MWSAuthToken = MWSAuthToken;
            return this;
        }

        /// <summary>
        /// Checks if NextToken property is set.
        /// </summary>
        /// <returns>true if NextToken property is set.</returns>
        public bool IsSetNextToken()
        {
            return this._MWSAuthToken != null;
        }

        /// <summary>
        /// Gets and sets the ListParticipations property.
        /// </summary>
        [XmlElementAttribute(ElementName = "SellerId")]
        public string SellerId
        {
            get { return this._sellerId; }
            set { this._sellerId = value; }
        }

        /// <summary>
        /// Sets the ListParticipations property.
        /// </summary>
        /// <param name="listParticipations">ListParticipations property.</param>
        /// <returns>this instance.</returns>
        public GetAuthTokenResult WithsellerId(string sellerId)
        {
            this._sellerId = sellerId;
            return this;
        }

        /// <summary>
        /// Checks if ListParticipations property is set.
        /// </summary>
        /// <returns>true if ListParticipations property is set.</returns>
        public bool IsSetsellerId()
        {
            return this._sellerId != null;
        }

        public override void ReadFragmentFrom(IMwsReader reader)
        {
            _MWSAuthToken = reader.Read<string>("MWSAuthToken");
            _sellerId = reader.Read<string>("SellerId");
        }

        public override void WriteFragmentTo(IMwsWriter writer)
        {
            writer.Write("MWSAuthToken", _MWSAuthToken);
            writer.Write("SellerId", _sellerId);
        }

        public override void WriteTo(IMwsWriter writer)
        {
            writer.Write("https://mws.amazonservices.com/Sellers/2011-07-01", "GetAuthTokenResult ", this);
        }

        public GetAuthTokenResult(): base()
        {
        }
    }
}
