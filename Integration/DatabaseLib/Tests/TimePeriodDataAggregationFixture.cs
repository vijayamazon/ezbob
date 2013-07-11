using System;
using System.Collections.Generic;
using System.Linq;
using EzBob.CommonLib.TimePeriodLogic;
using EzBob.CommonLib.TimePeriodLogic.BoundaryCalculation;
using EzBob.CommonLib.TimePeriodLogic.DependencyChain;
using EzBob.CommonLib.TimePeriodLogic.DependencyChain.Factories;
using NUnit.Framework;

namespace EZBob.DatabaseLib.Tests
{
	[TestFixture]
	public class TimePeriodDataAggregationFixture
	{
		private ITimeBoundaryCalculationStrategy _TimeBoundaryCalculationStrategyByStep;
		private ITimeBoundaryCalculationStrategy _TimeBoundaryCalculationStrategyByEntire;
		private ITimePeriodNodesCreationTreeFactory _FactoryByStep;
		private ITimePeriodNodesCreationTreeFactory _FactoryByEntire;
		private ITimePeriodNodesCreationTreeFactory _FactoryByHardCode;

		[SetUp]
		public void SetUp()
		{
			ScannerTest.Register();
			_TimeBoundaryCalculationStrategyByStep = TimeBoundaryCalculationStrategyFactory.Create( TimeBoundaryCalculationStrategyType.ByStep );
			_TimeBoundaryCalculationStrategyByEntire = TimeBoundaryCalculationStrategyFactory.Create( TimeBoundaryCalculationStrategyType.ByEntire );

			_FactoryByStep = TimePeriodNodesCreationTreeFactoryFactory.CreateSameTimeBoundaryCalculationStrategy( _TimeBoundaryCalculationStrategyByStep );
			_FactoryByEntire = TimePeriodNodesCreationTreeFactoryFactory.CreateSameTimeBoundaryCalculationStrategy( _TimeBoundaryCalculationStrategyByEntire );
			_FactoryByHardCode = TimePeriodNodesCreationTreeFactoryFactory.CreateHardCodeTimeBoundaryCalculationStrategy();
		}

		[Test]
		public void TimePeriodLeafAttainability()
		{
			
			var chain = TimePeriodChainContructor.CreateTimePeriodChain( new TimePeriodNodeFactory(), _FactoryByStep );
			var root = chain.Root;

			Assert.AreEqual( root.TimePeriodType, TimePeriodEnum.Lifetime );
			TimePeriodNode lastNode = root.Leaf;
			Assert.AreEqual( lastNode.TimePeriodType, TimePeriodEnum.Month );
			Assert.AreEqual( root.Root.TimePeriodType, TimePeriodEnum.Lifetime );
			root = lastNode.Root;
			Assert.AreEqual( root.TimePeriodType, TimePeriodEnum.Lifetime );
			Assert.AreEqual( root.Leaf.TimePeriodType, TimePeriodEnum.Month );
		}

		[Test]
		public void HasFullTimePeriodData()
		{
			var t = new TimePeriodNodeWithDataFactory<TimeDependentDataTest>().Create( TimePeriodEnum.Month3, _TimeBoundaryCalculationStrategyByStep ) as TimePeriodNodeWithData<TimeDependentDataTest>;

			var updateTime = new DateTime( 2012, 08, 03 );
			var list = new ReceivedDataListTest( updateTime)
			           	{
			           		new TimeDependentDataTest(new DateTime(2012, 06, 03)),
			           		new TimeDependentDataTest(new DateTime(2012, 07, 03)),
			           		new TimeDependentDataTest(new DateTime(2012, 08, 02))
			           	};

			t.SetSourceData( list );
			Assert.IsTrue( t.HasFullTimePeriodData( updateTime ) );

			list = new ReceivedDataListTest( updateTime )
			           	{
			           		new TimeDependentDataTest(new DateTime(2012, 07, 03)),
			           		new TimeDependentDataTest(new DateTime(2012, 08, 02)),
			           	};

			t.SetSourceData( list );

			Assert.IsTrue(t.HasFullTimePeriodData(updateTime));


		}

