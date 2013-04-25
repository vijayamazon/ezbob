IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteCustomer]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DeleteCustomer]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE DeleteCustomer
	@CustomerID INT,
	@PasswordValidator VARCHAR(50)
AS
BEGIN

IF @PasswordValidator != 'I am sure I want to delete!'
BEGIN
	RAISERROR('Cant delete customer without password validation', 16, 1)
	RETURN -1
END

IF EXISTS (SELECT 1 FROM DecisionHistory WHERE CustomerId = @CustomerID AND Action = 'Approve')
BEGIN
	RAISERROR('Cant delete customer with approved loans', 16, 1)
	RETURN -2
END
	
IF EXISTS (SELECT 1 FROM Loan WHERE Loan.CustomerId = @CustomerID)
BEGIN
	RAISERROR('Cant delete customer with a loan', 16, 1)
	RETURN -3
END

BEGIN TRANSACTION DeleteCustomer

	DELETE FROM ExperianConsentAgreement WHERE CustomerId = @CustomerID
	
	DELETE FROM ExperianDefaultAccountsData WHERE CustomerId = @CustomerID
	
	DELETE FROM DirectorAddressRelation WHERE DirectorId IN 
		(SELECT id FROM Director WHERE CustomerId = @CustomerID)
	
	DELETE FROM Director WHERE CustomerId = @CustomerID

	DELETE FROM MP_AmazonFeedbackItem WHERE AmazonFeedbackId IN 
		(SELECT MP_AmazonFeedback.Id FROM MP_AmazonFeedback WHERE CustomerMarketPlaceId IN 
			(SELECT Id FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerID))
	
	DELETE FROM MP_AmazonFeedback WHERE CustomerMarketPlaceId IN 
		(SELECT Id FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerID)

	DELETE FROM MP_AmazonOrderItem WHERE AmazonOrderId IN 
		(SELECT MP_AmazonOrder.Id FROM MP_AmazonOrder WHERE CustomerMarketPlaceId IN
			(SELECT Id FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerID))

	DELETE FROM MP_AmazonOrderItemDetailCatgory WHERE AmazonOrderItemDetailId IN
		(SELECT Id FROM MP_AmazonOrderItemDetail WHERE OrderItem2Id IN 
			(SELECT Id FROM MP_AmazonOrderItem2 WHERE AmazonOrderId IN 
				(SELECT MP_AmazonOrder.Id FROM MP_AmazonOrder WHERE CustomerMarketPlaceId IN
					(SELECT Id FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerID))))
						
	DELETE FROM MP_AmazonOrderItemDetail WHERE OrderItem2Id IN 
		(SELECT Id FROM MP_AmazonOrderItem2 WHERE AmazonOrderId IN 
			(SELECT MP_AmazonOrder.Id FROM MP_AmazonOrder WHERE CustomerMarketPlaceId IN
				(SELECT Id FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerID)))
			
	DELETE FROM MP_AmazonOrderItem2Payment WHERE OrderItem2Id IN 
		(SELECT MP_AmazonOrderItem2.Id FROM MP_AmazonOrderItem2 WHERE AmazonOrderId IN 
			(SELECT MP_AmazonOrder.Id FROM MP_AmazonOrder WHERE CustomerMarketPlaceId IN
				(SELECT Id FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerID)))

	DELETE FROM MP_AmazonOrderItem2 WHERE AmazonOrderId IN 
		(SELECT MP_AmazonOrder.Id FROM MP_AmazonOrder WHERE CustomerMarketPlaceId IN
			(SELECT Id FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerID))

	DELETE FROM MP_AmazonOrder WHERE CustomerMarketPlaceId IN
		(SELECT Id FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerID)
		
	DELETE FROM MP_CustomerMarketplaceUpdatingCounter WHERE CustomerMarketplaceUpdatingActionLogId IN
		(SELECT Id FROM MP_CustomerMarketplaceUpdatingActionLog WHERE CustomerMarketplaceUpdatingHistoryRecordId IN
			(SELECT Id FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerID))
		
	DELETE FROM MP_CustomerMarketPlaceUpdatingHistory WHERE CustomerMarketPlaceId IN
		(SELECT Id FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerID)
		
	DELETE FROM MP_CustomerMarketplaceUpdatingActionLog WHERE CustomerMarketplaceUpdatingHistoryRecordId IN
		(SELECT Id FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerID)
			
	DELETE FROM MP_EbayAmazonInventoryItem WHERE InventoryId IN
		(SELECT Id FROM MP_EbayAmazonInventory WHERE CustomerMarketPlaceId IN
			(SELECT Id FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerID))
		
	DELETE FROM MP_EbayAmazonInventory WHERE CustomerMarketPlaceId IN
		(SELECT Id FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerID)
			
	DELETE FROM MP_EbayFeedbackItem WHERE EbayFeedbackId IN 
		(SELECT Id FROM MP_EbayFeedback WHERE CustomerMarketPlaceId IN 	
			(SELECT Id FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerID))
		
	DELETE FROM MP_EbayRaitingItem WHERE EbayFeedbackId IN
		(SELECT Id FROM MP_EbayFeedback WHERE CustomerMarketPlaceId IN 	
			(SELECT Id FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerID))
	
	DELETE FROM MP_EbayFeedback WHERE CustomerMarketPlaceId IN 	
		(SELECT Id FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerID)
				
	DELETE FROM MP_EbayExternalTransaction WHERE OrderItemId IN 
		(SELECT Id FROM MP_EbayOrderItem WHERE OrderId IN
			(SELECT Id FROM MP_EbayOrder WHERE CustomerMarketPlaceId IN 
				(SELECT Id FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerID)))

	DELETE FROM MP_EbayTransaction WHERE OrderItemId IN
		(SELECT Id FROM MP_EbayOrderItem WHERE OrderId IN
			(SELECT Id FROM MP_EbayOrder WHERE CustomerMarketPlaceId IN 
				(SELECT Id FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerID)))
			
	DELETE FROM MP_EbayOrderItem WHERE OrderId IN
		(SELECT Id FROM MP_EbayOrder WHERE CustomerMarketPlaceId IN 
			(SELECT Id FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerID))

	DELETE FROM MP_EbayOrder WHERE CustomerMarketPlaceId IN 
		(SELECT Id FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerID)
			
	DELETE FROM MP_EbayUserData WHERE CustomerMarketPlaceId IN 
		(SELECT Id FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerID)
		
	DELETE FROM MP_EbayUserAdditionalAccountData WHERE EbayUserAccountDataId IN
		(SELECT Id FROM MP_EbayUserAccountData WHERE CustomerMarketPlaceId	 IN 
			(SELECT Id FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerID))
			
	DELETE FROM MP_EbayUserAccountData WHERE CustomerMarketPlaceId IN 
		(SELECT Id FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerID)
			
	DELETE FROM MP_EbayUserAddressData WHERE Id IN
		(SELECT RegistrationAddressId FROM MP_EbayUserData WHERE CustomerMarketPlaceId IN 
			(SELECT Id FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerID))
			
	DELETE FROM MP_EbayUserAddressData WHERE Id IN
		(SELECT SellerInfoSellerPaymentAddressId FROM MP_EbayUserData WHERE CustomerMarketPlaceId IN 
			(SELECT Id FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerID))			
	
	DELETE FROM MP_ExperianBankCache WHERE ServiceLogId IN
		(SELECT Id FROM MP_ServiceLog WHERE CustomerId = @CustomerID)
	
	DELETE FROM MP_ExperianDataCache WHERE CustomerId = @CustomerID
	
	DELETE FROM MP_ServiceLog WHERE CustomerId = @CustomerID
		
	DELETE FROM MP_PayPalPersonalInfo WHERE CustomerMarketPlaceId IN 
		(SELECT Id FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerID)	

	DELETE FROM MP_PayPalTransactionItem WHERE TransactionId IN 
		(SELECT Id FROM MP_PayPalTransaction WHERE CustomerMarketPlaceId IN 
			(SELECT Id FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerID))

	DELETE FROM MP_PayPalTransaction WHERE CustomerMarketPlaceId IN 
		(SELECT Id FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerID)
	
	DELETE FROM MP_TeraPeakOrderItem WHERE TeraPeakOrderId IN
		(SELECT Id FROM MP_TeraPeakOrder WHERE CustomerMarketPlaceId IN 
			(SELECT Id FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerID))
	
	DELETE FROM MP_TeraPeakOrder WHERE CustomerMarketPlaceId IN 
		(SELECT Id FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerID)

	DELETE FROM MP_AnalyisisFunctionValues WHERE CustomerMarketPlaceId IN 
		(SELECT Id FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerID)
		
	DELETE FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerID

	DELETE FROM PayPointCard WHERE CustomerId = @CustomerID

	DELETE FROM PersonalInfoHistory WHERE CustomerId = @CustomerID
	
	DELETE FROM PostcodeServiceLog WHERE CustomerId = @CustomerID
	
	DELETE FROM Strategy_CustomerUpdateHistory WHERE CustomerId = @CustomerID
		
	DELETE FROM CustomerAddress WHERE addressId IN 
		(SELECT addressId FROM CustomerAddressRelation WHERE customerId = @CustomerID)

	DELETE FROM CustomerAddressRelation WHERE customerId = @CustomerID
	
	DELETE FROM CashRequests WHERE IdCustomer = @CustomerID

	DELETE FROM CustomerScoringResult WHERE CustomerId = @CustomerID

	DELETE FROM DecisionHistory WHERE CustomerId = @CustomerID

	DELETE FROM Director WHERE CustomerId = @CustomerID

	DELETE FROM EmailConfirmationRequest WHERE CustomerId = @CustomerID

	DELETE FROM Loan WHERE CustomerId = @CustomerID
	
	DELETE FROM MP_Alert WHERE CustomerId = @CustomerID
	
	DELETE FROM Customer WHERE Id = @CustomerID
		
	DELETE FROM CardInfo WHERE CardInfo.CustomerId = @CustomerID
	
COMMIT TRANSACTION DeleteCustomer

END
GO
