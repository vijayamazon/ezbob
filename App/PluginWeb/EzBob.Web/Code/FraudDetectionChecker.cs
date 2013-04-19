using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Repository;
using StructureMap;

namespace EzBob.Web.Code
{
    public class FraudDetectionChecker
    {
        private readonly FraudUserRepository _fraudUserRepository;
        private readonly FraudDetectionRepository _fraudDetectioRepository;

        public FraudDetectionChecker()
        {
            _fraudUserRepository = ObjectFactory.GetInstance<FraudUserRepository>();
            _fraudDetectioRepository = ObjectFactory.GetInstance<FraudDetectionRepository>();
        }

        public string ExternalSystemDecision(Customer customer)
        {
            return "";
        }

        public string InternalSystemDecision(Customer customer)
        {
            return "";
        }
    }
}