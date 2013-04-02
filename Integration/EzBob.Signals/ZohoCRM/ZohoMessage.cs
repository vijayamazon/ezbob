using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Scorto.Flow.Signal;
using ZohoCRM;

namespace EzBob.Signals.ZohoCRM
{
    [Serializable]
    public class ZohoMessage : ManagementSignalBase, ISerializable
    {
        public int CustomerId { get; set; }
        public ZohoMethodType MethodType { get; set; }
        
        public ZohoMessage()
        {
        }

        public ZohoMessage(SerializationInfo info, StreamingContext context)
        {
            CustomerId = JsonConvert.DeserializeObject<int>(info.GetString("CustomerId"));
            MethodType = JsonConvert.DeserializeObject<ZohoMethodType>(info.GetString("MethodType"));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("CustomerId", JsonConvert.SerializeObject(CustomerId));
            info.AddValue("MethodType", JsonConvert.SerializeObject(MethodType));
        }
    }
}