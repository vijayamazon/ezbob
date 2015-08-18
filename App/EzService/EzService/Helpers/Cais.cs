using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzService.Helpers
{
    using Ezbob.Backend.Strategies.Misc;
    using EzService.Interfaces;

    internal class Cais : Executor, IEzCais
    {
        public Cais(EzServiceInstanceRuntimeData oData)
            : base(oData) {}

        public ActionMetaData CaisGenerate(int underwriterId)
        {
            return Execute<CaisGenerate>(null, underwriterId, underwriterId);
        }

        public ActionMetaData CaisUpdate(int userId, int caisId)
        {
            return Execute<CaisUpdate>(null, userId, caisId);
        } 
    }
}
