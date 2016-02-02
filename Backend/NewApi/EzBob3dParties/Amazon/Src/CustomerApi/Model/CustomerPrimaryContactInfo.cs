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
 * Customer Primary Contact Info
 * API Version: 2014-03-01
 * Library Version: 2015-06-18
 * Generated: Thu Jun 18 19:32:10 GMT 2015
 */

namespace EzBob3dParties.Amazon.Src.CustomerApi.Model
{
    using EzBob3dParties.Amazon.Src.Common;

    public class CustomerPrimaryContactInfo : AbstractMwsObject
    {

        private string _email;
        private string _fullName;

        /// <summary>
        /// Gets and sets the Email property.
        /// </summary>
        public string Email
        {
            get { return this._email; }
            set { this._email = value; }
        }

        /// <summary>
        /// Sets the Email property.
        /// </summary>
        /// <param name="email">Email property.</param>
        /// <returns>this instance.</returns>
        public CustomerPrimaryContactInfo WithEmail(string email)
        {
            this._email = email;
            return this;
        }

        /// <summary>
        /// Checks if Email property is set.
        /// </summary>
        /// <returns>true if Email property is set.</returns>
        public bool IsSetEmail()
        {
            return this._email != null;
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
        public CustomerPrimaryContactInfo WithFullName(string fullName)
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


        public override void ReadFragmentFrom(IMwsReader reader)
        {
            this._email = reader.Read<string>("Email");
            this._fullName = reader.Read<string>("FullName");
        }

        public override void WriteFragmentTo(IMwsWriter writer)
        {
            writer.Write("Email", this._email);
            writer.Write("FullName", this._fullName);
        }

        public override void WriteTo(IMwsWriter writer)
        {
            writer.Write("http://mws.amazonservices.com/schema/CustomerInformation/2014-03-01", "CustomerPrimaryContactInfo", this);
        }

        public CustomerPrimaryContactInfo() : base()
        {
        }
    }
}
