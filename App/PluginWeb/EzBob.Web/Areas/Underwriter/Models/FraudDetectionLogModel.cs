using EZBob.DatabaseLib.Model.Fraud;
using EzBob.Web.Code;

namespace EzBob.Web.Areas.Underwriter.Models
{
	using System;
	using System.Collections.Generic;

	public class FraudDetectionLogModel
	{
		public DateTime? LastCheckDate { get; set; }
		public List<FraudDetectionLogRowModel> FraudDetectionLogRows { get; set; }
	}

    public class FraudDetectionLogRowModel
    {
        public FraudDetectionLogRowModel(FraudDetection fraudDetection)
        {
            Id = fraudDetection.Id;
            CompareField = fraudDetection.CompareField;
            CurrentField = fraudDetection.CurrentField;
            Value = fraudDetection.Value;
            Concurrence = fraudDetection.Concurrence;
            Type = fraudDetection.ExternalUser != null ? "External" : "Internal";
        }

        public int Id { get; set; }
        public string Type { get; set; }    
        public string CurrentField { get; set; }
        public string CompareField { get; set; }
        public string Value { get; set; }
        public string Concurrence { get; set; }
    }
}