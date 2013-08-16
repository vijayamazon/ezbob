using System;
using System.Globalization;
using System.IO;
using System.Linq;
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

        //public LoanAgreement(string name, string template, Database.Loans.Loan loan, int templateId)
        public LoanAgreement(string name, Database.Loans.Loan loan, int templateId)
        {
            Name = name;
            Template = "OBSOLETE COLUMN!! This template can be found in LoanAgreementTemplate, id " + templateId.ToString() + "."; // Obsolete line, to be removed.
            Loan = loan;
            FilePath = LongFilenameWithDir();
            TemplateId = templateId;
        }

        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Template { get; set; } // Obsolete line, to be removed.
        public virtual Database.Loans.Loan Loan { get; set; }
        public virtual string FilePath { get; set; }
        public virtual string ZohoId { get; set; }
        public virtual int TemplateId { get; set; }


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
            Map(x => x.Template).CustomType("StringClob"); // Obsolete line, to be removed.
            References(x => x.Loan, "LoanId");
            Map(x => x.ZohoId).Length(100);
            Map(x => x.TemplateId);
        }
    }

    public class LoanAgreementTemplate
    {
        public LoanAgreementTemplate()
        {
        }

        public LoanAgreementTemplate(string template)
        {
            Template = template;
        }

        public virtual int Id { get; set; }
        public virtual string Template { get; set; }
    }

    public class LoanAgreementTemplateMap : ClassMap<LoanAgreementTemplate>
    {
        public LoanAgreementTemplateMap()
        {
            Table("LoanAgreementTemplate");
            Id(x => x.Id).GeneratedBy.HiLo("100");
            Map(x => x.Template).CustomType("StringClob");
        }
    }

    public interface ILoanAgreementTemplateRepository : IRepository<LoanAgreementTemplate>
    {
    }

    public class LoanAgreementTemplateRepository : NHibernateRepositoryBase<LoanAgreementTemplate>, ILoanAgreementTemplateRepository
    {
        public LoanAgreementTemplateRepository(ISession session)
            : base(session)
        {
        }
    }
}