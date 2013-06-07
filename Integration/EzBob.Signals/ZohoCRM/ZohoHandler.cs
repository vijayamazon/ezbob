using System;
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
            Log.InfoFormat("Update customer Id={0}", Message.CustomerId);
            var zohoFacade = ObjectFactory.GetInstance<IZohoFacade>();
            var customerRepository = ObjectFactory.GetInstance<CustomerRepository>();
            try
            {
                customerRepository.EnsureTransaction(() =>
                    {
                        var customer = customerRepository.Get(Message.CustomerId);

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
                Log.Error(e);
            }
            finally
            {
                Common.RemoveMessage(Signal.Id, Log);
            }
        }

        public override object Clone()
        {
            return new ZohoHandler();
        }
    }
}
