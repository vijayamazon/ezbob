using System;
using EzBob.TeraPeakServiceLib.Requests.SellerResearch;

namespace EzBob.TeraPeakServiceLib
{
	internal class TeraPearServiceMock : ITeraPeakService
	{
		private readonly DateTime _FirstDate;
		private readonly DateTime _LastDate;
		//public static readonly DateTime FirstDate = new DateTime(2011, 12, 30);
		//public static readonly DateTime LastDate = new DateTime( 2012, 03, 30 );

		public TeraPearServiceMock(DateTime firstDate, DateTime lastDate)
		{
			_FirstDate = firstDate;
			_LastDate = lastDate;
		}

		public GetSellerResearchResults RequestData( SearchQueryDatesRange req )
		{
			ModifiedDateQuery modifiedQuery = null;

			if ( req.EndDate < _FirstDate )
			{
				modifiedQuery = new ModifiedDateQuery
				                	{
				                		Dates = new ModifiedDateRange
				                		        	{
				                		        		StartDate = _FirstDate,
				                		        		EndDate = _FirstDate
				                		        	}
				                	};
			}
			else if ( req.StartDate > _LastDate )
			{
				modifiedQuery = new ModifiedDateQuery
				                	{
				                		Dates = new ModifiedDateRange
				                		        	{
				                		        		StartDate = _LastDate,
				                		        		EndDate = _LastDate
				                		        	}
				                	};
			}
			else if ( req.StartDate <  _FirstDate && req.EndDate > _LastDate )
			{
				modifiedQuery = new ModifiedDateQuery
				                	{
				                		Dates = new ModifiedDateRange
				                		        	{
				                		        		StartDate = _FirstDate,
				                		        		EndDate = _LastDate
				                		        	}
				                	};
			}
			else if(req.StartDate < _FirstDate && req.EndDate <= _LastDate)
			{
				modifiedQuery = new ModifiedDateQuery
				                	{
				                		Dates = new ModifiedDateRange
				                		        	{
				                		        		StartDate = _FirstDate,
				                		        		EndDate = req.EndDate
				                		        	}
				                	};
			}
			else if(req.StartDate >= _FirstDate && req.EndDate > _LastDate)
			{
				modifiedQuery = new ModifiedDateQuery
				                	{
				                		Dates = new ModifiedDateRange
				                		        	{
				                		        		StartDate = req.StartDate,
				                		        		EndDate = _LastDate
				                		        	}
				                	};
			}

			if ( modifiedQuery != null )
			{
				var dates = modifiedQuery.Dates;
				if ( dates.StartDate < _FirstDate || dates.EndDate > _LastDate )
				{
					throw new InvalidProgramException();
				}
			}
			else
			{
				var dates = req;
				if ( dates.StartDate < _FirstDate || dates.EndDate > _LastDate )
				{
					throw new InvalidProgramException();
				}
			}

			return new GetSellerResearchResults
			       	{
			       		ModifiedQuery = modifiedQuery
			       	};
		}

		public GetSellerResearchResults SearchBySeller(string sellerId, ResultSellerInfo resultSellerInfo, SearchQueryDatesRange searchQueryDates)
		{
			return RequestData( searchQueryDates );
		}
	}
}