using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezbob.Backend.Strategies.Investor
{
    using Ezbob.Backend.Models.Investor;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using Ezbob.Database;

    public class SaveInvestorContactList : AStrategy
    {
        public SaveInvestorContactList(int investor, IEnumerable<InvestorContactModel> contacts)
        {

            this.InvestorID = investor;
			this.contacts = contacts;
		
		}//ctor
        public override string Name { get { return "SaveInvestorContactList"; } }
        public override void Execute()
        {
            DateTime now = DateTime.UtcNow;
            try {
                var dbContacts = new List<I_InvestorContact>();
                foreach (var contact in this.contacts)
                {
                    dbContacts.Add(new I_InvestorContact
                    {
                        InvestorContactID = contact.InvestorContactID,
                        InvestorID = this.InvestorID,
                        PersonalName = contact.PersonalName,
                        LastName = contact.LastName,
                        Email = contact.Email,
                        Role = contact.Role,
                        Comment = contact.Comment,
                        IsPrimary = contact.IsPrimary,
                        Mobile = contact.Mobile,
                        OfficePhone = contact.OfficePhone,
                        IsActive = contact.IsActive,
                        Timestamp = now,
                    });
                }

                DB.ExecuteNonQuery("I_InvestorEditContactList", CommandSpecies.StoredProcedure,
                    DB.CreateTableParameter<I_InvestorContact>("Tbl", dbContacts)
                    );
            }
            catch (Exception ex)
            {
                Log.Warn(ex, "Failed to edit investor contract on DB");
              
                Result = false;
                throw;
            }
            Result = true;

        }
        public bool Result { get; set; }
        public int InvestorID { get; private set; }

        private readonly IEnumerable<InvestorContactModel> contacts;
       
    }

}
