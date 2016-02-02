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
 * List Customers By Next Token Result
 * API Version: 2014-03-01
 * Library Version: 2015-06-18
 * Generated: Thu Jun 18 19:32:10 GMT 2015
 */

namespace EzBob3dParties.Amazon.Src.CustomerApi.Model
{
    using System.Collections.Generic;
    using EzBob3dParties.Amazon.Src.Common;

    public class ListCustomersByNextTokenResult : AbstractMwsObject
    {

        private List<Customer> _customerList;
        private string _nextToken;
        private bool _moreResultsAvailable;

        /// <summary>
        /// Gets and sets the CustomerList property.
        /// </summary>
        public List<Customer> CustomerList
        {
            get
            {
                if(this._customerList == null)
                {
                    this._customerList = new List<Customer>();
                }
                return this._customerList;
            }
            set { this._customerList = value; }
        }

        /// <summary>
        /// Sets the CustomerList property.
        /// </summary>
        /// <param name="customerList">CustomerList property.</param>
        /// <returns>this instance.</returns>
        public ListCustomersByNextTokenResult WithCustomerList(Customer[] customerList)
        {
            this._customerList.AddRange(customerList);
            return this;
        }

        /// <summary>
        /// Checks if CustomerList property is set.
        /// </summary>
        /// <returns>true if CustomerList property is set.</returns>
        public bool IsSetCustomerList()
        {
            return this.CustomerList.Count > 0;
        }

        /// <summary>
        /// Gets and sets the NextToken property.
        /// </summary>
        public string NextToken
        {
            get { return this._nextToken; }
            set { this._nextToken = value; }
        }

        /// <summary>
        /// Sets the NextToken property.
        /// </summary>
        /// <param name="nextToken">NextToken property.</param>
        /// <returns>this instance.</returns>
        public ListCustomersByNextTokenResult WithNextToken(string nextToken)
        {
            this._nextToken = nextToken;
            return this;
        }

        /// <summary>
        /// Checks if NextToken property is set.
        /// </summary>
        /// <returns>true if NextToken property is set.</returns>
        public bool IsSetNextToken()
        {
            return this._nextToken != null;
        }

        /// <summary>
        /// Gets and sets the MoreResultsAvailable property.
        /// </summary>
        public bool MoreResultsAvailable
        {
            get { return this._moreResultsAvailable; }
            set { this._moreResultsAvailable = value; }
        }

        /// <summary>
        /// Sets the MoreResultsAvailable property.
        /// </summary>
        /// <param name="moreResultsAvailable">MoreResultsAvailable property.</param>
        /// <returns>this instance.</returns>
        public ListCustomersByNextTokenResult WithMoreResultsAvailable(bool moreResultsAvailable)
        {
            this._moreResultsAvailable = moreResultsAvailable;
            return this;
        }

        /// <summary>
        /// Checks if MoreResultsAvailable property is set.
        /// </summary>
        /// <returns>true if MoreResultsAvailable property is set.</returns>
        public bool IsSetMoreResultsAvailable()
        {
            return this._moreResultsAvailable != null;
        }


        public override void ReadFragmentFrom(IMwsReader reader)
        {
            this._customerList = reader.ReadList<Customer>("CustomerList", "Customer");
            this._nextToken = reader.Read<string>("NextToken");
            this._moreResultsAvailable = reader.Read<bool>("MoreResultsAvailable");
        }

        public override void WriteFragmentTo(IMwsWriter writer)
        {
            writer.WriteList("CustomerList", "Customer", this._customerList);
            writer.Write("NextToken", this._nextToken);
            writer.Write("MoreResultsAvailable", this._moreResultsAvailable);
        }

        public override void WriteTo(IMwsWriter writer)
        {
            writer.Write("http://mws.amazonservices.com/schema/CustomerInformation/2014-03-01", "ListCustomersByNextTokenResult", this);
        }

        public ListCustomersByNextTokenResult() : base()
        {
        }
    }
}
