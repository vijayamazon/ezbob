using System;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper.ValueType;
using EzBob.CommonLib;

namespace EzBob.eBayDbLib
{
	/*
	Note:
	 * 1) create function in eBayDatabaseFunctionStorage
	 * 2) define new case in class ebayDatabaseFunctionTypeConverter for display name	 
	 */

	public enum eBayDatabaseFunctionType
	{		
		/*UserBillingEmail,
		UsereBayGoodStanding,
		UserEIASToken,
		UserEmail,
		UserFeedbackPrivate,
		UserIDVerified,
		UserNewUser,
		UserPayPalAccountStatus,
		UserPayPalAccountType,
		UserQualifiesForSelling,
		UserRegistrationAddress,
		UserRegistrationDate,		
		UserSellerInfoQualifiesForB2BVAT,
		UserSellerInfoSellerBusinessType,
		UserSellerInfoSellerPaymentAddress,
		UserSellerInfoStoreOwner,
		UserSellerInfoStoreSite,
		UserSellerInfoStoreURL,
		UserSellerInfoTopRatedSeller,
		UserSellerInfoTopRatedSellerDetailsTopRatedProgram,
		UserSite,
		UserSkypeID,
		UserUserID,
		UserUserIDChanged,
		UserUserIDLastChanged,

		// account info
		AccountID,
		AccountSummaryAccountState,
		AccountSummaryAmountPastDueCurrency,
		AccountSummaryAmountPastDueValue,
		AccountSummaryBankAccountInfo,
		AccountSummaryBankModifyDate,
		AccountSummaryCreditCardExpiration,
		AccountSummaryCreditCardInfo,
		AccountSummaryCreditCardModifyDate,
		AccountSummaryCurrentBalance,
		AccountSummaryPastDue,
		AccountSummaryPaymentMethod,
		AccountSummaryAdditionalAccount,
		AccountCurrency,

		// feedback info
		FeedbackRepeatBuyerCount,
		FeedbackRepeatBuyerPercent,
		FeedbackTransactionPercent,
		FeedbackUniqueBuyerCount,
		UniqueNegativeFeedbackCount,
		UniquePositiveFeedbackCount,
		UniqueNeutralFeedbackCount,

		NegativeFeedbackByPeriod,
		PositiveFeedbackByPeriod,
		NeutralFeedbackByPeriod,

		RatingCountItemAsDescribed,
		RatingItemAsDescribed,
		RatingCountCommunication,
		RatingCommunication,
		RatingCountShippingTime,
		RatingShippingTime,
		RatingCountShippingAndHandlingCharges,
		RatingShippingAndHandlingCharges*/
		AverageItemsPerOrder,
		AverageSumOfOrder,
		CancelledOrdersCount,
		NumOfOrders,
		OrdersCancellationRate,
		TotalItemsOrdered,
		TotalSumOfOrders,
		TotalSumOfOrdersAnnualized,
		InventoryTotalItems,
		InventoryTotalValue,
        TopCategories
	}

	internal class eBayDatabaseFunctionStorage : DatabaseFunctionStorage<eBayDatabaseFunctionType>
	{
		private static eBayDatabaseFunctionStorage _Instance;

