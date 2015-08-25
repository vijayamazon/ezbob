using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayGround {
    using DAL;
    using DAL.ThirdParty;
    using Models;
    using StructureMap;

    internal class Program {

        private static readonly string TheConnectionString = "Server=localhost;Database=ezbob;User Id=ezbobuser;Password=ezbobuser;MultipleActiveResultSets=true";
        private static readonly string ConnectionString = "connectionString";

        private static void Main(string[] args) {

            IContainer container = ConfigureDependenices();
            DalService service = container.GetInstance<DalService>();
            CustomerDetails customerDetails = service.GetCustomerDetails(27);
            int kk = 0;
        }

        private static IContainer ConfigureDependenices() {
            return new Container(c => {
                c.ForSingletonOf<DalService>()
                    .Use<DalService>();
                c.ForSingletonOf<CustomerQueries>()
                    .Use<CustomerQueries>()
                    .Ctor<string>(ConnectionString)
                    .Is(TheConnectionString);
                c.ForSingletonOf<ExperianQuery>()
                    .Use<ExperianQuery>()
                    .Ctor<string>(ConnectionString)
                    .Is(TheConnectionString);
            });
        }
    }
}
