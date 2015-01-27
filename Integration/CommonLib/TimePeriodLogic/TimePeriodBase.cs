using System;
using System.Collections.Generic;
using System.Linq;
using StructureMap;

namespace EzBob.CommonLib.TimePeriodLogic
{
	public abstract class TimePeriodBase : ITimePeriod
	{
		public static List<ITimePeriod> AllTimePeriods { get; private set; }

		static TimePeriodBase()
		{
		    var helper = ObjectFactory.GetInstance<IDatabaseDataHelper>();

			AllTimePeriods = new List<ITimePeriod>();

			foreach ( TimePeriodEnum enumItem in Enum.GetValues( typeof( TimePeriodEnum ) ) )
			{
				AllTimePeriods.Add( TimePeriodFactory.Create( enumItem ) );
			}

			helper.InitFunctionTimePeriod();
		}

		public static readonly ITimePeriod Month	= new TimePeriodMonthly( TimePeriodEnum.Month, new Guid( "{318795D7-C51D-4B18-8E1F-5A563B3091F4}" ), 1 );
		public static readonly ITimePeriod Month3	= new TimePeriodMonthly( TimePeriodEnum.Month3, new Guid( "{AA13A708-5230-4F24-895F-E05D513278BD}" ), 3 );
		public static readonly ITimePeriod Month6	= new TimePeriodMonthly( TimePeriodEnum.Month6, new Guid( "{33E0E7AE-92E0-4AAB-A042-10CF34526368}" ), 6 );
		public static readonly ITimePeriod Month15 = new TimePeriodMonthly( TimePeriodEnum.Month15, new Guid( "{D4D706A6-400E-492C-9462-B15454A39347}" ), 15 );
		public static readonly ITimePeriod Month18 = new TimePeriodMonthly( TimePeriodEnum.Month18, new Guid( "{D67CD383-A7B1-46A0-8449-A8AF9212429F}" ), 18 );
		public static readonly ITimePeriod Year		= new TimePeriodYearly( TimePeriodEnum.Year, new Guid( "{1F9E6CEF-7251-4E1C-AC35-801265E732CD}" ), 1);
		public static readonly ITimePeriod Year2 = new TimePeriodYearly( TimePeriodEnum.Year2, new Guid( "{45884DFB-06B9-4617-A537-262D1BEF4AE7}" ), 2 );
		public static readonly ITimePeriod LifeTime = new TimePeriodLifeTime( TimePeriodEnum.Lifetime, new Guid( "{3A552C6D-C28D-4D5B-9590-7D4A8094BD0A}" ) );
		public static readonly ITimePeriod Zero		= new TimePeriodNone( TimePeriodEnum.Zero, new Guid( "{16619B19-ABF5-4AE0-9040-93EFD2E71FDB}" ) );

		protected TimePeriodBase( TimePeriodEnum timePeriodType, Guid internalId )
		{
			TimePeriodType = timePeriodType;
			InternalId = internalId;

			ConvertedTypeInfo info = new DatabaseAnalysisFunctionTimePeriodTypeConverter().Convert( timePeriodType );
			Name = info.Name;
			DisplayName = info.DisplayName;
			Description = info.Description;			
		}

		public TimePeriodEnum TimePeriodType { get; private set; }

		public Guid InternalId { get; private set; }

		public string Description { get; private set; }

		public string Name { get; private set; }

		public string DisplayName { get; private set; }

		public abstract int DaysInPeriod { get; }

		public abstract int MonthsInPeriod { get; }

		public static ITimePeriod GetById(Guid id)
		{
			return AllTimePeriods.FirstOrDefault( tp => tp.InternalId.Equals( id ) );
		}

		public override string ToString()
		{
			return string.Format( "{0}", DisplayName );
		}		
	}

	internal class DatabaseAnalysisFunctionTimePeriodTypeConverter : IDatabaseEnumTypeConverter<TimePeriodEnum>
	{
		public ConvertedTypeInfo Convert(TimePeriodEnum type)
		{
			string name;
			string displayName = string.Empty;
			string description;

			switch ( type )
			{
				case TimePeriodEnum.Lifetime:
					name = "Lifetime";
					description = "All data";
					displayName = "Lifetime";
					break;

				case TimePeriodEnum.Month:
					name = "30";
					description = "30 days - 1 month";
					displayName = "1M";
					break;

				case TimePeriodEnum.Year:
					name = "365";
					description = "1 year - 12 months - 365 days";
					displayName = "12M";
					break;

				case TimePeriodEnum.Month3:
					name = "90";
					description = "90 days - 3 months";
					displayName = "3M";
					break;

				case TimePeriodEnum.Month6:
					name = "180";
					description = "180 days - 6 months";
					displayName = "6M";
					break;

				case TimePeriodEnum.Zero:
					name = "0";
					description = "All data";
					displayName = "Lifetime";
					break;

				case TimePeriodEnum.Month15:
					name = "15 months";
					description = "15 months";
					displayName = "15M";
					break;

				case TimePeriodEnum.Month18:
					name = "18 months";
					description = "18 months";
					displayName = "18M";
					break;

				case TimePeriodEnum.Year2:
					name = "2 years";
					description = "24 months - 2 years";
					displayName = "24M";
					break;
				default:
					throw new NotImplementedException();
			}

			return new ConvertedTypeInfo( name, displayName, description );
		}
	}
}
