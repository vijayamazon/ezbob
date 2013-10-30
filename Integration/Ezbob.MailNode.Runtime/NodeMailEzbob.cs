namespace WorkflowObjects
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using Aspose.Words;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Loans;
	using EzBob.Web.Areas.Customer.Models;
	using EzBob.Web.Code.Agreements;
	using MailApi;
	using MailApi.Model;
	using NHibernate;
	using Newtonsoft.Json;
	using StructureMap.Attributes;
	using ExportResult = Scorto.Export.Templates.ExportResult;
	using log4net;

	public class NodeMailEzbob : NodeMail
    {
        [SetterProperty]
        public EzbobMailNodeAttachRelationRepository ExportResultsRepo { get; set; }

        [SetterProperty]
        public ISession Session { get; set; }

		[SetterProperty]
		public ILoanAgreementRepository LoanAgreementRepository { get; set; }

		public AgreementRenderer AgreementRenderer;

        [SetterProperty]
        public IConfigurationVariablesRepository VariablesRepository { get; set; }

        [SetterProperty]
        public IMailTemplateRelationRepository MailTemplateRelationRepository { get; set; }

        [SetterProperty]
		public IMail Mail { get; set; }

		private static readonly ILog log = LogManager.GetLogger(typeof(NodeMailEzbob));


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
			var isLateBy14DaysMailSendViaMandrill = VariablesRepository.GetByName("LateBy14DaysMailSendViaMandrill").Value.ToLower() == "yes";

            if (isMandrillEnable || 
				(isGreetingMailSendViaMandrill && Templates[0].DisplayName == "Thanks for joining us.docx") ||
				(isLateBy14DaysMailSendViaMandrill && Templates[0].DisplayName == "Late by 14 days.docx") ||
				Templates[0].DisplayName == "Congratulations you are qualified.docx" ||
				Templates[0].DisplayName == "Congratulations you are qualified - not first.docx" ||
				Templates[0].DisplayName == "Congratulations you are qualified - offline.docx" ||
				Templates[0].DisplayName == "Congratulations you are qualified - offline - not first.docx" ||
				Templates[0].DisplayName == "Get cash - approval.docx" ||
				Templates[0].DisplayName == "Get cash - approval - not first.docx")
            {
				var variables = new Dictionary<string, string>();
				foreach (VariableConnectionDescriptor variable in iworkflow.VariableConnectionDescriptors.Where(vc => vc.TargetVariableOwnerName == _ec.CurrentNodeName))
				{
					if (!variables.ContainsKey(variable.SourceVariableName))
					{
						variables.Add(variable.SourceVariableName, Convert.ToString(_ec[variable.SourceVariableName]));
					}
					string target;
					if (variable.TargetVariableName.ToLower() == "mp_counter" ||
						variable.TargetVariableName.ToLower() == "updatecmp_error" ||
						variable.TargetVariableName.ToLower() == "cp_addressto" ||
						variable.TargetVariableName.ToLower() == "cp_addresscc")
					{
						target = variable.TargetVariableName;
					}
					else
					{
						target = variable.TargetVariableName.Split(new[] { '_' })[0];
					}

					if (!variables.ContainsKey(target))
					{
						variables.Add(target, Convert.ToString(_ec[variable.SourceVariableName]));
					}
				}

				log.InfoFormat("Going to send template:'{0}' via Mandrill. Will use the following variables:", Templates[0].DisplayName);
				foreach (var variable in variables)
				{
					log.InfoFormat("Key:'{0}' Value:'{1}'", variable.Key, variable.Value);
				}

				NodeMailParams.Subject = variables.FirstOrDefault(x => x.Key == "EmailSubject" || x.Key.ToLower() == "subject").Value ?? "Default Subject";
				NodeMailParams.To = variables.FirstOrDefault(x => x.Key == "CP_AddressTo" || x.Key.ToLower() == "email" || x.Key.ToLower() == "emailto" || x.Key.ToLower() == "emailscalar" || x.Key.ToLower() == "app_email").Value;
                NodeMailParams.CC = variables.FirstOrDefault(x => x.Key == "CP_AddressCC" || x.Key.ToLower() == "emailcc").Value;

	            List<attachment> attachments = HandleAttachments();

				var templateName = MailTemplateRelationRepository.GetByInternalName(Templates[0].DisplayName);
				var sendStatus = Mail.Send(variables, NodeMailParams.To, templateName, NodeMailParams.Subject, NodeMailParams.CC, attachments);
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
                        NodeName = iworkflow.CurrentNodeName,
                    };
                AddExportresult(exportResult);
                return "Next";
            }

	        return base.Execute(iworkflow);
        }

		private List<attachment> HandleAttachments()
		{
			int loanId;
			List<attachment> attachments = null;
			if (int.TryParse(NodeMailParams.CC, out loanId))
			{
				NodeMailParams.CC = "";
				var loanAgreements = LoanAgreementRepository.GetByLoanId(loanId);
				Log.DebugFormat("loanAgreements {0}", loanAgreements.Count());
				attachments = new List<attachment>();
				foreach (var loanAgreement in loanAgreements)
				{
					AgreementRenderer = new AgreementRenderer();
					var content = AgreementRenderer.AggrementToBase64String(loanAgreement.TemplateRef.Template,
					                                                        JsonConvert.DeserializeObject<AgreementModel>(
						                                                        loanAgreement.Loan.AgreementModel));
					var name = loanAgreement.ShortFilename();
					Log.DebugFormat("Adding attachment {0} loanId {2} loanAgreementId {1}", name, loanAgreement.Id, loanId);
					attachments.Add(new attachment
						{
							content = content,
							name = name,
							type = "application/pdf"
						});
				}
			}
			return attachments;
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