using System;
using System.Linq;
using EzBob.CommonLib;
using EzBob.CommonLib.TimePeriodLogic.BoundaryCalculation;
using NUnit.Framework;

namespace EzBobTest
{
	[TestFixture]
	public class TimeBoundaryCalculationTests
	{
		[Test]
		public void LinqGroup()
		{
			var data = TestOrderItemDataCreator.Create();

			var topPrice = ( data.GroupBy( x => new { x.Price, x.Count}, 
					( key, group ) => new { Price = key.Price, Count = key.Count, Counter = group.Count() } ) ).OrderByDescending( x => x.Counter ).Take( 3 ).ToList();

			Assert.IsTrue( topPrice.Count == 3 );
			Assert.AreEqual( topPrice[0].Counter, 5 );
			Assert.AreEqual( topPrice[1].Counter, 3 );
			Assert.AreEqual( topPrice[2].Counter, 2 );
			var rez = topPrice.Select( p => data.First( d => d.Price == p.Price && d.Count == p.Count ) ).OrderBy( x => x.Price).ThenBy(x=> x.Count) .ToList();

			Assert.IsTrue( rez.Count == 3 );
			Assert.AreEqual( rez[0].Price, 15.96 );
			Assert.AreEqual( rez[0].Count, 1 );
			Assert.AreEqual( rez[1].Price, 200.36 );
			Assert.AreEqual( rez[1].Count, 1 );
			Assert.AreEqual( rez[2].Price, 200.36 );
			Assert.AreEqual( rez[2].Count, 2 );			


		}

		[Test]
		public void LinqTake()
		{
			var a = new[] { 5, 3, 6 };

			var rez = a.Take( 5 );

			Assert.AreEqual( rez.Count(), 3 );
		}

		[Test]
		public void TimeBoundaryCalculationStrategyByStep()
		{
			var strategy = TimeBoundaryCalculationStrategyFactory.Create( TimeBoundaryCalculationStrategyType.ByStep );

			var toDate = new DateTime( 2012, 08, 05, 15, 30, 59 );

			var i = strategy.GetCountIncludedMonths( toDate, toDate );
			Assert.AreEqual( i, 1 );

			i = strategy.GetCountIncludedMonths( toDate.AddSeconds(-1), toDate );
			Assert.AreEqual( i, 1 );

			i = strategy.GetCountIncludedMonths( toDate.AddMonths(-1), toDate );
			Assert.AreEqual( i, 2 );

			i = strategy.GetCountIncludedMonths( toDate.AddMonths(-1).AddSeconds(1), toDate );
			Assert.AreEqual( i, 1 );

			i = strategy.GetCountIncludedMonths( toDate.AddMonths(-6), toDate );
			Assert.AreEqual( i, 7 );

			i = strategy.GetCountIncludedMonths( toDate.AddMonths( -6 ).AddSeconds(1), toDate );
			Assert.AreEqual( i, 6 );
		}

		[Test]
		public void TimeBoundaryCalculationStrategyByEntire()
		{
			var strategy = TimeBoundaryCalculationStrategyFactory.Create( TimeBoundaryCalculationStrategyType.ByEntire );

			var toDate = new DateTime( 2012, 08, 01, 0, 0, 0 );

			var i = strategy.GetCountIncludedMonths( toDate, toDate );
			Assert.AreEqual( i, 1 );

			i = strategy.GetCountIncludedMonths( toDate.AddSeconds( -1 ), toDate );
			Assert.AreEqual( i,2 );

			i = strategy.GetCountIncludedMonths( toDate.AddMonths( -1 ), toDate );
			Assert.AreEqual( i, 2 );

			i = strategy.GetCountIncludedMonths( toDate.AddMonths( -1 ).AddSeconds( 1 ), toDate );
			Assert.AreEqual( i, 2 );

			i = strategy.GetCountIncludedMonths( toDate.AddMonths( -6 ), toDate );
			Assert.AreEqual( i, 7 );

			i = strategy.GetCountIncludedMonths( toDate.AddMonths( -6 ).AddSeconds( 1 ), toDate );
			Assert.AreEqual( i, 7 );
		}
		
	}

	internal class TestOrderItemDataCreator
	{
		public static TestOrderItemData[] Create()
		{
			return new[]
			{
				new TestOrderItemData { Price = 11.23, Count = 1},
				new TestOrderItemData { Price = 15.96,Count = 1},
				new TestOrderItemData { Price = 15.96,Count = 1},
				new TestOrderItemData { Price = 15.96,Count = 1},
				new TestOrderItemData { Price = 15.96,Count = 1},
				new TestOrderItemData { Price = 15.96,Count = 1},
				new TestOrderItemData { Price = 200.36,Count = 2},
				new TestOrderItemData { Price = 200.36,Count = 2},
				new TestOrderItemData { Price = 200.36,Count = 1},
				new TestOrderItemData { Price = 200.36,Count = 1},
				new TestOrderItemData { Price = 200.36,Count = 1},				
				new TestOrderItemData { Price = 156.30,Count = 1},
				new TestOrderItemData { Price = 99.80,Count = 1},
				new TestOrderItemData { Price = 10.45,Count = 5},
				new TestOrderItemData { Price = 39.77,Count = 1},
				new TestOrderItemData { Price = 67.30,Count = 1},
				new TestOrderItemData { Price = 50.00,Count = 2},
			};


		}
	}

}