/******************************************************************************* 
 *  Copyright 2008-2009 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 *  Licensed under the Apache License, Version 2.0 (the "License"); 
 *  
 *  You may not use this file except in compliance with the License. 
 *  You may obtain a copy of the License at: http://aws.amazon.com/apache2.0
 *  This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
 *  CONDITIONS OF ANY KIND, either express or implied. See the License for the 
 *  specific language governing permissions and limitations under the License.
 * ***************************************************************************** 
 * 
 *  Marketplace Web Service Orders CSharp Library
 *  API Version: 2011-01-01
 *  Generated: Fri Nov 04 00:50:29 GMT 2011 
 * 
 */


using System;
using System.Xml.Serialization;
using System.Text;

namespace MarketplaceWebServiceOrders.MarketplaceWebServiceOrders.Model
{
    [XmlTypeAttribute(Namespace = "https://mws.amazonservices.com/Orders/2011-01-01")]
    [XmlRootAttribute(Namespace = "https://mws.amazonservices.com/Orders/2011-01-01", IsNullable = false)]
    public class PaymentExecutionDetailItem
    {
    
        private  Money paymentField;
        private String subPaymentMethodField;


        /// <summary>
        /// Gets and sets the Payment property.
        /// </summary>
        [XmlElementAttribute(ElementName = "Payment")]
        public Money Payment
        {
            get { return this.paymentField ; }
            set { this.paymentField = value; }
        }



        /// <summary>
        /// Sets the Payment property
        /// </summary>
        /// <param name="payment">Payment property</param>
        /// <returns>this instance</returns>
        public PaymentExecutionDetailItem WithPayment(Money payment)
        {
            this.paymentField = payment;
            return this;
        }



        /// <summary>
        /// Checks if Payment property is set
        /// </summary>
        /// <returns>true if Payment property is set</returns>
        public Boolean IsSetPayment()
        {
            return this.paymentField != null;
        }




        /// <summary>
        /// Gets and sets the SubPaymentMethod property.
        /// </summary>
        [XmlElementAttribute(ElementName = "SubPaymentMethod")]
        public String SubPaymentMethod
        {
            get { return this.subPaymentMethodField ; }
            set { this.subPaymentMethodField= value; }
        }



        /// <summary>
        /// Sets the SubPaymentMethod property
        /// </summary>
        /// <param name="subPaymentMethod">SubPaymentMethod property</param>
        /// <returns>this instance</returns>
        public PaymentExecutionDetailItem WithSubPaymentMethod(String subPaymentMethod)
        {
            this.subPaymentMethodField = subPaymentMethod;
            return this;
        }



        /// <summary>
        /// Checks if SubPaymentMethod property is set
        /// </summary>
        /// <returns>true if SubPaymentMethod property is set</returns>
        public Boolean IsSetSubPaymentMethod()
        {
            return  this.subPaymentMethodField != null;

        }




        /// <summary>
        /// XML fragment representation of this object
        /// </summary>
        /// <returns>XML fragment for this object.</returns>
        /// <remarks>
        /// Name for outer tag expected to be set by calling method. 
        /// This fragment returns inner properties representation only
        /// </remarks>


        protected internal String ToXMLFragment() {
            StringBuilder xml = new StringBuilder();
            if (IsSetPayment()) {
                Money  paymentObj = this.Payment;
                xml.Append("<Payment>");
                xml.Append(paymentObj.ToXMLFragment());
                xml.Append("</Payment>");
            } 
            if (IsSetSubPaymentMethod()) {
                xml.Append("<SubPaymentMethod>");
                xml.Append(EscapeXML(this.SubPaymentMethod));
                xml.Append("</SubPaymentMethod>");
            }
            return xml.ToString();
        }

        /**
         * 
         * Escape XML special characters
         */
        private String EscapeXML(String str) {
            StringBuilder sb = new StringBuilder();
            foreach (Char c in str)
            {
                switch (c) {
                case '&':
                    sb.Append("&amp;");
                    break;
                case '<':
                    sb.Append("&lt;");
                    break;
                case '>':
                    sb.Append("&gt;");
                    break;
                case '\'':
                    sb.Append("&#039;");
                    break;
                case '"':
                    sb.Append("&quot;");
                    break;
                default:
                    sb.Append(c);
                    break;
                }
            }
            return sb.ToString();
        }



    }

}