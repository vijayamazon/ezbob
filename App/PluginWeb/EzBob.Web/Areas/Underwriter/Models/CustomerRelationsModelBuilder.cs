namespace EzBob.Web.Areas.Underwriter.Models
{
	using System.Linq;
	using EZBob.DatabaseLib.Model.CustomerRelations;
	using EZBob.DatabaseLib.Model.Database.Loans;

	public class CustomerRelationsModelBuilder
	{
		private readonly LoanRepository _loanRepository;
		private readonly CustomerRelationsRepository _customerRelationsRepository;

		public CustomerRelationsModelBuilder(LoanRepository loanRepository, CustomerRelationsRepository customerRelationsRepository)
		{
			_loanRepository = loanRepository;
			_customerRelationsRepository = customerRelationsRepository;
		}

		public IOrderedEnumerable<CustomerRelationsModel> Create(int customerId)
		{

			var crm =
				_customerRelationsRepository.ByCustomer(customerId)
											.Select(customerRelations => CustomerRelationsModel.Create(customerRelations))
											.ToList();
			var tookLoan =
				_loanRepository.ByCustomer(customerId)
							   .Select(l => CustomerRelationsModel.CreateTookLoan(l))
							   .ToList();
			var repaidLoan =
				_loanRepository.ByCustomer(customerId)
							   .Where(l => l.DateClosed.HasValue)
							   .Select(l => CustomerRelationsModel.CreateRepaidLoan(l))
							   .ToList();

			crm.AddRange(tookLoan);
			crm.AddRange(repaidLoan);

			return crm.OrderByDescending(x => x.DateTime);
		}

	
	}
}