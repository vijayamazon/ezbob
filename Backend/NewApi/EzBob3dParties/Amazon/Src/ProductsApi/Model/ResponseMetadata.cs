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
 * Response Metadata
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
    public class ResponseMetadata : AbstractMwsObject
    {

        private string _requestId;

        /// <summary>
        /// Gets and sets the RequestId property.
        /// </summary>
        [XmlElement(ElementName = "RequestId")]
        public string RequestId
        {
            get { return this._requestId; }
            set { this._requestId = value; }
        }

        /// <summary>
        /// Sets the RequestId property.
        /// </summary>
        /// <param name="requestId">RequestId property.</param>
        /// <returns>this instance.</returns>
        public ResponseMetadata WithRequestId(string requestId)
        {
            this._requestId = requestId;
            return this;
        }

        /// <summary>
        /// Checks if RequestId property is set.
        /// </summary>
        /// <returns>true if RequestId property is set.</returns>
        public bool IsSetRequestId()
        {
            return this._requestId != null;
        }


        public override void ReadFragmentFrom(IMwsReader reader)
        {
            this._requestId = reader.Read<string>("RequestId");
        }

        public override void WriteFragmentTo(IMwsWriter writer)
        {
            writer.Write("RequestId", this._requestId);
        }

        public override void WriteTo(IMwsWriter writer)
        {
            writer.Write("http://mws.amazonservices.com/schema/Products/2011-10-01", "ResponseMetadata", this);
        }

        public ResponseMetadata() : base()
        {
        }
    }
}
