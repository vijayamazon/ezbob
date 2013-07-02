using System;
using System.Threading;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using Scorto.Flow.Signal;
using StructureMap;
using ZohoCRM;
using log4net;

namespace EzBob.Signals.ZohoCRM
{
    public class ZohoHandler : SignalHandlerBase<ZohoMessage>
    {
        private static readonly ILog Log = LogManager.GetLogger("EzBob.Signals.ZohoCRM.UpdatingOrCreatingCustomer");

        public override void Execute()
        {
            var customerRepository = ObjectFactory.GetInstance<CustomerRepository>();
            var zohoFacade = ObjectFactory.GetInstance<IZohoFacade>();

            for (var i = 0; i < 5; i++ )
            {
                var customer = customerRepository.TryGet(Message.CustomerId);
                if (customer != null)
                {
                    _Execute(customer, customerRepository, zohoFacade);
                    Common.RemoveMessage(Signal.Id, Log);
                    return;
                }
                Thread.Sleep(5000);
            }
            Log.ErrorFormat("Cannot  update customer Id={0}. Reason: customer was not found.", Message.CustomerId);
        }

        private void _Execute(Customer customer, IRepository<Customer> customerRepository, IZohoFacade zohoFacade)
        {
            Log.InfoFormat("Trying to update customer Id={0}", Message.CustomerId);

            try
            {
                customerRepository.EnsureTransaction(() =>
                    {
                        switch (Message.MethodType)
                        {
                            case ZohoMethodType.UpdateOrCreate:
                                zohoFacade.UpdateOrCreate(customer);
                                break;
                            case ZohoMethodType.RegisterLead:
                                zohoFacade.RegisterLead(customer);
                                break;
                            case ZohoMethodType.ConvertLead:
                                zohoFacade.ConvertLead(customer);
                                break;
                        }
                    });
            }
            catch (Exception e)
            {
                Log.Warn(e);
            }
        }

        public override object Clone()
        {
            return new ZohoHandler();
        }
    }
}
