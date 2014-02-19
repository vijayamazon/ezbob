//using FluentNHibernate.Mapping;

//namespace EZBob.DatabaseLib.Model.Database {
//	public class LoyaltyProgramActionMap : ClassMap<LoyaltyProgramAction> {
//		public LoyaltyProgramActionMap() {
//			Table("LoyaltyProgramActions");
//			ReadOnly();
//			Id(x => x.ActionID);
//			Map(x => x.ActionName).Length(20);
//			Map(x => x.ActionDescription).Length(256);
//			Map(x => x.Cost);
//			Map(x => x.ActionTypeID);
//		} // constructor
//	} // class LoyaltyProgramActionMap
//} // namespace EZBob.DatabaseLib.Model.Database
