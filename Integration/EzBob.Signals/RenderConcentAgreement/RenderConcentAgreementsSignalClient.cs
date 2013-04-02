using ApplicationMng.Signal;
using EzBob.Web.Areas.Customer.Models;
using Scorto.Flow.Signal;

namespace EzBob.Signals.RenderConcentAgreement
{
    public class RenderConcentAgreementsSignalClient : SignalClient<RenderConcentAgreementsMessage>
    {
        private readonly RenderConcentAgreementsMessage _message = new RenderConcentAgreementsMessage();

        public RenderConcentAgreementsSignalClient(AgreementModel data)
        {
            _message.CustomerData = data;
        }

        protected override string GetMessageLabel(RenderConcentAgreementsMessage message)
        {
            return "Rendering Concent Agreements";
        }

        public override void Execute(long applicationId, PriorityInfo priorityInfo)
        {
            PerformReaction(_message, applicationId, priorityInfo);
        }

        public void AddAgreement(string name, string template, string filename)
        {
            _message.AddAgreement(name, template, filename);
        }
    }
}