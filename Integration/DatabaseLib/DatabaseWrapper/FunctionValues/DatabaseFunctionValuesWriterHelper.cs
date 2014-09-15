using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EZBob.DatabaseLib.DatabaseWrapper.Functions;
using EZBob.DatabaseLib.DatabaseWrapper.ValueType;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EzBob.CommonLib.TimePeriodLogic;

namespace EZBob.DatabaseLib.DatabaseWrapper.FunctionValues
{
	using Repository;

	class DatabaseFunctionValuesWriterHelper
	{
    	private readonly DatabaseDataHelper _Helper;
    	private readonly AnalyisisFunctionValueRepository _AnalyisisFunctionValueRepository;
		private readonly CustomerMarketPlaceRepository _CustomerMarketPlaceRepository;
        private readonly AnalysisFunctionTimePeriodRepository _AnalysisFunctionTimePeriodRepository;
        private readonly AnalyisisFunctionRepository _AnalyisisFunctionRepository;

        public DatabaseFunctionValuesWriterHelper( DatabaseDataHelper helper,
                                                    AnalyisisFunctionValueRepository analyisisFunctionValueRepository, 
                                                    CustomerMarketPlaceRepository customerMarketPlaceRepository, 
                                                    AnalysisFunctionTimePeriodRepository analysisFunctionTimePeriodRepository,
                                                    AnalyisisFunctionRepository analyisisFunctionRepository)
        {
        	_Helper = helper;
        	_AnalyisisFunctionValueRepository = analyisisFunctionValueRepository;
            _CustomerMarketPlaceRepository = customerMarketPlaceRepository;
            _AnalysisFunctionTimePeriodRepository = analysisFunctionTimePeriodRepository;
            _AnalyisisFunctionRepository = analyisisFunctionRepository;
        }

		public void SetRangeOfData<TEnum>(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, IEnumerable<IWriteDataInfo<TEnum>> dataInfo, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
		    if (dataInfo == null)
		    {
		        return;
		    }
		    var databaseMarketPlaceBase = databaseCustomerMarketPlace.Marketplace as DatabaseMarketplaceBase<TEnum>;
			Debug.Assert( databaseMarketPlaceBase != null );
			WriteRange( databaseCustomerMarketPlace, dataInfo.Select( i => CreateWriteValues( databaseMarketPlaceBase, i ) ), historyRecord );
		}
		public void SetData<TEnum>( IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, IWriteDataInfo<TEnum> data, MP_CustomerMarketplaceUpdatingHistory historyRecord )
		{
            if (data == null)
            {
                return;
            }
			var databaseMarketPlaceBase = databaseCustomerMarketPlace.Marketplace as DatabaseMarketplaceBase<TEnum>;
			Debug.Assert( databaseMarketPlaceBase != null );

			var dbFunctionValue = CreateWriteValues( databaseMarketPlaceBase, data );

			Write( databaseCustomerMarketPlace, dbFunctionValue, historyRecord );
		}

    	private IDatabaseAnalysisFunctionValues CreateWriteValues<TEnum>( DatabaseMarketplaceBase<TEnum> databaseMarketPlaceBase, 
																			IWriteDataInfo<TEnum> dataInfo )
		{
			Debug.Assert( databaseMarketPlaceBase != null );
			var funcFactory = databaseMarketPlaceBase.FunctionFactory;
			return new DatabaseAnalysisFunctionValueInfo
				(
					funcFactory.Create( dataInfo.FunctionType ),
					TimePeriodFactory.Create( dataInfo.TimePeriodType ),
					dataInfo.Value,
					dataInfo.UpdatedDate
				);
		}

		private void WriteRange(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, IEnumerable<IDatabaseAnalysisFunctionValues> dataList, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
            if (dataList == null)
            {
                return;
            }
			var customerMarketPlace = _CustomerMarketPlaceRepository.Get( databaseCustomerMarketPlace.Id );

		    _AnalyisisFunctionValueRepository.EnsureTransaction(() => dataList.ToList().ForEach(data => Write(customerMarketPlace, data, historyRecord)));
		}

