using System.Collections.Generic;
using System.Web.Mvc;
using EZBob.DatabaseLib.Model.Database;
using NHibernate;
using NHibernate.Criterion;
using PluginWebApp.Code.jqGrid;
using Scorto.PluginWeb.Core.jqGrid;

namespace EzBob.Web.Code
{
    public class UnderwriterGridResult : GridCriteriaResult<EZBob.DatabaseLib.Model.Database.Customer>
    {
        private bool _isTest;

        public UnderwriterGridResult(ISession session, IEnumerable<KeyValuePair<string, string>> aliases, GridModel<Customer> gridModel, GridSettings settings) : base(session, aliases, gridModel, settings)
        {
        }

        public override void ExecuteResult(ControllerContext context)
        {
            _isTest = context.RequestContext.HttpContext.Request.Params["IsTest"] == "true";
            base.ExecuteResult(context);
        }

        protected override void OnCustomizeFilter(ICriteria criteria)
        {
            if (!_isTest)
            {
                criteria.Add (
                                    Restrictions.Or(Restrictions.Eq("IsTest", false),
                                    Restrictions.IsNull("IsTest"))
                    );
            }
            criteria.Add(Restrictions.IsNotNull("PersonalInfo"));
            criteria.Add(Restrictions.Eq("IsSuccessfullyRegistered", true));
            criteria.Add(Restrictions.IsNotEmpty("CashRequests"));
            base.OnCustomizeFilter(criteria);
        }
    }
}