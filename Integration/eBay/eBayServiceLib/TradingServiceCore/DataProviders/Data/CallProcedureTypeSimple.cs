namespace EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Data
{
	public class CallProcedureTypeSimple : CallProcedureType
	{
		public static readonly CallProcedureType ConfirmIdentity = new CallProcedureTypeSimple( CallProcedureTypeEnum.ConfirmIdentity );
		public static readonly CallProcedureType FetchToken = new CallProcedureTypeSimple( CallProcedureTypeEnum.FetchToken );
		public static readonly CallProcedureType GetSessionId = new CallProcedureTypeSimple( CallProcedureTypeEnum.GetSessionId );

		public CallProcedureTypeSimple(CallProcedureTypeEnum type) 
			: base(type)
		{
		}

		public override bool IsTokenDependent
		{
			get { return false; }
		}
	}
}