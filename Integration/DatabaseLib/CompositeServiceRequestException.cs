using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EZBob.DatabaseLib
{
	public class CompositeServiceRequestException : Exception, IEnumerable<Exception>
	{
		public IEnumerable<Exception> Exceptions { get; private set; }

		public CompositeServiceRequestException( IEnumerable<Exception> exceptions )
		{
			Exceptions = exceptions.ToArray();
			
		}

		public override string Message
		{
			get
			{
				var count = Exceptions.Count();

				if ( count > 1 )
				{
					var errors = Exceptions.Select( ( s, i ) => string.Format( "#{0}: {1}", i + 1, s.Message ) );
					return string.Join( "\n", errors );
				}
				else if ( count == 1 )
				{
					return Exceptions.Select( s => s.Message ).First();
				}

				return string.Empty;
			}
		}

		public string MessageWithCallStack
		{
			get
			{
				var count = Exceptions.Count();

				if ( count > 1 )
				{
					var errors = Exceptions.Select( ( s, i ) => string.Format( "#{0}: {1}\n{2}", i + 1, s.Message, s.StackTrace ) );
					return string.Join( "\n\n", errors );
				}
				else if ( count == 1 )
				{
					return Exceptions.Select( s => string.Format("{0}\n{1}", s.Message, s.StackTrace) ).First();
				}

				return string.Empty;
			}
		}

		public IEnumerator<Exception> GetEnumerator()
		{
			return Exceptions.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}