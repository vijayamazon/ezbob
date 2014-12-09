using System;
using System.Web.Services.Protocols;
using EzBob.CommonLib;

namespace EZBob.DatabaseLib
{
	using Ezbob.Utils.Serialization;

	public interface IServiceResponceExceptionWrapper
	{
		ServiceErrorType[] Errors { get; }
		Exception BaseException { get; }
	}

	public abstract class ServiceResponceExceptionWrapperBase : IServiceResponceExceptionWrapper
	{
		public ServiceErrorType[] Errors { get; protected set; }
		public Exception BaseException { get; protected set; }
	}

	public class ServiceErrorType
	{
		public string LongMessage { get; set; }
		public string ErrorCode { get; set; }
		public string SeverityCode { get; set; }
	}

	public class ServiceRequestException : Exception, IServiceRequestException
	{
		public ServiceFaultDetail Fault { get; private set; }

		internal ServiceRequestException( ServiceFaultDetail fault, Exception ex )
			:base(fault.DetailedMessage, ex)
		{
			Fault = fault;
		}

		public static bool TryExtractErrorMessage( SoapException ex, out ServiceFaultDetail fault)
		{
			fault = null;
			if ( ex == null )
			{
				return false;
			}

			try
			{
				fault = Serialized.Deserialize<ServiceFaultDetail>( ex.Detail.InnerXml );
			}
			catch
			{
				return false;
			}

			return true;
		}

		public bool HasErrorWithCode(string code)
		{
			return string.Equals( Fault.ErrorCode, code, StringComparison.InvariantCultureIgnoreCase );
		}

	}

	public interface IServiceRequestException
	{
		bool HasErrorWithCode( string code );
	}
}
