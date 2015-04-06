﻿namespace Ezbob.Backend.Strategies.SalesForce {
	using SalesForceLib;
	using SalesForceLib.Models;
	using StructureMap;

	public class AddActivity : AStrategy {

		public AddActivity(int? customerID, ActivityModel model) {
			this.salesForce = ObjectFactory
				.With("userName").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceUserName.Value)
				.With("password").EqualTo(ConfigManager.CurrentValues.Instance.SalesForcePassword.Value)
				.With("token").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceToken.Value)
				.With("environment").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceEnvironment.Value)
				.GetInstance<ISalesForceAppClient>();
			this.customerID = customerID;
			this.eventModel = model;
		}
		public override string Name { get { return "AddEvent"; } }

		public override void Execute() {
            if (this.customerID.HasValue) {
                Log.Info("Adding SalesForce event {1} to customer {0} ", this.eventModel.Type, this.customerID.Value);
			}

            this.eventModel.Email = this.eventModel.Email.ToLower();
            this.salesForce.CreateActivity(this.eventModel);
		}
		private readonly ISalesForceAppClient salesForce;
		private readonly int? customerID;
		private readonly ActivityModel eventModel;
	}
}
