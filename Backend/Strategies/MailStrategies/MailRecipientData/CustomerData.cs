namespace EzBob.Backend.Strategies.MailStrategies {
	using System;
	using Ezbob.Database;

	public class CustomerData {
		#region method Load

		public virtual void Load(int customerId, AConnection oDb) {
			oDb.ForEachRowSafe((sr, bRowsetStart) => {
					Id = sr["Id"];
					FirstName = sr["FirstName"];
					Surname = sr["Surname"];
					FullName = sr["FullName"];
					Mail = sr["Mail"];
					IsOffline = sr["IsOffline"];
					NumOfLoans = sr["NumOfLoans"];
					RefNum = sr["RefNum"];
					MobilePhone = sr["MobilePhone"];
					DaytimePhone = sr["DaytimePhone"];
					IsTest = sr["IsTest"];
					return ActionResult.SkipAll;
				},
				"GetBasicCustomerData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);

			if (Id != customerId)
				throw new Exception("Failed to find a customer by id " + customerId);
		} // Load

		#endregion method Load

		#region method ToString

		public override string ToString() {
			return string.Format(
				"{0}: {1} {2} ({5}, {3}) {4} loan #: {6}, mobile: {7}, land line: {8}, test: {9}",
				Id,
				FirstName,
				Surname,
				FullName,
				Mail,
				IsOffline ? "offline" : "online",
				NumOfLoans,
				MobilePhone,
				DaytimePhone,
				IsTest ? "yes" : "no"
			);
		} // ToString

		#endregion method ToString

		#region properties

		public virtual int Id { get; protected set; }
		public virtual string FirstName { get; protected set; }
		public virtual string Surname { get; protected set; }
		public virtual string FullName { get; protected set; }
		public virtual string Mail { get; protected set; }
		public virtual bool IsOffline { get; protected set; }
		public virtual int NumOfLoans { get; protected set; }
		public virtual string RefNum { get; protected set; }
		public virtual string MobilePhone { get; protected set; }
		public virtual string DaytimePhone { get; protected set; }
		public virtual bool IsTest { get; protected set; }

		#endregion properties
	} // class CustomerData
} // namespace
