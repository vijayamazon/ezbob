using System;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Email;

namespace EzBob.Web.Code.Email
{
    public class EmailConfirmation : IEmailConfirmation
    {
        private readonly IEmailConfirmationRequestRepository _requests;

        public EmailConfirmation(IEmailConfirmationRequestRepository requests)
        {
            _requests = requests;
        }

        public string GenerateLink(Customer customer)
        {
            customer.EmailState = EmailConfirmationRequestState.Pending;
            var request = new EmailConfirmationRequest()
                              {
                                  Customer = customer,
                                  Date = DateTime.UtcNow,
                                  State = EmailConfirmationRequestState.Pending
                              };
            _requests.Save(request);
            var guid = request.Id.ToString();

            var address = string.Format("<a href='{0}/confirm/{1}'>click here</a>", ConfigManager.CurrentValues.Instance.CustomerSite.Value, guid);

            return address;
        }

        public void ConfirmEmail(Guid guid)
        {
            var request = _requests.Get(guid);
            if (request == null)
            {
                throw new EmailConfirmationRequestNotFoundException(guid.ToString());
            }
            var customer = request.Customer;

            if (request.State != EmailConfirmationRequestState.Pending)
            {
                throw new EmailConfirmationRequestInvalidStateException();
            }

            request.State = EmailConfirmationRequestState.Confirmed;
            customer.EmailState = EmailConfirmationRequestState.Confirmed;
        }

        public void ConfirmEmail(Customer customer)
        {
            customer.EmailState = EmailConfirmationRequestState.ManuallyConfirmed;
            var requests = _requests.PendingByCustomer(customer);
            foreach (var request in requests)
            {
                request.State = EmailConfirmationRequestState.ManuallyConfirmed;
            }
        }
    }
}