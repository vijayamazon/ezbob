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
		private readonly CRMStatusGroupRepository _crmStatusGroupRepository;
		private readonly CRMRanksRepository _crmRanksRepository;
		private readonly CustomerRelationStateRepository _customerRelationStateRepository;
		private readonly ICustomerRelationFollowUpRepository _customerRelationFollowUpRepository;
		public CustomerRelationsModelBuilder(LoanRepository loanRepository, CustomerRelationsRepository customerRelationsRepository, ISession session)
		{
			_loanRepository = loanRepository;
			_customerRelationsRepository = customerRelationsRepository;
			_customerRelationFollowUpRepository = new CustomerRelationFollowUpRepository(session);
			_crmStatusGroupRepository = new CRMStatusGroupRepository(session);
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
				crmModel.LastFollowUp = state.FollowUp;
				crmModel.LastStatus = state.Status != null ? state.Status.Name : string.Empty;
			}

			crmModel.FollowUps = _customerRelationFollowUpRepository.GetByCustomer(customerId).Select(x => new FollowUpModel
				{
					Comment = x.Comment,
					Created = x.DateAdded,
					FollowUpDate = x.FollowUpDate,
					IsClosed = x.IsClosed,
					CloseDate = x.CloseDate,
					Id = x.Id
				}).OrderByDescending(x => x.FollowUpDate);
			return crmModel;
		}
	}
}