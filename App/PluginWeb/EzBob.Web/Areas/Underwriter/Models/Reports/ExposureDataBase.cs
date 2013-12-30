using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EZBob.DatabaseLib.Model.Database;

namespace EzBob.Web.Areas.Underwriter.Models.Reports
{
    public class ExposureDataBase : PercentObtainer
    {
        public ExposureDataBase()
        {}

        protected ExposureDataBase(ExposureDataBase data)
        {
            Processed = data.Processed;
            ProcessedAmount = data.ProcessedAmount;
            Approved = data.Approved;
            ApprovedAmount = data.ApprovedAmount;
            Paid = data.Paid;
            PaidAmount = data.PaidAmount;
            Late30 = data.Late30;
            Late30Amount = data.Late30Amount;
            Late60 = data.Late60;
            Late60Amount = data.Late60Amount;
            Late90 = data.Late90;
            Late90Amount = data.Late90Amount;
            Defaults = data.Defaults;
            DefaultsAmount = data.DefaultsAmount;
            Exposure = data.Exposure;
            OpenCreditLine = data.OpenCreditLine;
        }

        protected ExposureDataBase(ExposureDataBaseRow data)
        {
			if (data==null) return;
            Processed = data.Processed;
            ProcessedAmount = data.ProcessedAmount;
            Approved = data.Approved;
            ApprovedAmount = data.ApprovedAmount;
            Paid = data.Paid;
            PaidAmount = data.PaidAmount;
            Late30 = data.Late30;
            Late30Amount = data.Late30Amount;
            Late60 = data.Late60;
            Late60Amount = data.Late60Amount;
            Late90 = data.Late90;
            Late90Amount = data.Late90Amount;
            Defaults = data.Defaults;
            DefaultsAmount = data.DefaultsAmount;
            Exposure = data.Exposure;
            OpenCreditLine = data.OpenCreditLine;
        }

        public int Processed { get; set; }
        public double ProcessedAmount { get; set; }
        public int Approved { get; set; }
        public decimal ApprovedToProcessed
        { get { return GetPercent(Approved, Processed); } }

        public double ApprovedAmount { get; set; }
        public decimal ApprovedAmountToProcessedAmount
        { get { return GetPercent(ApprovedAmount, ProcessedAmount); } }

        public int Paid { get; set; }
        public decimal PaidToApproved
        { get { return GetPercent(Approved, Processed); } }

        public double PaidAmount { get; set; }
        public decimal PaidAmountToApprovedAmount
        { get { return GetPercent(PaidAmount, ApprovedAmount); } }

        public int Late30 { get; set; }
        public decimal Late30ToApproved
        { get { return GetPercent(Late30, Approved); } }

        public double Late30Amount { get; set; }
        public decimal Late30AmountToApprovedAmount
        { get { return GetPercent(Late30Amount, ApprovedAmount); } }

        public int Late60 { get; set; }
        public decimal Late60ToApproved
        { get { return GetPercent(Late60, Approved); } }

        public double Late60Amount { get; set; }
        public decimal Late60AmountToApprovedAmount
        { get { return GetPercent(Late60Amount, ApprovedAmount); } }

        public int Late90 { get; set; }
        public decimal Late90ToApproved
        { get { return GetPercent(Late90, Processed); } }

        public double Late90Amount { get; set; }
        public decimal Late90AmountToApprovedAmount
        { get { return GetPercent(Late90, ApprovedAmount); } }

        public int Defaults { get; set; }
        public decimal DefaultsToApproved
        { get { return GetPercent(Defaults, Approved); } }

        public double DefaultsAmount { get; set; }
        public decimal DefaultsAmountToApprovedAmount
        { get { return GetPercent(DefaultsAmount, ApprovedAmount); } }

        public double Exposure { get; set; }
        public double OpenCreditLine { get; set; }

    }
}