		private eBayDatabaseFunctionStorage()
			: base( new ebayDatabaseFunctionTypeConverter())
		{
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.NumOfOrders, DatabaseValueTypeEnum.Integer, "{EA9CBDAF-3C46-4617-9D5C-5509DBB99129}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.AverageItemsPerOrder, DatabaseValueTypeEnum.Integer, "{7829570C-A046-43DB-8BD3-966713D36F58}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.AverageSumOfOrder, DatabaseValueTypeEnum.Double, "{3A96AE26-7E4E-4CB7-86A3-A66F9040565F}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.CancelledOrdersCount, DatabaseValueTypeEnum.Integer, "{35E1D5BD-5516-4333-B8F8-4BDE93528D0B}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.OrdersCancellationRate, DatabaseValueTypeEnum.Double, "{F7557576-C18B-4E31-95EB-D3E84E08BA71}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.TotalItemsOrdered, DatabaseValueTypeEnum.Integer, "{2CEA9947-674A-4E1F-B879-D47E627E6E0D}" );
			CreateFunctionAndAddToCollection(eBayDatabaseFunctionType.TotalSumOfOrders, DatabaseValueTypeEnum.Double, "{B90CA21F-069B-4076-BB87-D6FC09457971}");
			CreateFunctionAndAddToCollection(eBayDatabaseFunctionType.TotalSumOfOrdersAnnualized, DatabaseValueTypeEnum.Double, "{29C87037-2133-4873-9208-96A4F0163D54}");
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.InventoryTotalItems, DatabaseValueTypeEnum.Integer, "{4D8E5159-5001-4DD8-8B25-EB86C9D1F891}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.InventoryTotalValue, DatabaseValueTypeEnum.Double, "{E612A31D-C9F8-4636-BDC9-E7671BA29A48}" );
            CreateFunctionAndAddToCollection(eBayDatabaseFunctionType.TopCategories, DatabaseValueTypeEnum.String, "{0E0D49A5-0C37-4820-BC02-0E884059666A}");

			// user info
			/*
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.UserBillingEmail, DatabaseValueTypeEnum.String, "{A9F3069E-26CA-4EAD-B39C-10B4995D409D}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.UsereBayGoodStanding, DatabaseValueTypeEnum.Boolean, "{7B17BF66-D7C6-4B26-AE09-62241A360241}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.UserEIASToken, DatabaseValueTypeEnum.String, "{864DCAE4-0328-440C-A455-44EEEEF6D596}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.UserEmail, DatabaseValueTypeEnum.String, "{89374266-B278-4831-80F5-A9FCC3E9BB00}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.UserFeedbackPrivate, DatabaseValueTypeEnum.Boolean, "{D8B75367-3448-4890-826D-285B033421D9}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.UserIDVerified, DatabaseValueTypeEnum.Boolean, "{E7788CDA-0A67-4EB7-A3F3-1606BF300DFA}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.UserNewUser, DatabaseValueTypeEnum.Boolean, "{E85DA806-75D6-4455-A9EF-579A2EA58DB9}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.UserPayPalAccountStatus, DatabaseValueTypeEnum.String, "{57B2C454-256D-4240-8562-F71C19C3629C}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.UserPayPalAccountType, DatabaseValueTypeEnum.String, "{0AE6DE04-7DA6-4316-A70E-1C54D2FF20CA}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.UserQualifiesForSelling, DatabaseValueTypeEnum.Boolean, "{3895B41B-B8D8-497B-A441-1B6ABD28FA8A}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.UserRegistrationAddress, DatabaseValueTypeEnum.Xml, "{C0CC2158-5A73-4832-8025-7FC49A1694F5}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.UserRegistrationDate, DatabaseValueTypeEnum.DateTime, "{9BA8BC7D-DD91-43CD-9E9B-6A9E1E554AFC}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.UserSellerInfoQualifiesForB2BVAT, DatabaseValueTypeEnum.Boolean, "{B640152E-FBAF-479F-A367-7D8E0277ABB5}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.UserSellerInfoSellerBusinessType, DatabaseValueTypeEnum.String, "{06E8C9ED-3677-45A8-8E2F-AAA98CA09468}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.UserSellerInfoSellerPaymentAddress, DatabaseValueTypeEnum.Xml, "{63D7B90F-D029-407A-8B20-4DBAED89AFF1}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.UserSellerInfoStoreOwner, DatabaseValueTypeEnum.Boolean, "{A4F91BB3-FDE8-46D9-8822-B6DFEA23E418}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.UserSellerInfoStoreSite, DatabaseValueTypeEnum.String, "{7C094879-3CE4-41CD-9B47-2D9E72C71E20}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.UserSellerInfoStoreURL, DatabaseValueTypeEnum.String, "{5D5C5733-9DB8-416A-86E9-927B329308FB}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.UserSellerInfoTopRatedSeller, DatabaseValueTypeEnum.Boolean, "{22B199DF-F29C-4E6C-8B77-15F92D0AD44C}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.UserSellerInfoTopRatedSellerDetailsTopRatedProgram, DatabaseValueTypeEnum.String, "{6246C43E-C16A-4774-9992-2B40E80CAC47}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.UserSite, DatabaseValueTypeEnum.String, "{135F0B2C-2FC7-496C-AB39-F5F58CF442A5}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.UserSkypeID, DatabaseValueTypeEnum.String, "{C62A2F78-0D2A-4813-A59C-4992ED59C0FD}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.UserUserID, DatabaseValueTypeEnum.String, "{0A90F8A4-8F7C-41DC-95DD-18DA02E6AEAE}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.UserUserIDChanged, DatabaseValueTypeEnum.Boolean, "{3C6936BD-EC05-4D93-B865-5619219419AC}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.UserUserIDLastChanged, DatabaseValueTypeEnum.DateTime, "{BB595075-98C1-416D-ADAF-959988E38A96}" );
			// account info
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.AccountID, DatabaseValueTypeEnum.String, "{7D739447-C38D-4633-B02F-65D628C060A4}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.AccountSummaryAccountState, DatabaseValueTypeEnum.String, "{DE8270F1-6C8B-4A9D-B5CE-DB88ACE4C94E}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.AccountSummaryAmountPastDueCurrency, DatabaseValueTypeEnum.String, "{F3FFE38C-1626-470A-94E5-35AAE44FCE98}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.AccountSummaryAmountPastDueValue, DatabaseValueTypeEnum.Double, "{A69277FE-0245-4FA1-811F-397A136DD9FD}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.AccountSummaryBankAccountInfo, DatabaseValueTypeEnum.String, "{752FB0E7-48DE-414C-865F-82339D0BA0D7}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.AccountSummaryBankModifyDate, DatabaseValueTypeEnum.DateTime, "{3D6A117F-7583-476B-BD38-918346C2C4E8}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.AccountSummaryCreditCardExpiration, DatabaseValueTypeEnum.DateTime, "{23EA4DE6-690B-46D2-BB5E-E106CE0D271A}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.AccountSummaryCreditCardInfo, DatabaseValueTypeEnum.String, "{E5F46335-3518-4238-932F-2454E4C0E23D}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.AccountSummaryCreditCardModifyDate, DatabaseValueTypeEnum.DateTime, "{3AE8A86C-BC64-49C4-9918-C937422DA090}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.AccountSummaryCurrentBalance, DatabaseValueTypeEnum.Double, "{914A91C2-6805-436D-8A2D-09C63E187146}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.AccountSummaryPastDue, DatabaseValueTypeEnum.Boolean, "{EA2E58CB-FBF9-4B89-A5A1-F81B923DA460}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.AccountSummaryPaymentMethod, DatabaseValueTypeEnum.String, "{DF1EF5F1-E80B-454E-8B07-EA1A5455FA1E}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.AccountSummaryAdditionalAccount, DatabaseValueTypeEnum.String, "{D08287AB-D025-4E1B-9EC5-4F58D4F8AD70}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.AccountCurrency, DatabaseValueTypeEnum.String, "{49579DCA-1EBF-418E-82AC-6E5BBB9BA4DC}" );

			// feedback info
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.FeedbackRepeatBuyerCount, DatabaseValueTypeEnum.Integer, "{2EA18686-F314-4F7F-AFA8-31815256A597}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.FeedbackRepeatBuyerPercent, DatabaseValueTypeEnum.Double, "{89267204-7BFE-4C7F-8DDE-F96FFDDA79EC}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.FeedbackTransactionPercent, DatabaseValueTypeEnum.Double, "{F82ECE00-C936-4B15-821B-706A84DF8077}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.FeedbackUniqueBuyerCount, DatabaseValueTypeEnum.Integer, "{7F920C46-E859-470E-837D-29E48F06D495}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.UniqueNegativeFeedbackCount, DatabaseValueTypeEnum.Integer, "{87DE2228-BCF6-4266-99A4-49BEFA63874D}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.UniquePositiveFeedbackCount, DatabaseValueTypeEnum.Integer, "{D5893C8A-174B-4D68-B7FD-918EFE0601CD}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.UniqueNeutralFeedbackCount, DatabaseValueTypeEnum.Integer, "{C2C9199D-C245-48BA-8ECD-F41925A67F6C}" );

			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.NegativeFeedbackByPeriod, DatabaseValueTypeEnum.Integer, "{136B2B81-6BFB-47AE-BC2A-B28785199D87}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.PositiveFeedbackByPeriod, DatabaseValueTypeEnum.Integer, "{A3FC7EA5-344F-4459-B87C-FF2A1602AA85}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.NeutralFeedbackByPeriod, DatabaseValueTypeEnum.Integer, "{EF4DB3FD-0235-4EC1-812A-793EDEB4FE3C}" );
			//rating
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.RatingCountItemAsDescribed, DatabaseValueTypeEnum.Integer, "{916DF857-DDB8-42C9-9824-D2139F76904F}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.RatingItemAsDescribed, DatabaseValueTypeEnum.Double, "{AEF5A0F4-5354-4B11-85E8-5DEB4F5A08B6}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.RatingCountCommunication, DatabaseValueTypeEnum.Integer, "{9273B019-5A78-4D78-B0AF-BAE02243B3E5}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.RatingCommunication, DatabaseValueTypeEnum.Double, "{E5939395-8B8A-453E-BD25-E913E69F8743}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.RatingCountShippingTime, DatabaseValueTypeEnum.Integer, "{63B78264-6C62-4B52-A24D-3789866FE9F7}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.RatingShippingTime, DatabaseValueTypeEnum.Double, "{55F4AD8E-F19E-4560-8459-0717B74A462A}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.RatingCountShippingAndHandlingCharges, DatabaseValueTypeEnum.Integer, "{FBE71FAA-C128-4EFD-8C04-44FF93026085}" );
			CreateFunctionAndAddToCollection( eBayDatabaseFunctionType.RatingShippingAndHandlingCharges, DatabaseValueTypeEnum.Double, "{7E64F208-FE5B-49D5-A66D-5258E80A18AC}" );
			*/
		}

