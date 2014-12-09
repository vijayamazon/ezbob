using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EZBob.DatabaseLib.Model.Database;

namespace EzBob.Web.Areas.Underwriter.Models.Reports
{
    public class PerformenceDataBase : PercentObtainer
    {
        public PerformenceDataBase()
        {}

        protected PerformenceDataBase(PerformenceDataBaseRow data)
        {
            Processed = data.Processed;
            ProcessedAmount = data.ProcessedAmount;
            Approved = data.Approved;
            ApprovedAmount = data.ApprovedAmount;
            Rejected = data.Rejected;
            RejectedAmount = data.RejectedAmount;
            Escalated = data.Escalated;
            EscalatedAmount = data.EscalatedAmount;
            LatePayments = data.LatePayments;
            LatePaymentsAmount = data.LatePaymentsAmount;
            LowSide = data.LowSide;
            LowSideAmount = data.LowSideAmount;
            HighSide = data.HighSide;
            HighSideAmount = data.HighSideAmount;
            MaxTime = data.MaxTime;
            AvgTime = data.AvgTime;
        }

        protected PerformenceDataBase(PerformenceDataBase data)
        {
            Processed = data.Processed;
            ProcessedAmount = data.ProcessedAmount;
            Approved = data.Approved;
            ApprovedAmount = data.ApprovedAmount;
            Rejected = data.Rejected;
            RejectedAmount = data.RejectedAmount;
            Escalated = data.Escalated;
            EscalatedAmount = data.EscalatedAmount;
            LatePayments = data.LatePayments;
            LatePaymentsAmount = data.LatePaymentsAmount;
            LowSide = data.LowSide;
            LowSideAmount = data.LowSideAmount;
            HighSide = data.HighSide;
            HighSideAmount = data.HighSideAmount;
            MaxTime = data.MaxTime;
            AvgTime = data.AvgTime;
        }

        public int Processed { get; set; }
        public double ProcessedAmount { get; set; }
        public int Approved { get; set; }
        public decimal ApprovedToProcessed
        { get { return GetPercent(Approved, Processed); } }

        public double ApprovedAmount { get; set; }
        public decimal ApprovedAmountToProcessedAmount
        { get { return GetPercent(ApprovedAmount, ProcessedAmount); } }

        public int Rejected { get; set; }
        public decimal RejectedToProcessed
        { get { return GetPercent(Rejected, Processed); } }

        public double RejectedAmount { get; set; }
        public decimal RejectedAmountToProcessedAmount
        { get { return GetPercent(RejectedAmount, ProcessedAmount); } }

        public int Escalated { get; set; }
        public decimal EscalatedToProcessed
        { get { return GetPercent(Escalated, Processed); } }

        public double EscalatedAmount { get; set; }
        public decimal EscalatedAmountToProcessedAmount
        { get { return GetPercent(EscalatedAmount, ProcessedAmount); } }

        public int LatePayments { get; set; }
        public decimal LatePaymentsToApproved
        { get { return GetPercent(LatePayments, Approved); } }

        public double LatePaymentsAmount { get; set; }
        public decimal LatePaymentsAmountToApprovedAmount
        { get { return GetPercent(LatePaymentsAmount, ApprovedAmount); } }

        public int Defaults { get; set; }
        public decimal DefaultsToApproved
        { get { return GetPercent(Defaults, Processed); } }

        public double DefaultsAmount { get; set; }
        public decimal DefaultsAmountToApprovedAmount
        { get { return GetPercent(DefaultsAmount, ApprovedAmount); } }

        public int LowSide { get; set; }
        public decimal LowSideToProcessed
        { get { return GetPercent(LowSide, Processed); } }

        public double LowSideAmount { get; set; }
        public decimal LowSideAmountToProcessedAmount
        { get { return GetPercent(LowSideAmount, ProcessedAmount); } }

        public int HighSide { get; set; }
        public decimal HighSideToProcessed
        { get { return GetPercent(HighSide, Processed); } }

        public double HighSideAmount { get; set; }
        public decimal HighSideAmountToProcessedAmount
        { get { return GetPercent(HighSideAmount, ProcessedAmount); } }

        public long MaxTime { get; set; }
        public long AvgTime { get; set; }

    }
}
