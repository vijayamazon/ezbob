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
 * Product
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
    public class Product : AbstractMwsObject
    {

        private IdentifierType _identifiers;
        private AttributeSetList _attributeSets;
        private RelationshipList _relationships;
        private CompetitivePricingType _competitivePricing;
        private SalesRankList _salesRankings;
        private LowestOfferListingList _lowestOfferListings;
        private OffersList _offers;

        /// <summary>
        /// Gets and sets the Identifiers property.
        /// </summary>
        [XmlElement(ElementName = "Identifiers")]
        public IdentifierType Identifiers
        {
            get { return this._identifiers; }
            set { this._identifiers = value; }
        }

        /// <summary>
        /// Sets the Identifiers property.
        /// </summary>
        /// <param name="identifiers">Identifiers property.</param>
        /// <returns>this instance.</returns>
        public Product WithIdentifiers(IdentifierType identifiers)
        {
            this._identifiers = identifiers;
            return this;
        }

        /// <summary>
        /// Checks if Identifiers property is set.
        /// </summary>
        /// <returns>true if Identifiers property is set.</returns>
        public bool IsSetIdentifiers()
        {
            return this._identifiers != null;
        }

        /// <summary>
        /// Gets and sets the AttributeSets property.
        /// </summary>
        [XmlElement(ElementName = "AttributeSets")]
        public AttributeSetList AttributeSets
        {
            get { return this._attributeSets; }
            set { this._attributeSets = value; }
        }

        /// <summary>
        /// Sets the AttributeSets property.
        /// </summary>
        /// <param name="attributeSets">AttributeSets property.</param>
        /// <returns>this instance.</returns>
        public Product WithAttributeSets(AttributeSetList attributeSets)
        {
            this._attributeSets = attributeSets;
            return this;
        }

        /// <summary>
        /// Checks if AttributeSets property is set.
        /// </summary>
        /// <returns>true if AttributeSets property is set.</returns>
        public bool IsSetAttributeSets()
        {
            return this._attributeSets != null;
        }

        /// <summary>
        /// Gets and sets the Relationships property.
        /// </summary>
        [XmlElement(ElementName = "Relationships")]
        public RelationshipList Relationships
        {
            get { return this._relationships; }
            set { this._relationships = value; }
        }

        /// <summary>
        /// Sets the Relationships property.
        /// </summary>
        /// <param name="relationships">Relationships property.</param>
        /// <returns>this instance.</returns>
        public Product WithRelationships(RelationshipList relationships)
        {
            this._relationships = relationships;
            return this;
        }

        /// <summary>
        /// Checks if Relationships property is set.
        /// </summary>
        /// <returns>true if Relationships property is set.</returns>
        public bool IsSetRelationships()
        {
            return this._relationships != null;
        }

        /// <summary>
        /// Gets and sets the CompetitivePricing property.
        /// </summary>
        [XmlElement(ElementName = "CompetitivePricing")]
        public CompetitivePricingType CompetitivePricing
        {
            get { return this._competitivePricing; }
            set { this._competitivePricing = value; }
        }

        /// <summary>
        /// Sets the CompetitivePricing property.
        /// </summary>
        /// <param name="competitivePricing">CompetitivePricing property.</param>
        /// <returns>this instance.</returns>
        public Product WithCompetitivePricing(CompetitivePricingType competitivePricing)
        {
            this._competitivePricing = competitivePricing;
            return this;
        }

        /// <summary>
        /// Checks if CompetitivePricing property is set.
        /// </summary>
        /// <returns>true if CompetitivePricing property is set.</returns>
        public bool IsSetCompetitivePricing()
        {
            return this._competitivePricing != null;
        }

        /// <summary>
        /// Gets and sets the SalesRankings property.
        /// </summary>
        [XmlElement(ElementName = "SalesRankings")]
        public SalesRankList SalesRankings
        {
            get { return this._salesRankings; }
            set { this._salesRankings = value; }
        }

        /// <summary>
        /// Sets the SalesRankings property.
        /// </summary>
        /// <param name="salesRankings">SalesRankings property.</param>
        /// <returns>this instance.</returns>
        public Product WithSalesRankings(SalesRankList salesRankings)
        {
            this._salesRankings = salesRankings;
            return this;
        }

        /// <summary>
        /// Checks if SalesRankings property is set.
        /// </summary>
        /// <returns>true if SalesRankings property is set.</returns>
        public bool IsSetSalesRankings()
        {
            return this._salesRankings != null;
        }

        /// <summary>
        /// Gets and sets the LowestOfferListings property.
        /// </summary>
        [XmlElement(ElementName = "LowestOfferListings")]
        public LowestOfferListingList LowestOfferListings
        {
            get { return this._lowestOfferListings; }
            set { this._lowestOfferListings = value; }
        }

        /// <summary>
        /// Sets the LowestOfferListings property.
        /// </summary>
        /// <param name="lowestOfferListings">LowestOfferListings property.</param>
        /// <returns>this instance.</returns>
        public Product WithLowestOfferListings(LowestOfferListingList lowestOfferListings)
        {
            this._lowestOfferListings = lowestOfferListings;
            return this;
        }

        /// <summary>
        /// Checks if LowestOfferListings property is set.
        /// </summary>
        /// <returns>true if LowestOfferListings property is set.</returns>
        public bool IsSetLowestOfferListings()
        {
            return this._lowestOfferListings != null;
        }

        /// <summary>
        /// Gets and sets the Offers property.
        /// </summary>
        [XmlElement(ElementName = "Offers")]
        public OffersList Offers
        {
            get { return this._offers; }
            set { this._offers = value; }
        }

        /// <summary>
        /// Sets the Offers property.
        /// </summary>
        /// <param name="offers">Offers property.</param>
        /// <returns>this instance.</returns>
        public Product WithOffers(OffersList offers)
        {
            this._offers = offers;
            return this;
        }

        /// <summary>
        /// Checks if Offers property is set.
        /// </summary>
        /// <returns>true if Offers property is set.</returns>
        public bool IsSetOffers()
        {
            return this._offers != null;
        }


        public override void ReadFragmentFrom(IMwsReader reader)
        {
            this._identifiers = reader.Read<IdentifierType>("Identifiers");
            this._attributeSets = reader.Read<AttributeSetList>("AttributeSets");
            this._relationships = reader.Read<RelationshipList>("Relationships");
            this._competitivePricing = reader.Read<CompetitivePricingType>("CompetitivePricing");
            this._salesRankings = reader.Read<SalesRankList>("SalesRankings");
            this._lowestOfferListings = reader.Read<LowestOfferListingList>("LowestOfferListings");
            this._offers = reader.Read<OffersList>("Offers");
        }

        public override void WriteFragmentTo(IMwsWriter writer)
        {
            writer.Write("Identifiers", this._identifiers);
            writer.Write("AttributeSets", this._attributeSets);
            writer.Write("Relationships", this._relationships);
            writer.Write("CompetitivePricing", this._competitivePricing);
            writer.Write("SalesRankings", this._salesRankings);
            writer.Write("LowestOfferListings", this._lowestOfferListings);
            writer.Write("Offers", this._offers);
        }

        public override void WriteTo(IMwsWriter writer)
        {
            writer.Write("http://mws.amazonservices.com/schema/Products/2011-10-01", "Product", this);
        }

        public Product() : base()
        {
        }
    }
}
