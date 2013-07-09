namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System.Collections.Generic;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.CustomerRelations;
	using Models;
	using Infrastructure.csrf;
	using Scorto.Web;

    public class CustomerRelationsController : Controller
    {
		private readonly CustomerRelationsRepository customerRelationsRepository;

		public CustomerRelationsController(CustomerRelationsRepository customerRelationsRepository)
        {
			this.customerRelationsRepository = customerRelationsRepository;
        }

		private CustomerRelationsModel GetModel(CustomerRelations customerRelations)
		{
			return new CustomerRelationsModel
				{
					User = customerRelations.UserName,
					Action = customerRelations.Action.Name,
					Status = customerRelations.Status.Name,
					DateTime = customerRelations.Timestamp,
					Comment = customerRelations.Comment,
					Incoming = customerRelations.Incoming
				};
		}

        [Ajax]
        [HttpGet]
        [Transactional]
        [ValidateJsonAntiForgeryToken]
        public JsonNetResult Index(int id)
        {
            var models = new List<CustomerRelationsModel>();
			
	        foreach (CustomerRelations customerRelations in customerRelationsRepository.ByCustomer(id))
	        {
		        models.Add(GetModel(customerRelations));
	        }

            return this.JsonNet(models);
        }

    }
}
