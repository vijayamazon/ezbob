using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using EzBob.Signals.RenderAgreements;
using EzBob.Web.Areas.Customer.Models;
using Newtonsoft.Json;
using Scorto.Flow.Signal;

namespace EzBob.Signals.RenderConcentAgreement
{
    [Serializable]
    public class RenderConcentAgreementsMessage : ManagementSignalBase, ISerializable
    {
        private List<AgreementItem> _items = new List<AgreementItem>();
        public AgreementModel CustomerData { get; set; }

        public List<AgreementItem> Items
        {
            get { return _items; }
            set { _items = value; }
        }

        
        public RenderConcentAgreementsMessage()
        {

        }


        public RenderConcentAgreementsMessage(SerializationInfo info, StreamingContext context)
        {
            CustomerData = JsonConvert.DeserializeObject<AgreementModel>(info.GetString("CustomerData"));
            Items = JsonConvert.DeserializeObject<List<AgreementItem>>(info.GetString("Items"));
        }
        
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("CustomerData", JsonConvert.SerializeObject(CustomerData));
            info.AddValue("Items", JsonConvert.SerializeObject(Items));
        }

        public void AddAgreement(string name, string template, string filename)
        {
            _items.Add(new AgreementItem(name, template, filename));
        }
    }
}