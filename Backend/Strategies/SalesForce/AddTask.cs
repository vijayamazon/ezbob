﻿namespace Ezbob.Backend.Strategies.SalesForce {
	using SalesForceLib;
	using SalesForceLib.Models;
	using StructureMap;

	public class AddTask : AStrategy {

		public AddTask(int? customerID, TaskModel model) {
		    this.salesForce = ObjectFactory
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
			
			Log.Info("Adding SalesForce task {1} to customer {0} {1}", this.taskModel.Subject, this.customerID, this.taskModel.Email);

            if (string.IsNullOrEmpty(this.taskModel.Description)) {
                this.taskModel.Description = this.taskModel.Subject;
            }

		    this.taskModel.Email = this.taskModel.Email.ToLower();

		    this.salesForce.CreateTask(this.taskModel);
		}
		private readonly ISalesForceAppClient salesForce;
		private readonly int? customerID;
		private readonly TaskModel taskModel;
	}
}
