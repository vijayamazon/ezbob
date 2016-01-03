using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels.Enums
{
    public enum LandRegistryRequestType
    {
        None = 0,
        Enquiry = 1,//EnquiryByPropertyDescription
        EnquiryPoll = 2,//EnquiryByPropertyDescriptionPoll
        Res = 3,//RegisterExtractService
        ResPoll = 4,//RegisterExtractServicePoll
    }
}
