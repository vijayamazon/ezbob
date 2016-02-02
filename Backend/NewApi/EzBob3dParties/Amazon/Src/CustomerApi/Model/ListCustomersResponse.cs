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
 * List Customers Response
 * API Version: 2014-03-01
 * Library Version: 2015-06-18
 * Generated: Thu Jun 18 19:32:10 GMT 2015
 */

namespace EzBob3dParties.Amazon.Src.CustomerApi.Model
{
    using EzBob3dParties.Amazon.Src.Common;

    public class ListCustomersResponse : AbstractMwsObject, IMWSResponse
    {

        private ListCustomersResult _listCustomersResult;
        private ResponseMetadata _responseMetadata;
        private ResponseHeaderMetadata _responseHeaderMetadata;

        /// <summary>
        /// Gets and sets the ListCustomersResult property.
        /// </summary>
        public ListCustomersResult ListCustomersResult
        {
            get { return this._listCustomersResult; }
            set { this._listCustomersResult = value; }
        }

        /// <summary>
        /// Sets the ListCustomersResult property.
        /// </summary>
        /// <param name="listCustomersResult">ListCustomersResult property.</param>
        /// <returns>this instance.</returns>
        public ListCustomersResponse WithListCustomersResult(ListCustomersResult listCustomersResult)
        {
            this._listCustomersResult = listCustomersResult;
            return this;
        }

        /// <summary>
        /// Checks if ListCustomersResult property is set.
        /// </summary>
        /// <returns>true if ListCustomersResult property is set.</returns>
        public bool IsSetListCustomersResult()
        {
            return this._listCustomersResult != null;
        }

        /// <summary>
        /// Gets and sets the ResponseMetadata property.
        /// </summary>
        public ResponseMetadata ResponseMetadata
        {
            get { return this._responseMetadata; }
            set { this._responseMetadata = value; }
        }

        /// <summary>
        /// Sets the ResponseMetadata property.
        /// </summary>
        /// <param name="responseMetadata">ResponseMetadata property.</param>
        /// <returns>this instance.</returns>
        public ListCustomersResponse WithResponseMetadata(ResponseMetadata responseMetadata)
        {
            this._responseMetadata = responseMetadata;
            return this;
        }

        /// <summary>
        /// Checks if ResponseMetadata property is set.
        /// </summary>
        /// <returns>true if ResponseMetadata property is set.</returns>
        public bool IsSetResponseMetadata()
        {
            return this._responseMetadata != null;
        }

        /// <summary>
        /// Gets and sets the ResponseHeaderMetadata property.
        /// </summary>
        public ResponseHeaderMetadata ResponseHeaderMetadata
        {
            get { return this._responseHeaderMetadata; }
            set { this._responseHeaderMetadata = value; }
        }

        /// <summary>
        /// Sets the ResponseHeaderMetadata property.
        /// </summary>
        /// <param name="responseHeaderMetadata">ResponseHeaderMetadata property.</param>
        /// <returns>this instance.</returns>
        public ListCustomersResponse WithResponseHeaderMetadata(ResponseHeaderMetadata responseHeaderMetadata)
        {
            this._responseHeaderMetadata = responseHeaderMetadata;
            return this;
        }

        /// <summary>
        /// Checks if ResponseHeaderMetadata property is set.
        /// </summary>
        /// <returns>true if ResponseHeaderMetadata property is set.</returns>
        public bool IsSetResponseHeaderMetadata()
        {
            return this._responseHeaderMetadata != null;
        }


        public override void ReadFragmentFrom(IMwsReader reader)
        {
            this._listCustomersResult = reader.Read<ListCustomersResult>("ListCustomersResult");
            this._responseMetadata = reader.Read<ResponseMetadata>("ResponseMetadata");
        }

        public override void WriteFragmentTo(IMwsWriter writer)
        {
            writer.Write("ListCustomersResult", this._listCustomersResult);
            writer.Write("ResponseMetadata", this._responseMetadata);
        }

        public override void WriteTo(IMwsWriter writer)
        {
            writer.Write("http://mws.amazonservices.com/schema/CustomerInformation/2014-03-01", "ListCustomersResponse", this);
        }

        public ListCustomersResponse() : base()
        {
        }
    }
}
