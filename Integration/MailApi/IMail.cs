using System.Collections.Generic;

namespace MailApi
{
    public interface IMail
    {
        void SendMessageFinishWizard(string emailTo, string fullName);
        string Send(Dictionary<string,string> parameters, string to, string templateName, string subject = "", string cc = "");
    }
}
