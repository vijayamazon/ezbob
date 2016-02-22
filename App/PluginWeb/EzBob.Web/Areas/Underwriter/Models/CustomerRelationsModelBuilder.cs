namespace EzBob.Web.Areas.Underwriter.Models {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib.Model.CustomerRelations;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Broker;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using NHibernate;

	public class CustomerRelationsModelBuilder {
		public CustomerRelationsModelBuilder(LoanRepository loanRepository, CustomerRelationsRepository customerRelationsRepository, ISession session) {
			_loanRepository = loanRepository;
			_customerRelationsRepository = customerRelationsRepository;
			_customerRelationFollowUpRepository = new CustomerRelationFollowUpRepository(session);
			_customerRelationStateRepository = new CustomerRelationStateRepository(session);
			_customerRepository = new CustomerRepository(session);
			_brokerRepository = new BrokerRepository(session);
			customerPhoneRepository = new CustomerPhoneRepository(session);
			frequentActionItemsRepository = new FrequentActionItemsRepository(session);
			frequentActionItemsForCustomerRepository = new FrequentActionItemsForCustomerRepository(session);
		    collectionStatusHistory = new CustomerStatusHistoryRepository(session);
		    collectionLogRepository = new CollectionLogRepository(session);
		} // constructor

		public CrmModel Create(int customerId) {
			var crm = _customerRelationsRepository
				.ByCustomer(customerId)
				.Select(customerRelations => CustomerRelationsModel.Create(customerRelations))
				.ToList();

			var tookLoan = _loanRepository
				.ByCustomer(customerId)
				.Select(CustomerRelationsModel.CreateTookLoan)
				.ToList();

			var repaidLoan = _loanRepository
				.ByCustomer(customerId)
				.Where(l => l.Status == LoanStatus.PaidOff)
				.Select(CustomerRelationsModel.CreateRepaidLoan)
				.ToList();

		    var changeCustomerStatus = collectionStatusHistory
                .GetAll()
		        .Where(x => x.CustomerId == customerId)
		        .Select(CustomerRelationsModel.CreateChangeStatus)
		        .ToList();

		    var collectionLog = collectionLogRepository
                .GetForCustomer(customerId)
		        .Select(CustomerRelationsModel.CreateCollectionLog)
		        .ToList();
                
			crm.AddRange(tookLoan);
			crm.AddRange(repaidLoan);
		    crm.AddRange(changeCustomerStatus);
            crm.AddRange(collectionLog);

			var crmModel = new CrmModel {
				CustomerRelations = crm.OrderByDescending(x => x.DateTime)
			};

			crmModel.PhoneNumbers = new List<CrmPhoneNumber>();
			var phoneNumbers = customerPhoneRepository.GetAll().Where(x => x.CustomerId == customerId && x.IsCurrent);
			foreach (var phone in phoneNumbers)
			{
				var crmPhoneNumber = new CrmPhoneNumber {IsVerified = phone.IsVerified, Number = phone.Phone, Type = phone.PhoneType};
				crmModel.PhoneNumbers.Add(crmPhoneNumber);
			}

			var state = _customerRelationStateRepository.GetByCustomer(customerId);
			if (state != null) {
				crmModel.CurrentRank = state.Rank;
				crmModel.LastFollowUp = state.FollowUp;
				crmModel.LastStatus = state.Status != null ? state.Status.Name : string.Empty;
			} // if

			crmModel.FollowUps = _customerRelationFollowUpRepository.GetByCustomer(customerId).Select(x => new FollowUpModel {
				Comment = x.Comment,
				Created = x.DateAdded,
				FollowUpDate = x.FollowUpDate,
				IsClosed = x.IsClosed,
				CloseDate = x.CloseDate,
				Id = x.Id
			}).OrderByDescending(x => x.FollowUpDate);

			var customer = _customerRepository.ReallyTryGet(customerId);
			if (customer != null && customer.PersonalInfo != null) {
				crmModel.CustomerName = customer.PersonalInfo.FirstName;
				crmModel.Phone = customer.PersonalInfo.MobilePhone;
				crmModel.IsPhoneVerified = customer.PersonalInfo.MobilePhoneVerified;
			}
			else if(customer == null) {
				var broker = _brokerRepository.GetByID(customerId);
				if (broker != null) {
					crmModel.CustomerName = broker.ContactName;
					crmModel.Phone = broker.ContactMobile;
					crmModel.IsPhoneVerified = true;
				}
			}

			crmModel.CreditResult = customer != null && customer.CreditResult.HasValue ? customer.CreditResult.Value.ToString() : "";
			crmModel.ActionItems = CreateCustomerActionItemList(customerId);
			crmModel.CostumeActionItem = new ActionItem();
			if (customer != null && !string.IsNullOrEmpty(customer.CostumeActionItem))
			{
				crmModel.CostumeActionItem.IsChecked = true;
				crmModel.CostumeActionItem.Text = customer.CostumeActionItem;
			}

			return crmModel;
		} // Create

		private List<FrequentActionItem> CreateCustomerActionItemList(int customerId)
		{
			var list = new List<FrequentActionItem>();
			var actionItemsForCustomerItemsIds = new List<int>();
			IQueryable<FrequentActionItemsForCustomer> actionItemsForCustomer = frequentActionItemsForCustomerRepository.GetAll().Where(x => x.CustomerId == customerId && x.UnmarkedDate == null);
			foreach (FrequentActionItemsForCustomer frequentActionItemsForCustomer in actionItemsForCustomer)
			{
				actionItemsForCustomerItemsIds.Add(frequentActionItemsForCustomer.ItemId);
				var checkedActionItem = new FrequentActionItem
				{
					Id = frequentActionItemsForCustomer.ItemId,
					IsChecked = true
				};
				FrequentActionItems matchingItem = frequentActionItemsRepository.GetAll().FirstOrDefault(x => x.Id == frequentActionItemsForCustomer.ItemId);
				if (matchingItem == null)
				{
					throw new Exception("Item id in FrequentActionItemsForCustomer: {0} is not in FrequentActionItems");
				}
				checkedActionItem.Text = matchingItem.Item;
				list.Add(checkedActionItem);
			}

			IQueryable<FrequentActionItems> otherActionItems = frequentActionItemsRepository.GetAll().Where(x => x.IsActive && !actionItemsForCustomerItemsIds.Contains(x.Id));
			foreach (FrequentActionItems frequentActionItem in otherActionItems)
			{
				var uncheckedActionItem = new FrequentActionItem
				{
					Id = frequentActionItem.Id,
					IsChecked = false,
					Text = frequentActionItem.Item
				};
				list.Add(uncheckedActionItem);
			}

			return list;
		}

		private readonly LoanRepository _loanRepository;
		private readonly CustomerRelationsRepository _customerRelationsRepository;
		private readonly CustomerRelationStateRepository _customerRelationStateRepository;
		private readonly ICustomerRelationFollowUpRepository _customerRelationFollowUpRepository;
		private readonly CustomerRepository _customerRepository;
		private readonly BrokerRepository _brokerRepository;
		private readonly CustomerPhoneRepository customerPhoneRepository;
		private readonly FrequentActionItemsRepository frequentActionItemsRepository;
		private readonly FrequentActionItemsForCustomerRepository frequentActionItemsForCustomerRepository;
	    private readonly CustomerStatusHistoryRepository collectionStatusHistory;
	    private readonly CollectionLogRepository collectionLogRepository;
	} // class CustomerRelationsModelBuilder
} // namespace