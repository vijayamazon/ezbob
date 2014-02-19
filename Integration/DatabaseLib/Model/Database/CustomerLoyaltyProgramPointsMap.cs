//using FluentNHibernate.Mapping;

//namespace EZBob.DatabaseLib.Model.Database {
//	public class CustomerLoyaltyProgramPointsMap : ClassMap<CustomerLoyaltyProgramPoints> {
//		public CustomerLoyaltyProgramPointsMap() {
//			Table("CustomerLoyaltyProgramPoints");
//			ReadOnly();
//			Id(x => x.CustomerID);
//			References(x => x.Customer, "CustomerID");
//			Map(x => x.EarnedPoints);
//			Map(x => x.LastActionDate);
//		} // constructor
//	} // class CustomerLoyaltyProgramPointsMap
//} // namespace EZBob.DatabaseLib.Model.Database
