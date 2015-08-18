using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzService.Helpers
{
    using Ezbob.Backend.Models;
    using Ezbob.Backend.Strategies.Misc;
    using EzBob.Backend.Models;
    using EzService.Interfaces;

    internal class Agreement : Executor, IEzAgreement
    {
        public Agreement(EzServiceInstanceRuntimeData oData)
            : base(oData) {}
        public ActionMetaData SaveAgreement(int customerId, AgreementModel model, string refNumber, string name, TemplateModel template, string path1, string path2) {
            return Execute<SaveAgreement>(customerId, null, customerId, model, refNumber, name, template, path1, path2);
        }
    }
}
