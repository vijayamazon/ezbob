using System;
using System.Threading;
using SalesForceLib.Models;

namespace SalesForceMigrationTool {
	using Ezbob.Database;
	using log4net;
	using SalesForceLib;

	internal class SalesForceReruner {

		public SalesForceReruner(ISalesForceAppClient sfClient, AConnection db) {
			this.sfClient = sfClient;
			this.DB = db;
		}

		public void Rerun() {
			DB.ForEachRowSafe(RerunOneSalesForce, "GetSalesForceForRerun", CommandSpecies.StoredProcedure);
		}

		private ActionResult RerunOneSalesForce(SafeReader sf, bool bRowSetStart) {
			try {
				int salesForceLogID = sf["SalesForceLogID"];
				string typeStr = sf["Type"];
				string modelStr = sf["Model"];

				SalesForceType type = typeStr.ParseEnum<SalesForceType>();
				switch (type) {
					case SalesForceType.AddActivity:
						var aModel = modelStr.JsonStringToObject<ActivityModel>();
						this.sfClient.CreateActivity(aModel);


						break;
					case SalesForceType.AddOpportunity:
						var oModel = modelStr.JsonStringToObject<OpportunityModel>();
						if (string.IsNullOrEmpty(oModel.Name))
						{
							oModel.Name = oModel.Email + "rerun";
						}
						this.sfClient.CreateOpportunity(oModel);
						break;
					case SalesForceType.AddTask:
						var tModel = modelStr.JsonStringToObject<TaskModel>();
						this.sfClient.CreateTask(tModel);
						break;
					case SalesForceType.AddUpdateContact:
						var cModel = modelStr.JsonStringToObject<ContactModel>();
						this.sfClient.CreateUpdateContact(cModel);
						break;
					case SalesForceType.AddUpdateLeadAccount:
						var lModel = modelStr.JsonStringToObject<LeadAccountModel>();
						if (!string.IsNullOrEmpty(lModel.LeadSource) && lModel.LeadSource.Length > 40)
						{
							lModel.LeadSource = lModel.LeadSource.Substring(0, 40);
						}
						this.sfClient.CreateUpdateLeadAccount(lModel);
						break;
					case SalesForceType.UpdateOpportunity:
						var o2Model = modelStr.JsonStringToObject<OpportunityModel>();
						this.sfClient.UpdateOpportunity(o2Model);
						break;
					default:
						Log.Warn("Unknown type");
						break;
				}

				UpdateStatus(salesForceLogID);
				Thread.Sleep(100);
			} catch(Exception ex) {
				Log.Error("Failed to rerun " + sf["SalesForceLogID"], ex);
				//
			}
			return ActionResult.Continue;
		}

		private void UpdateStatus(int salesForceLogID)
		{
			DB.ExecuteNonQuery("UpdateSalesForceRerunStatus", CommandSpecies.StoredProcedure,
				new QueryParameter("@SalesForceLogID", salesForceLogID),
				new QueryParameter("Now", DateTime.UtcNow),
				new QueryParameter("RerunSuccess", !this.sfClient.HasError),
				new QueryParameter("Error", this.sfClient.Error)
				);
		}

		private readonly ISalesForceAppClient sfClient;
		private readonly AConnection DB;
		protected static ILog Log = LogManager.GetLogger(typeof(SalesForceReruner));
	}

	public enum SalesForceType {
		AddActivity,
		AddOpportunity,
		AddTask,
		AddUpdateContact,
		AddUpdateLeadAccount,
		UpdateOpportunity
	}
}
