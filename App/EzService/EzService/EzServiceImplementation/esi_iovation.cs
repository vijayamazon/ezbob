namespace EzService.EzServiceImplementation {
    using Ezbob.Backend.Models;
    using Ezbob.Backend.Strategies.Iovation;

    partial class EzServiceImplementation : IEzService {
        public ActionMetaData IovationCheck(IovationCheckModel model) {
            return Execute<IovationCheck>(model.CustomerID, null, model);
        }
    }
}
