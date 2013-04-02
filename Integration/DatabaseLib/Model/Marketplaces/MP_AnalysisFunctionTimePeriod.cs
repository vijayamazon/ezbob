using Iesi.Collections.Generic;

namespace EZBob.DatabaseLib.Model.Database {
    
    public class MP_AnalysisFunctionTimePeriod 
	{
        public MP_AnalysisFunctionTimePeriod() 
		{
			AnalyisisFunctionValues = new HashedSet<MP_AnalyisisFunctionValue>();
        }
        public virtual int Id { get; set; }
        public virtual ISet<MP_AnalyisisFunctionValue> AnalyisisFunctionValues { get; set; }
        public virtual string Name { get; set; }
        public virtual System.Guid InternalId { get; set; }
        public virtual string Description { get; set; }
    }
}
