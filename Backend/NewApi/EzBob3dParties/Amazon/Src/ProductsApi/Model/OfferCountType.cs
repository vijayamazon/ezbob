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
 * Offer Count Type
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
    public class OfferCountType : AbstractMwsObject
    {

        private string _condition;
        private string _fulfillmentChannel;
        private decimal? _offerCount;

        /// <summary>
        /// Gets and sets the condition property.
        /// </summary>
        [XmlAttribute(AttributeName = "condition")]
        public string condition
        {
            get { return this._condition; }
            set { this._condition = value; }
        }

        /// <summary>
        /// Sets the condition property.
        /// </summary>
        /// <param name="condition">condition property.</param>
        /// <returns>this instance.</returns>
        public OfferCountType Withcondition(string condition)
        {
            this._condition = condition;
            return this;
        }

        /// <summary>
        /// Checks if condition property is set.
        /// </summary>
        /// <returns>true if condition property is set.</returns>
        public bool IsSetcondition()
        {
            return this._condition != null;
        }

        /// <summary>
        /// Gets and sets the fulfillmentChannel property.
        /// </summary>
        [XmlAttribute(AttributeName = "fulfillmentChannel")]
        public string fulfillmentChannel
        {
            get { return this._fulfillmentChannel; }
            set { this._fulfillmentChannel = value; }
        }

        /// <summary>
        /// Sets the fulfillmentChannel property.
        /// </summary>
        /// <param name="fulfillmentChannel">fulfillmentChannel property.</param>
        /// <returns>this instance.</returns>
        public OfferCountType WithfulfillmentChannel(string fulfillmentChannel)
        {
            this._fulfillmentChannel = fulfillmentChannel;
            return this;
        }

        /// <summary>
        /// Checks if fulfillmentChannel property is set.
        /// </summary>
        /// <returns>true if fulfillmentChannel property is set.</returns>
        public bool IsSetfulfillmentChannel()
        {
            return this._fulfillmentChannel != null;
        }

        /// <summary>
        /// Gets and sets the OfferCount property.
        /// </summary>
        [XmlElement(ElementName = "OfferCount")]
        public decimal OfferCount
        {
            get { return this._offerCount.GetValueOrDefault(); }
            set { this._offerCount = value; }
        }

        /// <summary>
        /// Sets the OfferCount property.
        /// </summary>
        /// <param name="offerCount">OfferCount property.</param>
        /// <returns>this instance.</returns>
        public OfferCountType WithOfferCount(decimal offerCount)
        {
            this._offerCount = offerCount;
            return this;
        }

        /// <summary>
        /// Checks if OfferCount property is set.
        /// </summary>
        /// <returns>true if OfferCount property is set.</returns>
        public bool IsSetOfferCount()
        {
            return this._offerCount != null;
        }


        public override void ReadFragmentFrom(IMwsReader reader)
        {
            this._condition = reader.ReadAttribute<string>("condition");
            this._fulfillmentChannel = reader.ReadAttribute<string>("fulfillmentChannel");
            this._offerCount = reader.ReadValue<decimal?>();
        }

        public override void WriteFragmentTo(IMwsWriter writer)
        {
            writer.WriteAttribute("condition",this._condition);
            writer.WriteAttribute("fulfillmentChannel",this._fulfillmentChannel);
            writer.WriteValue(this._offerCount);
        }

        public override void WriteTo(IMwsWriter writer)
        {
            writer.Write("http://mws.amazonservices.com/schema/Products/2011-10-01", "OfferCountType", this);
        }

        public OfferCountType() : base()
        {
        }
    }
}
