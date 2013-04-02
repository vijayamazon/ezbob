using System;
using System.Collections.Generic;
using EzBob.CommonLib;
using EzBob.CommonLib.MarketplaceSpecificTypes.TeraPeakOrdersData;
using EzBob.TeraPeakServiceLib.Requests.SellerResearch;
using NUnit.Framework;

namespace EzBob.TeraPeakServiceLib
{
	[TestFixture]
	internal class TerapeakTestFixture
	{
		[Test]
		public void TestParceDataWithError()
		{
			var resultString =
			@"<?xml version=""1.0"" encoding=""UTF-8""?>
<GetSellerResearchResults>
	<Timestamp>2012-04-17 09:08:03</Timestamp>
	<ProcessingTime>0.042</ProcessingTime>
	<ImageURL><![CDATA[http://api.dataunison.com/images/dataunison_powered_by_114x36.gif]]></ImageURL>
	<LinkURL><![CDATA[http://www.dataunison.com/ebay_research/api.php]]></LinkURL>
	<CallsRemaining>494</CallsRemaining>
	<CallLimitResetTime>2012-04-18 01:00:00</CallLimitResetTime>
	<Errors>
		<Error id = '2153'>Error</Error>
	</Errors>
</GetSellerResearchResults>";

			var rez = SerializeDataHelper.DeserializeTypeFromString<GetSellerResearchResults>( resultString );

			Assert.NotNull( rez );
			Assert.IsNull( rez.SearchResults );
			Assert.NotNull( rez.Errors );
			Assert.IsTrue( rez.HasError );
			Assert.AreEqual( rez.Errors.Length, 1 );
			Assert.NotNull(rez.Errors[0].Error);
			Assert.AreEqual( rez.Errors[0].Error.Id, 2153 );
			Assert.AreEqual( rez.Errors[0].Error.Error, "Error" );
		}

		[Test]
		public void TestParceString()
		{
			var resultString =
				@"<?xml version=""1.0"" encoding=""UTF-8""?>
<GetSellerResearchResults>
	<Timestamp>2012-04-13 04:38:20</Timestamp>
	<ProcessingTime>0.087</ProcessingTime>
	<ImageURL><![CDATA[http://api.dataunison.com/images/dataunison_powered_by_114x36.gif]]></ImageURL>
	<LinkURL><![CDATA[http://www.dataunison.com/ebay_research/api.php]]></LinkURL>
	<CallsRemaining>490</CallsRemaining>
	<CallLimitResetTime>2012-04-14 01:00:00</CallLimitResetTime>
	<ModifiedQuery>
		<Dates>
			<StartDate>2012-01-01</StartDate>
			<EndDate>2012-01-01</EndDate>
		</Dates>
	</ModifiedQuery>
	<SearchResults>
		<Statistics>
			<Revenue>1.23</Revenue>
			<Listings>4</Listings>
			<Successful>5</Successful>
			<Bids>6</Bids>
			<ItemsOffered>7</ItemsOffered>
			<ItemsSold>8</ItemsSold>
			<AverageSellersPerDay>9</AverageSellersPerDay>
			<SuccessRate>10.11</SuccessRate>
		</Statistics>
	</SearchResults>
</GetSellerResearchResults>
";

			var rez = SerializeDataHelper.DeserializeTypeFromString<GetSellerResearchResults>(resultString);

			Assert.NotNull(rez);
			Assert.NotNull(rez.SearchResults);

		}

		[Test]
		public void TestSerializeEmptyData()
		{
			var dataEmpty = new GetSellerResearchResults
			                	{
			                		CallsRemaining = 100,
			                		SearchResults = new SearchQueryResult
			                		                	{
			                		                		Statistics = new QueryResultStatistic
			                		                		             	{
			                		                		             	}
			                		                	}
			                	};
			var xmlRezEmpty = SerializeDataHelper.SerializeToString( dataEmpty );

		}

		[Test]
		public void TestParceEmptyDataString()
		{
			var resultStringEmpty =
				@"<?xml version=""1.0"" encoding=""UTF-8""?>
<GetSellerResearchResults>
	<Timestamp>2012-04-13 04:38:20</Timestamp>
	<ProcessingTime>0.087</ProcessingTime>
	<ImageURL><![CDATA[http://api.dataunison.com/images/dataunison_powered_by_114x36.gif]]></ImageURL>
	<LinkURL><![CDATA[http://www.dataunison.com/ebay_research/api.php]]></LinkURL>
	<CallsRemaining>490</CallsRemaining>
	<CallLimitResetTime>2012-04-14 01:00:00</CallLimitResetTime>
	<ModifiedQuery>
		<Dates>
			<StartDate>2012-01-01</StartDate>
			<EndDate>2012-01-01</EndDate>
		</Dates>
	</ModifiedQuery>
	<SearchResults>
		<Statistics>
			<Revenue/>
			<Listings/>
			<Successful/>
			<Bids/>
			<ItemsOffered/>
			<ItemsSold/>
			<AverageSellersPerDay/>
			<SuccessRate/>
		</Statistics>
	</SearchResults>
</GetSellerResearchResults>
";


			var rezEmpty = SerializeDataHelper.DeserializeTypeFromString<GetSellerResearchResults>( resultStringEmpty );

			Assert.NotNull( rezEmpty );
			
		}

		[Test]
		public void TestMockService()
		{
			TestMockService1();
			TestMockService2();
			TestMockService3();
			TestMockService4();
			TestMockServiceComposite();
		}

		[Test]
		public void TestMockService1()
		{
			var serviseFirstDate = new DateTime( 2012, 1, 2 );
			var serviceLastDate = new DateTime( 2012, 1, 7 );
			var service = new TeraPearServiceMock( serviseFirstDate, serviceLastDate );

			DateTime dLeftOut1 = new DateTime( 2012, 1, 1 );
			DateTime dRightOut1 = new DateTime( 2012, 1, 31 );

			var r1 = new SearchQueryDatesRange( dLeftOut1, dRightOut1, RangeMarkerType.Temporary);
			var rez1 = service.RequestData( r1 );
			Assert.NotNull( rez1.ModifiedQuery );
			Assert.NotNull( rez1.ModifiedQuery.Dates );
			Assert.IsTrue( rez1.ModifiedQuery.Dates.StartDate.Equals( serviseFirstDate ) );
			Assert.IsTrue( rez1.ModifiedQuery.Dates.EndDate.Equals( serviceLastDate ) );
		}

		[Test]
		public void TestMockService2()
		{
			var serviseFirstDate = new DateTime( 2012, 1, 1 );
			var serviceLastDate = new DateTime( 2012, 1, 7 );
			var service = new TeraPearServiceMock( serviseFirstDate, serviceLastDate );

			DateTime d1 = new DateTime( 2012, 1, 1 );
			DateTime dRightOut1 = new DateTime( 2012, 1, 31 );

			var r1 = new SearchQueryDatesRange( d1, dRightOut1, RangeMarkerType.Temporary);
			var rez1 = service.RequestData( r1 );
			Assert.NotNull( rez1.ModifiedQuery );
			Assert.NotNull( rez1.ModifiedQuery.Dates );
			Assert.IsTrue( rez1.ModifiedQuery.Dates.StartDate.Equals( serviseFirstDate ) );
			Assert.IsTrue( rez1.ModifiedQuery.Dates.EndDate.Equals( serviceLastDate ) );
		}

		[Test]
		public void TestMockService3()
		{
			var serviseFirstDate = new DateTime( 2012, 1, 12 );
			var serviceLastDate = new DateTime( 2012, 1, 31 );
			var service = new TeraPearServiceMock( serviseFirstDate, serviceLastDate );

			DateTime dLeftOut1 = new DateTime( 2012, 1, 1 );
			DateTime d2 = new DateTime( 2012, 1, 31 );

			var r1 = new SearchQueryDatesRange( dLeftOut1, d2, RangeMarkerType.Temporary);
			var rez1 = service.RequestData( r1 );
			Assert.NotNull( rez1.ModifiedQuery );
			Assert.NotNull( rez1.ModifiedQuery.Dates );
			Assert.IsTrue( rez1.ModifiedQuery.Dates.StartDate.Equals( serviseFirstDate ) );
			Assert.IsTrue( rez1.ModifiedQuery.Dates.EndDate.Equals( serviceLastDate ) );
		}

		[Test]
		public void TestMockService4()
		{
			var serviseFirstDate = new DateTime( 2011, 1, 12 );
			var serviceLastDate = new DateTime( 2012, 3, 31 );
			var service = new TeraPearServiceMock( serviseFirstDate, serviceLastDate );

			DateTime d1 = new DateTime( 2012, 1, 1 );
			DateTime d2 = new DateTime( 2012, 1, 31 );

			var r1 = new SearchQueryDatesRange( d1, d2, RangeMarkerType.Temporary);
			var rez1 = service.RequestData( r1 );
			Assert.IsNull( rez1.ModifiedQuery );			
		}

		[Test]
		public void TestMockServiceComposite()
		{
			DateTime dLeftOut1 = new DateTime(2011, 11, 10);
			DateTime dLeftOut2 = new DateTime(2011, 11, 25);
			DateTime dRightOut1 = new DateTime( 2012, 4, 1 );
			DateTime dRightOut2 = new DateTime( 2012, 4, 25 );
			DateTime d1 = new DateTime( 2012, 2, 20 );
			DateTime d2 = new DateTime( 2012, 3, 1 );
			
			var r1 = new SearchQueryDatesRange( dLeftOut1, dLeftOut2, RangeMarkerType.Temporary);
			var r2 = new SearchQueryDatesRange( dLeftOut2, d1, RangeMarkerType.Temporary);
			var r3 = new SearchQueryDatesRange( d2, dRightOut1, RangeMarkerType.Temporary);
			var r4 = new SearchQueryDatesRange( dRightOut1, dRightOut2, RangeMarkerType.Temporary);
			var r5 = new SearchQueryDatesRange( dLeftOut2, dRightOut1, RangeMarkerType.Temporary);
			var r6 = new SearchQueryDatesRange( d1, d2, RangeMarkerType.Temporary);

			var serviseFirstDate = new DateTime( 2011, 12, 14 );
			var serviceLastDate = new DateTime( 2012, 03, 18 );
			var service = new TeraPearServiceMock( serviseFirstDate, serviceLastDate );

			var rez1 = service.RequestData( r1 );
			Assert.NotNull( rez1.ModifiedQuery );
			Assert.NotNull( rez1.ModifiedQuery.Dates);
			Assert.IsTrue( rez1.ModifiedQuery.Dates.StartDate.Equals( serviseFirstDate ) );
			Assert.IsTrue( rez1.ModifiedQuery.Dates.EndDate.Equals( serviseFirstDate ) );

			var rez2 = service.RequestData( r2 );
			Assert.NotNull( rez2.ModifiedQuery );
			Assert.NotNull( rez2.ModifiedQuery.Dates );
			Assert.IsTrue( rez2.ModifiedQuery.Dates.StartDate.Equals( serviseFirstDate ) );
			Assert.IsTrue( rez2.ModifiedQuery.Dates.EndDate.Equals( r2.EndDate ) );

			var rez3 = service.RequestData( r3 );
			Assert.NotNull( rez3.ModifiedQuery );
			Assert.NotNull( rez3.ModifiedQuery.Dates );
			Assert.IsTrue( rez3.ModifiedQuery.Dates.StartDate.Equals( r3.StartDate ) );
			Assert.IsTrue( rez3.ModifiedQuery.Dates.EndDate.Equals( serviceLastDate ) );

			var rez4 = service.RequestData( r4 );
			Assert.NotNull( rez4.ModifiedQuery );
			Assert.NotNull( rez4.ModifiedQuery.Dates );
			Assert.IsTrue( rez4.ModifiedQuery.Dates.StartDate.Equals( serviceLastDate ) );
			Assert.IsTrue( rez4.ModifiedQuery.Dates.EndDate.Equals( serviceLastDate ) );

			var rez5 = service.RequestData( r5 );
			Assert.NotNull( rez5.ModifiedQuery );
			Assert.NotNull( rez5.ModifiedQuery.Dates );
			Assert.IsTrue( rez5.ModifiedQuery.Dates.StartDate.Equals( serviseFirstDate ) );
			Assert.IsTrue( rez5.ModifiedQuery.Dates.EndDate.Equals( serviceLastDate ) );

			var rez6 = service.RequestData( r6 );
			Assert.IsNull( rez6.ModifiedQuery );
			
		}

		[Test]
		public void TestQueue1()
		{
			var startRequestsDate = new DateTime( 2011, 04, 23 );
			var creationInfo = new TeraPeakRequestDataInfo
			{
				StartDate = startRequestsDate,
				CountSteps = 5,
				StepType = TeraPeakRequestStepEnum.ByMonth
			};

			List<SearchQueryDatesRange> ranges = TerapeakRequestsQueue.CreateQueriesDates( creationInfo, DateTime.Now );
			var queue = new TerapeakRequestsQueue( ranges );
			// Queries:
			// 1. 23/04/2011 - 30/04/2011
			// 2. 01/05/2011 - 31/05/2011
			// 3. 01/06/2012 - 30/06/2012
			// 4. 01/07/2012 - 31/07/2012
			// 5. 01/08/2012 - 31/08/2012
			var qData = queue.Items;
			Assert.AreEqual( queue.Count, 5 );
			Assert.AreEqual( qData[0].StartDate, new DateTime(2011, 4, 1) ); Assert.AreEqual( qData[0].EndDate, new DateTime(2011, 4, 30, 23, 59 , 59) ); 
			Assert.AreEqual( qData[1].StartDate, new DateTime(2011, 5, 1) ); Assert.AreEqual( qData[1].EndDate, new DateTime(2011, 5, 31, 23, 59 , 59) ); 
			Assert.AreEqual( qData[2].StartDate, new DateTime(2011, 6, 1) ); Assert.AreEqual( qData[2].EndDate, new DateTime(2011, 6, 30, 23, 59 , 59) ); 
			Assert.AreEqual( qData[3].StartDate, new DateTime(2011, 7, 1) ); Assert.AreEqual( qData[3].EndDate, new DateTime(2011, 7, 31, 23, 59 , 59) ); 
			Assert.AreEqual( qData[4].StartDate, new DateTime(2011, 8, 1) ); Assert.AreEqual( qData[4].EndDate, new DateTime(2011, 8, 31, 23, 59 , 59) );

			var r1 = queue.Peek();
			Assert.AreEqual( queue.Count, 5 );
			Assert.AreEqual( r1.StartDate, new DateTime(2011, 4, 1) ); Assert.AreEqual( r1.EndDate, new DateTime(2011, 4, 30, 23, 59 , 59) );

			var modifiedRange1 = new ModifiedDateQuery
				{
				
					Dates = new ModifiedDateRange
					{
						StartDate = new DateTime( 2011, 6, 10 ),
						EndDate = new DateTime( 2011, 6, 10 )
					}
				};

			queue.RemoveAndCorrectQueue(r1, modifiedRange1);

			Assert.AreEqual( queue.Count, 3 );
			qData = queue.Items;
			Assert.AreEqual( qData[0].StartDate, new DateTime(2011, 6, 10) ); Assert.AreEqual( qData[0].EndDate, new DateTime(2011, 6, 30, 23, 59 , 59) ); 
			Assert.AreEqual( qData[1].StartDate, new DateTime(2011, 7, 1) ); Assert.AreEqual( qData[1].EndDate, new DateTime(2011, 7, 31, 23, 59 , 59) ); 
			Assert.AreEqual( qData[2].StartDate, new DateTime(2011, 8, 1) ); Assert.AreEqual( qData[2].EndDate, new DateTime(2011, 8, 31, 23, 59 , 59) );

			var r2 = queue.Peek();
			Assert.AreEqual( queue.Count, 3 );
			Assert.AreEqual( r2.StartDate, new DateTime(2011, 6, 10) ); Assert.AreEqual( r2.EndDate, new DateTime(2011, 6, 30, 23, 59 , 59) );

			queue.RemoveAndCorrectQueue( r2, null );
			Assert.AreEqual( queue.Count, 2 );
			qData = queue.Items;
			Assert.AreEqual( qData[0].StartDate, new DateTime(2011, 7, 1) ); Assert.AreEqual( qData[0].EndDate, new DateTime(2011, 7, 31, 23, 59 , 59) ); 
			Assert.AreEqual( qData[1].StartDate, new DateTime(2011, 8, 1) ); Assert.AreEqual( qData[1].EndDate, new DateTime(2011, 8, 31, 23, 59 , 59) );

			var modifiedRange2 = new ModifiedDateQuery
			{

				Dates = new ModifiedDateRange
				{
					StartDate = new DateTime( 2011, 7, 1 ),
					EndDate = new DateTime( 2011, 7, 10 )
				}
			};

			var r3 = queue.Peek();
			Assert.AreEqual( queue.Count, 2 );
			Assert.AreEqual( r3.StartDate, new DateTime( 2011, 7, 1 ) ); Assert.AreEqual( r3.EndDate, new DateTime( 2011, 7, 31, 23, 59 , 59 ) );

			queue.RemoveAndCorrectQueue( r3, modifiedRange2 );
			Assert.AreEqual( queue.Count, 0 );


		}

		[Test]
		public void TestQueue2()
		{
			var startRequestsDate = new DateTime( 2011, 04, 23 );
			var creationInfo = new TeraPeakRequestDataInfo
			{
				StartDate = startRequestsDate,
				CountSteps = 5,
				StepType = TeraPeakRequestStepEnum.ByMonth
			};

			List<SearchQueryDatesRange> ranges = TerapeakRequestsQueue.CreateQueriesDates(creationInfo, DateTime.Now);
			var queue = new TerapeakRequestsQueue( ranges );
			// Queries:
			// 1. 23/04/2011 - 30/04/2011
			// 2. 01/05/2011 - 31/05/2011
			// 3. 01/06/2012 - 30/06/2012
			// 4. 01/07/2012 - 31/07/2012
			// 5. 01/08/2012 - 31/08/2012
			var qData = queue.Items;
			Assert.AreEqual( queue.Count, 5 );
			Assert.AreEqual( qData[0].StartDate, new DateTime(2011, 4, 1) ); Assert.AreEqual( qData[0].EndDate, new DateTime(2011, 4, 30, 23, 59 , 59) ); 
			Assert.AreEqual( qData[1].StartDate, new DateTime(2011, 5, 1) ); Assert.AreEqual( qData[1].EndDate, new DateTime(2011, 5, 31, 23, 59 , 59) ); 
			Assert.AreEqual( qData[2].StartDate, new DateTime(2011, 6, 1) ); Assert.AreEqual( qData[2].EndDate, new DateTime(2011, 6, 30, 23, 59 , 59) ); 
			Assert.AreEqual( qData[3].StartDate, new DateTime(2011, 7, 1) ); Assert.AreEqual( qData[3].EndDate, new DateTime(2011, 7, 31, 23, 59 , 59) ); 
			Assert.AreEqual( qData[4].StartDate, new DateTime(2011, 8, 1) ); Assert.AreEqual( qData[4].EndDate, new DateTime(2011, 8, 31, 23, 59 , 59) );

			var r1 = queue.Peek();
			Assert.AreEqual( queue.Count, 5 );
			Assert.AreEqual( r1.StartDate, new DateTime(2011, 4, 1) ); Assert.AreEqual( r1.EndDate, new DateTime(2011, 4, 30, 23, 59 , 59) );

			var modifiedRange1 = new ModifiedDateQuery
				{
				
					Dates = new ModifiedDateRange
					{
						StartDate = new DateTime( 2011, 4, 21 ),
						EndDate = new DateTime( 2011, 4, 30 )
					}
				};

			queue.RemoveAndCorrectQueue(r1, modifiedRange1);

			Assert.AreEqual( queue.Count, 4 );
			qData = queue.Items;
			Assert.AreEqual( qData[0].StartDate, new DateTime(2011, 5, 1) ); Assert.AreEqual( qData[0].EndDate, new DateTime(2011, 5, 31, 23, 59 , 59) ); 
			Assert.AreEqual( qData[1].StartDate, new DateTime(2011, 6, 1) ); Assert.AreEqual( qData[1].EndDate, new DateTime(2011, 6, 30, 23, 59 , 59) ); 
			Assert.AreEqual( qData[2].StartDate, new DateTime(2011, 7, 1) ); Assert.AreEqual( qData[2].EndDate, new DateTime(2011, 7, 31, 23, 59 , 59) ); 
			Assert.AreEqual( qData[3].StartDate, new DateTime(2011, 8, 1) ); Assert.AreEqual( qData[3].EndDate, new DateTime(2011, 8, 31, 23, 59 , 59) );

			var r2 = queue.Peek();
			Assert.AreEqual( r2.StartDate, new DateTime(2011, 5, 1) ); Assert.AreEqual( r2.EndDate, new DateTime(2011, 5, 31, 23, 59 , 59) );

			var modifiedRange2 = new ModifiedDateQuery
			{

				Dates = new ModifiedDateRange
				{
					StartDate = new DateTime( 2011, 4, 30 ),
					EndDate = new DateTime( 2011, 4, 30 )
				}
			};

			queue.RemoveAndCorrectQueue( r2, modifiedRange2 );
			Assert.AreEqual( queue.Count, 0 );


		}

		[Test]
		public void TestQueue3()
		{
			var startRequestsDate = new DateTime( 2011, 04, 23 );
			var creationInfo = new TeraPeakRequestDataInfo
			{
				StartDate = startRequestsDate,
				CountSteps = 5,
				StepType = TeraPeakRequestStepEnum.ByMonth
			};

			List<SearchQueryDatesRange> ranges = TerapeakRequestsQueue.CreateQueriesDates(creationInfo, DateTime.Now);
			var queue = new TerapeakRequestsQueue( ranges );
			// Queries:
			// 1. 23/04/2011 - 30/04/2011
			// 2. 01/05/2011 - 31/05/2011
			// 3. 01/06/2012 - 30/06/2012
			// 4. 01/07/2012 - 31/07/2012
			// 5. 01/08/2012 - 31/08/2012
			var qData = queue.Items;
			Assert.AreEqual( queue.Count, 5 );
			Assert.AreEqual( qData[0].StartDate, new DateTime(2011, 4, 1) ); Assert.AreEqual( qData[0].EndDate, new DateTime(2011, 4, 30, 23, 59 , 59) ); 
			Assert.AreEqual( qData[1].StartDate, new DateTime(2011, 5, 1) ); Assert.AreEqual( qData[1].EndDate, new DateTime(2011, 5, 31, 23, 59 , 59) ); 
			Assert.AreEqual( qData[2].StartDate, new DateTime(2011, 6, 1) ); Assert.AreEqual( qData[2].EndDate, new DateTime(2011, 6, 30, 23, 59 , 59) ); 
			Assert.AreEqual( qData[3].StartDate, new DateTime(2011, 7, 1) ); Assert.AreEqual( qData[3].EndDate, new DateTime(2011, 7, 31, 23, 59 , 59) ); 
			Assert.AreEqual( qData[4].StartDate, new DateTime(2011, 8, 1) ); Assert.AreEqual( qData[4].EndDate, new DateTime(2011, 8, 31, 23, 59 , 59) );

			var r1 = queue.Peek();
			Assert.AreEqual( queue.Count, 5 );
			Assert.AreEqual( r1.StartDate, new DateTime(2011, 4, 1) ); Assert.AreEqual( r1.EndDate, new DateTime(2011, 4, 30, 23, 59 , 59) );

			var modifiedRange1 = new ModifiedDateQuery
				{
				
					Dates = new ModifiedDateRange
					{
						StartDate = new DateTime( 2011, 4, 21 ),
						EndDate = new DateTime( 2011, 4, 25 )
					}
				};

			queue.RemoveAndCorrectQueue(r1, modifiedRange1);

			Assert.AreEqual( queue.Count, 4 );
		}

		[Test]
		public void TestQueryDataRangeCorrection()
		{
			var serviseFirstDate = new DateTime( 2011, 12, 30 );
			var serviceLastDate = new DateTime( 2012, 03, 15 );


			var startRequestsDate = new DateTime( 2011, 8, 23 );
			var allQieriesWithData = TestQueryDataRangeCorrection( serviseFirstDate, serviceLastDate, startRequestsDate );
			// Queries:
			// 1. 30/12/2011 - 31/12/2011
			// 3. 01/01/2012 - 31/01/2012
			// 4. 01/02/2012 - 29/02/2012
			// 5. 01/03/2012 - 30/03/2012 - data [01/03/2012 - 15/03/2012] 
			Assert.AreEqual( allQieriesWithData.Count, 4 );
			Assert.AreEqual( allQieriesWithData[0].StartDate, new DateTime(2011, 12, 30) ); Assert.AreEqual( allQieriesWithData[0].EndDate, new DateTime(2011, 12, 31, 23, 59 , 59) ); 
			Assert.AreEqual( allQieriesWithData[1].StartDate, new DateTime(2012, 1, 1) ); Assert.AreEqual( allQieriesWithData[1].EndDate, new DateTime(2012, 1, 31, 23, 59 , 59) ); 
			Assert.AreEqual( allQieriesWithData[2].StartDate, new DateTime(2012, 2, 1) ); Assert.AreEqual( allQieriesWithData[2].EndDate, new DateTime(2012, 2, 29, 23, 59 , 59) ); 
			Assert.AreEqual( allQieriesWithData[3].StartDate, new DateTime(2012, 3, 1) ); Assert.AreEqual( allQieriesWithData[3].EndDate, new DateTime(2012, 3, 15) ); 			
		}

		[Test]
		public void TestQueryDataRangeCorrection2()
		{
			var serviseFirstDate = new DateTime( 2012, 1, 2 );
			var serviceLastDate = new DateTime( 2012, 1, 7 );


			var startRequestsDate = new DateTime( 2011, 8, 23 );
			var allQieriesWithData = TestQueryDataRangeCorrection( serviseFirstDate, serviceLastDate, startRequestsDate );
			// Queries:
			// 1. 30/12/2011 - 31/12/2011
			// 3. 01/01/2012 - 31/01/2012
			// 4. 01/02/2012 - 29/02/2012
			// 5. 01/03/2012 - 30/03/2012 - data [01/03/2012 - 15/03/2012] 
			Assert.AreEqual( allQieriesWithData.Count, 1 );
			Assert.AreEqual( allQieriesWithData[0].StartDate, new DateTime( 2012, 1, 2 ) ); Assert.AreEqual( allQieriesWithData[0].EndDate, new DateTime( 2012, 1, 7 ) );			
		}

		private List<SearchQueryDatesRange> TestQueryDataRangeCorrection( DateTime serviseFirstDate, DateTime serviceLastDate, DateTime startRequestsDate )
		{
			var allQueries = new List<SearchQueryDatesRange>();
			
			var service = new TeraPearServiceMock( serviseFirstDate, serviceLastDate );

			var peakRequestDataInfo = new TeraPeakRequestDataInfo
			{
				StartDate = startRequestsDate,
				CountSteps = 10,
				StepType = TeraPeakRequestStepEnum.ByMonth
			};

			var ranges = TerapeakRequestsQueue.CreateQueriesDates(peakRequestDataInfo, DateTime.Now);

			var queue = new TerapeakRequestsQueue( ranges );

			int i = 0;

			while ( queue.HasItems )
			{
				SearchQueryDatesRange datesRange = queue.Peek();

				GetSellerResearchResults rez = service.RequestData( datesRange );
				if ( rez.HasError )
				{
					// log error and return
					break;
				}

				var modifiedDateQuery = rez.ModifiedQuery;

				SearchQueryDatesRange rezDataRange = null;

				if ( modifiedDateQuery == null)
				{
					rezDataRange = datesRange;
				}
				else if ( modifiedDateQuery.Dates.StartDate >= datesRange.StartDate &&
					modifiedDateQuery.Dates.EndDate <= datesRange.EndDate )
				{
					rezDataRange = new SearchQueryDatesRange( modifiedDateQuery.Dates.StartDate.Value, modifiedDateQuery.Dates.EndDate.Value, RangeMarkerType.Temporary);
				}
				else
				{
					 // no data to store
				}

				if ( rezDataRange != null )
				{
					allQueries.Add( rezDataRange );
				}

				queue.RemoveAndCorrectQueue( datesRange, modifiedDateQuery );
				++i;
			}

			return allQueries;
		}		

		[Test]
		public void TestCreateQueriesDates()
		{
			var creationInfo = new TeraPeakRequestDataInfo
			{
				StartDate = new DateTime(2012, 1, 1),
				CountSteps = 10,
				StepType = TeraPeakRequestStepEnum.ByMonth
			};

			var ranges = TerapeakRequestsQueue.CreateQueriesDates(creationInfo, DateTime.Now);

			Assert.AreEqual( ranges.Count, creationInfo.CountSteps );
			Assert.AreEqual( ranges[0].StartDate, new DateTime( 2012, 1, 1 ) );	Assert.AreEqual( ranges[0].EndDate, new DateTime( 2012, 1, 31, 23, 59 , 59 ) );
			Assert.AreEqual( ranges[1].StartDate, new DateTime( 2012, 2, 1 ) );Assert.AreEqual( ranges[1].EndDate, new DateTime( 2012, 2, 29, 23, 59 , 59 ) );
			Assert.AreEqual( ranges[2].StartDate, new DateTime( 2012, 3, 1 ) );Assert.AreEqual( ranges[2].EndDate, new DateTime( 2012, 3, 31, 23, 59 , 59 ) );
			Assert.AreEqual( ranges[3].StartDate, new DateTime( 2012, 4, 1 ) );Assert.AreEqual( ranges[3].EndDate, new DateTime( 2012, 4, 30, 23, 59 , 59 ) );
			Assert.AreEqual( ranges[4].StartDate, new DateTime( 2012, 5, 1 ) );Assert.AreEqual( ranges[4].EndDate, new DateTime( 2012, 5, 31 , 23, 59 , 59) );
			Assert.AreEqual( ranges[5].StartDate, new DateTime( 2012, 6, 1 ) );Assert.AreEqual( ranges[5].EndDate, new DateTime( 2012, 6, 30 , 23, 59 , 59) );
			Assert.AreEqual( ranges[6].StartDate, new DateTime( 2012, 7, 1 ) );Assert.AreEqual( ranges[6].EndDate, new DateTime( 2012, 7, 31 , 23, 59 , 59) );
			Assert.AreEqual( ranges[7].StartDate, new DateTime( 2012, 8, 1 ) );Assert.AreEqual( ranges[7].EndDate, new DateTime( 2012, 8, 31 , 23, 59 , 59) );
			Assert.AreEqual( ranges[8].StartDate, new DateTime( 2012, 9, 1 ) );Assert.AreEqual( ranges[8].EndDate, new DateTime( 2012, 9, 30 , 23, 59 , 59) );
			Assert.AreEqual( ranges[9].StartDate, new DateTime( 2012, 10, 1 ) );Assert.AreEqual( ranges[9].EndDate, new DateTime( 2012, 10, 31 , 23, 59 , 59) );
			
		}

	}
}