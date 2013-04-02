using System;
using EzBob.CommonLib;
using EzBob.PayPalServiceLib.Common;
using EzBobTest;
using NUnit.Framework;

namespace EzBob
{
	[TestFixture]
	public class SplitDateRangesFixture
	{
		[Test]
		public void SplitDateRangesOne()
		{
			var endDate = new DateTime( 2012, 10, 3, 14, 31, 50 );
			var startDate = endDate.AddYears( -1 );

			var ranges = UsefulFunctions.SplitDateRanges( startDate, endDate, 12 );
			Assert.NotNull( ranges );
			Assert.AreEqual( ranges.Count, 1 );
			Assert.AreEqual( ranges[0].Item1, startDate );
			Assert.AreEqual( ranges[0].Item2, endDate );
		}

		[Test]
		public void SplitDateRanges()
		{
			var endDate = new DateTime( 2012, 10, 3, 14, 31, 50 );
			var startDate = endDate.AddYears( -1 );

			var ranges = UsefulFunctions.SplitDateRanges( startDate, endDate, 6 );
			Assert.NotNull( ranges );
			Assert.AreEqual( ranges.Count, 2 );
			Assert.AreEqual( ranges[0].Item1, new DateTime( 2011, 10, 3, 14, 31, 50 ) ); Assert.AreEqual( ranges[0].Item2, new DateTime( 2012, 4, 3, 14, 31, 50 ) );
			Assert.AreEqual( ranges[1].Item1, new DateTime( 2012, 4, 3, 14, 31, 51 ) ); Assert.AreEqual( ranges[1].Item2, new DateTime( 2012, 10, 3, 14, 31, 50 ) );

		}

		[Test]
		public void SplitDateRanges2()
		{
			var endDate = new DateTime( 2012, 10, 3, 14, 31, 50 );
			var startDate = endDate.AddYears( -1 ).AddHours( -10 );

			var ranges = UsefulFunctions.SplitDateRanges( startDate, endDate, 6 );
			Assert.NotNull( ranges );
			Assert.AreEqual( ranges.Count, 2 );
			Assert.AreEqual( ranges[0].Item1, new DateTime( 2011, 10, 3, 4, 31, 50 ) ); Assert.AreEqual( ranges[0].Item2, new DateTime( 2012, 4, 3, 4, 31, 50 ) );
			Assert.AreEqual( ranges[1].Item1, new DateTime( 2012, 4, 3, 4, 31, 51 ) ); Assert.AreEqual( ranges[1].Item2, new DateTime( 2012, 10, 3, 14, 31, 50 ) );

		}

		[Test]
		public void SplitDateRanges3()
		{
			var endDate = new DateTime( 2012, 10, 3, 14, 31, 50 );
			var startDate = endDate.AddMonths( -6 );

			var ranges = UsefulFunctions.SplitDateRanges( startDate, endDate, 6 );
			Assert.NotNull( ranges );
			Assert.AreEqual( ranges.Count, 1 );
			Assert.AreEqual( ranges[0].Item1, new DateTime( 2012, 4, 3, 14, 31, 50 ) ); Assert.AreEqual( ranges[0].Item2, new DateTime( 2012, 10, 3, 14, 31, 50 ) );

		}

		[Test]
		public void SplitDateRanges4()
		{
			var endDate = new DateTime( 2012, 10, 3, 14, 31, 50 );
			var startDate = endDate.AddMonths( -6 ).AddHours( -10 );
	
			var ranges = UsefulFunctions.SplitDateRanges( startDate, endDate, 6 );
			Assert.NotNull( ranges );
			Assert.AreEqual( ranges.Count, 1 );
			Assert.AreEqual( ranges[0].Item1, new DateTime( 2012, 4, 3, 4, 31, 50 ) ); Assert.AreEqual( ranges[0].Item2, new DateTime( 2012, 10, 3, 14, 31, 50 ) );

		}

		[Test]
		public void SplitDateRanges5()
		{
			var endDate = new DateTime( 2012, 10, 3, 14, 31, 50 );
			var startDate = endDate.AddMonths( -6 ).AddDays( -1 );

			var ranges = UsefulFunctions.SplitDateRanges( startDate, endDate, 6 );
			Assert.NotNull( ranges );
			Assert.AreEqual( ranges.Count, 2 );
			Assert.AreEqual( ranges[0].Item1, new DateTime( 2012, 4, 2, 14, 31, 50 ) ); Assert.AreEqual( ranges[0].Item2, new DateTime( 2012, 10, 2, 14, 31, 50 ) );
			Assert.AreEqual( ranges[1].Item1, new DateTime( 2012, 10, 2, 14, 31, 51 ) ); Assert.AreEqual( ranges[1].Item2, new DateTime( 2012, 10, 3, 14, 31, 50 ) );

		}

