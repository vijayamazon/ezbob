using System;

namespace EZBob.DatabaseLib.Model.Database 
{
    
    public class MP_AnalyisisFunctionValue 
	{        
        public virtual long Id { get; set; }
        public virtual MP_CustomerMarketPlace CustomerMarketPlace { get; set; }
        public virtual MP_AnalyisisFunction AnalyisisFunction { get; set; }
        public virtual MP_AnalysisFunctionTimePeriod AnalysisFunctionTimePeriod { get; set; }
        public virtual DateTime Updated { get; set; }
        public virtual string ValueString { get; set; }
        public virtual int? ValueInt { get; set; }
        public virtual double? ValueFloat { get; set; }
        public virtual DateTime? ValueDate { get; set; }
        public virtual string ValueXml { get; set; }
        public virtual string Value { get; set; }
		public virtual bool? ValueBoolean { get; set; }

    	public virtual MP_CustomerMarketplaceUpdatingHistory HistoryRecord { get; set; }
	}
}
