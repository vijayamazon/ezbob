using System.Linq;
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
            var isMandrillEnable = VariablesRepository.GetByName("MandrillEnable").Value == "Yes";
            if (isMandrillEnable)
            {
                var variables = (iworkflow.VariableConnectionDescriptors.Where(
                    vc => vc.TargetVariableOwnerName == _ec.CurrentNodeName))
                    .ToLookup(k => k.SourceVariableName, k => _ec[k.SourceVariableName].ToString())
                    .Distinct()
                    .ToDictionary(k => k.Key, v => v.First());
                var templateName = MailTemplateRelationRepository.GetByInternalName(Templates[0].DisplayName);
                var subject = variables["EmailSubject"] ?? "Default Subject";
                var to = variables["email"];
                var cc = variables["emailCC"];

                return Mail.Send(variables, to, templateName, subject, cc);
            }
            return base.Execute(iworkflow);
        }
    }
}