		[Test]
		public void NoData()
		{
			var t = new TimePeriodNodeWithDataFactory<TimeDependentDataTest>().Create( TimePeriodEnum.Month3, _TimeBoundaryCalculationStrategyByStep ) as TimePeriodNodeWithData<TimeDependentDataTest>;

			var updateTime = new DateTime( 2012, 08, 03 );

			Assert.IsFalse( t.HasFullTimePeriodData( updateTime ) );
			Assert.IsFalse( t.HasData );
			//Assert.IsFalse( t.IsParentsHasData );
			Assert.IsNull( t.Child );
			Assert.AreEqual(t.CountData, 0);
			//Assert.IsNull(t.GetAllData());			
			Assert.IsTrue( t.IsLeaf );
			Assert.IsTrue( t.IsRoot );
			Assert.IsNull( t.Parent );
			Assert.AreEqual( t.TimePeriodType, TimePeriodEnum.Month3 );

			var chain = TimePeriodChainContructor.CreateDataChain( new TimePeriodNodeWithDataFactory<TimeDependentDataTest>(), null, _FactoryByStep );

			Assert.IsTrue( chain.HasNoData );
			Assert.NotNull( chain.AllNodes );
			Assert.NotNull( chain.AllNodesWithData );
			Assert.AreEqual( chain.AllNodesWithData.Count(), 0 );
			Assert.AreEqual( chain.CountData, 0 );
			Assert.AreEqual( chain.CountNodesWithData, 0 );			
			Assert.AreEqual( chain.CountNodes, 0 );
			Assert.IsFalse( chain.HasAtLeastOneNode );
			Assert.IsNull( chain.Leaf );
			Assert.IsNull( chain.Root );
			
			var data = TimePeriodChainContructor.ExtractDataWithCorrectTimePeriod( chain, updateTime );

			Assert.IsNull( data );
		}

		[Test]
		public void CreateDataChain()
		{
			var updateTime = new DateTime( 2012, 08, 03 );
			var list = new ReceivedDataListTest( updateTime)
			           	{
			           		new TimeDependentDataTest(new DateTime(2012, 06, 03)),
			           		new TimeDependentDataTest(new DateTime(2012, 07, 03)),
			           		new TimeDependentDataTest(new DateTime(2012, 08, 03))
			           	};
			var chain = TimePeriodChainContructor.CreateDataChain( new TimePeriodNodeWithDataFactory<TimeDependentDataTest>(), list, _FactoryByStep );

			Assert.IsTrue( chain.HasAtLeastOneNode );
			Assert.NotNull( chain.Root );
			Assert.NotNull( chain.Leaf );
			Assert.AreEqual( chain.Root.TimePeriodType, TimePeriodEnum.Lifetime );
			Assert.AreEqual( chain.Leaf.TimePeriodType, TimePeriodEnum.Month );

			
			Assert.AreEqual( chain.CountData, 3 );
			Assert.IsNotNull( chain.AllNodesWithData );
			Assert.AreEqual( chain.CountNodesWithData, 2 );

			var leafMonth = chain.FindItem( TimePeriodEnum.Month ) as TimePeriodNodeWithData<TimeDependentDataTest>;
			
			Assert.IsNotNull( leafMonth );
			Assert.AreEqual( leafMonth.CountData, 1 );
			var leafMonth3 = chain.FindItem( TimePeriodEnum.Month3 ) as TimePeriodNodeWithData<TimeDependentDataTest>;
			Assert.IsNotNull( leafMonth3 );
			Assert.AreEqual( leafMonth3.CountData, 2 );
		}

		[Test]
		public void GetAllData()
		{
			var updateTime = new DateTime( 2012, 08, 03 );
			var list = new ReceivedDataListTest( updateTime )
			           	{
			           		new TimeDependentDataTest(new DateTime(2012, 06, 03)),
			           		new TimeDependentDataTest(new DateTime(2012, 07, 02)),
			           		new TimeDependentDataTest(new DateTime(2012, 08, 02))
			           	};
			var chain = TimePeriodChainContructor.CreateDataChain( new TimePeriodNodeWithDataFactory<TimeDependentDataTest>(), list, _FactoryByStep );

			var leafMonth = chain.FindItem( TimePeriodEnum.Month ) as TimePeriodNodeWithData<TimeDependentDataTest>;

			var timePeriodChainMonth1 = new TimePeriodChainWithData<TimeDependentDataTest>( leafMonth );
			var leafMonthAllData = timePeriodChainMonth1.GetAllData();

			Assert.NotNull( leafMonthAllData );
			Assert.AreEqual( leafMonthAllData.Count, 1 );
			Assert.AreEqual( leafMonthAllData.First().RecordTime, new DateTime( 2012, 08, 02 ) );

			var leafMonth3 = chain.FindItem( TimePeriodEnum.Month3 ) as TimePeriodNodeWithData<TimeDependentDataTest>;
			var timePeriodChainMonth3 = new TimePeriodChainWithData<TimeDependentDataTest>( leafMonth3 );
			var leafMonthAllData3 = timePeriodChainMonth3.GetAllData().ToList();
			
			Assert.NotNull( leafMonthAllData3 );
			Assert.AreEqual( leafMonthAllData3.Count, 3 );
			Assert.AreEqual( leafMonthAllData3[0].RecordTime, new DateTime( 2012, 08, 02 ) );			
			Assert.AreEqual( leafMonthAllData3[1].RecordTime, new DateTime( 2012, 07, 02 ) );
			Assert.AreEqual( leafMonthAllData3[2].RecordTime, new DateTime( 2012, 06, 03 ) );
			
		}