		private void Write( IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, IDatabaseAnalysisFunctionValues data, MP_CustomerMarketplaceUpdatingHistory historyRecord )
		{
            if (data == null)
            {
                return;
            }
			var mpCustomerMarketPlace = _CustomerMarketPlaceRepository.Get( databaseCustomerMarketPlace.Id );

            Write(mpCustomerMarketPlace, data, historyRecord);
		}

		private MP_AnalyisisFunction GetFunction(IDatabaseFunction databaseFunction)
		{
			return _AnalyisisFunctionRepository.Get( databaseFunction.InternalId );
		}

		private MP_AnalysisFunctionTimePeriod GetTimePeriod( ITimePeriod timePeriod )
		{
			return _AnalysisFunctionTimePeriodRepository.Get( timePeriod.InternalId );
		}

    	private void Write(MP_CustomerMarketPlace databaseCustomerMarketPlace, IDatabaseAnalysisFunctionValues data, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
			if ( data == null )
			{
				return;
			}

    		if ( data.Value != null )
			{
				CheckValue( data.Function.FunctionValueType.ValueType, data.Value );
			}

			var analysisFunctionTimePeriod = GetTimePeriod( data.TimePeriod );

			var analyisisFunction = GetFunction( data.Function );

			var analyisisFunctionValue = new MP_AnalyisisFunctionValue
    		    {
    		        CustomerMarketPlace = databaseCustomerMarketPlace,
    		        AnalyisisFunction = analyisisFunction,
    		        AnalysisFunctionTimePeriod = analysisFunctionTimePeriod,
    		        Value = data.Value == null ? null : data.Value.ToString(),
    		        Updated = data.UpdatedDate,
					HistoryRecord = historyRecord
    		    };


    		DatabaseValueTypeEnum valueType = data.ValueType;

			switch ( valueType )
			{
				case DatabaseValueTypeEnum.Integer:
					analyisisFunctionValue.ValueInt = (int?)data.Value;
					break;

				case DatabaseValueTypeEnum.Double:
					if ( data.Value is float )
					{
						analyisisFunctionValue.ValueFloat = (float?)data.Value;
					}
					else
					{
						analyisisFunctionValue.ValueFloat = (double?)data.Value;
					}
					break;

				case DatabaseValueTypeEnum.DateTime:				
					var dateValue = data.Value as DateTime?;
					analyisisFunctionValue.ValueDate = !dateValue.HasValue || DateTime.MinValue.Equals( dateValue ) ? null : dateValue;
					break;

				case DatabaseValueTypeEnum.String:
					analyisisFunctionValue.ValueString = (string)data.Value;
					break;

				case DatabaseValueTypeEnum.Xml:
					analyisisFunctionValue.ValueXml = (string)data.Value;
					break;

				case DatabaseValueTypeEnum.Boolean:
					analyisisFunctionValue.ValueBoolean = (bool?)data.Value;
					break;

				default:
					throw new NotImplementedException();
			}
			
			_AnalyisisFunctionValueRepository.SaveOrUpdate( analyisisFunctionValue );
		}

		#region CheckValue
		private static void CheckValue<T>(DatabaseValueTypeEnum functionValueType, T value)
		{
			bool rez = false;
			switch (functionValueType)
			{
				case DatabaseValueTypeEnum.String:
					rez = value is string;
					break;

				case DatabaseValueTypeEnum.Integer:
					rez = value is int;
					break;

				case DatabaseValueTypeEnum.Double:
					rez = value is double || value is float;
					break;

				case DatabaseValueTypeEnum.DateTime:
					rez = value is DateTime;
					break;

				case DatabaseValueTypeEnum.Xml:
					rez = value is string;
					break;

				case DatabaseValueTypeEnum.Boolean:
					rez = value is bool;
					break;

				default:
					throw new NotImplementedException();
			}

			if ( !rez )
			{
				throw new ArgumentException();
			}
		}
		#endregion
	}
}