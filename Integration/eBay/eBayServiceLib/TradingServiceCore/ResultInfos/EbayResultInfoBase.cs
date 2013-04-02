using System;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib;
using EzBob.CommonLib;
using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;

namespace EzBob.eBayServiceLib.TradingServiceCore.ResultInfos
{
	public abstract class EbayResultInfoBase : ResultInfoBase, IEbayDataInfo
	{
		protected EbayResultInfoBase( DateTime submittedDate ) 
			: base(submittedDate)
		{
		}

		protected EbayResultInfoBase( IEnumerable<ErrorInfo> errors, DateTime submittedDate )
			:base(errors, submittedDate)
		{
		}
		
		public abstract DataInfoTypeEnum DataInfoType { get; }

		public RequestsCounterData RequestsCounter { get; set; }

		public void IncrementRequests( string method = null, string details = null )
		{
			if ( RequestsCounter == null )
			{
				RequestsCounter = new RequestsCounterData();
			}
			RequestsCounter.IncrementRequests( method, details );
		}
	}

	public abstract class ResultInfoBase : IResultDataInfo
	{
		private readonly List<ErrorInfo> _Errors;

		protected ResultInfoBase( DateTime submittedDate )
		{
			SubmittedDate = submittedDate;
			_Errors = new List<ErrorInfo>();
		}

		protected ResultInfoBase( IEnumerable<ErrorInfo> errors, DateTime submittedDate )			
		{
			SubmittedDate = submittedDate;
			_Errors.AddRange( errors );
		}

		protected void AddErrors( IEnumerable<ErrorInfo> errors )
		{
			if ( errors == null || !errors.Any() )
			{
				return;
			}

			_Errors.AddRange( errors );
		}

		public int ErrorCount
		{
			get { return _Errors != null ? _Errors.Count : 0; }
		}

		public bool HasError
		{
			get { return ErrorCount > 0; }
		}

		public ErrorInfo[] Errors
		{
			get { return _Errors.ToArray(); }
		}

		public DateTime SubmittedDate { get; private set; }
	}
}