	namespace EZBob.DatabaseLib.Model.Database
	{
		using System;

		public class MP_CompanyFilesMetaData
		{
			public virtual int Id { get; set; }
			public virtual DateTime Created { get; set; }
			public virtual Customer Customer { get; set; }
			public virtual string FileName { get; set; }
			public virtual string FilePath { get; set; }
			public virtual string FileContentType { get; set; }
			public virtual bool? IsBankStatement { get; set; }
		}
	}

	namespace EZBob.DatabaseLib.Model.Database.Mapping
	{
		using FluentNHibernate.Mapping;
		using NHibernate.Type;

		public class MP_CompanyFilesMetaDataMap : ClassMap<MP_CompanyFilesMetaData>
		{
			public MP_CompanyFilesMetaDataMap()
			{
				Table("MP_CompanyFilesMetaData");
				Id(x => x.Id);
				Map(x => x.Created).CustomType<UtcDateTimeType>();
				Map(x => x.FileName).Length(300);
				Map(x => x.FilePath);
				Map(x => x.FileContentType).Length(300);
				Map(x => x.IsBankStatement);
				References(x => x.Customer, "CustomerId");

			}
		}
	}

	namespace EZBob.DatabaseLib.Model.Database.Repository
	{
		using System.Collections.Generic;
		using System.Linq;
		using ApplicationMng.Repository;
		using NHibernate;

		public interface ICompanyFilesMetaDataRepository : IRepository<MP_CompanyFilesMetaData>
		{
			IEnumerable<MP_CompanyFilesMetaData> GetByCustomerId(int customerId);
			IEnumerable<string> GetBankStatementFiles(int customerId);
			IEnumerable<string> GetFinancialDocumentFiles(int customerId);
		}

		public class CompanyFilesMetaDataRepository : NHibernateRepositoryBase<MP_CompanyFilesMetaData>, ICompanyFilesMetaDataRepository
		{
			public CompanyFilesMetaDataRepository(ISession session)
				: base(session)
			{
			}

			public IEnumerable<MP_CompanyFilesMetaData> GetByCustomerId(int customerId)
			{
				return GetAll().Where(x => x.Customer.Id == customerId);
			}

			public IEnumerable<string> GetBankStatementFiles(int customerId) {
				return GetAll()
					.Where(x => x.Customer.Id == customerId && x.IsBankStatement.HasValue && x.IsBankStatement.Value == true)
					.Select(x => x.FileName);
			}

			public IEnumerable<string> GetFinancialDocumentFiles(int customerId) {
				return GetAll()
					.Where(x => x.Customer.Id == customerId && x.IsBankStatement.HasValue && x.IsBankStatement.Value == true)
					.Select(x => x.FileName);
			}
		}
	}

