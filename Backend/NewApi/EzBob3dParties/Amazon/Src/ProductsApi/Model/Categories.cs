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
 * Categories
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
    public class Categories : AbstractMwsObject
    {

        private string _productCategoryId;
        private string _productCategoryName;
        private Categories _parent;

        /// <summary>
        /// Gets and sets the ProductCategoryId property.
        /// </summary>
        [XmlElement(ElementName = "ProductCategoryId")]
        public string ProductCategoryId
        {
            get { return this._productCategoryId; }
            set { this._productCategoryId = value; }
        }

        /// <summary>
        /// Sets the ProductCategoryId property.
        /// </summary>
        /// <param name="productCategoryId">ProductCategoryId property.</param>
        /// <returns>this instance.</returns>
        public Categories WithProductCategoryId(string productCategoryId)
        {
            this._productCategoryId = productCategoryId;
            return this;
        }

        /// <summary>
        /// Checks if ProductCategoryId property is set.
        /// </summary>
        /// <returns>true if ProductCategoryId property is set.</returns>
        public bool IsSetProductCategoryId()
        {
            return this._productCategoryId != null;
        }

        /// <summary>
        /// Gets and sets the ProductCategoryName property.
        /// </summary>
        [XmlElement(ElementName = "ProductCategoryName")]
        public string ProductCategoryName
        {
            get { return this._productCategoryName; }
            set { this._productCategoryName = value; }
        }

        /// <summary>
        /// Sets the ProductCategoryName property.
        /// </summary>
        /// <param name="productCategoryName">ProductCategoryName property.</param>
        /// <returns>this instance.</returns>
        public Categories WithProductCategoryName(string productCategoryName)
        {
            this._productCategoryName = productCategoryName;
            return this;
        }

        /// <summary>
        /// Checks if ProductCategoryName property is set.
        /// </summary>
        /// <returns>true if ProductCategoryName property is set.</returns>
        public bool IsSetProductCategoryName()
        {
            return this._productCategoryName != null;
        }

        /// <summary>
        /// Gets and sets the Parent property.
        /// </summary>
        [XmlElement(ElementName = "Parent")]
        public Categories Parent
        {
            get { return this._parent; }
            set { this._parent = value; }
        }

        /// <summary>
        /// Sets the Parent property.
        /// </summary>
        /// <param name="parent">Parent property.</param>
        /// <returns>this instance.</returns>
        public Categories WithParent(Categories parent)
        {
            this._parent = parent;
            return this;
        }

        /// <summary>
        /// Checks if Parent property is set.
        /// </summary>
        /// <returns>true if Parent property is set.</returns>
        public bool IsSetParent()
        {
            return this._parent != null;
        }


        public override void ReadFragmentFrom(IMwsReader reader)
        {
            this._productCategoryId = reader.Read<string>("ProductCategoryId");
            this._productCategoryName = reader.Read<string>("ProductCategoryName");
            this._parent = reader.Read<Categories>("Parent");
        }

        public override void WriteFragmentTo(IMwsWriter writer)
        {
            writer.Write("ProductCategoryId", this._productCategoryId);
            writer.Write("ProductCategoryName", this._productCategoryName);
            writer.Write("Parent", this._parent);
        }

        public override void WriteTo(IMwsWriter writer)
        {
            writer.Write("http://mws.amazonservices.com/schema/Products/2011-10-01", "Categories", this);
        }

        public Categories() : base()
        {
        }
    }
}
