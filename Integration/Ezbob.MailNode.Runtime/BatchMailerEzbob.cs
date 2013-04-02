using System.Collections.Generic;
using Scorto.AutoNodes.NodeMail;
using Scorto.Email.Senders;
using Scorto.Export.Templates;

namespace Ezbob.MailNode.Runtime
{
    public class BatchMailerEzbob : BatchMailer
    {
        protected override IEnumerable<EmailAttachment> SetAttachments(IEnumerable<ExportResult> results)
        {
            return new LinkedList<EmailAttachment>();
        }
    }
}
