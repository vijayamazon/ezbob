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
		TotalNetExpenses,
		NumOfTotalTransactions,
		RevenuesForTransactions,
		NetNumOfRefundsAndReturns,
		TransferAndWireOut,
		TransferAndWireIn,
	}

	internal class PayPalDatabaseFunctionStorage : DatabaseFunctionStorage<PayPalDatabaseFunctionType>
	{
		private static PayPalDatabaseFunctionStorage _Instance;

		private PayPalDatabaseFunctionStorage()
			: base(new PayPalDatabaseFunctionTypeConverter())
		{
			CreateFunctionAndAddToCollection(PayPalDatabaseFunctionType.TransactionsNumber, DatabaseValueTypeEnum.Integer, "{D51543B7-32D4-450F-A047-58AA72C747E3}");
			CreateFunctionAndAddToCollection(PayPalDatabaseFunctionType.TotalNetInPayments, DatabaseValueTypeEnum.Double, "{9370A525-890D-402B-9BAA-5C89E9905CA2}");
			CreateFunctionAndAddToCollection(PayPalDatabaseFunctionType.TotalNetOutPayments, DatabaseValueTypeEnum.Double, "{B8780D1E-3902-4DD1-97A6-D185F3DE7555}");
			CreateFunctionAndAddToCollection(PayPalDatabaseFunctionType.TotalNetRevenues, DatabaseValueTypeEnum.Double, "{9E95B0DE-4EBB-433C-B87D-84C124099006}");
			CreateFunctionAndAddToCollection(PayPalDatabaseFunctionType.TotalNetExpenses, DatabaseValueTypeEnum.Double, "{858D167C-A98B-4DE7-A194-32C0A14604CD}");
			CreateFunctionAndAddToCollection(PayPalDatabaseFunctionType.NumOfTotalTransactions, DatabaseValueTypeEnum.Integer, "{D2A09968-3BAD-4589-B6C5-14D4F35A45DC}");
			CreateFunctionAndAddToCollection(PayPalDatabaseFunctionType.RevenuesForTransactions, DatabaseValueTypeEnum.Double, "{F042B900-071A-4EB5-9DFB-00FE0A630C55}");
			CreateFunctionAndAddToCollection(PayPalDatabaseFunctionType.NetNumOfRefundsAndReturns, DatabaseValueTypeEnum.Integer, "{7583139B-639C-49BF-A46B-DF98EE7305AF}");
			CreateFunctionAndAddToCollection(PayPalDatabaseFunctionType.TransferAndWireOut, DatabaseValueTypeEnum.Double, "{03029A8F-2E7C-4331-8892-0C3F93788718}");
			CreateFunctionAndAddToCollection(PayPalDatabaseFunctionType.TransferAndWireIn, DatabaseValueTypeEnum.Double, "{E875B318-D398-4B2D-8E0C-19DA034B9E97}");
		}

		public static PayPalDatabaseFunctionStorage Instance
		{
			get
			{
				return _Instance ?? (_Instance = new PayPalDatabaseFunctionStorage());
			}
		}
	}

	internal class PayPalDatabaseFunctionTypeConverter : IDatabaseEnumTypeConverter<PayPalDatabaseFunctionType>
	{
		public ConvertedTypeInfo Convert(PayPalDatabaseFunctionType type)
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
				case PayPalDatabaseFunctionType.TotalNetExpenses:
					displayName = "TotalNetExpenses";
					break;
				case PayPalDatabaseFunctionType.NumOfTotalTransactions:
					displayName = "Num Of Total Transactions";
					break;
				case PayPalDatabaseFunctionType.RevenuesForTransactions:
					displayName = "Revenues For Transactions";
					break;
				case PayPalDatabaseFunctionType.NetNumOfRefundsAndReturns:
					displayName = "Net Num Of Refunds And Returns";
					break;
				case PayPalDatabaseFunctionType.TransferAndWireOut:
					displayName = "Transfer And Wire Out";
					break;
				case PayPalDatabaseFunctionType.TransferAndWireIn:
					displayName = "Transfer And Wire In";
					break;
				default:
					throw new NotImplementedException();
			}

			return new ConvertedTypeInfo(name, displayName, description);
		}
	}
}
