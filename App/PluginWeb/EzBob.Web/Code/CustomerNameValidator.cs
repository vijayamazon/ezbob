using System.Text.RegularExpressions;

namespace EzBob.Web.Areas.Customer.Controllers
{
    public interface ICustomerNameValidator
    {
        bool CheckCustomerName(string customer, string firstName, string surname);
    }

    public class FakeCustomerNameValidator : ICustomerNameValidator
    {
        public bool CheckCustomerName(string customer, string firstName, string surname)
        {
            return true;
        }
    }

    public class CustomerNameValidator : ICustomerNameValidator
    {
        public virtual bool CheckCustomerName(string customer, string firstName, string surname)
        {
            // if didn't get customer from paypoint, assume it is valid name
            if (string.IsNullOrEmpty(customer)) return true;

            var m = Regex.Match(customer, @"^\s*(\w+)\s+(\w+)\s*$");

            if (m.Groups.Count != 3) return false;

            firstName = firstName.ToLower();
            surname = surname.ToLower();

            var cName = m.Groups[1].Value.ToLower();
            var cSurname = m.Groups[2].Value.ToLower();

            if (cName == firstName && cSurname == surname) return true;

            return false;
        } 
    }
}