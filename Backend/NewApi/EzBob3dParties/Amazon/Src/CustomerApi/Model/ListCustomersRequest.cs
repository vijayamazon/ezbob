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
 * List Customers Request
 * API Version: 2014-03-01
 * Library Version: 2015-06-18
 * Generated: Thu Jun 18 19:32:10 GMT 2015
 */

namespace EzBob3dParties.Amazon.Src.CustomerApi.Model
{
    using System;
    using EzBob3dParties.Amazon.Src.Common;

    public class ListCustomersRequest : AbstractMwsObject
    {

        private string _sellerId;
        private string _mwsAuthToken;
        private string _marketplaceId;
        private string _dateRangeType;
        private DateTime? _dateRangeStart;
        private DateTime? _dateRangeEnd;

        /// <summary>
        /// Gets and sets the SellerId property.
        /// </summary>
        public string SellerId
        {
            get { return this._sellerId; }
            set { this._sellerId = value; }
        }

        /// <summary>
        /// Sets the SellerId property.
        /// </summary>
        /// <param name="sellerId">SellerId property.</param>
        /// <returns>this instance.</returns>
        public ListCustomersRequest WithSellerId(string sellerId)
        {
            this._sellerId = sellerId;
            return this;
        }

        /// <summary>
        /// Checks if SellerId property is set.
        /// </summary>
        /// <returns>true if SellerId property is set.</returns>
        public bool IsSetSellerId()
        {
            return this._sellerId != null;
        }

        /// <summary>
        /// Gets and sets the MWSAuthToken property.
        /// </summary>
        public string MWSAuthToken
        {
            get { return this._mwsAuthToken; }
            set { this._mwsAuthToken = value; }
        }

        /// <summary>
        /// Sets the MWSAuthToken property.
        /// </summary>
        /// <param name="mwsAuthToken">MWSAuthToken property.</param>
        /// <returns>this instance.</returns>
        public ListCustomersRequest WithMWSAuthToken(string mwsAuthToken)
        {
            this._mwsAuthToken = mwsAuthToken;
            return this;
        }

        /// <summary>
        /// Checks if MWSAuthToken property is set.
        /// </summary>
        /// <returns>true if MWSAuthToken property is set.</returns>
        public bool IsSetMWSAuthToken()
        {
            return this._mwsAuthToken != null;
        }

        /// <summary>
        /// Gets and sets the MarketplaceId property.
        /// </summary>
        public string MarketplaceId
        {
            get { return this._marketplaceId; }
            set { this._marketplaceId = value; }
        }

        /// <summary>
        /// Sets the MarketplaceId property.
        /// </summary>
        /// <param name="marketplaceId">MarketplaceId property.</param>
        /// <returns>this instance.</returns>
        public ListCustomersRequest WithMarketplaceId(string marketplaceId)
        {
            this._marketplaceId = marketplaceId;
            return this;
        }

        /// <summary>
        /// Checks if MarketplaceId property is set.
        /// </summary>
        /// <returns>true if MarketplaceId property is set.</returns>
        public bool IsSetMarketplaceId()
        {
            return this._marketplaceId != null;
        }

        /// <summary>
        /// Gets and sets the DateRangeType property.
        /// </summary>
        public string DateRangeType
        {
            get { return this._dateRangeType; }
            set { this._dateRangeType = value; }
        }

        /// <summary>
        /// Sets the DateRangeType property.
        /// </summary>
        /// <param name="dateRangeType">DateRangeType property.</param>
        /// <returns>this instance.</returns>
        public ListCustomersRequest WithDateRangeType(string dateRangeType)
        {
            this._dateRangeType = dateRangeType;
            return this;
        }

        /// <summary>
        /// Checks if DateRangeType property is set.
        /// </summary>
        /// <returns>true if DateRangeType property is set.</returns>
        public bool IsSetDateRangeType()
        {
            return this._dateRangeType != null;
        }

        /// <summary>
        /// Gets and sets the DateRangeStart property.
        /// </summary>
        public DateTime DateRangeStart
        {
            get { return this._dateRangeStart.GetValueOrDefault(); }
            set { this._dateRangeStart = value; }
        }

        /// <summary>
        /// Sets the DateRangeStart property.
        /// </summary>
        /// <param name="dateRangeStart">DateRangeStart property.</param>
        /// <returns>this instance.</returns>
        public ListCustomersRequest WithDateRangeStart(DateTime dateRangeStart)
        {
            this._dateRangeStart = dateRangeStart;
            return this;
        }

        /// <summary>
        /// Checks if DateRangeStart property is set.
        /// </summary>
        /// <returns>true if DateRangeStart property is set.</returns>
        public bool IsSetDateRangeStart()
        {
            return this._dateRangeStart != null;
        }

        /// <summary>
        /// Gets and sets the DateRangeEnd property.
        /// </summary>
        public DateTime DateRangeEnd
        {
            get { return this._dateRangeEnd.GetValueOrDefault(); }
            set { this._dateRangeEnd = value; }
        }

        /// <summary>
        /// Sets the DateRangeEnd property.
        /// </summary>
        /// <param name="dateRangeEnd">DateRangeEnd property.</param>
        /// <returns>this instance.</returns>
        public ListCustomersRequest WithDateRangeEnd(DateTime dateRangeEnd)
        {
            this._dateRangeEnd = dateRangeEnd;
            return this;
        }

        /// <summary>
        /// Checks if DateRangeEnd property is set.
        /// </summary>
        /// <returns>true if DateRangeEnd property is set.</returns>
        public bool IsSetDateRangeEnd()
        {
            return this._dateRangeEnd != null;
        }


        public override void ReadFragmentFrom(IMwsReader reader)
        {
            this._sellerId = reader.Read<string>("SellerId");
            this._mwsAuthToken = reader.Read<string>("MWSAuthToken");
            this._marketplaceId = reader.Read<string>("MarketplaceId");
            this._dateRangeType = reader.Read<string>("DateRangeType");
            this._dateRangeStart = reader.Read<DateTime?>("DateRangeStart");
            this._dateRangeEnd = reader.Read<DateTime?>("DateRangeEnd");
        }

        public override void WriteFragmentTo(IMwsWriter writer)
        {
            writer.Write("SellerId", this._sellerId);
            writer.Write("MWSAuthToken", this._mwsAuthToken);
            writer.Write("MarketplaceId", this._marketplaceId);
            writer.Write("DateRangeType", this._dateRangeType);
            writer.Write("DateRangeStart", this._dateRangeStart);
            writer.Write("DateRangeEnd", this._dateRangeEnd);
        }

        public override void WriteTo(IMwsWriter writer)
        {
            writer.Write("http://mws.amazonservices.com/schema/CustomerInformation/2014-03-01", "ListCustomersRequest", this);
        }

        public ListCustomersRequest() : base()
        {
        }
    }
}
