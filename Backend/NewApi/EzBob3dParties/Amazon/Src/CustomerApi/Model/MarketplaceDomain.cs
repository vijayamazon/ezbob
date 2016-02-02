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
 * Marketplace Domain
 * API Version: 2014-03-01
 * Library Version: 2015-06-18
 * Generated: Thu Jun 18 19:32:10 GMT 2015
 */

namespace EzBob3dParties.Amazon.Src.CustomerApi.Model
{
    using System;
    using EzBob3dParties.Amazon.Src.Common;

    public class MarketplaceDomain : AbstractMwsObject
    {

        private string _domainName;
        private bool? _origin;
        private DateTime? _associatedOn;
        private DateTime? _lastUpdatedOn;

        /// <summary>
        /// Gets and sets the DomainName property.
        /// </summary>
        public string DomainName
        {
            get { return this._domainName; }
            set { this._domainName = value; }
        }

        /// <summary>
        /// Sets the DomainName property.
        /// </summary>
        /// <param name="domainName">DomainName property.</param>
        /// <returns>this instance.</returns>
        public MarketplaceDomain WithDomainName(string domainName)
        {
            this._domainName = domainName;
            return this;
        }

        /// <summary>
        /// Checks if DomainName property is set.
        /// </summary>
        /// <returns>true if DomainName property is set.</returns>
        public bool IsSetDomainName()
        {
            return this._domainName != null;
        }

        /// <summary>
        /// Gets and sets the Origin property.
        /// </summary>
        public bool Origin
        {
            get { return this._origin.GetValueOrDefault(); }
            set { this._origin = value; }
        }

        /// <summary>
        /// Sets the Origin property.
        /// </summary>
        /// <param name="origin">Origin property.</param>
        /// <returns>this instance.</returns>
        public MarketplaceDomain WithOrigin(bool origin)
        {
            this._origin = origin;
            return this;
        }

        /// <summary>
        /// Checks if Origin property is set.
        /// </summary>
        /// <returns>true if Origin property is set.</returns>
        public bool IsSetOrigin()
        {
            return this._origin != null;
        }

        /// <summary>
        /// Gets and sets the AssociatedOn property.
        /// </summary>
        public DateTime AssociatedOn
        {
            get { return this._associatedOn.GetValueOrDefault(); }
            set { this._associatedOn = value; }
        }

        /// <summary>
        /// Sets the AssociatedOn property.
        /// </summary>
        /// <param name="associatedOn">AssociatedOn property.</param>
        /// <returns>this instance.</returns>
        public MarketplaceDomain WithAssociatedOn(DateTime associatedOn)
        {
            this._associatedOn = associatedOn;
            return this;
        }

        /// <summary>
        /// Checks if AssociatedOn property is set.
        /// </summary>
        /// <returns>true if AssociatedOn property is set.</returns>
        public bool IsSetAssociatedOn()
        {
            return this._associatedOn != null;
        }

        /// <summary>
        /// Gets and sets the LastUpdatedOn property.
        /// </summary>
        public DateTime LastUpdatedOn
        {
            get { return this._lastUpdatedOn.GetValueOrDefault(); }
            set { this._lastUpdatedOn = value; }
        }

        /// <summary>
        /// Sets the LastUpdatedOn property.
        /// </summary>
        /// <param name="lastUpdatedOn">LastUpdatedOn property.</param>
        /// <returns>this instance.</returns>
        public MarketplaceDomain WithLastUpdatedOn(DateTime lastUpdatedOn)
        {
            this._lastUpdatedOn = lastUpdatedOn;
            return this;
        }

        /// <summary>
        /// Checks if LastUpdatedOn property is set.
        /// </summary>
        /// <returns>true if LastUpdatedOn property is set.</returns>
        public bool IsSetLastUpdatedOn()
        {
            return this._lastUpdatedOn != null;
        }


        public override void ReadFragmentFrom(IMwsReader reader)
        {
            this._domainName = reader.Read<string>("DomainName");
            this._origin = reader.Read<bool?>("Origin");
            this._associatedOn = reader.Read<DateTime?>("AssociatedOn");
            this._lastUpdatedOn = reader.Read<DateTime?>("LastUpdatedOn");
        }

        public override void WriteFragmentTo(IMwsWriter writer)
        {
            writer.Write("DomainName", this._domainName);
            writer.Write("Origin", this._origin);
            writer.Write("AssociatedOn", this._associatedOn);
            writer.Write("LastUpdatedOn", this._lastUpdatedOn);
        }

        public override void WriteTo(IMwsWriter writer)
        {
            writer.Write("http://mws.amazonservices.com/schema/CustomerInformation/2014-03-01", "MarketplaceDomain", this);
        }

        public MarketplaceDomain() : base()
        {
        }
    }
}
