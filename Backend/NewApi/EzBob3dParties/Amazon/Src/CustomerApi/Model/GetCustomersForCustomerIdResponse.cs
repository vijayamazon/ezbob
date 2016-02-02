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
 * Get Customers For Customer Id Response
 * API Version: 2014-03-01
 * Library Version: 2015-06-18
 * Generated: Thu Jun 18 19:32:10 GMT 2015
 */

namespace EzBob3dParties.Amazon.Src.CustomerApi.Model
{
    using EzBob3dParties.Amazon.Src.Common;

    public class GetCustomersForCustomerIdResponse : AbstractMwsObject, IMWSResponse
    {

        private GetCustomersForCustomerIdResult _getCustomersForCustomerIdResult;
        private ResponseMetadata _responseMetadata;
        private ResponseHeaderMetadata _responseHeaderMetadata;

        /// <summary>
        /// Gets and sets the GetCustomersForCustomerIdResult property.
        /// </summary>
        public GetCustomersForCustomerIdResult GetCustomersForCustomerIdResult
        {
            get { return this._getCustomersForCustomerIdResult; }
            set { this._getCustomersForCustomerIdResult = value; }
        }

        /// <summary>
        /// Sets the GetCustomersForCustomerIdResult property.
        /// </summary>
        /// <param name="getCustomersForCustomerIdResult">GetCustomersForCustomerIdResult property.</param>
        /// <returns>this instance.</returns>
        public GetCustomersForCustomerIdResponse WithGetCustomersForCustomerIdResult(GetCustomersForCustomerIdResult getCustomersForCustomerIdResult)
        {
            this._getCustomersForCustomerIdResult = getCustomersForCustomerIdResult;
            return this;
        }

        /// <summary>
        /// Checks if GetCustomersForCustomerIdResult property is set.
        /// </summary>
        /// <returns>true if GetCustomersForCustomerIdResult property is set.</returns>
        public bool IsSetGetCustomersForCustomerIdResult()
        {
            return this._getCustomersForCustomerIdResult != null;
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
        public GetCustomersForCustomerIdResponse WithResponseMetadata(ResponseMetadata responseMetadata)
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
        public GetCustomersForCustomerIdResponse WithResponseHeaderMetadata(ResponseHeaderMetadata responseHeaderMetadata)
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
            this._getCustomersForCustomerIdResult = reader.Read<GetCustomersForCustomerIdResult>("GetCustomersForCustomerIdResult");
            this._responseMetadata = reader.Read<ResponseMetadata>("ResponseMetadata");
        }

        public override void WriteFragmentTo(IMwsWriter writer)
        {
            writer.Write("GetCustomersForCustomerIdResult", this._getCustomersForCustomerIdResult);
            writer.Write("ResponseMetadata", this._responseMetadata);
        }

        public override void WriteTo(IMwsWriter writer)
        {
            writer.Write("http://mws.amazonservices.com/schema/CustomerInformation/2014-03-01", "GetCustomersForCustomerIdResponse", this);
        }

        public GetCustomersForCustomerIdResponse() : base()
        {
        }
    }
}
