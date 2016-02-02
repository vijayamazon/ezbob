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
 * Message List
 * API Version: 2011-10-01
 * Library Version: 2015-09-01
 * Generated: Thu Sep 10 06:52:19 PDT 2015
 */

namespace EzBob3dParties.Amazon.Src.ProductsApi.Model
{
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using EzBob3dParties.Amazon.Src.Common;

    [XmlType(Namespace = "http://mws.amazonservices.com/schema/Products/2011-10-01")]
    [XmlRoot(Namespace = "http://mws.amazonservices.com/schema/Products/2011-10-01", IsNullable = false)]
    public class MessageList : AbstractMwsObject
    {

        private List<Message> _message;

        /// <summary>
        /// Gets and sets the Message property.
        /// </summary>
        [XmlElement(ElementName = "Message")]
        public List<Message> Message
        {
            get
            {
                if(this._message == null)
                {
                    this._message = new List<Message>();
                }
                return this._message;
            }
            set { this._message = value; }
        }

        /// <summary>
        /// Sets the Message property.
        /// </summary>
        /// <param name="message">Message property.</param>
        /// <returns>this instance.</returns>
        public MessageList WithMessage(Message[] message)
        {
            this._message.AddRange(message);
            return this;
        }

        /// <summary>
        /// Checks if Message property is set.
        /// </summary>
        /// <returns>true if Message property is set.</returns>
        public bool IsSetMessage()
        {
            return this.Message.Count > 0;
        }


        public override void ReadFragmentFrom(IMwsReader reader)
        {
            this._message = reader.ReadList<Message>("Message");
        }

        public override void WriteFragmentTo(IMwsWriter writer)
        {
            writer.WriteList("Message", this._message);
        }

        public override void WriteTo(IMwsWriter writer)
        {
            writer.Write("http://mws.amazonservices.com/schema/Products/2011-10-01", "MessageList", this);
        }

        public MessageList() : base()
        {
        }
    }
}
