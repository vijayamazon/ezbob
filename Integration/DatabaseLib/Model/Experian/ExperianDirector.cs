
namespace EZBob.DatabaseLib.Model.Database {
	using System;

	public class ExperianDirector {
		public virtual int ID { get; set; }
		public virtual int CustomerID { get; set; }
		public virtual string FirstName { get; set; }
		public virtual string MiddleName { get; set; }
		public virtual string LastName { get; set; }
		public virtual DateTime BirthDate { get; set; }
		public virtual char? Gender { get; set; }
		public virtual string Email { get; set; }
		public virtual string MobilePhone { get; set; }
		public virtual bool IsDirector { get; set; }
		public virtual bool IsShareholder { get; set; }
		public virtual bool IsDeleted { get; set; }
		public virtual string RefNum { get; set; }
		public virtual string Line1 { get; set; }
		public virtual string Line2 { get; set; }
		public virtual string Line3 { get; set; }
		public virtual string Town { get; set; }
		public virtual string County { get; set; }
		public virtual string Postcode { get; set; }
	} // class ExperianDirector
} // namespace

namespace EZBob.DatabaseLib.Model.Database.Mapping {
	using System.Collections.Generic;
	using System.Linq;
	using ApplicationMng.Repository;
	using FluentNHibernate.Mapping;
	using NHibernate;

	public class ExperianDirectorModelMap : ClassMap<ExperianDirector> {
		public ExperianDirectorModelMap() {
			Table("ExperianDirectors");
			ReadOnly();
			Cache.ReadOnly().Region("LongTerm").ReadOnly();

			Id(x => x.ID, "ExperianDirectorID");

			Map(x => x.CustomerID);
			Map(x => x.FirstName).Length(512);
			Map(x => x.MiddleName).Length(512);
			Map(x => x.LastName).Length(512);
			Map(x => x.BirthDate);
			Map(x => x.Gender);
			Map(x => x.Email).Length(512);
			Map(x => x.MobilePhone).Length(512);
			Map(x => x.IsDirector);
			Map(x => x.IsShareholder);
			Map(x => x.IsDeleted);
			Map(x => x.RefNum, "DirectorRefNum").Length(512);
			Map(x => x.Line1).Length(512);
			Map(x => x.Line2).Length(512);
			Map(x => x.Line3).Length(512);
			Map(x => x.Town).Length(512);
			Map(x => x.County).Length(512);
			Map(x => x.Postcode).Length(512);
		} // constructor
	} // class ExperianDirectorModelMap

	public class ExperianDirectorRepository : NHibernateRepositoryBase<ExperianDirector> {
		public ExperianDirectorRepository(ISession session) : base(session) {
		} // constructor

		public IQueryable<ExperianDirector> Find(int nCustomerID) {
			return GetAll().Where(ed => (ed.CustomerID == nCustomerID) && !ed.IsDeleted);
		} // Find

		public IEnumerable<string> GetCustomerDirectorsLastNames(int nCustomerID)
		{
			return GetAll().Where(ed => (ed.CustomerID == nCustomerID) && !ed.IsDeleted).Select(x => x.LastName);
		}
	} // class ExperianDirectorRepository

} // namespace

