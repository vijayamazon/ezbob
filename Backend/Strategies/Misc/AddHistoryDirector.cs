using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezbob.Backend.Strategies.Misc
{
    using Ezbob.Backend.Models;
    using Ezbob.Database;

    public class AddHistoryDirector :  AStrategy {

        public AddHistoryDirector(Esigner edirector) {
            this.EDirector = edirector;
        }
        public override string Name
        {
            get { return "Add DirectorHistory"; }
        }
        public override void Execute() {

            DB.ExecuteNonQuery("AddHistoryDirector", CommandSpecies.StoredProcedure,
                 new QueryParameter("@DirectorID", this.EDirector.DirectorID),
                 new QueryParameter("@CustomerID", this.EDirector.CustomerID),
                 new QueryParameter("@Name", this.EDirector.FirstName),
                 new QueryParameter("@DateOfBirth", this.EDirector.BirthDate),
                 new QueryParameter("@Middle", this.EDirector.MiddleName),
                 new QueryParameter("@Surname", this.EDirector.LastName),
                 new QueryParameter("@Gender", this.EDirector.Gender),
                 new QueryParameter("@Email", this.EDirector.Email),
                 new QueryParameter("@Phone", this.EDirector.MobilePhone),
                 new QueryParameter("@CompanyId", this.EDirector.CompanyId),
                 new QueryParameter("@IsShareholder", this.EDirector.IsShareholder),
                 new QueryParameter("@IsDirector",this.EDirector.IsDirector ),
                 new QueryParameter("@UserId", this.EDirector.UserId)
                );
        }
        private readonly Esigner EDirector;
    }
}
