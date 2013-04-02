using System;
using EZBob.DatabaseLib.Model.Database;
using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Email
{

    public enum EmailConfirmationRequestState
    {
        /// <summary>
        /// The state of the email is unknown. For old customers.
        /// </summary>
        Unknown,
        /// <summary>
        /// Request was issued, but not confirmed
        /// </summary>
        Pending,
        /// <summary>
        /// The email in this request was confirmed
        /// </summary>
        Confirmed,
        /// <summary>
        /// The request is canceled and not valid any more
        /// </summary>
        Canceled,
        /// <summary>
        /// Manually confirmed customer
        /// </summary>
        ManuallyConfirmed
    }

    public class EmailConfirmationRequestStateType : EnumStringType<EmailConfirmationRequestState>
    {

    }


    public class EmailConfirmationRequest
    {
        virtual public Guid Id { get; set; }
        virtual public Customer Customer { get; set; }
        virtual public DateTime Date { get; set; }
        virtual public EmailConfirmationRequestState State { get; set; }
    }

    public class EmailConfirmationRequestMap : ClassMap<EmailConfirmationRequest>
    {
        public EmailConfirmationRequestMap()
        {
            Id(x => x.Id).GeneratedBy.GuidComb();
            References(x => x.Customer, "CustomerId");
            Map(x => x.Date, "`Date`");
            Map(x => x.State).CustomType<EmailConfirmationRequestStateType>();
        }
    }
}