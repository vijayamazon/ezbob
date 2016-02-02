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
 * Get Lowest Offer Listings For SKU Request
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
    public class GetLowestOfferListingsForSKURequest : AbstractMwsObject
    {

        private string _sellerId;
        private string _mwsAuthToken;
        private string _marketplaceId;
        private SellerSKUListType _sellerSKUList;
        private string _itemCondition;
        private bool? _excludeMe;

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
        public GetLowestOfferListingsForSKURequest WithSellerId(string sellerId)
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
        public GetLowestOfferListingsForSKURequest WithMWSAuthToken(string mwsAuthToken)
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
        public GetLowestOfferListingsForSKURequest WithMarketplaceId(string marketplaceId)
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
        /// Gets and sets the SellerSKUList property.
        /// </summary>
        [XmlElement(ElementName = "SellerSKUList")]
        public SellerSKUListType SellerSKUList
        {
            get { return this._sellerSKUList; }
            set { this._sellerSKUList = value; }
        }

        /// <summary>
        /// Sets the SellerSKUList property.
        /// </summary>
        /// <param name="sellerSKUList">SellerSKUList property.</param>
        /// <returns>this instance.</returns>
        public GetLowestOfferListingsForSKURequest WithSellerSKUList(SellerSKUListType sellerSKUList)
        {
            this._sellerSKUList = sellerSKUList;
            return this;
        }

        /// <summary>
        /// Checks if SellerSKUList property is set.
        /// </summary>
        /// <returns>true if SellerSKUList property is set.</returns>
        public bool IsSetSellerSKUList()
        {
            return this._sellerSKUList != null;
        }

        /// <summary>
        /// Gets and sets the ItemCondition property.
        /// </summary>
        [XmlElement(ElementName = "ItemCondition")]
        public string ItemCondition
        {
            get { return this._itemCondition; }
            set { this._itemCondition = value; }
        }

        /// <summary>
        /// Sets the ItemCondition property.
        /// </summary>
        /// <param name="itemCondition">ItemCondition property.</param>
        /// <returns>this instance.</returns>
        public GetLowestOfferListingsForSKURequest WithItemCondition(string itemCondition)
        {
            this._itemCondition = itemCondition;
            return this;
        }

        /// <summary>
        /// Checks if ItemCondition property is set.
        /// </summary>
        /// <returns>true if ItemCondition property is set.</returns>
        public bool IsSetItemCondition()
        {
            return this._itemCondition != null;
        }

        /// <summary>
        /// Gets and sets the ExcludeMe property.
        /// </summary>
        [XmlElement(ElementName = "ExcludeMe")]
        public bool ExcludeMe
        {
            get { return this._excludeMe.GetValueOrDefault(); }
            set { this._excludeMe = value; }
        }

        /// <summary>
        /// Sets the ExcludeMe property.
        /// </summary>
        /// <param name="excludeMe">ExcludeMe property.</param>
        /// <returns>this instance.</returns>
        public GetLowestOfferListingsForSKURequest WithExcludeMe(bool excludeMe)
        {
            this._excludeMe = excludeMe;
            return this;
        }

        /// <summary>
        /// Checks if ExcludeMe property is set.
        /// </summary>
        /// <returns>true if ExcludeMe property is set.</returns>
        public bool IsSetExcludeMe()
        {
            return this._excludeMe != null;
        }


        public override void ReadFragmentFrom(IMwsReader reader)
        {
            this._sellerId = reader.Read<string>("SellerId");
            this._mwsAuthToken = reader.Read<string>("MWSAuthToken");
            this._marketplaceId = reader.Read<string>("MarketplaceId");
            this._sellerSKUList = reader.Read<SellerSKUListType>("SellerSKUList");
            this._itemCondition = reader.Read<string>("ItemCondition");
            this._excludeMe = reader.Read<bool?>("ExcludeMe");
        }

        public override void WriteFragmentTo(IMwsWriter writer)
        {
            writer.Write("SellerId", this._sellerId);
            writer.Write("MWSAuthToken", this._mwsAuthToken);
            writer.Write("MarketplaceId", this._marketplaceId);
            writer.Write("SellerSKUList", this._sellerSKUList);
            writer.Write("ItemCondition", this._itemCondition);
            writer.Write("ExcludeMe", this._excludeMe);
        }

        public override void WriteTo(IMwsWriter writer)
        {
            writer.Write("http://mws.amazonservices.com/schema/Products/2011-10-01", "GetLowestOfferListingsForSKURequest", this);
        }

        public GetLowestOfferListingsForSKURequest() : base()
        {
        }
    }
}
