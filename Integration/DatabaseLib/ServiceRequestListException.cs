using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EZBob.DatabaseLib
{
	public class ServiceRequestListException : Exception, IServiceRequestException, IEnumerable<ServiceFaultDetail>
	{
		public IEnumerable<ServiceFaultDetail> Faults { get; private set; }

		internal ServiceRequestListException( IEnumerable<ServiceFaultDetail> faults, Exception ex )
			:base(null, ex)
		{
			Faults = faults.ToArray();
		}

		public bool HasErrorWithCode(string code)
		{
			return Faults.Any( f => string.Equals( f.ErrorCode, code, StringComparison.InvariantCultureIgnoreCase ) );
		}

		public override string Message
		{
			get
			{
				if ( Faults.Count() == 1 )
				{
					return Faults.First().DetailedMessage;
				}
				else
				{
					var errors = Faults.Select( ( s, i ) => string.Format( "#{0}: {1}", i + 1, s.DetailedMessage ) );
					return string.Join( "\n", errors );
				}
			}
		}

		public IEnumerator<ServiceFaultDetail> GetEnumerator()
		{
			return Faults.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}