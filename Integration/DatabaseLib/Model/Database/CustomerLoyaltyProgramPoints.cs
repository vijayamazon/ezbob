using System;
using ApplicationMng.Repository;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Database {
	#region class CustomerLoyaltyProgramPoints

	public class CustomerLoyaltyProgramPoints {
		#region public

		public virtual int CustomerID { get; set; }
		public virtual Customer Customer { get; set; }
		public virtual Int64 EarnedPoints { get; set; }
		public virtual DateTime LastActionDate { get; set; }

		#endregion public
	} // class CustomerLoyaltyProgramPoints

	#endregion class CustomerLoyaltyProgramPoints

	#region class CustomerLoyaltyProgramPointsRepository

	public class CustomerLoyaltyProgramPointsRepository : NHibernateRepositoryBase<CustomerLoyaltyProgramPoints> {
		public CustomerLoyaltyProgramPointsRepository(ISession session) : base(session) { } // constructor
	} // class CustomerLoyaltyProgramPointsRepository

	#endregion class CustomerLoyaltyProgramPointsRepository
} // namespace EZBob.DatabaseLib.Model.Database
