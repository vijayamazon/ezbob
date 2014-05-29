namespace EzBob.Web.Areas.Underwriter.Models
{
	using System.Linq;
	using EZBob.DatabaseLib.Model.CustomerRelations;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using NHibernate;

	public class CustomerRelationsModelBuilder
	{
		private readonly LoanRepository _loanRepository;
		private readonly CustomerRelationsRepository _customerRelationsRepository;
		private readonly CRMActionsRepository _crmActionsRepository;
		private readonly CRMStatusesRepository _crmStatusesRepository;
		private readonly CRMRanksRepository _crmRanksRepository;
		private readonly CustomerRelationStateRepository _customerRelationStateRepository;
		public CustomerRelationsModelBuilder(LoanRepository loanRepository, CustomerRelationsRepository customerRelationsRepository, ISession session)
		{
			_loanRepository = loanRepository;
			_customerRelationsRepository = customerRelationsRepository;
			_crmStatusesRepository = new CRMStatusesRepository(session);
			_crmActionsRepository = new CRMActionsRepository(session);
			_crmRanksRepository = new CRMRanksRepository(session);
			_customerRelationStateRepository = new CustomerRelationStateRepository(session);
		}

		public CrmModel Create(int customerId)
		{

			var crm =
				_customerRelationsRepository
					.ByCustomer(customerId)
					.Select(customerRelations => CustomerRelationsModel.Create(customerRelations))
					.ToList();

			var tookLoan =
				_loanRepository
					.ByCustomer(customerId)
					.Select(CustomerRelationsModel.CreateTookLoan)
					.ToList();

			var repaidLoan =
				_loanRepository
					.ByCustomer(customerId)
					.Where(l => l.DateClosed.HasValue)
					.Select(CustomerRelationsModel.CreateRepaidLoan)
					.ToList();

			crm.AddRange(tookLoan);
			crm.AddRange(repaidLoan);

			var crmModel = new CrmModel
			{
				CustomerRelations = crm.OrderByDescending(x => x.DateTime)
			};

			var state = _customerRelationStateRepository.GetByCustomer(customerId);
			if (state != null)
			{
				crmModel.CurrentRank = state.Rank;
				crmModel.IsFollowed = state.IsFollowUp.HasValue && state.IsFollowUp.Value;
			}

			return crmModel;

		}

		public CrmStaticModel GetStaticCrmModel()
		{
			var model = new CrmStaticModel
			{
				CrmActions = _crmActionsRepository.GetAll(),
				CrmStatuses = _crmStatusesRepository.GetAll(),
				CrmRanks = _crmRanksRepository.GetAll()
			};

			return model;
		}
	}
}