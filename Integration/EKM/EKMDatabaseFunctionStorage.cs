﻿using EZBob.DatabaseLib.Common;
using EzBob.CommonLib;
using EZBob.DatabaseLib.DatabaseWrapper.ValueType;
using System;

namespace EKM
{
	public enum EkmDatabaseFunctionType
	{
        //Stas test
        NumOfOrders,
       // TotalItemsOrdered,
        TotalSumOfOrders,//revenue
        //AverageItemsPerOrder,
        AverageSumOfOrder,
	}

    internal class EKMDatabaseFunctionStorage : DatabaseFunctionStorage<EkmDatabaseFunctionType>
    {
        private static EKMDatabaseFunctionStorage _Instance;

        private EKMDatabaseFunctionStorage()
            : base(new EKMDatabaseFunctionTypeConverter())
        {
            //Stas test
            CreateFunctionAndAddToCollection(EkmDatabaseFunctionType.NumOfOrders, DatabaseValueTypeEnum.Integer, "{fa09ce65-d6a9-4656-b00c-5e635d2083c2}");
            //CreateFunctionAndAddToCollection(EKMDatabaseFunctionType.TotalItemsOrdered, DatabaseValueTypeEnum.Integer, "{e216ef11-ddc2-4721-91fe-6237928c79b2}");
            CreateFunctionAndAddToCollection(EkmDatabaseFunctionType.TotalSumOfOrders, DatabaseValueTypeEnum.Double, "{8687dabc-c092-4296-92ff-42612a59157a}");
            //CreateFunctionAndAddToCollection(EKMDatabaseFunctionType.AverageItemsPerOrder, DatabaseValueTypeEnum.Integer, "{b16493fa-da57-40ec-9939-2d86ff93f774}");
            CreateFunctionAndAddToCollection(EkmDatabaseFunctionType.AverageSumOfOrder, DatabaseValueTypeEnum.Double, "{86a7e774-b48b-44c1-8db1-f3212e5063f2}");
            
        }

        public static EKMDatabaseFunctionStorage Instance
        {
            get
            {
                return _Instance ?? (_Instance = new EKMDatabaseFunctionStorage());
            }
        }
    }

	internal class EKMDatabaseFunctionTypeConverter : IDatabaseEnumTypeConverter<EkmDatabaseFunctionType>
	{
		public ConvertedTypeInfo Convert(EkmDatabaseFunctionType type)
		{
            //Stas test
            string displayName = string.Empty;
            string description = string.Empty;

            string name = type.ToString();

            switch (type)
            {
                case EkmDatabaseFunctionType.NumOfOrders:
                    displayName = "Num of Orders";
                    break;

                //case EKMDatabaseFunctionType.AverageItemsPerOrder:
                //    displayName = "Average Items per Order";
                //    break;

                case EkmDatabaseFunctionType.AverageSumOfOrder:
                    displayName = "Average Sum of Order";
                    break;

                //case EKMDatabaseFunctionType.TotalItemsOrdered:
                //    displayName = "Total Items Ordered";
                //    break;

                case EkmDatabaseFunctionType.TotalSumOfOrders:
                    displayName = "Total Sum of Orders";
                    break;

                default:
                    throw new NotImplementedException();
            }

            return new ConvertedTypeInfo(name, displayName, description);
		}
	}
}