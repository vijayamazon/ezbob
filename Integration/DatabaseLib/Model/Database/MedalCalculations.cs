namespace EZBob.DatabaseLib.Model.Database
{
	using System;
	using FluentNHibernate.Mapping;

	public class MedalCalculations
	{
		public virtual int Id { get; set; }
		public virtual bool IsActive { get; set; }
		public virtual int CustomerId { get; set; }
		public virtual string Medal { get; set; }
		public virtual string MedalType { get; set; }
		public virtual decimal TotalScore { get; set; }
		public virtual decimal TotalScoreNormalized { get; set; }
		public virtual DateTime CalculationTime { get; set; }
		public virtual string Error { get; set; }

		//values
		public virtual int BusinessScore { get; set; }
		public virtual decimal FreeCashFlowValue { get; set; }
		public virtual decimal TangibleEquityValue { get; set; }
		public virtual DateTime? BusinessSeniority { get; set; }
		public virtual int ConsumerScore { get; set; }
		public virtual decimal NetWorth { get; set; }
		public virtual string MaritalStatus { get; set; }
		public virtual int NumberOfStores { get; set; }
		public virtual int PositiveFeedbacks { get; set; }
		public virtual DateTime? EzbobSeniority { get; set; }
		public virtual int NumOfLoans { get; set; }
		public virtual int NumOfLateRepayments { get; set; }
		public virtual int NumOfEarlyRepayments { get; set; }
		public virtual decimal ValueAdded { get; set; }

		// Calculated data
		public virtual decimal AnnualTurnover { get; set; }
		public virtual decimal TangibleEquity { get; set; }
		public virtual decimal FreeCashFlow { get; set; }
		public virtual string InnerFlowName { get; set; }

		// Weights, grades, scores
		public virtual decimal BusinessScoreWeight { get; set; }
		public virtual decimal BusinessScoreGrade { get; set; }
		public virtual decimal BusinessScoreScore { get; set; }

		public virtual decimal FreeCashFlowWeight { get; set; }
		public virtual decimal FreeCashFlowGrade { get; set; }
		public virtual decimal FreeCashFlowScore { get; set; }

		public virtual decimal AnnualTurnoverWeight { get; set; }
		public virtual decimal AnnualTurnoverGrade { get; set; }
		public virtual decimal AnnualTurnoverScore { get; set; }

		public virtual decimal TangibleEquityWeight { get; set; }
		public virtual decimal TangibleEquityGrade { get; set; }
		public virtual decimal TangibleEquityScore { get; set; }

		public virtual decimal BusinessSeniorityWeight { get; set; }
		public virtual decimal BusinessSeniorityGrade { get; set; }
		public virtual decimal BusinessSeniorityScore { get; set; }

		public virtual decimal ConsumerScoreWeight { get; set; }
		public virtual decimal ConsumerScoreGrade { get; set; }
		public virtual decimal ConsumerScoreScore { get; set; }

		public virtual decimal NetWorthWeight { get; set; }
		public virtual decimal NetWorthGrade { get; set; }
		public virtual decimal NetWorthScore { get; set; }

		public virtual decimal MaritalStatusWeight { get; set; }
		public virtual decimal MaritalStatusGrade { get; set; }
		public virtual decimal MaritalStatusScore { get; set; }

		public virtual decimal NumberOfStoresWeight { get; set; }
		public virtual decimal NumberOfStoresGrade { get; set; }
		public virtual decimal NumberOfStoresScore { get; set; }

		public virtual decimal PositiveFeedbacksWeight { get; set; }
		public virtual decimal PositiveFeedbacksGrade { get; set; }
		public virtual decimal PositiveFeedbacksScore { get; set; }

		public virtual decimal EzbobSeniorityWeight { get; set; }
		public virtual decimal EzbobSeniorityGrade { get; set; }
		public virtual decimal EzbobSeniorityScore { get; set; }

		public virtual decimal NumOfLoansWeight { get; set; }
		public virtual decimal NumOfLoansGrade { get; set; }
		public virtual decimal NumOfLoansScore { get; set; }

		public virtual decimal NumOfLateRepaymentsWeight { get; set; }
		public virtual decimal NumOfLateRepaymentsGrade { get; set; }
		public virtual decimal NumOfLateRepaymentsScore { get; set; }

		public virtual decimal NumOfEarlyRepaymentsWeight { get; set; }
		public virtual decimal NumOfEarlyRepaymentsGrade { get; set; }
		public virtual decimal NumOfEarlyRepaymentsScore { get; set; }
	}

	public class MedalCalculationsMap : ClassMap<MedalCalculations>
	{
		public MedalCalculationsMap()
		{
			Table("MedalCalculations");
			Id(x => x.Id).GeneratedBy.Native().Column("Id");
			Map(x => x.IsActive);
			Map(x => x.CustomerId);
			Map(x => x.Medal);
			Map(x => x.MedalType);
			Map(x => x.TotalScore);
			Map(x => x.TotalScoreNormalized);
			Map(x => x.CalculationTime);
			Map(x => x.Error).Length(500);

			Map(x => x.BusinessScore);
			Map(x => x.FreeCashFlowValue);
			Map(x => x.TangibleEquityValue);
			Map(x => x.BusinessSeniority);
			Map(x => x.ConsumerScore);
			Map(x => x.NetWorth);
			Map(x => x.MaritalStatus);
			Map(x => x.NumberOfStores);
			Map(x => x.PositiveFeedbacks);
			Map(x => x.EzbobSeniority);
			Map(x => x.NumOfLoans);
			Map(x => x.NumOfLateRepayments);
			Map(x => x.NumOfEarlyRepayments);
			Map(x => x.ValueAdded);
			Map(x => x.AnnualTurnover);
			Map(x => x.TangibleEquity);
			Map(x => x.FreeCashFlow);
			Map(x => x.InnerFlowName);
			Map(x => x.BusinessScoreWeight);
			Map(x => x.BusinessScoreGrade);
			Map(x => x.BusinessScoreScore);
			Map(x => x.FreeCashFlowWeight);
			Map(x => x.FreeCashFlowGrade);
			Map(x => x.FreeCashFlowScore);
			Map(x => x.AnnualTurnoverWeight);
			Map(x => x.AnnualTurnoverGrade);
			Map(x => x.AnnualTurnoverScore);
			Map(x => x.TangibleEquityWeight);
			Map(x => x.TangibleEquityGrade);
			Map(x => x.TangibleEquityScore);
			Map(x => x.BusinessSeniorityWeight);
			Map(x => x.BusinessSeniorityGrade);
			Map(x => x.BusinessSeniorityScore);
			Map(x => x.ConsumerScoreWeight);
			Map(x => x.ConsumerScoreGrade);
			Map(x => x.ConsumerScoreScore);
			Map(x => x.NetWorthWeight);
			Map(x => x.NetWorthGrade);
			Map(x => x.NetWorthScore);
			Map(x => x.MaritalStatusWeight);
			Map(x => x.MaritalStatusGrade);
			Map(x => x.MaritalStatusScore);
			Map(x => x.NumberOfStoresWeight);
			Map(x => x.NumberOfStoresGrade);
			Map(x => x.NumberOfStoresScore);
			Map(x => x.PositiveFeedbacksWeight);
			Map(x => x.PositiveFeedbacksGrade);
			Map(x => x.PositiveFeedbacksScore);
			Map(x => x.EzbobSeniorityWeight);
			Map(x => x.EzbobSeniorityGrade);
			Map(x => x.EzbobSeniorityScore);
			Map(x => x.NumOfLoansWeight);
			Map(x => x.NumOfLoansGrade);
			Map(x => x.NumOfLoansScore);
			Map(x => x.NumOfLateRepaymentsWeight);
			Map(x => x.NumOfLateRepaymentsGrade);
			Map(x => x.NumOfLateRepaymentsScore);
			Map(x => x.NumOfEarlyRepaymentsWeight);
			Map(x => x.NumOfEarlyRepaymentsGrade);
			Map(x => x.NumOfEarlyRepaymentsScore);
		}
	}
}

namespace EZBob.DatabaseLib.Model.Database.Repository
{
	using System.Collections.Generic;
	using System.Linq;
	using ApplicationMng.Repository;
	using Database;
	using NHibernate;

	public interface IMedalCalculationsRepository : IRepository<MedalCalculations>
	{
		IEnumerable<MedalCalculations> GetAllNewMedals(int customerId);
		MedalCalculations GetActiveMedal(int customerId);
	}

	public class MedalCalculationsRepository : NHibernateRepositoryBase<MedalCalculations>, IMedalCalculationsRepository
	{
		public MedalCalculationsRepository(ISession session)
			: base(session)
		{
		}
		
		public IEnumerable<MedalCalculations> GetAllNewMedals(int customerId)
		{
			return GetAll().Where(x => x.CustomerId == customerId);
		}

		public MedalCalculations GetActiveMedal(int customerId)
		{
			return GetAll().FirstOrDefault(x => x.CustomerId == customerId && x.IsActive==true);
		}
	}
}
