using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobRest.Modules.Company.Validators
{
    /// <summary>
    /// Previous 
    /// </summary>
    class PreviousLivingAddressValidator : LivingAddressValidator
    {
        public PreviousLivingAddressValidator()
            : base("[Previous living address]: ") { }
    }
}