		public static eBayDatabaseFunctionStorage Instance
		{
			get
			{
				return _Instance ?? ( _Instance = new eBayDatabaseFunctionStorage() );
			}
		}
	}

	internal class ebayDatabaseFunctionTypeConverter : IDatabaseEnumTypeConverter<eBayDatabaseFunctionType>
	{
		public ConvertedTypeInfo Convert(eBayDatabaseFunctionType type)
		{
			string displayName = string.Empty;			
			string description = string.Empty;

			string name = type.ToString();

			switch (type)
			{
				case eBayDatabaseFunctionType.NumOfOrders:
					displayName = "Num of Orders";
					break;

				case eBayDatabaseFunctionType.AverageItemsPerOrder:
					displayName = "Average Items per Order";
					break;

				case eBayDatabaseFunctionType.AverageSumOfOrder:
					displayName = "Average Sum of Order";
					break;

				case eBayDatabaseFunctionType.CancelledOrdersCount:
					displayName = "Cancelled Orders Count";
					break;

				case eBayDatabaseFunctionType.OrdersCancellationRate:
					displayName = "Orders Cancellation Rate";
					break;

				case eBayDatabaseFunctionType.TotalItemsOrdered:
					displayName = "Total Items Ordered";
					break;

				case eBayDatabaseFunctionType.TotalSumOfOrders:
					displayName = "Total Sum of Orders";
					break;

				case eBayDatabaseFunctionType.TotalSumOfOrdersAnnualized:
					displayName = "Total Sum of Orders Annualized";
					break;

				case eBayDatabaseFunctionType.InventoryTotalValue:
					displayName = "Total Value of Inventory";
					break;

				case eBayDatabaseFunctionType.InventoryTotalItems:
					displayName = "Total Items in Inventory";
					break;

                case eBayDatabaseFunctionType.TopCategories:
					displayName = "Top Categories";
					break;
				/*case eBayDatabaseFunctionType.UserBillingEmail:
					name = "UserBillingEmail";
					break;

				case eBayDatabaseFunctionType.UserEIASToken:
					name = "UserEIASToken";
					break;

				case eBayDatabaseFunctionType.UserEmail:
					name = "UserEmail";
					break;

				case eBayDatabaseFunctionType.UserFeedbackPrivate:
					name = "UserFeedbackPrivate";
					break;

				case eBayDatabaseFunctionType.UserIDVerified:
					name = "UserIDVerified";
					break;

				case eBayDatabaseFunctionType.UserNewUser:
					name = "UserNewUser";
					break;

				case eBayDatabaseFunctionType.UserPayPalAccountStatus:
					name = "UserPayPalAccountStatus";
					break;

				case eBayDatabaseFunctionType.UserPayPalAccountType:
					name = "UserPayPalAccountType";
					break;

				case eBayDatabaseFunctionType.UserQualifiesForSelling:
					name = "UserQualifiesForSelling";
					break;

				case eBayDatabaseFunctionType.UserRegistrationAddress:
					name = "UserRegistrationAddress";
					break;

				case eBayDatabaseFunctionType.UserRegistrationDate:
					name = "UserRegistrationDate";
					break;

				case eBayDatabaseFunctionType.UserSellerInfoQualifiesForB2BVAT:
					name = "UserSellerInfoQualifiesForB2BVAT";
					break;

				case eBayDatabaseFunctionType.UserSellerInfoSellerBusinessType:
					name = "UserSellerInfoSellerBusinessType";
					break;

				case eBayDatabaseFunctionType.UserSellerInfoSellerPaymentAddress:
					name = "UserSellerInfoSellerPaymentAddress";
					break;

				case eBayDatabaseFunctionType.UserSellerInfoStoreOwner:
					name = "UserSellerInfoStoreOwner";
					break;

				case eBayDatabaseFunctionType.UserSellerInfoStoreSite:
					name = "UserSellerInfoStoreSite";
					break;

				case eBayDatabaseFunctionType.UserSellerInfoStoreURL:
					name = "UserSellerInfoStoreURL";
					break;

				case eBayDatabaseFunctionType.UserSellerInfoTopRatedSeller:
					name = "UserSellerInfoTopRatedSeller";
					break;

				case eBayDatabaseFunctionType.UserSellerInfoTopRatedSellerDetailsTopRatedProgram:
					name = "UserSellerInfoTopRatedSellerDetailsTopRatedProgram";
					break;

				case eBayDatabaseFunctionType.UserSite:
					name = "UserSite";
					break;

				case eBayDatabaseFunctionType.UserSkypeID:
					name = "UserSkypeID";
					break;

				case eBayDatabaseFunctionType.UserUserID:
					name = "UserUserID";
					break;

				case eBayDatabaseFunctionType.UserUserIDChanged:
					name = "UserUserIDChanged";
					break;

				case eBayDatabaseFunctionType.UserUserIDLastChanged:
					name = "UserUserIDLastChanged";
					break;

				case eBayDatabaseFunctionType.UsereBayGoodStanding:
					name = "UsereBayGoodStanding";
					break;

				// account info
				case eBayDatabaseFunctionType.AccountID:
					name = "AccountID";
					break;

				case eBayDatabaseFunctionType.AccountSummaryAccountState:
					name = "AccountSummaryAccountState";
					break;

				case eBayDatabaseFunctionType.AccountSummaryAmountPastDueCurrency:
					name = "AccountSummaryAmountPastDueCurrency";
					break;

				case eBayDatabaseFunctionType.AccountSummaryAmountPastDueValue:
					name = "AccountSummaryAmountPastDueValue";
					break;

				case eBayDatabaseFunctionType.AccountSummaryBankAccountInfo:
					name = "AccountSummaryBankAccountInfo";
					break;

				case eBayDatabaseFunctionType.AccountSummaryBankModifyDate:
					name = "AccountSummaryBankModifyDate";
					break;

				case eBayDatabaseFunctionType.AccountSummaryCreditCardExpiration:
					name = "AccountSummaryCreditCardExpiration";
					break;

				case eBayDatabaseFunctionType.AccountSummaryCreditCardInfo:
					name = "AccountSummaryCreditCardInfo";
					break;

				case eBayDatabaseFunctionType.AccountSummaryCreditCardModifyDate:
					name = "AccountSummaryCreditCardModifyDate";
					break;

				case eBayDatabaseFunctionType.AccountSummaryCurrentBalance:
					name = "AccountSummaryCurrentBalance";
					break;

				case eBayDatabaseFunctionType.AccountSummaryPastDue:
					name = "AccountSummaryPastDue";
					break;

				case eBayDatabaseFunctionType.AccountSummaryPaymentMethod:
					name = "AccountSummaryPaymentMethod";
					break;

				case eBayDatabaseFunctionType.AccountSummaryAdditionalAccount:
					name = "AccountSummaryAdditionalAccount";
					break;

				case eBayDatabaseFunctionType.AccountCurrency:
					name = "AccountCurrency";
					break;

				// feedback info
				case eBayDatabaseFunctionType.FeedbackRepeatBuyerCount:
					name = "FeedbackRepeatBuyerCount";

					break;
				case eBayDatabaseFunctionType.FeedbackRepeatBuyerPercent:
					name = "FeedbackRepeatBuyerPercent";
					break;

				case eBayDatabaseFunctionType.FeedbackTransactionPercent:
					name = "FeedbackTransactionPercent";
					break;

				case eBayDatabaseFunctionType.FeedbackUniqueBuyerCount:
					name = "FeedbackUniqueBuyerCount";
					break;

				case eBayDatabaseFunctionType.UniqueNegativeFeedbackCount:
					name = "UniqueNegativeFeedbackCount";
					break;

				case eBayDatabaseFunctionType.UniquePositiveFeedbackCount:
					name = "UniquePositiveFeedbackCount";
					break;

				case eBayDatabaseFunctionType.UniqueNeutralFeedbackCount:
					name = "UniqueNeutralFeedbackCount";
					break;

				case eBayDatabaseFunctionType.NegativeFeedbackByPeriod:
					name = "NegativeFeedbackByPeriod";
					break;

				case eBayDatabaseFunctionType.PositiveFeedbackByPeriod:
					name = "PositiveFeedbackByPeriod";
					break;

				case eBayDatabaseFunctionType.NeutralFeedbackByPeriod:
					name = "NeutralFeedbackByPeriod";
					break;

				//rating
				case eBayDatabaseFunctionType.RatingCountItemAsDescribed:
					name = "RatingCountItemAsDescribed";
					break;

				case eBayDatabaseFunctionType.RatingItemAsDescribed:
					name = "RatingItemAsDescribed";
					break;

				case eBayDatabaseFunctionType.RatingCountCommunication:
					name = "RatingCountCommunication";
					break;

				case eBayDatabaseFunctionType.RatingCommunication:
					name = "RatingCommunication";
					break;

				case eBayDatabaseFunctionType.RatingCountShippingTime:
					name = "RatingCountShippingTime";
					break;

				case eBayDatabaseFunctionType.RatingShippingTime:
					name = "RatingShippingTime";
					break;

				case eBayDatabaseFunctionType.RatingCountShippingAndHandlingCharges:
					name = "RatingCountShippingAndHandlingCharges";
					break;

				case eBayDatabaseFunctionType.RatingShippingAndHandlingCharges:
					name = "RatingShippingAndHandlingCharges";
					break;
				*/
				default:
					throw new NotImplementedException();
			}

			return new ConvertedTypeInfo( name, displayName, description );
		}
	}
}
