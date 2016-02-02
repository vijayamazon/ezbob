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
 * Address
 * API Version: 2014-03-01
 * Library Version: 2015-06-18
 * Generated: Thu Jun 18 19:32:10 GMT 2015
 */

namespace EzBob3dParties.Amazon.Src.CustomerApi.Model
{
    using EzBob3dParties.Amazon.Src.Common;

    public class Address : AbstractMwsObject
    {

        private bool? _isDefaultAddress;
        private string _fullName;
        private string _addressLine1;
        private string _addressLine2;
        private string _city;
        private string _stateOrRegion;
        private string _postalCode;
        private string _countryCode;

        /// <summary>
        /// Gets and sets the IsDefaultAddress property.
        /// </summary>
        public bool IsDefaultAddress
        {
            get { return this._isDefaultAddress.GetValueOrDefault(); }
            set { this._isDefaultAddress = value; }
        }

        /// <summary>
        /// Sets the IsDefaultAddress property.
        /// </summary>
        /// <param name="isDefaultAddress">IsDefaultAddress property.</param>
        /// <returns>this instance.</returns>
        public Address WithIsDefaultAddress(bool isDefaultAddress)
        {
            this._isDefaultAddress = isDefaultAddress;
            return this;
        }

        /// <summary>
        /// Checks if IsDefaultAddress property is set.
        /// </summary>
        /// <returns>true if IsDefaultAddress property is set.</returns>
        public bool IsSetIsDefaultAddress()
        {
            return this._isDefaultAddress != null;
        }

        /// <summary>
        /// Gets and sets the FullName property.
        /// </summary>
        public string FullName
        {
            get { return this._fullName; }
            set { this._fullName = value; }
        }

        /// <summary>
        /// Sets the FullName property.
        /// </summary>
        /// <param name="fullName">FullName property.</param>
        /// <returns>this instance.</returns>
        public Address WithFullName(string fullName)
        {
            this._fullName = fullName;
            return this;
        }

        /// <summary>
        /// Checks if FullName property is set.
        /// </summary>
        /// <returns>true if FullName property is set.</returns>
        public bool IsSetFullName()
        {
            return this._fullName != null;
        }

        /// <summary>
        /// Gets and sets the AddressLine1 property.
        /// </summary>
        public string AddressLine1
        {
            get { return this._addressLine1; }
            set { this._addressLine1 = value; }
        }

        /// <summary>
        /// Sets the AddressLine1 property.
        /// </summary>
        /// <param name="addressLine1">AddressLine1 property.</param>
        /// <returns>this instance.</returns>
        public Address WithAddressLine1(string addressLine1)
        {
            this._addressLine1 = addressLine1;
            return this;
        }

        /// <summary>
        /// Checks if AddressLine1 property is set.
        /// </summary>
        /// <returns>true if AddressLine1 property is set.</returns>
        public bool IsSetAddressLine1()
        {
            return this._addressLine1 != null;
        }

        /// <summary>
        /// Gets and sets the AddressLine2 property.
        /// </summary>
        public string AddressLine2
        {
            get { return this._addressLine2; }
            set { this._addressLine2 = value; }
        }

        /// <summary>
        /// Sets the AddressLine2 property.
        /// </summary>
        /// <param name="addressLine2">AddressLine2 property.</param>
        /// <returns>this instance.</returns>
        public Address WithAddressLine2(string addressLine2)
        {
            this._addressLine2 = addressLine2;
            return this;
        }

        /// <summary>
        /// Checks if AddressLine2 property is set.
        /// </summary>
        /// <returns>true if AddressLine2 property is set.</returns>
        public bool IsSetAddressLine2()
        {
            return this._addressLine2 != null;
        }

        /// <summary>
        /// Gets and sets the City property.
        /// </summary>
        public string City
        {
            get { return this._city; }
            set { this._city = value; }
        }

