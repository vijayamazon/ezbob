using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationMng.Repository;
using EzBob.CommonLib.MarketplaceSpecificTypes.TeraPeakOrdersData;
using NHibernate;
using NHibernate.Linq;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_TeraPeakOrderItem
	{
        private IList<MP_TeraPeakCategoryStatistics> _categoryStatistics = new List<MP_TeraPeakCategoryStatistics>();
	    public virtual int Id { get; set; }

		public virtual MP_TeraPeakOrder Order { get; set; }

		public virtual DateTime StartDate { get; set; }
		public virtual DateTime EndDate { get; set; }
		public virtual double? Revenue { get; set; }
		public virtual int? Listings { get; set; }
		public virtual int? Transactions { get; set; }
		public virtual int? Successful { get; set; }
		public virtual int? Bids { get; set; }
		public virtual int? ItemsOffered { get; set; }
		public virtual int? ItemsSold { get; set; }
		public virtual int? AverageSellersPerDay { get; set; }
		public virtual double? SuccessRate { get; set; }

		public virtual RangeMarkerType RangeMarker { get; set; }

	    public virtual IList<MP_TeraPeakCategoryStatistics> CategoryStatistics
	    {
	        get { return _categoryStatistics; }
	        set { _categoryStatistics = value; }
	    }
	}

    public interface IMP_TeraPeakOrderItemRepository : IRepository<MP_TeraPeakOrderItem>
    {
    }

    public class MP_TeraPeakOrderItemRepository : NHibernateRepositoryBase<MP_TeraPeakOrderItem>, IMP_TeraPeakOrderItemRepository
    {
        public MP_TeraPeakOrderItemRepository(ISession session)
            : base(session)
        {
        }

        public List<MP_TeraPeakOrderItem> GetTeraPeakOrderItems(int customerMarketPlaceId)
        {
            return Session
                .Query<MP_TeraPeakOrderItem>()
                .Where(o => o.Order.CustomerMarketPlace.Id == customerMarketPlaceId)
                .OrderBy(o => o.StartDate)
                .ToList();
        }
    }
}