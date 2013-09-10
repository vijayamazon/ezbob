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

		GrossIncome,
		GrossProfitMargin,
		RevenuePerTrasnaction,
		NetSumOfRefundsAndReturns,
		RatioNetSumOfRefundsAndReturnsToNetRevenues,
		NetTransfersAmount,
		OutstandingBalance,
		NumTransfersOut,
		AmountPerTransferOut,
		NumTransfersIn,
		AmountPerTransferIn,
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

			CreateFunctionAndAddToCollection(PayPalDatabaseFunctionType.GrossIncome, DatabaseValueTypeEnum.Double, "{918eb136-1b69-434f-ba98-820ad1485efb}");
			CreateFunctionAndAddToCollection(PayPalDatabaseFunctionType.GrossProfitMargin, DatabaseValueTypeEnum.Double, "{df5cfd73-01f7-45fa-91f2-68d501df7469}");
			CreateFunctionAndAddToCollection(PayPalDatabaseFunctionType.RevenuePerTrasnaction, DatabaseValueTypeEnum.Double, "{48c15030-b545-4735-9c79-6eed266e16bf}");
			CreateFunctionAndAddToCollection(PayPalDatabaseFunctionType.NetSumOfRefundsAndReturns, DatabaseValueTypeEnum.Double, "{295aac57-d3c1-4100-85e9-2767747eabc2}");
			CreateFunctionAndAddToCollection(PayPalDatabaseFunctionType.RatioNetSumOfRefundsAndReturnsToNetRevenues, DatabaseValueTypeEnum.Double, "{625e1592-369c-4a48-91ef-2e3f85eafbef}");
			CreateFunctionAndAddToCollection(PayPalDatabaseFunctionType.NetTransfersAmount, DatabaseValueTypeEnum.Double, "{bf7e3d08-1c94-4059-8820-20dae4a0fabc}");
			CreateFunctionAndAddToCollection(PayPalDatabaseFunctionType.OutstandingBalance, DatabaseValueTypeEnum.Double, "{6695c6ce-97fc-433a-ab59-30419b388ec6}");
			CreateFunctionAndAddToCollection(PayPalDatabaseFunctionType.NumTransfersOut, DatabaseValueTypeEnum.Integer, "{939e1685-a8dd-405c-91dd-9bdaa8d1ace1}");
			CreateFunctionAndAddToCollection(PayPalDatabaseFunctionType.AmountPerTransferOut, DatabaseValueTypeEnum.Double, "{42acb093-077a-4a97-bc0c-af186813d90b}");
			CreateFunctionAndAddToCollection(PayPalDatabaseFunctionType.NumTransfersIn, DatabaseValueTypeEnum.Integer, "{f18a8389-6aab-4107-9aee-6aae234226fa}");
			CreateFunctionAndAddToCollection(PayPalDatabaseFunctionType.AmountPerTransferIn, DatabaseValueTypeEnum.Double, "{42cab093-077a-4a97-bc0c-af186813d90b}");
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

				case PayPalDatabaseFunctionType.GrossIncome:
					displayName = "Gross Income";
					break;
				case PayPalDatabaseFunctionType.GrossProfitMargin:
					displayName = "Gross Profit Margin";
					break;
				case PayPalDatabaseFunctionType.RevenuePerTrasnaction:
					displayName = "Revenue Per Trasnaction";
					break;
				case PayPalDatabaseFunctionType.NetSumOfRefundsAndReturns:
					displayName = "Net Sum Of Refunds And Returns";
					break;
				case PayPalDatabaseFunctionType.RatioNetSumOfRefundsAndReturnsToNetRevenues:
					displayName = "Ratio Net Sum Of Refunds And Returns To Net Revenues";
					break;
				case PayPalDatabaseFunctionType.NetTransfersAmount:
					displayName = "Net Transfers Amount";
					break;
				case PayPalDatabaseFunctionType.OutstandingBalance:
					displayName = "Outstanding Balance";
					break;
				case PayPalDatabaseFunctionType.NumTransfersOut:
					displayName = "Num Transfers Out";
					break;
				case PayPalDatabaseFunctionType.AmountPerTransferOut:
					displayName = "Amount Per Transfer Out";
					break;
				case PayPalDatabaseFunctionType.NumTransfersIn:
					displayName = "Num Transfers In";
					break;
				case PayPalDatabaseFunctionType.AmountPerTransferIn:
					displayName = "Amount Per Transfer In";
					break;
				default:
					throw new NotImplementedException();
			}

			return new ConvertedTypeInfo(name, displayName, description);
		}
	}
}
