using ApplicationMng.Signal;
using Scorto.Flow.Signal;
using ZohoCRM;

namespace EzBob.Signals.ZohoCRM
{
    public class ZohoSignalClient : SignalClient<ZohoMessage>
    {
        private readonly ZohoMessage _msg = new ZohoMessage();

        public ZohoSignalClient(int customerId, ZohoMethodType methodType = ZohoMethodType.UpdateOrCreate)
        {
            _msg.CustomerId = customerId;
            _msg.MethodType = methodType;
        }

        protected override string GetMessageLabel(ZohoMessage message)
        {
            return "Zoho CRM update";
        }

        public override void Execute(long applicationId, PriorityInfo priorityInfo)
        {
            PerformReaction(_msg, applicationId, priorityInfo);
        }
        public void Execute()
        {
            Execute(0, new PriorityInfo(null, null));
        }
    }
}