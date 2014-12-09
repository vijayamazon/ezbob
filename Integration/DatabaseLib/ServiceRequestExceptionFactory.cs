using System;
using System.Collections.Generic;
using System.Linq;

namespace EZBob.DatabaseLib
{
	public static class ServiceRequestExceptionFactory
	{
		public static Exception Create( ServiceFaultDetail fault, Exception ex )
		{
			return new ServiceRequestException( fault, ex );
		}

		public static Exception Create( IEnumerable<ServiceFaultDetail> faults, Exception ex )
		{
			return new ServiceRequestListException( faults, ex );
		}

		public static Exception Create( IServiceResponceExceptionWrapper wrapper )
		{
			Exception ex = wrapper.BaseException;

			if ( wrapper.Errors == null || wrapper.Errors.Length == 0 )
			{
				return ex;
			}

			if ( wrapper.Errors.Length == 1 )
			{
				var err = wrapper.Errors.First();
				return Create( new ServiceFaultDetail
					                                    {
						                                    Severity = err.SeverityCode,
						                                    ErrorCode = err.ErrorCode,
						                                    DetailedMessage = err.LongMessage
					                                    }, 
				                                    ex );
			}
			else
			{
				return Create( 
					wrapper.Errors.Select( err => new ServiceFaultDetail
						                              {
							                              Severity = err.SeverityCode,
							                              ErrorCode = err.ErrorCode,
							                              DetailedMessage = err.LongMessage
						                              } 
						), 
					ex );
			}
		}

	}
}
