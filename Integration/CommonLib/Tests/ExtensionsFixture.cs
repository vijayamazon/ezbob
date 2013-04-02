using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace EzBob.CommonLib.Tests
{
	[TestFixture]
	public class ExtensionsFixture
	{

		[Test]
		public void GetCountMonthsTo1()
		{
			DateTime to, from;
			int countMonths;
			//-----------------
			from = new DateTime(2012, 11, 23, 12,56, 30);
			to = new DateTime( 2012, 11, 1, 12, 56, 30 );
			countMonths = from.GetCountMonthsToByEntire( to );
			Assert.AreEqual( countMonths, 0 );
			//------------------
			from = new DateTime( 2012, 12, 1, 12, 56, 30 );
			countMonths = from.GetCountMonthsToByEntire( to );
			Assert.AreEqual( countMonths, 0 );
			//------------------
			from = new DateTime( 2012, 10, 31, 23, 59, 59 );
			to = new DateTime( 2012, 11, 1, 0, 0, 0 );
			countMonths = from.GetCountMonthsToByEntire( to );
			Assert.AreEqual( countMonths, 2 );
			//-----------------
			from = new DateTime( 2011, 10, 31, 23, 59, 59 );
			to = new DateTime( 2012, 11, 1, 0, 0, 0 );
			countMonths = from.GetCountMonthsToByEntire( to );
			Assert.AreEqual( countMonths, 14 );
			//-----------------
			from = new DateTime( 2011, 12, 31, 23, 59, 59 );
			to = new DateTime( 2012, 11, 1, 0, 0, 0 );
			countMonths = from.GetCountMonthsToByEntire( to );
			Assert.AreEqual( countMonths, 12 );
			//-----------------
		}
	}
}
