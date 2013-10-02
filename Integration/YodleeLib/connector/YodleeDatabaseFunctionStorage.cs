namespace YodleeLib.connector
{
	using EZBob.DatabaseLib.Common;
	using EzBob.CommonLib;
	using EZBob.DatabaseLib.DatabaseWrapper.ValueType;
	using System;

	public enum YodleeDatabaseFunctionType
	{
		TotlaIncome,
		TotalExpense,
		NumberOfTransactions,
		CurrentBalance,
		AvailableBalance
	}

	internal class YodleeDatabaseFunctionStorage : DatabaseFunctionStorage<YodleeDatabaseFunctionType>
	{
		private static YodleeDatabaseFunctionStorage _Instance;

		private YodleeDatabaseFunctionStorage()
			: base(new YodleeDatabaseFunctionTypeConverter())
		{
			CreateFunctionAndAddToCollection(YodleeDatabaseFunctionType.TotlaIncome, DatabaseValueTypeEnum.Double, "{4E9ED37D-9D0B-4095-8E72-FDADDD65234D}");
			CreateFunctionAndAddToCollection(YodleeDatabaseFunctionType.TotalExpense, DatabaseValueTypeEnum.Double, "{57545B4E-017F-4A91-B5CD-96479E14FE08}");
			CreateFunctionAndAddToCollection(YodleeDatabaseFunctionType.NumberOfTransactions, DatabaseValueTypeEnum.Integer, "{5D61285F-73C8-4645-9108-B23CC2DA8520}");
			CreateFunctionAndAddToCollection(YodleeDatabaseFunctionType.CurrentBalance, DatabaseValueTypeEnum.Double, "{621290B5-EC51-44A7-AD06-373EDC6367D8}");
			CreateFunctionAndAddToCollection(YodleeDatabaseFunctionType.AvailableBalance, DatabaseValueTypeEnum.Double, "{598C5ED7-59A5-4905-BFDF-F35EFAA095BA}");
		}

		public static YodleeDatabaseFunctionStorage Instance
		{
			get
			{
				return _Instance ?? (_Instance = new YodleeDatabaseFunctionStorage());
			}
		}
	}

	internal class YodleeDatabaseFunctionTypeConverter : IDatabaseEnumTypeConverter<YodleeDatabaseFunctionType>
	{
		public ConvertedTypeInfo Convert(YodleeDatabaseFunctionType type)
		{
			string displayName;
			string description = string.Empty;

			string name = type.ToString();

			switch (type)
			{
				case YodleeDatabaseFunctionType.TotlaIncome:
					displayName = "Total Income";
					break;

				case YodleeDatabaseFunctionType.TotalExpense:
					displayName = "Total Expense";
					break;

				case YodleeDatabaseFunctionType.NumberOfTransactions:
					displayName = "Number Of Transactions";
					break;
				case YodleeDatabaseFunctionType.CurrentBalance:
					displayName = "Current Balance";
					break;

				case YodleeDatabaseFunctionType.AvailableBalance:
					displayName = "Available Balance";
					break;

				default:
					throw new NotImplementedException();
			}

			return new ConvertedTypeInfo(name, displayName, description);
		}
	}
}
