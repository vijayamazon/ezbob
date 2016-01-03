using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobService
{
    using EzBobCommon;

    public class InfoAccomulatorWithPayload <T> : InfoAccumulator
    {
        public T PayLoad { get; set; }
    }
}
