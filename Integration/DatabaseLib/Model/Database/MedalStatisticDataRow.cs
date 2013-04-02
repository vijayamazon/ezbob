using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZBob.DatabaseLib.Model.Database
{
    public class MedalStatisticDataRow
    {
        public virtual string Medal { get; set; }
        public virtual int EbayStoresCount { get; set; }
        public virtual double EbayStoresAverage { get; set; }
        public virtual int PayPalStoresCount { get; set; }
        public virtual double PayPalStoresAverage { get; set; }
        public virtual int AmazonStoresCount { get; set; }
        public virtual double AmazonStoresAverage { get; set; }
        public virtual int ScorePointsMin { get; set; }
        public virtual int ScorePointsMax { get; set; }
        public virtual double ScorePointsAverage { get; set; }
        public virtual int ExperianRatingMin { get; set; }
        public virtual int ExperianRatingMax { get; set; }
        public virtual double ExperianRatingAverage { get; set; }
        public virtual int AnualTurnoverMin { get; set; }
        public virtual int AnualTurnoverMax { get; set; }
        public virtual double AnualTurnoverAverage { get; set; }
        public virtual int CustomersCount { get; set; }
        public virtual int AmazonReviews { get; set; }
        public virtual int AmazonRating { get; set; }
        public virtual int EbayReviews { get; set; }
        public virtual int EbayRating { get; set; }
    }
}
