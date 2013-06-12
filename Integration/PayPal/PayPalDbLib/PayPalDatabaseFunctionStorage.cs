using System;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper.ValueType;
using EzBob.CommonLib;

namespace EzBob.PayPalDbLib
{
	public enum PayPalDatabaseFunctionType
	{
		TransactionsNumber,
		TotalNetInPayments,
		TotalNetOutPayments,
		TotalNetRevenues,
	}

	internal class PayPalDatabaseFunctionStorage : DatabaseFunctionStorage<PayPalDatabaseFunctionType>
	{
		private static PayPalDatabaseFunctionStorage _Instance;

		private PayPalDatabaseFunctionStorage()
			: base(new PayPalDatabaseFunctionTypeConverter())
		{
			CreateFunctionAndAddToCollection( PayPalDatabaseFunctionType.TransactionsNumber, DatabaseValueTypeEnum.Integer, "{D51543B7-32D4-450F-A047-58AA72C747E3}" );
			CreateFunctionAndAddToCollection( PayPalDatabaseFunctionType.TotalNetInPayments, DatabaseValueTypeEnum.Double, "{9370A525-890D-402B-9BAA-5C89E9905CA2}" );
			CreateFunctionAndAddToCollection( PayPalDatabaseFunctionType.TotalNetOutPayments, DatabaseValueTypeEnum.Double, "{B8780D1E-3902-4DD1-97A6-D185F3DE7555}" );
			CreateFunctionAndAddToCollection(PayPalDatabaseFunctionType.TotalNetRevenues, DatabaseValueTypeEnum.Double, "{9E95B0DE-4EBB-433C-B87D-84C124099006}");
		}

		public static PayPalDatabaseFunctionStorage Instance
		{
			get
			{
				return _Instance ?? ( _Instance = new PayPalDatabaseFunctionStorage() );
			}
		}
	}

	internal class PayPalDatabaseFunctionTypeConverter : IDatabaseEnumTypeConverter<PayPalDatabaseFunctionType>
	{
		public ConvertedTypeInfo Convert( PayPalDatabaseFunctionType type )
		{
			string name = type.ToString();
			string displayName = string.Empty;
			string description = string.Empty;

			switch (type)
			{
				case PayPalDatabaseFunctionType.TransactionsNumber:
					displayName = "Transactions Number";
					break;

				case PayPalDatabaseFunctionType.TotalNetInPayments:
					displayName = "Total Net In Payments";
					break;

				case PayPalDatabaseFunctionType.TotalNetOutPayments:
					displayName = "Total Net Out Payments";
					break;
				case PayPalDatabaseFunctionType.TotalNetRevenues:
					displayName = "Total Net Revenues";
					break;

				default:
					throw new NotImplementedException();
			}

			return new ConvertedTypeInfo( name, displayName, description );
		}
	}
}