		[Test]
		public void ExtractDataWithCorrectTimePeriod()
		{
			var updateTime = new DateTime( 2012, 08, 03 );
			var list = new ReceivedDataListTest( updateTime )
			           	{
			           		new TimeDependentDataTest(new DateTime(2012, 06, 03)),
			           		new TimeDependentDataTest(new DateTime(2012, 07, 02)),
			           		new TimeDependentDataTest(new DateTime(2012, 08, 02))
			           	};
			var chain = TimePeriodChainContructor.CreateDataChain( new TimePeriodNodeWithDataFactory<TimeDependentDataTest>(), list, _FactoryByStep );

			var dict = TimePeriodChainContructor.ExtractDataWithCorrectTimePeriod( chain, updateTime );

			Assert.IsNotNull( dict );
			Assert.AreEqual( dict.Count, 2 );
			Assert.IsTrue( dict.ContainsKey(TimePeriodEnum.Month) );
			Assert.IsTrue( dict.ContainsKey(TimePeriodEnum.Month3) );

			Assert.AreEqual( dict[TimePeriodEnum.Month].CountData, 1 );
			Assert.AreEqual( dict[TimePeriodEnum.Month3].CountData, 3 );

			list = new ReceivedDataListTest( updateTime )
			           	{
							new TimeDependentDataTest(new DateTime(2012, 05, 03)),
			           		new TimeDependentDataTest(new DateTime(2012, 06, 03)),
			           		new TimeDependentDataTest(new DateTime(2012, 07, 03)),
			           		new TimeDependentDataTest(new DateTime(2012, 08, 02))
			           	};
			chain = TimePeriodChainContructor.CreateDataChain( new TimePeriodNodeWithDataFactory<TimeDependentDataTest>(), list, _FactoryByStep );
			dict = TimePeriodChainContructor.ExtractDataWithCorrectTimePeriod<TimeDependentDataTest>( chain, updateTime );

			Assert.IsNotNull( dict );
			Assert.AreEqual( dict.Count, 3 );
			Assert.IsTrue( dict.ContainsKey( TimePeriodEnum.Month ) );
			Assert.IsTrue( dict.ContainsKey( TimePeriodEnum.Month3 ) );
			Assert.IsTrue(dict.ContainsKey(TimePeriodEnum.Month6));

			Assert.AreEqual( dict[TimePeriodEnum.Month].CountData, 1 );
			Assert.AreEqual( dict[TimePeriodEnum.Month3].CountData, 3 );
			Assert.AreEqual(dict[TimePeriodEnum.Month6].CountData, 4);
			

		}
		[Test]
		public void Test()
		{
			var updateTime = new DateTime( 2012, 08, 1 );
			var list = new ReceivedDataListTest( updateTime )
			           	{
							new TimeDependentDataTest(new DateTime(2012, 05, 7)),
			           		new TimeDependentDataTest(new DateTime(2012, 06, 8)),
			           		new TimeDependentDataTest(new DateTime(2012, 07, 9)),
			           		new TimeDependentDataTest(new DateTime(2012, 08, 1))
			           	};	
			
			var timeChain = TimePeriodChainContructor.CreateDataChain( new TimePeriodNodeWithDataFactory<TimeDependentDataTest>(), list, _FactoryByEntire );
			Assert.AreEqual( timeChain.CountData, 4 );
			Assert.NotNull( timeChain );

			var timePeriodData = TimePeriodChainContructor.ExtractDataWithCorrectTimePeriod( timeChain, updateTime );

			Assert.NotNull( timePeriodData );
			Assert.AreEqual( timePeriodData.Count, 3 );			
			Assert.AreEqual( timePeriodData[TimePeriodEnum.Month].CountData, 1 );
			Assert.AreEqual( timePeriodData[TimePeriodEnum.Month3].CountData, 3 );
			Assert.AreEqual( timePeriodData[TimePeriodEnum.Month6].CountData, 4 );
			Assert.AreEqual( timePeriodData.Sum( td => td.Value.CountData ), 8 );
			Assert.IsTrue( timePeriodData[TimePeriodEnum.Month].HasData );
			Assert.IsTrue( timePeriodData[TimePeriodEnum.Month3].HasData );
			Assert.IsTrue(timePeriodData[TimePeriodEnum.Month6].HasData);

			Assert.AreEqual( timePeriodData[TimePeriodEnum.Month].CountMonthsFor( updateTime, _TimeBoundaryCalculationStrategyByEntire ), 1 );
			Assert.AreEqual( timePeriodData[TimePeriodEnum.Month3].CountMonthsFor( updateTime, _TimeBoundaryCalculationStrategyByEntire ), 3 );
			Assert.AreEqual(timePeriodData[TimePeriodEnum.Month6].CountMonthsFor(updateTime, _TimeBoundaryCalculationStrategyByEntire), 4);
			
		}

