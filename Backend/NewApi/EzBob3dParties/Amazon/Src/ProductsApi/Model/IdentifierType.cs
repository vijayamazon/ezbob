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
 * Identifier Type
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
    public class IdentifierType : AbstractMwsObject
    {

        private ASINIdentifier _marketplaceASIN;
        private SellerSKUIdentifier _skuIdentifier;

        /// <summary>
        /// Gets and sets the MarketplaceASIN property.
        /// </summary>
        [XmlElement(ElementName = "MarketplaceASIN")]
        public ASINIdentifier MarketplaceASIN
        {
            get { return this._marketplaceASIN; }
            set { this._marketplaceASIN = value; }
        }

        /// <summary>
        /// Sets the MarketplaceASIN property.
        /// </summary>
        /// <param name="marketplaceASIN">MarketplaceASIN property.</param>
        /// <returns>this instance.</returns>
        public IdentifierType WithMarketplaceASIN(ASINIdentifier marketplaceASIN)
        {
            this._marketplaceASIN = marketplaceASIN;
            return this;
        }

        /// <summary>
        /// Checks if MarketplaceASIN property is set.
        /// </summary>
        /// <returns>true if MarketplaceASIN property is set.</returns>
        public bool IsSetMarketplaceASIN()
        {
            return this._marketplaceASIN != null;
        }

        /// <summary>
        /// Gets and sets the SKUIdentifier property.
        /// </summary>
        [XmlElement(ElementName = "SKUIdentifier")]
        public SellerSKUIdentifier SKUIdentifier
        {
            get { return this._skuIdentifier; }
            set { this._skuIdentifier = value; }
        }

        /// <summary>
        /// Sets the SKUIdentifier property.
        /// </summary>
        /// <param name="skuIdentifier">SKUIdentifier property.</param>
        /// <returns>this instance.</returns>
        public IdentifierType WithSKUIdentifier(SellerSKUIdentifier skuIdentifier)
        {
            this._skuIdentifier = skuIdentifier;
            return this;
        }

        /// <summary>
        /// Checks if SKUIdentifier property is set.
        /// </summary>
        /// <returns>true if SKUIdentifier property is set.</returns>
        public bool IsSetSKUIdentifier()
        {
            return this._skuIdentifier != null;
        }


        public override void ReadFragmentFrom(IMwsReader reader)
        {
            this._marketplaceASIN = reader.Read<ASINIdentifier>("MarketplaceASIN");
            this._skuIdentifier = reader.Read<SellerSKUIdentifier>("SKUIdentifier");
        }

        public override void WriteFragmentTo(IMwsWriter writer)
        {
            writer.Write("MarketplaceASIN", this._marketplaceASIN);
            writer.Write("SKUIdentifier", this._skuIdentifier);
        }

        public override void WriteTo(IMwsWriter writer)
        {
            writer.Write("http://mws.amazonservices.com/schema/Products/2011-10-01", "IdentifierType", this);
        }

        public IdentifierType() : base()
        {
        }
    }
}
