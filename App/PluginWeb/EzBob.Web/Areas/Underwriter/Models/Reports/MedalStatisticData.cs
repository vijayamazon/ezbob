using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EZBob.DatabaseLib.Model.Database;

namespace EzBob.Web.Areas.Underwriter.Models.Reports
{
    public class MedalStatisticData
    {
        public MedalStatisticData()
        {}

        public MedalStatisticData(MedalStatisticDataRow data)
        {
            Medal = data.Medal;
            EbayStoresCount = data.EbayStoresCount;
            EbayStoresAverage = data.EbayStoresAverage;
            PayPalStoresCount = data.PayPalStoresCount;
            PayPalStoresAverage = data.PayPalStoresAverage;
            AmazonStoresCount = data.AmazonStoresCount;
            AmazonStoresAverage = data.AmazonStoresAverage;
            ScorePointsMin = data.ScorePointsMin;
            ScorePointsMax = data.ScorePointsMax;
            ScorePointsAverage = data.ScorePointsAverage;
            ScorePointsMin = data.ExperianRatingMin;
            ScorePointsMax = data.ExperianRatingMax;
            ExperianRatingAverage = data.ExperianRatingAverage;
            AnualTurnoverMin = data.AnualTurnoverMin;
            AnualTurnoverMax = data.AnualTurnoverMax;
            AnualTurnoverAverage = data.AnualTurnoverAverage;
            CustomersCount = data.CustomersCount;
            AmazonReviews = data.AmazonReviews;
            AmazonRating = data.AmazonRating;
            EbayReviews = data.EbayReviews;
            EbayRating = data.EbayRating;
        }

        private string FromatRange(int min, int max)
        {
            return min == max ? string.Format("{0}", min) : string.Format("{0} - {1}", min, max);
        }

        public string Medal { get; set; }
        public int EbayStoresCount { get; set; }
        public double EbayStoresAverage { get; set; }
        public int PayPalStoresCount { get; set; }
        public double PayPalStoresAverage { get; set; }
        public int AmazonStoresCount { get; set; }
        public double AmazonStoresAverage { get; set; }
        public string ScorePointsRange { get { return FromatRange(ScorePointsMin, ScorePointsMax); } }
        public int ScorePointsMin { get; set; }
        public int ScorePointsMax { get; set; }
        public double ScorePointsAverage { get; set; }
        public string ExperianRatingRange { get { return FromatRange(ExperianRatingMin, ExperianRatingMax); } }
        public int ExperianRatingMin { get; set; }
        public int ExperianRatingMax { get; set; }
        public double ExperianRatingAverage { get; set; }
        public string AnualTurnoverRange { get { return FromatRange(AnualTurnoverMin, AnualTurnoverMax); } }
        public int AnualTurnoverMin { get; set; }
        public int AnualTurnoverMax { get; set; }
        public double AnualTurnoverAverage { get; set; }
        public int CustomersCount { get; set; }
        public int AmazonReviews { get; set; }
        public int AmazonRating { get; set; }
        public int EbayReviews { get; set; }
        public int EbayRating { get; set; }
    }
}