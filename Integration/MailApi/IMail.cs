using System.Collections.Generic;

namespace MailApi
{
	using Model;

	public interface IMail
    {
        string Send(Dictionary<string,string> parameters, string to, string templateName, string subject = "", string cc = "", List<attachment> attachments = null);
        /// <summary>
        /// Send message without template
        /// </summary>
        /// <param name="to"></param>
        /// <param name="text">Also can be html</param>
        /// <param name="subject"></param>
        /// <returns></returns>
        string Send(string to, string text, string subject);
        string GetRenderedTemplate(Dictionary<string, string> parameters, string templateName);
    }
}
