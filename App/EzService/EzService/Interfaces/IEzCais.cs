using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzService.Interfaces
{
    using System.ServiceModel;

    public interface IEzCais
    {
        [OperationContract]
        ActionMetaData CaisGenerate(int underwriterId);

        [OperationContract]
        ActionMetaData CaisUpdate(int userId, int caisId);
    }
}
