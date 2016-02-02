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
 * Get Customers By Customer Id Response
 * API Version: 2014-03-01
 * Library Version: 2015-06-18
 * Generated: Thu Jun 18 19:32:10 GMT 2015
 */

namespace EzBob3dParties.Amazon.Src.CustomerApi.Model
{
    using EzBob3dParties.Amazon.Src.Common;

    public class GetCustomersByCustomerIdResponse : AbstractMwsObject, IMWSResponse
    {

        private GetCustomersByCustomerIdResult _getCustomersByCustomerIdResult;
        private ResponseMetadata _responseMetadata;
        private ResponseHeaderMetadata _responseHeaderMetadata;

        /// <summary>
        /// Gets and sets the GetCustomersByCustomerIdResult property.
        /// </summary>
        public GetCustomersByCustomerIdResult GetCustomersByCustomerIdResult
        {
            get { return this._getCustomersByCustomerIdResult; }
            set { this._getCustomersByCustomerIdResult = value; }
        }

        /// <summary>
        /// Sets the GetCustomersByCustomerIdResult property.
        /// </summary>
        /// <param name="getCustomersByCustomerIdResult">GetCustomersByCustomerIdResult property.</param>
        /// <returns>this instance.</returns>
        public GetCustomersByCustomerIdResponse WithGetCustomersByCustomerIdResult(GetCustomersByCustomerIdResult getCustomersByCustomerIdResult)
        {
            this._getCustomersByCustomerIdResult = getCustomersByCustomerIdResult;
            return this;
        }

        /// <summary>
        /// Checks if GetCustomersByCustomerIdResult property is set.
        /// </summary>
        /// <returns>true if GetCustomersByCustomerIdResult property is set.</returns>
        public bool IsSetGetCustomersByCustomerIdResult()
        {
            return this._getCustomersByCustomerIdResult != null;
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
        public GetCustomersByCustomerIdResponse WithResponseMetadata(ResponseMetadata responseMetadata)
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
        public GetCustomersByCustomerIdResponse WithResponseHeaderMetadata(ResponseHeaderMetadata responseHeaderMetadata)
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
            this._getCustomersByCustomerIdResult = reader.Read<GetCustomersByCustomerIdResult>("GetCustomersByCustomerIdResult");
            this._responseMetadata = reader.Read<ResponseMetadata>("ResponseMetadata");
        }

        public override void WriteFragmentTo(IMwsWriter writer)
        {
            writer.Write("GetCustomersByCustomerIdResult", this._getCustomersByCustomerIdResult);
            writer.Write("ResponseMetadata", this._responseMetadata);
        }

        public override void WriteTo(IMwsWriter writer)
        {
            writer.Write("http://mws.amazonservices.com/schema/CustomerInformation/2014-03-01", "GetCustomersByCustomerIdResponse", this);
        }

        public GetCustomersByCustomerIdResponse() : base()
        {
        }
    }
}
