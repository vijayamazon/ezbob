﻿namespace EzBob.Web.Areas.Underwriter.Models
{
	using System;
	using System.Collections.Generic;
    using EZBob.DatabaseLib.Model.Fraud;

	public class FraudDetectionLogModel
	{
		public DateTime? LastCheckDate { get; set; }
        public string CustomerRefNumber { get; set; }
		public List<FraudDetectionLogRowModel> FraudDetectionLogRows { get; set; }
	}

    public class FraudDetectionLogRowModel
    {
        
        public FraudDetectionLogRowModel(FraudDetection fraudDetection)
        {
            var type = fraudDetection.ExternalUser != null ? "External" : "Internal";
            if (!string.IsNullOrEmpty(fraudDetection.CurrentField) && fraudDetection.CurrentField.StartsWith("Iovation"))
            {
                type = "Iovation";
            }

            Id = fraudDetection.Id;
            CompareField = fraudDetection.CompareField;
            CurrentField = fraudDetection.CurrentField;
            Value = fraudDetection.Value;
            Concurrence = fraudDetection.Concurrence;
            Type = type;
        }

        public int Id { get; set; }
        public string Type { get; set; }    
        public string CurrentField { get; set; }
        public string CompareField { get; set; }
        public string Value { get; set; }
        public string Concurrence { get; set; }
    }
}