		[Test]
		public void SplitDateRanges6()
		{
			var endDate = new DateTime( 2012, 10, 3, 14, 31, 50 );
			var startDate = endDate.AddMonths( -5 );

			var ranges = UsefulFunctions.SplitDateRanges( startDate, endDate, 6 );
			Assert.NotNull( ranges );
			Assert.AreEqual( ranges.Count, 1 );
			Assert.AreEqual( ranges[0].Item1, new DateTime( 2012, 5, 3, 14, 31, 50 ) ); Assert.AreEqual( ranges[0].Item2, new DateTime( 2012, 10, 3, 14, 31, 50 ) );			
		}

		[Test]
		public void SplitDateRanges7()
		{
			var endDate = new DateTime( 2012, 10, 3, 14, 31, 50 );
			var startDate = endDate.AddMonths( -6 );

			var ranges = UsefulFunctions.SplitDateRanges( startDate, endDate, 1 );
			Assert.NotNull( ranges );
			Assert.AreEqual( ranges.Count, 6 );
			Assert.AreEqual( ranges[0].Item1, new DateTime( 2012, 4, 3, 14, 31, 50 ) ); Assert.AreEqual( ranges[0].Item2, new DateTime( 2012, 5, 3, 14, 31, 50 ) );
			Assert.AreEqual( ranges[1].Item1, new DateTime( 2012, 5, 3, 14, 31, 51 ) ); Assert.AreEqual( ranges[1].Item2, new DateTime( 2012, 6, 3, 14, 31, 50 ) );
			Assert.AreEqual( ranges[2].Item1, new DateTime( 2012, 6, 3, 14, 31, 51 ) ); Assert.AreEqual( ranges[2].Item2, new DateTime( 2012, 7, 3, 14, 31, 50 ) );
			Assert.AreEqual( ranges[3].Item1, new DateTime( 2012, 7, 3, 14, 31, 51 ) ); Assert.AreEqual( ranges[3].Item2, new DateTime( 2012, 8, 3, 14, 31, 50 ) );
			Assert.AreEqual( ranges[4].Item1, new DateTime( 2012, 8, 3, 14, 31, 51 ) ); Assert.AreEqual( ranges[4].Item2, new DateTime( 2012, 9, 3, 14, 31, 50 ) );
			Assert.AreEqual( ranges[5].Item1, new DateTime( 2012, 9, 3, 14, 31, 51 ) ); Assert.AreEqual( ranges[5].Item2, new DateTime( 2012, 10, 3, 14, 31, 50 ) );			
			
		}

		[Test]
		public void SplitDateRanges8()
		{
			var endDate = new DateTime( 2012, 10, 3, 14, 31, 50 );
			var startDate = endDate.AddMonths( -6 ).AddHours( -10 );

			var ranges = UsefulFunctions.SplitDateRanges( startDate, endDate, 1 );
			Assert.NotNull( ranges );
			Assert.AreEqual( ranges.Count, 6 );
			Assert.AreEqual( ranges[0].Item1, new DateTime( 2012, 4, 3, 4, 31, 50 ) ); Assert.AreEqual( ranges[0].Item2, new DateTime( 2012, 5, 3, 4, 31, 50 ) );
			Assert.AreEqual( ranges[1].Item1, new DateTime( 2012, 5, 3, 4, 31, 51 ) ); Assert.AreEqual( ranges[1].Item2, new DateTime( 2012, 6, 3, 4, 31, 50 ) );
			Assert.AreEqual( ranges[2].Item1, new DateTime( 2012, 6, 3, 4, 31, 51 ) ); Assert.AreEqual( ranges[2].Item2, new DateTime( 2012, 7, 3, 4, 31, 50 ) );
			Assert.AreEqual( ranges[3].Item1, new DateTime( 2012, 7, 3, 4, 31, 51 ) ); Assert.AreEqual( ranges[3].Item2, new DateTime( 2012, 8, 3, 4, 31, 50 ) );
			Assert.AreEqual( ranges[4].Item1, new DateTime( 2012, 8, 3, 4, 31, 51 ) ); Assert.AreEqual( ranges[4].Item2, new DateTime( 2012, 9, 3, 4, 31, 50 ) );
			Assert.AreEqual( ranges[5].Item1, new DateTime( 2012, 9, 3, 4, 31, 51 ) ); Assert.AreEqual( ranges[5].Item2, new DateTime( 2012, 10, 3, 14, 31, 50 ) );			

		}

		[Test]
		public void SplitDateRanges9()
		{
			var endDate = new DateTime( 2012, 10, 3, 14, 31, 50 );
			var startDate = endDate.AddHours( -10 );

			var ranges = UsefulFunctions.SplitDateRanges( startDate, endDate, 1 );
			Assert.NotNull( ranges );
			Assert.AreEqual( ranges.Count, 1 );
			Assert.AreEqual( ranges[0].Item1, new DateTime( 2012, 10, 3, 4, 31, 50 ) );
			Assert.AreEqual( ranges[0].Item2, new DateTime( 2012, 10, 3, 14, 31, 50 ) );

		}

	}
}