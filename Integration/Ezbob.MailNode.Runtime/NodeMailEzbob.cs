using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
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
        

        public NodeMailEzbob(string initialValue) : base(initialValue)
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
            get
            {
                return NodeMailParams.TypeName;
            }
        }
       
    }
}
