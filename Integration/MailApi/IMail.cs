namespace MailApi
{
	using System.Collections.Generic;
	using Model;

	public interface IMail
    {
        string Send(Dictionary<string,string> parameters, string to, string templateName, string subject = "", string cc = "", List<attachment> attachments = null);
        string GetRenderedTemplate(Dictionary<string, string> parameters, string templateName);
    }
}
