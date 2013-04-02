using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using EzBob.Web.Areas.Customer.Models;
using Newtonsoft.Json;
using Scorto.Flow.Signal;

namespace EzBob.Signals.RenderAgreements
{
    [Serializable]
    public class RenderAgreementsMessage : ManagementSignalBase, ISerializable
    {
        public RenderAgreementsMessage()
        {
        }
        
        public RenderAgreementsMessage(SerializationInfo info, StreamingContext context)
        {
            CustomerData = JsonConvert.DeserializeObject<AgreementModel>(info.GetString("CustomerData"));
            Items = JsonConvert.DeserializeObject<List<AgreementItem>>(info.GetString("Items"));
            RefNumber = info.GetString("RefNumber");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("CustomerData", JsonConvert.SerializeObject(CustomerData));
            info.AddValue("Items", JsonConvert.SerializeObject(Items));
            info.AddValue("RefNumber", RefNumber);
        }

        private List<AgreementItem> _items = new List<AgreementItem>();
        public AgreementModel CustomerData { get; set; }

        public List<AgreementItem> Items
        {
            get { return _items; }
            set { _items = value; }
        }

        public string RefNumber { get; set; }

        public void AddAgreement(string name, string template, string filename)
        {
            _items.Add(new AgreementItem(name, template, filename));
        }
    }
}