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
 * Qualifiers Type
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
    public class QualifiersType : AbstractMwsObject
    {

        private string _itemCondition;
        private string _itemSubcondition;
        private string _fulfillmentChannel;
        private string _shipsDomestically;
        private ShippingTimeType _shippingTime;
        private string _sellerPositiveFeedbackRating;

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
        public QualifiersType WithItemCondition(string itemCondition)
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
        /// Gets and sets the ItemSubcondition property.
        /// </summary>
        [XmlElement(ElementName = "ItemSubcondition")]
        public string ItemSubcondition
        {
            get { return this._itemSubcondition; }
            set { this._itemSubcondition = value; }
        }

        /// <summary>
        /// Sets the ItemSubcondition property.
        /// </summary>
        /// <param name="itemSubcondition">ItemSubcondition property.</param>
        /// <returns>this instance.</returns>
        public QualifiersType WithItemSubcondition(string itemSubcondition)
        {
            this._itemSubcondition = itemSubcondition;
            return this;
        }

        /// <summary>
        /// Checks if ItemSubcondition property is set.
        /// </summary>
        /// <returns>true if ItemSubcondition property is set.</returns>
        public bool IsSetItemSubcondition()
        {
            return this._itemSubcondition != null;
        }

        /// <summary>
        /// Gets and sets the FulfillmentChannel property.
        /// </summary>
        [XmlElement(ElementName = "FulfillmentChannel")]
        public string FulfillmentChannel
        {
            get { return this._fulfillmentChannel; }
            set { this._fulfillmentChannel = value; }
        }

        /// <summary>
        /// Sets the FulfillmentChannel property.
        /// </summary>
        /// <param name="fulfillmentChannel">FulfillmentChannel property.</param>
        /// <returns>this instance.</returns>
        public QualifiersType WithFulfillmentChannel(string fulfillmentChannel)
        {
            this._fulfillmentChannel = fulfillmentChannel;
            return this;
        }

        /// <summary>
        /// Checks if FulfillmentChannel property is set.
        /// </summary>
        /// <returns>true if FulfillmentChannel property is set.</returns>
        public bool IsSetFulfillmentChannel()
        {
            return this._fulfillmentChannel != null;
        }

        /// <summary>
        /// Gets and sets the ShipsDomestically property.
        /// </summary>
        [XmlElement(ElementName = "ShipsDomestically")]
        public string ShipsDomestically
        {
            get { return this._shipsDomestically; }
            set { this._shipsDomestically = value; }
        }

        /// <summary>
        /// Sets the ShipsDomestically property.
        /// </summary>
        /// <param name="shipsDomestically">ShipsDomestically property.</param>
        /// <returns>this instance.</returns>
        public QualifiersType WithShipsDomestically(string shipsDomestically)
        {
            this._shipsDomestically = shipsDomestically;
            return this;
        }

        /// <summary>
        /// Checks if ShipsDomestically property is set.
        /// </summary>
        /// <returns>true if ShipsDomestically property is set.</returns>
        public bool IsSetShipsDomestically()
        {
            return this._shipsDomestically != null;
        }

        /// <summary>
        /// Gets and sets the ShippingTime property.
        /// </summary>
        [XmlElement(ElementName = "ShippingTime")]
        public ShippingTimeType ShippingTime
        {
            get { return this._shippingTime; }
            set { this._shippingTime = value; }
        }

        /// <summary>
        /// Sets the ShippingTime property.
        /// </summary>
        /// <param name="shippingTime">ShippingTime property.</param>
        /// <returns>this instance.</returns>
        public QualifiersType WithShippingTime(ShippingTimeType shippingTime)
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
        /// Gets and sets the SellerPositiveFeedbackRating property.
        /// </summary>
        [XmlElement(ElementName = "SellerPositiveFeedbackRating")]
        public string SellerPositiveFeedbackRating
        {
            get { return this._sellerPositiveFeedbackRating; }
            set { this._sellerPositiveFeedbackRating = value; }
        }

        /// <summary>
        /// Sets the SellerPositiveFeedbackRating property.
        /// </summary>
        /// <param name="sellerPositiveFeedbackRating">SellerPositiveFeedbackRating property.</param>
        /// <returns>this instance.</returns>
        public QualifiersType WithSellerPositiveFeedbackRating(string sellerPositiveFeedbackRating)
        {
            this._sellerPositiveFeedbackRating = sellerPositiveFeedbackRating;
            return this;
        }

        /// <summary>
        /// Checks if SellerPositiveFeedbackRating property is set.
        /// </summary>
        /// <returns>true if SellerPositiveFeedbackRating property is set.</returns>
        public bool IsSetSellerPositiveFeedbackRating()
        {
            return this._sellerPositiveFeedbackRating != null;
        }


        public override void ReadFragmentFrom(IMwsReader reader)
        {
            this._itemCondition = reader.Read<string>("ItemCondition");
            this._itemSubcondition = reader.Read<string>("ItemSubcondition");
            this._fulfillmentChannel = reader.Read<string>("FulfillmentChannel");
            this._shipsDomestically = reader.Read<string>("ShipsDomestically");
            this._shippingTime = reader.Read<ShippingTimeType>("ShippingTime");
            this._sellerPositiveFeedbackRating = reader.Read<string>("SellerPositiveFeedbackRating");
        }

        public override void WriteFragmentTo(IMwsWriter writer)
        {
            writer.Write("ItemCondition", this._itemCondition);
            writer.Write("ItemSubcondition", this._itemSubcondition);
            writer.Write("FulfillmentChannel", this._fulfillmentChannel);
            writer.Write("ShipsDomestically", this._shipsDomestically);
            writer.Write("ShippingTime", this._shippingTime);
            writer.Write("SellerPositiveFeedbackRating", this._sellerPositiveFeedbackRating);
        }

        public override void WriteTo(IMwsWriter writer)
        {
            writer.Write("http://mws.amazonservices.com/schema/Products/2011-10-01", "QualifiersType", this);
        }

        public QualifiersType() : base()
        {
        }
    }
}
