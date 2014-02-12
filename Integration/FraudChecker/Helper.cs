using System;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Fraud;

namespace FraudChecker
{
    public static class Helper
    {
        public static string PrepareResultForOutput(IEnumerable<FraudDetection> fraudDetections)
        {
            return string.Join("\n",
                fraudDetections.Select(
                    x =>
                        string.Format(
                            "<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td></tr>",
                            x.ExternalUser != null ? "External" : "Internal",
                            x.CurrentField,
                            x.CompareField,
                            x.Value,
                            x.Concurrence)));
        }

        public static string ConcurrencePrepare(FraudDetection val)
        {
            if (val.ExternalUser != null)
            {
                return string.Format("{0} {1} (id={2})",
                    val.ExternalUser.FirstName,
                    val.ExternalUser.LastName,
                    val.ExternalUser.Id);
            }

            string fullname;
            int id;

            if (val.InternalCustomer == null) //for own check as DOB<21
            {
                fullname = val.CurrentCustomer.PersonalInfo.Fullname;
                id = val.CurrentCustomer.Id;
            }
            else
            {
                fullname = val.InternalCustomer.PersonalInfo != null ? val.InternalCustomer.PersonalInfo.Fullname : "-";
                id = val.InternalCustomer.Id;
            }
            return string.Format("{0} (id={1})", fullname, id);
        }

        public static Dictionary<string, string> GetCustomerPhones(Customer customer)
        {
            var retVal = new Dictionary<string, string>();
            if (customer.PersonalInfo == null)
            {
                return retVal;
            }
	        
			var businessPhone = customer.Company.BusinessPhone;
	        retVal.Add("BusinessPhone", businessPhone);
            
            if (customer.PersonalInfo != null && !string.IsNullOrEmpty(customer.PersonalInfo.DaytimePhone))
                retVal.Add("DaytimePhone", customer.PersonalInfo.DaytimePhone);

            if (customer.PersonalInfo != null && !string.IsNullOrEmpty(customer.PersonalInfo.MobilePhone))
                retVal.Add("MobilePhone", customer.PersonalInfo.MobilePhone);

            return retVal;
        }

        public static FraudDetection CreateDetection(string currentField, Customer currentCustomer,
                                                      Customer internalCustomer, string compareField,
                                                      FraudUser externalUser,
                                                      string value)
        {
            return new FraudDetection
            {
                CompareField = compareField,
                CurrentCustomer = currentCustomer,
                InternalCustomer = internalCustomer,
                CurrentField = currentField,
                ExternalUser = externalUser,
                Value = value ?? String.Empty
            };
        }

	    public static IEnumerable<string> GetCustomerIps(Customer customer)
	    {
		    return customer.Session.Select(s => s.Ip);
	    }

	    public static void AddValue(Dictionary<Customer, List<MpPhone>> mpPhoneDetections, Customer cd, string mpType, string phone)
	    {
			if (!mpPhoneDetections.ContainsKey(cd))
			{
				mpPhoneDetections[cd] = new List<MpPhone> {new MpPhone {MpType = mpType, Phone = phone}};
			}
			else
			{
				mpPhoneDetections[cd].Add(new MpPhone { MpType = mpType, Phone = phone });
			}
	    }
    }
}
