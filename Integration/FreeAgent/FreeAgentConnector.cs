namespace FreeAgent
{
    using System;
    using System.Web.Script.Serialization;
    using EZBob.DatabaseLib.DatabaseWrapper.Order;
    using log4net;

	public class FreeAgentConnector
	{
		private static readonly ILog _Log = LogManager.GetLogger(typeof(FreeAgentConnector));

        public bool Validate(string userName, out string errMsg, out string token)
        {
            if (1 == 2)
            {
                errMsg = "Invalid Credentials";
                return false;
            }

            errMsg = string.Empty;
	        token = string.Empty;

			// keep token\s
			// make api call to company at least for company name 
			// make sure that token\s and company details are kept in security info
			// make sure name is kept as display name

            return true;
        }

		public static FreeAgentOrdersList GetOrders(string userName, FreeAgentSecurityInfo securityInfo, DateTime fromDate) /*should get order id?*/
        {
			// get another token

			// get invoices

			//FreeAgentStub
	        string response =
		        "{\"invoices\":[{\"url\":\"https://api.freeagent.com/v2/invoices/6450854\",\"contact\":\"https://api.freeagent.com/v2/contacts/1739223\",\"dated_on\":\"2013-04-10\",\"due_on\":\"2013-05-10\",\"reference\":\"001\",\"currency\":\"GBP\",\"exchange_rate\":\"1.0\",\"net_value\":\"180.0\",\"total_value\":\"180.0\",\"paid_value\":\"180.0\",\"due_value\":\"0.0\",\"status\":\"Paid\",\"omit_header\":false,\"payment_terms_in_days\":30,\"paid_on\":\"2013-06-05\",\"invoice_items\":[{\"url\":\"https://api.freeagent.com/v2/invoice_items/10612989\",\"position\":1,\"description\":\"first\",\"item_type\":\"Hours\",\"price\":\"40.0\",\"quantity\":\"3.0\",\"category\":\"https://api.freeagent.com/v2/categories/001\"},{\"url\":\"https://api.freeagent.com/v2/invoice_items/10612990\",\"position\":2,\"description\":\"second\",\"item_type\":\"Hours\",\"price\":\"30.0\",\"quantity\":\"2.0\",\"category\":\"https://api.freeagent.com/v2/categories/001\"}]}]}";


			var js = new JavaScriptSerializer();
			var invoicesList = (InvoicesList)js.Deserialize(response, typeof(InvoicesList));
			var freeAgentOrdersItemList = new FreeAgentOrdersList(DateTime.UtcNow, invoicesList.Invoices);
			return freeAgentOrdersItemList;
        }
	}
}