        /// <summary>
        /// Sets the City property.
        /// </summary>
        /// <param name="city">City property.</param>
        /// <returns>this instance.</returns>
        public Address WithCity(string city)
        {
            this._city = city;
            return this;
        }

        /// <summary>
        /// Checks if City property is set.
        /// </summary>
        /// <returns>true if City property is set.</returns>
        public bool IsSetCity()
        {
            return this._city != null;
        }

        /// <summary>
        /// Gets and sets the StateOrRegion property.
        /// </summary>
        public string StateOrRegion
        {
            get { return this._stateOrRegion; }
            set { this._stateOrRegion = value; }
        }

        /// <summary>
        /// Sets the StateOrRegion property.
        /// </summary>
        /// <param name="stateOrRegion">StateOrRegion property.</param>
        /// <returns>this instance.</returns>
        public Address WithStateOrRegion(string stateOrRegion)
        {
            this._stateOrRegion = stateOrRegion;
            return this;
        }

        /// <summary>
        /// Checks if StateOrRegion property is set.
        /// </summary>
        /// <returns>true if StateOrRegion property is set.</returns>
        public bool IsSetStateOrRegion()
        {
            return this._stateOrRegion != null;
        }

        /// <summary>
        /// Gets and sets the PostalCode property.
        /// </summary>
        public string PostalCode
        {
            get { return this._postalCode; }
            set { this._postalCode = value; }
        }

        /// <summary>
        /// Sets the PostalCode property.
        /// </summary>
        /// <param name="postalCode">PostalCode property.</param>
        /// <returns>this instance.</returns>
        public Address WithPostalCode(string postalCode)
        {
            this._postalCode = postalCode;
            return this;
        }

        /// <summary>
        /// Checks if PostalCode property is set.
        /// </summary>
        /// <returns>true if PostalCode property is set.</returns>
        public bool IsSetPostalCode()
        {
            return this._postalCode != null;
        }

        /// <summary>
        /// Gets and sets the CountryCode property.
        /// </summary>
        public string CountryCode
        {
            get { return this._countryCode; }
            set { this._countryCode = value; }
        }

        /// <summary>
        /// Sets the CountryCode property.
        /// </summary>
        /// <param name="countryCode">CountryCode property.</param>
        /// <returns>this instance.</returns>
        public Address WithCountryCode(string countryCode)
        {
            this._countryCode = countryCode;
            return this;
        }

        /// <summary>
        /// Checks if CountryCode property is set.
        /// </summary>
        /// <returns>true if CountryCode property is set.</returns>
        public bool IsSetCountryCode()
        {
            return this._countryCode != null;
        }


        public override void ReadFragmentFrom(IMwsReader reader)
        {
            this._isDefaultAddress = reader.Read<bool?>("IsDefaultAddress");
            this._fullName = reader.Read<string>("FullName");
            this._addressLine1 = reader.Read<string>("AddressLine1");
            this._addressLine2 = reader.Read<string>("AddressLine2");
            this._city = reader.Read<string>("City");
            this._stateOrRegion = reader.Read<string>("StateOrRegion");
            this._postalCode = reader.Read<string>("PostalCode");
            this._countryCode = reader.Read<string>("CountryCode");
        }

        public override void WriteFragmentTo(IMwsWriter writer)
        {
            writer.Write("IsDefaultAddress", this._isDefaultAddress);
            writer.Write("FullName", this._fullName);
            writer.Write("AddressLine1", this._addressLine1);
            writer.Write("AddressLine2", this._addressLine2);
            writer.Write("City", this._city);
            writer.Write("StateOrRegion", this._stateOrRegion);
            writer.Write("PostalCode", this._postalCode);
            writer.Write("CountryCode", this._countryCode);
        }

        public override void WriteTo(IMwsWriter writer)
        {
            writer.Write("http://mws.amazonservices.com/schema/CustomerInformation/2014-03-01", "Address", this);
        }

        public Address() : base()
        {
        }
    }
}
