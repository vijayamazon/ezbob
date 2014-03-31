namespace EzBob.Backend.Strategies.MailStrategies {
	using System;
	using System.Data;
	using Ezbob.Database;

	public class CustomerData {
		#region method Load

		public virtual void Load(int customerId, AConnection oDb) {
			DataTable dt = oDb.ExecuteReader(
				"GetBasicCustomerData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);

			Id = customerId;

			if (dt.Rows.Count != 1)
				throw new Exception("Failed to find a customer by id " + customerId);

			var sr = new SafeReader(dt.Rows[0]);

			FirstName = sr["FirstName"];
			Surname = sr["Surname"];
			FullName = sr["FullName"];
			Mail = sr["Mail"];
			IsOffline = sr["IsOffline"];
			NumOfLoans = sr["NumOfLoans"];
			RefNum = sr["RefNum"];
		} // Load

		#endregion method Load

		#region method ToString

		public override string ToString() {
			return string.Format(
				"{0}: {1} {2} ({5}, {3}) {4} loan #: {6}",
				Id,
				FirstName,
				Surname,
				FullName,
				Mail,
				IsOffline ? "offline" : "online",
				NumOfLoans
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

		#endregion properties
	} // class CustomerData
} // namespace
