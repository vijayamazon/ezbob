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
 * Get My Price For ASIN Request
 * API Version: 2011-10-01
 * Library Version: 2015-09-01
 * Generated: Thu Sep 10 06:52:19 PDT 2015
 */

namespace EzBob3dParties.Amazon.Src.ProductsApi.Model
{
    using System.Xml.Serialization;
    using EzBob3dParties.Amazon.Src.Common;

    [XmlType(Namespace = "http://mws.amazonservices.com/schema/Products/2011-10-01")]
    [XmlRoot(Namespace = "http://mws.amazonservices.com/schema/Products/2011-10-01", IsNullable = false)]
    public class GetMyPriceForASINRequest : AbstractMwsObject
    {

        private string _sellerId;
        private string _mwsAuthToken;
        private string _marketplaceId;
        private ASINListType _asinList;

        /// <summary>
        /// Gets and sets the SellerId property.
        /// </summary>
        [XmlElement(ElementName = "SellerId")]
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
        public GetMyPriceForASINRequest WithSellerId(string sellerId)
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

        /// <summary>
        /// Gets and sets the MWSAuthToken property.
        /// </summary>
        [XmlElement(ElementName = "MWSAuthToken")]
        public string MWSAuthToken
        {
            get { return this._mwsAuthToken; }
            set { this._mwsAuthToken = value; }
        }

        /// <summary>
        /// Sets the MWSAuthToken property.
        /// </summary>
        /// <param name="mwsAuthToken">MWSAuthToken property.</param>
        /// <returns>this instance.</returns>
        public GetMyPriceForASINRequest WithMWSAuthToken(string mwsAuthToken)
        {
            this._mwsAuthToken = mwsAuthToken;
            return this;
        }

        /// <summary>
        /// Checks if MWSAuthToken property is set.
        /// </summary>
        /// <returns>true if MWSAuthToken property is set.</returns>
        public bool IsSetMWSAuthToken()
        {
            return this._mwsAuthToken != null;
        }

        /// <summary>
        /// Gets and sets the MarketplaceId property.
        /// </summary>
        [XmlElement(ElementName = "MarketplaceId")]
        public string MarketplaceId
        {
            get { return this._marketplaceId; }
            set { this._marketplaceId = value; }
        }

        /// <summary>
        /// Sets the MarketplaceId property.
        /// </summary>
        /// <param name="marketplaceId">MarketplaceId property.</param>
        /// <returns>this instance.</returns>
        public GetMyPriceForASINRequest WithMarketplaceId(string marketplaceId)
        {
            this._marketplaceId = marketplaceId;
            return this;
        }

        /// <summary>
        /// Checks if MarketplaceId property is set.
        /// </summary>
        /// <returns>true if MarketplaceId property is set.</returns>
        public bool IsSetMarketplaceId()
        {
            return this._marketplaceId != null;
        }

        /// <summary>
        /// Gets and sets the ASINList property.
        /// </summary>
        [XmlElement(ElementName = "ASINList")]
        public ASINListType ASINList
        {
            get { return this._asinList; }
            set { this._asinList = value; }
        }

        /// <summary>
        /// Sets the ASINList property.
        /// </summary>
        /// <param name="asinList">ASINList property.</param>
        /// <returns>this instance.</returns>
        public GetMyPriceForASINRequest WithASINList(ASINListType asinList)
        {
            this._asinList = asinList;
            return this;
        }

        /// <summary>
        /// Checks if ASINList property is set.
        /// </summary>
        /// <returns>true if ASINList property is set.</returns>
        public bool IsSetASINList()
        {
            return this._asinList != null;
        }


        public override void ReadFragmentFrom(IMwsReader reader)
        {
            this._sellerId = reader.Read<string>("SellerId");
            this._mwsAuthToken = reader.Read<string>("MWSAuthToken");
            this._marketplaceId = reader.Read<string>("MarketplaceId");
            this._asinList = reader.Read<ASINListType>("ASINList");
        }

        public override void WriteFragmentTo(IMwsWriter writer)
        {
            writer.Write("SellerId", this._sellerId);
            writer.Write("MWSAuthToken", this._mwsAuthToken);
            writer.Write("MarketplaceId", this._marketplaceId);
            writer.Write("ASINList", this._asinList);
        }

        public override void WriteTo(IMwsWriter writer)
        {
            writer.Write("http://mws.amazonservices.com/schema/Products/2011-10-01", "GetMyPriceForASINRequest", this);
        }

        public GetMyPriceForASINRequest() : base()
        {
        }
    }
}
