using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database.Mapping {
	public class WriteOffReasonMap : ClassMap<Loans.WriteOffReason> {
		public WriteOffReasonMap() {
			Table("WriteOffReasons");
			ReadOnly();
			Id(x => x.WriteOffReasonId);
			Map(x => x.ReasonName);
		} // constructor
	} // class WriteOffReasonMap
} // namespace EZBob.DatabaseLib.Model.Database.Mapping 

