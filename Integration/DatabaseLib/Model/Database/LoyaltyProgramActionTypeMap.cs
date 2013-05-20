using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database {
	public class LoyaltyProgramActionTypeMap : ClassMap<LoyaltyProgramActionType> {
		public LoyaltyProgramActionTypeMap() {
			Table("LoyaltyProgramActionTypes");
			ReadOnly();
			Id(x => x.ActionTypeID);
			Map(x => x.ActionTypeName).Length(256);
		} // constructor
	} // class LoyaltyProgramActionTypeMap
} // namespace EZBob.DatabaseLib.Model.Database
