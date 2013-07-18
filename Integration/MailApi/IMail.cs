using System.Collections.Generic;

namespace MailApi
{
    public interface IMail
    {
        string Send(Dictionary<string,string> parameters, string to, string templateName, string subject = "", string cc = "");
        string GetRenderedTemplate(Dictionary<string, string> parameters, string templateName);
    }
}