		[Test]
		//[Ignore]
		public void DifferentTimeBoundaryCalculation()
		{
			var updateTime = new DateTime( 2012, 08, 05 );
			var list = new ReceivedDataListTest( updateTime )
			           	{
							new TimeDependentDataTest(new DateTime(2012, 02, 7)),
							new TimeDependentDataTest(new DateTime(2012, 03, 7)),
							new TimeDependentDataTest(new DateTime(2012, 04, 7)),
							new TimeDependentDataTest(new DateTime(2012, 05, 7)),
			           		new TimeDependentDataTest(new DateTime(2012, 06, 8)),
			           		new TimeDependentDataTest(new DateTime(2012, 07, 9)),
			           		new TimeDependentDataTest(new DateTime(2012, 08, 1))
			           	};

			TimePeriodChainWithData<TimeDependentDataTest> timeChain;
			Dictionary<TimePeriodEnum, ReceivedDataListTimeDependentInfo<TimeDependentDataTest>> timePeriodData;
			//--------------------------------
			timeChain = TimePeriodChainContructor.CreateDataChain( new TimePeriodNodeWithDataFactory<TimeDependentDataTest>(), list, _FactoryByHardCode );
			Assert.AreEqual( timeChain.CountData, 7 );
			Assert.NotNull( timeChain );

			
			timePeriodData = TimePeriodChainContructor.ExtractDataWithCorrectTimePeriod( timeChain, updateTime );
			Assert.AreEqual( timePeriodData.Count, 4 );
			Assert.AreEqual( timePeriodData[TimePeriodEnum.Month].CountData, 2 );
			Assert.AreEqual( timePeriodData[TimePeriodEnum.Month3].CountData, 3 );
			Assert.AreEqual( timePeriodData[TimePeriodEnum.Month6].CountData, 6 );
			Assert.AreEqual( timePeriodData[TimePeriodEnum.Year].CountData, 7 );
			
			//--------------------------------
			timeChain = TimePeriodChainContructor.CreateDataChain( new TimePeriodNodeWithDataFactory<TimeDependentDataTest>(), list,_FactoryByEntire );
			Assert.AreEqual( timeChain.CountData, 7 );
			Assert.NotNull( timeChain );

			timePeriodData = TimePeriodChainContructor.ExtractDataWithCorrectTimePeriod( timeChain, updateTime );
			Assert.AreEqual( timePeriodData.Count, 4 );
			Assert.AreEqual( timePeriodData[TimePeriodEnum.Month].CountData, 1 );
			Assert.AreEqual( timePeriodData[TimePeriodEnum.Month3].CountData, 3 );
			Assert.AreEqual( timePeriodData[TimePeriodEnum.Month6].CountData, 6 );
			Assert.AreEqual(timePeriodData[TimePeriodEnum.Year].CountData, 7);

			//--------------------------------
			timeChain = TimePeriodChainContructor.CreateDataChain( new TimePeriodNodeWithDataFactory<TimeDependentDataTest>(), list, _FactoryByStep );
			Assert.AreEqual( timeChain.CountData, 7 );
			Assert.NotNull( timeChain );

			timePeriodData = TimePeriodChainContructor.ExtractDataWithCorrectTimePeriod( timeChain, updateTime );
			Assert.AreEqual( timePeriodData.Count, 3 );
			Assert.AreEqual( timePeriodData[TimePeriodEnum.Month].CountData, 2 );
			Assert.AreEqual( timePeriodData[TimePeriodEnum.Month3].CountData, 4 );
			Assert.AreEqual( timePeriodData[TimePeriodEnum.Month6].CountData, 7 );

		}

