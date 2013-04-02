namespace EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Data
{
	public class CallProcedureTypeTokenDependent : CallProcedureType
	{
		public static readonly CallProcedureType GetAccount = new CallProcedureTypeTokenDependent( CallProcedureTypeEnum.GetAccount );
		public static readonly CallProcedureType GeteBayDetails = new CallProcedureTypeTokenDependent( CallProcedureTypeEnum.GeteBayDetails );
		public static readonly CallProcedureType GetOrders = new CallProcedureTypeTokenDependent( CallProcedureTypeEnum.GetOrders );
		public static readonly CallProcedureType GetTokenStatus = new CallProcedureTypeTokenDependent( CallProcedureTypeEnum.GetTokenStatus );
		public static readonly CallProcedureType GetUser = new CallProcedureTypeTokenDependent( CallProcedureTypeEnum.GetUser );
        public static readonly CallProcedureType GetFeedback = new CallProcedureTypeTokenDependent(CallProcedureTypeEnum.GetFeedBack );
		public static readonly CallProcedureType GetCategories = new CallProcedureTypeTokenDependent( CallProcedureTypeEnum.GetCategories );
		public static readonly CallProcedureType GetItem = new CallProcedureTypeTokenDependent( CallProcedureTypeEnum.GetItem );
		public static readonly CallProcedureType GeteBayOfficialTime = new CallProcedureTypeTokenDependent( CallProcedureTypeEnum.GeteBayOfficialTime );

		public CallProcedureTypeTokenDependent(CallProcedureTypeEnum type) 
			: base(type)
		{
		}

		public override bool IsTokenDependent
		{
			get { return true; }
		}
	}
}