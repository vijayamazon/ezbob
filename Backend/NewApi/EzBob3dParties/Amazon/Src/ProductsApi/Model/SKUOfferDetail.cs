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
 * SKU Offer Detail
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
    public class SKUOfferDetail : AbstractMwsObject
    {

        private bool _myOffer;
        private string _subCondition;
        private SellerFeedbackType _sellerFeedbackRating;
        private DetailedShippingTimeType _shippingTime;
        private MoneyType _listingPrice;
        private Points _points;
        private MoneyType _shipping;
        private ShipsFromType _shipsFrom;
        private bool _isFulfilledByAmazon;
        private bool? _isBuyBoxWinner;
        private bool? _isFeaturedMerchant;

        /// <summary>
        /// Gets and sets the MyOffer property.
        /// </summary>
        [XmlElement(ElementName = "MyOffer")]
        public bool MyOffer
        {
            get { return this._myOffer; }
            set { this._myOffer = value; }
        }

        /// <summary>
        /// Sets the MyOffer property.
        /// </summary>
        /// <param name="myOffer">MyOffer property.</param>
        /// <returns>this instance.</returns>
        public SKUOfferDetail WithMyOffer(bool myOffer)
        {
            this._myOffer = myOffer;
            return this;
        }

        /// <summary>
        /// Checks if MyOffer property is set.
        /// </summary>
        /// <returns>true if MyOffer property is set.</returns>
        public bool IsSetMyOffer()
        {
            return this._myOffer != null;
        }

        /// <summary>
        /// Gets and sets the SubCondition property.
        /// </summary>
        [XmlElement(ElementName = "SubCondition")]
        public string SubCondition
        {
            get { return this._subCondition; }
            set { this._subCondition = value; }
        }

        /// <summary>
        /// Sets the SubCondition property.
        /// </summary>
        /// <param name="subCondition">SubCondition property.</param>
        /// <returns>this instance.</returns>
        public SKUOfferDetail WithSubCondition(string subCondition)
        {
            this._subCondition = subCondition;
            return this;
        }

        /// <summary>
        /// Checks if SubCondition property is set.
        /// </summary>
        /// <returns>true if SubCondition property is set.</returns>
        public bool IsSetSubCondition()
        {
            return this._subCondition != null;
        }

        /// <summary>
        /// Gets and sets the SellerFeedbackRating property.
        /// </summary>
        [XmlElement(ElementName = "SellerFeedbackRating")]
        public SellerFeedbackType SellerFeedbackRating
        {
            get { return this._sellerFeedbackRating; }
            set { this._sellerFeedbackRating = value; }
        }

        /// <summary>
        /// Sets the SellerFeedbackRating property.
        /// </summary>
        /// <param name="sellerFeedbackRating">SellerFeedbackRating property.</param>
        /// <returns>this instance.</returns>
        public SKUOfferDetail WithSellerFeedbackRating(SellerFeedbackType sellerFeedbackRating)
        {
            this._sellerFeedbackRating = sellerFeedbackRating;
            return this;
        }

        /// <summary>
        /// Checks if SellerFeedbackRating property is set.
        /// </summary>
        /// <returns>true if SellerFeedbackRating property is set.</returns>
        public bool IsSetSellerFeedbackRating()
        {
            return this._sellerFeedbackRating != null;
        }

        /// <summary>
        /// Gets and sets the ShippingTime property.
        /// </summary>
        [XmlElement(ElementName = "ShippingTime")]
        public DetailedShippingTimeType ShippingTime
        {
            get { return this._shippingTime; }
            set { this._shippingTime = value; }
        }

        /// <summary>
        /// Sets the ShippingTime property.
        /// </summary>
        /// <param name="shippingTime">ShippingTime property.</param>
        /// <returns>this instance.</returns>
        public SKUOfferDetail WithShippingTime(DetailedShippingTimeType shippingTime)
        {
            this._shippingTime = shippingTime;
            return this;
        }

        /// <summary>
        /// Checks if ShippingTime property is set.
        /// </summary>
        /// <returns>true if ShippingTime property is set.</returns>
        public bool IsSetShippingTime()
        {
            return this._shippingTime != null;
        }

        /// <summary>
        /// Gets and sets the ListingPrice property.
        /// </summary>
        [XmlElement(ElementName = "ListingPrice")]
        public MoneyType ListingPrice
        {
            get { return this._listingPrice; }
            set { this._listingPrice = value; }
        }

        /// <summary>
        /// Sets the ListingPrice property.
        /// </summary>
        /// <param name="listingPrice">ListingPrice property.</param>
        /// <returns>this instance.</returns>
        public SKUOfferDetail WithListingPrice(MoneyType listingPrice)
        {
            this._listingPrice = listingPrice;
            return this;
        }

        /// <summary>
        /// Checks if ListingPrice property is set.
        /// </summary>
        /// <returns>true if ListingPrice property is set.</returns>
        public bool IsSetListingPrice()
        {
            return this._listingPrice != null;
        }

        /// <summary>
        /// Gets and sets the Points property.
        /// </summary>
        [XmlElement(ElementName = "Points")]
        public Points Points
        {
            get { return this._points; }
            set { this._points = value; }
        }

        /// <summary>
        /// Sets the Points property.
        /// </summary>
        /// <param name="points">Points property.</param>
        /// <returns>this instance.</returns>
        public SKUOfferDetail WithPoints(Points points)
        {
            this._points = points;
            return this;
        }

        /// <summary>
        /// Checks if Points property is set.
        /// </summary>
        /// <returns>true if Points property is set.</returns>
        public bool IsSetPoints()
        {
            return this._points != null;
        }

        /// <summary>
        /// Gets and sets the Shipping property.
        /// </summary>
        [XmlElement(ElementName = "Shipping")]
        public MoneyType Shipping
        {
            get { return this._shipping; }
            set { this._shipping = value; }
        }

        /// <summary>
        /// Sets the Shipping property.
        /// </summary>
        /// <param name="shipping">Shipping property.</param>
        /// <returns>this instance.</returns>
        public SKUOfferDetail WithShipping(MoneyType shipping)
        {
            this._shipping = shipping;
            return this;
        }

        /// <summary>
        /// Checks if Shipping property is set.
        /// </summary>
        /// <returns>true if Shipping property is set.</returns>
        public bool IsSetShipping()
        {
            return this._shipping != null;
        }

        /// <summary>
        /// Gets and sets the ShipsFrom property.
        /// </summary>
        [XmlElement(ElementName = "ShipsFrom")]
        public ShipsFromType ShipsFrom
        {
            get { return this._shipsFrom; }
            set { this._shipsFrom = value; }
        }

        /// <summary>
        /// Sets the ShipsFrom property.
        /// </summary>
        /// <param name="shipsFrom">ShipsFrom property.</param>
        /// <returns>this instance.</returns>
        public SKUOfferDetail WithShipsFrom(ShipsFromType shipsFrom)
        {
            this._shipsFrom = shipsFrom;
            return this;
        }

        /// <summary>
        /// Checks if ShipsFrom property is set.
        /// </summary>
        /// <returns>true if ShipsFrom property is set.</returns>
        public bool IsSetShipsFrom()
        {
            return this._shipsFrom != null;
        }

        /// <summary>
        /// Gets and sets the IsFulfilledByAmazon property.
        /// </summary>
        [XmlElement(ElementName = "IsFulfilledByAmazon")]
        public bool IsFulfilledByAmazon
        {
            get { return this._isFulfilledByAmazon; }
            set { this._isFulfilledByAmazon = value; }
        }

        /// <summary>
        /// Sets the IsFulfilledByAmazon property.
        /// </summary>
        /// <param name="isFulfilledByAmazon">IsFulfilledByAmazon property.</param>
        /// <returns>this instance.</returns>
        public SKUOfferDetail WithIsFulfilledByAmazon(bool isFulfilledByAmazon)
        {
            this._isFulfilledByAmazon = isFulfilledByAmazon;
            return this;
        }

        /// <summary>
        /// Checks if IsFulfilledByAmazon property is set.
        /// </summary>
        /// <returns>true if IsFulfilledByAmazon property is set.</returns>
        public bool IsSetIsFulfilledByAmazon()
        {
            return this._isFulfilledByAmazon != null;
        }

        /// <summary>
        /// Gets and sets the IsBuyBoxWinner property.
        /// </summary>
        [XmlElement(ElementName = "IsBuyBoxWinner")]
        public bool IsBuyBoxWinner
        {
            get { return this._isBuyBoxWinner.GetValueOrDefault(); }
            set { this._isBuyBoxWinner = value; }
        }

        /// <summary>
        /// Sets the IsBuyBoxWinner property.
        /// </summary>
        /// <param name="isBuyBoxWinner">IsBuyBoxWinner property.</param>
        /// <returns>this instance.</returns>
        public SKUOfferDetail WithIsBuyBoxWinner(bool isBuyBoxWinner)
        {
            this._isBuyBoxWinner = isBuyBoxWinner;
            return this;
        }

        /// <summary>
        /// Checks if IsBuyBoxWinner property is set.
        /// </summary>
        /// <returns>true if IsBuyBoxWinner property is set.</returns>
        public bool IsSetIsBuyBoxWinner()
        {
            return this._isBuyBoxWinner != null;
        }

        /// <summary>
        /// Gets and sets the IsFeaturedMerchant property.
        /// </summary>
        [XmlElement(ElementName = "IsFeaturedMerchant")]
        public bool IsFeaturedMerchant
        {
            get { return this._isFeaturedMerchant.GetValueOrDefault(); }
            set { this._isFeaturedMerchant = value; }
        }

        /// <summary>
        /// Sets the IsFeaturedMerchant property.
        /// </summary>
        /// <param name="isFeaturedMerchant">IsFeaturedMerchant property.</param>
        /// <returns>this instance.</returns>
        public SKUOfferDetail WithIsFeaturedMerchant(bool isFeaturedMerchant)
        {
            this._isFeaturedMerchant = isFeaturedMerchant;
            return this;
        }

        /// <summary>
        /// Checks if IsFeaturedMerchant property is set.
        /// </summary>
        /// <returns>true if IsFeaturedMerchant property is set.</returns>
        public bool IsSetIsFeaturedMerchant()
        {
            return this._isFeaturedMerchant != null;
        }


        public override void ReadFragmentFrom(IMwsReader reader)
        {
            this._myOffer = reader.Read<bool>("MyOffer");
            this._subCondition = reader.Read<string>("SubCondition");
            this._sellerFeedbackRating = reader.Read<SellerFeedbackType>("SellerFeedbackRating");
            this._shippingTime = reader.Read<DetailedShippingTimeType>("ShippingTime");
            this._listingPrice = reader.Read<MoneyType>("ListingPrice");
            this._points = reader.Read<Points>("Points");
            this._shipping = reader.Read<MoneyType>("Shipping");
            this._shipsFrom = reader.Read<ShipsFromType>("ShipsFrom");
            this._isFulfilledByAmazon = reader.Read<bool>("IsFulfilledByAmazon");
            this._isBuyBoxWinner = reader.Read<bool?>("IsBuyBoxWinner");
            this._isFeaturedMerchant = reader.Read<bool?>("IsFeaturedMerchant");
        }

        public override void WriteFragmentTo(IMwsWriter writer)
        {
            writer.Write("MyOffer", this._myOffer);
            writer.Write("SubCondition", this._subCondition);
            writer.Write("SellerFeedbackRating", this._sellerFeedbackRating);
            writer.Write("ShippingTime", this._shippingTime);
            writer.Write("ListingPrice", this._listingPrice);
            writer.Write("Points", this._points);
            writer.Write("Shipping", this._shipping);
            writer.Write("ShipsFrom", this._shipsFrom);
            writer.Write("IsFulfilledByAmazon", this._isFulfilledByAmazon);
            writer.Write("IsBuyBoxWinner", this._isBuyBoxWinner);
            writer.Write("IsFeaturedMerchant", this._isFeaturedMerchant);
        }

        public override void WriteTo(IMwsWriter writer)
        {
            writer.Write("http://mws.amazonservices.com/schema/Products/2011-10-01", "SKUOfferDetail", this);
        }

        public SKUOfferDetail() : base()
        {
        }
    }
}
