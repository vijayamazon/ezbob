using System;

namespace EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Data
{
	public static class CallProcedureHelper
	{
		public static CallProcedureInfo CreateInfo( CallProcedureType callProcedureType )
		{
			string serviceName;

			string description = string.Empty;
			string displayName = string.Empty;

			switch (callProcedureType.Type)
			{
				case CallProcedureTypeEnum.ConfirmIdentity:
					serviceName = "ConfirmIdentity";
					break;

				case CallProcedureTypeEnum.FetchToken:
					serviceName = "FetchToken";
					break;

				case CallProcedureTypeEnum.GetAccount:
					serviceName = "GetAccount";
					break;

				case CallProcedureTypeEnum.GeteBayDetails:
					serviceName = "GeteBayDetails";
					break;

				case CallProcedureTypeEnum.GetTokenStatus:
					serviceName = "GetTokenStatus";
					break;

				case CallProcedureTypeEnum.GetSessionId:
					serviceName = "GetSessionId";
					break;

				case CallProcedureTypeEnum.GetUser:
					serviceName = "GetUser";
					break;

				case CallProcedureTypeEnum.GetOrders:
					serviceName = "GetOrders";
					break;

                case CallProcedureTypeEnum.GetFeedBack:
                    serviceName = "GetFeedBack";
                    break;

				case CallProcedureTypeEnum.GetCategories:
					serviceName = "GetCategories";
					break;

				case CallProcedureTypeEnum.GetItem:
					serviceName = "GetItem";
					break;

				case CallProcedureTypeEnum.GeteBayOfficialTime:
					serviceName = "GeteBayOfficialTime";
					break;

				default:
					throw new NotImplementedException();
			}

			return new CallProcedureInfo( serviceName, description, displayName );
		}
	}
}