		[Test]
		public void CalculationWithGap()
		{
			var updateTime = new DateTime( 2012, 08, 05 );

			TimePeriodChainWithData<TimeDependentDataTest> timeChain;
			Dictionary<TimePeriodEnum, ReceivedDataListTimeDependentInfo<TimeDependentDataTest>> timePeriodData;
			//-------------------------------
			var list = new ReceivedDataListTest( updateTime )
			           	{							
							new TimeDependentDataTest(new DateTime(2012, 03, 7)),
			           		new TimeDependentDataTest(new DateTime(2012, 08, 1))
			           	};

			
			timeChain = TimePeriodChainContructor.CreateDataChain( new TimePeriodNodeWithDataFactory<TimeDependentDataTest>(), list, _FactoryByHardCode );
			Assert.AreEqual( timeChain.CountData, 2 );
			Assert.NotNull( timeChain );

			timePeriodData = TimePeriodChainContructor.ExtractDataWithCorrectTimePeriod( timeChain, updateTime );
			Assert.AreEqual( timePeriodData.Count, 3 );
			Assert.AreEqual( timePeriodData[TimePeriodEnum.Month].CountData, 1 );
			Assert.AreEqual( timePeriodData[TimePeriodEnum.Month3].CountData, 1 );
			Assert.AreEqual( timePeriodData[TimePeriodEnum.Month6].CountData, 2 );			
			

			//--------------------------------			
			list = new ReceivedDataListTest( updateTime )
			           	{							
							new TimeDependentDataTest(new DateTime(2012, 02, 7)),
			           		new TimeDependentDataTest(new DateTime(2012, 08, 1))
			           	};

			
			
			timeChain = TimePeriodChainContructor.CreateDataChain( new TimePeriodNodeWithDataFactory<TimeDependentDataTest>(), list, _FactoryByHardCode );
			Assert.AreEqual( timeChain.CountData, 2 );
			Assert.NotNull( timeChain );

			timePeriodData = TimePeriodChainContructor.ExtractDataWithCorrectTimePeriod( timeChain, updateTime );
			Assert.AreEqual( timePeriodData.Count, 4 );
			Assert.AreEqual( timePeriodData[TimePeriodEnum.Month].CountData, 1 );
			Assert.AreEqual( timePeriodData[TimePeriodEnum.Month3].CountData, 1 );
			Assert.AreEqual( timePeriodData[TimePeriodEnum.Month6].CountData, 1 );
			Assert.AreEqual( timePeriodData[TimePeriodEnum.Year].CountData, 2 );

			//--------------------------------			
			list = new ReceivedDataListTest( updateTime )
			           	{							
							new TimeDependentDataTest(new DateTime(2012, 02, 7)),
			           		new TimeDependentDataTest(new DateTime(2012, 07, 1))
			           	};



			timeChain = TimePeriodChainContructor.CreateDataChain( new TimePeriodNodeWithDataFactory<TimeDependentDataTest>(), list, _FactoryByHardCode );
			Assert.AreEqual( timeChain.CountData, 2 );
			Assert.NotNull( timeChain );

			timePeriodData = TimePeriodChainContructor.ExtractDataWithCorrectTimePeriod( timeChain, updateTime );
			Assert.AreEqual( timePeriodData.Count, 4 );
			Assert.AreEqual( timePeriodData[TimePeriodEnum.Month].CountData, 0 );
			Assert.AreEqual( timePeriodData[TimePeriodEnum.Month3].CountData, 1 );
			Assert.AreEqual( timePeriodData[TimePeriodEnum.Month6].CountData, 1 );
			Assert.AreEqual(timePeriodData[TimePeriodEnum.Year].CountData, 2);
			
		}
	}
}