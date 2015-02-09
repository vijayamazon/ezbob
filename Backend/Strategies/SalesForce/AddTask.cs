﻿namespace Ezbob.Backend.Strategies.SalesForce {
	using SalesForceLib;
	using SalesForceLib.Models;
	using StructureMap;

	public class AddTask : AStrategy {

		public AddTask(int? customerID, TaskModel model) {
			salesForce = ObjectFactory
				.With("userName").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceUserName.Value)
				.With("password").EqualTo(ConfigManager.CurrentValues.Instance.SalesForcePassword.Value)
				.With("token").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceToken.Value)
				.With("environment").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceEnvironment.Value)
				.GetInstance<ISalesForceAppClient>();
			this.customerID = customerID;
			this.taskModel = model;
		}
		public override string Name { get { return "AddTask"; } }

		public override void Execute() {
			if (customerID.HasValue) {
				Log.Info("Adding SalesForce task {1} to customer {0} ", taskModel.Subject, customerID.Value);
			}
			salesForce.CreateTask(taskModel);
		}
		private readonly ISalesForceAppClient salesForce;
		private readonly int? customerID;
		private readonly TaskModel taskModel;
	}
}
