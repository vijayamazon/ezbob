﻿using EZBob.DatabaseLib.Common;
using EzBob.CommonLib;
using EZBob.DatabaseLib.DatabaseWrapper.ValueType;
using System;

namespace YodleeLib.connector
{
    public enum YodleeDatabaseFunctionType
    {
        TotlaIncome,
        TotalExpense,
        CurrentBalance,
    }

    internal class YodleeDatabaseFunctionStorage : DatabaseFunctionStorage<YodleeDatabaseFunctionType>
    {
        private static YodleeDatabaseFunctionStorage _Instance;

        private YodleeDatabaseFunctionStorage()
            : base(new YodleeDatabaseFunctionTypeConverter())
        {
            CreateFunctionAndAddToCollection(YodleeDatabaseFunctionType.TotlaIncome, DatabaseValueTypeEnum.Integer, "{4E9ED37D-9D0B-4095-8E72-FDADDD65234D}");
            CreateFunctionAndAddToCollection(YodleeDatabaseFunctionType.TotalExpense, DatabaseValueTypeEnum.Double, "{57545B4E-017F-4A91-B5CD-96479E14FE08}");
            CreateFunctionAndAddToCollection(YodleeDatabaseFunctionType.CurrentBalance, DatabaseValueTypeEnum.Double, "{621290B5-EC51-44A7-AD06-373EDC6367D8}");
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

                case YodleeDatabaseFunctionType.CurrentBalance:
                    displayName = "Current Balance";
                    break;

                default:
                    throw new NotImplementedException();
            }

            return new ConvertedTypeInfo(name, displayName, description);
        }
    }
}
