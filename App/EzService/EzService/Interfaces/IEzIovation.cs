using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzService.Interfaces {
    using System.ServiceModel;
    using Ezbob.Backend.Models;

    public interface IEzIovation {
        [OperationContract]
        ActionMetaData IovationCheck(IovationCheckModel model);
    }
}
