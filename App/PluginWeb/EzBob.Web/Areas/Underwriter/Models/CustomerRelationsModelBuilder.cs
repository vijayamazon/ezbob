﻿namespace EzBob.Web.Areas.Underwriter.Models {
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
				.Where(l => l.DateClosed.HasValue)
				.Select(CustomerRelationsModel.CreateRepaidLoan)
				.ToList();

			crm.AddRange(tookLoan);
			crm.AddRange(repaidLoan);

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
				var broker = _brokerRepository.GetByUserId(customerId);
				if (broker != null) {
					crmModel.CustomerName = broker.ContactName;
					crmModel.Phone = broker.ContactMobile;
					crmModel.IsPhoneVerified = true;
				}
			}

			crmModel.CreditResult = customer != null && customer.CreditResult.HasValue ? customer.CreditResult.Value.ToString() : "";
			crmModel.ActionItems = frequentActionItemsRepository.GetAll().Where(x => x.IsActive).Select(x => x.Item).ToList();

			return crmModel;
		} // Create

		private readonly LoanRepository _loanRepository;
		private readonly CustomerRelationsRepository _customerRelationsRepository;
		private readonly CustomerRelationStateRepository _customerRelationStateRepository;
		private readonly ICustomerRelationFollowUpRepository _customerRelationFollowUpRepository;
		private readonly CustomerRepository _customerRepository;
		private readonly BrokerRepository _brokerRepository;
		private readonly CustomerPhoneRepository customerPhoneRepository;
		private readonly FrequentActionItemsRepository frequentActionItemsRepository;
	} // class CustomerRelationsModelBuilder
} // namespace