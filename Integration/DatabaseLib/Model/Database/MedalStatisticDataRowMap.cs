using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{
    class MedalStatisticDataRowMap : ClassMap<MedalStatisticDataRow>
    {
        public MedalStatisticDataRowMap()
        {
            Id(x => x.Medal);
            Map(x => x.EbayStoresCount);
            Map(x => x.EbayStoresAverage);
            Map(x => x.PayPalStoresCount);
            Map(x => x.PayPalStoresAverage);
            Map(x => x.AmazonStoresCount);
            Map(x => x.AmazonStoresAverage);
            Map(x => x.ScorePointsMin);
            Map(x => x.ScorePointsMax);
            Map(x => x.ScorePointsAverage);
            Map(x => x.ExperianRatingMin);
            Map(x => x.ExperianRatingMax);
            Map(x => x.ExperianRatingAverage);
            Map(x => x.AnualTurnoverMin);
            Map(x => x.AnualTurnoverMax);
            Map(x => x.AnualTurnoverAverage);
            Map(x => x.CustomersCount);
            Map(x => x.AmazonReviews);
            Map(x => x.AmazonRating);
            Map(x => x.EbayReviews);
            Map(x => x.EbayRating);
        }
    }
}
