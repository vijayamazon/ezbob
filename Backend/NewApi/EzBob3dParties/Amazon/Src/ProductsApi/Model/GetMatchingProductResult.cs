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
 * Get Matching Product Result
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
    public class GetMatchingProductResult : AbstractMwsObject
    {

        private Product _product;
        private Error _error;
        private string _asin;
        private string _status;

        /// <summary>
        /// Gets and sets the Product property.
        /// </summary>
        [XmlElement(ElementName = "Product")]
        public Product Product
        {
            get { return this._product; }
            set { this._product = value; }
        }

        /// <summary>
        /// Sets the Product property.
        /// </summary>
        /// <param name="product">Product property.</param>
        /// <returns>this instance.</returns>
        public GetMatchingProductResult WithProduct(Product product)
        {
            this._product = product;
            return this;
        }

        /// <summary>
        /// Checks if Product property is set.
        /// </summary>
        /// <returns>true if Product property is set.</returns>
        public bool IsSetProduct()
        {
            return this._product != null;
        }

        /// <summary>
        /// Gets and sets the Error property.
        /// </summary>
        [XmlElement(ElementName = "Error")]
        public Error Error
        {
            get { return this._error; }
            set { this._error = value; }
        }

        /// <summary>
        /// Sets the Error property.
        /// </summary>
        /// <param name="error">Error property.</param>
        /// <returns>this instance.</returns>
        public GetMatchingProductResult WithError(Error error)
        {
            this._error = error;
            return this;
        }

        /// <summary>
        /// Checks if Error property is set.
        /// </summary>
        /// <returns>true if Error property is set.</returns>
        public bool IsSetError()
        {
            return this._error != null;
        }

        /// <summary>
        /// Gets and sets the ASIN property.
        /// </summary>
        [XmlAttribute(AttributeName = "ASIN")]
        public string ASIN
        {
            get { return this._asin; }
            set { this._asin = value; }
        }

        /// <summary>
        /// Sets the ASIN property.
        /// </summary>
        /// <param name="asin">ASIN property.</param>
        /// <returns>this instance.</returns>
        public GetMatchingProductResult WithASIN(string asin)
        {
            this._asin = asin;
            return this;
        }

        /// <summary>
        /// Checks if ASIN property is set.
        /// </summary>
        /// <returns>true if ASIN property is set.</returns>
        public bool IsSetASIN()
        {
            return this._asin != null;
        }

        /// <summary>
        /// Gets and sets the status property.
        /// </summary>
        [XmlAttribute(AttributeName = "status")]
        public string status
        {
            get { return this._status; }
            set { this._status = value; }
        }

        /// <summary>
        /// Sets the status property.
        /// </summary>
        /// <param name="status">status property.</param>
        /// <returns>this instance.</returns>
        public GetMatchingProductResult Withstatus(string status)
        {
            this._status = status;
            return this;
        }

        /// <summary>
        /// Checks if status property is set.
        /// </summary>
        /// <returns>true if status property is set.</returns>
        public bool IsSetstatus()
        {
            return this._status != null;
        }


        public override void ReadFragmentFrom(IMwsReader reader)
        {
            this._asin = reader.ReadAttribute<string>("ASIN");
            this._status = reader.ReadAttribute<string>("status");
            this._product = reader.Read<Product>("Product");
            this._error = reader.Read<Error>("Error");
        }

        public override void WriteFragmentTo(IMwsWriter writer)
        {
            writer.WriteAttribute("ASIN",this._asin);
            writer.WriteAttribute("status",this._status);
            writer.Write("Product", this._product);
            writer.Write("Error", this._error);
        }

        public override void WriteTo(IMwsWriter writer)
        {
            writer.Write("http://mws.amazonservices.com/schema/Products/2011-10-01", "GetMatchingProductResult", this);
        }

        public GetMatchingProductResult() : base()
        {
        }
    }
}
