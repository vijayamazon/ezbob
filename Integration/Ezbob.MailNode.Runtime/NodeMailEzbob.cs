using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Aspose.Words;
using EZBob.DatabaseLib.Model;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using MailApi;
using NHibernate;
using StructureMap.Attributes;
using ExportResult = Scorto.Export.Templates.ExportResult;

namespace WorkflowObjects
{
    public class NodeMailEzbob : NodeMail
    {
        [SetterProperty]
        public EzbobMailNodeAttachRelationRepository ExportResultsRepo { get; set; }

        [SetterProperty]
        public ISession Session { get; set; }

        [SetterProperty]
        public IConfigurationVariablesRepository VariablesRepository { get; set; }

        [SetterProperty]
        public IMailTemplateRelationRepository MailTemplateRelationRepository { get; set; }

        [SetterProperty]
        public IMail Mail { get; set; }


        public NodeMailEzbob(string initialValue)
            : base(initialValue)
        {
        }

        public override int AddExportresult(ExportResult result)
        {
            //skip saving if filetype is pdf
            if (result.FileType == 1)
            {
                return 0;
            }
            var exportresultId = base.AddExportresult(result);

            var model = new EzbobMailNodeAttachRelation
                {
                    To = NodeMailParams.To,
                    Export = Session.Load<EZBob.DatabaseLib.Model.Database.ExportResult>(exportresultId)
                };
            ExportResultsRepo.Save(model);

            return exportresultId;
        }

        public override string TypeName
        {
            get { return NodeMailParams.TypeName; }
        }

        /// <summary>
        /// Send mail with internal mail node or mandrill
        /// </summary>
        /// <param name="iworkflow"></param>
        /// <returns></returns>
        public override string Execute(IWorkflow iworkflow)
        {
            var isMandrillEnable = VariablesRepository.GetByName("MandrillEnable").Value.ToLower() == "yes";
            var isGreetingMailSendViaMandrill = VariablesRepository.GetByName("GreetingMailSendViaMandrill").Value.ToLower() == "yes";

            if (isMandrillEnable || (isGreetingMailSendViaMandrill && Templates[0].DisplayName == "Thanks for joining us.docx"))
            {
                var variables = (iworkflow.VariableConnectionDescriptors.Where(
                    vc => vc.TargetVariableOwnerName == _ec.CurrentNodeName))
                    .ToLookup(k => k.SourceVariableName, k => Convert.ToString(_ec[k.SourceVariableName]))
                    .Distinct()
                    .ToDictionary(k => k.Key, v => v.First());

                NodeMailParams.Subject = variables.FirstOrDefault(x => x.Key == "EmailSubject" || x.Key == "Subject").Value ?? "Default Subject";
                NodeMailParams.To = variables.FirstOrDefault(x => x.Key == "CP_AddressTo" ||x.Key == "email" ).Value;
                NodeMailParams.CC = variables.FirstOrDefault(x => x.Key == "CP_AddressCC" || x.Key == "emailCC").Value;

                var templateName = MailTemplateRelationRepository.GetByInternalName(Templates[0].DisplayName);
                var sendStatus = Mail.Send(variables, NodeMailParams.To, templateName, NodeMailParams.Subject, NodeMailParams.CC);
                var renderedHtml = Mail.GetRenderedTemplate(variables, templateName);

                if (sendStatus == null || renderedHtml == null)
                {
                    return "Next";
                }

                //save mandrill rendered template into DB export result
                var exportResult = new ExportResult
                    {
                        ApplicationID = iworkflow.ApplicationId,
                        CreationDate = DateTime.UtcNow,
                        FileName = string.Format("{0}({1}).docx", 
                        NodeMailParams.Subject,
                        DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture)),
                        FileType = 0,
                        BinaryBody = HtmlToDocxBinnary(renderedHtml),
                        NodeName = iworkflow.CurrentNodeName
                    };
                AddExportresult(exportResult);
                return "Next";
            }
            return base.Execute(iworkflow);
        }

        private static byte[] HtmlToDocxBinnary(string html)
        {
            var doc = new Document();
            var docBuilder = new DocumentBuilder(doc);
            docBuilder.InsertHtml(html);

            using (var streamForDoc = new MemoryStream())
            {
                doc.Save(streamForDoc, SaveFormat.Docx);
                return streamForDoc.ToArray();
            }
        }
    }
}