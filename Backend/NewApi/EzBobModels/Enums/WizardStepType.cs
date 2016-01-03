using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels.Enums
{
    public enum WizardStepType
    {
        SignUp = 1,
        Marketplace = 2,
        PaymentAccounts = 3, // just because there are some customers in that state in DB
        AllStep = 4,
        PersonalDetails = 5,
        CompanyDetails = 6,
    }
}
