using System.Collections.Generic;
using System.Web.Mvc;
using EZBob.DatabaseLib.Model.Database;
using NHibernate;
using NHibernate.Criterion;
using PluginWebApp.Code.jqGrid;
using Scorto.PluginWeb.Core.jqGrid;

namespace EzBob.Web.Code
{
	using System.Linq;
	using EZBob.DatabaseLib;
	using StructureMap;

	public class UnderwriterGridResult : GridCriteriaResult<Customer>
	{
		private bool _isTest;

		public UnderwriterGridResult(ISession session, IEnumerable<KeyValuePair<string, string>> aliases, GridModel<Customer> gridModel, GridSettings settings)
			: base(session, aliases, gridModel, settings)
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
				criteria.Add(
									Restrictions.Or(Restrictions.Eq("IsTest", false),
									Restrictions.IsNull("IsTest"))
					);
			}
			criteria.Add(Restrictions.IsNotNull("PersonalInfo"));
			var dbHelper = ObjectFactory.GetInstance<DatabaseDataHelper>();
			var ws = dbHelper.WizardSteps.GetAll().FirstOrDefault(x => x.TheLastOne);
			if (ws != null)
			{
				criteria.Add(Restrictions.Eq("WizardStep", ws));
			}
			criteria.Add(Restrictions.IsNotEmpty("CashRequests"));
			base.OnCustomizeFilter(criteria);
		}
	}
}