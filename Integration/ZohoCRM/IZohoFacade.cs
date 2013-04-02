using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Loans;

namespace ZohoCRM
{
    public interface IZohoFacade
    {
        /// <summary>
        /// Registers new lead in Zoho CRM
        /// </summary>
        /// <param name="customer"></param>
        void RegisterLead(Customer customer);
        void UpdateLoans(Customer customer);
        void ConvertLead(Customer customer);
        void UpdateCustomer(Customer customer);
        void AddFile(string id, string fileName, byte[] doc);
        void CreateOffer(Customer customer, CashRequest cashRequest);
        void CreateLoan(Customer customer, Loan loan);
        void MoreAMLInformation(Customer customer);
        void MoreBWAInformation(Customer customer);
        void RejectOffer(CashRequest cashRequest);
        void ApproveOffer(CashRequest cashRequest);
        void UpdateCashRequest(CashRequest cr);
        void UpdateOrCreate(Customer customer);
        void UpdateOfferOnGetCash(CashRequest cashRequest, Customer customer);
    }

    public enum ZohoMethodType
    {
        RegisterLead = 0,
        ConvertLead = 1,
        UpdateOrCreate = 2
    }
}