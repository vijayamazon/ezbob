using System;
using EZBob.DatabaseLib.Model.Database;

namespace EzBob.Web.Code.Email
{
    public interface IEmailConfirmation
    {
        string GenerateLink(Customer customer);
        void ConfirmEmail(Guid guid);
        void ConfirmEmail(Customer customer);
    }
}