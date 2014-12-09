namespace EZBob.DatabaseLib.Model.Marketplaces.Yodlee
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ApplicationMng.Repository;
    using NHibernate;
    using NHibernate.Linq;

    public class MP_YodleeTransactionCategories
    {
        public virtual string CategoryId { get; set; }
        public virtual string Name { get; set; }
        public virtual string Type { get; set; }
    }

    public interface IMP_YodleeTransactionCategoriesRepository : IRepository<MP_YodleeTransactionCategories>
    {
        MP_YodleeTransactionCategories GetYodleeTransactionCategoryByCategoryId(string categoryId);
		MP_YodleeTransactionCategories GetYodleeTransactionCategoryByCategoryId(long? categoryId);
    }

    public class MP_YodleeTransactionCategoriesRepository : NHibernateRepositoryBase<MP_YodleeTransactionCategories>,
                                                       IMP_YodleeTransactionCategoriesRepository
    {
        public MP_YodleeTransactionCategoriesRepository(ISession session)
            : base(session)
        {

        }

        public MP_YodleeTransactionCategories GetYodleeTransactionCategoryByCategoryId(string categoryId)
        {
            MP_YodleeTransactionCategories category;

                category = _session
                .Query<MP_YodleeTransactionCategories>()
                .FirstOrDefault(t => t.CategoryId == categoryId);

			if(category == null)
			{
                category = _session
                .Query<MP_YodleeTransactionCategories>()
                .FirstOrDefault(t => t.CategoryId == "1");
            }

            return category;
        }

		public MP_YodleeTransactionCategories GetYodleeTransactionCategoryByCategoryId(long? categoryId)
		{
			return GetYodleeTransactionCategoryByCategoryId(categoryId.ToString());
		}

    }

}
