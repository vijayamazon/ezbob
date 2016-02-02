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
 * Summary
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
    public class Summary : AbstractMwsObject
    {

        private decimal _totalOfferCount;
        private NumberOfOffers _numberOfOffers;
        private LowestPrices _lowestPrices;
        private BuyBoxPrices _buyBoxPrices;
        private MoneyType _listPrice;
        private MoneyType _suggestedLowerPricePlusShipping;
        private BuyBoxEligibleOffers _buyBoxEligibleOffers;

        /// <summary>
        /// Gets and sets the TotalOfferCount property.
        /// </summary>
        [XmlElement(ElementName = "TotalOfferCount")]
        public decimal TotalOfferCount
        {
            get { return this._totalOfferCount; }
            set { this._totalOfferCount = value; }
        }

        /// <summary>
        /// Sets the TotalOfferCount property.
        /// </summary>
        /// <param name="totalOfferCount">TotalOfferCount property.</param>
        /// <returns>this instance.</returns>
        public Summary WithTotalOfferCount(decimal totalOfferCount)
        {
            this._totalOfferCount = totalOfferCount;
            return this;
        }

        /// <summary>
        /// Checks if TotalOfferCount property is set.
        /// </summary>
        /// <returns>true if TotalOfferCount property is set.</returns>
        public bool IsSetTotalOfferCount()
        {
            return this._totalOfferCount != null;
        }

        /// <summary>
        /// Gets and sets the NumberOfOffers property.
        /// </summary>
        [XmlElement(ElementName = "NumberOfOffers")]
        public NumberOfOffers NumberOfOffers
        {
            get { return this._numberOfOffers; }
            set { this._numberOfOffers = value; }
        }

        /// <summary>
        /// Sets the NumberOfOffers property.
        /// </summary>
        /// <param name="numberOfOffers">NumberOfOffers property.</param>
        /// <returns>this instance.</returns>
        public Summary WithNumberOfOffers(NumberOfOffers numberOfOffers)
        {
            this._numberOfOffers = numberOfOffers;
            return this;
        }

        /// <summary>
        /// Checks if NumberOfOffers property is set.
        /// </summary>
        /// <returns>true if NumberOfOffers property is set.</returns>
        public bool IsSetNumberOfOffers()
        {
            return this._numberOfOffers != null;
        }

        /// <summary>
        /// Gets and sets the LowestPrices property.
        /// </summary>
        [XmlElement(ElementName = "LowestPrices")]
        public LowestPrices LowestPrices
        {
            get { return this._lowestPrices; }
            set { this._lowestPrices = value; }
        }

        /// <summary>
        /// Sets the LowestPrices property.
        /// </summary>
        /// <param name="lowestPrices">LowestPrices property.</param>
        /// <returns>this instance.</returns>
        public Summary WithLowestPrices(LowestPrices lowestPrices)
        {
            this._lowestPrices = lowestPrices;
            return this;
        }

        /// <summary>
        /// Checks if LowestPrices property is set.
        /// </summary>
        /// <returns>true if LowestPrices property is set.</returns>
        public bool IsSetLowestPrices()
        {
            return this._lowestPrices != null;
        }

        /// <summary>
        /// Gets and sets the BuyBoxPrices property.
        /// </summary>
        [XmlElement(ElementName = "BuyBoxPrices")]
        public BuyBoxPrices BuyBoxPrices
        {
            get { return this._buyBoxPrices; }
            set { this._buyBoxPrices = value; }
        }

        /// <summary>
        /// Sets the BuyBoxPrices property.
        /// </summary>
        /// <param name="buyBoxPrices">BuyBoxPrices property.</param>
        /// <returns>this instance.</returns>
        public Summary WithBuyBoxPrices(BuyBoxPrices buyBoxPrices)
        {
            this._buyBoxPrices = buyBoxPrices;
            return this;
        }

        /// <summary>
        /// Checks if BuyBoxPrices property is set.
        /// </summary>
        /// <returns>true if BuyBoxPrices property is set.</returns>
        public bool IsSetBuyBoxPrices()
        {
            return this._buyBoxPrices != null;
        }

        /// <summary>
        /// Gets and sets the ListPrice property.
        /// </summary>
        [XmlElement(ElementName = "ListPrice")]
        public MoneyType ListPrice
        {
            get { return this._listPrice; }
            set { this._listPrice = value; }
        }

        /// <summary>
        /// Sets the ListPrice property.
        /// </summary>
        /// <param name="listPrice">ListPrice property.</param>
        /// <returns>this instance.</returns>
        public Summary WithListPrice(MoneyType listPrice)
        {
            this._listPrice = listPrice;
            return this;
        }

        /// <summary>
        /// Checks if ListPrice property is set.
        /// </summary>
        /// <returns>true if ListPrice property is set.</returns>
        public bool IsSetListPrice()
        {
            return this._listPrice != null;
        }

        /// <summary>
        /// Gets and sets the SuggestedLowerPricePlusShipping property.
        /// </summary>
        [XmlElement(ElementName = "SuggestedLowerPricePlusShipping")]
        public MoneyType SuggestedLowerPricePlusShipping
        {
            get { return this._suggestedLowerPricePlusShipping; }
            set { this._suggestedLowerPricePlusShipping = value; }
        }

        /// <summary>
        /// Sets the SuggestedLowerPricePlusShipping property.
        /// </summary>
        /// <param name="suggestedLowerPricePlusShipping">SuggestedLowerPricePlusShipping property.</param>
        /// <returns>this instance.</returns>
        public Summary WithSuggestedLowerPricePlusShipping(MoneyType suggestedLowerPricePlusShipping)
        {
            this._suggestedLowerPricePlusShipping = suggestedLowerPricePlusShipping;
            return this;
        }

        /// <summary>
        /// Checks if SuggestedLowerPricePlusShipping property is set.
        /// </summary>
        /// <returns>true if SuggestedLowerPricePlusShipping property is set.</returns>
        public bool IsSetSuggestedLowerPricePlusShipping()
        {
            return this._suggestedLowerPricePlusShipping != null;
        }

        /// <summary>
        /// Gets and sets the BuyBoxEligibleOffers property.
        /// </summary>
        [XmlElement(ElementName = "BuyBoxEligibleOffers")]
        public BuyBoxEligibleOffers BuyBoxEligibleOffers
        {
            get { return this._buyBoxEligibleOffers; }
            set { this._buyBoxEligibleOffers = value; }
        }

        /// <summary>
        /// Sets the BuyBoxEligibleOffers property.
        /// </summary>
        /// <param name="buyBoxEligibleOffers">BuyBoxEligibleOffers property.</param>
        /// <returns>this instance.</returns>
        public Summary WithBuyBoxEligibleOffers(BuyBoxEligibleOffers buyBoxEligibleOffers)
        {
            this._buyBoxEligibleOffers = buyBoxEligibleOffers;
            return this;
        }

        /// <summary>
        /// Checks if BuyBoxEligibleOffers property is set.
        /// </summary>
        /// <returns>true if BuyBoxEligibleOffers property is set.</returns>
        public bool IsSetBuyBoxEligibleOffers()
        {
            return this._buyBoxEligibleOffers != null;
        }


        public override void ReadFragmentFrom(IMwsReader reader)
        {
            this._totalOfferCount = reader.Read<decimal>("TotalOfferCount");
            this._numberOfOffers = reader.Read<NumberOfOffers>("NumberOfOffers");
            this._lowestPrices = reader.Read<LowestPrices>("LowestPrices");
            this._buyBoxPrices = reader.Read<BuyBoxPrices>("BuyBoxPrices");
            this._listPrice = reader.Read<MoneyType>("ListPrice");
            this._suggestedLowerPricePlusShipping = reader.Read<MoneyType>("SuggestedLowerPricePlusShipping");
            this._buyBoxEligibleOffers = reader.Read<BuyBoxEligibleOffers>("BuyBoxEligibleOffers");
        }

        public override void WriteFragmentTo(IMwsWriter writer)
        {
            writer.Write("TotalOfferCount", this._totalOfferCount);
            writer.Write("NumberOfOffers", this._numberOfOffers);
            writer.Write("LowestPrices", this._lowestPrices);
            writer.Write("BuyBoxPrices", this._buyBoxPrices);
            writer.Write("ListPrice", this._listPrice);
            writer.Write("SuggestedLowerPricePlusShipping", this._suggestedLowerPricePlusShipping);
            writer.Write("BuyBoxEligibleOffers", this._buyBoxEligibleOffers);
        }

        public override void WriteTo(IMwsWriter writer)
        {
            writer.Write("http://mws.amazonservices.com/schema/Products/2011-10-01", "Summary", this);
        }

        public Summary() : base()
        {
        }
    }
}
