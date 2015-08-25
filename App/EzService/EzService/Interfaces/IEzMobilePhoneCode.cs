using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzService.Interfaces {
    using System.ServiceModel;

    public interface IEzMobilePhoneCode {
        [OperationContract]
        BoolActionResult GenerateMobileCode(string phone);

        [OperationContract]
        BoolActionResult ValidateMobileCode(string phone, string code);

        [OperationContract]
        BoolActionResult SendSms(int userId, int underwriterId, string phone, string content);
    }
}
