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
 * Customer
 * API Version: 2014-03-01
 * Library Version: 2015-06-18
 * Generated: Thu Jun 18 19:32:10 GMT 2015
 */

namespace EzBob3dParties.Amazon.Src.CustomerApi.Model
{
    using System.Collections.Generic;
    using EzBob3dParties.Amazon.Src.Common;

    public class Customer : AbstractMwsObject
    {

        private string _customerId;
        private List<Address> _shippingAddressList;
        private CustomerPrimaryContactInfo _primaryContactInfo;
        private string _accountType;
        private List<MarketplaceDomain> _associatedMarketplaces;

        /// <summary>
        /// Gets and sets the CustomerId property.
        /// </summary>
        public string CustomerId
        {
            get { return this._customerId; }
            set { this._customerId = value; }
        }

        /// <summary>
        /// Sets the CustomerId property.
        /// </summary>
        /// <param name="customerId">CustomerId property.</param>
        /// <returns>this instance.</returns>
        public Customer WithCustomerId(string customerId)
        {
            this._customerId = customerId;
            return this;
        }

        /// <summary>
        /// Checks if CustomerId property is set.
        /// </summary>
        /// <returns>true if CustomerId property is set.</returns>
        public bool IsSetCustomerId()
        {
            return this._customerId != null;
        }

        /// <summary>
        /// Gets and sets the ShippingAddressList property.
        /// </summary>
        public List<Address> ShippingAddressList
        {
            get
            {
                if(this._shippingAddressList == null)
                {
                    this._shippingAddressList = new List<Address>();
                }
                return this._shippingAddressList;
            }
            set { this._shippingAddressList = value; }
        }

        /// <summary>
        /// Sets the ShippingAddressList property.
        /// </summary>
        /// <param name="shippingAddressList">ShippingAddressList property.</param>
        /// <returns>this instance.</returns>
        public Customer WithShippingAddressList(Address[] shippingAddressList)
        {
            this._shippingAddressList.AddRange(shippingAddressList);
            return this;
        }

        /// <summary>
        /// Checks if ShippingAddressList property is set.
        /// </summary>
        /// <returns>true if ShippingAddressList property is set.</returns>
        public bool IsSetShippingAddressList()
        {
            return this.ShippingAddressList.Count > 0;
        }

        /// <summary>
        /// Gets and sets the PrimaryContactInfo property.
        /// </summary>
        public CustomerPrimaryContactInfo PrimaryContactInfo
        {
            get { return this._primaryContactInfo; }
            set { this._primaryContactInfo = value; }
        }

        /// <summary>
        /// Sets the PrimaryContactInfo property.
        /// </summary>
        /// <param name="primaryContactInfo">PrimaryContactInfo property.</param>
        /// <returns>this instance.</returns>
        public Customer WithPrimaryContactInfo(CustomerPrimaryContactInfo primaryContactInfo)
        {
            this._primaryContactInfo = primaryContactInfo;
            return this;
        }

        /// <summary>
        /// Checks if PrimaryContactInfo property is set.
        /// </summary>
        /// <returns>true if PrimaryContactInfo property is set.</returns>
        public bool IsSetPrimaryContactInfo()
        {
            return this._primaryContactInfo != null;
        }

        /// <summary>
        /// Gets and sets the AccountType property.
        /// </summary>
        public string AccountType
        {
            get { return this._accountType; }
            set { this._accountType = value; }
        }

        /// <summary>
        /// Sets the AccountType property.
        /// </summary>
        /// <param name="accountType">AccountType property.</param>
        /// <returns>this instance.</returns>
        public Customer WithAccountType(string accountType)
        {
            this._accountType = accountType;
            return this;
        }

        /// <summary>
        /// Checks if AccountType property is set.
        /// </summary>
        /// <returns>true if AccountType property is set.</returns>
        public bool IsSetAccountType()
        {
            return this._accountType != null;
        }

        /// <summary>
        /// Gets and sets the AssociatedMarketplaces property.
        /// </summary>
        public List<MarketplaceDomain> AssociatedMarketplaces
        {
            get
            {
                if(this._associatedMarketplaces == null)
                {
                    this._associatedMarketplaces = new List<MarketplaceDomain>();
                }
                return this._associatedMarketplaces;
            }
            set { this._associatedMarketplaces = value; }
        }

        /// <summary>
        /// Sets the AssociatedMarketplaces property.
        /// </summary>
        /// <param name="associatedMarketplaces">AssociatedMarketplaces property.</param>
        /// <returns>this instance.</returns>
        public Customer WithAssociatedMarketplaces(MarketplaceDomain[] associatedMarketplaces)
        {
            this._associatedMarketplaces.AddRange(associatedMarketplaces);
            return this;
        }

        /// <summary>
        /// Checks if AssociatedMarketplaces property is set.
        /// </summary>
        /// <returns>true if AssociatedMarketplaces property is set.</returns>
        public bool IsSetAssociatedMarketplaces()
        {
            return this.AssociatedMarketplaces.Count > 0;
        }


        public override void ReadFragmentFrom(IMwsReader reader)
        {
            this._customerId = reader.Read<string>("CustomerId");
            this._shippingAddressList = reader.ReadList<Address>("ShippingAddressList", "ShippingAddress");
            this._primaryContactInfo = reader.Read<CustomerPrimaryContactInfo>("PrimaryContactInfo");
            this._accountType = reader.Read<string>("AccountType");
            this._associatedMarketplaces = reader.ReadList<MarketplaceDomain>("AssociatedMarketplaces", "MarketplaceDomain");
        }

        public override void WriteFragmentTo(IMwsWriter writer)
        {
            writer.Write("CustomerId", this._customerId);
            writer.WriteList("ShippingAddressList", "ShippingAddress", this._shippingAddressList);
            writer.Write("PrimaryContactInfo", this._primaryContactInfo);
            writer.Write("AccountType", this._accountType);
            writer.WriteList("AssociatedMarketplaces", "MarketplaceDomain", this._associatedMarketplaces);
        }

        public override void WriteTo(IMwsWriter writer)
        {
            writer.Write("http://mws.amazonservices.com/schema/CustomerInformation/2014-03-01", "Customer", this);
        }

        public Customer() : base()
        {
        }
    }
}
