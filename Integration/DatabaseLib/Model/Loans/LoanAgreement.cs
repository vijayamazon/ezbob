using System;
using System.Globalization;
using System.IO;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database.Loans;
using FluentNHibernate.Mapping;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Loans
{
    public class LoanAgreement
    {
        public LoanAgreement()
        {
        }

        public LoanAgreement(string name, string template, Database.Loans.Loan loan)
        {
            Name = name;
            Template = template;
            Loan = loan;
            FilePath = LongFilenameWithDir();
        }

        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Template { get; set; }
        public virtual Database.Loans.Loan Loan { get; set; }
        public virtual string FilePath { get; set; }
        public virtual string ZohoId { get; set; }

        public virtual string ShortFilename()
        {
            return String.Format("{0}_{1}_{2}_{3}.pdf",
                                 Name,
                                 Loan.Customer.PersonalInfo.FirstName,
                                 Loan.Customer.PersonalInfo.Surname,
                                 Loan.Date.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture));
        }

        public virtual string LongFilename()
        {
            return String.Format("{0}_{1}_{2}_{3:000}_{4}.pdf",
                                 Name,
                                 Loan.Customer.PersonalInfo.FirstName,
                                 Loan.Customer.PersonalInfo.Surname,
                                 Loan.Customer.Id,
                                 Loan.Date.ToString("dd-MM-yyyy_hh-mm-ss", CultureInfo.InvariantCulture));
        }

        public virtual string LongFilenameWithDir()
        {
            var filename = LongFilename();
            var dirYear = (Loan.Date.Year).ToString(CultureInfo.InvariantCulture);
            var dirMonth = (Loan.Date.Month).ToString(CultureInfo.InvariantCulture);
            var dirDay = (Loan.Date.Day).ToString(CultureInfo.InvariantCulture);
            return Path.Combine(dirYear, dirMonth, dirDay, Loan.RefNumber, filename);
        }
    }

    public interface ILoanAgreementRepository : IRepository<LoanAgreement>
    {

    }

    public class LoanAgreementRepository : NHibernateRepositoryBase<LoanAgreement>, ILoanAgreementRepository
    {
        public LoanAgreementRepository(ISession session) : base(session)
        {
        }
    }

    public class LoanAgreementMap : ClassMap<LoanAgreement>
    {
        public LoanAgreementMap()
        {
            Table("LoanAgreement");
            Id(x => x.Id).GeneratedBy.HiLo("100");
            Map(x => x.Name).Length(200);
            Map(x => x.FilePath).Length(400);
            Map(x => x.Template).CustomType("StringClob");
            References(x => x.Loan, "LoanId");
            Map(x => x.ZohoId).Length(100);
        }
    }
}