using ApplicationMng.Signal;
using EzBob.Web.Areas.Customer.Models;
using Scorto.Flow.Signal;

namespace EzBob.Signals.RenderAgreements
{
    public class RenderAgreementsSignalClient : SignalClient<RenderAgreementsMessage>
    {

        private RenderAgreementsMessage _msg = new RenderAgreementsMessage();

        public RenderAgreementsSignalClient(AgreementModel model, string refNumber)
        {
            _msg.CustomerData = model;
            _msg.RefNumber = refNumber;
        }

        protected override string GetMessageLabel(RenderAgreementsMessage message)
        {
            return "Rendering Agreements";
        }

        public override void Execute(long applicationId, PriorityInfo priorityInfo)
        {
            PerformReaction(_msg, applicationId, priorityInfo);
        }

        public void AddAgreement(string name, string template, string filename)
        {
            _msg.AddAgreement(name, template, filename);
        }
    }
}