using System;
using EZBob.DatabaseLib.Model.Database.Repository;
using StructureMap;
using log4net;

namespace ZohoCRM
{
    public class EzBobZohoApi
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (EzBobZohoApi));
        private readonly ICustomerRepository _customerRepository;
        private readonly IZohoFacade _zohoFacade;

        public EzBobZohoApi()
        {
            _zohoFacade = ObjectFactory.GetInstance<IZohoFacade>();
            _customerRepository = ObjectFactory.GetInstance<ICustomerRepository>();
        }

        public void UpdateOrCreate(int customerId)
        {
            try
            {
                _customerRepository.EnsureTransaction(
                    () => _zohoFacade.UpdateOrCreate(_customerRepository.Get(customerId)));
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}