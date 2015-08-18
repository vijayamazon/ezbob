using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzService.Interfaces
{
    using System.ServiceModel;
    using Ezbob.Backend.Models;
    using EzBob.Backend.Models;

    public interface IEzAgreement
    {
        [OperationContract]
        ActionMetaData SaveAgreement(
            int customerId,
            AgreementModel model,
            string refNumber,
            string name,
            TemplateModel template,
            string path1,
            string path2
            );
    }
}
