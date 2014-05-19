USE [master]
GO
/****** Object:  Database [ezbob]    Script Date: 04-Nov-13 5:03:46 PM ******/
/*
CREATE DATABASE [ezbob]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'ezbob', FILENAME = N'c:\Program Files\Microsoft SQL Server\MSSQL11.SQLEXPRESS\MSSQL\DATA\ezbob.mdf' , SIZE = 1320192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'ezbob_log', FILENAME = N'c:\Program Files\Microsoft SQL Server\MSSQL11.SQLEXPRESS\MSSQL\DATA\ezbob_log.LDF' , SIZE = 4715200KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO
*/
ALTER DATABASE [ezbob] SET COMPATIBILITY_LEVEL = 100
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [ezbob].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [ezbob] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [ezbob] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [ezbob] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [ezbob] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [ezbob] SET ARITHABORT OFF 
GO
ALTER DATABASE [ezbob] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [ezbob] SET AUTO_CREATE_STATISTICS ON 
GO
ALTER DATABASE [ezbob] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [ezbob] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [ezbob] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [ezbob] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [ezbob] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [ezbob] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [ezbob] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [ezbob] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [ezbob] SET  DISABLE_BROKER 
GO
ALTER DATABASE [ezbob] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [ezbob] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [ezbob] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [ezbob] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [ezbob] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [ezbob] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [ezbob] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [ezbob] SET RECOVERY FULL 
GO
ALTER DATABASE [ezbob] SET  MULTI_USER 
GO
ALTER DATABASE [ezbob] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [ezbob] SET DB_CHAINING OFF 
GO
ALTER DATABASE [ezbob] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [ezbob] SET TARGET_RECOVERY_TIME = 0 SECONDS 
GO
USE [ezbob]
GO
/****** Object:  User [ezbobuser]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE USER [ezbobuser] WITHOUT LOGIN WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_owner] ADD MEMBER [ezbobuser]
GO
/****** Object:  UserDefinedTableType [dbo].[LoanIdListTable]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE TYPE [dbo].[LoanIdListTable] AS TABLE(
	[LoanID] [int] NOT NULL
)
GO






/****** Object:  StoredProcedure [dbo].[Cert_Thumbprint_By_UserName]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Cert_Thumbprint_By_UserName]
(
    @pLogin        nvarchar(30)
)
AS
BEGIN
    select  certificateThumbprint as thumbprint, domainUserName
    from security_user
    WHERE UserName=@pLogin AND isdeleted != 2;
END;

GO

/****** Object:  StoredProcedure [dbo].[ClearErrorForOtherMarketplaces]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[ClearErrorForOtherMarketplaces]
@MarketPlacesWithErrors VARCHAR(500),
@CustomerId INT
AS
BEGIN
	DECLARE @Delimiter VARCHAR(500),
		@TempValue VARCHAR(500),
        @TempIntValue INT

	SET @Delimiter = ','

	CREATE TABLE #tmp
	(
        id INT
	)

	SET @MarketPlacesWithErrors = isnull(@MarketPlacesWithErrors,'')

	DECLARE @TempString VARCHAR(500),
		@Ordinal INT,
		@CharIndex INT

	SET @TempString = @MarketPlacesWithErrors
	SET @CharIndex = charindex(@Delimiter, @TempString)
	SET @Ordinal = 0

	WHILE @CharIndex != 0 
	BEGIN     
		SET @Ordinal += 1        
		SET @TempIntValue = 0
		SET @TempValue = substring(@TempString, 0, @CharIndex)
		SET @TempIntValue = convert(INT, @TempValue) 

		IF @TempIntValue IS NOT NULL AND @TempIntValue != 0
		BEGIN
			INSERT INTO #tmp VALUES (@TempIntValue)
		END

		SET @TempString = substring(@TempString, @CharIndex + 1, len(@TempString) - @CharIndex)     
		SET @CharIndex = charindex(@Delimiter, @TempString)
	END

	IF @TempString != '' 
	BEGIN
		SET @Ordinal += 1 
		SET @TempIntValue = 0
		SET @TempIntValue = convert(INT, @TempString) 

		IF @TempIntValue IS NOT NULL AND @TempIntValue != 0
		BEGIN
			INSERT INTO #tmp VALUES (@TempIntValue)
		END
	END

	UPDATE MP_CustomerMarketPlace SET UpdateError = NULL WHERE CustomerId = @CustomerId AND Id NOT IN (SELECT Id FROM #tmp)
	DROP TABLE #tmp
END

GO
/****** Object:  StoredProcedure [dbo].[CONTROL_HISTORY_INSERT]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CONTROL_HISTORY_INSERT] 
(
    @pApplicationId  BIGINT,
    @pUserId         BIGINT,
    @pSecurityAppId  BIGINT,
    @pStrategyId     BIGINT,
    @pCurrentNodeId     BIGINT,
    @pCurrentNodePostfix NVARCHAR(MAX),
    @pDetailName     NVARCHAR(MAX),
    @pValue          NVARCHAR(MAX)
)
AS
BEGIN
   
    INSERT INTO control_history
      (
        applicationid,
        strategyid,
        userid,
        nodeid,
        currentnodepostfix,
        controlname,
        controlvalue,
        securityappid
      )
    VALUES
      (
        @pApplicationId,
        @pStrategyId,
        @pUserId,
        @pCurrentNodeId,
        @pCurrentNodePostfix,
        @pDetailName,
        @pValue,
        @pSecurityAppId
      );
END

GO



/****** Object:  StoredProcedure [dbo].[CustomerScoringResult_Insert]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CustomerScoringResult_Insert]
    @pCustomerId    int,
    @pAC_Parameters nvarchar(MAX),
    @AC_Descriptors nvarchar(MAX),
    @Result_Weight  nvarchar(MAX),
    @pResult_MAXPossiblePoints  nvarchar(MAX),
    @pMedal         nvarchar(20),
    @pScorePoints  [numeric](8, 3),
    @pScoreResult  [numeric](8, 3)
AS
BEGIN
    insert into CustomerScoringResult
    (
	CustomerId,
	AC_Parameters,
	AC_Descriptors,
	Result_Weights,
	Result_MAXPossiblePoints,
	Medal,
	ScorePoints,
	ScoreResult	
     )

  values
    (
     @pCustomerId,
     @pAC_Parameters,
     @AC_Descriptors,
     @Result_Weight,
     @pResult_MAXPossiblePoints,
     @pMedal,
     @pScorePoints,
     @pScoreResult
    )

END

GO
/****** Object:  StoredProcedure [dbo].[DataSource_GetDataSourceByName]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DataSource_GetDataSourceByName]
(
  @pDataSourceName nvarchar(max)
)
AS
BEGIN

  SELECT *
  FROM DataSource_Sources
  WHERE Name = @pDataSourceName
    AND (DataSource_Sources.IsDeleted IS NULL OR DataSource_Sources.IsDeleted = 0);

END

GO
/****** Object:  StoredProcedure [dbo].[DataSource_GetDataSourcesList]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DataSource_GetDataSourcesList]
(
  @pDataSourceType nvarchar(255)
)
AS
BEGIN
  select
   ds.ID,
   ds.NAME,
   ds.Description,
   ds.TYPE,
   ds.CREATIONDATE,
   ds.TERMINATIONDATE,
   ds.DISPLAYNAME,
   usr.username
  from datasource_sources ds 
  left outer join security_user usr on ds.userid = usr.userid
  WHERE ((ds.IsDeleted = 0) OR (ds.IsDeleted IS NULL))
   AND ds.Type=@pDataSourceType;
	

END

GO
/****** Object:  StoredProcedure [dbo].[DataSource_GetDSByDispName]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DataSource_GetDSByDispName]
(
  @pDisplayName nvarchar(max)
)
AS
BEGIN

  SELECT *
  FROM DataSource_Sources
  WHERE DisplayName = @pDisplayName
    AND (DataSource_Sources.IsDeleted IS NULL OR DataSource_Sources.IsDeleted = 0)
    AND DataSource_Sources.TerminationDate IS NULL;

END

GO
/****** Object:  StoredProcedure [dbo].[DataSource_GetDSByName]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DataSource_GetDSByName]
(
  @pDataSourceName nvarchar(max)
)
AS
BEGIN

  select * from datasource_sources
  where (isdeleted is null) AND DisplayName =  @pDataSourceName;

END

GO
/****** Object:  StoredProcedure [dbo].[DataSource_GetKeyNames]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DataSource_GetKeyNames]

AS
BEGIN
	SELECT * FROM [DataSource_KeyFields];
END

GO
/****** Object:  StoredProcedure [dbo].[DataSource_GetLinkedStr]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DataSource_GetLinkedStr]
(
  @pDataSourceName nvarchar(255)
)
AS
BEGIN

  SELECT [Strategy_Strategy].[StrategyId] as [ID], 
         [Strategy_Strategy].[displayname] as [Name]
  FROM [Strategy_Strategy], [DataSource_StrategyRel]
  WHERE [DataSource_StrategyRel].[DataSourceName] = @pDataSourceName
    AND [Strategy_Strategy].[StrategyId] = [DataSource_StrategyRel].[StrategyId]
    AND ([Strategy_Strategy].[IsDeleted] IS NULL OR [Strategy_Strategy].[IsDeleted] = 0);

END

GO
/****** Object:  StoredProcedure [dbo].[DataSource_InsertKeyName]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DataSource_InsertKeyName]
(
  @pKeyName nvarchar(max)
)
AS
BEGIN

  IF NOT EXISTS(SELECT KeyNameId FROM dbo.DataSource_KeyFields
  WHERE KeyName = @pKeyName)
  BEGIN
	INSERT INTO [dbo].[DataSource_KeyFields]([KeyName])
     VALUES(@pKeyName)
  END


END

GO
/****** Object:  StoredProcedure [dbo].[DataSource_InsertNew]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DataSource_InsertNew]
(
 @pDataSourceName nvarchar(max),
 @pDataSourceType nvarchar(max),
 @pDocument nvarchar(max),
 @pSignedData nvarchar(max),
 @pDisplayName nvarchar(max),
 @pDescription nvarchar(max),
 @pUserId int
)
AS
BEGIN

  DECLARE @id int;

  UPDATE DataSource_Sources
  SET [TerminationDate] = GETDATE()
  WHERE [TerminationDate] IS NULL and [DisplayName] = @pDisplayName;
  
  INSERT INTO DataSource_Sources
   ([Name],
    [Description],
    [Type],
    [Document],
    [SignedDocument],
    [CreationDate],
    [DisplayName],
    [UserId])
  values
   (@pDataSourceName,
    @pDescription,
    @pDataSourceType,
    @pDocument,
    @pSignedData,
    GETDATE(),
    @pDisplayName,
    @pUserId);

  set @id = @@IDENTITY;

  SELECT @id;
  RETURN @id;

END

GO
/****** Object:  StoredProcedure [dbo].[DataSource_SaveKey]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DataSource_SaveKey]
(
	@pRequestId bigint,
	@pKeyNameId bigint,
	@pKeyValue nvarchar(max)
)

AS
BEGIN
	INSERT INTO [dbo].[DataSource_KeyData]
           ([RequestId],[KeyNameId],[Value])
     VALUES
           (@pRequestId, @pKeyNameId, @pKeyValue)
END

GO
/****** Object:  StoredProcedure [dbo].[DataSource_SaveRequest]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DataSource_SaveRequest]
(
  @pAppId bigint,
  @pRequest nvarchar(max),
  @pReqId bigint out
)
AS
BEGIN
	INSERT INTO [dbo].[DataSource_Requests]
           ([ApplicationId],[Request])
     VALUES (@pAppId, @pRequest);
     SET @pReqId = @@IDENTITY;
END

GO
/****** Object:  StoredProcedure [dbo].[DataSource_SaveResponse]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DataSource_SaveResponse]
(
	@pReqId bigint,
	@pResponse nvarchar(max)
)

AS
BEGIN
	INSERT INTO [dbo].[DataSource_Responses]
           ([RequestId],[Response])
     VALUES
           (@pReqId, @pResponse);

END

GO
/****** Object:  StoredProcedure [dbo].[DataSource_Update]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DataSource_Update]
(
  @pId bigint,
  @pDescription nvarchar(max),
  @pUserId int,
  @pSignedData nvarchar(max),
  @pDocument nvarchar(max)
)
AS
BEGIN

  UPDATE DataSource_Sources
  SET
    [Description] = @pDescription,
    [UserId] = @pUserId,
    [Document] = @pDocument,
    [SIGNEDDOCUMENT] = @pSignedData
  WHERE [Id] = @pId;

END

GO
/****** Object:  StoredProcedure [dbo].[Delete_DS_Strategy_Rel]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Delete_DS_Strategy_Rel]
  @pStrategyId int
AS
BEGIN

  if @pStrategyId > 0
  begin
       DELETE FROM DataSource_StrategyRel WHERE StrategyId = @pStrategyId;
  end;

END

GO
/****** Object:  StoredProcedure [dbo].[DeleteCustomer]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DeleteCustomer]
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
/****** Object:  StoredProcedure [dbo].[DeleteDataSource]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DeleteDataSource]
  @pDataSourceId int,
  @pSignedData [nvarchar](max)
AS
BEGIN

  UPDATE DataSource_Sources
  SET 
     IsDeleted = Id,
     SignedDocumentDelete = @pSignedData
  WHERE DisplayName = (SELECT DisplayName FROM
  DataSource_Sources WHERE  Id = @pDataSourceId);

END

GO

/****** Object:  StoredProcedure [dbo].[DeleteOldPayPointBalanceData]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DeleteOldPayPointBalanceData]
@Date DATE
AS
BEGIN
	DELETE FROM
		PayPointBalance
	WHERE
		CONVERT(DATE, date) = @Date
	
	UPDATE LoanTransaction SET
		Reconciliation = 'not tested'
	WHERE
		CONVERT(DATE, PostDate) = @Date
		AND
		Type = 'PaypointTransaction'
END

GO

/****** Object:  StoredProcedure [dbo].[EKMGetNewShops]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[EKMGetNewShops]	
AS
BEGIN
	DECLARE @LastHandledId INT
	SELECT @LastHandledId = CONVERT(INT, CfgValue) FROM EkmConnectorConfigs WHERE CfgKey='LastHandledId'

	SELECT 
		CustomerId,
		Id ShopId,
		DisplayName,
		cast(SecurityData as varchar(max)) as SecurityData  
	FROM MP_CustomerMarketPlace 
	WHERE 
		MarketPlaceId = 4 AND 
		Id > @LastHandledId

END

GO
/****** Object:  StoredProcedure [dbo].[EKMGetShopByCustomerId]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[EKMGetShopByCustomerId]
@CustomerId INT 
AS
BEGIN
	SELECT * FROM MP_CustomerMarketPlace WHERE MarketPlaceId = 4 AND CustomerId=@CustomerId 
END


GO
/****** Object:  StoredProcedure [dbo].[EkmUpdateLastHandledId]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[EkmUpdateLastHandledId]
	@LastHandledId VARCHAR(100)
AS
BEGIN

	UPDATE EkmConnectorConfigs SET CfgValue=@LastHandledId WHERE CfgKey='LastHandledId'

END

GO

/****** Object:  StoredProcedure [dbo].[Export_DeleteNodeLinks]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Export_DeleteNodeLinks]
  (
    @pNodeId int
  )
AS
BEGIN
   delete from export_templatenoderel 
   where nodeid = @pNodeId;
END;

GO
/****** Object:  StoredProcedure [dbo].[Export_GetExportFile]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Export_GetExportFile]
(
       @fileId int
)
AS
BEGIN
   SELECT
          Export_Results.FileName
        , Export_Results.BinaryBody
        , Export_Results.ApplicationId
        , Export_Results.NodeName
        , Export_Results.SignedDocumentId
   FROM Export_Results
   WHERE export_results.id = @fileId;

END

GO
/****** Object:  StoredProcedure [dbo].[Export_GetNodeTemplateLinks]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Export_GetNodeTemplateLinks]
(
  @pTemplateId INT
)
AS
BEGIN
	select * from dbo.Export_TemplateNodeRel
	where TemplateId = @pTemplateId
END;

GO
/****** Object:  StoredProcedure [dbo].[Export_GetResultsByNodeName]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Export_GetResultsByNodeName]
(
       @pApplicationId int,
       @pNodeName nvarchar(max)
)
AS
BEGIN
   select export_results.id,
          export_results.filename,
          export_results.filetype,
          export_results.creationdate,
          export_results.applicationid,
          export_results.sourcetemplateid,
          export_results.status,
          export_results.nodename,
          export_results.SignedDocumentId
   from export_results
   where export_results.applicationid = @pApplicationId
     and export_results.nodename = @pNodeName;
END

GO
/****** Object:  StoredProcedure [dbo].[Export_GetTemplateById]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Export_GetTemplateById]
(
  @pTemplateId BIGINT
)

AS
BEGIN
       select id, uploaddate, exceptiontype,variablesxml, binarybody, signedDocument
       from export_templateslist
       where Id = @pTemplateId;
END;

GO
/****** Object:  StoredProcedure [dbo].[Export_GetTemplateLinks]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Export_GetTemplateLinks]
(
  @pNodeId INT,
  @pShowHistory INT
)

AS
BEGIN
       select id, displayname, terminationdate,
       (select outputtype from export_templatenoderel
       where templateid=id and nodeid = @pNodeId) as "outputtype"
       from export_templateslist
       where isdeleted is null
       and 
       ((@pShowHistory is null and terminationdate is null)
       or (@pShowHistory is not null))
       order BY displayname ASC,case when terminationdate is null then 0 else 1 end, terminationdate desc;

END;

GO
/****** Object:  StoredProcedure [dbo].[Export_GetTemplateVars]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Export_GetTemplateVars]
(
  @pTemplateId int
)

AS
BEGIN
       select variablesxml
       from export_templateslist
       where id = @pTemplateId;
END;

GO
/****** Object:  StoredProcedure [dbo].[Export_LinkStrategy]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Export_LinkStrategy]
  (
    @pTemplateName nvarchar(max),
    @pStrategyId int
  )
AS
BEGIN
	DECLARE @l_templateId int;

    select @l_templateId=id from  export_templateslist
      where upper(filename) = upper(@pTemplateName) and isdeleted is null;

   if NOT EXISTS(select templateid from  export_templatestratrel
    where templateid = @l_templateId and strategyid = @pStrategyId)
	BEGIN
       insert into export_templatestratrel
         (templateid, strategyid)
       values
         (@l_templateId, @pStrategyId);
	END
     
 
END

GO
/****** Object:  StoredProcedure [dbo].[Export_LinkTemplate]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Export_LinkTemplate]
  (
    @pTemplateId INT,
    @pNodeId INT,
    @pOutputType INT,
    @pIsLinked INT
  )
AS
BEGIN
   delete from export_templatenoderel
   where templateid = @pTemplateId and nodeid = @pNodeId;
   
   IF @pIsLinked is not null 
   BEGIN
	insert into export_templatenoderel
         (templateid, nodeid, outputtype)
       values
         (@pTemplateId, @pNodeId, @pOutputType);
   END

END;

GO
/****** Object:  StoredProcedure [dbo].[Export_UpdateStateMode]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Export_UpdateStateMode]
(
    @pApplicationId      INT,
    @pStatusMode         INT
)
AS
BEGIN
   	  update export_results
		 set statusmode = @pStatusMode
	   where applicationid = @pApplicationId;
END;

GO
/****** Object:  StoredProcedure [dbo].[Export_UpdateTemplate]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Export_UpdateTemplate]
(
    @pId             INT,
    @pFileName       NVARCHAR(MAX),
    @pDescription    NVARCHAR(MAX),
    @pExceptionType  INT,
    @pBinaryBody     VARBINARY(MAX),
    @pVariablesXml   NVARCHAR(MAX),
    @pUserId         INT,
    @pDisplayName    NVARCHAR(MAX),
    @pSignedDocument ntext
)
AS
BEGIN
    IF @pId IS NULL
    BEGIN
        IF NOT EXISTS(
               SELECT *
               FROM   Export_TemplatesList etl
               WHERE  UPPER(etl.[FileName]) = UPPER(@pFileName) AND etl.IsDeleted IS NULL
           )
        BEGIN
        	UPDATE export_templateslist SET terminationdate = GETDATE()
        	WHERE upper(displayname) = upper(@pDisplayName) and terminationdate is null;
        	
            INSERT INTO export_templateslist
              (
                FILENAME,
                DESCRIPTION,
                binarybody,
                variablesxml,
                exceptiontype,
                userId,
                DisplayName,
                SignedDocument
              )
            VALUES
              (
                @pFileName,
                @pDescription,
                @pBinaryBody,
                @pVariablesXml,
                @pExceptionType,
                @pUserId,
                @pDisplayName,
                @pSignedDocument
              );
              SELECT @@IDENTITY;
        END
        ELSE
        	RAISERROR( 'dublicated_name', 16, 1);
        	SELECT NULL;
        	RETURN;

    END
    ELSE
    BEGIN
        UPDATE export_templateslist
        SET    DESCRIPTION = @pDescription,
               exceptiontype = @pExceptionType
        WHERE  id = @pId;
	SELECT @pId;
    END
END;

GO


/****** Object:  StoredProcedure [dbo].[GetAvailableStrategyTypes]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetAvailableStrategyTypes]
AS
BEGIN
	SET NOCOUNT ON;
	SELECT DISTINCT StrategyType FROM Strategy_Strategy;
END

GO


/****** Object:  StoredProcedure [dbo].[GetDailyReport]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetDailyReport]
  (@date Datetime)
AS
BEGIN
	SET NOCOUNT ON;
	SELECT     ROW_NUMBER() OVER(ORDER BY Loan.RefNum) AS Id, query.Date,  Customer.Name as CustomerName,  query.st as Status, query.LoanRepayment AS Paid,
			   query.AmountDue + query.LoanRepayment AS Expected, Loan.Date AS OriginationDate, Loan.LoanAmount,  
			   Loan.Balance as LoanBalance, Loan.RefNum AS LoanRef

	FROM         (SELECT     Date, AmountDue, LoanRepayment, 'EarlyPaid' AS st, loanid
				   FROM          dbo.GetFullyEarlyPaid(@date) AS fullyEarlyPaid
				   UNION
				   SELECT     Date, AmountDue, LoanRepayment, 'Fully paid on time' AS st, loanid
				   FROM         dbo.GetFullyPaidOnTime(@date) AS fullyPaidOnTime
				   UNION
				   SELECT     Date, AmountDue, LoanRepayment, 'partialEarlyPaid' AS st, loanid
				   FROM         dbo.GetPartialEarlyPaid(@date) AS partialEarlyPaid
				   UNION
				   SELECT     Date, AmountDue, LoanRepayment, 'partial Paid On Time' AS st, loanid
				   FROM         dbo.GetPartialPaidOnTime(@date) AS partialPaidOnTime
				   UNION
				   SELECT     Date, AmountDue, LoanRepayment, 'Not Paid' AS st, loanid
				   FROM         dbo.GetNotPaid(@date) AS notPaid
				   UNION
				   SELECT     getdate(), LatePaymentsAmount, 0 , 'Late repayment' AS st, loanid
				   FROM       [dbo].[GetLatePayments] (@date) AS lateRepayment
				   
				   ) AS query 
				   
				   
	INNER JOIN Loan ON Loan.Id = query.loanid 
	INNER JOIN Customer ON Loan.CustomerId = Customer.Id

END

GO
/****** Object:  StoredProcedure [dbo].[GetEkmConnectorConfigs]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetEkmConnectorConfigs]
AS
BEGIN
	SELECT 
		CfgKey, 
		CfgValue 
	FROM 
		EkmConnectorConfigs	
END

GO


/****** Object:  StoredProcedure [dbo].[GetExperianData]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetExperianData]
	@FirstIdToHandle BIGINT, -- Will handle the one after this
	@LastIdToHandle BIGINT
AS
BEGIN
	SELECT 
		Id, 
		ServiceType, 
		InsertDate, 
		RequestData, 
		ResponseData, 
		CustomerId 
	FROM 
		MP_ServiceLog 
	WHERE 
		ServiceType = 'Consumer Request' AND
		Id > @FirstIdToHandle AND
		Id <= @LastIdToHandle
	ORDER BY
		Id ASC
END

GO
/****** Object:  StoredProcedure [dbo].[GetExperianDataAgentConfigs]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetExperianDataAgentConfigs]
AS
BEGIN
	SELECT 
		CfgKey, 
		CfgValue 
	FROM 
		ExperianDataAgentConfigs	
END

GO
/****** Object:  StoredProcedure [dbo].[GetExperianDefaultAccountId]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetExperianDefaultAccountId]
AS
BEGIN
	SELECT ExperianAccountStatuses.Id FROM ExperianAccountStatuses WHERE DetailedStatus = 'Default'
END

GO
/****** Object:  StoredProcedure [dbo].[GetExposurePerMedalReport]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetExposurePerMedalReport]
	@dateStart DateTime, 
	@dateEnd DateTime
AS
BEGIN

	SELECT dbo.Medals.Medal, ISNULL(pr.Processed, 0) as Processed, ISNULL(pr.ProcessedAmount, 0) as ProcessedAmount, 
			ISNULL(ag.Approved, 0) as Approved, ISNULL(ag.ApprovedAmount,0) as ApprovedAmount, 
			ISNULL(pr.Paid, 0) as Paid, ISNULL(pr.PaidAmount,0) as PaidAmount, 
			ISNULL(pr.Defaults, 0) as Defaults, ISNULL(pr.DefaultsAmount,0) as DefaultsAmount, 
			ISNULL(pr.Late30, 0) as Late30, ISNULL(pr.Late30Amount,0) as Late30Amount, 
			ISNULL(pr.Late60, 0) as Late60, ISNULL(pr.Late60Amount,0) as Late60Amount, 
			ISNULL(pr.Late90, 0) as Late90, ISNULL(pr.Late90Amount,0) as Late90Amount,
			ISNULL(pr.Exposure, 0) as Exposure, ISNULL(ol.OpenCreditLine, 0) as OpenCreditLine
			FROM 
			dbo.Medals LEFT OUTER JOIN  
		(SELECT UPPER(dbo.CashRequests.MedalType) as Medal, Count(dbo.CashRequests.SystemCalculatedSum) as Processed, Sum(dbo.CashRequests.SystemCalculatedSum) as ProcessedAmount,
		SUM(Paid) as Paid, SUM(PaidAmount) as PaidAmount, 
		SUM(Defaults) as Defaults, SUM(DefaultsAmount) as DefaultsAmount,
		SUM(Late30) as Late30, SUM(Late30Amount) as Late30Amount, 
		SUM(Late60) as Late60, SUM(Late60Amount) as Late60Amount, 
		SUM(Late90) as Late90, SUM(Late90Amount) as Late90Amount,
		SUM(Exposure) as Exposure
		FROM dbo.CashRequests LEFT OUTER JOIN GetLoanByRequestCashId() as lp ON dbo.CashRequests.Id = lp.RequestCashId
		Where dbo.CashRequests.SystemDecisionDate >= @dateStart
				AND dbo.CashRequests.UnderwriterDecisionDate <= @dateEnd
		GROUP BY UPPER(dbo.CashRequests.MedalType)) AS pr 
		ON UPPER (dbo.Medals.Medal) = UPPER(pr.Medal)
		LEFT OUTER JOIN GetApprovedGroupedByMedal(@dateStart, @dateEnd) AS ag ON pr.Medal = ag.Medal
		LEFT OUTER JOIN GetOpenCreditLineByMedal() AS ol ON UPPER(pr.Medal) = UPPER(ol.Medal)
END

GO
/****** Object:  StoredProcedure [dbo].[GetExposurePerUnderwriterReport]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetExposurePerUnderwriterReport]
	@dateStart DateTime, 
	@dateEnd DateTime
AS
BEGIN
	SELECT pr.IdUnderwriter, Security_User.FullName Underwriter, ISNULL(pr.Processed, 0) as Processed, ISNULL(pr.ProcessedAmount, 0) as ProcessedAmount, 
			ISNULL(ag.Approved, 0) as Approved, ISNULL(ag.ApprovedAmount,0) as ApprovedAmount, 
			ISNULL(pr.Paid, 0) as Paid, ISNULL(pr.PaidAmount,0) as PaidAmount, 
			ISNULL(pr.Defaults, 0) as Defaults, ISNULL(pr.DefaultsAmount,0) as DefaultsAmount, 
			ISNULL(pr.Late30, 0) as Late30, ISNULL(pr.Late30Amount,0) as Late30Amount, 
			ISNULL(pr.Late60, 0) as Late60, ISNULL(pr.Late60Amount,0) as Late60Amount, 
			ISNULL(pr.Late90, 0) as Late90, ISNULL(pr.Late90Amount,0) as Late90Amount,
			ISNULL(pr.Exposure, 0) as Exposure, ISNULL (ol.OpenCreditLine, 0) as OpenCreditLine
			FROM 
		(SELECT dbo.CashRequests.IdUnderwriter as IdUnderwriter, Count(dbo.CashRequests.SystemCalculatedSum) as Processed, Sum(dbo.CashRequests.SystemCalculatedSum) as ProcessedAmount,
		SUM(Paid) as Paid, SUM(PaidAmount) as PaidAmount, 
		SUM(Defaults) as Defaults, SUM(DefaultsAmount) as DefaultsAmount,
		SUM(Late30) as Late30, SUM(Late30Amount) as Late30Amount, 
		SUM(Late60) as Late60, SUM(Late60Amount) as Late60Amount, 
		SUM(Late90) as Late90, SUM(Late90Amount) as Late90Amount,
		SUM(Exposure) as Exposure
		FROM dbo.CashRequests LEFT OUTER JOIN GetLoanByRequestCashId() as lp ON dbo.CashRequests.Id = lp.RequestCashId
		Where dbo.CashRequests.SystemDecisionDate >= @dateStart
				AND dbo.CashRequests.UnderwriterDecisionDate <= @dateEnd
		GROUP BY dbo.CashRequests.IdUnderwriter) AS pr 
		LEFT OUTER JOIN GetApprovedGrouped(@dateStart, @dateEnd) AS ag ON pr.IdUnderwriter = ag.IdUnderwriter
		LEFT OUTER JOIN GetOpenCreditLineByUnderwriter() AS ol ON pr.IdUnderwriter = ol.IdUnderwriter
		LEFT OUTER JOIN Security_User ON pr.IdUnderwriter = Security_User.UserId
		
END

GO
/****** Object:  StoredProcedure [dbo].[GetFirstStepCustomers]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetFirstStepCustomers]
	@DateStart    DATETIME,
	@DateEnd      DATETIME
AS
BEGIN
if OBJECT_ID('tempdb..#Shops') is not NULL
BEGIN
                DROP TABLE #Shops
END
 
if OBJECT_ID('tempdb..#MaxOffer') is not NULL
BEGIN
                DROP TABLE #MaxOffer
END
 
if OBJECT_ID('tempdb..#SumOfLoans') is not NULL
BEGIN
                DROP TABLE #SumOfLoans
END
 
if OBJECT_ID('tempdb..#tmp1') is not NULL
BEGIN
                DROP TABLE #tmp1
END
 
if OBJECT_ID('tempdb..#AnualTurnover') is not NULL
BEGIN
                DROP TABLE #AnualTurnover
END

---- # OF SHOPS PER CUSTOMER ----
 
SELECT CustomerId,count(MarketPlaceId) AS Shops
INTO #Shops
FROM MP_CustomerMarketPlace
GROUP BY CustomerId
 
---- MAX OFFER THAT WAS OFFERED TO CUSTOMER ----
 
SELECT IdCustomer AS CustomerId,max(ManagerApprovedSum) MaxApproved
INTO #MaxOffer
FROM CashRequests
GROUP BY IdCustomer
               
---- SUM OF LOANS THAT CUST GOT -----
 
SELECT CustomerId,sum(LoanAmount) SumOfLoans
INTO #SumOfLoans
FROM Loan
GROUP BY CustomerId
 
----- CALC FOR ANUAL TURNOVER -----
SELECT IdCustomer AS CustomerId,max(CreationDate)AS CreationDate
INTO #tmp1
FROM CashRequests
GROUP BY IdCustomer
ORDER BY 1
 
SELECT A.CustomerId,A.creationdate,R.MedalType,R.AnualTurnover,R.ExpirianRating
INTO #AnualTurnover
FROM #tmp1 A
JOIN CashRequests R ON R.IdCustomer = A.CustomerId
WHERE R.CreationDate = A.CreationDate
 
--------------------------------------------------------------------------

SELECT x.eMail
FROM
(
 
SELECT
                   Customer.Id,
                   Customer.Name AS eMail,
                   Customer.GreetingMailSentDate AS DateRegister,
                   Customer.FirstName,
                   Customer.Surname SurName,
                   Customer.DaytimePhone,
                   Customer.MobilePhone,
                   #Shops.Shops,
                   #MaxOffer.MaxApproved,
                   #SumOfLoans.SumOfLoans,
                   T.AnualTurnover,
                   T.ExpirianRating,
                   T.MedalType,
                   CASE
                   WHEN T.MedalType='Silver' THEN ROUND(T.AnualTurnover*0.06,0)
                   WHEN T.MedalType='Gold' THEN ROUND(T.AnualTurnover*0.08,0)
                   WHEN T.MedalType='Platinum' THEN ROUND(T.AnualTurnover*0.10,0)
                   WHEN T.MedalType='Diamond' THEN ROUND(T.AnualTurnover*0.12,0)
                   END PhoneOffer
FROM   Customer
 
LEFT JOIN #Shops ON #Shops.CustomerId = Customer.Id
LEFT JOIN #MaxOffer ON #MaxOffer.CustomerId = Customer.Id
LEFT JOIN #SumOfLoans ON #SumOfLoans.CustomerId = Customer.Id
LEFT JOIN #AnualTurnover T ON T.CustomerId = Customer.Id
WHERE
                                Customer.IsTest = 0
                                AND Name NOT like '%ezbob%'
                                AND Name NOT LIKE '%liatvanir%'
                                AND Customer.GreetingMailSentDate >= @DateStart AND Customer.GreetingMailSentDate < @DateEnd
                               
 
GROUP BY Customer.Id,Customer.Name,Customer.FirstName,Customer.Surname,Customer.DaytimePhone,Customer.MobilePhone,
                                Customer.GreetingMailSentDate,#Shops.Shops,#MaxOffer.MaxApproved,#SumOfLoans.SumOfLoans,T.ExpirianRating,
                                T.AnualTurnover,T.MedalType
 ) x
 WHERE
 x.FirstName IS NULL AND x.SurName IS NULL AND x.Shops IS NULL
 
 END

GO

/****** Object:  StoredProcedure [dbo].[GetLastAutoresponderDate]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetLastAutoresponderDate]
	@Email NVARCHAR(300)
AS
BEGIN
SELECT TOP 1 DateOfAutoResponse FROM AutoresponderLog WHERE Email = @Email ORDER BY DateOfAutoResponse DESC 
END

GO
/****** Object:  StoredProcedure [dbo].[GetLastStepCustomers]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[GetLastStepCustomers]
	@DateStart    DATETIME,
	@DateEnd      DATETIME
AS
BEGIN
SELECT DISTINCT
 c.Name AS eMail,
 c.FirstName AS FirstName,
 c.Surname AS SurName,
 CASE WHEN cr.ManagerApprovedSum IS NOT NULL THEN cr.ManagerApprovedSum ELSE cr.SystemCalculatedSum END AS MaxApproved
FROM
 CashRequests cr
 INNER JOIN Customer c ON cr.IdCustomer = c.Id AND c.IsTest = 0
 LEFT JOIN (
  SELECT DISTINCT
   l.CustomerId
  FROM
   Loan l
   INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
  WHERE
   CONVERT(DATE, l.Date) >= @DateStart
 ) lt ON c.Id = lt.CustomerId
WHERE
 lt.CustomerId IS NULL
 AND
 (
  (cr.IdUnderwriter IS NOT NULL AND cr.UnderwriterDecision = 'Approved')
  OR
  (cr.IdUnderwriter IS NULL AND cr.SystemDecision = 'Approve')
 )
 AND
 (
  (cr.IdUnderwriter IS NOT NULL AND CONVERT(DATE, cr.UnderwriterDecisionDate) = @DateStart)
  OR
  (cr.IdUnderwriter IS NULL AND CONVERT(DATE, cr.SystemDecisionDate) = @DateStart)
 )

END

GO

/****** Object:  StoredProcedure [dbo].[GetMaxServiceLogId]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetMaxServiceLogId]
	@UpperBound BIGINT
AS
BEGIN
	DECLARE @MaxId BIGINT
	SELECT @MaxId = max(Id) FROM MP_ServiceLog
	SELECT max(Id) AS MaxIdInBounds, @MaxId AS MaxId FROM MP_ServiceLog WHERE Id <= @UpperBound
END

GO
/****** Object:  StoredProcedure [dbo].[GetMedalStatisticReport]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetMedalStatisticReport]
	@dateStart DateTime, 
	@dateEnd DateTime
AS
BEGIN
	select 
			Medals.Medal,
			ISNULL( EbayStoresCount, 0) EbayStoresCount, 
			ISNULL( EbayStoresAverage, 0) EbayStoresAverage, 
			ISNULL( PayPalStoresCount, 0) PayPalStoresCount, 
			ISNULL( PayPalStoresAverage, 0) PayPalStoresAverage, 
			ISNULL( AmazonStoresCount, 0) AmazonStoresCount, 
			ISNULL( AmazonStoresAverage, 0) AmazonStoresAverage, 
			ISNULL( ScorePointsMin, 0) ScorePointsMin,
			ISNULL( ScorePointsMax, 0) ScorePointsMax,
			ISNULL( ScorePointsAverage, 0) ScorePointsAverage,
			ISNULL( ExperianRatingMin, 0) ExperianRatingMin,
			ISNULL( ExperianRatingMax, 0) ExperianRatingMax,
			ISNULL( ExperianRatingAverage, 0) ExperianRatingAverage,
			ISNULL( AnualTurnoverMin, 0) AnualTurnoverMin,
			ISNULL( AnualTurnoverMax, 0) AnualTurnoverMax,
			ISNULL( AnualTurnoverAverage, 0) AnualTurnoverAverage,
			ISNULL( CustomersCount, 0) CustomersCount,
			ISNULL( AmazonReviews, 0) AmazonReviews,
			ISNULL( AmazonRating, 0) AmazonRating,
			ISNULL( EbayReviews, 0) EbayReviews,
			ISNULL( EbayRating, 0) EbayRating
	 from
	 Medals LEFT OUTER JOIN 
	(select Medal, 
			SUM(ebayStores) EbayStoresCount, 
			AVG(Cast(ebayStores as Float)) as EbayStoresAverage, 
			SUM(payPalStores) PayPalStoresCount, 
			AVG(Cast(payPalStores as float)) as PayPalStoresAverage, 
			SUM(amazonStores) AmazonStoresCount, 
			AVG(CAST(amazonStores as float))as AmazonStoresAverage, 
			MIN(ScorePoints) ScorePointsMin,
			MAX(ScorePoints) ScorePointsMax,
			AVG(ScorePoints) ScorePointsAverage,
			MIN(ExperianRating) ExperianRatingMin,
			MAX(ExperianRating) ExperianRatingMax,
			AVG(ExperianRating) ExperianRatingAverage,
			MIN(AnualTurnover) AnualTurnoverMin,
			MAX(AnualTurnover) AnualTurnoverMax,
			AVG(AnualTurnover) AnualTurnoverAverage,
			Count(IdCustomer) CustomersCount,
			AVG(AmazonReviews) AmazonReviews,
			AVG(AmazonRating) AmazonRating,
			AVG(EbayReviews) EbayReviews,
			AVG(EbayRating) EbayRating
	FROM
	(select UPPER(MedalType) Medal, c.IdCustomer, 
		ISNULL(es.StoresCount, 0) as ebayStores, 
		ISNULL(ps.StoresCount, 0) as payPalStores, 
		ISNULL(ams.StoresCount, 0) as amazonStores ,
		ISNULL(c.ScorePoints, 0) as ScorePoints, 
		ISNULL(c.ExperianRating, 0) as ExperianRating, 
		ISNULL(c.AnualTurnover, 0) as AnualTurnover,
		ISNULL(ar.Reviews, 0) as AmazonReviews,
		ISNULL(ar.Rating, 0) as AmazonRating,
		ISNULL(ar.Reviews, 0) as EbayReviews,
		ISNULL(er.Rating, 0) as EbayRating
		from 
	(SELECT DISTINCT CashRequests.MedalType, CashRequests.IdCustomer, CashRequests.ScorePoints, CashRequests.ExpirianRating as ExperianRating, CashRequests.AnualTurnover
		FROM CashRequests
		INNER JOIN 
		(SELECT MAX(CreationDate) CreationDate, IdCustomer FROM CashRequests
			WHERE CreationDate >= @dateStart
			AND CreationDate <= @dateEnd
			GROUP By (IdCustomer)) as cr ON cr.CreationDate = CashRequests.CreationDate AND cr.IdCustomer = CashRequests.IdCustomer
		WHERE MedalType IS NOT NULL
	) as c 
	LEFT OUTER JOIN (SELECT * FROM GETSTORESCOUNT(1, @dateStart, @dateEnd)) as es 
	ON c.IdCustomer = es.CustomerId	
	LEFT OUTER JOIN	(SELECT * FROM GETSTORESCOUNT(2, @dateStart, @dateEnd))as ams 
	ON c.IdCustomer = ams.CustomerId
	LEFT OUTER JOIN (SELECT * FROM GETSTORESCOUNT(3, @dateStart, @dateEnd))as ps 
	ON c.IdCustomer = ps.CustomerId
	LEFT OUTER JOIN (SELECT * FROM GetAmazonReviews() )as ar 
	ON c.IdCustomer = ar.CustomerId
	LEFT OUTER JOIN (SELECT * FROM GetEbayReviews() )as er 
	ON c.IdCustomer = er.CustomerId) as m
	GROUP BY Medal) as r
	ON UPPER(Medals.Medal) = UPPER(r.Medal)
END

GO

/****** Object:  StoredProcedure [dbo].[GetMonitoredSites]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetMonitoredSites]
AS
BEGIN
	SELECT 
		Site 
	FROM 
		MonitoredSites	
END

GO
/****** Object:  StoredProcedure [dbo].[GetMonthlyTurnoverPerCashRequest]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetMonthlyTurnoverPerCashRequest]
AS
BEGIN
DECLARE @CustomerId INT,
		@latestAggr DATETIME,
		@analysisFuncId INT,
		@mpName NVARCHAR(255),
		@mpId INT,
		@mpTypeId INT,
		@monthTimePeroidId INT,
		@TotalMonthlyTurnover NUMERIC(18,4),
		@AggrVal NUMERIC(18,4)
		
SELECT @monthTimePeroidId = Id FROM MP_AnalysisFunctionTimePeriod WHERE Name = '30'
		
SELECT DISTINCT IdCustomer AS CustumerId, convert(NUMERIC(18,4),0) AS MonthlyTurnover INTO #tmp FROM CashRequests

DECLARE customerCursor CURSOR FOR SELECT CustumerId FROM #tmp
OPEN customerCursor
FETCH NEXT FROM customerCursor INTO @CustomerId
WHILE @@FETCH_STATUS = 0
BEGIN
	SET @TotalMonthlyTurnover = 0
	DECLARE marketplaceCursor CURSOR FOR SELECT Id, MarketPlaceId FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerId
	OPEN marketplaceCursor
	FETCH NEXT FROM marketplaceCursor INTO @mpId, @mpTypeId
	WHILE @@FETCH_STATUS = 0
	BEGIN
		SELECT @mpName = Name FROM MP_MarketplaceType WHERE Id = @mpTypeId
		IF @mpName = 'Pay Pal'
		BEGIN
			SELECT @analysisFuncId = Id FROM MP_AnalyisisFunction WHERE MarketPlaceId=@mpTypeId AND Name='TotalNetInPayments'
		END
		ELSE
		BEGIN
			IF @mpName = 'PayPoint'
			BEGIN
				SELECT @analysisFuncId = Id FROM MP_AnalyisisFunction WHERE MarketPlaceId=@mpTypeId AND Name='SumOfAuthorisedOrders'
			END
			ELSE
			BEGIN
				SELECT @analysisFuncId = Id FROM MP_AnalyisisFunction WHERE MarketPlaceId=@mpTypeId AND Name='TotalSumOfOrders'
			END
		END
		
		SELECT @latestAggr = Max(Updated) FROM MP_AnalyisisFunctionValues WHERE AnalyisisFunctionId=@analysisFuncId AND CustomerMarketPlaceId=@mpId
		IF @latestAggr IS NOT NULL
		BEGIN
			SELECT @AggrVal = ValueFloat FROM MP_AnalyisisFunctionValues WHERE AnalyisisFunctionId=@analysisFuncId AND CustomerMarketPlaceId=@mpId AND Updated = @latestAggr AND AnalysisFunctionTimePeriodId = @monthTimePeroidId
			
			SET @TotalMonthlyTurnover = @TotalMonthlyTurnover + @AggrVal
		END
		
	
		FETCH NEXT FROM marketplaceCursor INTO @mpId, @mpTypeId
	END
	CLOSE marketplaceCursor
	DEALLOCATE marketplaceCursor 
	
	UPDATE #tmp SET MonthlyTurnover=@TotalMonthlyTurnover WHERE CustumerId = @CustomerId

	FETCH NEXT FROM customerCursor INTO @CustomerId
END
CLOSE customerCursor
DEALLOCATE customerCursor

SELECT CashRequests.Id,CashRequests.IdCustomer,CashRequests.IdUnderwriter,CashRequests.CreationDate,CashRequests.SystemDecision,CashRequests.UnderwriterDecision,CashRequests.SystemDecisionDate,CashRequests.UnderwriterDecisionDate,CashRequests.EscalatedDate,CashRequests.SystemCalculatedSum,CashRequests.ManagerApprovedSum,CashRequests.MedalType,CashRequests.EscalationReason,CashRequests.APR,CashRequests.RepaymentPeriod,CashRequests.ScorePoints,CashRequests.ExpirianRating,CashRequests.AnualTurnover,CashRequests.InterestRate,CashRequests.UseSetupFee,CashRequests.EmailSendingBanned,CashRequests.LoanTypeId,CashRequests.UnderwriterComment,CashRequests.HasLoans,CashRequests.LoanTemplate,CashRequests.IsLoanTypeSelectionAllowed,CashRequests.DiscountPlanId,CashRequests.LoanSourceID,CashRequests.OfferStart,CashRequests.OfferValidUntil,CashRequests.IsCustomerRepaymentPeriodSelectionAllowed, #tmp.MonthlyTurnover 
FROM CashRequests, #tmp
WHERE #tmp.CustumerId = CashRequests.IdCustomer

DROP TABLE #tmp

END
GO
/****** Object:  StoredProcedure [dbo].[GetNdx]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetNdx] @pNodeId  int
AS	
BEGIN
	SELECT node.[ndx], node.[signedDocument] FROM Strategy_Node node where node.NodeId = @pNodeId
    RETURN    
END

GO
/****** Object:  StoredProcedure [dbo].[GetNewCustomers]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetNewCustomers]
AS
BEGIN
	SELECT 
		Customer.Id, 
		Customer.Name 
	FROM 
		Customer,
		SupportAgentConfigs
	WHERE
		SupportAgentConfigs.CfgKey = 'MaxCustomerId' AND
		CONVERT(INT, SupportAgentConfigs.CfgValue) < Customer.Id
	ORDER BY
		Customer.Id ASC
END

GO
/****** Object:  StoredProcedure [dbo].[GetNodeFilesForRestoreAndSave]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetNodeFilesForRestoreAndSave]
(
       @pNodeId int
)
AS
BEGIN
     select sn.NodeId,
            sn.Name,
            sn.NDX
     from Strategy_node sn
     where sn.nodeid = @pNodeId
END

GO
/****** Object:  StoredProcedure [dbo].[GetPacnetAgentConfigs]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetPacnetAgentConfigs]
AS
BEGIN	
	SELECT 
		CfgKey, 
		CfgValue 
	FROM 
		PacnetAgentConfigs	
END

GO
/****** Object:  StoredProcedure [dbo].[GetPayPalExpensesDetails]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
* 2012-10-08 O.Zemskyi Removing duplicates
* 
*/

CREATE PROCEDURE [dbo].[GetPayPalExpensesDetails]
	@marketPlaceId int
AS
BEGIN
	CREATE TABLE #Incomes
(
	Payer NVARCHAR (255),
	M1 float, 
	M3 float,
	M6 float,
	M12 float,
	M15 float,
	M18 float,
	M24 float,
	M24Plus float
)

DECLARE @name VARCHAR (max)
DECLARE tp_Cursor CURSOR FOR 
select Payer from [GetBiggestExpensesPayPalTransactions]( @marketPlaceId)
OPEN tp_Cursor;
FETCH NEXT FROM tp_Cursor INTO @name;
WHILE @@FETCH_STATUS = 0
begin
	insert INTO #Incomes select * from [GetExpensesPayPalTransactionsByPayer]( @marketPlaceId, @name) 
FETCH NEXT FROM tp_Cursor INTO @name;
end
CLOSE tp_Cursor
DEALLOCATE tp_Cursor;

select NEWID() as Id, * from #Incomes
DROP TABLE #Incomes
END

GO
/****** Object:  StoredProcedure [dbo].[GetPayPalIncomeDetails]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
* 2012-10-08 O.Zemskyi Removing duplicates
* 
*/
CREATE PROCEDURE [dbo].[GetPayPalIncomeDetails]
	@marketPlaceId int
AS
BEGIN
	CREATE TABLE #Incomes
(
	Payer nvarchar(255),
	M1 float, 
	M3 float,
	M6 float,
	M12 float,
	M15 float,
	M18 float,
	M24 float,
	M24Plus float
)

DECLARE @name VARCHAR (max)
DECLARE tp_Cursor CURSOR FOR 
select Payer from [GetBiggestIncomePayPalTransactions]( @marketPlaceId)
OPEN tp_Cursor;
FETCH NEXT FROM tp_Cursor INTO @name;
WHILE @@FETCH_STATUS = 0
begin
	insert INTO #Incomes select * from [GetIncomePayPalTransactionsByPayer]( @marketPlaceId, @name) 
FETCH NEXT FROM tp_Cursor INTO @name;
end
CLOSE tp_Cursor
DEALLOCATE tp_Cursor;

select NEWID() as Id, * from #Incomes
DROP TABLE #Incomes
END

GO
/****** Object:  StoredProcedure [dbo].[GetPayPalTotalExpensesByMarketplace]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetPayPalTotalExpensesByMarketplace]
	@marketplaceId int
AS
BEGIN

	SELECT NEWID() as Id, * from GetTotalExpensesPayPalTransactions (@marketplaceId)
END

GO
/****** Object:  StoredProcedure [dbo].[GetPayPalTotalIncomeByMarketplace]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetPayPalTotalIncomeByMarketplace]
	@marketplaceId int
AS
BEGIN

	SELECT NEWID() as Id, * from GetTotalIncomePayPalTransactions (@marketplaceId)
END

GO
/****** Object:  StoredProcedure [dbo].[GetPayPalTotalTransactionsByMarketplace]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetPayPalTotalTransactionsByMarketplace]
	@marketplaceId int
AS
BEGIN

	SELECT NEWID() as Id, * from GetTotalTransactionsPayPalTransactions (@marketplaceId)
END

GO
/****** Object:  StoredProcedure [dbo].[GetPerformencePerMedalReport]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetPerformencePerMedalReport]
	@dateStart DateTime, 
	@dateEnd DateTime
AS
BEGIN
	SELECT dbo.Medals.Medal, ISNULL(pr.Processed, 0) as Processed, ISNULL(pr.ProcessedAmount, 0) as ProcessedAmount, 
			ISNULL(pr.MaxTime,0) as MaxTime, ISNULL(pr.AvgTime,0 ) as AvgTime,
			ISNULL(ag.Approved, 0) as Approved, ISNULL(ag.ApprovedAmount,0) as ApprovedAmount, 
			ISNULL(rg.Rejected, 0) as Rejected, ISNULL(rg.RejectedAmount,0) as RejectedAmount, 
			ISNULL(eg.Escalated, 0) as Escalated, ISNULL(eg.EscalatedAmount,0) as EscalatedAmount,
			ISNULL(hg.HighSide, 0) as HighSide, ISNULL(hg.HighSideAmount,0) as HighSideAmount,
			ISNULL(lg.LowSided, 0) as LowSide, ISNULL(lg.LowSideAmount,0) as LowSideAmount,
			ISNULL(pr.LatePayments, 0) as LatePayments, ISNULL(pr.LatePaymentsAmount,0) as LatePaymentsAmount
			FROM 
			dbo.Medals LEFT OUTER JOIN  
	(SELECT dbo.CashRequests.MedalType as Medal, Count(dbo.CashRequests.SystemCalculatedSum) Processed, Sum(dbo.CashRequests.SystemCalculatedSum) ProcessedAmount,
		Max(DateDiff(minute, dbo.CashRequests.SystemDecisionDate, dbo.CashRequests.UnderwriterDecisionDate)) MaxTime, Max(DateDiff(minute, dbo.CashRequests.SystemDecisionDate, dbo.CashRequests.UnderwriterDecisionDate)) AvgTime,
		COUNT(lp.LatePaymentsAmount) as LatePayments, SUM(lp.LatePaymentsAmount) as LatePaymentsAmount
		FROM dbo.CashRequests LEFT OUTER JOIN GetLoanLatePaymentsGrouped() as lp ON dbo.CashRequests.Id = lp.RequestCashId
		Where dbo.CashRequests.SystemDecisionDate >= @dateStart
				AND dbo.CashRequests.UnderwriterDecisionDate <= @dateEnd
		GROUP BY dbo.CashRequests.MedalType) AS pr 
		ON UPPER (dbo.Medals.Medal) = UPPER(pr.Medal)
		LEFT OUTER JOIN GetApprovedGroupedByMedal(@dateStart, @dateEnd) AS ag ON pr.Medal = ag.Medal
		LEFT OUTER JOIN GetRejectedGroupedByMedal(@dateStart, @dateEnd) AS rg ON pr.Medal = rg.Medal
		LEFT OUTER JOIN GetEscalatedGroupedByMedal(@dateStart, @dateEnd) AS eg ON pr.Medal = eg.Medal
		LEFT OUTER JOIN GetHighSideGroupedByMedal(@dateStart, @dateEnd) AS hg ON pr.Medal = hg.Medal
		LEFT OUTER JOIN GetLowSideGroupedByMedal(@dateStart, @dateEnd) AS lg ON pr.Medal = lg.Medal
END

GO
/****** Object:  StoredProcedure [dbo].[GetPerformencePerUnderwriterReport]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetPerformencePerUnderwriterReport]
	@dateStart DateTime, 
	@dateEnd DateTime
AS
BEGIN
	SELECT pr.IdUnderwriter, dbo.Security_User.FullName as Underwriter, ISNULL(pr.Processed, 0) as Processed, ISNULL(pr.ProcessedAmount, 0) as ProcessedAmount, 
			ISNULL(pr.MaxTime,0) as MaxTime, ISNULL(pr.AvgTime,0 ) as AvgTime,
			ISNULL(ag.Approved, 0) as Approved, ISNULL(ag.ApprovedAmount,0) as ApprovedAmount, 
			ISNULL(rg.Rejected, 0) as Rejected, ISNULL(rg.RejectedAmount,0) as RejectedAmount, 
			ISNULL(eg.Escalated, 0) as Escalated, ISNULL(eg.EscalatedAmount,0) as EscalatedAmount,
			ISNULL(hg.HighSide, 0) as HighSide, ISNULL(hg.HighSideAmount,0) as HighSideAmount,
			ISNULL(lg.LowSided, 0) as LowSide, ISNULL(lg.LowSideAmount,0) as LowSideAmount,
			ISNULL(pr.LatePayments, 0) as LatePayments, ISNULL(pr.LatePaymentsAmount,0) as LatePaymentsAmount
			FROM 
	(SELECT dbo.CashRequests.IdUnderwriter, Count(dbo.CashRequests.SystemCalculatedSum) Processed, Sum(dbo.CashRequests.SystemCalculatedSum) ProcessedAmount,
		Max(DateDiff(minute, dbo.CashRequests.SystemDecisionDate, dbo.CashRequests.UnderwriterDecisionDate)) MaxTime, Max(DateDiff(minute, dbo.CashRequests.SystemDecisionDate, dbo.CashRequests.UnderwriterDecisionDate)) AvgTime,
		COUNT(lp.LatePaymentsAmount) as LatePayments, SUM(lp.LatePaymentsAmount) as LatePaymentsAmount
		FROM dbo.CashRequests LEFT OUTER JOIN GetLoanLatePaymentsGrouped() as lp ON dbo.CashRequests.Id = lp.RequestCashId
		Where dbo.CashRequests.SystemDecisionDate >= @dateStart
				AND dbo.CashRequests.UnderwriterDecisionDate <= @dateEnd
				AND IdUnderwriter IS NOT NULL
		GROUP BY dbo.CashRequests.IdUnderwriter) AS pr 
		LEFT OUTER JOIN GetApprovedGrouped(@dateStart, @dateEnd) AS ag ON pr.IdUnderwriter = ag.IdUnderwriter
		LEFT OUTER JOIN GetRejectedGrouped(@dateStart, @dateEnd) AS rg ON pr.IdUnderwriter = rg.IdUnderwriter
		LEFT OUTER JOIN GetEscalatedGrouped(@dateStart, @dateEnd) AS eg ON pr.IdUnderwriter = eg.IdUnderwriter
		LEFT OUTER JOIN GetHighSideGrouped(@dateStart, @dateEnd) AS hg ON pr.IdUnderwriter = hg.IdUnderwriter
		LEFT OUTER JOIN GetLowSideGrouped(@dateStart, @dateEnd) AS lg ON pr.IdUnderwriter = lg.IdUnderwriter
		LEFT OUTER JOIN dbo.Security_User ON pr.IdUnderwriter = dbo.Security_User.UserId
END

GO


/****** Object:  StoredProcedure [dbo].[GetSecirityRolesByUserId]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetSecirityRolesByUserId]
	@iUserId bigint
AS
BEGIN
	select r.RoleId, r.Name, r.Description
	from Security_Role r
	left join Security_UserRoleRelation rr on rr.RoleId = r.RoleId
	where rr.UserId = @iUserId
	
END

GO
/****** Object:  StoredProcedure [dbo].[GetSecondStepCustomers]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetSecondStepCustomers]
	@DateStart    DATETIME,
	@DateEnd      DATETIME
	
AS
BEGIN
--DECLARE 	@DateStart    DATETIME= '2013-02-14'
--DECLARE 	@DateEnd      DATETIME= '2013-02-18'
			

if OBJECT_ID('tempdb..#SumOfOrdersId') is not NULL 
BEGIN
	DROP TABLE #SumOfOrdersId
END

if OBJECT_ID('tempdb..#Shops') is not NULL
BEGIN
	DROP TABLE #Shops
END

if OBJECT_ID('tempdb..#Paypal') is not NULL
BEGIN
	DROP TABLE #Paypal
END
 
if OBJECT_ID('tempdb..#MP_Stores') is not NULL 
BEGIN
	DROP TABLE #MP_Stores
END

if OBJECT_ID('tempdb..#temp1') is not NULL 
BEGIN
	DROP TABLE #temp1
END

if OBJECT_ID('tempdb..#temp2') is not NULL 
BEGIN
	DROP TABLE #temp2
END

if OBJECT_ID('tempdb..#temp3') is not NULL 
BEGIN
	DROP TABLE #temp3
END

---- SumOfOrders ID ----
SELECT Id 
INTO #SumOfOrdersId
FROM MP_AnalyisisFunction 
WHERE Name = 'TotalSumOfOrders'


---- # OF SHOPS PER CUSTOMER ----
DECLARE @eBayId 	INT,
		@AmazonId 	INT,
		@PaypalId 	INT;

SET @eBayId   = (SELECT Id FROM MP_MarketplaceType WHERE Id = 1 );
SET @AmazonId = (SELECT Id FROM MP_MarketplaceType WHERE Id = 2 );
SET @PaypalId = (SELECT Id FROM MP_MarketplaceType WHERE Id = 3 );


SELECT CustomerId,count(MarketPlaceId) AS Shops
INTO #Shops
FROM MP_CustomerMarketPlace
WHERE MarketPlaceId IN (@eBayId,@AmazonId)
GROUP BY CustomerId

---- PAYPAL ACCOUNT PER CUSTOMER ----
SELECT CustomerId,count(MarketPlaceId) AS Paypal
INTO #Paypal
FROM MP_CustomerMarketPlace
WHERE MarketPlaceId IN (@PaypalId)
GROUP BY CustomerId


---- TEMP FOLDER WITH STORES ANUAL SALES ----
DECLARE @SalesPeriod INT;
		
SET @SalesPeriod = (SELECT Id FROM MP_AnalysisFunctionTimePeriod WHERE Id = 4 );

SELECT A.CustomerMarketPlaceId,max(A.Updated) UpdatedDate
INTO #MP_Stores
FROM MP_AnalyisisFunctionValues A, #SumOfOrdersId B
WHERE A.AnalyisisFunctionId = B.Id
	AND A.AnalysisFunctionTimePeriodId = @SalesPeriod
GROUP BY A.CustomerMarketPlaceId


---- TEMP FOLDER WITH STORES ANUAL SALES INC CUSTOMER ID ----
SELECT A.CustomerMarketPlaceId,A.UpdatedDate,round(B.ValueFloat,1) AS AnualSales,C.CustomerId
INTO #temp1
FROM #MP_Stores A,
	 MP_AnalyisisFunctionValues B,
	 MP_CustomerMarketPlace C,
	 #SumOfOrdersId D
WHERE 	A.CustomerMarketPlaceId = b.CustomerMarketPlaceId
		AND A.UpdatedDate=b.Updated
		AND A.CustomerMarketPlaceId = C.Id
	    AND B.AnalyisisFunctionId = D.Id
		AND B.AnalysisFunctionTimePeriodId = 4
ORDER BY 1

---- JOIN TEMP1 WITH CUSTOMER TABLE ----
SELECT  C.Id,C.Name AS EmailAddress,C.FirstName,C.Surname,A.CustomerMarketPlaceId,A.UpdatedDate,A.AnualSales,C.WizardStep
INTO #temp2
FROM #temp1 A
JOIN Customer C ON C.Id = A.CustomerId
WHERE C.Name NOT like '%ezbob%' 
  AND C.Name NOT LIKE '%liatvanir%'
  AND C.istest!=1
GROUP BY A.CustomerMarketPlaceId,A.UpdatedDate,A.AnualSales,C.Id,C.Name,
		 C.FirstName,C.Surname,C.WizardStep
ORDER BY 1

---- TEMP TABLE WITH CUSTOMER DETAILS ----
SELECT C.Id,C.Name,C.GreetingMailSentDate,C.FirstName,C.Surname,C.WizardStep
INTO #temp3
FROM Customer C
WHERE C.Name NOT like '%ezbob%' 
  AND C.Name NOT LIKE '%liatvanir%'
  AND C.istest!=1

---- FINAL TABLE WITH CUSTID, STORES & ANUAL SALES ----
SELECT A.Name AS eMail,
	   --A.Id AS CustomerId, C.Shops AS NumOfStores,  CASE WHEN D.Paypal >= 1 THEN 'Y' ELSE 'N' END AS HasPaypal, Sum(B.AnualSales) AS AnualSales,  Sum(B.AnualSales)*0.06 AS ApproximateLoanOfferNotRounded,
	   ROUND((Sum(B.AnualSales)*0.06)/100, 0) * 100 AS ApproximateLoanOffer
	   
FROM #temp3 A
LEFT JOIN #temp2 B ON A.Id = B.Id
LEFT JOIN #Shops C ON C.CustomerId = A.Id
LEFT JOIN #Paypal D ON D.CustomerId = A.Id
WHERE A.GreetingMailSentDate >= @DateStart 
	  AND A.GreetingMailSentDate < @DateEnd AND A.FirstName IS NULL AND A.Surname IS NULL AND B.AnualSales > 8000
GROUP BY A.Id,C.Shops,D.Paypal, A.Name, A.FirstName, A.Surname

END

GO

/****** Object:  StoredProcedure [dbo].[GetSupportAgentConfigs]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetSupportAgentConfigs]
AS
BEGIN
	SELECT 
		CfgKey, 
		CfgValue 
	FROM 
		SupportAgentConfigs	
END

GO

/****** Object:  StoredProcedure [dbo].[GetUserFrom]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetUserFrom]
(
  @iIsDeleted int,
  @iUserRoleId int
)
AS
BEGIN
	if ( @iUserRoleId <= -1)
		begin
			if (@iIsDeleted > -1)
				begin	
					select
						su.userid
						,su.username
						,su.fullname
						,su.EMail
					from
						security_user su where su.isdeleted = @iIsDeleted;
				end;
			else
				begin
					select
						su.userid
						,su.username
						,su.fullname
						,su.EMail
					from
						security_user su;
				end;
		end;
	else
		begin
		if (@iIsDeleted > -1)
				begin	
					select
						su.userid
						,su.username
						,su.fullname
						,su.EMail
					from
						security_user su 
						left join Security_UserRoleRelation surr on su.UserId = surr.UserId
					where 
						su.isdeleted = @iIsDeleted and
						surr.RoleId = @iUserRoleId
				end;
			else
				begin
					select
						su.userid
						,su.username
						,su.fullname
						,su.EMail
					from
						security_user su
						left join Security_UserRoleRelation surr on su.UserId = surr.UserId
					where 						
						surr.RoleId = @iUserRoleId
				end;
		end;
   
END;

GO
/****** Object:  StoredProcedure [dbo].[GetUserTo]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetUserTo]
(
  @iUserId int,
  @iIsDeleted int,
  @iUserRoleId int
)
AS
BEGIN
	if ( @iUserRoleId <= -1)
		begin
			if (@iIsDeleted > -1)
				begin
					select
						su.userid
						,su.username
						,su.fullname
						,su.EMail
					from
						security_user su 
					where 
						su.isdeleted = @iIsDeleted and su.userid != @iUserId;
				end;
			else
				begin
					select
						su.userid
						,su.username
						,su.fullname
						,su.EMail
					from
						security_user su 
					where 
						su.userid != @iUserId;
				end;
		end;
	else
		begin
			if (@iIsDeleted > -1)
				begin
					select
						su.userid
						,su.username
						,su.fullname
						,su.EMail
					from
						security_user su 
						left join Security_UserRoleRelation surr on su.UserId = surr.UserId
					where 
						su.isdeleted = @iIsDeleted and 
						su.userid != @iUserId and
						surr.RoleId = @iUserRoleId
				end; 
			else
				begin
					select
						su.userid
						,su.username
						,su.fullname
						,su.EMail
					from
						security_user su 
						left join Security_UserRoleRelation surr on su.UserId = surr.UserId
					where 
						su.userid != @iUserId and
						surr.RoleId = @iUserRoleId;
			end;
		end;			
END;

GO
/****** Object:  StoredProcedure [dbo].[Greeting_Mail_Sent]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Greeting_Mail_Sent] 
(@UserId int,
@GreetingMailSent int)



AS
BEGIN
declare @GreetingMailSentDate datetime  

set @GreetingMailSentDate = GETUTCDATE()

UPDATE [dbo].[Customer]
   SET [GreetingMailSent] = @GreetingMailSent, [GreetingMailSentDate] = @GreetingMailSentDate
 WHERE Id = @UserId

SET NOCOUNT ON;
SELECT @@IDENTITY;
END

GO
/****** Object:  StoredProcedure [dbo].[Insert_DS_Strategy_Rel]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Insert_DS_Strategy_Rel]
(
    @pDataSourceName nvarchar(255),
    @pStrategyId int
)
AS
BEGIN
  DECLARE @newId int;
  set @newId = null

  SELECT @newId = Id 
  FROM DataSource_StrategyRel 
    WHERE DataSourceName = @pDataSourceName 
	  AND StrategyId = @pStrategyId;

  if @newId is null
  begin
  	insert into DATASOURCE_STRATEGYREL
	(DataSourceName, StrategyId)
  	values
  	(@pDataSourceName, @pStrategyId);
  end;

END

GO
/****** Object:  StoredProcedure [dbo].[Insert_Security_Role]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Insert_Security_Role]
       @pName nvarchar(255),
       @pDescription nvarchar(50),
       @pNewRoleId int OUTPUT
AS
BEGIN
     SET NOCOUNT ON;
     INSERT INTO Security_Role (Name, Description) 
     VALUES (@pName, @pDescription);

   SELECT @pNewRoleId = SCOPE_IDENTITY() 

END

GO
/****** Object:  StoredProcedure [dbo].[InsertAutoresponderLog]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[InsertAutoresponderLog]
	@Email NVARCHAR(300),
	@Name NVARCHAR(300)
AS
BEGIN
INSERT INTO AutoresponderLog(Email, Name, DateOfAutoResponse) VALUES (@Email, @Name, getdate())
END

GO
/****** Object:  StoredProcedure [dbo].[InsertExperianAccount]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[InsertExperianAccount]
	@ServiceLogId BIGINT,
	@CustomerId BIGINT,
	@AccountType VARCHAR(100),
	@DefMonth DATETIME,
	@Balance INT,
	@CurrentDefBalance INT
AS
BEGIN
	INSERT INTO ExperianDefaultAccountsData (ServiceLogId, CustomerId, AccountType, DefMonth, Balance, CurrentDefBalance) 
	VALUES (@ServiceLogId, @CustomerId, @AccountType, @DefMonth, @Balance, @CurrentDefBalance)
END

GO
/****** Object:  StoredProcedure [dbo].[InsertPacNetBalance]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[InsertPacNetBalance]
  @Date		             DATETIME
 ,@Amount                FLOAT
 ,@Fees				     FLOAT
 ,@CurrentBalance        FLOAT
 ,@IsCredit              BIT = 0
AS
BEGIN	
	INSERT INTO dbo.PacNetBalance ( Date, Amount ,Fees ,CurrentBalance, IsCredit )
	VALUES(	@Date, @Amount, @Fees, @CurrentBalance, @IsCredit )
END

GO
/****** Object:  StoredProcedure [dbo].[InsertPayPointData]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[InsertPayPointData]
@_class      VARCHAR(300),
@acquirer    VARCHAR(300),
@amount      DECIMAL(18, 2),
@auth_code   VARCHAR(300),
@authorised  VARCHAR(300),
@card_type   VARCHAR(300),
@cid         VARCHAR(300),
@company_no  VARCHAR(300),
@country     VARCHAR(300),
@currency    VARCHAR(300),
@cv2avs      VARCHAR(300),
@date        DATETIME,
@deferred    VARCHAR(300),
@emvValue    VARCHAR(300),
@fraud_code  VARCHAR(300),
@FraudScore  VARCHAR(300),
@ip          VARCHAR(300),
@lastfive    VARCHAR(300),
@merchant_no VARCHAR(300),
@message     VARCHAR(300),
@MessageType VARCHAR(300),
@mid         VARCHAR(300),
@name        VARCHAR(300),
@options     VARCHAR(300),
@status      VARCHAR(300),
@tid         VARCHAR(300),
@trans_id    VARCHAR(300)
AS
	INSERT INTO PayPointBalance (
		acquirer, amount, auth_code, authorised, card_type, cid,
		_class, company_no, country, currency, cv2avs, date,
		deferred, emvValue, fraud_code, FraudScore,
		ip, lastfive, merchant_no, message, MessageType, mid,
		name, options, status, tid, trans_id
	)
	VALUES (
		@acquirer, @amount, @auth_code, @authorised, @card_type, @cid,
		@_class, @company_no, @country, @currency, @cv2avs, @date,
		@deferred, @emvValue, @fraud_code, @FraudScore,
		@ip, @lastfive, @merchant_no, @message, @MessageType, @mid,
		@name, @options, @status, @tid, @trans_id
	)

GO
/****** Object:  StoredProcedure [dbo].[InsertSiteAnalytics]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[InsertSiteAnalytics]
@Date DATETIME,
@CodeName NVARCHAR(300),
@Value INT
AS
BEGIN	
	IF NOT EXISTS (SELECT Id FROM SiteAnalyticsCodes WHERE Name = @CodeName)
		INSERT INTO SiteAnalyticsCodes (Name, Description)
			Values (@CodeName, '@' + @CodeName)

	INSERT INTO SiteAnalytics ([Date], SiteAnalyticsCode, SiteAnalyticsValue)
	SELECT
		@Date,
		c.Id,
		@Value
	FROM
		SiteAnalyticsCodes c
	WHERE
		c.Name = @CodeName
END

GO
/****** Object:  StoredProcedure [dbo].[InsertStrategyCustomerUpdateTime]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[InsertStrategyCustomerUpdateTime] 
(@CustomerId int,
 @StartDate datetime,
 @EndDate datetime)
      
           
AS
BEGIN

insert INTO  [dbo].[Strategy_CustomerUpdateHistory] (CustomerId, StartDate, EndDate)
     VALUES (@CustomerId, @StartDate, @EndDate);

SET NOCOUNT ON;
SELECT @@IDENTITY;
END

GO
/****** Object:  StoredProcedure [dbo].[InsertStrategyMarketPlaceUpdateTime]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[InsertStrategyMarketPlaceUpdateTime] 
(@MarketPlaceId int,
 @StartDate datetime,
 @EndDate datetime)
      
           
AS
BEGIN

insert INTO  [dbo].[Strategy_MarketPlaceUpdateHistory] (MarketPlaceId, StartDate, EndDate)
     VALUES (@MarketPlaceId, @StartDate, @EndDate);

SET NOCOUNT ON;
SELECT @@IDENTITY;
END

GO
/****** Object:  StoredProcedure [dbo].[LoadPayPointBalanceColumns]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[LoadPayPointBalanceColumns]
AS
	SELECT
		name
	FROM
		syscolumns
	WHERE
		id = OBJECT_ID('PayPointBalance')
		AND
		name != 'Id'
	ORDER BY
		name

GO
/****** Object:  StoredProcedure [dbo].[LockBegin]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[LockBegin]
	@pLockType nvarchar(100),
	@pLockTimeout int
AS
BEGIN
  DECLARE @lLockId bigint,
  @lLastUpdateTime datetime,
  @lTimeoutPeriod int,
  @lLocked int;

  SELECT
    @lLockId = [Id],
    @lLastUpdateTime = [LastUpdateTime],
    @lTimeoutPeriod = [TimeoutPeriod]
  FROM LockedObject WITH (ROWLOCK)
  WHERE [Type] = @pLockType;

  SET @lLocked = -1;
  IF NOT(@lLockId is NULL)
    IF(cast(DATEDIFF(second, DATEADD(second, @lTimeoutPeriod, @lLastUpdateTime), GETDATE()) as bigint) <= 0)
      SET @lLocked = 1;
    ELSE
      SET @lLocked = 0;

  IF (@lLocked = 1)
    SELECT -1;
  ELSE
    BEGIN
      IF @lLocked = 0
        DELETE FROM [LockedObject]
        WHERE [Type] = @pLockType;

      INSERT INTO [LockedObject]
          ([Type]
          ,[TimeoutPeriod]
          ,[LastUpdateTime])
      VALUES
          (@pLockType
          ,@pLockTimeout
          ,GETDATE())
      SELECT @@IDENTITY;
    END;
END

GO
/****** Object:  StoredProcedure [dbo].[LockCheckStatus]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[LockCheckStatus]
	@pLockId BIGINT
AS
BEGIN
  DECLARE @lLockId bigint;

  SELECT
    @lLockId = [Id]
  FROM LockedObject WITH (ROWLOCK)
  WHERE [Id] = @pLockId;

  IF @lLockId is NULL
    SELECT 0;
  ELSE
    BEGIN
      UPDATE [LockedObject] WITH (ROWLOCK)
      SET [LastUpdateTime] = GETDATE()
      WHERE [Id] = @pLockId;

      SELECT 1;
    END;
END

GO
/****** Object:  StoredProcedure [dbo].[LockRelease]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[LockRelease]
	@pLockId BIGINT
AS
BEGIN
  DELETE FROM [LockedObject]
  WHERE [Id] = @pLockId;
END

GO
/****** Object:  StoredProcedure [dbo].[Log_ServerAction_Insert]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[Log_ServerAction_Insert]
	-- Add the parameters for the stored procedure here
	@pCommand nvarchar(255),
    @pRequest ntext,
    @pApplicationId int,
    @pUserHost nvarchar(255),
    @pid bigint output
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	INSERT INTO Log_ServiceAction (Command, Request, ApplicationId, UserHost) 
	VALUES (@pCommand, @pRequest, @pApplicationId, @pUserHost)
    -- Insert statements for procedure here
	SELECT @pid = SCOPE_IDENTITY()
END

GO
/****** Object:  StoredProcedure [dbo].[Log_TraceLogInsert]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[Log_TraceLogInsert]
   @pApplicationId int, 
   @pType int, 
   @pCode int, 
   @pMessage ntext, 
   @pData ntext, 
   @pInsertDate datetime, 
   @pThreadId int 
as
begin
    INSERT INTO Log_TraceLog (
                              ApplicationId, Type, Code, 
                              Message, Data, InsertDate, ThreadId
                              ) 
    VALUES (@pApplicationId, @pType, @pCode, 
            @pMessage, @pData, @pInsertDate, @pThreadId);
            
    
end

GO
/****** Object:  StoredProcedure [dbo].[MC_AddCampaignClickStat]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MC_AddCampaignClickStat]
	@Title    	 NVARCHAR(300)
   ,@Url         NVARCHAR(300)
   ,@Email       NVARCHAR(300)
   ,@EmailsSent  INT
   ,@Clicks      INT
   ,@Date        DATETIME
AS
BEGIN
INSERT INTO MC_CampaignClicks (Date, Title, Url ,EmailsSent ,Clicks ,Email) VALUES (@Date, @Title, @Url ,@EmailsSent ,@Clicks ,@Email)
END

GO
/****** Object:  StoredProcedure [dbo].[MP_GeAnnualSalesAmazon]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MP_GeAnnualSalesAmazon]
		@iCustomerMarketPlaceId int
AS
BEGIN


select SUM(TotalTable3.Price * TotalTable3.Quantity / TotalTable3.Rate) 
 from 
   (
    select aoi8.OrderTotal as Price,
           aoi8.NumberOfItemsShipped as Quantity, 
          (select TableRate.Rate from dbo.MP_GetCurrencyRate1(aoi8.OrderTotalCurrency, aoi8.PurchaseDate) TableRate) as Rate
     from  MP_AmazonOrder ao8
    left join MP_AmazonOrderItem2 aoi8 on aoi8.AmazonOrderId = ao8.Id 
    left join MP_CustomerMarketPlace cmp1 ON  ao8.CustomerMarketPlaceId = cmp1.Id
    where aoi8.OrderStatus LIKE 'Shipped' and
		cmp1.Id= @iCustomerMarketPlaceId

          AND DATEADD(MONTH, 12, aoi8.PurchaseDate ) >= GETDATE()
  )as TotalTable3


END

GO
/****** Object:  StoredProcedure [dbo].[MP_GeAnnualSalesEBay]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MP_GeAnnualSalesEBay]
	@iCustomerMarketPlaceId int

AS
BEGIN
	
select SUM(TotalTable12.Price * TotalTable12.Quantity / TotalTable12.Rate) 
 from 
  (
    select et8.price as Price,
           et8.QuantityPurchased as Quantity, 
          (select TableRate.Rate from dbo.MP_GetCurrencyRate1(eoi8.AmountPaidCurrency, eoi8.CreatedTime) TableRate) as Rate
   from MP_EbayOrderItem eoi8
   left join MP_EbayOrder eo8 on eoi8.OrderId = eo8.Id
   left join MP_EbayTransaction et8 on et8.OrderItemId = eoi8.Id
   left join MP_CustomerMarketPlace cmp ON eo8.CustomerMarketPlaceId = cmp.Id
   where (eoi8.OrderStatus LIKE 'Completed' 
    OR eoi8.OrderStatus LIKE 'Authenticated'
    OR eoi8.OrderStatus LIKE 'Shipped') and
    cmp.Id= @iCustomerMarketPlaceId
     AND DATEADD(MONTH, 12, eoi8.CreatedTime ) >= GETDATE()
  ) as TotalTable12



END

GO
/****** Object:  StoredProcedure [dbo].[MP_GetAllCustomerMPUMI]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MP_GetAllCustomerMPUMI]
(
	@CustomerId  int,
	@iNumberOfMarcketPlaces int output,
	@iCustomerMarketPlaceId	int output

)
AS
BEGIN

   
	SELECT @iNumberOfMarcketPlaces = COUNT(cmp.Id)
	FROM MP_CustomerMarketPlace cmp
		LEFT JOIN Customer c ON cmp.CustomerId = c.Id
	WHERE c.Id = @CustomerId
	


	SELECT cmp.Id
	FROM MP_CustomerMarketPlace cmp
		LEFT JOIN Customer c ON cmp.CustomerId = c.Id
	WHERE c.Id = @CustomerId
	
		


END

GO
/****** Object:  StoredProcedure [dbo].[MP_GetAmazonFeedback]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MP_GetAmazonFeedback]
	@iCustomerMarketPlaceId int



	
AS
BEGIN

select Negative, Positive, Neutral 

from MP_CustomerMarketPlace cmp
LEFT JOIN MP_EbayFeedback fb ON fb.CustomerMarketPlaceId=@iCustomerMarketPlaceId
          and fb.Created = (select MAX(fb1.Created)
          from MP_EbayFeedback fb1
          where fb1.CustomerMarketPlaceId = fb.CustomerMarketPlaceId)  
  LEFT JOIN MP_EbayFeedbackItem fbi on fbi.EbayFeedbackId = fb.Id and fbi.TimePeriodId = 4


WHERE cmp.MarketPlaceId=2

END

GO
/****** Object:  StoredProcedure [dbo].[MP_GetAmazonItemsOrdered]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MP_GetAmazonItemsOrdered]
	@iCustomerMarketPlaceId int,
	@iMonthsShiftNumber int
AS
BEGIN

DECLARE @iOrdersNumber int;
--	
select @iOrdersNumber = SUM(i.NumberOfItemsShipped)
from MP_AmazonOrderItem2 i
left join MP_AmazonOrder o on i.AmazonOrderId = o.Id
where i.OrderStatus LIKE 'Shipped'
   AND o.CustomerMarketPlaceId = @iCustomerMarketPlaceId

   AND DATEADD(MONTH, @iMonthsShiftNumber, i.PurchaseDate ) >= GETDATE()

return @iOrdersNumber;
	
END

GO
/****** Object:  StoredProcedure [dbo].[MP_GetAmazonOrdersCancellationRate]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MP_GetAmazonOrdersCancellationRate]
	@iCustomerMarketPlaceId int,
	@iMonthsShiftNumber int,
	@CancellationRate float output
AS
BEGIN

	DECLARE @iCanceledItems float, @iTotalItems float;
	--
	select @iCanceledItems = COUNT(i.Id)
	from MP_AmazonOrderItem2 i
	left join MP_AmazonOrder o on i.AmazonOrderId = o.Id
	where i.OrderStatus LIKE 'Canceled'
		AND o.CustomerMarketPlaceId = @iCustomerMarketPlaceId
		
		AND DATEADD(MONTH, @iMonthsShiftNumber, i.PurchaseDate ) >= GETDATE()

	select @iTotalItems = SUM(i.NumberOfItemsShipped)
	from MP_AmazonOrderItem2 i
	left join MP_AmazonOrder o on i.AmazonOrderId = o.Id
	where (i.OrderStatus LIKE 'Shipped'
		OR i.OrderStatus LIKE 'Canceled')
		AND o.CustomerMarketPlaceId = @iCustomerMarketPlaceId
		
	    AND DATEADD(MONTH, @iMonthsShiftNumber, i.PurchaseDate ) >= GETDATE()
                 
	select @CancellationRate = @iCanceledItems/@iTotalItems; 

END

GO
/****** Object:  StoredProcedure [dbo].[MP_GetAmazonOrdersCancelled]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MP_GetAmazonOrdersCancelled]
	@iCustomerMarketPlaceId int,
	@iMonthsShiftNumber int
AS
BEGIN
	
DECLARE @iOrdersNumber int;
--	
select @iOrdersNumber = Count(i.Id)
from MP_AmazonOrderItem2 i
left join MP_AmazonOrder o on i.AmazonOrderId = o.Id
where i.OrderStatus LIKE 'Canceled'
   AND o.CustomerMarketPlaceId = @iCustomerMarketPlaceId

   AND DATEADD(MONTH, @iMonthsShiftNumber, i.PurchaseDate ) >= GETDATE()
	                 
	return @iOrdersNumber;

END

GO
/****** Object:  StoredProcedure [dbo].[MP_GetAmazonOrdersNumber]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MP_GetAmazonOrdersNumber]
	@iCustomerMarketPlaceId int,
	@iMonthsShiftNumber int
AS
BEGIN

DECLARE @iOrdersNumber int;
--	
select @iOrdersNumber = COUNT(i.Id)
from MP_AmazonOrderItem2 i
left join MP_AmazonOrder o on i.AmazonOrderId = o.Id
where i.OrderStatus LIKE 'Shipped'
   AND o.CustomerMarketPlaceId = @iCustomerMarketPlaceId

   AND DATEADD(MONTH, @iMonthsShiftNumber, i.PurchaseDate ) >= GETDATE()

return @iOrdersNumber;
	
END

GO
/****** Object:  StoredProcedure [dbo].[MP_GetAmazonOrdersTotalSum]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MP_GetAmazonOrdersTotalSum]
	@iCustomerMarketPlaceId int,
	@iMonthsShiftNumber int
AS
BEGIN
	
DECLARE @iPaidAmount int;
 --
 select @iPaidAmount = SUM(i.OrderTotal*i.NumberOfItemsShipped* dbo.MP_GetCurrencyRate(i.OrderTotalCurrency, i.PurchaseDate) )
 from MP_AmazonOrderItem2 i
 left join MP_AmazonOrder o on i.AmazonOrderId = o.Id
where i.OrderStatus LIKE 'Shipped'
   AND o.CustomerMarketPlaceId = @iCustomerMarketPlaceId
   AND DATEADD(MONTH, @iMonthsShiftNumber, i.PurchaseDate ) >= GETDATE()
                  
 return @iPaidAmount;

	
END

GO
/****** Object:  StoredProcedure [dbo].[MP_GetEbayFeedback]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MP_GetEbayFeedback]
	@iCustomerMarketPlaceId int

	
	
AS
BEGIN

 
select Negative, Positive, Neutral 

from MP_CustomerMarketPlace cmp
LEFT JOIN MP_EbayFeedback fb ON fb.CustomerMarketPlaceId=@iCustomerMarketPlaceId
          and fb.Created = (select MAX(fb1.Created)
          from MP_EbayFeedback fb1
          where fb1.CustomerMarketPlaceId = fb.CustomerMarketPlaceId)  
  LEFT JOIN MP_EbayFeedbackItem fbi on fbi.EbayFeedbackId = fb.Id and fbi.TimePeriodId = 4


WHERE cmp.MarketPlaceId=1

END

GO
/****** Object:  StoredProcedure [dbo].[MP_GetEbayItemsOrdered]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MP_GetEbayItemsOrdered]
	@iCustomerMarketPlaceId int,
	@iMonthsShiftNumber int
AS
BEGIN
	
	DECLARE @iCountItems int;
	--
	select @iCountItems = SUM(t.QuantityPurchased)
	from MP_EbayOrderItem i
	left join MP_EbayOrder o on i.OrderId = o.Id
	left join MP_EbayTransaction t on t.OrderItemId = i.Id
	where (i.OrderStatus LIKE 'Completed' 
	  OR i.OrderStatus LIKE 'Authenticated'
	  OR i.OrderStatus LIKE 'Shipped')
	   AND o.CustomerMarketPlaceId = @iCustomerMarketPlaceId
	   AND DATEADD(MONTH, @iMonthsShiftNumber, i.CreatedTime ) >= GETDATE()
	                 
	return @iCountItems ;

END

GO
/****** Object:  StoredProcedure [dbo].[MP_GetEbayOrdersCancellationRate]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MP_GetEbayOrdersCancellationRate]
	@iCustomerMarketPlaceId int,
	@iMonthsShiftNumber int,
	@CancellationRate float output
AS
BEGIN

	DECLARE @iCanceledItems float, @iTotalItems float;
	--
	select @iCanceledItems = COUNT(i.Id)
	from MP_EbayOrderItem i
	left join MP_EbayOrder o on i.OrderId = o.Id
	where i.OrderStatus LIKE 'Cancelled' 
	   AND o.CustomerMarketPlaceId = @iCustomerMarketPlaceId
	   AND DATEADD(MONTH, @iMonthsShiftNumber, i.CreatedTime ) >= GETDATE()

	select @iTotalItems = SUM(t.QuantityPurchased)
	from MP_EbayOrderItem i
	left join MP_EbayOrder o on i.OrderId = o.Id
	left join MP_EbayTransaction t on t.OrderItemId = i.Id
	where (i.OrderStatus LIKE 'Completed' 
	  OR i.OrderStatus LIKE 'Authenticated'
	  OR i.OrderStatus LIKE 'Shipped'
	  OR i.OrderStatus LIKE 'Cancelled')
	   AND o.CustomerMarketPlaceId = @iCustomerMarketPlaceId
	   AND DATEADD(MONTH, @iMonthsShiftNumber, i.CreatedTime ) >= GETDATE()
                 
	select @CancellationRate = @iCanceledItems/@iTotalItems; 

END

GO
/****** Object:  StoredProcedure [dbo].[MP_GetEbayOrdersCancelled]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MP_GetEbayOrdersCancelled]
	@iCustomerMarketPlaceId int,
	@iMonthsShiftNumber int
AS
BEGIN
	
	DECLARE @iCountItems int;
	--
	select @iCountItems = COUNT(i.Id)
	from MP_EbayOrderItem i
	left join MP_EbayOrder o on i.OrderId = o.Id
	where i.OrderStatus LIKE 'Cancelled' 
	   AND o.CustomerMarketPlaceId = @iCustomerMarketPlaceId
	   AND DATEADD(MONTH, @iMonthsShiftNumber, i.CreatedTime ) >= GETDATE()
	                 
	return @iCountItems;

END

GO
/****** Object:  StoredProcedure [dbo].[MP_GetEbayOrdersNumber]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MP_GetEbayOrdersNumber]
	@iCustomerMarketPlaceId int,
	@iMonthsShiftNumber int
AS
BEGIN

DECLARE @iOrdersNumber int;
--	
select @iOrdersNumber = COUNT(o.Id)
from MP_EbayOrderItem i
left join MP_EbayOrder o on i.OrderId = o.Id
where (i.OrderStatus LIKE 'Completed' 
   OR i.OrderStatus LIKE 'Authenticated'
   OR i.OrderStatus LIKE 'Shipped')
   AND o.CustomerMarketPlaceId = @iCustomerMarketPlaceId
   AND DATEADD(MONTH, @iMonthsShiftNumber, i.CreatedTime ) >= GETDATE()

return @iOrdersNumber;
	
END

GO
/****** Object:  StoredProcedure [dbo].[MP_GetEbayOrdersTotalSum]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MP_GetEbayOrdersTotalSum]
	@iCustomerMarketPlaceId int,
	@iMonthsShiftNumber int
AS
BEGIN
	
DECLARE @iPaidAmount int;
 --
 select @iPaidAmount = SUM(i.AmountPaidAmount*t.QuantityPurchased*dbo.MP_GetCurrencyRate(i.AmountPaidCurrency, i.CreatedTime))
 from MP_EbayOrderItem i
 left join MP_EbayOrder o on i.OrderId = o.Id
  left join MP_EbayTransaction t on t.OrderItemId = i.Id
 where (i.OrderStatus LIKE 'Completed' 
   OR i.OrderStatus LIKE 'Authenticated'
   OR i.OrderStatus LIKE 'Shipped')
    AND o.CustomerMarketPlaceId = @iCustomerMarketPlaceId
    AND DATEADD(MONTH, @iMonthsShiftNumber, i.CreatedTime ) >= GETDATE()
                  
 return @iPaidAmount;

	
END

GO
/****** Object:  StoredProcedure [dbo].[MP_GetInventoryCashValue]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MP_GetInventoryCashValue]
	@iCustomerMarketPlaceId int,
	@iMarketPlaceId int
AS
BEGIN
select    
(select SUM(TableInventoryCashValue.Quantity * TableInventoryCashValue.Price / TableInventoryCashValue.Rate)
  from
  (
   select ii1.Quantity as Quantity,
       ii1.Price as Price,
       (select TableRate.Rate from dbo.MP_GetCurrencyRate1(ii1.Currency, NULL) TableRate) as Rate
       --dbo.MP_GetCurrencyRate(ii.Currency, NULL ))
             
   from MP_EbayAmazonInventory inv1 
   LEFT JOIN MP_EbayAmazonInventoryItem ii1 on ii1.InventoryId = inv1.Id
   where inv1.CustomerMarketPlaceId = cmp.Id  
   and inv1.Created = (select MAX(inv2.Created)
      from MP_EbayAmazonInventory inv2
      where inv2.CustomerMarketPlaceId = inv1.CustomerMarketPlaceId)
  ) as TableInventoryCashValue) as fsda


FROM MP_CustomerMarketPlace cmp 

  
WHERE cmp.Id = @iCustomerMarketPlaceId and cmp.MarketPlaceId=@iMarketPlaceId
	
END

GO
/****** Object:  StoredProcedure [dbo].[MP_GetInventoryItemsCount]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MP_GetInventoryItemsCount]
	@iCustomerMarketPlaceId int
AS
BEGIN
	
	DECLARE @iCountItems int;
	--
	select @iCountItems = SUM(i.Quantity)
	from MP_EbayAmazonInventory v
	left join MP_EbayAmazonInventoryItem i on i.InventoryId = v.Id
	where v.CustomerMarketPlaceId = @iCustomerMarketPlaceId
	and v.Created = (select MAX(v1.Created)
	                 from MP_EbayAmazonInventory v1
	                 where v1.CustomerMarketPlaceId = @iCustomerMarketPlaceId)
	                 
	return @iCountItems;
	
END

GO
/****** Object:  StoredProcedure [dbo].[MP_GetPayPalActivity]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MP_GetPayPalActivity]
	@iUserId int

AS
BEGIN
	
Select   
-- Number of orders
cmp.Id as umi,
 aot.TransactionsNumber1M,
 aot.TransactionsNumber3M,
 aot.TransactionsNumber6M,
 aot.TransactionsNumber1Y,

 aot.TotalPayments1M,
 aot.TotalPayments3M,
 aot.TotalPayments3M,
 aot.TotalPayments1Y,
 
 aot.TotalTransactions1M,
 aot.TotalTransactions3M,
 aot.TotalTransactions6M,
 aot.TotalTransactions1Y



FROM MP_CustomerMarketPlace cmp
 LEFT JOIN Customer c ON cmp.CustomerId = c.Id


left join
(
 select PayPalOrderTable.CustomerMarketPlaceId,
  COUNT(distinct ppti1.Id) as TransactionsNumber1M,
  COUNT(distinct ppti2.Id)as TransactionsNumber3M,
  COUNT(distinct ppti3.Id)as TransactionsNumber6M,
  COUNT(distinct ppti4.Id)as TransactionsNumber1Y,
  SUM(ppti1.NetAmountAmount) as TotalPayments1M,
  SUM(ppti2.NetAmountAmount) as TotalPayments3M,
  SUM(ppti3.NetAmountAmount) as TotalPayments6M,
  SUM(ppti4.NetAmountAmount) as TotalPayments1Y,
  
  SUM(ppti5.NetAmountAmount) as TotalTransactions1M,
  SUM(ppti6.NetAmountAmount) as TotalTransactions3M,
  SUM(ppti7.NetAmountAmount) as TotalTransactions6M,
  SUM(ppti8.NetAmountAmount) as TotalTransactions1Y

 from
 (
  select ppt.CustomerMarketPlaceId,
   ppti.Id,
   ppti.NetAmountAmount,
   ppti.Created,
   ppti.Type,
      (select TableRate.Rate from dbo.MP_GetCurrencyRate1(ppti.NetAmountCurrency, ppti.Created) TableRate) as Rate
  from  MP_PayPalTransaction ppt
  left join MP_PayPalTransactionItem ppti on ppti.TransactionId = ppt.Id 
     
 ) as PayPalOrderTable
 left join MP_PayPalTransactionItem ppti1 on  ppti1.Id = PayPalOrderTable.Id AND DATEADD(MONTH, 1, PayPalOrderTable.Created) >= GETDATE() and ppti1.Status = 'Completed'  and ppti1.Type = 'Payment'
 left join MP_PayPalTransactionItem ppti2 on  ppti2.Id = PayPalOrderTable.Id AND DATEADD(MONTH, 3, PayPalOrderTable.Created ) >= GETDATE() and ppti2.Status = 'Completed'  and ppti2.Type = 'Payment'
 left join MP_PayPalTransactionItem ppti3 on  ppti3.Id = PayPalOrderTable.Id AND DATEADD(MONTH, 6, PayPalOrderTable.Created ) >= GETDATE() and ppti3.Status = 'Completed'  and ppti3.Type = 'Payment'
 left join MP_PayPalTransactionItem ppti4 on  ppti4.Id = PayPalOrderTable.Id AND DATEADD(MONTH, 12, PayPalOrderTable.Created ) >= GETDATE() and ppti4.Status = 'Completed'  and ppti4.Type = 'Payment'
 left join MP_PayPalTransactionItem ppti5 on  ppti5.Id = PayPalOrderTable.Id AND DATEADD(MONTH, 1, PayPalOrderTable.Created) >= GETDATE() and ppti5.Status = 'Completed'  and ppti5.Type = 'Transfer'
 left join MP_PayPalTransactionItem ppti6 on  ppti6.Id = PayPalOrderTable.Id AND DATEADD(MONTH, 3, PayPalOrderTable.Created ) >= GETDATE() and ppti6.Status = 'Completed'  and ppti6.Type = 'Transfer'
 left join MP_PayPalTransactionItem ppti7 on  ppti7.Id = PayPalOrderTable.Id AND DATEADD(MONTH, 6, PayPalOrderTable.Created ) >= GETDATE() and ppti7.Status = 'Completed'  and ppti7.Type = 'Transfer'
 left join MP_PayPalTransactionItem ppti8 on  ppti8.Id = PayPalOrderTable.Id AND DATEADD(MONTH, 12, PayPalOrderTable.Created ) >= GETDATE() and ppti8.Status = 'Completed'  and ppti8.Type = 'Transfer'
 group by CustomerMarketPlaceId
) as aot on aot.CustomerMarketPlaceId = cmp.id


WHERE c.Id = @iUserId

END

GO
/****** Object:  StoredProcedure [dbo].[MP_GetScoreCardData]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MP_GetScoreCardData]
	@CustomerId int, 
	@AnnualSalesTotal int output,
	@MaxFeedback int output,
	@MPsNumber int output,
	@EZBOBSeniority int output,
	@OnTimeLoans int output,
	@LatePayments int output,
	@EarlyPayments int output,
	@MaxMPSeniority int output
	
AS
BEGIN

select  c.Id,
IsNull(AnnualSalesEbay.Total, 0) + IsNull(AnnualSalesAmazon.Total, 0) as AnnualSalesTotal,
 FeedBack.MaxFeedback,
( select count(cmp.Id) from MP_CustomerMarketPlace cmp WHERE cmp.customerId = c.Id ) as MPsNumber,
ISNULL(DateDiff( DAY, c.GreetingMailSentDate, GETDATE()), 0) as EZBOBSeniority,
( select count(l.id) from Loan l where l.CustomerId = c.Id and l.PaymentStatus = 'OnTime') as OnTimeLoans,
 
 ( select count(ls.id) from LoanSchedule ls 
  LEFT JOIN Loan l on l.Id = ls.Id  
  where ls.Status = 'Late'
  and c.Id = l.CustomerId) as LatePayments,
   
   ( select count(ls.id) from LoanSchedule ls 
  LEFT JOIN Loan l on l.Id = ls.Id  
  where ls.Status = 'Early'
  and c.Id = l.CustomerId) as EarlyPayments,
 
 ISNULL(DateDiff( DAY, MarketplaceSeniorityTable.MarketplaceSeniority, GETDATE()), 0) as AccountAge

from Customer c
left join 
(
select TotalTable12.CustomerId as CustomerId,
 SUM(TotalTable12.Price * TotalTable12.Quantity / TotalTable12.Rate) as Total
 from 
  (
    select 
           c.Id as CustomerId,
     et8.price as Price,
           et8.QuantityPurchased as Quantity, 
          (select TableRate.Rate from dbo.MP_GetCurrencyRate1(eoi8.AmountPaidCurrency, eoi8.CreatedTime) TableRate) as Rate
   from MP_EbayOrderItem eoi8
   left join MP_EbayOrder eo8 on eoi8.OrderId = eo8.Id
   left join MP_EbayTransaction et8 on et8.OrderItemId = eoi8.Id
   left join MP_CustomerMarketPlace cmp ON eo8.CustomerMarketPlaceId = cmp.Id
   left join Customer c on c.Id = cmp.CustomerId
   where (eoi8.OrderStatus LIKE 'Completed' 
    OR eoi8.OrderStatus LIKE 'Authenticated'
    OR eoi8.OrderStatus LIKE 'Shipped')    
 and cmp.EliminationPassed = 1 
    AND DATEADD(MONTH, 12, eoi8.CreatedTime ) >= GETDATE()    
  ) as TotalTable12
 group by CustomerId) as AnnualSalesEbay on c.id = AnnualSalesEbay.CustomerId

left join 
(
select TotalTable3.CustomerId,
SUM(TotalTable3.Price * TotalTable3.Quantity / TotalTable3.Rate) as Total
 from 
   (
    select  c.Id as CustomerId, 
   aoi8.OrderTotal as Price,
           aoi8.NumberOfItemsShipped as Quantity, 
          (select TableRate.Rate from dbo.MP_GetCurrencyRate1(aoi8.OrderTotalCurrency, aoi8.PurchaseDate) TableRate) as Rate
     from  MP_AmazonOrder ao8
    left join MP_AmazonOrderItem2 aoi8 on aoi8.AmazonOrderId = ao8.Id 
    left join MP_CustomerMarketPlace cmp ON  ao8.CustomerMarketPlaceId = cmp.Id
    left join Customer c on c.Id = cmp.CustomerId
    where aoi8.OrderStatus LIKE 'Shipped'   
   AND cmp.EliminationPassed = 1
            AND DATEADD(MONTH, 12, aoi8.PurchaseDate ) >= GETDATE()
  )as TotalTable3 
  group by customerId) as AnnualSalesAmazon on c.Id = AnnualSalesAmazon.CustomerId

left join
(
select FeedbackTable.CustomerId as CustomerId,
Max(FeedbackTable.Feedback) as MaxFeedback
from
(
Select  c.Id as CustomerId,
 cmp.Id as umi,
 
    fbi.Positive as Feedback
 
 FROM MP_CustomerMarketPlace cmp
 LEFT JOIN Customer c  ON cmp.CustomerId = c.Id
 LEFT JOIN MP_AmazonFeedback fb ON fb.CustomerMarketPlaceId=cmp.Id
        and fb.Created = (select MAX(fb1.Created)
        from MP_AmazonFeedback fb1
        where fb1.CustomerMarketPlaceId = fb.CustomerMarketPlaceId)  
 LEFT JOIN MP_AmazonFeedbackItem fbi on fbi.AmazonFeedbackId = fb.Id and fbi.TimePeriodId = 5

 union 

Select  c.Id as CustomerId,
cmp.Id as umi,
 
    fbi2.Positive as Feedback
 
 FROM MP_CustomerMarketPlace cmp
 LEFT JOIN Customer c  ON cmp.CustomerId = c.Id
  LEFT JOIN MP_EbayFeedback fb2 ON fb2.CustomerMarketPlaceId=cmp.Id
          and fb2.Created = (select MAX(fb1.Created)
          from MP_EbayFeedback fb1
          where fb1.CustomerMarketPlaceId = fb2.CustomerMarketPlaceId)  
  LEFT JOIN MP_EbayFeedbackItem fbi2 on fbi2.EbayFeedbackId = fb2.Id and fbi2.TimePeriodId = 6
) as FeedbackTable
group by FeedbackTable.CustomerId
) as FeedBack on c.Id = Feedback.CustomerId
left join 
(
select MarketplacesDatesTable.CustomerId,
MIN(MarketplacesDatesTable.MarketplaceSeniority) as MarketplaceSeniority
from 
(
select cmp.CustomerId,
      aoi.PurchaseDate as MarketplaceSeniority
from  MP_AmazonOrderItem2 aoi
    left join MP_AmazonOrder ao on aoi.AmazonOrderId = ao.Id 
    left join MP_CustomerMarketPlace cmp ON ao.CustomerMarketPlaceId = cmp.CustomerId

union 

select cmp.CustomerId,
  eoi.ShippedTime as MarketplaceSeniority
from MP_EbayOrderItem eoi
left join MP_EbayOrder eo on eoi.OrderId = eo.Id
left join MP_CustomerMarketplace cmp on eo.CustomerMarketPlaceId = cmp.CustomerId
) as MarketplacesDatesTable
group by MarketplacesDatesTable.CustomerId
)as MarketplaceSeniorityTable on MarketplaceSeniorityTable.CustomerId = c.Id


where c.Id = @CustomerId



END

GO
/****** Object:  StoredProcedure [dbo].[MP_IsCustomerMarketPlacesUpdated]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		KOT
-- Create date: 12 April 2012
-- Description:	Return MarketPlaceUpdatedState {0 - false, 1 - true} 
-- =============================================
CREATE PROCEDURE [dbo].[MP_IsCustomerMarketPlacesUpdated]
(
	@pCustomerId  int,
	@isUpdated int output
)
AS
BEGIN

	DECLARE @notUpdatedCount int;
    -- Insert statements for procedure here
	SELECT @notUpdatedCount = COUNT(*)
	FROM Customer c
		LEFT JOIN MP_CustomerMarketPlace cmp ON cmp.CustomerId = c.Id
				
	WHERE c.Id = @pCustomerId
	
		AND (cmp.UpdatingStart IS NULL
		      OR ( cmp.UpdatingStart IS NOT NULL
					AND cmp.UpdatingEnd IS NULL 
				  )
			)
	
	IF @notUpdatedCount > 0
		SET @isUpdated = 0
	ELSE
		SET @isUpdated = 1
	
	
	return @isUpdated;
	
END

GO
/****** Object:  StoredProcedure [dbo].[MP_UpdateEliminationWarning]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MP_UpdateEliminationWarning] 
(@Id int,
 @Warning varchar(max))

AS
BEGIN

UPDATE [dbo].[MP_CustomerMarketPlace]
   SET  [Warning] = @Warning 
		
 WHERE Id = @Id



 SET NOCOUNT ON;
SELECT @@IDENTITY;
END

GO
/****** Object:  StoredProcedure [dbo].[MP_UpdateMPEliminationStatus]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MP_UpdateMPEliminationStatus] 
(@iCustomerMarketPlaceId int, @EliminationPassed int)

AS
BEGIN

UPDATE [dbo].[MP_CustomerMarketPlace]
   SET [EliminationPassed] = @EliminationPassed
 WHERE Id = @iCustomerMarketPlaceId


END

GO
/****** Object:  StoredProcedure [dbo].[Nodes_GetDeletedList]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE  PROCEDURE  [dbo].[Nodes_GetDeletedList]
AS
BEGIN
      select security_application.name as "appname",
              strategy_node.name as "nodename",
              strategy_node.nodeid,
              strategy_node.displayname,
              strategy_node.description,
              strategy_node.nodecomment,
              security_application.applicationid,
              strategy_node.terminationdate,
              strategy_node.containsprint,
              strategy_node.executionduration,
              strategy_node.ishardreaction,
              strategy_node.Icon,
              strategy_node.Guid AS "nodeguid",
              strategy_node.Ndx
         from strategy_node,
              security_application
        where security_application.applicationid = strategy_node.applicationid
        and strategy_node.isdeleted <> 0;
END

GO
/****** Object:  StoredProcedure [dbo].[Nodes_GetShortList]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE  PROCEDURE  [dbo].[Nodes_GetShortList]
AS
BEGIN
      select security_application.name as "appname",
              strategy_node.name as "nodename",
              strategy_node.nodeid,
              strategy_node.displayname,
              strategy_node.description,
              strategy_node.nodecomment,
              security_application.applicationid,
              strategy_node.terminationdate,
              strategy_node.containsprint,
              strategy_node.executionduration,
              strategy_node.ishardreaction,
              strategy_node.Icon,
              strategy_node.Guid AS "nodeguid",
              strategy_node.Ndx
         from strategy_node,
              security_application
        where security_application.applicationid = strategy_node.applicationid
        and strategy_node.isdeleted = 0;
END

GO
/****** Object:  StoredProcedure [dbo].[Paging_Cursor]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Paging_Cursor] (
@pTables nvarchar(1000),
@pPK nvarchar(100),
@pSort nvarchar(200) = NULL,
@pPageNumber int = 1,
@pPageSize int = 10,
@pFields nvarchar(1000) = '*',
@pFilter nvarchar(1000) = NULL,
@pGroup nvarchar(1000) = NULL)
AS

/*Find the @PK type*/
DECLARE @PKTable nvarchar(100)
DECLARE @PKName nvarchar(100)
DECLARE @type nvarchar(100)
DECLARE @prec int

IF CHARINDEX('.', @pPK) > 0
	BEGIN
		SET @PKTable = SUBSTRING(@pPK, 0, CHARINDEX('.',@pPK))
		SET @PKName = SUBSTRING(@pPK, CHARINDEX('.',@pPK) + 1, LEN(@pPK))
	END
ELSE
	BEGIN
		SET @PKTable = @pTables
		SET @PKName = @pPK
	END

SELECT @type=t.name, @prec=c.prec
FROM sysobjects o 
JOIN syscolumns c on o.id=c.id
JOIN systypes t on c.xusertype=t.xusertype
WHERE o.name = @PKTable AND c.name = @PKName

IF CHARINDEX('char', @type) > 0
   SET @type = @type + '(' + CAST(@prec AS nvarchar) + ')'

IF @type is NULL
	SET @type = 'bigint'

DECLARE @strPageSize nvarchar(50)
DECLARE @strStartRow nvarchar(50)
DECLARE @strFilter nvarchar(1000)
DECLARE @strGroup nvarchar(1000)

/*Default Sorting*/
IF @pSort IS NULL OR @pSort = ''
	SET @pSort = @pPK

/*Default Page Number*/
IF @pPageNumber < 1
	SET @pPageNumber = 1

/*Set paging variables.*/
SET @strPageSize = CAST(@pPageSize AS nvarchar(50))
SET @strStartRow = CAST(((@pPageNumber - 1)*@pPageSize + 1) AS nvarchar(50))

/*Set filter & group variables.*/
IF @pFilter IS NOT NULL AND @pFilter != ''
	SET @strFilter = ' WHERE ' + @pFilter + ' '
ELSE
	SET @strFilter = ''
IF @pGroup IS NOT NULL AND @pGroup != ''
	SET @strGroup = ' GROUP BY ' + @pGroup + ' '
ELSE
	SET @strGroup = ''

/*Execute dynamic query*/	
EXEC(
'
DECLARE @PageSize int
SET @PageSize = ' + @strPageSize + '

DECLARE @PK ' + @type + '
DECLARE @tblPK TABLE (
            PK  ' + @type + ' NOT NULL PRIMARY KEY
            )
Insert into dbo.Log_TraceLog(Message) values ( ''Before Declare cursor'')
DECLARE PagingCursor CURSOR DYNAMIC READ_ONLY FOR
SELECT '  + @pPK + ' FROM ' + @pTables + @strFilter + ' ' + @strGroup + ' ORDER BY ' + @pSort + '
OPEN PagingCursor

FETCH RELATIVE ' + @strStartRow + ' FROM PagingCursor INTO @PK
SET NOCOUNT ON

WHILE @PageSize > 0 AND @@FETCH_STATUS = 0
BEGIN
            INSERT @tblPK (PK)  VALUES (@PK)
            FETCH NEXT FROM PagingCursor INTO @PK
            SET @PageSize = @PageSize - 1
END

CLOSE       PagingCursor
DEALLOCATE  PagingCursor

SELECT ' + @pFields + ' FROM ' + @pTables + ' JOIN @tblPK tblPK ON ' + @pPK + ' = tblPK.PK ' + @strFilter + ' ' + @strGroup + ' ORDER BY ' + @pSort 
)

GO
/****** Object:  StoredProcedure [dbo].[PAGING_CURSOR_COUNT]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PAGING_CURSOR_COUNT]
  @pTableName nvarchar(4000),
  @pCondition nvarchar(4000)  = null
AS
BEGIN
  DECLARE @strSELECT nvarchar(4000);
  DECLARE @ParmDefinition nvarchar(500);
  DECLARE @CntValue int;

  SET @ParmDefinition = N'@CntOUT Int OUTPUT';

  SET @strSELECT = N' SELECT @CntOUT = count(*) FROM '+@pTableName+
      CASE 
        WHEN @pCondition is null then
           ''
        ELSE
          ' WHERE  '+ @pCondition
      END;


EXECUTE sp_executesql @strSELECT,
  @ParmDefinition,
  @CntOUT = @CntValue OUTPUT;

Select @CntValue

END;

GO
/****** Object:  StoredProcedure [dbo].[PaypointOneTypeReconciliation]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PaypointOneTypeReconciliation]
@Date DATE,
@IncludeFive BIT,
@SuccessOnly BIT,
@Caption NVARCHAR(64)
AS
BEGIN
	DECLARE @EzbobTotal DECIMAL(18, 2)
	DECLARE @PaypointTotal DECIMAL(18, 2)

	DECLARE @Amount DECIMAL(18, 2), @EzbobCount INT, @PaypointCount INT

	------------------------------------------------------------------------------

	CREATE TABLE #paypoint (
		Amount DECIMAL(18, 2) NOT NULL,
		Counter INT NOT NULL
	)

	CREATE TABLE #ezbob (
		Amount DECIMAL(18, 2) NOT NULL,
		Counter INT NOT NULL
	)

	CREATE TABLE #res (
		Amount DECIMAL(18, 2) NOT NULL,
		EzbobCount INT NOT NULL,
		PaypointCount INT NOT NULL
	)

	------------------------------------------------------------------------------

	INSERT INTO #paypoint
	SELECT
		amount,
		COUNT(*)
	FROM
		PayPointBalance
	WHERE
		(
			(@SuccessOnly = 1 AND auth_code != '')
			OR
			(@SuccessOnly = 0 AND auth_code = '')
		)
		AND
		CONVERT(DATE, date) = @Date
		AND
		(@IncludeFive = 1 OR Amount != 5)
	GROUP BY
		amount

	------------------------------------------------------------------------------

	INSERT INTO #ezbob
	SELECT
		Amount,
		COUNT(*)
	FROM
		LoanTransaction
	WHERE
		Type = 'PaypointTransaction'
		AND
		(
			(@SuccessOnly = 1 AND Status = 'Done')
			OR
			(@SuccessOnly = 0 AND Status != 'Done')
		)
		AND
		CONVERT(DATE, PostDate) = @Date
		AND
		(@IncludeFive = 1 OR Amount != 5)
	GROUP BY
		Amount

	------------------------------------------------------------------------------

	INSERT INTO #res
	SELECT
		e.Amount,
		ISNULL(e.Counter, 0),
		ISNULL(p.Counter, 0)	
	FROM
		#ezbob e
		LEFT JOIN #paypoint p ON e.Amount = p.Amount

	------------------------------------------------------------------------------

	INSERT INTO #res
	SELECT
		p.Amount,
		ISNULL(e.Counter, 0),
		ISNULL(p.Counter, 0)	
	FROM
		#ezbob e
		RIGHT JOIN #paypoint p ON e.Amount = p.Amount
	WHERE
		e.Amount IS NULL

	------------------------------------------------------------------------------

	SELECT
		@PaypointTotal = ISNULL(SUM(Amount * Counter), 0)
	FROM
		#paypoint

	------------------------------------------------------------------------------

	SELECT
		@EzbobTotal = ISNULL(SUM(Amount * Counter), 0)
	FROM
		#ezbob

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Amount, Css)
		VALUES (
			'Ezbob Total ' + @Caption + ' Transactions',
			@EzbobTotal,
			@Caption + CASE WHEN @EzbobTotal = @PaypointTotal THEN '' ELSE ' unmatched' END
		)

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Amount, Css)
		VALUES ('Paypoint Total ' + @Caption + ' Transactions',
			@PaypointTotal,
			@Caption + CASE WHEN @EzbobTotal = @PaypointTotal THEN '' ELSE ' unmatched' END
		)

	------------------------------------------------------------------------------

	DELETE FROM #res WHERE EzbobCount = PaypointCount

	------------------------------------------------------------------------------

	DECLARE cur CURSOR FOR
		SELECT Amount, EzbobCount, PaypointCount
		FROM #res
		ORDER BY Amount

	OPEN cur

	------------------------------------------------------------------------------

	FETCH NEXT FROM cur INTO @Amount, @EzbobCount, @PaypointCount

	------------------------------------------------------------------------------

	WHILE @@FETCH_STATUS = 0
	BEGIN
		INSERT INTO #out(Caption, TranID)
		SELECT
			'Ezbob',
			t.Id
		FROM
			LoanTransaction t
		WHERE
			CONVERT(DATE, t.PostDate) = @Date
			AND
			(
				(@SuccessOnly = 1 AND t.Status = 'Done')
				OR
				(@SuccessOnly = 0 AND t.Status != 'Done')
			)
			AND
			t.Type = 'PaypointTransaction'
			AND
			t.Amount = @Amount

		-----------------------------------------------------------------------------

		INSERT INTO #out(Caption, TranID)
		SELECT
			'Paypoint',
			b.Id
		FROM
			PayPointBalance b
		WHERE
			(
				(@SuccessOnly = 1 AND b.auth_code != '')
				OR
				(@SuccessOnly = 0 AND b.auth_code = '')
			)
			AND
			CONVERT(DATE, b.date) = @Date
			AND
			(@IncludeFive = 1 OR b.Amount != 5)
			AND
			b.amount = @Amount

		-------------------------------------------------------------------------

		FETCH NEXT FROM cur INTO @Amount, @EzbobCount, @PaypointCount
	END

	------------------------------------------------------------------------------

	CLOSE cur
	DEALLOCATE cur

	------------------------------------------------------------------------------

	DROP TABLE #res
	DROP TABLE #ezbob
	DROP TABLE #paypoint
END

GO


/****** Object:  StoredProcedure [dbo].[RptAddReportUser]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptAddReportUser]
@UserName NVARCHAR(50),
@Name     NVARCHAR(50),
@Password VARBINARY(20),
@Salt     VARBINARY(20)
AS
BEGIN
	INSERT INTO ReportUsers (UserName, Name, Password, Salt)
		VALUES (@UserName, @Name, @Password, @Salt)
END

GO
/****** Object:  StoredProcedure [dbo].[RptAddUserReportMap]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptAddUserReportMap]
@UserName   NVARCHAR(50),
@ReportType NVARCHAR(200)
AS
BEGIN
	DECLARE @UserId INT = (SELECT Id FROM ReportUsers WHERE UserName = @UserName)
	DECLARE @ReportId INT = (SELECT Id FROM ReportScheduler WHERE ReportScheduler.Type = @ReportType)

	INSERT INTO ReportsUsersMap (UserID, ReportID)
		VALUES (@UserId, @ReportId)
END

GO
/****** Object:  StoredProcedure [dbo].[RptAdsReport]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptAdsReport]
@time DATETIME
AS
BEGIN
	SELECT
		ReferenceSource,
		count(1) TotalUsers,
		PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), SUM(CreditSum))), 1), 2) TotalCredit
	FROM
		Customer
	WHERE
		CONVERT(DATE, @time) <= GreetingMailSentDate
	GROUP BY
		ReferenceSource
	ORDER BY
		ReferenceSource
END

GO
/****** Object:  StoredProcedure [dbo].[RptBugs]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create PROCEDURE [dbo].[RptBugs]
@DateStart DATETIME,
@DateEnd   DATETIME
AS
BEGIN
	SELECT
		ROW_NUMBER() OVER (ORDER BY b.DateOpened DESC) AS RowID,
		c.Name,
		b.Type,
		b.DateOpened,
		b.TextOpened,
		s.UserName
	FROM
		Bug b 
		JOIN Customer c ON b.CustomerId = c.Id
		JOIN Security_User s ON b.UnderwriterOpenedId = s.UserId 
	WHERE
		b.State = 'Opened' 
	ORDER BY
		b.DateOpened DESC
END

GO
/****** Object:  StoredProcedure [dbo].[RptCallManagement]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptCallManagement]
		@DateStart DATETIME,
		@DateEnd   DATETIME
AS
BEGIN
	SET @DateStart = CONVERT(DATE, @DateStart)
	SET @DateEnd = CONVERT(DATE, @DateEnd)

----------- GET Customer's Data and CRM Data---------
IF OBJECT_ID('tempdb..#temp1') IS NOT NULL DROP TABLE #temp1

SELECT c.Id cId, c.Name eMail, c.Fullname name, c.LastStatus Status, c.ManagerApprovedSum OpenOffer,
(x.Comment) CRMComment, (x.Status) CRMStatus, (x.Action) CRMAction
INTO #temp1
FROM Customer c
LEFT OUTER JOIN 
	(SELECT cr.Id crId, cr.CustomerId CustomerId, cr.Comment Comment, crma.Name Action, crms.Name Status 
	 FROM CustomerRelations cr
	 LEFT JOIN CustomerRelations cr2 ON (cr.CustomerId = cr2.CustomerId AND cr.Id < cr2.Id)
	 LEFT OUTER JOIN CRMActions crma ON crma.Id = cr.ActionId
     LEFT OUTER JOIN  CRMStatuses crms ON crms.Id = cr.StatusId
     WHERE cr2.Id IS NULL
     ) x 
ON c.Id = x.CustomerId
WHERE c.Id NOT IN 
	(SELECT C.Id
	 FROM Customer C 
	 WHERE Name LIKE '%ezbob%'
	 OR Name LIKE '%liatvanir%'
	 OR Name LIKE '%q@q%'
	 OR Name LIKE '%1@1%'
	 OR C.IsTest=1
	 )
AND c.WizardStep = 4  -- finished wizard only

----------- GET Loan Count Data---------
IF OBJECT_ID('tempdb..#temp2') IS NOT NULL DROP TABLE #temp2

SELECT c.Id cId, count(l.Id) AmountOfLoans, ISNULL(sum(l.Balance), 0) OutstandingBalance
INTO #temp2
FROM Customer c
LEFT JOIN Loan l ON c.Id = l.CustomerId
WHERE c.Id NOT IN 
				  ( SELECT C.Id
					FROM Customer C 
					WHERE Name LIKE '%ezbob%'
					OR Name LIKE '%liatvanir%'
					OR Name LIKE '%q@q%'
					OR Name LIKE '%1@1%'
					OR C.IsTest=1
					)
GROUP BY c.Id

----------- GET Loan Status---------
IF OBJECT_ID('tempdb..#temp3') IS NOT NULL DROP TABLE #temp3
SELECT x.cId, 
COALESCE(
  CASE WHEN x.StatusId='0' THEN 'Late' ELSE NULL END, 
  CASE WHEN x.StatusId='1' THEN 'Live' ELSE NULL END,
  CASE WHEN x.StatusId='2' THEN 'PaidOff' ELSE NULL END,
  CASE WHEN x.StatusId='3' THEN '-' ELSE NULL END)
  AS LoanStatus
 INTO #temp3
 FROM 
 (
SELECT t.cId, min(t.Status) StatusId FROM 
(
SELECT c.Id cId, 
  COALESCE(
  CASE WHEN l.Status='Late' THEN '0' ELSE NULL END, 
  CASE WHEN l.Status='Live' THEN '1' ELSE NULL END,
  CASE WHEN l.Status='PaidOff' THEN '2' ELSE NULL END,
  CASE WHEN l.Status IS NULL THEN '3' ELSE NULL END)
  AS Status
FROM Customer c
LEFT JOIN Loan l ON c.Id = l.CustomerId
WHERE c.Id NOT IN 
				  ( SELECT C.Id
					FROM Customer C 
					WHERE Name LIKE '%ezbob%'
					OR Name LIKE '%liatvanir%'
					OR Name LIKE '%q@q%'
					OR Name LIKE '%1@1%'
				  	OR C.IsTest=1
				  )
) t
GROUP BY t.cId
) x

--------- FINAL TABLE MERGE ----------

SELECT T1.eMail, T1.name, T1.Status, T1.OpenOffer, T2.AmountOfLoans, T2.OutstandingBalance,
T3.LoanStatus, T1.CRMStatus, T1.CRMAction, T1.CRMComment
FROM #temp1 T1  
LEFT JOIN #temp2 T2 ON T1.cId = T2.cId
LEFT JOIN #temp3 T3 ON T1.cId = T3.cId


DROP TABLE #temp1
DROP TABLE #temp2
DROP TABLE #temp3

END

GO
/****** Object:  StoredProcedure [dbo].[RptChangePassword]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptChangePassword]
@UserName NVARCHAR(50),
@Password VARBINARY(20),
@Salt     VARBINARY(20) 
AS 
BEGIN
	UPDATE ReportUsers SET
		Password = @Password,
		Salt = @Salt
	WHERE
		UserName = @UserName
END

GO
/****** Object:  StoredProcedure [dbo].[RptCustAnualSales]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptCustAnualSales]
@DateStart DATETIME,
@DateEnd   DATETIME
AS
BEGIN
	IF OBJECT_ID('tempdb..#SumOfOrdersId') IS NOT NULL
		DROP TABLE #SumOfOrdersId

	IF OBJECT_ID('tempdb..#Shops') IS NOT NULL
		DROP TABLE #Shops

	IF OBJECT_ID('tempdb..#Paypal') IS NOT NULL
		DROP TABLE #Paypal

	IF OBJECT_ID('tempdb..#MP_Stores') IS NOT NULL
		DROP TABLE #MP_Stores

	IF OBJECT_ID('tempdb..#temp1') IS NOT NULL
		DROP TABLE #temp1

	IF OBJECT_ID('tempdb..#temp2') IS NOT NULL
		DROP TABLE #temp2

	IF OBJECT_ID('tempdb..#temp3') IS NOT NULL
		DROP TABLE #temp3

	---- SumOfOrders ID ----
	SELECT
		Id
	INTO
		#SumOfOrdersId
	FROM
		MP_AnalyisisFunction
	WHERE
		Name = 'TotalSumOfOrders'


	---- # OF SHOPS PER CUSTOMER ----
	DECLARE
		@eBayId   INT,
		@AmazonId INT,
		@PaypalId INT

	SET @eBayId   = (SELECT Id FROM MP_MarketplaceType WHERE Id = 1)
	SET @AmazonId = (SELECT Id FROM MP_MarketplaceType WHERE Id = 2)
	SET @PaypalId = (SELECT Id FROM MP_MarketplaceType WHERE Id = 3)

	SELECT
		CustomerId,
		COUNT(MarketPlaceId) AS Shops
	INTO
		#Shops
	FROM
		MP_CustomerMarketPlace
	WHERE
		MarketPlaceId IN (@eBayId, @AmazonId)
	GROUP BY
		CustomerId

	---- PAYPAL ACCOUNT PER CUSTOMER ----
	SELECT
		CustomerId,
		COUNT(MarketPlaceId) AS Paypal
	INTO
		#Paypal
	FROM
		MP_CustomerMarketPlace
	WHERE
		MarketPlaceId IN (@PaypalId)
	GROUP BY
		CustomerId

	---- TEMP FOLDER WITH STORES ANUAL SALES ----
	DECLARE @SalesPeriod INT

	SET @SalesPeriod = (SELECT Id FROM MP_AnalysisFunctionTimePeriod WHERE Id = 4);

	SELECT
		A.CustomerMarketPlaceId,
		MAX(A.Updated) UpdatedDate
	INTO
		#MP_Stores
	FROM
		MP_AnalyisisFunctionValues A
		INNER JOIN #SumOfOrdersId B ON A.AnalyisisFunctionId = B.Id
	WHERE
		A.AnalysisFunctionTimePeriodId = @SalesPeriod
	GROUP BY
		A.CustomerMarketPlaceId

	---- TEMP FOLDER WITH STORES ANUAL SALES INC CUSTOMER ID ----
	SELECT
		A.CustomerMarketPlaceId,
		A.UpdatedDate,
		ROUND(B.ValueFloat, 1) AS AnualSales,
		C.CustomerId
	INTO
		#temp1
	FROM
		#MP_Stores A
		INNER JOIN MP_AnalyisisFunctionValues B
			ON A.CustomerMarketPlaceId = b.CustomerMarketPlaceId
			AND A.UpdatedDate = b.Updated
		INNER JOIN MP_CustomerMarketPlace C ON A.CustomerMarketPlaceId = C.Id
		INNER JOIN #SumOfOrdersId D ON B.AnalyisisFunctionId = D.Id
	WHERE
		B.AnalysisFunctionTimePeriodId = 4
	ORDER BY
		1

	---- JOIN TEMP1 WITH CUSTOMER TABLE ----
	SELECT
		C.Id,
		C.Name AS EmailAddress,
		C.FirstName,
		C.Surname,
		A.CustomerMarketPlaceId,
		A.UpdatedDate,
		A.AnualSales,
		C.WizardStep
	INTO
		#temp2
	FROM
		#temp1 A
		JOIN Customer C ON C.Id = A.CustomerId
	WHERE
		C.Name NOT like '%ezbob%' 
		AND
		C.Name NOT LIKE '%liatvanir%'
		AND
		C.Name NOT LIKE '%@test%'
		AND
		C.IsTest = 0
	GROUP BY
		A.CustomerMarketPlaceId,
		A.UpdatedDate,
		A.AnualSales,
		C.Id,C.Name,
		C.FirstName,
		C.Surname,
		C.WizardStep
	ORDER BY
		1

	---- TEMP TABLE WITH CUSTOMER DETAILS ----
	SELECT
		C.Id,
		C.Name,
		C.GreetingMailSentDate,
		C.FirstName,
		C.Surname,
		C.WizardStep
	INTO
		#temp3
	FROM
		Customer C
	WHERE
		C.Name NOT like '%ezbob%' 
		AND 
		C.Name NOT LIKE '%liatvanir%'
		AND
		C.Name NOT LIKE '%@test%'
		AND
		C.IsTest = 0

	---- FINAL TABLE WITH CUSTID, STORES & ANUAL SALES ----
	SELECT
		A.Id AS CustomerId,
		A.WizardStep,	
		C.Shops AS NumOfStores,
		CASE
			WHEN D.Paypal >= 1 THEN 'Y'
			ELSE 'N'
		END AS HasPaypal,
		SUM(B.AnualSales) AS AnualSales,
		ROUND((SUM(B.AnualSales) * 0.06), -2) AS OfferAmount
	FROM
		#temp3 A
		LEFT JOIN #temp2 B ON A.Id = B.Id
		LEFT JOIN #Shops C ON C.CustomerId = A.Id
		LEFT JOIN #Paypal D ON D.CustomerId = A.Id
	WHERE
		CONVERT(DATE, @DateStart) <= A.GreetingMailSentDate AND A.GreetingMailSentDate < CONVERT(DATE, @DateEnd)
	GROUP BY
		A.Id,
		A.WizardStep,
		C.Shops,
		D.Paypal

	DROP TABLE #SumOfOrdersId
	DROP TABLE #Shops
	DROP TABLE #Paypal
	DROP TABLE #MP_Stores
	DROP TABLE #temp1
	DROP TABLE #temp2
	DROP TABLE #temp3
END

GO
/****** Object:  StoredProcedure [dbo].[RptCustomerReport]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptCustomerReport]
@DateStart DATETIME
AS
BEGIN
	SELECT
		Name,
		CASE WHEN WizardStep=4 THEN 1 ELSE 0 END AS IsSuccessfullyRegistered,
		AccountNumber,
		Status,
		PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), CreditSum)), 1), 2) CreditSum,
		ReferenceSource
	FROM
		Customer
	WHERE
		CONVERT(DATE, @DateStart) <= GreetingMailSentDate
END

GO
/****** Object:  StoredProcedure [dbo].[RptDailyStats]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptDailyStats]
@DateStart DATETIME,
@DateEnd   DATETIME
AS
BEGIN
	SELECT
		@DateStart = CONVERT(DATE, @DateStart),
		@DateEnd = CONVERT(DATE, @DateEnd)

	IF OBJECT_ID('tempdb..#TodayClients') is not NULL
		DROP TABLE #TodayClients

	IF OBJECT_ID('tempdb..#PastClients') is not NULL
		DROP TABLE #PastClients

	IF OBJECT_ID('tempdb..#NewClients') is not NULL
		DROP TABLE #NewClients

	IF OBJECT_ID('tempdb..#NewOffers') is not NULL
		DROP TABLE #NewOffers

	IF OBJECT_ID('tempdb..#ReportPart1') is not NULL
		DROP TABLE #ReportPart1

	IF OBJECT_ID('tempdb..#ReportPart2') is not NULL
		DROP TABLE #ReportPart2

	SELECT DISTINCT
		IdCustomer
	INTO
		#TodayClients
	FROM
		CashRequests
	WHERE
		@DateStart <= CreationDate AND CreationDate < @DateEnd

	SELECT DISTINCT
		C.IdCustomer
	INTO
		#PastClients
	FROM
		CashRequests C
		INNER JOIN #TodayClients T ON C.IdCustomer = T.IdCustomer
	WHERE
		CreationDate < @DateStart

	SELECT
		t.IdCustomer
	INTO
		#NewClients
	FROM
		#TodayClients t
		LEFT JOIN #PastClients p ON t.IdCustomer = p.IdCustomer
	WHERE
		p.IdCustomer IS NULL

	SELECT
		C.IdCustomer,
		MIN(Id) OfferId
	INTO
		#NewOffers
	FROM
		CashRequests C
		INNER JOIN #NewClients N ON C.IdCustomer = N.IdCustomer
	GROUP BY
		c.IdCustomer

	SELECT
		'Applications' Line,
		'Total' Type,
		COUNT(1) Counter,
		UnderwriterDecision,
		PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), SUM(ManagerApprovedSum))), 1), 2) Value
	INTO
		#ReportPart1
	FROM
		CashRequests
	WHERE
		@DateStart <= CreationDate AND CreationDate < @DateEnd
	GROUP BY
		UnderwriterDecision

	INSERT INTO #ReportPart1
	SELECT
		'Applications' Line,
		'New' Type,
		COUNT(1) Counter,
		UnderwriterDecision,
		PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), SUM(ManagerApprovedSum))), 1), 2) Value
	FROM
		CashRequests
		INNER JOIN #NewOffers ON CashRequests.Id = #NewOffers.OfferId
	WHERE
		@DateStart <= CreationDate AND CreationDate < @DateEnd
	GROUP BY
		UnderwriterDecision

	INSERT INTO #ReportPart1
	SELECT
		'Applications' Line,
		'Old' Type,
		COUNT(1) Counter,
		UnderwriterDecision,
		PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), SUM(ManagerApprovedSum))), 1), 2) Value
	FROM
		CashRequests
		LEFT JOIN #NewOffers ON CashRequests.Id = #NewOffers.OfferId
	WHERE
		@DateStart <= CreationDate AND CreationDate < @DateEnd
		AND
		#NewOffers.OfferId IS NULL
	GROUP BY
		UnderwriterDecision

	SELECT
		'Loans' Line,
		'Total' Type,
		COUNT(DISTINCT Id) Counter,
		'' AS UnderwriterDescision,
		PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), SUM(LoanAmount))), 1), 2) Value
	INTO
		#ReportPart2
	FROM
		Loan
	WHERE
		@DateStart <= [Date] AND [Date] < @DateEnd

	INSERT INTO #ReportPart2
	SELECT
		'Loans' Line,
		'Old' Type,
		COUNT(DISTINCT l.Id) Counter,
		'' AS UnderwriterDescision,
		PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), SUM(l.LoanAmount))), 1), 2) Value
	FROM
		Loan l
		INNER JOIN Loan old ON l.CustomerId = old.CustomerID AND old.[Date] < @DateStart
	WHERE
		@DateStart <= l.[Date] AND l.[Date] < @DateEnd

	INSERT INTO #ReportPart2
	SELECT
		'Loans' Line,
		'New' Type,
		COUNT(DISTINCT l.Id) Counter,
		'' AS UnderwriterDescision,
		PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), SUM(l.LoanAmount))), 1), 2) Value
	FROM
		Loan l
		LEFT JOIN Loan old ON l.CustomerId = old.CustomerID AND old.[Date] < @DateStart
	WHERE
		old.Id IS NULL
		AND
		@DateStart <= l.[Date] AND l.[Date] < @DateEnd

	SELECT
		*
	FROM
		#ReportPart1

	UNION

	SELECT
		*
	FROM
		#ReportPart2

	UNION

	SELECT
		'Payments' Line,
		'' Type,
		COUNT(1) Counter,
		'' AS UnderwriterDescision,
		PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), sum(Amount))), 1), 2) Value
	FROM
		LoanTransaction
	WHERE
		@DateStart  <= PostDate AND PostDate < @DateEnd
		AND
		Type = 'PaypointTransaction'
		AND
		Description = 'payment from customer'

	UNION

	SELECT
		'Registers' Line,
		'' Type,
		COUNT(1) Counter,
		'' AS UnderwriterDescision,
		'0' Value
	FROM
		Customer
	WHERE
		@DateStart <= GreetingMailSentDate AND GreetingMailSentDate < @DateEnd

	DROP TABLE #TodayClients
	DROP TABLE #PastClients
	DROP TABLE #NewClients
	DROP TABLE #NewOffers
	DROP TABLE #ReportPart1
	DROP TABLE #ReportPart2
END

GO
/****** Object:  StoredProcedure [dbo].[RptDidntTakeLoan]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptDidntTakeLoan]
AS
BEGIN
	SET NOCOUNT ON

	IF OBJECT_ID('tempdb..#tmp1') IS NOT NULL
		DROP TABLE #tmp1

	IF OBJECT_ID('tempdb..#tmp2') IS NOT NULL
		DROP TABLE #tmp2

	SELECT
		IdCustomer,
		CreationDate,
		UnderwriterDecision,
		UnderwriterDecisionDate,
		ManagerApprovedSum,
		InterestRate,
		RepaymentPeriod,
		LoanTypeId
	INTO
		#tmp1
	FROM
		dbo.CashRequests

	SELECT
		L.CustomerId,
		SUM(L.LoanAmount) AS LoanAmount
	INTO
		#tmp2
	FROM
		Loan L
	GROUP BY
		L.CustomerId

	SELECT
		C.Id,
		C.Name,
		CONVERT(DATE, C.GreetingMailSentDate) AS SignUpDate,
		C.FirstName,
		C.Surname,
		C.DaytimePhone,
		C.MobilePhone,
		C.LimitedBusinessPhone,
		C.NonLimitedBusinessPhone,
		T1.UnderwriterDecisionDate AS ApprovedDate,
		T1.ManagerApprovedSum,
		T1.InterestRate,
		T1.RepaymentPeriod,
		CASE T1.LoanTypeId
			WHEN 2 THEN 'HalfWay Loan'
			WHEN 1 THEN 'Standard Loan'
		END LoanType
	FROM
		Customer C
		LEFT JOIN #tmp1 T1 ON T1.IdCustomer = C.Id
		LEFT JOIN #tmp2 T2 ON T2.CustomerId = C.Id
	WHERE
		C.IsTest = 0
		AND
		C.Name NOT like '%ezbob%'
		AND
		C.Name NOT LIKE '%liatvanir%'
		AND
		C.Name NOT LIKE '%test@%'
		AND
		T1.UnderwriterDecision = 'Approved'
		AND
		T2.LoanAmount IS NULL
		AND
		T1.ManagerApprovedSum IS NOT NULL
	GROUP BY
		C.Id,
		C.Name,
		C.GreetingMailSentDate,
		C.FirstName,
		C.Surname,
		C.DaytimePhone,
		C.MobilePhone,
		C.LimitedBusinessPhone,
		C.NonLimitedBusinessPhone,
		T1.UnderwriterDecision,
		T1.UnderwriterDecisionDate,
		T1.ManagerApprovedSum,
		T1.InterestRate,
		T1.RepaymentPeriod,
		T1.LoanTypeId

	DROP TABLE #tmp1
	DROP TABLE #tmp2

	SET NOCOUNT OFF
END

GO
/****** Object:  StoredProcedure [dbo].[RptEarnedInterest]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptEarnedInterest]
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	SELECT
		l.Date AS IssueDate,
		c.Id AS ClientID,
		l.Id AS LoanID,
		c.Fullname AS ClientName,
		c.Name AS ClientEmail,
		la.Amount AS LoanAmount,
		ISNULL(SUM(t.Amount), 0) AS TotalRepaid,
		ISNULL(SUM(t.LoanRepayment), 0) AS PrincipalRepaid
	FROM
		Loan l
		INNER JOIN Customer c ON l.CustomerID = c.Id
		INNER JOIN LoanTransaction la
			ON l.Id = la.LoanId
			AND la.Type = 'PacnetTransaction'
			AND la.Status = 'Done'
		LEFT JOIN LoanTransaction t
			ON l.ID = t.LoanId
			AND t.Type = 'PaypointTransaction'
			AND t.Status = 'Done'
	GROUP BY
		l.Date,
		c.Id,
		l.ID,
		c.Fullname,
		c.Name,
		la.Amount
END

GO
/****** Object:  StoredProcedure [dbo].[RptEarnedInterest_ForPeriod]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptEarnedInterest_ForPeriod]
@DateStart DATE,
@DateEnd DATE
AS
SELECT
	l.Id AS LoanID,
	CONVERT(DATE, l.Date) AS IssueDate,
	t.Amount
FROM
	Loan l
	INNER JOIN Customer c
		ON l.CustomerId = c.Id
		AND c.IsTest = 0
	INNER JOIN LoanTransaction t
		ON l.Id = t.LoanId
		AND t.Status = 'Done'
		AND t.Type = 'PacnetTransaction'
WHERE
	l.Date < @DateEnd
UNION
SELECT
	l.Id AS LoanID,
	CONVERT(DATE, l.Date) AS IssueDate,
	t.Amount
FROM
	Loan l
	INNER JOIN LoanSchedule s
		ON l.Id = s.LoanId
	INNER JOIN Customer c
		ON l.CustomerId = c.Id
		AND c.IsTest = 0
	INNER JOIN LoanTransaction t
		ON l.Id = t.LoanId
		AND t.Status = 'Done'
		AND t.Type = 'PacnetTransaction'
WHERE
	@DateStart <= s.Date

GO
/****** Object:  StoredProcedure [dbo].[RptEarnedInterest_IssuedLoans]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptEarnedInterest_IssuedLoans]
@DateStart DATE,
@DateEnd DATE
AS
SELECT
	l.Id AS LoanID,
	CONVERT(DATE, l.Date) AS IssueDate,
	t.Amount
FROM
	Loan l
	INNER JOIN Customer c
		ON l.CustomerId = c.Id
		AND c.IsTest = 0
	INNER JOIN LoanTransaction t
		ON l.Id = t.LoanId
		AND t.Status = 'Done'
		AND t.Type = 'PacnetTransaction'
WHERE
	@DateStart <= l.Date AND l.Date < @DateEnd

GO
/****** Object:  StoredProcedure [dbo].[RptEarnedInterest_LoanDates]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptEarnedInterest_LoanDates]
AS
SELECT
	CONVERT(INT, 0),
	s.LoanId,
	CONVERT(DATE, s.Date),
	s.InterestRate
FROM
	LoanSchedule s
	INNER JOIN Loan l ON s.LoanId = l.Id
	INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
UNION
SELECT
	CONVERT(INT, 1),
	t.LoanId,
	CONVERT(DATE, t.PostDate),
	SUM(t.LoanRepayment)
FROM
	LoanTransaction t
	INNER JOIN Loan l ON t.LoanId = l.Id
	INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
WHERE
	t.Status = 'Done'
	AND
	t.Type = 'PaypointTransaction'
	AND
	t.LoanRepayment > 0
GROUP BY
	t.LoanId,
	CONVERT(DATE, t.PostDate)
ORDER BY
	2, 3, 1

GO
/****** Object:  StoredProcedure [dbo].[RptExecutive]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptExecutive]
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	SELECT
		@DateStart = CONVERT(DATE, @DateStart),
		@DateEnd = CONVERT(DATE, @DateEnd)

	------------------------------------------------------------------------------

	DECLARE @TotalGivenLoanCountClose NUMERIC(18, 2)
	DECLARE @TotalGivenLoanValueClose NUMERIC(18, 2)
	DECLARE @TotalRepaidPrincipalClose NUMERIC(18, 2)

	DECLARE @PACNET NVARCHAR(32) = 'PacnetTransaction'
	DECLARE @PAYPOINT NVARCHAR(32) = 'PaypointTransaction'
	DECLARE @DONE NVARCHAR(4) = 'Done'
	DECLARE @Indent NVARCHAR(48) = '&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;'

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	CREATE TABLE #out (
		SortOrder INT IDENTITY(1, 1) NOT NULL,
		Caption NVARCHAR(300) NOT NULL,
		Number SQL_VARIANT,    -- INT
		Amount SQL_VARIANT,    -- DECIMAL(18, 2)
		Principal SQL_VARIANT, -- DECIMAL(18, 2)
		Interest SQL_VARIANT,  -- DECIMAL(18, 2)
		Fees SQL_VARIANT,      -- DECIMAL(18, 2)
		Css NVARCHAR(256) NULL
	)

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #out(Caption, Number, Amount, Principal, Interest, Fees, Css)
		VALUES ('Visitors', 'Number', 'Amount', 'Principal', 'Interest', 'Fees', 'total')

	------------------------------------------------------------------------------

	SELECT
		0 AS SortOrder,
		Name
	INTO
		#t
	FROM
		SiteAnalyticsCodes
	WHERE
		1 = 0

	------------------------------------------------------------------------------

	INSERT INTO #t (SortOrder, Name) VALUES (1, 'UKVisitors')
	INSERT INTO #t (SortOrder, Name) VALUES (2, 'ReturningVisitors')
	INSERT INTO #t (SortOrder, Name) VALUES (3, 'NewVisitors')
	INSERT INTO #t (SortOrder, Name) VALUES (4, 'PageDashboard')
	INSERT INTO #t (SortOrder, Name) VALUES (5, 'PageLogon')
	INSERT INTO #t (SortOrder, Name) VALUES (6, 'PagePacnet')
	INSERT INTO #t (SortOrder, Name) VALUES (7, 'PageGetCash')

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number)
	SELECT
		c.Description,
		ISNULL(SUM(ISNULL(a.SiteAnalyticsValue, 0)), 0)
	FROM
		#t
		LEFT JOIN SiteAnalyticsCodes c ON #t.Name = c.Name
		LEFT JOIN SiteAnalytics a ON c.Id = a.SiteAnalyticsCode
	WHERE
		c.Name = #t.Name
		AND
		@DateStart <= a.Date AND a.Date < @DateEnd
	GROUP BY
		c.Description,
		#t.SortOrder
	ORDER BY
		#t.SortOrder

	DROP TABLE #t

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #out(Caption, Number, Amount, Principal, Interest, Fees, Css)
		VALUES ('Funnel', 'Number', 'Amount', 'Principal', 'Interest', 'Fees', 'total')

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number)
	SELECT
		'Registrations',
		ISNULL(COUNT(*), 0)
	FROM
		Customer c
	WHERE
		c.IsTest = 0
		AND
		@DateStart <= c.GreetingMailSentDate AND c.GreetingMailSentDate < @DateEnd

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number)
	SELECT
		'Entered data source',
		ISNULL(COUNT(DISTINCT m.CustomerId), 0)
	FROM
		MP_CustomerMarketPlace m
		INNER JOIN Customer c ON m.CustomerId = c.Id
	WHERE
		c.IsTest = 0
		AND
		@DateStart <= c.GreetingMailSentDate AND c.GreetingMailSentDate < @DateEnd

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number)
	SELECT
		'Finished',
		ISNULL(COUNT(*), 0)
	FROM
		Customer c
	WHERE
		c.WizardStep >= 3
		AND
		c.IsTest = 0
		AND
		@DateStart <= c.GreetingMailSentDate AND c.GreetingMailSentDate < @DateEnd

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number)
	SELECT
		'Approved',
		ISNULL(COUNT(DISTINCT r.IdCustomer), 0)
	FROM
		Customer c
		INNER JOIN CashRequests r ON c.Id = r.IdCustomer
	WHERE
		r.UnderwriterDecision = 'Approved'
		AND
		c.IsTest = 0
		AND
		@DateStart <= c.GreetingMailSentDate AND c.GreetingMailSentDate < @DateEnd

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number)
	SELECT
		'Rejected',
		ISNULL(COUNT(DISTINCT r.IdCustomer), 0)
	FROM
		Customer c
		INNER JOIN CashRequests r ON c.Id = r.IdCustomer
	WHERE
		r.UnderwriterDecision = 'Rejected'
		AND
		NOT EXISTS (
			SELECT acr.Id
			FROM CashRequests acr
			WHERE acr.IdCustomer = r.IdCustomer
			AND acr.UnderwriterDecision = 'Approved'
		)
		AND
		c.IsTest = 0
		AND
		@DateStart <= c.GreetingMailSentDate AND c.GreetingMailSentDate < @DateEnd

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number)
	SELECT
		'Pending',
		ISNULL(COUNT(DISTINCT r.IdCustomer), 0)
	FROM
		Customer c
		INNER JOIN CashRequests r ON c.Id = r.IdCustomer
	WHERE
		NOT EXISTS (
			SELECT acr.Id
			FROM CashRequests acr
			WHERE acr.IdCustomer = r.IdCustomer
			AND acr.UnderwriterDecision IN ('Approved', 'Rejected')
		)
		AND
		c.IsTest = 0
		AND
		@DateStart <= c.GreetingMailSentDate AND c.GreetingMailSentDate < @DateEnd

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #out(Caption, Number, Amount, Principal, Interest, Fees, Css)
		VALUES ('Daily Approvals', 'Number', 'Amount', 'Principal', 'Interest', 'Fees', 'total')

	------------------------------------------------------------------------------

	SELECT
		r.IdCustomer AS CustomerID,
		ISNULL(COUNT(DISTINCT r.Id), 0) AS RequestCount,
		ISNULL(MAX(r.ManagerApprovedSum), 0) AS MaxApprovedSum,
		CONVERT(INT, 0) AS OldRequestCount
	INTO
		#cr
	FROM
		CashRequests r
		INNER JOIN Customer c ON r.IdCustomer = c.Id AND c.IsTest = 0
	WHERE
		@DateStart <= r.UnderwriterDecisionDate AND r.UnderwriterDecisionDate < @DateEnd
		AND
		r.UnderwriterDecision = 'Approved'
	GROUP BY
		r.IdCustomer

	------------------------------------------------------------------------------

	UPDATE #cr SET
		OldRequestCount = ISNULL((
			SELECT COUNT(DISTINCT Id)
			FROM CashRequests old
			WHERE old.IdCustomer = #cr.CustomerID
			AND old.UnderwriterDecisionDate < @DateStart
		), 0)

	------------------------------------------------------------------------------

	INSERT INTO #out(Caption, Number, Amount)
	SELECT
		'Total',
		ISNULL(COUNT(DISTINCT CustomerID), 0),
		ISNULL(SUM(MaxApprovedSum), 0)
	FROM
		#cr

	------------------------------------------------------------------------------

	INSERT INTO #out(Caption, Number, Amount)
	SELECT
		'New',
		ISNULL(COUNT(DISTINCT CustomerID), 0),
		ISNULL(SUM(MaxApprovedSum), 0)
	FROM
		#cr
	WHERE
		OldRequestCount = 0

	------------------------------------------------------------------------------

	INSERT INTO #out(Caption, Number, Amount)
	SELECT
		'Old',
		ISNULL(COUNT(DISTINCT CustomerID), 0),
		ISNULL(SUM(MaxApprovedSum), 0)
	FROM
		#cr
	WHERE
		OldRequestCount != 0

	------------------------------------------------------------------------------

	DROP TABLE #cr

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #out(Caption, Number, Amount, Principal, Interest, Fees, Css)
		VALUES ('Issued Loans', 'Number', 'Amount', 'Principal', 'Interest', 'Fees', 'total')

	------------------------------------------------------------------------------

	CREATE TABLE #l (
		LoanID INT,
		CustomerID INT,
		LoanAmount DECIMAL(18, 2),
		PreviousLoansCount INT,
		PaidOffLoansCount INT
	)

	------------------------------------------------------------------------------

	INSERT INTO #l (LoanID, CustomerID, LoanAmount, PreviousLoansCount, PaidOffLoansCount)
	SELECT
		l.Id,
		l.CustomerId,
		t.Amount,
		0,
		0
	FROM
		Customer c
		INNER JOIN Loan l ON c.Id = l.CustomerId AND c.IsTest = 0
		INNER JOIN LoanTransaction t ON l.Id = t.LoanId
	WHERE
		t.Status = @DONE AND t.Type = @PACNET
		AND
		@DateStart <= l.Date AND l.Date < @DateEnd

	------------------------------------------------------------------------------

	UPDATE #l SET
		PreviousLoansCount = (
			SELECT ISNULL(COUNT(*), 0)
			FROM Loan l
			WHERE #l.CustomerID = l.CustomerId
			AND #l.LoanID > l.Id
		)

	------------------------------------------------------------------------------

	UPDATE #l SET
		PaidOffLoansCount = (
			SELECT ISNULL(COUNT(*), 0)
			FROM Loan l
			WHERE #l.CustomerID = l.CustomerId
			AND #l.LoanID != l.Id
			AND l.Status = 'PaidOff'
		)

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number, Amount)
	SELECT
		'Total',
		ISNULL(COUNT(*), 0),
		ISNULL(SUM(ISNULL(LoanAmount, 0)), 0)
	FROM
		#l

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number, Amount)
	SELECT
		@Indent + 'New loans',
		ISNULL(COUNT(*), 0),
		ISNULL(SUM(ISNULL(LoanAmount, 0)), 0)
	FROM
		#l
	WHERE
		PreviousLoansCount = 0

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number, Amount)
	SELECT
		@Indent + 'Existing loans',
		ISNULL(COUNT(*), 0),
		ISNULL(SUM(ISNULL(LoanAmount, 0)), 0)
	FROM
		#l
	WHERE
		PreviousLoansCount != 0

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number, Amount)
	SELECT
		@Indent + @Indent + 'Existing fully paid',
		ISNULL(COUNT(*), 0),
		ISNULL(SUM(ISNULL(LoanAmount, 0)), 0)
	FROM
		#l
	WHERE
		PreviousLoansCount != 0
		AND
		PaidOffLoansCount = PreviousLoansCount

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number, Amount)
	SELECT
		@Indent + @Indent + 'Existing open loans',
		ISNULL(COUNT(*), 0),
		ISNULL(SUM(ISNULL(LoanAmount, 0)), 0)
	FROM
		#l
	WHERE
		PreviousLoansCount != 0
		AND
		PaidOffLoansCount != PreviousLoansCount

	------------------------------------------------------------------------------

	DROP TABLE #l

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	CREATE TABLE #known_tran_time_status (
		TransactionID INT,
		Status INT
	)

	------------------------------------------------------------------------------

	INSERT INTO #known_tran_time_status
	SELECT DISTINCT
		TransactionID,
		CASE StatusBefore
			WHEN 'Late' THEN 1
			WHEN 'Paid' THEN 1
			WHEN 'PaidEarly' THEN 2
			ELSE 3
		END
	FROM
		LoanScheduleTransaction
	UNION
	SELECT DISTINCT
		TransactionID,
		CASE StatusAfter
			WHEN 'Late' THEN 1
			WHEN 'Paid' THEN 1
			WHEN 'PaidEarly' THEN 2
			ELSE 3
		END
	FROM
		LoanScheduleTransaction

	------------------------------------------------------------------------------

	SELECT
		TransactionID,
		MIN(Status) AS Status
	INTO
		#grouped_tran_time_status
	FROM
		#known_tran_time_status
	GROUP BY
		TransactionID

	------------------------------------------------------------------------------

	SELECT
		t.Id AS TransactionID,
		ISNULL(g.Status, 3) AS Status
	INTO
		#tran_time_status
	FROM
		LoanTransaction t
		LEFT JOIN #grouped_tran_time_status g ON t.Id = g.TransactionID

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #out(Caption, Number, Amount, Principal, Interest, Fees, Css)
		VALUES ('Repayments', 'Number', 'Amount', 'Principal', 'Interest', 'Fees', 'total')

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number, Amount, Principal, Interest, Fees)
	SELECT
		'Total',
		ISNULL(COUNT(t.Id), 0),
		ISNULL(SUM(t.Amount), 0),
		ISNULL(SUM(t.LoanRepayment), 0),
		ISNULL(SUM(t.Interest), 0),
		ISNULL(SUM(t.Fees), 0)
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanID = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		@DateStart <= t.PostDate AND t.PostDate < @DateEnd
		AND
		t.Type = @PAYPOINT AND t.Status = @DONE

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number, Amount, Principal, Interest, Fees)
	SELECT
		'Early payments',
		ISNULL(COUNT(t.Id), 0),
		ISNULL(SUM(t.Amount), 0),
		ISNULL(SUM(t.LoanRepayment), 0),
		ISNULL(SUM(t.Interest), 0),
		ISNULL(SUM(t.Fees), 0)
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanID = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
		INNER JOIN #tran_time_status s ON t.Id = s.TransactionID AND s.Status = 2
	WHERE
		@DateStart <= t.PostDate AND t.PostDate < @DateEnd
		AND
		t.Type = @PAYPOINT AND t.Status = @DONE

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number, Amount, Principal, Interest, Fees)
	SELECT
		'On time payments',
		ISNULL(COUNT(t.Id), 0),
		ISNULL(SUM(t.Amount), 0),
		ISNULL(SUM(t.LoanRepayment), 0),
		ISNULL(SUM(t.Interest), 0),
		ISNULL(SUM(t.Fees), 0)
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanID = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
		INNER JOIN #tran_time_status s ON t.Id = s.TransactionID AND s.Status = 3
	WHERE
		@DateStart <= t.PostDate AND t.PostDate < @DateEnd
		AND
		t.Type = @PAYPOINT AND t.Status = @DONE

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number, Amount, Principal, Interest, Fees)
	SELECT
		'Late payments',
		ISNULL(COUNT(t.Id), 0),
		ISNULL(SUM(t.Amount), 0),
		ISNULL(SUM(t.LoanRepayment), 0),
		ISNULL(SUM(t.Interest), 0),
		ISNULL(SUM(t.Fees), 0)
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanID = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
		INNER JOIN #tran_time_status s ON t.Id = s.TransactionID AND s.Status = 1
	WHERE
		@DateStart <= t.PostDate AND t.PostDate < @DateEnd
		AND
		t.Type = @PAYPOINT AND t.Status = @DONE

	------------------------------------------------------------------------------

	DROP TABLE #tran_time_status
	DROP TABLE #grouped_tran_time_status
	DROP TABLE #known_tran_time_status

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number, Amount)
	SELECT
		'Loans paid fully',
		ISNULL(COUNT(DISTINCT l.Id), 0),
		ISNULL(SUM(l.LoanAmount), 0)
	FROM
		Loan l
		INNER JOIN Customer c ON l.CustomerId = c.Id
	WHERE
		c.IsTest = 0
		AND
		@DateStart <= l.DateClosed AND l.DateClosed < @DateEnd

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #out(Caption, Number, Amount, Principal, Interest, Fees, Css)
		VALUES ('Total Book', 'Number', 'Amount', 'Principal', 'Interest', 'Fees', 'total')

	------------------------------------------------------------------------------

	SELECT
		@TotalGivenLoanCountClose = ISNULL( COUNT(DISTINCT t.LoanId), 0 ),
		@TotalGivenLoanValueClose = ISNULL( SUM(ISNULL(t.Amount, 0)), 0 )
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		t.Type = @PACNET AND t.Status = @DONE
		AND
		t.PostDate < @DateEnd

	------------------------------------------------------------------------------

	SELECT
		@TotalRepaidPrincipalClose = ISNULL( SUM(ISNULL(t.LoanRepayment, 0)), 0 )
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		t.Type = @PAYPOINT AND t.Status = @DONE
		AND
		t.PostDate < @DateEnd

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number, Amount)
	SELECT
		'Total issued loans',
		@TotalGivenLoanCountClose,
		@TotalGivenLoanValueClose

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number, Amount, Principal, Interest, Fees)
	SELECT
		'Total repayments',
		ISNULL(COUNT(DISTINCT t.Id), 0),
		ISNULL(SUM(t.Amount), 0),
		ISNULL(SUM(t.LoanRepayment), 0),
		ISNULL(SUM(t.Interest), 0),
		ISNULL(SUM(t.Fees), 0)
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		t.Type = @PAYPOINT AND t.Status = @DONE
		AND
		t.PostDate < @DateEnd

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Number, Amount)
	SELECT
		'Outstanding balance',
		@TotalGivenLoanCountClose,
		@TotalGivenLoanValueClose - @TotalRepaidPrincipalClose

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		Caption,
		Number,
		Amount,
		Principal,
		Interest,
		Fees,
		Css
	FROM
		#out
	ORDER BY
		SortOrder

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	DROP TABLE #out
END

GO
/****** Object:  StoredProcedure [dbo].[RptFinancialStats]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptFinancialStats]
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	DECLARE @TotalGivenLoanValueOpen NUMERIC(18, 2)
	DECLARE @TotalRepaidPrincipalOpen NUMERIC(18, 2)
	
	DECLARE @TotalGivenLoanValueClose NUMERIC(18, 2)
	DECLARE @TotalRepaidPrincipalClose NUMERIC(18, 2)

	DECLARE @TotalBalanceSum NUMERIC(18, 2)

	DECLARE @InterestReceived NUMERIC(18, 2)

	DECLARE @Defaults NUMERIC(18, 2)
	DECLARE @SetupFee NUMERIC(18, 2)

	DECLARE @PACNET NVARCHAR(32)
	DECLARE @PAYPOINT NVARCHAR(32)
	DECLARE @DONE NVARCHAR(4)

	DECLARE @Indent NVARCHAR(64)

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		@DateStart = CONVERT(DATE, @DateStart),
		@DateEnd = CONVERT(DATE, @DateEnd),
		@PACNET = 'PacnetTransaction',
		@PAYPOINT = 'PaypointTransaction',
		@DONE = 'Done',
		@Indent = '&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;'

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	CREATE TABLE #output (
		SortOrder NUMERIC(18, 6) NOT NULL,
		Caption NVARCHAR(128) NOT NULL,
		Value NUMERIC(18, 2) NULL
	)

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		@TotalGivenLoanValueOpen = ISNULL( SUM(ISNULL(t.Amount, 0)), 0 )
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		t.Type = @PACNET AND t.Status = @DONE
		AND
		t.PostDate < @DateStart

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		@TotalGivenLoanValueClose = ISNULL( SUM(ISNULL(t.Amount, 0)), 0 )
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		t.Type = @PACNET AND t.Status = @DONE
		AND
		t.PostDate < @DateEnd

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		@TotalRepaidPrincipalOpen = ISNULL( SUM(ISNULL(t.LoanRepayment, 0)), 0 )
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		t.Type = @PAYPOINT AND t.Status = @DONE
		AND
		t.PostDate < @DateStart

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		@TotalRepaidPrincipalClose = ISNULL( SUM(ISNULL(t.LoanRepayment, 0)), 0 )
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		t.Type = @PAYPOINT AND t.Status = @DONE
		AND
		t.PostDate < @DateEnd

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SET @TotalBalanceSum = 
		@TotalGivenLoanValueOpen  - @TotalRepaidPrincipalOpen +
		@TotalGivenLoanValueClose - @TotalRepaidPrincipalClose
		
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		@InterestReceived = ISNULL(SUM(t.Interest), 0)
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		t.Type = @PAYPOINT AND t.Status = @DONE
		AND
		@DateStart <= t.PostDate AND t.PostDate < @DateEnd

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		@Defaults = ISNULL(SUM(t.Amount), 0)
	FROM
		Loan l
		INNER JOIN LoanTransaction t
			ON l.Id = t.LoanId
			AND t.Status = @DONE
			AND t.Type = @PACNET
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0 AND c.CollectionStatus = 4
	WHERE
		@DateStart <= l.Date AND l.Date < @DateEnd

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		@SetupFee = ISNULL(SUM(l.SetupFee), 0)
	FROM
		Loan l
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		@DateStart <= l.Date AND l.Date < @DateEnd

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	CREATE TABLE #known_tran_time_status (
		TransactionID INT,
		Status INT
	)

	------------------------------------------------------------------------------

	INSERT INTO #known_tran_time_status
	SELECT DISTINCT
		TransactionID,
		CASE StatusBefore
			WHEN 'Late' THEN 1
			WHEN 'Paid' THEN 1
			WHEN 'PaidEarly' THEN 2
			ELSE 3
		END
	FROM
		LoanScheduleTransaction
	UNION
	SELECT DISTINCT
		TransactionID,
		CASE StatusAfter
			WHEN 'Late' THEN 1
			WHEN 'Paid' THEN 1
			WHEN 'PaidEarly' THEN 2
			ELSE 3
		END
	FROM
		LoanScheduleTransaction

	------------------------------------------------------------------------------

	SELECT
		TransactionID,
		MIN(Status) AS Status
	INTO
		#grouped_tran_time_status
	FROM
		#known_tran_time_status
	GROUP BY
		TransactionID

	------------------------------------------------------------------------------

	SELECT
		t.Id AS TransactionID,
		ISNULL(g.Status, 3) AS Status
	INTO
		#tran_time_status
	FROM
		LoanTransaction t
		LEFT JOIN #grouped_tran_time_status g ON t.Id = g.TransactionID

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #output
	SELECT
		1,
		'Opening Balance',
		@TotalGivenLoanValueOpen - @TotalRepaidPrincipalOpen

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #output
	SELECT
		7,
		'Loans Issued #',
		ISNULL(COUNT(*), 0)
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		t.Type = @PACNET AND t.Status = @DONE
		AND
		@DateStart <= t.PostDate AND t.PostDate < @DateEnd

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #output
	SELECT
		2,
		'Loans Issued Value',
		ISNULL( SUM(ISNULL(t.Amount, 0)), 0 )
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		t.Type = @PACNET AND t.Status = @DONE
		AND
		@DateStart <= t.PostDate AND t.PostDate < @DateEnd

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #output
	SELECT
		3.1,
		@Indent + 'Principal Repaid Early',
		ISNULL( SUM(ISNULL(LoanRepayment, 0)), 0)
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
		INNER JOIN #tran_time_status s ON t.Id = s.TransactionID AND s.Status = 2
	WHERE
		@DateStart <= t.PostDate AND t.PostDate < @DateEnd
		AND
		t.Type = @PAYPOINT AND t.Status = @DONE

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #output
	SELECT
		3.2,
		@Indent + 'Principal Repaid On Time',
		ISNULL( SUM(ISNULL(LoanRepayment, 0)), 0)
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
		INNER JOIN #tran_time_status s ON t.Id = s.TransactionID AND s.Status = 3
	WHERE
		@DateStart <= t.PostDate AND t.PostDate < @DateEnd
		AND
		t.Type = @PAYPOINT AND t.Status = @DONE

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #output
	SELECT
		3.3,
		@Indent + 'Principal Repaid Late',
		ISNULL( SUM(ISNULL(LoanRepayment, 0)), 0)
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
		INNER JOIN #tran_time_status s ON t.Id = s.TransactionID AND s.Status = 1
	WHERE
		@DateStart <= t.PostDate AND t.PostDate < @DateEnd
		AND
		t.Type = @PAYPOINT AND t.Status = @DONE

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #output
	SELECT
		3,
		'Principal Repaid',
		SUM(Value)
	FROM
		#output
	WHERE
		SortOrder IN (3.1, 3.2, 3.3)

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #output
	SELECT
		5,
		'Defaults',
		@Defaults

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #output
	SELECT
		8,
		'Average Loan Amount',
		ISNULL(AVG(t.Amount), 0)
	FROM
		Loan l
		INNER JOIN LoanTransaction t
			ON l.Id = t.LoanId
			AND t.Status = 'Done'
			AND t.Type = 'PacnetTransaction'
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		@DateStart <= l.Date AND l.Date < @DateEnd

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #output
	SELECT
		9,
		'Interest Received',
		@InterestReceived

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #output
	SELECT
		10,
		'Yield %',
		CASE @TotalBalanceSum
			WHEN 0 THEN 0
			ELSE 100 * 2 * @InterestReceived / @TotalBalanceSum
		END

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #output
	SELECT
		11,
		'Fees Paid',
		@SetupFee + ISNULL(SUM(t.Fees), 0)
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		t.Type = @PAYPOINT AND t.Status = @DONE
		AND
		@DateStart <= t.PostDate AND t.PostDate < @DateEnd

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #output
	VALUES (11.1, 'Setup Fee', @SetupFee)

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		Name,
		CONVERT(DECIMAL(18, 6), 0) AS Charge
	INTO
		#c
	FROM
		ConfigurationVariables
	WHERE
		1 = 0

	------------------------------------------------------------------------------

	INSERT INTO #c
	SELECT
		cv.Name,
		CONVERT(DECIMAL(18, 2),
			ISNULL(SUM(CASE
				WHEN ISNULL(AmountPaid, 0) > 0 THEN
					CASE WHEN ISNULL(AmountPaid, 0) < ISNULL(Amount, 0) THEN ISNULL(AmountPaid, 0) ELSE Amount END
				ELSE 0
			END), 0)
		)
	FROM
		LoanCharges ch
		INNER JOIN Loan l ON ch.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
		INNER JOIN ConfigurationVariables cv ON ch.ConfigurationVariableId = cv.Id
	WHERE
		@DateStart <= ch.Date AND ch.Date < @DateEnd
		AND
		cv.Name LIKE '%Charge'
	GROUP BY
		cv.Name
	
	------------------------------------------------------------------------------

	INSERT INTO #output
	SELECT
		CONVERT(DECIMAL(18, 6), '12.' + cv.Value),
		@Indent + cv.Name,
		ISNULL(#c.Charge, 0)
	FROM
		ConfigurationVariables cv
		LEFT JOIN #c ON cv.Name = #c.Name
	WHERE
		cv.Name LIKE '%Charge'

	------------------------------------------------------------------------------

	INSERT INTO #output
	SELECT
		12,
		'Total charges',
		ISNULL(SUM(Charge), 0)
	FROM
		#c

	------------------------------------------------------------------------------

	DROP TABLE #c

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #output
	SELECT
		6,
		'Closing Balance',
		@TotalGivenLoanValueClose - @TotalRepaidPrincipalClose - @Defaults

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #output
	SELECT
		4,
		'Closing Balance before Defaults',
		@TotalGivenLoanValueClose - @TotalRepaidPrincipalClose

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		SortOrder,
		Caption,
		Value
	FROM
		#output
	ORDER BY
		SortOrder

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	DROP TABLE #tran_time_status
	DROP TABLE #grouped_tran_time_status
	DROP TABLE #known_tran_time_status
	DROP TABLE #output
END

GO
/****** Object:  StoredProcedure [dbo].[RptFinishedLoans]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptFinishedLoans]
@DateStart DATETIME,
@DateEnd   DATETIME
AS
BEGIN

SET @DateEnd = CONVERT(DATE, @DateEnd)
SET @DateStart = CONVERT(DATE, @DateStart)

--IF datediff(day, @DateStart, @DateEnd) = 1 SET @DateStart = dateadd(week, -1,@DateEnd)

---------------------------CRM Notes------------------------------ 
 IF OBJECT_ID('tempdb..#CRMNotes') IS NOT NULL DROP TABLE #CRMNotes
 SELECT
  max(CR.Id) CrmId,
  CR.CustomerId
INTO
#CRMNotes
 FROM
  CustomerRelations CR
  INNER JOIN CashRequests O
   ON O.IdCustomer = CR.CustomerId
   AND O.UnderwriterDecision = 'Approved'
  INNER JOIN CRMStatuses sts ON CR.StatusId = sts.Id
 GROUP BY
  CR.CustomerId
  
-------------------------------CRM Final--------------------------
 IF OBJECT_ID('tempdb..#CRMFinal') IS NOT NULL DROP TABLE #CRMFinal
 SELECT
  CR.CustomerId,
  CR.UserName,
  sts.Name CRMStatus,
  CR.Comment CRMComment,
  act.Name CRMAction
 INTO
  #CRMFinal
 FROM
  CustomerRelations CR
  INNER JOIN #CRMNotes N ON CR.Id = N.CrmId
  INNER JOIN CRMStatuses sts ON CR.StatusId = sts.Id
  INNER JOIN CRMActions act ON CR.ActionId = act.Id
  
--------------------------Finished Loans-----------------------------  
IF OBJECT_ID('tempdb..#FinishedLoans') IS NOT NULL DROP TABLE #FinishedLoans
SELECT c.Id cId, count(l.Id) NumberOfLoans, max(l.DateClosed) DateClosed, 'Fully Repaid' AS LoanStatus
INTO #FinishedLoans
FROM Customer c
INNER JOIN Loan l
ON c.Id = l.CustomerId
WHERE l.DateClosed IS NOT NULL
AND c.Id NOT IN 
 (SELECT cc.Id 
  FROM Customer cc 
  INNER JOIN Loan ll 
  ON cc.Id = ll.CustomerId 
  WHERE ll.DateClosed IS NULL 
  GROUP BY cc.Id)
GROUP BY c.Id

----------------------Almost Finished Loans--------------------------
IF OBJECT_ID('tempdb..#CountStillToPay') IS NOT NULL DROP TABLE #CountStillToPay
SELECT LoanId, Count  
INTO #CountStillToPay
FROM (
SELECT LoanId, count(Status) Count  FROM LoanSchedule
WHERE Status = 'StillToPay'
GROUP BY LoanId ) x
WHERE x.Count <=2

IF OBJECT_ID('tempdb..#AlmostFinishedLoans') IS NOT NULL DROP TABLE #AlmostFinishedLoans
SELECT c.Id cId, count(l.Id) NumberOfLoans, NULL DateClosed, (convert(NVARCHAR,max(p.Count)) + ' Installments Left') AS LoanStatus
INTO #AlmostFinishedLoans
FROM Customer c
INNER JOIN Loan l ON c.Id = l.CustomerId
JOIN #CountStillToPay p ON p.LoanId = l.Id
GROUP BY c.Id


IF OBJECT_ID('tempdb..#Merged') IS NOT NULL DROP TABLE #Merged
SELECT x.cId cId, x.NumberOfLoans NumberOfLoans, x.DateClosed DateClosed, x.LoanStatus LoanStatus
INTO #Merged
FROM
(SELECT * FROM #FinishedLoans
UNION 
SELECT * FROM #AlmostFinishedLoans) x



--------------------------MaxApprovedSum-----------------------------  
IF OBJECT_ID('tempdb..#MaxApproved') IS NOT NULL DROP TABLE #MaxApproved
 SELECT 
 ROW_NUMBER() OVER (ORDER BY C.id) AS 'RowNumber',
  C.Id,
  C.Name AS Email,
  C.FullName Name,
  L.NumberOfLoans NumberOfLoans,  
  max(O.ManagerApprovedSum) MaxApprovedSum,
  C.DaytimePhone DayPhone,
  C.MobilePhone MobilePhone,
  CR.CRMStatus CRMStatus,
  CR.CRMAction CRMAction,
  CR.CRMComment CRMComment,
  L.DateClosed DateClosed,
  L.LoanStatus LoanStatus
  INTO #MaxApproved
 FROM
  #Merged L 
  LEFT JOIN Customer C ON C.Id = L.cId
  INNER JOIN CashRequests O
   ON C.Id = O.IdCustomer
   AND O.UnderwriterDecision = 'Approved'
  LEFT JOIN #CRMFinal CR ON CR.CustomerId = O.IdCustomer
 
 WHERE C.CreditResult <> 'Late' -- is late
 AND C.IsWasLate <> 1
 AND   C.Id NOT IN 
   (SELECT C.Id
    FROM Customer C 
    WHERE Name LIKE '%ezbob%'
    OR Name LIKE '%liatvanir%'
    OR Name LIKE '%q@q%'
    OR Name LIKE '%1@1%'
    OR C.IsTest=1)
 AND C.CollectionStatus = 0 -- Is Enabled Good (Disabled/Legal/Fraud/FraudSuspect/Risky/Bad)
 GROUP BY C.Id, C.Name, C.FullName,L.NumberOfLoans,C.DaytimePhone, C.MobilePhone, CR.CRMStatus, CR.CRMAction, CR.CRMComment,L.DateClosed, L.LoanStatus
-------------------------Underwriter Descision-----------------
IF OBJECT_ID('tempdb..#Decision') IS NOT NULL DROP TABLE #Decision;

WITH e AS
(
     SELECT UnderwriterDecision, UnderwriterDecisionDate, IdCustomer cId ,
         ROW_NUMBER() OVER
         (
             PARTITION BY IdCustomer
             ORDER BY UnderwriterDecisionDate DESC
         ) AS Recency
     FROM CashRequests
     WHERE UnderwriterDecisionDate IS NOT NULL
)

SELECT *
INTO #Decision
FROM e
WHERE Recency = 1

--------------------------Final Merge-------------------------- 
 SELECT m.Id Id, m.Email Email, m.Name Name, m.NumberOfLoans NumberOfLoans,m.LoanStatus, m.MaxApprovedSum, m.DayPhone, m.MobilePhone, m.CRMStatus, m.CRMAction, m.CRMComment, d.UnderwriterDecision
 FROM #MaxApproved m
 LEFT JOIN #Decision d ON m.Id = d.cId
 --WHERE DateClosed >= @DateStart AND DateClosed <= @DateEnd
 --GROUP BY m.Id, m.Email, m.Name, m.NumberOfLoans, m.MaxApprovedSum, m.DayPhone, m.MobilePhone, m.CRMStatus, m.CRMAction, m.CRMComment, m.DateClosed, m.LoanStatus, d.UnderwriterDecision
 ORDER BY m.DateClosed DESC, m.LoanStatus
 
 
 ----------------Drop Temp Tables------------------------------
 DROP TABLE #CRMNotes
 DROP TABLE #CRMFinal
 DROP TABLE #FinishedLoans
 DROP TABLE #MaxApproved
 DROP TABLE #AlmostFinishedLoans
 DROP TABLE #CountStillToPay
 DROP TABLE #Merged
 
END

GO
/****** Object:  StoredProcedure [dbo].[RptGetCampaignClickStats]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptGetCampaignClickStats]
@DateStart DATETIME,
@DateEnd   DATETIME
AS
BEGIN
	SELECT
		[Date],
		Title,
		Url,
		EmailsSent,
		Clicks,
		Email
	FROM
		MC_CampaignClicks
	ORDER BY
		[Date] DESC,
		Title
END

GO
/****** Object:  StoredProcedure [dbo].[RptGetUserName]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptGetUserName]
@UserName NVARCHAR(50)
AS
BEGIN
	SELECT
		Name
	FROM
		ReportUsers
	WHERE
		UserName = @UserName
END

GO
/****** Object:  StoredProcedure [dbo].[RptGetUserReports]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptGetUserReports]
@UserName NVARCHAR(50) = NULL
AS
BEGIN
	DECLARE @UserId INT = (SELECT Id FROM ReportUsers WHERE UserName = @UserName)
	
	SELECT
		rs.Id,
		rs.Type,
		rs.Title,
		rs.StoredProcedure,
		rs.IsDaily,
		rs.IsWeekly,
		rs.IsMonthly,
		rs.Header,
		rs.Fields,
		rs.ToEmail,
		rs.IsMonthToDate
	FROM
		ReportScheduler rs
		INNER JOIN ReportsUsersMap rum ON rs.Id = rum.ReportID
		INNER JOIN ReportUsers ru ON rum.UserID = ru.Id
	WHERE
		@UserName IS NULL
		OR
		ru.UserName = @UserName
	ORDER BY
		rs.Title
END

GO
/****** Object:  StoredProcedure [dbo].[RptLoansGiven]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptLoansGiven]
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	SELECT
		l.Id AS LoanID,
		l.Date,
		c.Id AS ClientID,
		c.Name AS ClientEmail,
		c.Fullname AS ClientName,
		lt.Name AS LoanTypeName,
		ISNULL(out.Fees, 0) AS SetupFee,
		ISNULL(out.Amount, 0) AS LoanAmount,
		s.Period,
		s.PlannedInterest,
		s.PlannedRepaid,
		ISNULL(pay.TotalPrincipalRepaid, 0) AS TotalPrincipalRepaid,
		ISNULL(pay.TotalInterestRepaid, 0) AS TotalInterestRepaid,
		0 AS EarnedInterest,
		ISNULL(exi.ExpectedInterest, 0) AS ExpectedInterest,
		0 AS AccruedInterest,
		0 AS TotalInterest,
		ISNULL(fc.Fees, 0) AS TotalFeesRepaid,
		ISNULL(fc.Charges, 0) AS TotalCharges,
		l.InterestRate AS BaseInterest,
		dp.Name AS DiscountPlan,
		CASE ISNULL(out.Counter, 0)
			WHEN 1 THEN ''
			ELSE 'unmatched'
		END AS RowLevel
	FROM
		Loan l
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
		INNER JOIN LoanType lt ON l.LoanTypeId = lt.Id
		INNER JOIN (
			SELECT
				s.LoanId,
				COUNT(DISTINCT s.Id) AS Period,
				SUM(s.Interest) AS PlannedInterest,
				SUM(s.AmountDue) AS PlannedRepaid
			FROM
				LoanSchedule s
				INNER JOIN Loan l ON s.LoanId = l.Id
			GROUP BY
				s.LoanId
		) s ON l.Id = s.LoanId
		INNER JOIN CashRequests cr ON l.RequestCashId = cr.Id
		INNER JOIN DiscountPlan dp ON cr.DiscountPlanId = dp.Id
		LEFT JOIN (
			SELECT
				s.LoanId,
				SUM(s.Interest) AS ExpectedInterest
			FROM
				LoanSchedule s
			WHERE
				s.Date > GETDATE()
			GROUP BY
				s.LoanId
		) exi ON l.Id = exi.LoanId
		LEFT JOIN (
			SELECT
				t.LoanId,
				SUM(t.Amount) AS Amount,
				SUM(t.Fees) AS Fees,
				COUNT(DISTINCT t.Id) AS Counter
			FROM
				LoanTransaction t
				INNER JOIN Loan l ON t.LoanId = l.Id
			WHERE
		 		t.Status = 'Done'
		 		AND
		 		t.Type = 'PacnetTransaction'
			GROUP BY
				t.LoanId
		) out ON l.Id = out.LoanId
		LEFT JOIN (
			SELECT
				t.LoanId,
				SUM(t.LoanRepayment) AS TotalPrincipalRepaid,
				SUM(t.Interest) AS TotalInterestRepaid
			FROM
				LoanTransaction t
				INNER JOIN Loan l ON t.LoanId = l.Id
			WHERE
		 		t.Status = 'Done'
		 		AND
		 		t.Type = 'PaypointTransaction'
			GROUP BY
				t.LoanId
		) pay ON l.Id = pay.LoanId
		LEFT JOIN dbo.udfLoanFeesAndCharges(@DateStart, @DateEnd) fc ON l.Id = fc.LoanID
	WHERE
		CONVERT(DATE, @DateStart) <= l.Date AND l.Date < CONVERT(DATE, @DateEnd)
	ORDER BY
		l.Date
END

GO
/****** Object:  StoredProcedure [dbo].[RptLoansOverall]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptLoansOverall]
@DateStart DATETIME,
@DateEnd   DATETIME
AS
BEGIN
	SELECT
		x.[Date] LoanDate,
		x.Name eMail,
		x.FirstName + ' ' + x.Surname AS Name,
		SUM(x.Repaid) Repaid,
		SUM(x.NetoRepaid) AS PrincipalRepaid,
		SUM(x.StillToPay) AS PrincipalOutstanding,
		SUM(x.Given) AS OriginalLoanPrincipal,
		SUM(x.Fees) Fees,
		SUM(x.Interest) Interest,
		CASE
			WHEN SUM(x.StillToPay) = 0 THEN '0'
			ELSE '1'
		END AS LoanStatus
	FROM (
		SELECT
			l.[Date],
			c.Name,
			c.FirstName,
			c.Surname,
			SUM(lt.Amount) Repaid,
			SUM(lt.Amount) - SUM(lt.Fees) + SUM(lt.Interest) AS NetoRepaid,
			0 StillToPay,
			0 Given,
			SUM(lt.Fees) Fees,
			SUM(lt.Interest) Interest
		FROM
			Loan l
			JOIN LoanTransaction lt ON l.Id = lt.LoanId
			JOIN Customer c ON l.CustomerId = c.Id
		WHERE
			lt.Status = 'Done'
			AND
			lt.Type = 'PaypointTransaction'
			AND
			c.IsTest = 0
		GROUP BY
			l.[Date],
			c.Name,
			c.FirstName,
			c.Surname

		UNION

		SELECT
			l.[Date],
			c.Name,
			c.FirstName,
			c.Surname,
			0 Repaid,
			0 NetoRepaid,
			0 StillToPay,
			SUM(lt.Amount) Given,
			SUM(lt.Fees) Fees,
			0 Interest
		FROM
			Loan l
			JOIN LoanTransaction lt ON lt.LoanId = l.Id
			JOIN Customer c ON l.CustomerId = c.Id
		WHERE
			lt.Status = 'Done'
			AND
			lt.Type = 'PacnetTransaction'
			AND
			c.IsTest = 0
		GROUP BY
			l.[Date],
			c.Name,
			c.FirstName,
			c.Surname

		UNION

		SELECT
			l.[Date],
			c.Name,
			c.FirstName,
			c.Surname,
			0 Repaid,
			0 NetoRepaid,
			SUM(ls.LoanRepayment) StillToPay,
			0 Given,
			0 Fees,
			0 Interest
		FROM
			Loan l
			JOIN LoanSchedule ls ON ls.LoanId = l.Id
			JOIN Customer c ON l.CustomerId = c.Id
		WHERE
			ls.Status IN ('StillToPay', 'Late')
			AND
			c.IsTest = 0
		GROUP BY
			l.[Date],
			c.Name,
			c.FirstName,
			c.Surname
	) AS x
	GROUP BY
		x.[Date],
		x.Name,
		x.FirstName,
		x.Surname

	UNION

	SELECT
		NULL LoanDate,
		'' Name,
		'' Name,
		SUM(x.Repaid) Repaid,
		SUM(x.NetoRepaid) AS PrincipalRepaid,
		SUM(x.StillToPay) AS PrincipalOutstanding,
		SUM(x.Given) AS OriginalLoanPrincipal,
		SUM(x.Fees) Fees,
		SUM(x.Interest) Interest,
		'' AS LoanStatus
	FROM (
		SELECT
			NULL LoanDate,
			'' Name,
			'' FirstName,
			'' Surname,
			SUM(lt.Amount) Repaid,
			SUM(lt.Amount) - SUM(lt.Fees) + SUM(lt.Interest) AS NetoRepaid,
			0 StillToPay,
			0 Given,
			SUM(lt.Fees) Fees,
			SUM(lt.Interest) Interest
		FROM
			Loan l
			JOIN LoanTransaction lt ON lt.LoanId = l.Id
			JOIN Customer c ON l.CustomerId = c.Id
		WHERE
			lt.Status = 'Done'
			AND
			lt.Type = 'PaypointTransaction'
			AND
			c.IsTest = 0

		UNION

		SELECT
			NULL LoanDate,
			'' Name,
			'' FirstName,
			'' Surname,
			0 Repaid,
			0 NetoRepaid,
			0 StillToPay,
			SUM(lt.Amount) Given,
			SUM(lt.Fees) Fees,
			0 Interest
		FROM
			Loan l
			JOIN LoanTransaction lt ON lt.LoanId = l.Id
			JOIN Customer c ON l.CustomerId = c.Id
		WHERE
			lt.Status = 'Done'
			AND
			lt.Type = 'PacnetTransaction'
			AND
			c.IsTest = 0

		UNION

		SELECT
			NULL LoanDate,
			'' Name,
			'' FirstName,
			'' Surname,
			0 Repaid,
			0 NetoRepaid,
			SUM(ls.LoanRepayment) StillToPay,
			0 Given,
			0 Fees,
			0 Interest
		FROM
			Loan l
			JOIN LoanSchedule ls ON ls.LoanId = l.Id
			JOIN Customer c ON l.CustomerId = c.Id
		WHERE
			ls.Status IN ('StillToPay', 'Late')
			AND
			c.IsTest = 0
	) AS x

	ORDER BY x.[Date]
END

GO
/****** Object:  StoredProcedure [dbo].[RptLoanStats_CashRequests]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[RptLoanStats_CashRequests]
AS
SELECT
	r.Id AS RequestID,
	r.LoanTypeId AS LoanTypeID,
	lt.Type AS LoanType,
	r.IsLoanTypeSelectionAllowed,
	CASE dp.IsDefault
		WHEN 1 THEN '0'
		ELSE dp.Name
	END AS DiscountPlanName,
	c.Id AS CustomerID,
	c.Fullname AS CustomerName,
	CASE
		WHEN r.IdUnderwriter IS NULL
			THEN r.SystemDecisionDate
		ELSE
			r.UnderwriterDecisionDate
	END AS DecisionDate,
	ISNULL(CASE
		WHEN r.IdUnderwriter IS NULL
			THEN CASE
				WHEN r.UnderwriterComment = 'Auto Re-Approval' THEN r.ManagerApprovedSum
				ELSE r.SystemCalculatedSum
			END
		ELSE
			ISNULL(r.ManagerApprovedSum, r.SystemCalculatedSum)
	END, 0) AS ApprovedSum,
	r.InterestRate AS ApprovedRate,
	ISNULL(r.ExpirianRating, 0) AS CreditScore,
	ISNULL(r.AnualTurnover, 0) AS AnnualTurnover,
	r.MedalType,
	c.Gender,
	c.DateOfBirth,
	c.MartialStatus,
	c.ResidentialStatus,
	c.TypeOfBusiness,
	c.ReferenceSource,
	ISNULL(lmt.LoanId, 0) AS LoanID,
	ISNULL(lmt.LoanAmount, 0) AS LoanAmount,
	ISNULL(lmt.Date, 'Jul 1 1976') AS LoanIssueDate,
	ISNULL(lmt.AgreementModel, '{ "Term": 0 }') AS AgreementModel
FROM
	CashRequests r
	INNER JOIN Customer c ON r.IdCustomer = c.Id AND c.IsTest = 0
	INNER JOIN LoanType lt ON r.LoanTypeId = lt.Id
	INNER JOIN DiscountPlan dp ON r.DiscountPlanId = dp.Id
	LEFT JOIN (
		SELECT
			l.Id AS LoanId,
			l.RequestCashId,
			ISNULL(ISNULL(mt.Amount, l.LoanAmount), 0) AS LoanAmount,
			l.Date,
			l.AgreementModel
		FROM
			Loan l
			INNER JOIN LoanTransaction mt
				ON l.Id = mt.LoanId
				AND mt.Type = 'PacnetTransaction'
				AND mt.Status = 'Done'
				AND mt.Description NOT LIKE 'Non-cash.%'
	) lmt ON r.Id = lmt.RequestCashId
WHERE
	(
		(r.IdUnderwriter IS NOT NULL AND r.UnderwriterDecision = 'Approved')
		OR
		(r.IdUnderwriter IS NULL AND r.SystemDecision = 'Approve')
	)
	AND
	r.CreationDate >= 'Sep 4 2012'
ORDER BY
	r.IdCustomer,
	CASE 
		WHEN r.IdUnderwriter IS NULL
			THEN r.SystemDecisionDate
		ELSE
			r.UnderwriterDecisionDate
	END

GO
/****** Object:  StoredProcedure [dbo].[RptMarketPlacesLoanStats]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptMarketPlacesLoanStats]
	@DateStart DATETIME,
	@DateEnd DATETIME
AS
BEGIN
	CREATE TABLE #ApprovedCustomers
	(
		CustomerId INT,
		Gender CHAR(1),
		Age NUMERIC(5,2),
		ApprovedLoan INT,
		LoanedAmount INT,
		ExperianScore INT
	)

	CREATE TABLE #Turnovers
	(
		CustomerId INT,
		MarketPlaceTypeId INT,
		MarketPlaceId INT,
		Turnover NUMERIC(18,2),
		ProratedLoanAmount NUMERIC(18,2),
		ProratedApprovedAmount NUMERIC(18,2),
		Gender CHAR(1),
		Age NUMERIC(5,2)
	)

	CREATE TABLE #tmp
	(
		MarketPlaceTypeId INT,
		NumOfShopsApproved INT,
		NumOfShopsLoaned INT,
		AvgTurnover NUMERIC(18,2),
		AvgLoanAmount NUMERIC(18,2),
		AvgApprovedAmount NUMERIC(18,2),
		AvgScore NUMERIC(5,2),
		PercentMen NUMERIC(5,2),
		AvgAge NUMERIC(5,2)
	)

	DECLARE @CustomerId INT,
			@ApprovedSum INT,
			@LoanedAmount INT,
			@MarketPlaceTypeId INT,
			@MarketPlaceId INT,
			@ExperianScore INT,
			@MarketPlaceName NVARCHAR(255),
			@AnalysisFuncId INT,
			@LatestAggregationTimeStamp DATETIME,
			@AggregateValue FLOAT,
			@NumOfApproved INT,
			@NumOfLoaned INT,
			@AvgTurnover NUMERIC(18,2),
			@AvgScore NUMERIC(18,2),
			@PercentMen NUMERIC(18,2),
			@AvgAge NUMERIC(18,2),
			@GenderHelper INT,
			@AgeHelper INT,
			@GenderHelper2 INT,
			@AvgLoaned NUMERIC(18,2),
			@AvgApproved NUMERIC(18,2),
			@TotalCustomerLoans NUMERIC(18,2),
			@TotalCustomerApproved NUMERIC(18,2),
			@TotalCustomerTurnover NUMERIC(18,2),
			@TurnoverForCurrentMp NUMERIC(18,2),
			@Age NUMERIC(18,2),
			@Gender CHAR(1)

	INSERT INTO #ApprovedCustomers 
	SELECT DISTINCT Customer.Id, Gender, DATEDIFF(hour,DateOfBirth,GETDATE())/8766.0 AS Age, 0,0,0 FROM Customer, DecisionHistory 
	WHERE 
		Customer.IsTest = 0 AND
		Customer.Id=DecisionHistory.CustomerId AND 
		Action='Approve' AND 
		GreetingMailSentDate >= @DateStart AND 
		GreetingMailSentDate < @DateEnd AND 
		WizardStep=4 

	DECLARE cur CURSOR FOR SELECT CustomerId FROM #ApprovedCustomers
	OPEN cur
	FETCH NEXT FROM cur INTO @CustomerId
	WHILE @@FETCH_STATUS = 0
	BEGIN
		SELECT @ApprovedSum = sum(ManagerApprovedSum) FROM CashRequests WHERE IdCustomer=@CustomerId AND ManagerApprovedSum IS NOT NULL
		SELECT @LoanedAmount = sum(Loan.LoanAmount) FROM Loan WHERE CustomerId=@CustomerId
		SELECT @ExperianScore = ExperianScore FROM (SELECT CustomerId, ExperianScore, row_number() OVER(PARTITION BY CustomerId ORDER BY Id DESC) AS rn FROM MP_ExperianDataCache) as T WHERE CustomerId=@CustomerId

		UPDATE #ApprovedCustomers SET ApprovedLoan = @ApprovedSum, LoanedAmount = @LoanedAmount, ExperianScore = @ExperianScore WHERE CustomerId=@CustomerId

		FETCH NEXT FROM cur INTO @CustomerId
	END
	CLOSE cur
	DEALLOCATE cur

	DECLARE cur CURSOR FOR SELECT CustomerId, Gender, Age FROM #ApprovedCustomers
	OPEN cur
	FETCH NEXT FROM cur INTO @CustomerId, @Gender, @Age
	WHILE @@FETCH_STATUS = 0
	BEGIN
		DECLARE mpcur CURSOR FOR SELECT MP_MarketplaceType.Id, Name, MP_CustomerMarketPlace.Id FROM MP_MarketplaceType, MP_CustomerMarketPlace WHERE MP_MarketplaceType.Id = MarketPlaceId AND CustomerId = @CustomerId AND Name != 'PayPoint'
		OPEN mpcur
		FETCH NEXT FROM mpcur INTO @MarketPlaceTypeId, @MarketPlaceName, @MarketPlaceId
		WHILE @@FETCH_STATUS = 0
		BEGIN	
			IF @MarketPlaceName = 'Pay Pal'
			BEGIN
				SELECT @AnalysisFuncId = Id FROM MP_AnalyisisFunction WHERE MarketPlaceId=@MarketPlaceTypeId AND Name='TotalNetInPayments'
			END
			ELSE
			BEGIN
				IF @MarketPlaceName = 'PayPoint'
				BEGIN
					SELECT @AnalysisFuncId = Id FROM MP_AnalyisisFunction WHERE MarketPlaceId=@MarketPlaceTypeId AND Name='SumOfAuthorisedOrders'
				END
				ELSE
				BEGIN
					SELECT @AnalysisFuncId = Id FROM MP_AnalyisisFunction WHERE MarketPlaceId=@MarketPlaceTypeId AND Name='TotalSumOfOrders'
				END
			END			
			
			IF @AnalysisFuncId IS NOT NULL
			BEGIN
				SELECT @LatestAggregationTimeStamp = Max(Updated) FROM MP_AnalyisisFunctionValues WHERE AnalyisisFunctionId=@AnalysisFuncId AND CustomerMarketPlaceId=@MarketPlaceId
				IF @LatestAggregationTimeStamp IS NOT NULL
				BEGIN
					SELECT TOP 1 ValueFloat INTO #MaxAggrValueFinished FROM MP_AnalyisisFunctionValues WHERE AnalyisisFunctionId=@AnalysisFuncId AND CustomerMarketPlaceId=@MarketPlaceId AND Updated = @LatestAggregationTimeStamp AND AnalysisFunctionTimePeriodId < 5 ORDER BY AnalysisFunctionTimePeriodId DESC
					
					SELECT @AggregateValue = ValueFloat FROM #MaxAggrValueFinished
					
					INSERT INTO #Turnovers VALUES (@CustomerId, @MarketPlaceTypeId, @MarketPlaceId, @AggregateValue, 0, 0, @Gender, @Age)
					
										
					DROP TABLE #MaxAggrValueFinished
				END
			END			
		
			FETCH NEXT FROM mpcur INTO @MarketPlaceTypeId, @MarketPlaceName, @MarketPlaceId
		END
		CLOSE mpcur
		DEALLOCATE mpcur

		FETCH NEXT FROM cur INTO @CustomerId, @Gender, @Age
	END
	CLOSE cur
	DEALLOCATE cur

	DECLARE cur CURSOR FOR SELECT DISTINCT CustomerId FROM #Turnovers
	OPEN cur
	FETCH NEXT FROM cur INTO @CustomerId
	WHILE @@FETCH_STATUS = 0
	BEGIN
		SELECT @TotalCustomerLoans = sum(LoanedAmount) FROM #ApprovedCustomers WHERE CustomerId=@CustomerId AND LoanedAmount IS NOT NULL
		SELECT @TotalCustomerApproved = sum(ApprovedLoan) FROM #ApprovedCustomers WHERE CustomerId=@CustomerId AND ApprovedLoan IS NOT NULL
		SELECT @TotalCustomerTurnover = sum(Turnover) FROM #Turnovers WHERE CustomerId=@CustomerId
			
		DECLARE mpcur CURSOR FOR SELECT MarketPlaceId FROM #Turnovers WHERE CustomerId=@CustomerId
		OPEN mpcur
		FETCH NEXT FROM mpcur INTO @MarketPlaceId
		WHILE @@FETCH_STATUS = 0
		BEGIN
			SELECT @TurnoverForCurrentMp = Turnover FROM #Turnovers WHERE CustomerId=@CustomerId AND MarketPlaceId = @MarketPlaceId
			UPDATE #Turnovers SET 
				ProratedLoanAmount = @TurnoverForCurrentMp * @TotalCustomerLoans / @TotalCustomerTurnover, 
				ProratedApprovedAmount = @TurnoverForCurrentMp * @TotalCustomerApproved / @TotalCustomerTurnover
			WHERE CustomerId=@CustomerId AND MarketPlaceId = @MarketPlaceId

			FETCH NEXT FROM mpcur INTO @MarketPlaceId
		END
		CLOSE mpcur
		DEALLOCATE mpcur


		FETCH NEXT FROM cur INTO @CustomerId
	END
	CLOSE cur
	DEALLOCATE cur
	
	DECLARE mpcur CURSOR FOR SELECT Id FROM MP_MarketplaceType WHERE Name != 'PayPoint'
	OPEN mpcur
	FETCH NEXT FROM mpcur INTO @MarketPlaceTypeId
	WHILE @@FETCH_STATUS = 0
	BEGIN
		SELECT @NumOfApproved = count(1) FROM #Turnovers WHERE MarketPlaceTypeId = @MarketPlaceTypeId
		SELECT @NumOfLoaned = count(1) FROM #Turnovers, #ApprovedCustomers WHERE MarketPlaceTypeId = @MarketPlaceTypeId AND #Turnovers.CustomerId = #ApprovedCustomers.CustomerId AND #ApprovedCustomers.LoanedAmount IS NOT NULL
		
		SELECT @AvgTurnover = sum(Turnover)/count(1) FROM #Turnovers WHERE Turnover > 0 AND MarketPlaceTypeId = @MarketPlaceTypeId
		
		SELECT @AvgLoaned = sum(ProratedLoanAmount) / @NumOfLoaned FROM #Turnovers WHERE ProratedLoanAmount IS NOT NULL AND MarketPlaceTypeId = @MarketPlaceTypeId
		SELECT @AvgApproved = sum(ProratedApprovedAmount) / @NumOfApproved FROM #Turnovers WHERE ProratedApprovedAmount IS NOT NULL AND MarketPlaceTypeId = @MarketPlaceTypeId
		SELECT @AvgScore = sum(ExperianScore)/count(1) FROM #Turnovers, #ApprovedCustomers WHERE MarketPlaceTypeId = @MarketPlaceTypeId AND #Turnovers.CustomerId = #ApprovedCustomers.CustomerId AND #ApprovedCustomers.ExperianScore > 0
		
		SELECT @GenderHelper = count(1) FROM #Turnovers WHERE Gender = 'M' AND MarketPlaceTypeId = @MarketPlaceTypeId
		SELECT @GenderHelper2 = count(1) FROM #Turnovers WHERE MarketPlaceTypeId = @MarketPlaceTypeId
		SELECT @PercentMen = CASE WHEN @GenderHelper2 = 0 THEN 0 ELSE @GenderHelper * 100.0 / @GenderHelper2 END
			
		
		SELECT @AgeHelper = count(1) FROM #Turnovers WHERE MarketPlaceTypeId = @MarketPlaceTypeId
		IF @AgeHelper=0
		BEGIN
			SELECT @AvgAge = 0
		END
		ELSE
		BEGIN
			SELECT @AvgAge = sum(Age) / @AgeHelper FROM #Turnovers WHERE MarketPlaceTypeId = @MarketPlaceTypeId
		END
		
		INSERT INTO #tmp VALUES(@MarketPlaceTypeId, @NumOfApproved, @NumOfLoaned, @AvgTurnover, @AvgLoaned, @AvgApproved, @AvgScore, @PercentMen, @AvgAge)
	 
		FETCH NEXT FROM mpcur INTO @MarketPlaceTypeId
	END
	CLOSE mpcur
	DEALLOCATE mpcur

	SELECT 
		Name,
		CASE WHEN NumOfShopsApproved=0 THEN NULL ELSE NumOfShopsApproved END AS NumOfShopsApproved,
		CASE WHEN NumOfShopsLoaned=0 THEN NULL ELSE NumOfShopsLoaned END AS NumOfShopsLoaned,
		CONVERT(INT, AvgTurnover) AS AvgTurnover,
		CONVERT(INT, AvgLoanAmount) AS AvgLoanAmount,
		CONVERT(INT, AvgApprovedAmount) AS AvgApprovedAmount,
		CONVERT(INT, AvgScore) AS AvgScore,
		CASE WHEN PercentMen = 0 THEN NULL ELSE CONVERT(INT, PercentMen) END AS PercentMen,
		CASE WHEN AvgAge = 0 THEN NULL ELSE CONVERT(INT, AvgAge) END AS AvgAge,
		CASE WHEN NumOfShopsApproved = 0 THEN NULL ELSE CONVERT(INT, 100.0 * NumOfShopsLoaned / NumOfShopsApproved) END AS UtilizationByNum,
		CASE WHEN AvgTurnover=0 OR NumOfShopsApproved=0 THEN NULL ELSE CONVERT(INT, (100.0 * AvgLoanAmount * NumOfShopsLoaned) / (AvgApprovedAmount * NumOfShopsApproved)) END AS UtilizationByAmount	
	FROM #tmp, MP_MarketplaceType WHERE Id = MarketPlaceTypeId
	 
	DROP TABLE #ApprovedCustomers 
	DROP TABLE #Turnovers 
	DROP TABLE #tmp
END

GO
/****** Object:  StoredProcedure [dbo].[RptMarketPlacesStats]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptMarketPlacesStats]
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	SELECT
		@DateStart = CONVERT(DATE, @DateStart),
		@DateEnd = CONVERT(DATE, @DateEnd)

	CREATE TABLE #tmp (
		MarketPlaceId INT,
		NumOfShopsDidntFinish INT,
		AvgTurnoverDidntFinish NUMERIC(18,2),
		NumOfShopsFinish INT,
		AvgTurnoverFinish NUMERIC(18,2),
		AvgScore NUMERIC(15,2),
		PercentMen NUMERIC(5,2),
		AvgAge NUMERIC(5,2),
		PercentApproved NUMERIC(5,2),
		AvgAmountApproved NUMERIC(10,2),
		NumOfShopsApproved INT
	)

	CREATE TABLE #tmp2 (
		CustomerId INT,
		MarketPlaceTypeId INT,
		NumOfShops INT,
		TurnoverForType NUMERIC(18,2),
		TotalTurnover NUMERIC(18,2),
		ProratedLoan NUMERIC(18,2)
	)

	DECLARE @mpId INT,
		@mpName NVARCHAR(255),
		@numOfNotFinishedMps INT,
		@numOfFinishedMps INT,
		@numOfFinishedCustomers INT,
		@sumOfScore INT,
		@avgScore NUMERIC(15, 2),
		@currentMarketPlaceId INT,
		@latestAggr DATETIME,
		@analysisFuncId INT,
		@AggrVal FLOAT,
		@TotalTurnoverNotFinished NUMERIC(18,2),
		@TotalTurnoverNotFinishedCounter INT,
		@avgTurnOverNotFinished NUMERIC(18,2),
		@TotalTurnoverFinished NUMERIC(18,2),
		@TotalTurnoverFinishedCounter INT,
		@avgTurnOverFinished NUMERIC(18,2),
		@numOfMaleFinishedMps INT,
		@malePercent NUMERIC(5,2),
		@avgAge NUMERIC(18,2),
		@CustomerId INT,
		@TmpTotalTurnover NUMERIC(18,2),
		@loanAmount NUMERIC(18,2),
		@MarketPlaceTypeId INT,
		@TotalTurnover NUMERIC(18,2),
		@ProratedLoan NUMERIC(18,2),
		@numOfShops INT

	DECLARE cur CURSOR FOR
		SELECT
			Id,
			Name
		FROM
			MP_MarketplaceType
		WHERE
			Name != 'PayPoint'

	OPEN cur

	FETCH NEXT FROM cur INTO @mpId, @mpName

	WHILE @@FETCH_STATUS = 0
	BEGIN
		SELECT
			Id
		INTO
			#NotFinishedCustomers
		FROM
			Customer
		WHERE
			@DateStart <= GreetingMailSentDate AND GreetingMailSentDate < @DateEnd
			AND
			WizardStep != 4
			AND
			Customer.IsTest = 0

		SELECT
			Id,
			Gender,
			DATEDIFF(year, DateOfBirth, GETDATE()) AS Age
		INTO
			#FinishedCustomers
		FROM
			Customer
		WHERE
			Customer.IsTest = 0
			AND
			@DateStart <= GreetingMailSentDate AND GreetingMailSentDate < @DateEnd
			AND
			WizardStep = 4

		DELETE FROM
			#NotFinishedCustomers
		WHERE
			Id NOT IN (
				SELECT DISTINCT CustomerId
				FROM MP_CustomerMarketPlace
				INNER JOIN #NotFinishedCustomers ON CustomerId = #NotFinishedCustomers.Id
				WHERE MarketPlaceId = @mpId
			)

		DELETE FROM
			#FinishedCustomers
		WHERE
			Id NOT IN (
				SELECT DISTINCT CustomerId
				FROM MP_CustomerMarketPlace
				INNER JOIN #FinishedCustomers ON CustomerId = #FinishedCustomers.Id
				WHERE MarketPlaceId = @mpId
			)

		SET @numOfNotFinishedMps = 0
		SET @numOfFinishedMps = 0
		SET @TotalTurnoverNotFinished = 0
		SET @TotalTurnoverNotFinishedCounter = 0
		SET @TotalTurnoverFinished = 0
		SET @TotalTurnoverFinishedCounter = 0

		SELECT
			@numOfNotFinishedMps = COUNT(MarketPlaceId)
		FROM
			MP_CustomerMarketPlace
			INNER JOIN #NotFinishedCustomers ON CustomerId = #NotFinishedCustomers.Id
		WHERE
			MarketPlaceId = @mpId

		SELECT
			@numOfFinishedMps = COUNT(MarketPlaceId)
		FROM
			MP_CustomerMarketPlace
			INNER JOIN #FinishedCustomers ON CustomerId = #FinishedCustomers.Id
		WHERE
			MarketPlaceId = @mpId

		SELECT
			@numOfMaleFinishedMps = COUNT(MarketPlaceId)
		FROM
			MP_CustomerMarketPlace
			INNER JOIN #FinishedCustomers ON CustomerId = #FinishedCustomers.Id AND Gender = 'M'
		WHERE
			MarketPlaceId = @mpId

		SET @avgAge = 0

		IF @numOfFinishedMps != 0
			SELECT
				@avgAge = AVG(Age)
			FROM
				#FinishedCustomers

		SELECT
			CustomerId,
			ExperianScore
		INTO
			#ExperianScores
		FROM
			(
				SELECT
					CustomerId,
					ExperianScore,
					ROW_NUMBER() OVER(PARTITION BY CustomerId ORDER BY Id DESC) AS rn
				FROM
					MP_ExperianDataCache
			) as T
		WHERE
			rn = 1
			AND
			CustomerId IS NOT NULL
			AND
			ExperianScore != 0
			AND
			CustomerId IN (
				SELECT DISTINCT CustomerId
				FROM MP_CustomerMarketPlace
				INNER JOIN #FinishedCustomers ON CustomerId = #FinishedCustomers.Id
				WHERE MarketPlaceId = @mpId
			)

		SELECT
			@numOfFinishedCustomers = COUNT(1)
		FROM
			#ExperianScores

		SELECT
			@sumOfScore = SUM(ExperianScore)
		FROM
			#ExperianScores

		DROP TABLE #ExperianScores

		IF @sumOfScore IS NULL
			SET @avgScore = 0
		ELSE
			SET @avgScore = CONVERT(NUMERIC(15, 2), @sumOfScore) / CONVERT(NUMERIC(15, 2), @numOfFinishedCustomers)

		SELECT
			MP_CustomerMarketPlace.Id
		INTO
			#NotFinishedMarketPlaces
		FROM
			MP_CustomerMarketPlace
			INNER JOIN #NotFinishedCustomers ON CustomerId = #NotFinishedCustomers.Id
		WHERE
			MarketPlaceId = @mpId

		SELECT
			MP_CustomerMarketPlace.Id,
			CustomerId
		INTO
			#FinishedMarketPlaces
		FROM
			MP_CustomerMarketPlace
			INNER JOIN #FinishedCustomers ON CustomerId = #FinishedCustomers.Id
		WHERE
			MarketPlaceId = @mpId

		SELECT
			@analysisFuncId = Id
		FROM
			MP_AnalyisisFunction
		WHERE
			MarketPlaceId = @mpId
			AND
			(
				(@mpName = 'Pay pal' AND Name = 'TotalNetInPayments')
				OR
				(@mpName = 'PayPoint' AND Name = 'SumOfAuthorisedOrders')
				OR
				(@mpName NOT IN ('Pay pal', 'PayPoint') AND Name = 'TotalSumOfOrders')
			)

		IF @analysisFuncId IS NOT NULL
		BEGIN
			DECLARE marketCursor CURSOR FOR
				SELECT Id FROM #NotFinishedMarketPlaces

			OPEN marketCursor

			FETCH NEXT FROM marketCursor INTO @currentMarketPlaceId

			WHILE @@FETCH_STATUS = 0
			BEGIN
				SELECT
					@latestAggr = MAX(Updated)
				FROM
					MP_AnalyisisFunctionValues
				WHERE
					AnalyisisFunctionId = @analysisFuncId
					AND
					CustomerMarketPlaceId = @currentMarketPlaceId

				IF @latestAggr IS NOT NULL
				BEGIN
					SELECT TOP 1
						@AggrVal = ValueFloat
					FROM
						MP_AnalyisisFunctionValues
					WHERE
						AnalyisisFunctionId = @analysisFuncId
						AND
						CustomerMarketPlaceId = @currentMarketPlaceId
						AND
						Updated = @latestAggr
						AND
						AnalysisFunctionTimePeriodId < 5
					ORDER BY
						AnalysisFunctionTimePeriodId DESC

					SET @TotalTurnoverNotFinished = @TotalTurnoverNotFinished + @AggrVal

					SET @TotalTurnoverNotFinishedCounter = @TotalTurnoverNotFinishedCounter + 1
				END

				FETCH NEXT FROM marketCursor INTO @currentMarketPlaceId
			END

			CLOSE marketCursor
			DEALLOCATE marketCursor

			DECLARE marketCursor CURSOR FOR
				SELECT Id, CustomerId FROM #FinishedMarketPlaces

			OPEN marketCursor

			FETCH NEXT FROM marketCursor INTO @currentMarketPlaceId, @CustomerId

			WHILE @@FETCH_STATUS = 0
			BEGIN
				SELECT
					@latestAggr = Max(Updated)
				FROM
					MP_AnalyisisFunctionValues
				WHERE
					AnalyisisFunctionId = @analysisFuncId
					AND
					CustomerMarketPlaceId = @currentMarketPlaceId

				IF @latestAggr IS NOT NULL
				BEGIN
					SELECT TOP 1
						@AggrVal = ValueFloat
					FROM
						MP_AnalyisisFunctionValues
					WHERE
						AnalyisisFunctionId = @analysisFuncId
						AND
						CustomerMarketPlaceId = @currentMarketPlaceId
						AND
						Updated = @latestAggr
						AND
						AnalysisFunctionTimePeriodId < 5
					ORDER BY
						AnalysisFunctionTimePeriodId DESC

					SET @TotalTurnoverFinished = @TotalTurnoverFinished + @AggrVal

					SET @TotalTurnoverFinishedCounter = @TotalTurnoverFinishedCounter + 1

					IF NOT EXISTS (SELECT 1 FROM #tmp2 WHERE CustomerId = @CustomerId)
						INSERT INTO #tmp2 VALUES (@CustomerId, @mpId, 1, 0, 0, 0)

					IF NOT EXISTS (SELECT 1 FROM #tmp2 WHERE CustomerId = @CustomerId AND #tmp2.MarketPlaceTypeId = @mpId)
					BEGIN
						INSERT INTO #tmp2
						SELECT TOP 1
							@CustomerId,
							@mpId,
							1,
							0,
							TotalTurnover,
							0
						FROM
							#tmp2
						WHERE
							CustomerId = @CustomerId
					END

					UPDATE #tmp2 SET
						TurnoverForType = TurnoverForType + @AggrVal
					WHERE
						CustomerId = @CustomerId
						AND
						MarketPlaceTypeId = @mpId

					UPDATE #tmp2 SET
						TotalTurnover = TotalTurnover + @AggrVal
					WHERE
						CustomerId = @CustomerId
				END

				FETCH NEXT FROM marketCursor INTO @currentMarketPlaceId, @CustomerId
			END

			CLOSE marketCursor
			DEALLOCATE marketCursor
		END

		SET @avgTurnOverNotFinished = 0
		IF @TotalTurnoverNotFinishedCounter != 0
			SET @avgTurnOverNotFinished = @TotalTurnoverNotFinished / @TotalTurnoverNotFinishedCounter

		SET @avgTurnOverFinished = 0
		IF @TotalTurnoverFinishedCounter != 0
			SET @avgTurnOverFinished = @TotalTurnoverFinished / @TotalTurnoverFinishedCounter

		IF @numOfFinishedMps != 0
			SET @malePercent = @numOfMaleFinishedMps * 100.0 / @numOfFinishedMps
		ELSE
			SET @malePercent = 0

		INSERT INTO #tmp VALUES (
			@mpId, @numOfNotFinishedMps, @avgTurnOverNotFinished,
			@numOfFinishedMps, @avgTurnOverFinished, @avgScore,
			@malePercent, @avgAge, 0.0,
			0.0, 0
		)

		DROP TABLE #NotFinishedCustomers
		DROP TABLE #FinishedCustomers
		DROP TABLE #NotFinishedMarketPlaces
		DROP TABLE #FinishedMarketPlaces

		FETCH NEXT FROM cur INTO @mpId, @mpName
	END

	CLOSE cur
	DEALLOCATE cur

	DECLARE cur CURSOR FOR
		SELECT DISTINCT CustomerId FROM #tmp2

	OPEN cur

	FETCH NEXT FROM cur INTO @CustomerId

	WHILE @@FETCH_STATUS = 0
	BEGIN
		IF EXISTS (SELECT 1 FROM DecisionHistory WHERE CustomerId = @CustomerId AND Action = 'Approve')
		BEGIN
			DECLARE typeCur CURSOR FOR
				SELECT MarketPlaceTypeId
				FROM #tmp2
				WHERE CustomerId = @CustomerId

			OPEN typeCur

			FETCH NEXT FROM typeCur INTO @MarketPlaceTypeId

			WHILE @@FETCH_STATUS = 0
			BEGIN
				UPDATE #tmp SET
					NumOfShopsApproved = NumOfShopsApproved + 1
				WHERE
					MarketPlaceId = @MarketPlaceTypeId

				FETCH NEXT FROM typeCur INTO @MarketPlaceTypeId
			END

			CLOSE typeCur
			DEALLOCATE typeCur
		END

		FETCH NEXT FROM cur INTO @CustomerId
	END

	CLOSE cur
	DEALLOCATE cur

	DECLARE cur CURSOR FOR
		SELECT DISTINCT CustomerId FROM #tmp2

	OPEN cur

	FETCH NEXT FROM cur INTO @CustomerId

	WHILE @@FETCH_STATUS = 0
	BEGIN
		SELECT @loanAmount = ISNULL(Sum(LoanAmount), 0) FROM Loan WHERE CustomerId=@CustomerId

		DECLARE typeCur CURSOR FOR
			SELECT MarketPlaceTypeId
			FROM #tmp2
			WHERE CustomerId = @CustomerId

		OPEN typeCur

		FETCH NEXT FROM typeCur INTO @MarketPlaceTypeId

		WHILE @@FETCH_STATUS = 0
		BEGIN
			SELECT
				@TotalTurnover = TotalTurnover
			FROM
				#tmp2
			WHERE
				CustomerId = @CustomerId
				AND
				MarketPlaceTypeId = @MarketPlaceTypeId

			IF @TotalTurnover != 0
				UPDATE #tmp2 SET
					ProratedLoan = @loanAmount * TurnoverForType / @TotalTurnover
				WHERE
					CustomerId = @CustomerId
					AND
					MarketPlaceTypeId = @MarketPlaceTypeId

			FETCH NEXT FROM typeCur INTO @MarketPlaceTypeId
		END

		CLOSE typeCur
		DEALLOCATE typeCur

		FETCH NEXT FROM cur INTO @CustomerId
	END

	CLOSE cur
	DEALLOCATE cur

	DECLARE cur CURSOR FOR
		SELECT
			MarketPlaceTypeId,
			SUM(ProratedLoan)
		FROM
			#tmp2
		GROUP BY
			MarketPlaceTypeId

	OPEN cur

	FETCH NEXT FROM cur INTO @MarketPlaceTypeId, @ProratedLoan

	WHILE @@FETCH_STATUS = 0
	BEGIN
		UPDATE #tmp SET
			AvgAmountApproved = CASE NumOfShopsApproved
				 WHEN 0 THEN 0
				 ELSE @ProratedLoan / NumOfShopsApproved
			END
		WHERE
			MarketPlaceId = @MarketPlaceTypeId

		FETCH NEXT FROM cur INTO @MarketPlaceTypeId, @ProratedLoan
	END

	CLOSE cur
	DEALLOCATE cur

	DROP TABLE #tmp2

	SELECT
		Name,
		CASE
			WHEN NumOfShopsDidntFinish = 0 THEN NULL
			ELSE NumOfShopsDidntFinish
		END AS NumOfShopsDidntFinish,
		CASE
			WHEN AvgTurnoverDidntFinish = 0 THEN NULL
			ELSE CONVERT(INT, AvgTurnoverDidntFinish)
		END AS AvgTurnoverDidntFinish,
		CASE
			WHEN NumOfShopsFinish = 0 THEN NULL
			ELSE NumOfShopsFinish
		END AS NumOfShopsFinish,
		CASE
			WHEN AvgTurnoverFinish = 0 THEN NULL
			ELSE CONVERT(INT, AvgTurnoverFinish)
		END AS AvgTurnoverFinish,
		CASE
			WHEN AvgScore = 0 THEN NULL
			ELSE CONVERT(INT, AvgScore)
		END AS AvgScore,
		CASE
			WHEN PercentMen = 0 THEN NULL
			ELSE CONVERT(INT, PercentMen)
		END AS PercentMen,
		CASE
			WHEN AvgAge = 0 THEN NULL
			ELSE CONVERT(INT, AvgAge)
		END AS AvgAge,
		CASE
			WHEN NumOfShopsFinish IS NULL OR NumOfShopsFinish = 0 THEN NULL
			ELSE CASE
				WHEN NumOfShopsApproved = 0 THEN NULL
				ELSE CONVERT(INT, NumOfShopsApproved * 100.0 / NumOfShopsFinish)
			END
		END AS PercentApproved,
		CASE
			WHEN AvgAmountApproved = 0 THEN NULL
			ELSE CONVERT(INT, AvgAmountApproved)
		END AS AvgAmountApproved
	FROM
		#tmp
		INNER JOIN MP_MarketplaceType ON  #tmp.MarketPlaceId = MP_MarketplaceType.Id

	DROP TABLE #tmp
END

GO
/****** Object:  StoredProcedure [dbo].[RptNewClients]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create PROCEDURE [dbo].[RptNewClients]
@DateStart DATETIME,
@DateEnd   DATETIME
AS
BEGIN
	IF OBJECT_ID('tempdb..#tmp1') is not NULL
		DROP TABLE #tmp1

	IF OBJECT_ID('tempdb..#tmp2') is not NULL
		DROP TABLE #tmp2

	IF OBJECT_ID('tempdb..#tmpOffer') is not NULL
		DROP TABLE #tmpOffer

	SELECT
		CustomerId,
		COUNT(MarketPlaceId) Shops
	INTO
		#tmp2
	FROM
		MP_CustomerMarketPlace
	GROUP BY
		CustomerId

	SELECT
		IdCustomer CustomerId,
		MAX(ManagerApprovedSum) MaxApproved
	INTO
		#tmpOffer
	FROM
		CashRequests
	GROUP BY
		IdCustomer

	SELECT
		Name,
		DATEPART(dw,GreetingMailSentDate) DayOfWeek,
		GreetingMailSentDate DateRegister,
		ReferenceSource,
		#tmp2.Shops,
		Payment = CASE
			WHEN Customer.AccountNumber IS NULL THEN 'NO'
			WHEN Customer.AccountNumber IS NOT NULL THEN 'YES'
		END,
		Personal = CASE
			WHEN Customer.FirstName IS NULL THEN 'NO'
			WHEN Customer.FirstName IS NOT NULL THEN 'YES'
		END,
		Complete = CASE
			WHEN Customer.ApplyForLoan IS NULL THEN 'NO'
			WHEN Customer.ApplyForLoan IS NOT NULL THEN 'YES'
		END,
		CASE WHEN Customer.WizardStep=4 THEN 1 ELSE 0 END AS IsSuccessfullyRegistered,
		Customer.Status,
		Customer.MedalType,
		#tmpOffer.MaxApproved,
		Loan.LoanAmount,
		Customer.CreditSum CreditLeft,
		Loan.[Date],
		DATEPART(dw,Loan.[Date]) LoanDayOfWeek
	INTO
		#tmp1
	FROM
		Customer
		LEFT JOIN #tmp2 ON Customer.Id = #tmp2.CustomerId
		LEFT JOIN Loan ON Customer.Id = loan.CustomerId
		LEFT JOIN #tmpOffer ON Customer.Id = #tmpOffer.CustomerId
	WHERE
		CONVERT(DATE, @DateStart) <= GreetingMailSentDate AND GreetingMailSentDate < CONVERT(DATE, @DateEnd)
		AND
		IsTest = 0

	SELECT
		Name,
		DateRegister,
		Shops,
		Payment,
		Personal,
		Complete
	FROM
		#tmp1
	WHERE
		Name NOT LIKE '%ezbob%'
		AND
		Name NOT LIKE '%q.q%'
		AND
		Name NOT LIKE '%liatvanir%'
		AND
		Shops IS NOT NULL
		AND
		Status = 'Registered'

	DROP TABLE #tmp1
	DROP TABLE #tmp2
	DROP TABLE #tmpOffer
END

GO
/****** Object:  StoredProcedure [dbo].[RptNewClientsEKM]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptNewClientsEKM]
@DateStart DATETIME,
@DateEnd   DATETIME
AS
BEGIN
	IF OBJECT_ID('tempdb..#Shops') is not NULL
		DROP TABLE #Shops

	IF OBJECT_ID('tempdb..#EKMCLients') is not NULL
		DROP TABLE #EKMCLients

	IF OBJECT_ID('tempdb..#MaxOffer') is not NULL
		DROP TABLE #MaxOffer

	if OBJECT_ID('tempdb..#SumOfLoans') is not NULL
		DROP TABLE #SumOfLoans

	if OBJECT_ID('tempdb..#tmp1') is not NULL
		DROP TABLE #tmp1

	if OBJECT_ID('tempdb..#AnualTurnover') is not NULL
		DROP TABLE #AnualTurnover

	---- # OF SHOPS PER CUSTOMER ----

	SELECT
		CustomerId,
		COUNT(MarketPlaceId) AS Shops
	INTO
		#Shops
	FROM
		MP_CustomerMarketPlace
	GROUP BY
		CustomerId

	---- MAX OFFER THAT WAS OFFERED TO CUSTOMER ----

	SELECT
		IdCustomer AS CustomerId,
		MAX(ManagerApprovedSum) MaxApproved
	INTO
		#MaxOffer
	FROM
		CashRequests
	GROUP BY
		IdCustomer

	-- EKM CLients

	CREATE TABLE #EKMClients (CustomerId INT)

	INSERT INTO #EKMClients
	SELECT DISTINCT CustomerId
	FROM MP_CustomerMarketPlace S
		INNER JOIN MP_MarketplaceType T ON S.MarketPlaceId = T.Id AND T.Name = 'EKM'
	UNION
	SELECT Id
	FROM Customer
	WHERE ReferenceSource LIKE 'EKM%'

	---- SUM OF LOANS THAT CUST GOT -----

	SELECT
		CustomerId,
		SUM(LoanAmount) SumOfLoans
	INTO
		#SumOfLoans
	FROM
		Loan
	GROUP BY
		CustomerId

	----- CALC FOR ANUAL TURNOVER -----

	SELECT
		IdCustomer AS CustomerId,
		MAX(CreationDate) AS CreationDate
	INTO
		#tmp1
	FROM
		CashRequests
	GROUP BY
		IdCustomer
	ORDER BY
		1

	SELECT
		A.CustomerId,
		A.creationdate,
		R.MedalType,
		R.AnualTurnover,
		R.ExpirianRating
	INTO
		#AnualTurnover
	FROM
		#tmp1 A
		JOIN CashRequests R
			ON R.IdCustomer = A.CustomerId
			AND R.CreationDate = A.CreationDate

	SELECT
		Customer.Id,
		Customer.GreetingMailSentDate AS DateRegister,
		#Shops.Shops,
		#MaxOffer.MaxApproved,
		#SumOfLoans.SumOfLoans,
		Customer.ReferenceSource
	FROM
		Customer
		INNER JOIN #EKMClients ek ON Customer.Id = ek.CustomerId
		LEFT JOIN #Shops ON #Shops.CustomerId = Customer.Id
		LEFT JOIN #MaxOffer ON #MaxOffer.CustomerId = Customer.Id
		LEFT JOIN #SumOfLoans ON #SumOfLoans.CustomerId = Customer.Id
		LEFT JOIN #AnualTurnover T ON T.CustomerId = Customer.Id
	WHERE
		Customer.IsTest = 0
		AND
		Name NOT like '%ezbob%'
		AND
		Name NOT LIKE '%liatvanir%'
		AND
		CONVERT(DATE, @DateStart) <= Customer.GreetingMailSentDate AND Customer.GreetingMailSentDate < CONVERT(DATE, @DateEnd)
	ORDER BY
		SumOfLoans desc

	DROP TABLE #Shops
	DROP TABLE #EKMCLients
	DROP TABLE #MaxOffer
	DROP TABLE #SumOfLoans
	DROP TABLE #tmp1
	DROP TABLE #AnualTurnover
END

GO
/****** Object:  StoredProcedure [dbo].[RptNewClientsFull]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptNewClientsFull]
@DateStart    DATETIME,
@DateEnd      DATETIME
AS
BEGIN
	IF OBJECT_ID('tempdb..#tmp1') is not NULL
		DROP TABLE #tmp1

	IF OBJECT_ID('tempdb..#tmp2') is not NULL
		DROP TABLE #tmp2

	IF OBJECT_ID('tempdb..#tmpOffer') is not NULL
		DROP TABLE #tmpOffer

	SELECT
		CustomerId,
		count(MarketPlaceId) Shops
	INTO
		#tmp2
	FROM
		MP_CustomerMarketPlace
	GROUP BY
		CustomerId

	SELECT
		IdCustomer CustomerId,
		MAX(ManagerApprovedSum) MaxApproved
	INTO
		#tmpOffer
	FROM
		CashRequests
	GROUP BY
		IdCustomer

	SELECT
		Name,
		Customer.FirstName,
		Customer.Surname,
		Customer.DaytimePhone,
		Customer.MobilePhone,
		DATEPART(dw, GreetingMailSentDate) DayOfWeek,
		GreetingMailSentDate DateRegister,
		ReferenceSource,
		#tmp2.Shops,
		Payment = CASE
			WHEN Customer.AccountNumber IS NULL THEN 'NO'
			WHEN Customer.AccountNumber IS NOT NULL THEN 'YES'
		END,
		Personal = CASE
			WHEN Customer.FirstName IS NULL THEN 'NO'
			WHEN Customer.FirstName IS NOT NULL THEN 'YES'
		END,
		Complete = CASE
			WHEN Customer.ApplyForLoan IS NULL THEN 'NO'
			WHEN Customer.ApplyForLoan IS NOT NULL THEN 'YES'
		END,
		CASE WHEN Customer.WizardStep=4 THEN 1 ELSE 0 END AS IsSuccessfullyRegistered,
		Customer.Status,
		Customer.MedalType,
		PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), #tmpOffer.MaxApproved)), 1), 2) MaxApproved,
		PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), Loan.LoanAmount)), 1), 2) LoanAmount,
		PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), Customer.CreditSum)), 1), 2) CreditLeft,
		Loan.[Date],
		DATEPART(dw, Loan.[Date]) LoanDayOfWeek
	INTO
		#tmp1
	FROM
		Customer
		LEFT JOIN #tmp2 ON Customer.Id = #tmp2.CustomerId
		LEFT JOIN Loan ON loan.CustomerId = Customer.Id
		LEFT JOIN #tmpOffer ON #tmpOffer.CustomerId = Customer.Id
	WHERE
		CONVERT(DATE, @DateStart) <= GreetingMailSentDate AND GreetingMailSentDate < CONVERT(DATE, @DateEnd)
		AND
		IsTest = 0

	SELECT
		*
	FROM
		#tmp1
	WHERE
		Name NOT LIKE '%ezbob%'
		AND
		Name NOT LIKE '%q.q%'
		AND
		Name NOT LIKE '%liatvanir%'

	DROP TABLE #tmp1
	DROP TABLE #tmp2
	DROP TABLE #tmpOffer
END

GO
/****** Object:  StoredProcedure [dbo].[RptNewClientsFullEx]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptNewClientsFullEx]
@DateStart DATETIME,
@DateEnd   DATETIME
AS
BEGIN
	IF OBJECT_ID('tempdb..#Shops') IS NOT NULL
		DROP TABLE #Shops

	IF OBJECT_ID('tempdb..#MaxOffer') IS NOT NULL
		DROP TABLE #MaxOffer

	IF OBJECT_ID('tempdb..#SumOfLoans') IS NOT NULL
		DROP TABLE #SumOfLoans

	IF OBJECT_ID('tempdb..#tmp1') IS NOT NULL
		DROP TABLE #tmp1

	IF OBJECT_ID('tempdb..#AnualTurnover') IS NOT NULL
		DROP TABLE #AnualTurnover

	---- # OF SHOPS PER CUSTOMER ----

	SELECT
		CustomerId,
		COUNT(MarketPlaceId) AS Shops
	INTO
		#Shops
	FROM
		MP_CustomerMarketPlace
	GROUP BY
		CustomerId

	---- MAX OFFER THAT WAS OFFERED TO CUSTOMER ----

	SELECT
		IdCustomer AS CustomerId,
		MAX(ManagerApprovedSum) MaxApproved
	INTO
		#MaxOffer
	FROM
		CashRequests
	GROUP BY
		IdCustomer

	---- SUM OF LOANS THAT CUST GOT -----

	SELECT
		CustomerId,
		SUM(LoanAmount) SumOfLoans
	INTO
		#SumOfLoans
	FROM
		Loan
	GROUP BY
		CustomerId

	----- CALC FOR ANUAL TURNOVER -----

	SELECT
		IdCustomer AS CustomerId,
		MAX(CreationDate) AS CreationDate
	INTO
		#tmp1
	FROM
		CashRequests
	GROUP BY
		IdCustomer
	ORDER BY
		1

	SELECT
		A.CustomerId,
		A.creationdate,
		R.MedalType,
		R.AnualTurnover,
		R.ExpirianRating
	INTO
		#AnualTurnover
	FROM
		#tmp1 A
		JOIN CashRequests R ON R.IdCustomer = A.CustomerId
	WHERE
		R.CreationDate = A.CreationDate

	SELECT
		Customer.Id,
		Customer.Name AS eMail,
		Customer.GreetingMailSentDate AS DateRegister,
		Customer.FirstName,
		Customer.Surname SurName,
		Customer.DaytimePhone,
		Customer.MobilePhone,
		#Shops.Shops,
		PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), #MaxOffer.MaxApproved)), 1), 2) MaxApproved,
		PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), #SumOfLoans.SumOfLoans)), 1), 2) SumOfLoans,
		PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), T.AnualTurnover)), 1), 2) AnualTurnover,
		T.ExpirianRating,
		T.MedalType,
		CASE T.MedalType
			WHEN 'Silver'   THEN PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), ROUND(T.AnualTurnover * 0.06, 0))), 1), 2)
			WHEN 'Gold'     THEN PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), ROUND(T.AnualTurnover * 0.08, 0))), 1), 2)
			WHEN 'Platinum' THEN PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), ROUND(T.AnualTurnover * 0.10, 0))), 1), 2)
			WHEN 'Diamond'  THEN PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), ROUND(T.AnualTurnover * 0.12, 0))), 1), 2)
		END PhoneOffer
	FROM
		Customer
		LEFT JOIN #Shops ON Customer.Id = #Shops.CustomerId
		LEFT JOIN #MaxOffer ON Customer.Id = #MaxOffer.CustomerId
		LEFT JOIN #SumOfLoans ON Customer.Id = #SumOfLoans.CustomerId
		LEFT JOIN #AnualTurnover T ON Customer.Id = T.CustomerId
	WHERE
		Customer.IsTest = 0
		AND
		Name NOT LIKE '%ezbob%'
		AND
		Name NOT LIKE '%liatvanir%'
		AND
		CONVERT(DATE, @DateStart) <= Customer.GreetingMailSentDate AND Customer.GreetingMailSentDate < CONVERT(DATE, @DateEnd)
	GROUP BY
		Customer.Id,
		Customer.Name,
		Customer.FirstName,
		Customer.Surname,
		Customer.DaytimePhone,
		Customer.MobilePhone,
		Customer.GreetingMailSentDate,
		#Shops.Shops,
		#MaxOffer.MaxApproved,
		#SumOfLoans.SumOfLoans,
		T.ExpirianRating,
		T.AnualTurnover,
		T.MedalType

	DROP TABLE #Shops
	DROP TABLE #MaxOffer
	DROP TABLE #SumOfLoans
	DROP TABLE #tmp1
	DROP TABLE #AnualTurnover
END

GO
/****** Object:  StoredProcedure [dbo].[RptNewClientsStep1]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptNewClientsStep1]
@DateStart DATETIME,
@DateEnd   DATETIME
AS
BEGIN
	IF OBJECT_ID('tempdb..#tmp1') IS NOT NULL
		DROP TABLE #tmp1

	IF OBJECT_ID('tempdb..#tmp2') IS NOT NULL
		DROP TABLE #tmp2

	IF OBJECT_ID('tempdb..#tmpOffer') IS NOT NULL
		DROP TABLE #tmpOffer

	SELECT
		CustomerId,
		count(MarketPlaceId) Shops
	INTO
		#tmp2
	FROM
		MP_CustomerMarketPlace
	GROUP BY
		CustomerId

	SELECT
		IdCustomer CustomerId,
		max(ManagerApprovedSum) MaxApproved
	INTO
		#tmpOffer
	FROM
		CashRequests
	GROUP BY
		IdCustomer

	SELECT
		Name,
		DATEPART(dw,GreetingMailSentDate) DayOfWeek,
		GreetingMailSentDate DateRegister,
		ReferenceSource,
		#tmp2.Shops,
		Payment = CASE
			WHEN Customer.AccountNumber IS NULL THEN 'NO'
			WHEN Customer.AccountNumber IS NOT NULL THEN 'YES'
		END,
		Personal = CASE
			WHEN Customer.FirstName IS NULL THEN 'NO'
			WHEN Customer.FirstName IS NOT NULL THEN 'YES'
		END,
		Complete = CASE
			WHEN Customer.ApplyForLoan IS NULL THEN 'NO'
			WHEN Customer.ApplyForLoan IS NOT NULL THEN 'YES'
		END,
		CASE WHEN Customer.WizardStep=4 THEN 1 ELSE 0 END AS IsSuccessfullyRegistered,
		Customer.Status,
		Customer.MedalType,
		#tmpOffer.MaxApproved,
		Loan.LoanAmount,
		Customer.CreditSum CreditLeft,
		Loan.[Date],
		DATEPART(dw,Loan.[Date]) LoanDayOfWeek
	INTO
		#tmp1
	FROM
		Customer
		LEFT JOIN #tmp2 ON Customer.Id = #tmp2.CustomerId
		LEFT JOIN Loan ON Customer.Id = Loan.CustomerId
		LEFT JOIN #tmpOffer ON Customer.Id = #tmpOffer.CustomerId
	WHERE
		CONVERT(DATE, @DateStart) <= GreetingMailSentDate AND GreetingMailSentDate < CONVERT(DATE, @DateEnd)
		AND
		IsTest = 0

	SELECT
		Name,
		DateRegister,
		Shops,
		Payment,
		Personal,
		Complete
	FROM
		#tmp1
	WHERE
		Name NOT LIKE '%ezbob%'
		AND
		Name NOT LIKE '%q.q%'
		AND
		Name NOT LIKE '%liatvanir%'
		AND
		Shops IS NULL
		AND
		Status = 'Registered'

	DROP TABLE #tmp1
	DROP TABLE #tmp2
	DROP TABLE #tmpOffer
END

GO
/****** Object:  StoredProcedure [dbo].[RptNewLateClients]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptNewLateClients]
@DateStart DATE,
@DateEnd DATE
AS
SELECT C.Id,C.Name,C.FirstName,C.Surname,AmountDue FROM LoanSchedule S,Customer C,Loan L WHERE C.IsTest = 0 AND C.Id = L.CustomerId AND S.LoanId = L.Id AND S.Date >= @DateStart AND S.Date < @DateEnd AND S.Status IN ('StillToPay','Late') AND C.CollectionStatus != 0

GO
/****** Object:  StoredProcedure [dbo].[RptOpenPayments]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptOpenPayments]
@DateStart DATETIME,
@DateEnd   DATETIME
AS
BEGIN
	SELECT
		C.Id,
		C.Name,
		C.FirstName,
		C.Surname,
		C.DaytimePhone,
		C.MobilePhone,
		CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), L.LoanAmount)), 1) LoanAmount,
		CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), L.InterestRate * 100)), 1) InterestRate,
		CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), S.AmountDue)), 1) AmountDue,
		S.Position + 1 AS Payment,
		S.[Date],
		CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), S.LoanRepayment)), 1) LoanRepayment,
		CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), S.Interest)), 1) Interest,
		CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), S.Fees)), 1) Fees
	FROM 
		LoanSchedule S
		INNER JOIN Loan L ON S.LoanId = L.Id
		INNER JOIN Customer C
			ON L.CustomerId = C.Id
			AND C.IsTest = 0
			AND C.Id NOT IN (381, 1216, 492, 1013, 938, 368, 460, 792, 347, 517, 522, 394)
	WHERE 
		S.[Date] < CONVERT(DATE, @DateEnd)
		AND
		S.Status NOT IN ('PaidEarly', 'PaidOnTime')
		AND
		S.AmountDue > 0
	ORDER BY
		C.Surname
END

GO
/****** Object:  StoredProcedure [dbo].[RptOverallStats]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptOverallStats]
@DateStart DATETIME,
@DateEnd   DATETIME
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @Money_Given FLOAT = (
		SELECT
			SUM(t.Amount)
		FROM
			LoanTransaction t
			INNER JOIN Loan l ON t.LoanId = l.Id
			INNER JOIN Customer c ON l.CustomerId = c.Id
		WHERE
			t.Status = 'Done'
			AND
			t.Type = 'PacnetTransaction'
			AND
			c.IsTest = 0
	)

	DECLARE @Money_Repaid FLOAT = (
		SELECT
			SUM(t.Principal)
		FROM
			LoanTransaction t
			INNER JOIN Loan l ON t.LoanId = l.Id
			INNER JOIN Customer c ON l.CustomerId = c.Id
		WHERE
			t.Status = 'Done'
			AND
			t.Type = 'PaypointTransaction'
			AND
			c.IsTest = 0
	)

	DECLARE @Money_Out FLOAT = @Money_Given - @Money_Repaid

	-- DATEDIFF returns number of months since begining of time.
	-- The inner DATEADD creates the date "the first day of the next month".
	DECLARE @NextMonthStart DATETIME = (
		DATEADD(month,
			DATEDIFF(month, 0, GETDATE()) + 1,
		0)
	)
	-- SQL Server 2012 contains EOMONTH function.

	SELECT
		y.LineId,
		y.Name,
		y.Value
	FROM (
		SELECT
			11 LineId,
			'Total Anual Shop Revenue that where given loans' Name,
			-- Высокие... высокие отношения!
			PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), SUM(v.ValueFloat))), 1), 2) Value
		FROM
			MP_AnalyisisFunctionValues v
			INNER JOIN MP_AnalyisisFunction f ON v.AnalyisisFunctionId = f.Id AND f.Name = 'TotalSumOfOrders'
			INNER JOIN MP_AnalysisFunctionTimePeriod t ON v.AnalysisFunctionTimePeriodId = t.Id AND t.Name = '365'

		UNION

		SELECT
			10 LineId,
			'Total Money to be Repaid Until End of Month' Name,
			-- Не менее высокие отношения...
			PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), SUM(s.AmountDue))), 1), 2) Value
		FROM
			LoanSchedule s
			INNER JOIN Loan l ON s.LoanId = l.Id
			INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
		WHERE
			s.Status = 'StillToPay'
			AND
			GETDATE() <= s.[Date] AND s.[Date] < @NextMonthStart

		UNION

		SELECT
			2 LineId,
			'Total Money Given' Name,
			PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), @Money_Given)), 1), 2) Value

		UNION

		SELECT
			3 LineId,
			'Total Money Repaid' Name,
			PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), @Money_Repaid)), 1), 2) Value

		UNION

		SELECT
			1 LineId,
			'Total Money Out' Name,
			PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), @Money_Out)), 1), 2) Value

		UNION

		SELECT
			7 LineId,
			'Setup Fee' Name,
			PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), SUM(ISNULL(Fees, 0)))), 1), 2) Value
		FROM
			LoanTransaction
		WHERE
			Type = 'PacnetTransaction'
			AND
			Status = 'Done'

		UNION

		SELECT
			5 LineId,
			'Interest Back' Name,
			PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), SUM(LoanTransaction.Interest))), 1), 2) Value
		FROM
			LoanTransaction
			INNER JOIN Loan l ON LoanTransaction.LoanId = l.Id
			INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
		WHERE
			LoanTransaction.Type = 'PaypointTransaction'
			AND
			LoanTransaction.Status = 'Done'

		UNION

		SELECT
			4 LineId,
			'Principal Back' Name,
			PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), SUM(LoanTransaction.LoanRepayment))), 1), 2) Value
		FROM
			LoanTransaction
			INNER JOIN Loan l ON LoanTransaction.LoanId = l.Id
			INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
		WHERE
			LoanTransaction.Type = 'PaypointTransaction'
			AND
			LoanTransaction.Status = 'Done'

		UNION

		SELECT
			6 LineId,
			'Fees Back' Name,
			PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), SUM(LoanTransaction.Fees))), 1), 2) Value
		FROM
			LoanTransaction
			INNER JOIN Loan l ON LoanTransaction.LoanId = l.Id
			INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
		WHERE
			LoanTransaction.Type = 'PaypointTransaction'
			AND
			LoanTransaction.Status = 'Done'

		UNION

		SELECT
			8 LineId,
			'Late Money' Name,
			PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), SUM(x.Amount))), 1), 2) Value
		FROM (
			SELECT
				LoanSchedule.LoanId,
				MAX(LoanSchedule.AmountDue) Amount
			FROM
				LoanSchedule
				INNER JOIN Loan l ON LoanSchedule.LoanId = l.Id
				INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
			WHERE
				(LoanSchedule.Status='StillToPay' AND LoanSchedule.[Date] < GETDATE())
				OR
				(LoanSchedule.Status='Late' AND LoanSchedule.RepaymentAmount = 0)
			GROUP BY
				LoanSchedule.LoanId
		) x

		UNION

		SELECT
			9 LineId,
			'Late Principal' Name,
			PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), SUM(x.Amount))), 1), 2) Value
		FROM (
			SELECT
				LoanSchedule.LoanId,
				max(LoanSchedule.LoanRepayment) Amount
			FROM
				LoanSchedule
				INNER JOIN Loan l ON LoanSchedule.LoanId = l.Id
				INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
			WHERE
				(LoanSchedule.Status = 'StillToPay' AND LoanSchedule.[Date] < GETDATE())
				OR
				(LoanSchedule.Status = 'Late' AND LoanSchedule.RepaymentAmount = 0)
			GROUP BY
				LoanId
		) x
	) y
	ORDER BY
		y.LineId
END

GO
/****** Object:  StoredProcedure [dbo].[RptPacnetReconciliation]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptPacnetReconciliation]
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	DECLARE @Date DATE
	DECLARE @EzbobOut DECIMAL(18, 2)
	DECLARE @PacnetOut DECIMAL(18, 2)
	DECLARE @EzbobIn DECIMAL(18, 2)
	DECLARE @PacnetIn DECIMAL(18, 2)

	DECLARE @EzbobOutCount INT
	DECLARE @PacnetOutCount INT
	DECLARE @EzbobInCount INT
	DECLARE @PacnetInCount INT

	DECLARE @Amount DECIMAL(18, 2), @IsCredit BIT, @EzbobCount INT, @PacnetCount INT

	SELECT @Date = CONVERT(DATE, @DateStart)

	CREATE TABLE #pacnet (
		Amount DECIMAL(18, 2) NOT NULL,
		IsCredit BIT NOT NULL,
		Counter INT NOT NULL
	)

	CREATE TABLE #ezbob (
		Amount DECIMAL(18, 2) NOT NULL,
		IsCredit BIT NOT NULL,
		Counter INT NOT NULL
	)

	CREATE TABLE #res (
		Amount DECIMAL(18, 2) NOT NULL,
		IsCredit BIT NOT NULL,
		EzbobCount INT NOT NULL,
		PacnetCount INT NOT NULL
	)

	CREATE TABLE #out (
		SortOrder INT IDENTITY(1, 1) NOT NULL,
		Caption NVARCHAR(1000) NOT NULL,
		EzbobAmount DECIMAL(18, 2) NULL,
		EzbobCount INT NULL,
		PacnetAmount DECIMAL(18, 2) NULL,
		PacnetCount INT NULL,
		TransactionID INT NULL,
		Css NVARCHAR(128) NULL
	)

	INSERT INTO #pacnet
	SELECT
		Amount,
		IsCredit,
		COUNT(*)
	FROM
		PacNetBalance
	WHERE
		CONVERT(DATE, Date) = @Date
	GROUP BY
		Amount,
		IsCredit

	INSERT INTO #ezbob
	SELECT
		Amount,
		0,
		COUNT(*)
	FROM
		LoanTransaction
	WHERE
		Type = 'PacnetTransaction'
		AND
		Status = 'Done'
		AND
		CONVERT(DATE, PostDate) = @Date
	GROUP BY
		Amount

	INSERT INTO #res
	SELECT
		e.Amount,
		e.IsCredit,
		ISNULL(e.Counter, 0),
		ISNULL(p.Counter, 0)
	FROM
		#ezbob e
		LEFT JOIN #pacnet p ON e.Amount = p.Amount AND e.IsCredit = p.IsCredit

	INSERT INTO #res
	SELECT
		p.Amount,
		p.IsCredit,
		ISNULL(e.Counter, 0),
		ISNULL(p.Counter, 0)
	FROM
		#ezbob e
		RIGHT JOIN #pacnet p ON e.Amount = p.Amount AND e.IsCredit = p.IsCredit
	WHERE
		e.Amount IS NULL

	DELETE FROM #res WHERE EzbobCount = PacnetCount

	SELECT
		@PacnetIn = ISNULL(SUM(ISNULL(Amount * Counter, 0)), 0),
		@PacnetInCount = ISNULL(SUM(Counter), 0)
	FROM
		#pacnet
	WHERE
		IsCredit = 1

	SELECT
		@PacnetOut = ISNULL(SUM(ISNULL(Amount * Counter, 0)), 0),
		@PacnetOutCount = ISNULL(SUM(Counter), 0)
	FROM
		#pacnet
	WHERE
		IsCredit = 0

	SELECT
		@EzbobOut = ISNULL(SUM(ISNULL(Amount * Counter, 0)), 0),
		@EzbobOutCount = ISNULL(SUM(Counter), 0)
	FROM
		#ezbob
	WHERE
		IsCredit = 0

	SELECT
		@EzbobIn = ISNULL(SUM(ISNULL(Amount * Counter, 0)), 0),
		@EzbobInCount = ISNULL(SUM(Counter), 0)
	FROM
		#ezbob
	WHERE
		IsCredit = 1

	INSERT INTO #out (Caption, EzbobAmount, EzbobCount, PacnetAmount, PacnetCount, Css)
		VALUES ('Total In', @EzbobIn, @EzbobInCount, @PacnetIn, @PacnetInCount, 'total' + CASE WHEN @EzbobIn = @PacnetIn THEN '' ELSE ' unmatched' END)

	DECLARE cur CURSOR FOR
		SELECT Amount, IsCredit, EzbobCount, PacnetCount
		FROM #res
		WHERE IsCredit = 1
		ORDER BY Amount, IsCredit

	OPEN cur

	FETCH NEXT FROM cur INTO @Amount, @IsCredit, @EzbobCount, @PacnetCount

	WHILE @@FETCH_STATUS = 0
	BEGIN
		INSERT INTO #out (Caption, EzbobAmount, PacnetAmount, Css)
			VALUES (
				'Unmatched ' +
				(CASE @IsCredit WHEN 1 THEN 'credit' ELSE 'debit' END) +
				' ' + CONVERT(NVARCHAR, @Amount),
				@EzbobCount,
				@PacnetCount,
				'unmatched'
			)

		INSERT INTO #out(Caption, TransactionID)
		SELECT
			'Transaction',
			t.Id
		FROM
			LoanTransaction t
		WHERE
			CONVERT(DATE, t.PostDate) = @Date
			AND
			t.Status = 'Done'
			AND
			t.Type = 'PacnetTransaction'
			AND
			t.Amount = @Amount

		FETCH NEXT FROM cur INTO @Amount, @IsCredit, @EzbobCount, @PacnetCount
	END

	CLOSE cur
	DEALLOCATE cur

	INSERT INTO #out (Caption, EzbobAmount, EzbobCount, PacnetAmount, PacnetCount, Css)
		VALUES ('Total Out', @EzbobOut, @EzbobOutCount, @PacnetOut, @PacnetOutCount, 'total' + CASE WHEN @EzbobOut = @PacnetOut THEN '' ELSE ' unmatched' END)

	DECLARE cur CURSOR FOR
		SELECT Amount, IsCredit, EzbobCount, PacnetCount
		FROM #res
		WHERE IsCredit = 0
		ORDER BY Amount, IsCredit

	OPEN cur

	FETCH NEXT FROM cur INTO @Amount, @IsCredit, @EzbobCount, @PacnetCount

	WHILE @@FETCH_STATUS = 0
	BEGIN
		INSERT INTO #out (Caption, EzbobAmount, PacnetAmount, Css)
			VALUES (
				'Unmatched ' +
				(CASE @IsCredit WHEN 1 THEN 'credit' ELSE 'debit' END) +
				' ' + CONVERT(NVARCHAR, @Amount),
				@EzbobCount,
				@PacnetCount,
				'unmatched'
			)

		INSERT INTO #out(Caption, TransactionID)
		SELECT
			'Transaction',
			t.Id
		FROM
			LoanTransaction t
		WHERE
			CONVERT(DATE, t.PostDate) = @Date
			AND
			t.Status = 'Done'
			AND
			t.Type = 'PacnetTransaction'
			AND
			t.Amount = @Amount

		FETCH NEXT FROM cur INTO @Amount, @IsCredit, @EzbobCount, @PacnetCount
	END

	CLOSE cur
	DEALLOCATE cur

	SELECT
		o.SortOrder,
		o.Caption,
		o.EzbobAmount,
		o.EzbobCount,
		o.PacnetAmount,
		o.PacnetCount,
		t.Id,
		t.PostDate,
		t.LoanId,
		c.Id AS ClientID,
		c.Name AS ClientEmail,
		c.FirstName + ' ' + c.MiddleInitial + ' ' + c.Surname AS ClientName,
		t.Description,
		o.Css
	FROM
		#out o
		LEFT JOIN LoanTransaction t ON o.TransactionID = t.Id
		LEFT JOIN Loan l ON t.LoanId = l.Id
		LEFT JOIN Customer c ON l.CustomerId = c.Id
	ORDER BY
		SortOrder

	DROP TABLE #out
	DROP TABLE #res
	DROP TABLE #ezbob
	DROP TABLE #pacnet
END

GO
/****** Object:  StoredProcedure [dbo].[RptPaymentReport]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptPaymentReport]
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	SELECT
		LoanSchedule.Id,
		Customer.FirstName,
		Customer.Surname,
		Customer.Name,
		LoanSchedule.[Date],
		AmountDue
	FROM
		LoanSchedule
		INNER JOIN Loan ON Loan.Id = LoanSchedule.LoanId
		INNER JOIN Customer ON Customer.Id = Loan.CustomerId
	WHERE
		LoanSchedule.Status = 'StillToPay'
		AND
		Customer.IsTest = 0
		AND
		CONVERT(DATE, @DateStart) <= LoanSchedule.[Date] AND LoanSchedule.[Date] < CONVERT(DATE, @DateEnd)
	ORDER BY
		LoanSchedule.DATE
END

GO
/****** Object:  StoredProcedure [dbo].[RptPaymentsReceived]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptPaymentsReceived]
@DateStart DATETIME,
@DateEnd   DATETIME,
@ShowNonCashTransactions BIT = NULL
AS
BEGIN
	CREATE TABLE #t (
		PostDate DATETIME NULL,
		LoanID INT NOT NULL,
		ClientID INT NOT NULL,
		ClientEmail NVARCHAR(250) NOT NULL,
		ClientName NVARCHAR(752) NOT NULL,
		Amount NUMERIC(18, 2) NOT NULL,
		LoanRepayment NUMERIC(18, 4) NOT NULL,
		Interest NUMERIC(18, 2) NOT NULL,
		Fees NUMERIC(18, 2) NOT NULL,
		Rollover NUMERIC(18, 4) NOT NULL,
		TransactionType NVARCHAR(64) NOT NULL,
		Description NTEXT,
		SumMatch NVARCHAR(9) NOT NULL,
		RowLevel NVARCHAR(5) NOT NULL
	)

	INSERT INTO #t
	SELECT
		t.PostDate,
		t.LoanId,
		c.Id AS ClientID,
		c.Name AS ClientEmail,
		c.Fullname AS ClientName,
		ISNULL(t.Amount, 0),
		ISNULL(t.LoanRepayment, 0),
		ISNULL(t.Interest, 0),
		ISNULL(t.Fees, 0),
		ISNULL(t.Rollover, 0),
		m.Name AS TransactionType,
		t.Description,
		CASE
			WHEN t.LoanRepayment + t.Interest + t.Fees + t.Rollover = t.Amount
				THEN ''
			ELSE 'unmatched'
		END AS SumMatch,
		'' AS RowLevel
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id
		INNER JOIN LoanTransactionMethod m ON t.LoanTransactionMethodId = m.Id
	WHERE
		CONVERT(DATE, @DateStart) <= t.PostDate AND t.PostDate < CONVERT(DATE, @DateEnd)
		AND
		t.Status = 'Done'
		AND
		c.IsTest = 0
		AND
		t.Type = 'PaypointTransaction'
		AND
		(
			@ShowNonCashTransactions IS NULL
			OR
			(@ShowNonCashTransactions = 0 AND m.Name != 'Non-Cash')
			OR
			@ShowNonCashTransactions = 1
		)

	INSERT INTO #t
	SELECT
		NULL,
		COUNT(DISTINCT LoanID),
		ISNULL(COUNT(DISTINCT ClientID), 0),
		'' AS ClientEmail,
		'Total' AS ClientName,
		ISNULL(SUM(Amount), 0),
		ISNULL(SUM(LoanRepayment), 0),
		ISNULL(SUM(Interest), 0),
		ISNULL(SUM(Fees), 0),
		ISNULL(SUM(Rollover), 0),
		'',
		'',
		'' AS SumMatch,
		'total' AS RowLevel
	FROM
		#t
	WHERE
		RowLevel = ''

	SELECT
		PostDate,
		LoanId,
		ClientID,
		ClientEmail,
		ClientName,
		Amount,
		LoanRepayment,
		Interest,
		Fees,
		Rollover,
		TransactionType,
		Description,
		SumMatch,
		RowLevel
	FROM
		#t
	ORDER BY
		RowLevel DESC,
		PostDate
END

GO
/****** Object:  StoredProcedure [dbo].[RptPaypalEbayPhones]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptPaypalEbayPhones]

		@DateStart DATETIME,
		@DateEnd   DATETIME
AS
BEGIN

SET @DateStart = CONVERT(DATE, @DateStart)
SET @DateEnd = CONVERT(DATE, @DateEnd)

IF datediff(day, @DateStart, @DateEnd) = 1 SET @DateStart = dateadd(month, -1,@DateEnd)


IF OBJECT_ID('tempdb..#temp1') IS NOT NULL
BEGIN
                DROP TABLE #temp1
END
 
IF OBJECT_ID('tempdb..#temp2') IS NOT NULL
BEGIN
                DROP TABLE #temp2
END

IF OBJECT_ID('tempdb..#MP1') IS NOT NULL
BEGIN
                DROP TABLE #MP1
END

 
IF OBJECT_ID('tempdb..#MP_temp1') IS NOT NULL
BEGIN
                DROP TABLE #MP_temp1
END

----------- GET PHONE FROM CUSTOMERS WHO ADDED PAYPAL ---------

SELECT M.CustomerId,C.Name AS email,C.Fullname,C.GreetingMailSentDate AS SignUpDate,P.Phone
INTO #temp1
FROM MP_PayPalPersonalInfo P
JOIN MP_CustomerMarketPlace M ON M.Id = P.CustomerMarketPlaceId
JOIN Customer C ON C.Id = M.CustomerId
WHERE C.Id NOT IN 
				  ( SELECT C.Id
					FROM Customer C 
					WHERE Name LIKE '%ezbob%'
					OR Name LIKE '%liatvanir%'
					OR Name LIKE '%q@q%'
					OR Name LIKE '%1@1%'
					OR C.IsTest=1)
	  AND C.Fullname IS NULL	

----------- GET PHONE FROM CUSTOMERS WHO ADDED EBAY  ---------

SELECT 	M3.CustomerId,
		C.Name AS email,
		C.Fullname,
		C.GreetingMailSentDate AS SignUpDate,
		M1.Phone
INTO #temp2
FROM MP_EbayUserAddressData M1,
	 MP_EbayUserData M2,
	 MP_CustomerMarketPlace M3
JOIN Customer C ON C.Id = M3.CustomerId
WHERE 	M1.Id = M2.RegistrationAddressId 
		AND M2.CustomerMarketPlaceId = M3.Id 
		AND C.Fullname IS NULL
		AND C.Id NOT IN 
				  ( SELECT C.Id
					FROM Customer C 
					WHERE Name LIKE '%ezbob%'
					OR Name LIKE '%liatvanir%'
					OR Name LIKE '%q@q%'
					OR Name LIKE '%1@1%'
					OR C.IsTest=1)
GROUP BY M3.CustomerId,
		 C.Name,
		 C.Fullname,
		 C.GreetingMailSentDate,
		 M1.Phone


-------------- EBAY & PAYPAL TURNOVER ------------------
----------- MARKET PLACE TYPE = 1 (EBAY) ---------------
----------- MARKET PLACE TYPE = 3 (PAYPAL) -------------

SELECT A.CustomerMarketPlaceId,max(A.Updated) UpdatedDate,
	   CASE 	
	   WHEN A.AnalyisisFunctionId = 7 THEN 'eBay'
	   WHEN A.AnalyisisFunctionId = 50 THEN 'PayPal'
	   END AS MarketPlaceName
INTO #MP1
FROM MP_AnalyisisFunctionValues A
WHERE A.AnalyisisFunctionId IN (7,50)  
	AND A.AnalysisFunctionTimePeriodId = 4
GROUP BY A.CustomerMarketPlaceId,
		CASE 	
	    WHEN A.AnalyisisFunctionId = 7 THEN 'eBay'
	    WHEN A.AnalyisisFunctionId = 50 THEN 'PayPal'
	    END


SELECT C.CustomerId,max(round(B.ValueFloat,1)) AS AnualSales,convert(DATE,B.Updated) AS LastUpdateDate
INTO #MP_temp1
FROM #MP1 A,
	 MP_AnalyisisFunctionValues B,
	 MP_CustomerMarketPlace C
WHERE 	A.CustomerMarketPlaceId = B.CustomerMarketPlaceId
		AND A.UpdatedDate = B.Updated
		AND A.CustomerMarketPlaceId = C.Id
	    AND B.AnalyisisFunctionId IN (7,50)
		AND B.AnalysisFunctionTimePeriodId = 4
GROUP BY C.CustomerId,convert(DATE,B.Updated)
HAVING max(round(B.ValueFloat,1)) >= 10000
ORDER BY 1


--------- FINAL TABLE - PAYPAL & EBAY PHONES ----------

SELECT T2.CustomerId,T2.email,T2.SignUpDate,max(MP.AnualSales) AS Turnover,T2.Phone AS eBayPhone,T1.Phone AS PayPalPhone
FROM #temp2 T2  
LEFT JOIN #temp1 T1 ON T1.CustomerId = T2.CustomerId
JOIN #MP_temp1 MP ON MP.CustomerId = T2.CustomerId
JOIN Customer C ON C.Id = T2.CustomerId
WHERE T2.CustomerId NOT IN (SELECT R.IdCustomer FROM CashRequests R)
	  AND T2.SignUpDate >= @DateStart
	  AND T2.SignUpDate <= @DateEnd
GROUP BY T2.CustomerId,T2.email,T2.SignUpDate,T2.Phone,T1.Phone,C.WizardStep


DROP TABLE #temp1
DROP TABLE #temp2
DROP TABLE #MP1
DROP TABLE #MP_temp1


END

GO
/****** Object:  StoredProcedure [dbo].[RptPaypointReconciliation]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptPaypointReconciliation]
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	DECLARE @Date DATE
	DECLARE @IncludeFive BIT

	SELECT @Date = CONVERT(DATE, @DateStart)

	IF EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'Recon_Paypoint_Include_Five')
		SET @IncludeFive = CASE (SELECT Value FROM ConfigurationVariables WHERE Name = 'Recon_Paypoint_Include_Five') WHEN 'yes' THEN 1 ELSE 0 END

	CREATE TABLE #out (
		SortOrder INT IDENTITY(1, 1) NOT NULL,
		Caption NVARCHAR(512) NULL,
		Amount DECIMAL(18, 2) NULL,
		TranID INT NULL,
		Css NVARCHAR(128) NULL
	)

	INSERT INTO #out (Caption) VALUES ('Transactions of Amount 5 Are ' + (CASE @IncludeFive WHEN 1 THEN 'Included' ELSE 'Excluded' END))

	EXECUTE PaypointOneTypeReconciliation @Date, @IncludeFive, 1, 'Successful'

	EXECUTE PaypointOneTypeReconciliation @Date, @IncludeFive, 0, 'Failed'

	SELECT
		o.SortOrder,
		o.Caption,
		ISNULL(o.Amount, (CASE o.Caption WHEN 'Ezbob' THEN t.Amount ELSE b.amount END)) AS Amount,
		o.TranID,
		(CASE o.Caption WHEN 'Ezbob' THEN t.PostDate ELSE b.date END) AS Date,
		(CASE o.Caption WHEN 'Ezbob' THEN c.Id ELSE NULL END) AS ClientID,
		(CASE o.Caption WHEN 'Ezbob' THEN NULL ELSE b.lastfive END) AS CardNo,
		(CASE o.Caption WHEN 'Ezbob' THEN c.Fullname ELSE b.name END) AS ClientName,
		(CASE o.Caption WHEN 'Ezbob' THEN t.Description ELSE b.message END) AS Description,
		(CASE o.Caption WHEN 'Ezbob' THEN NULL ELSE b.trans_id END) AS NativePaypointID,
		o.Css
	FROM
		#out o
		LEFT JOIN LoanTransaction t ON o.TranID = t.Id
		LEFT JOIN Loan l ON t.LoanId = l.Id
		LEFT JOIN Customer c ON l.CustomerId = c.Id
		LEFT JOIN PayPointBalance b ON o.TranID = b.Id
	ORDER BY
		SortOrder

	DROP TABLE #out
END

GO
/****** Object:  StoredProcedure [dbo].[RptSaleStats]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptSaleStats]
@DateStart DATETIME,
@DateEnd   DATETIME,
@CustomerID INT = NULL,
@CustomerNameOrEmail NVARCHAR(256) = NULL
AS
BEGIN
	SELECT
		max(CR.Id) CrmId,
		CR.CustomerId
	INTO
		#CRMNotes
	FROM
		CustomerRelations CR
		INNER JOIN CashRequests O
			ON O.IdCustomer = CR.CustomerId
			AND O.UnderwriterDecision = 'Approved'
		INNER JOIN CRMStatuses sts ON CR.StatusId = sts.Id
		INNER JOIN Customer C ON CR.CustomerId = C.Id
	WHERE
		@DateStart <= O.CreationDate AND O.CreationDate < @DateEnd
		AND
		C.IsTest = 0
		AND
		(@CustomerID IS NULL OR @CustomerID = CR.CustomerId)
		AND
		(
			@CustomerNameOrEmail IS NULL
			OR
			@CustomerID IS NOT NULL
			OR
			(
				C.Name LIKE '%' + @CustomerNameOrEmail + '%'
				OR
				C.FullName LIKE '%' + @CustomerNameOrEmail + '%'
			)
		)
	GROUP BY
		CR.CustomerId

	------------------------------------------------------------------------------

	SELECT
		CR.CustomerId,
		CR.UserName,
		sts.Name,
		CR.Comment
	INTO
		#CRMFinal
	FROM
		CustomerRelations CR
		INNER JOIN #CRMNotes N ON CR.Id = N.CrmId
		INNER JOIN CRMStatuses sts ON CR.StatusId = sts.Id

	------------------------------------------------------------------------------

	SELECT 
		C.Id,
		C.Name AS Email,
		C.FullName,
		O.UnderwriterDecision,
		O.UnderwriterDecisionDate,
		O.ManagerApprovedSum,
		O.UnderwriterComment,
		L.LoanAmount,
		CR.Name AS CRMStatus,
		CR.Comment,
		CASE 
     		WHEN C.IsOffline = 1 THEN 'Offline'
            ELSE 'Online'
  		END AS SegmentType
	FROM
		Customer C
		INNER JOIN CashRequests O
			ON C.Id = O.IdCustomer
			AND O.UnderwriterDecision = 'Approved'
		LEFT JOIN Loan L
			ON O.Id = L.RequestCashId
		LEFT JOIN #CRMFinal CR ON CR.CustomerId = O.IdCustomer
	WHERE
		@DateStart <= O.CreationDate AND O.CreationDate < @DateEnd
		AND
		C.IsTest = 0
		AND
		(@CustomerID IS NULL OR @CustomerID = C.Id)
		AND
		(
			@CustomerNameOrEmail IS NULL
			OR
			@CustomerID IS NOT NULL
			OR
			(
				C.Name LIKE '%' + @CustomerNameOrEmail + '%'
				OR
				C.FullName LIKE '%' + @CustomerNameOrEmail + '%'
			)
		)
	ORDER BY
		O.CreationDate DESC
	
	------------------------------------------------------------------------------

	DROP TABLE #CRMNotes
	DROP TABLE #CRMFinal
END


GO
/****** Object:  StoredProcedure [dbo].[RptScheduler_GetHeaderAndFields]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptScheduler_GetHeaderAndFields]
@Type NVARCHAR(200)
AS
BEGIN
	SELECT
		Header,
		Fields
	FROM
		ReportScheduler
	WHERE
		Type = @Type
END

GO
/****** Object:  StoredProcedure [dbo].[RptScheduler_GetReportArgs]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptScheduler_GetReportArgs]
@RptType NVARCHAR(200) = NULL
AS
SELECT
	r.Id AS ReportID,
	r.Type AS ReportType,
	n.Id AS ArgumentID,
	n.Name AS ArgumentName
FROM
	ReportScheduler r
	INNER JOIN ReportArguments a ON r.Id = a.ReportId
	INNER JOIN ReportArgumentNames n ON a.ReportArgumentNameId = n.Id
WHERE
	@RptType IS NULL
	OR
	r.Type = @RptType
ORDER BY
	r.Type,
	n.Name

GO
/****** Object:  StoredProcedure [dbo].[RptScheduler_GetReportList]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptScheduler_GetReportList]
@RptType NVARCHAR(200) = NULL
AS
BEGIN
	SELECT
		Type,
		Title,
		StoredProcedure,
		IsDaily,
		IsWeekly,
		IsMonthly,
		IsMonthToDate,
		Header,
		Fields,
		ToEmail
	FROM
		ReportScheduler
	WHERE
		@RptType IS NULL OR LTRIM(RTRIM(@RptType)) = ''
		OR
		Type = @RptType
END

GO
/****** Object:  StoredProcedure [dbo].[RptSiteAnalytics]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptSiteAnalytics]
@DateStart DATETIME,
@DateEnd   DATETIME
AS
BEGIN
	SELECT
		sa.[Date],
		sac.Name,
		sac.Description,
		sa.SiteAnalyticsValue
	FROM
		SiteAnalytics sa
		INNER JOIN SiteAnalyticsCodes sac ON sa.SiteAnalyticsCode = sac.Id
	WHERE
		CONVERT(DATE, @DateStart) <= sa.[Date] AND sa.[Date] < CONVERT(DATE, @DateEnd)
END

GO
/****** Object:  StoredProcedure [dbo].[RptSourceRef]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptSourceRef]
@DateStart DATETIME,
@DateEnd   DATETIME
AS
BEGIN
	SET NOCOUNT ON	

	SET @DateStart = CONVERT(DATE, @DateStart)
	SET @DateEnd = CONVERT(DATE, @DateEnd)

	CREATE TABLE #out (
		TypeID INT NOT NULL,
		CustomerID INT NOT NULL,
		CashRequestID INT NULL,
		LoanID INT NULL,
		Status NVARCHAR(3),
		LoanCount INT NULL,
		MarketPlaces NVARCHAR(4000) NOT NULL DEFAULT ''
	)

	INSERT INTO #out (TypeID, CustomerID, Status, LoanCount)
	SELECT
		1,
		C.Id,
		'New',
		0
	FROM
		Customer C
	WHERE
		C.IsTest = 0
		AND
		@DateStart <= C.GreetingMailSentDate AND C.GreetingMailSentDate < @DateEnd

	INSERT INTO #out (TypeID, CustomerId, CashRequestID)
	SELECT
		2,
		R.IdCustomer,
		R.Id
	FROM 
		CashRequests R
		INNER JOIN Customer C ON C.Id = R.IdCustomer AND C.IsTest = 0
	WHERE
		@DateStart <= R.CreationDate AND R.CreationDate < @DateEnd

	INSERT INTO #out (TypeID, CustomerId, CashRequestID)
	SELECT
		3,
		R.IdCustomer,
		R.Id
	FROM 
		CashRequests R
		INNER JOIN Customer C ON C.Id = R.IdCustomer AND C.IsTest = 0
	WHERE
		@DateStart <= R.UnderwriterDecisionDate AND R.UnderwriterDecisionDate < @DateEnd
		AND
		R.UnderwriterDecision = 'Approved'

	INSERT INTO #out (TypeID, CustomerId, LoanID)
	SELECT
		4,
		L.CustomerId,
		L.Id
	FROM
		Loan L
		INNER JOIN Customer C ON L.CustomerId = C.Id AND C.IsTest = 0
	WHERE 
		@DateStart <= L.[Date] AND L.[Date] < @DateEnd

	UPDATE #out SET
		CashRequestID = (
			SELECT
				MAX(R.Id)
			FROM
				CashRequests R
				INNER JOIN Loan L
					ON R.IdCustomer = L.CustomerId
					AND R.UnderwriterDecisionDate <= L.Date
			WHERE
				#out.CustomerID = R.IdCustomer
		)
	WHERE
		LoanID IS NOT NULL

	UPDATE #out SET
		Status = (CASE
			WHEN EXISTS (
				SELECT
					Id
				FROM
					CashRequests R
				WHERE
					R.IdCustomer = #out.CustomerID
					AND
					R.Id < #out.CashRequestID
			) THEN 'Old'
			ELSE 'New'
		END)
	WHERE
		CashRequestID IS NOT NULL

	UPDATE #out SET
		LoanCount = ISNULL((
			SELECT COUNT (*)
			FROM Loan L
			WHERE L.CustomerId = #out.CustomerID
		), 0)
	WHERE
		CashRequestID IS NOT NULL

	UPDATE #out SET
		MarketPlaces = dbo.udfCustomerMarketPlaces(CustomerID)

	SELECT
		(CASE o.TypeID
			WHEN 1 THEN '1: Registered'
			WHEN 2 THEN '2: Applied'
			WHEN 3 THEN '3: Approved'
			WHEN 4 THEN '4: Issued'
		END) AS Type,
		o.CustomerID,
		c.GreetingMailSentDate,
		c.ReferenceSource,
		c.WizardStep,
		r.CreationDate,
		r.UnderwriterDecision,
		r.ManagerApprovedSum,
		l.LoanAmount,
		o.Status,
		o.LoanCount,
		o.MarketPlaces,
		r.InterestRate,
		l.Date AS LoanDate,
		c.FullName,
		c.Name AS Email
	FROM
		#out o
		INNER JOIN Customer c ON o.CustomerID = c.Id
		LEFT JOIN CashRequests r ON o.CashRequestID = r.Id
		LEFT JOIN Loan l ON o.LoanID = l.Id
	ORDER BY
		o.TypeID

	DROP TABLE #out

	SET NOCOUNT OFF
END

GO
/****** Object:  StoredProcedure [dbo].[RptStatsDaily]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptStatsDaily]
@DateStart DATETIME,
@DateEnd   DATETIME
AS
BEGIN
	SELECT
		@DateStart = CONVERT(DATE, @DateStart),
		@DateEnd = CONVERT(DATE, @DateEnd)

	------------------------------------------------------------------------------

	CREATE TABLE #output (
		SortOrder INT IDENTITY(1, 1) NOT NULL,
		Caption NVARCHAR(128),
		Counter INT,
		Decision NVARCHAR(128),
		Value DECIMAL(18, 2),
		Css NVARCHAR(128)
	)

	------------------------------------------------------------------------------

	SELECT DISTINCT
		CustomerId
	INTO
		#OldLoanCustomers
	FROM
		Loan
	WHERE
		[Date] < @DateStart
		
	------------------------------------------------------------------------------

	SELECT DISTINCT
		IdCustomer
	INTO
		#TodayClients
	FROM
		CashRequests
	WHERE
		@DateStart <= CreationDate AND CreationDate < @DateEnd

	------------------------------------------------------------------------------

	SELECT DISTINCT
		C.IdCustomer
	INTO
		#PastClients
	FROM
		CashRequests C
		INNER JOIN #TodayClients T ON C.IdCustomer = T.IdCustomer
	WHERE
		CreationDate < @DateStart

	------------------------------------------------------------------------------

	SELECT
		t.IdCustomer
	INTO
		#NewClients
	FROM
		#TodayClients t
		LEFT JOIN #PastClients p ON t.IdCustomer = p.IdCustomer
	WHERE
		p.IdCustomer IS NULL

	------------------------------------------------------------------------------

	SELECT
		C.IdCustomer,
		MIN(Id) OfferId
	INTO
		#NewOffers
	FROM
		CashRequests C
		INNER JOIN #NewClients N ON C.IdCustomer = N.IdCustomer
	GROUP BY
		c.IdCustomer

	------------------------------------------------------------------------------

	INSERT INTO #output(Caption, Css) VALUES ('Applications', 'total')

	------------------------------------------------------------------------------

	INSERT INTO #output(Caption, Counter, Decision, Value)
	SELECT
		'Total',
		COUNT(1),
		UnderwriterDecision,
		SUM(ManagerApprovedSum)
	FROM
		CashRequests
	WHERE
		@DateStart <= CreationDate AND CreationDate < @DateEnd
	GROUP BY
		UnderwriterDecision
	ORDER BY
		UnderwriterDecision

	------------------------------------------------------------------------------

	INSERT INTO #output(Caption, Counter, Decision, Value)
	SELECT
		'New',
		COUNT(1),
		UnderwriterDecision,
		SUM(ManagerApprovedSum)
	FROM
		CashRequests
		INNER JOIN #NewOffers ON CashRequests.Id = #NewOffers.OfferId
	WHERE
		@DateStart <= CreationDate AND CreationDate < @DateEnd
	GROUP BY
		UnderwriterDecision
	ORDER BY
		UnderwriterDecision

	------------------------------------------------------------------------------

	INSERT INTO #output(Caption, Counter, Decision, Value)
	SELECT
		'Old',
		COUNT(1),
		UnderwriterDecision,
		SUM(ManagerApprovedSum)
	FROM
		CashRequests
		LEFT JOIN #NewOffers ON CashRequests.Id = #NewOffers.OfferId
	WHERE
		@DateStart <= CreationDate AND CreationDate < @DateEnd
		AND
		#NewOffers.OfferId IS NULL
	GROUP BY
		UnderwriterDecision
	ORDER BY
		UnderwriterDecision

	------------------------------------------------------------------------------

	INSERT INTO #output(Caption, Css) VALUES ('Loans', 'total')

	------------------------------------------------------------------------------

	INSERT INTO #output(Caption, Counter, Value)
	SELECT
		'Total',
		COUNT(DISTINCT Id),
		SUM(LoanAmount)
	FROM
		Loan
	WHERE
		@DateStart <= [Date] AND [Date] < @DateEnd

	------------------------------------------------------------------------------

	INSERT INTO #output(Caption, Counter, Value)
	SELECT
		'Old',
		COUNT(DISTINCT l.Id),
		SUM(l.LoanAmount)
	FROM
		Loan l
		INNER JOIN #OldLoanCustomers old ON l.CustomerId = old.CustomerID
	WHERE
		@DateStart <= l.[Date] AND l.[Date] < @DateEnd

	------------------------------------------------------------------------------

	INSERT INTO #output(Caption, Counter, Value)
	SELECT
		'New',
		COUNT(DISTINCT l.Id),
		SUM(l.LoanAmount)
	FROM
		Loan l
		LEFT JOIN #OldLoanCustomers old ON l.CustomerId = old.CustomerID
	WHERE
		old.CustomerID IS NULL
		AND
		@DateStart <= l.[Date] AND l.[Date] < @DateEnd

	------------------------------------------------------------------------------

	INSERT INTO #output(Caption, Css) VALUES ('Payments', 'total')

	------------------------------------------------------------------------------

	INSERT INTO #output(Caption, Counter, Value)
	SELECT
		'Total',
		COUNT(1) Counter,
		SUM(Amount)
	FROM
		LoanTransaction
	WHERE
		@DateStart  <= PostDate AND PostDate < @DateEnd
		AND
		Type = 'PaypointTransaction'
		AND
		Description = 'payment from customer'

	------------------------------------------------------------------------------

	INSERT INTO #output(Caption, Css) VALUES ('Registers', 'total')

	------------------------------------------------------------------------------

	INSERT INTO #output(Caption, Counter)
	SELECT
		'Total' Line,
		COUNT(1) Counter
	FROM
		Customer
	WHERE
		@DateStart <= GreetingMailSentDate AND GreetingMailSentDate < @DateEnd

	------------------------------------------------------------------------------

	SELECT
		Caption,
		Counter,
		Decision,
		Value,
		Css
	FROM
		#output
	ORDER BY
		SortOrder

	------------------------------------------------------------------------------

	DROP TABLE #TodayClients
	DROP TABLE #PastClients
	DROP TABLE #NewClients
	DROP TABLE #NewOffers
	DROP TABLE #OldLoanCustomers
	DROP TABLE #output
END

GO
/****** Object:  StoredProcedure [dbo].[RptTstRpt]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptTstRpt]
@DateStart DATETIME,
@DateEnd DATETIME
AS
	SELECT 'Test report' AS SomeData, @DateStart AS FromTime, @DateEnd AS ToTime

GO

/****** Object:  StoredProcedure [dbo].[Security_ChangePassword]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Security_ChangePassword]
(
 @pLogin nvarchar(250),
 @pOldPassword nvarchar(max),
 @pNewPassword nvarchar(max),
 @pMaxOldPassCheck bigint
 )
AS
BEGIN
	DECLARE  @pUserId bigint,
	@pDisabledChange bigint,
	@pForceChange bigint;
  -- Check login and old password-------------------------------------------------------------
     select @pUserId=userid, @pDisabledChange=disablepasschange, @pForceChange=forcepasschange
       from security_user
     where  upper(username)  = upper(@pLogin) and password = @pOldPassword and isdeleted <> 1;
	 IF (@pUserId is null)
		BEGIN
			SELECT 2; -- invalid login
			RETURN 2;
		END
   -- Check change permission ------------------------------------------------------------------
   if (@pDisabledChange is not null and @pForceChange is null) 
	BEGIN
		SELECT 3; -- not allowed
		RETURN 3;
	END
 
 -- Old passwords check ----------------------------------------------------------------------
  /*if (@pMaxOldPassCheck is not null)
      BEGIN
        IF EXISTS(
        			SELECT * FROM (select TOP(@pMaxOldPassCheck+1) * from security_accountlog
							where eventtype = 1 and userid=@pUserId 
					order by eventdate DESC) AS tbl1 WHERE tbl1.[Data]=@pNewPassword
				)
			BEGIN
				SELECT 4;
				RETURN 4; -- Old pass validation failed
			END
      END*/
 
 update security_user
                set password = @pNewPassword,
                      passsettime = GETDATE(),
                      loginfailedcount = null,
                      lastbadlogin = null,
                      forcepasschange = null
                where userid = @pUserId; 
 -- add log entry for changing password
 insert into security_accountlog (eventtype, userid, data)
                values (1, @pUserId, @pNewPassword);
SELECT 1; 
RETURN 1; -- change ok     

END

GO
/****** Object:  StoredProcedure [dbo].[Security_GetSession]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Security_GetSession]
(
	@pSessionId nvarchar(MAX)
) 
AS

BEGIN

       select security_session.userid,
              appid,
              state,
              sessionid,
              security_session.creationdate,
              security_user.username
         from security_session,
              security_user
        where security_user.userid = security_session.userid
        AND security_session.sessionid = @pSessionId;

END

GO
/****** Object:  StoredProcedure [dbo].[Security_GetUserById]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Security_GetUserById]
(
  @pId bigint
)
AS
BEGIN
    select userid, username, fullname, password, creationdate, isdeleted, email, createuserid, deletiondate, deleteuserid, branchid, passsettime, loginfailedcount, disabledate, lastbadlogin, passexpperiod, forcepasschange, disablepasschange, certificateThumbprint
     from security_user
    WHERE userId = @pId;
END;

GO
/****** Object:  StoredProcedure [dbo].[Security_GetUserByLogin]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Security_GetUserByLogin]
(
  @pLogin nvarchar(MAX)
)
AS
BEGIN
    select userid, username, fullname, password, creationdate, isdeleted, email, createuserid, deletiondate, deleteuserid, branchid, passsettime, loginfailedcount, disabledate, lastbadlogin, passexpperiod, forcepasschange, disablepasschange, certificateThumbprint
     from security_user
    WHERE UPPER(username) = UPPER(@pLogin) AND isdeleted != 1;
END;

GO
/****** Object:  StoredProcedure [dbo].[Security_GrandServiceAction]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Security_GrandServiceAction]
(
		@pCommandName  nvarchar(255),
		@pAppId int,
		@pRoleId int
)
AS
BEGIN
     insert into Security.ServiceAction
     (Command, Name, AppId)
     values
     (@pCommandName, @pCommandName, @pAppId)
     
     
     insert into Security.RoleServiceActionRelation
     (RoleId, ServiceActionId)
     values
     (@pRoleId, @@IDENTITY)
  /* Procedure body */
END

GO
/****** Object:  StoredProcedure [dbo].[SetLateBy14Days]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SetLateBy14Days]
	@LoanId INT
AS
BEGIN
	UPDATE Loan SET Is14DaysLate = 1 WHERE Id = @LoanId
END
GO

/****** Object:  StoredProcedure [dbo].[Strategy_AssignEmbedded]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_AssignEmbedded]
  (
    @pStrategyId int,
    @pEmbStrategyName nvarchar(max)
  )
AS

BEGIN
 DECLARE @embStrategyId as int;

 select @embStrategyId = strategyid  from strategy_strategy
  where [name] = @pEmbStrategyName and isdeleted = 0;
  
    delete from strategy_embededrel
    where strategyid = @pStrategyId and embstrategyid=@embStrategyId;
	
	if (@pStrategyId <> @embStrategyId)
	begin
		insert into strategy_embededrel
		  (strategyid, embstrategyid)
		values
		  (@pStrategyId, @embStrategyId);
	end


END

GO
/****** Object:  StoredProcedure [dbo].[Strategy_DeletePublishName]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_DeletePublishName]
  @pId bigint
 ,@pSignedDocument NVARCHAR(max)
 ,@pUserId int
AS
BEGIN
   if @pId > 0 
   begin
      update strategy_strategy
        set state = 0
      where strategyid in (select strategyid from strategy_publicrel where publicid = @pId);
     
     update strategy_publicname 
        set
            IsDeleted = @pId,
            TerminationDate  = GETDATE(),
            SignedDocumentDelete = @pSignedDocument,
            DeleterUserId = @pUserId
     where publicnameid = @pId;
   end; 

END

GO
/****** Object:  StoredProcedure [dbo].[Strategy_DeletePubRel]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_DeletePubRel]

    @pPublicId bigint,
    @pStrategyId bigint,
    @pAction nvarchar(7),
    @pUserId INT,
    @pSignedDocument ntext,
    @pData ntext,
    @pAllData ntext

AS
   
BEGIN
     delete strategy_publicrel where publicid = @pPublicId and strategyid = @pStrategyId;
     
     update strategy_strategy
        set state = 0
      where strategyid = @pStrategyId;
     
     if Not (@pSignedDocument is null) begin
        execute Strategy_PublicSignInsert @pPublicId, @pAction, @pUserId, @pSignedDocument, @pData, @pStrategyId, @pAllData
     end;

END;

GO
/****** Object:  StoredProcedure [dbo].[Strategy_GetAssignedStrategies]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_GetAssignedStrategies]
(
  @pPublishNameId bigint
)
AS
BEGIN

   SELECT 
     t.strategyId
    ,t.name
    ,t.authorName
    ,t.state
    ,p.[percent]
    ,t.description
    ,t.DisplayName
    ,t.TermDate
    ,(select TOP 1 ss.SignedDocument 
      from Strategy_PublicSign ss 
      where ss.StrategyPublicId = p.PUBLICID
        and ss.StrategyId = p.strategyId
      ORDER BY Id DESC) as SignedDocument
   FROM STRATEGY_publicRel p 
     INNER JOIN strategy_vstrategy t ON p.strategyID = t.strategyId
   WHERE p.publicid = @pPublishNameId;
END;

GO
/****** Object:  StoredProcedure [dbo].[Strategy_GetDelStrategy]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_GetDelStrategy]
(
  @pStrategyName nvarchar(MAX)
) 
AS
BEGIN
    select strategyid from strategy_strategy 
    where strategy_strategy.name = @pStrategyName;   
END;

GO
/****** Object:  StoredProcedure [dbo].[Strategy_GetPublishNames]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_GetPublishNames]
	
AS
BEGIN
	SELECT PublicnameID, Name FROM Strategy_Publicname
       WHERE (IsStopped is null or IsStopped = 0)
         and (IsDeleted is null or IsDeleted = 0);

END

GO
/****** Object:  StoredProcedure [dbo].[Strategy_GetPublishNamesCount]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_GetPublishNamesCount] 
	
AS
BEGIN
	SELECT p.name, 
            (SELECT COUNT(strategyId) FROM Strategy_Publicrel 
            WHERE publicId = p.publicnameid) STRATEGYCOUNT, p.publicnameid, p.isstopped
                                FROM Strategy_PublicName p
            WHERE  (IsDeleted is null or IsDeleted = 0);

END

GO
/****** Object:  StoredProcedure [dbo].[Strategy_GetShortAsgStrategies]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_GetShortAsgStrategies]
(
  @pPublishNameId bigint
)
AS
BEGIN
       SELECT t.strategyId, t.name, t.state, p.[percent],
       t.DisplayName, t.TermDate
       ,(select TOP 1 ss.SignedDocument 
         from Strategy_PublicSign ss 
         where ss.StrategyPublicId = p.PUBLICID 
           and ss.StrategyId = p.strategyId
         ORDER BY Id DESC) as SignedDocument
       FROM STRATEGY_publicRel p, strategy_vstrategy t 
       WHERE p.strategyID = t.strategyId AND p.publicid = @pPublishNameId 
       ORDER BY p.strategyID;
END;

GO

/****** Object:  StoredProcedure [dbo].[Strategy_NodeDelete]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE  PROCEDURE [dbo].[Strategy_NodeDelete]
       @pNodeName NVARCHAR(max)
      ,@pSignedDocument NVARCHAR(max)
	  ,@pUserId int
AS
BEGIN
  update Strategy_Node set
    IsDeleted = NodeId,
    TerminationDate  = GETDATE(),
    SignedDocumentDelete = @pSignedDocument,
    DeleterUserId = @pUserId
  where upper([name]) = upper(@pNodeName);

END

GO
/****** Object:  StoredProcedure [dbo].[Strategy_NodeInsert]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE  PROCEDURE [dbo].[Strategy_NodeInsert]
       @pName nvarchar(max),
       @pDisplayName nvarchar(max),
       @pDescription nvarchar(max),
       @pExecutionDuration bigint,
       @pIcon image,
       @pGroupId tinyint,
       @pContainsPrint bigint,
       @pNodeId int OUTPUT,
       @pCustomUrl NVARCHAR(MAX),
       @pGUID NVARCHAR(MAX),
       @pComment NVARCHAR(MAX),
       @pNDX image,
       @pSignedDocument ntext,
	   @pUserId int
AS
BEGIN
	 -- Terminate current nodes
     update Strategy_Node set TerminationDate  = GETDATE()
     where upper([Name]) = upper(@pName) and TerminationDate is null;

     insert into Strategy_Node
     (
       Name,
       DisplayName,
       Description,
       ExecutionDuration,
       Icon,
       GroupId,
       ContainsPrint,
       CustomUrl,
       [guid],
       nodecomment,
       NDX,
       SignedDocument,
       CreatorUserId
     )
     values
     (
       @pName,
       @pDisplayName,
       @pDescription,
       @pExecutionDuration,
       @pIcon,
       @pGroupId,
       @pContainsPrint,
       @pCustomUrl,
       @pGUID,
       @pComment,
       @pNDX,
       @pSignedDocument,
	   @pUserId
       )
  
       select @pNodeId = SCOPE_IDENTITY()
END

GO
/****** Object:  StoredProcedure [dbo].[Strategy_NodeStrategyRelCheck]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create procedure [dbo].[Strategy_NodeStrategyRelCheck]
	@pNodeId int
as
begin
	SELECT DISTINCT [strat].[DisplayName],
	strat.TermDate
    FROM  [Strategy_Strategy] strat 
	  	  INNER JOIN [Strategy_NodeStrategyRel] strRel
		  ON [strat].[StrategyId] = [strRel].[StrategyId]
	WHERE [strRel].[NodeId] = @pNodeId 
		  AND [strat].[IsDeleted] = 0	
end

GO
/****** Object:  StoredProcedure [dbo].[Strategy_NodeStrategyRelDrop]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
Written by Kirill Sorudeykin, 	May 08, 2008
*/

create procedure [dbo].[Strategy_NodeStrategyRelDrop]
	@pStrategyId int
as
begin
DELETE FROM Strategy_NodeStrategyRel
			WHERE StrategyId = @pStrategyId
end

GO
/****** Object:  StoredProcedure [dbo].[Strategy_PublicSignInsert]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE  PROCEDURE [dbo].[Strategy_PublicSignInsert]
       @pStrategyPublicNameId bigint,
       @pAction nvarchar(7),
       @pUserId int,
       @pSignedDocument ntext,
       @pData ntext,
       @pStrategyId int,
	   @pAllData ntext
AS
BEGIN
  if @psignedDocument is not null
      INSERT INTO Strategy_PublicSign
        (StrategyPublicId
        ,Action
        ,Data
        ,SignedDocument
        ,UserId
        ,StrategyId
		,AllData)
      VALUES
        (@pStrategyPublicNameId
        ,@pAction
        ,@pData
        ,@pSignedDocument
        ,@pUserId
        ,@pStrategyId
		,@pAllData);
END

GO
/****** Object:  StoredProcedure [dbo].[Strategy_SchemaInsert]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_SchemaInsert] 
@pStrategyId AS INT,
@pBinaryData AS VARBINARY(MAX)

AS
BEGIN
	INSERT INTO [Strategy_Schemas]
           ([StrategyId],[BinaryData])
     VALUES
           (@pStrategyId, @pBinaryData)
END

GO
/****** Object:  StoredProcedure [dbo].[Strategy_SchemaSelect]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE  PROCEDURE [dbo].[Strategy_SchemaSelect] 
@pStrategyId AS INT

AS
BEGIN
	SELECT * FROM [Strategy_Schemas]
	WHERE StrategyId = @pStrategyId;
END

GO
/****** Object:  StoredProcedure [dbo].[Strategy_StrategyCheckIn]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_StrategyCheckIn] 
(
	@pStrategyName nvarchar(max),
	@pStrategyUserID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	IF NOT EXISTS( SELECT [displayname] FROM [Strategy_Strategy] WHERE [displayname] = @pStrategyName AND [IsDeleted] = 0)
	BEGIN
		RAISERROR( 'StrategyNoExist'
				 , 16
				 , 1
				 );
		RETURN;
	END;
	IF EXISTS( SELECT *
				   FROM [Strategy_Strategy]
				   WHERE [UserID] <> @pStrategyUserID
					 AND SubState = 0 /* Locked */
					 AND [displayname] = @pStrategyName
					 AND [IsDeleted] = 0 )
	BEGIN
		RAISERROR( 'StrategyIsLocked'
				 , 16
	  			 , 1
				 );
		RETURN;
	END;
	UPDATE [Strategy_Strategy]
       SET [UserID] = @pStrategyUserID,
           SubState = 1 --Unlocked
	 WHERE [displayname] = @pStrategyName
       AND State = 0 -- Challenge
  	   AND [IsDeleted] = 0;
END

GO
/****** Object:  StoredProcedure [dbo].[Strategy_StrategyInsert]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE  PROCEDURE [dbo].[Strategy_StrategyInsert] 
    @pStrategyName nvarchar(max)
    , @pStrategyDescription nvarchar(max)
    , @pStrategyIcon image
    , @pStrategyIsEmbeddingAllowed bit
    , @pStrategyXml ntext
    , @pStrategyUserID int
    , @pStrategyType nvarchar(255)
    , @pStrategyID int OUTPUT
    , @pSignedDocument ntext
    , @pDisplayName NVARCHAR(max)
    , @pRepublish BIGINT
AS
BEGIN
     SET ANSI_WARNINGS OFF
	DECLARE @pOldState AS BIGINT;
	select @pOldState=[state] from strategy_strategy
     where displayname = @pDisplayName and termdate is null;
     
   update Strategy_Strategy set termdate = GETDATE(),
   substate = 1
   where displayname = @pDisplayName and termdate is null;
   
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    INSERT INTO [Strategy_Strategy]( [Name]
                 , [Description]
                 , [Icon]
                 , [IsEmbeddingAllowed] 
                 , [XML] 
                 , [UserID] 
                 , AuthorId
                 , DisplayName
                 , SignedDocument
                 , [StrategyType]
                 ) 
    VALUES( @pStrategyName 
          , @pStrategyDescription 
          , @pStrategyIcon 
          , @pStrategyIsEmbeddingAllowed 
          , @pStrategyXML 
          , @pStrategyUserID 
          , @pStrategyUserID
          , @pDisplayName
          , @pSignedDocument
          , @pStrategyType
          ) 
		
    SELECT @pStrategyID = SCOPE_IDENTITY();

    IF @pRepublish IS NOT NULL
    BEGIN
    	update strategy_publicrel set
		strategyid = @pStrategyID
		where strategyid in ( 
		  select strategyid from strategy_strategy where
		  displayname = @pDisplayName and termdate is not null);
		
		UPDATE Strategy_Strategy
		SET [State] = 0 
		WHERE displayname = @pDisplayName and termdate is not null;
			
		UPDATE Strategy_Strategy
		SET	[State] = @pOldState WHERE StrategyId=@pStrategyID;

    END
END

GO
/****** Object:  StoredProcedure [dbo].[Strategy_StrategyList]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_StrategyList] 
(    
	@pUserId int,
    @pIsPublished bit = NULL,
    @pIsEmbeddingAllowed bit = NULL
)
AS
BEGIN
   SET NOCOUNT ON;
  
       SELECT s.[StrategyId]                      AS [StrategyId]
            , s.[Name]                            AS [Name]
            , s.[Description]                     AS [Description]
            , u.[FullName]                        AS [Author]
            , s.[CreationDate]                    AS [PublishingDate],
            s.TermDate AS [TermDate],
            s.DisplayName AS [DisplayName]
            , case s.[SubState]
                 when 0 /* Locked */ then case s.[UserId]
                                             when @pUserId then 0
                                             else 1
                                           end
                 when 1 then 0
                 else 1
              end                                 AS [IsReadOnly]
            , s.[IsEmbeddingAllowed]              AS [IsEmbeddingAllowed]
            , case State
                 when 1 then 1
                 else 0
              end                                 AS [IsPublished]
            , s.[Icon]				  AS [Icon]
            , s.[UserId]			  AS [UserId]
            , s.[IsMigrationSupported]		  AS [IsMigrationSupported]
         FROM [Strategy_Strategy] AS s JOIN [Security_User] AS u ON s.[AuthorID] = u.[UserID]
        WHERE ( s.[IsEmbeddingAllowed] = @pIsEmbeddingAllowed OR @pIsEmbeddingAllowed IS NULL )
          AND ( s.[IsDeleted] = 0)
       ORDER BY s.displayname, s.termdate desc;

END

GO
/****** Object:  StoredProcedure [dbo].[Strategy_StrategyParamInsert]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[Strategy_StrategyParamInsert]
                    @pParameterTypeName nvarchar(max)
                  , @pStrategyID int
                  , @pVariableName nvarchar(max)
                  , @pVariableDescription nvarchar(max)
                  , @pVariableIsInput bit
                  , @pVariableIsOutput bit
AS
BEGIN
     DECLARE @pParameterTypeID int;
   
     SELECT @pParameterTypeID = NULL;
     
     SELECT TOP 1 @pParameterTypeID = Strategy_ParameterType.ParamTypeID
     FROM Strategy_ParameterType
     WHERE @pParameterTypeName = [Strategy_ParameterType].[Name];
     IF @pParameterTypeID IS NULL
     BEGIN
          INSERT INTO [Strategy_ParameterType]( [Name] )
                 VALUES ( @pParameterTypeName )
                 
          SELECT @pParameterTypeID = SCOPE_IDENTITY()
     END
     INSERT INTO [Strategy_StrategyParameter]( [TypeID]
                 , [OwnerID]
                 , [Name]
                 , [Description]
                 , [IsInput]
                 , [IsOutput]
                 )
            VALUES( @pParameterTypeID
                  , @pStrategyID
                  , @pVariableName
                  , @pVariableDescription
                  , @pVariableIsInput
                  , @pVariableIsOutput
                  ) 
END

GO
/****** Object:  StoredProcedure [dbo].[Strategy_StrategyPublish]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_StrategyPublish]
(
    @pStrategyId bigint,
    @pPublishNameId bigint,
    @pPercent decimal,
    @pAction nvarchar(7),
    @pUserId INT,
    @pSignedDocument ntext,
    @pData ntext,
	@pAllData ntext
)
AS
BEGIN
  IF NOT EXISTS(SELECT * from strategy_publicrel  WHERE strategyid = @pStrategyId AND
  publicid = @pPublishNameId)
  BEGIN
  	 insert into strategy_publicrel
    (publicid, strategyid, [percent])
	values
    (@pPublishNameId, @pStrategyId, @pPercent);
    
    EXEC strategy_updatechampionstatus @pPublishNameId;
    if NOT (@pData IS NULL)
      execute Strategy_PublicSignInsert @pPublishNameId, @pAction, @pUserId, @pSignedDocument, @pData, @pStrategyId, @pAllData

  END
END

GO
/****** Object:  StoredProcedure [dbo].[Strategy_StrategyRePublish]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_StrategyRePublish]
  (
    @pDisplayName NVARCHAR(max),
    @pNewStrategyId BIGINT
  )
AS

BEGIN
  DECLARE @pOldStratId BIGINT, @pOldState BIGINT;

  select @pOldStratId = strategyid, @pOldState = [state] from strategy_strategy
  where displayname = @pDisplayName and isdeleted = 0 
  and termdate = (select max(termdate) from strategy_strategy
  where displayname = @pDisplayName and isdeleted = 0);
  
  update  strategy_publicrel
  set strategyid = @pNewStrategyId
  where strategyid = @pOldStratId;
  
  update strategy_strategy 
  set state = @pOldState
  where strategyid = @pNewStrategyId;

END

GO
/****** Object:  StoredProcedure [dbo].[Strategy_StrategySelect]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		IBOR
-- Create date: <Create Date,,>
-- Description:	Given strategy name return strategy XML
-- =============================================
CREATE PROCEDURE [dbo].[Strategy_StrategySelect]
    @pStrategyName nvarchar(400),
	@pStrategyUserId int,
	@pStrategyCheckOut bit,
	@pIsChampion bit
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	IF NOT EXISTS( SELECT [Name] FROM [Strategy_Strategy] WHERE [Name] = @pStrategyName AND [IsDeleted] = 0)
	BEGIN
		RAISERROR( 'StrategyNoExist'
				 , 16
				 , 1
				 );
		RETURN;
	END
	
	IF EXISTS( SELECT *
		   FROM [Strategy_Strategy]
		   WHERE [UserID] <> @pStrategyUserID
			 AND [SubState] = 0 /* Locked */
			 AND [Name] = @pStrategyName
			 AND [IsDeleted] = 0
             AND 1 = @pStrategyCheckOut)
    BEGIN
		RAISERROR( 'StrategyIsLocked'
				 , 16
				 , 1
				 );
		RETURN
	END
    SELECT [Xml], SignedDocument
      FROM [Strategy_Strategy]
     WHERE [Name] = @pStrategyName
       AND [IsDeleted] = 0

END

GO
/****** Object:  StoredProcedure [dbo].[Strategy_UpdateChampionStatus]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_UpdateChampionStatus]

    @pPublicId bigint

AS
 
BEGIN
	declare @stratId as int;

    update strategy_strategy
        set state = 0
      where strategyid in (select strategyid from strategy_publicrel
   where publicid = @pPublicId);

  select TOP(1) @stratId = strategyId  from strategy_publicrel
   where publicid = @pPublicId
   and [percent] = (select MAX([percent]) from strategy_publicrel
   where publicid = @pPublicId);
      
   if @stratId > 0 begin
     update strategy_strategy
        set state = 1
      where strategyid = @stratId;
   end;

END;

GO
/****** Object:  StoredProcedure [dbo].[Strategy_UpdatePublicRel]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_UpdatePublicRel]
    @pPublicId bigint,
    @pStrategyId bigint,
    @pPercent decimal,
    @pAction nvarchar(7),
    @pUserId INT,
    @pSignedDocument ntext,
    @pData ntext,
	@pAllData ntext

AS
BEGIN
  update strategy_publicrel
  set [percent] = @pPercent
  where publicid = @pPublicId and strategyid = @pStrategyId;
  EXEC dbo.strategy_updatechampionstatus @pPublicId; 
  if NOT (@pData IS NULL)
    execute Strategy_PublicSignInsert @pPublicId, @pAction, @pUserId, @pSignedDocument, @pData, @pStrategyId, @pAllData

END

GO
/****** Object:  StoredProcedure [dbo].[Strategy_UpdatePublishName]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_UpdatePublishName] 
	@pId as bigint OUTPUT,
	@pName as nvarchar(max),
	@pIsStopped as int
AS
BEGIN
	DECLARE @l_id int;

	if @pId is not null 
		begin
		  select @l_id = COUNT(publicnameid)  from strategy_publicname
		  where (upper(name) = upper(@pName))
                    and publicnameid <> @pId
                    and (IsDeleted is null or IsDeleted = 0);
		  if (@l_id > 0) begin
			RAISERROR('dublicated_name', 16,1);
			RETURN;
			end;
      
		   update strategy_publicname
			set name = @pName,
				isstopped = @pIsStopped
		  where publicnameid = @pId;
		end

else
	begin
      select @l_id = COUNT(publicnameid) from strategy_publicname
      where (upper(name) = upper(@pName))
        and (IsDeleted is null or IsDeleted = 0);
      if (@l_id > 0) begin
			RAISERROR('dublicated_name', 16,1);
			RETURN;
			end;
      insert into strategy_publicname
        (name)
      values
        (@pName);
	 set @pId = @@IDENTITY;
	end;

END

GO

/****** Object:  StoredProcedure [dbo].[Update_Main_Strat_Finish_Date]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Update_Main_Strat_Finish_Date] 
(@UserId int)

AS
BEGIN
declare @MainStratFinishDate datetime  

set @MainStratFinishDate = GETUTCDATE()

UPDATE [dbo].[Customer]
   SET [LastStartedMainStrategyEndTime] = @MainStratFinishDate
 WHERE Id = @UserId

SET NOCOUNT ON;
SELECT @@IDENTITY;
END

GO

/****** Object:  StoredProcedure [dbo].[UpdateAutoApproval]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[UpdateAutoApproval]
	@CustomerId INT,
    @AutoApproveAmount INT

AS
BEGIN
    DECLARE 
		@Score INT,
		@InterestRate NUMERIC(18,4)

	SELECT @Score = ExperianScore
	FROM
		(
			SELECT
				CustomerId,
				ExperianScore,
				ROW_NUMBER() OVER(PARTITION BY CustomerId ORDER BY Id DESC) AS rn
			FROM
				MP_ExperianDataCache
		) as T
	WHERE
		rn = 1 AND
		CustomerId = @CustomerId

	SELECT @InterestRate = LoanIntrestBase FROM BasicInterestRate WHERE FromScore <= @Score AND ToScore >= @Score
	IF @InterestRate IS NULL SET @InterestRate=0

	UPDATE CashRequests SET SystemCalculatedSum=@AutoApproveAmount, InterestRate=@InterestRate WHERE IdCustomer=@CustomerId
	UPDATE Customer SET SystemCalculatedSum = @AutoApproveAmount, CreditSum=@AutoApproveAmount, IsLoanTypeSelectionAllowed=1, LastStatus='Approve' WHERE Id=@CustomerId
END

GO
/****** Object:  StoredProcedure [dbo].[UpdateCashRequests]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateCashRequests] 
(@CustomerId int,
 @SystemCalculatedAmount int,
 @ManagerApprovedSum int,
 @SystemDecision varchar(50),
 @MedalType varchar(50),
 @ScorePoints numeric(8,3),
 @ExpirianRating int,
 @AnualTurnover int,
 @InterestRate decimal(18,4))

AS
BEGIN
declare @SystemDecisionDate datetime
set @SystemDecisionDate = GETUTCDATE()

UPDATE [dbo].[CashRequests]
   SET  [IdCustomer] = @CustomerId, 
		[SystemCalculatedSum] = @SystemCalculatedAmount,
		[ManagerApprovedSum] = @ManagerApprovedSum,
		[SystemDecision] = @SystemDecision,
		[SystemDecisionDate]= @SystemDecisionDate, 
		[MedalType]= @MedalType,
		[ScorePoints]= @ScorePoints,
		[ExpirianRating] = @ExpirianRating,
		[AnualTurnover] = @AnualTurnover,
		[InterestRate] = @InterestRate
 WHERE Id = (select MAX(id) from CashRequests
				where IdCustomer=@CustomerId)



 SET NOCOUNT ON;
SELECT @@IDENTITY;
END

GO
/****** Object:  StoredProcedure [dbo].[UpdateCashRequestsReApproval]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateCashRequestsReApproval] 
(@CustomerId int,
 @UnderwriterDecision nvarchar(50),
 @ManagerApprovedSum decimal(18, 0),
 @APR decimal(18, 0),
 @RepaymentPeriod int, 
 @InterestRate decimal(18, 4),
 @UseSetupFee int, 
 @OfferValidDays int,
 @EmailSendingBanned int,
 @LoanTypeId int,
 @UnderwriterComment nvarchar(200),
 @IsLoanTypeSelectionAllowed int,
 @DiscountPlanId int,
 @ExperianRating INT)
 
 
AS
BEGIN

declare @OfferStart Datetime, @OfferValidUntil datetime
set @OfferStart = GETUTCDATE()
DECLARE @ValidForHours INT
SELECT @ValidForHours = CONVERT(INT, Value) FROM ConfigurationVariables WHERE Name='OfferValidForHours'
set   @OfferValidUntil = DATEADD(hh, @ValidForHours ,GETUTCDATE())

UPDATE [dbo].[CashRequests]
   SET  
 UnderwriterDecision = @UnderwriterDecision,
 UnderwriterDecisionDate = GETUTCDATE(),
 ManagerApprovedSum = @ManagerApprovedSum,
 APR = @APR,
 RepaymentPeriod = @RepaymentPeriod, 
 InterestRate = @InterestRate,
 UseSetupFee = @UseSetupFee, 
 EmailSendingBanned = @EmailSendingBanned,
 LoanTypeId  = @LoanTypeId,
 UnderwriterComment = @UnderwriterComment,
 IsLoanTypeSelectionAllowed= @IsLoanTypeSelectionAllowed,
 DiscountPlanId = @DiscountPlanId,
 ExpirianRating = @ExperianRating
 

 WHERE Id = (select MAX(id) from CashRequests
				where IdCustomer=@CustomerId)
				
UPDATE Customer SET CreditSum = @ManagerApprovedSum WHERE Id = @CustomerId

 SET NOCOUNT ON;
SELECT @@IDENTITY;
END
GO
/****** Object:  StoredProcedure [dbo].[UpdateCollection]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateCollection] 
(@LoanId int,
 @Late30 Numeric(18),
 @Late30Num int,
 @Late60 Numeric(18),
 @Late60Num int,
 @Late90 Numeric(18),
 @Late90Num int,
 @PastDues Numeric(18),
 @PastDuesNum int,
 @IsDefaulted int,
 @Late90Plus numeric(18),
 @Late90PlusNum numeric(18)
)

 

AS
BEGIN

UPDATE [dbo].[Loan]
   SET  Late30 = @Late30,
	    Late30Num = @Late30Num,
		Late60 = @Late60,
		Late60Num = Late60Num,
		Late90 = @Late90,
		Late90Num = @Late90Num,
		PastDues = @PastDues,
		PastDuesNum = @PastDuesNum,
		IsDefaulted = @IsDefaulted,
		Late90Plus = @Late90Plus,
		Late90PlusNum = @Late90PlusNum
 WHERE Id = @LoanId


 SET NOCOUNT ON;
SELECT @@IDENTITY;
END

GO

/****** Object:  StoredProcedure [dbo].[UpdateExperianBusiness]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateExperianBusiness] 
(@CompanyRefNumber nvarchar(50),
 @ExperianError nvarchar(max),
 @ExperianScore int, 
 --@ExperianResult nvarchar(500),
 --@ExperianWarning nvarchar(max),
 --@ExperianReject nvarchar(max),
 @CustomerId bigint)

AS
BEGIN

UPDATE [dbo].[MP_ExperianDataCache]
   SET [ExperianError] = @ExperianError, 
    [ExperianScore] = @ExperianScore, 
--    [ExperianResult] = @ExperianResult,
--    [ExperianWarning] = @ExperianWarning,
--    [ExperianReject] = @ExperianReject,
    [CustomerId] = @CustomerId
 WHERE CompanyRefNumber = @CompanyRefNumber


 SET NOCOUNT ON;
SELECT @@IDENTITY;
END

GO
/****** Object:  StoredProcedure [dbo].[UpdateExperianBWA_AML]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateExperianBWA_AML] 
(@CustomerId int,
 @BWAResult nvarchar(100),
 @AMLResult nvarchar(100))
 
 
AS
BEGIN

UPDATE [dbo].[Customer]
   SET [BWAResult] = @BWAResult, 
    [AMLResult] = @AMLResult

WHERE Id = @CustomerId

UPDATE [dbo].[CardInfo]
   SET [BWAResult] = @BWAResult

WHERE CustomerId = @CustomerId

 


 SET NOCOUNT ON;
SELECT @@IDENTITY;
END

GO
/****** Object:  StoredProcedure [dbo].[UpdateExperianConsumer]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateExperianConsumer] 
(@Name nvarchar(500),
 @Surname nvarchar(500),
 @PostCode nvarchar(500),
 @ExperianError nvarchar(max),
 @ExperianScore int, 
 --@ExperianResult nvarchar(500),
 --@ExperianWarning nvarchar(max),
 --@ExperianReject nvarchar(max),
 @CustomerId bigint,
 @DirectorId bigint)

AS
BEGIN

UPDATE [dbo].[MP_ExperianDataCache]
   SET [ExperianError]  = @ExperianError, 
       [ExperianScore]  = @ExperianScore, 
--     [ExperianResult] = @ExperianResult,
--     [ExperianWarning]= @ExperianWarning,
--     [ExperianReject] = @ExperianReject,
       [CustomerId]     = @CustomerId,
       [DirectorId]     = @DirectorId 
 WHERE Name = @Name and Surname = @Surname and PostCode=@PostCode


 SET NOCOUNT ON;
SELECT @@IDENTITY;
END

GO
/****** Object:  StoredProcedure [dbo].[UpdateExperianFirstIdToHandle]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateExperianFirstIdToHandle]
	@FirstIdToHandle VARCHAR(100)
AS
BEGIN
	UPDATE ExperianDataAgentConfigs SET CfgValue = @FirstIdToHandle WHERE CfgKey = 'FirstIdToHandle'
END

GO
/****** Object:  StoredProcedure [dbo].[UpdateExportSigningId]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateExportSigningId]
   @pSignedDocumentId bigint,
   @pId int
AS
BEGIN

UPDATE Export_Results
SET SignedDocumentId = @pSignedDocumentId
WHERE Id = @pId;

END

GO
/****** Object:  StoredProcedure [dbo].[UpdateFiveDaysDueMailSent]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateFiveDaysDueMailSent] 
(@Id int,
 @UpdateFiveDaysDueMailSent bit)

AS
BEGIN

UPDATE [dbo].[LoanSchedule]
   SET  [FiveDaysDueMailSent] = @UpdateFiveDaysDueMailSent
		

 WHERE Id = @Id



 SET NOCOUNT ON;
SELECT @@IDENTITY;
END

GO
/****** Object:  StoredProcedure [dbo].[UpdateLastFoundCustomerId]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateLastFoundCustomerId]
	@LastFoundCustomerId VARCHAR(100)
AS
BEGIN
	UPDATE SupportAgentConfigs SET CfgValue=@LastFoundCustomerId WHERE CfgKey = 'MaxCustomerId'
END

GO
/****** Object:  StoredProcedure [dbo].[UpdateLastReportedCAISstatus]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateLastReportedCAISstatus] 
(@LoanId int
 ,@CAISStatus varchar(100)
 )


AS
BEGIN


		


UPDATE [dbo].[Loan]
   SET  LastReportedCAISStatus = @CAISStatus, 
		LastReportedCAISStatusDate	= GETUTCDATE() 
 WHERE Id = @LoanId



 SET NOCOUNT ON;
SELECT @@IDENTITY;
END

GO
/****** Object:  StoredProcedure [dbo].[UpdateLoanScheduleCustomDate]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateLoanScheduleCustomDate] 
(@Id int)

AS
BEGIN

declare @CurDate date
set @CurDate = cast (GETUTCDATE() as DATE)

UPDATE [dbo].[LoanSchedule]
   SET  CustomInstallmentDate = @CurDate 
 WHERE Id = @Id


 SET NOCOUNT ON;
SELECT @@IDENTITY;
END

GO
/****** Object:  StoredProcedure [dbo].[UpdateLoanScheduleStatus]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateLoanScheduleStatus] 
(@Id int,
 @Status varchar(50))

AS
BEGIN

UPDATE [dbo].[LoanSchedule]
   SET  [Status] = @Status
 WHERE Id = @Id


 SET NOCOUNT ON;
SELECT @@IDENTITY;
END

GO
/****** Object:  StoredProcedure [dbo].[UpdateLoanStatusToLate]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateLoanStatusToLate] 
(@LoanId int,
@CustomerId int,
@PaymentStatus varchar(50),
@LoanStatus varchar(50))

AS
BEGIN

UPDATE [dbo].[Loan]
  SET  [Status] = @LoanStatus,
		[PaymentStatus] = @PaymentStatus

WHERE Id = @LoanId

UPDATE [dbo].Customer
  SET  CreditResult = @LoanStatus,
		[IsWasLate] = 1	
		
WHERE Id = @CustomerId



SET NOCOUNT ON;
SELECT @@IDENTITY;
END

GO
/****** Object:  StoredProcedure [dbo].[UpdateMPErrorCustomer]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateMPErrorCustomer] 
(@UserId int,
 @UpdateError varchar(max),
 @TokenExpired int)

AS
BEGIN

UPDATE [dbo].[MP_CustomerMarketPlace]
   SET  [UpdateError] = @UpdateError,
		TokenExpired = @TokenExpired

 WHERE CustomerId = @UserId



 SET NOCOUNT ON;
SELECT @@IDENTITY;
END

GO
/****** Object:  StoredProcedure [dbo].[UpdateMPErrorMP]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateMPErrorMP] 
(@umi int,
 @UpdateError varchar(max),
 @TokenExpired int)

AS
BEGIN

UPDATE [dbo].[MP_CustomerMarketPlace]
   SET  [UpdateError] = @UpdateError,
		TokenExpired = @TokenExpired

 WHERE Id = @umi



 SET NOCOUNT ON;
SELECT @@IDENTITY;
END

GO
/****** Object:  StoredProcedure [dbo].[UpdateOnTimeCollection]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateOnTimeCollection]
(@LoanId int,
 @OnTime Numeric(18),
 @OnTimeNum int)

AS
BEGIN

UPDATE [dbo].[Loan]
   SET  OnTime = @OnTime,
	    OnTimeNum = @OnTime

   
 WHERE Id = @LoanId



 SET NOCOUNT ON;
SELECT @@IDENTITY;
END

GO
/****** Object:  StoredProcedure [dbo].[UpdateRole]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[UpdateRole]
    @pRoleId int, 
    @pRoleName nVarchar(255),
    @pRoleDescr nvarchar(500),
    @pUsersIds nvarchar(4000)
as
begin
  BEGIN TRANSACTION
  DECLARE @l_new_role_id int;
  SET @l_new_role_id = @pRoleId;
  
  if @l_new_Role_id > 0 
  BEGIN 
   UPDATE Security_Role 
      SET Name = @pRoleName, 
          Description = @pRoleDescr WHERE RoleId = @pRoleId;
          
   DELETE FROM Security_UserRoleRelation WHERE RoleId = @pRoleId;          
  END; 
  ELSE
    EXEC dbo.insert_security_role @pRoleName, @pRoleDescr, @l_new_role_id OUTPUT;  
  
  INSERT INTO Security_UserRoleRelation
    Select x.item, @l_new_role_id from dbo.GetTableFromList(@pUsersIds, ',') x;

  IF @@ERROR <> 0
  BEGIN
     ROLLBACK TRANSACTION
     RETURN (-1)
  END

 COMMIT TRANSACTION
 RETURN (0)
end;

GO
/****** Object:  StoredProcedure [dbo].[UpdateScoringResult]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateScoringResult] 
(@UserId int,
 @CreditResult varchar(100),
 @SystemDecision varchar(50),
 @Status varchar(250),
 @Medal nvarchar(50),
 @IsEliminated int,
 @ApplyForLoan datetime,
 @ValidFor datetime)


AS
BEGIN

--declare @tmp datetime

 if @ApplyForLoan = GETUTCDATE() or @ApplyForLoan is null 
		SET @ApplyForLoan = GETUTCDATE()

DECLARE @ValidForHours INT
SELECT @ValidForHours = CONVERT(INT, Value) FROM ConfigurationVariables WHERE Name='OfferValidForHours'

if @ValidFor <  DATEADD(hh,@ValidForHours ,@ApplyForLoan) or @ValidFor is NULL
set @ValidFor = DATEADD(hh,@ValidForHours ,GETUTCDATE())


UPDATE [dbo].[Customer]
   SET  [CreditResult] = @CreditResult, 
		[Status] = @Status,
		[SystemDecision] = @SystemDecision,
		[ApplyForLoan]= @ApplyForLoan,
		[ValidFor] = @ValidFor, 
		[MedalType]= @Medal,
		[Eliminated]=@IsEliminated
 WHERE Id = @UserId



 SET NOCOUNT ON;
SELECT @@IDENTITY;
END

GO

/****** Object:  StoredProcedure [dbo].[UpdateTransactionStatus]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateTransactionStatus] 
(@TrackingId nvarchar(100), @TransactionStatus nvarchar(50), @Description nvarchar(max))

AS
BEGIN

UPDATE [dbo].[LoanTransaction]
   SET [Status] = @TransactionStatus, [Description] = @Description
 WHERE TrackingNumber = @TrackingId


END

GO
/****** Object:  StoredProcedure [dbo].[UpdateTwoDaysDueMailSent]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateTwoDaysDueMailSent] 
(@Id int,
 @UpdateTwoDaysDueMailSent bit)

AS
BEGIN

UPDATE [dbo].[LoanSchedule]
   SET  [TwoDaysDueMailSent] = @UpdateTwoDaysDueMailSent
		

 WHERE Id = @Id



 SET NOCOUNT ON;
SELECT @@IDENTITY;
END

GO
/****** Object:  StoredProcedure [dbo].[UpdateTwoWeeksDueMailSent]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateTwoWeeksDueMailSent] 
(@Id int,
 @UpdateTwoWeeksDueMailSent bit)

AS
BEGIN

UPDATE [dbo].[LoanSchedule]
   SET  [TwoWeeksDueMailSent] = @UpdateTwoWeeksDueMailSent
		

 WHERE Id = @Id



 SET NOCOUNT ON;
SELECT @@IDENTITY;
END

GO
/****** Object:  StoredProcedure [dbo].[UpdateUserApplyForLoanTime]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateUserApplyForLoanTime] 
(@UserId int, 
@ApplyForLoan  datetime)
--@LoanAmount int,
--@Medal nvarchar(128)

AS
BEGIN

UPDATE [dbo].[Customer]
SET [ApplyForLoan] = @ApplyForLoan --, [CreditSum] = @LoanAmount, [MedalType] = @Medal
WHERE Id = @UserId

END

GO
/****** Object:  StoredProcedure [dbo].[VerifyServiceRegistration]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[VerifyServiceRegistration] 
(
	  @pKey nvarchar(255)
	, @pResult nvarchar(255) out
)
AS
BEGIN
	DECLARE @regKey nvarchar(255);
	
	select top 1 @regKey = [key] from ServiceRegistration
	if(@regKey IS NULL)
		INSERT INTO ServiceRegistration
			([key])
		VALUES
			(@pKey);
	else
		if(@regKey <> @pKey)
		begin
			SELECT @pResult = @regKey;
			RETURN;
		end;
	SELECT @pResult = NULL;
END

GO
/****** Object:  StoredProcedure [dbo].[ViewHelper_MonthlyTurnover]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[ViewHelper_MonthlyTurnover]
AS
BEGIN
DECLARE @CustomerId INT,
		@latestAggr DATETIME,
		@analysisFuncId INT,
		@mpName NVARCHAR(255),
		@mpId INT,
		@mpTypeId INT,
		@monthTimePeroidId INT,
		@TotalMonthlyTurnover NUMERIC(18,4),
		@AggrVal NUMERIC(18,4)
		
SELECT @monthTimePeroidId = Id FROM MP_AnalysisFunctionTimePeriod WHERE Name = '30'
		
SELECT DISTINCT IdCustomer AS CustumerId, convert(NUMERIC(18,4),0) AS MonthlyTurnover INTO #tmp FROM CashRequests

DECLARE customerCursor CURSOR FOR SELECT CustumerId FROM #tmp
OPEN customerCursor
FETCH NEXT FROM customerCursor INTO @CustomerId
WHILE @@FETCH_STATUS = 0
BEGIN
	SET @TotalMonthlyTurnover = 0
	DECLARE marketplaceCursor CURSOR FOR SELECT Id, MarketPlaceId FROM MP_CustomerMarketPlace WHERE CustomerId = @CustomerId
	OPEN marketplaceCursor
	FETCH NEXT FROM marketplaceCursor INTO @mpId, @mpTypeId
	WHILE @@FETCH_STATUS = 0
	BEGIN
		SELECT @mpName = Name FROM MP_MarketplaceType WHERE Id = @mpTypeId
		IF @mpName = 'Pay Pal'
		BEGIN
			SELECT @analysisFuncId = Id FROM MP_AnalyisisFunction WHERE MarketPlaceId=@mpTypeId AND Name='TotalNetInPayments'
		END
		ELSE
		BEGIN
			IF @mpName = 'PayPoint'
			BEGIN
				SELECT @analysisFuncId = Id FROM MP_AnalyisisFunction WHERE MarketPlaceId=@mpTypeId AND Name='SumOfAuthorisedOrders'
			END
			ELSE
			BEGIN
				SELECT @analysisFuncId = Id FROM MP_AnalyisisFunction WHERE MarketPlaceId=@mpTypeId AND Name='TotalSumOfOrders'
			END
		END
		
		SELECT @latestAggr = Max(Updated) FROM MP_AnalyisisFunctionValues WHERE AnalyisisFunctionId=@analysisFuncId AND CustomerMarketPlaceId=@mpId
		IF @latestAggr IS NOT NULL
		BEGIN
			SELECT @AggrVal = ValueFloat FROM MP_AnalyisisFunctionValues WHERE AnalyisisFunctionId=@analysisFuncId AND CustomerMarketPlaceId=@mpId AND Updated = @latestAggr AND AnalysisFunctionTimePeriodId = @monthTimePeroidId
			
			SET @TotalMonthlyTurnover = @TotalMonthlyTurnover + @AggrVal
		END
		
	
		FETCH NEXT FROM marketplaceCursor INTO @mpId, @mpTypeId
	END
	CLOSE marketplaceCursor
	DEALLOCATE marketplaceCursor 
	
	UPDATE #tmp SET MonthlyTurnover=@TotalMonthlyTurnover WHERE CustumerId = @CustomerId

	FETCH NEXT FROM customerCursor INTO @CustomerId
END
CLOSE customerCursor
DEALLOCATE customerCursor

SELECT CashRequests.Id,CashRequests.IdCustomer,CashRequests.IdUnderwriter,CashRequests.CreationDate,CashRequests.SystemDecision,CashRequests.UnderwriterDecision,CashRequests.SystemDecisionDate,CashRequests.UnderwriterDecisionDate,CashRequests.EscalatedDate,CashRequests.SystemCalculatedSum,CashRequests.ManagerApprovedSum,CashRequests.MedalType,CashRequests.EscalationReason,CashRequests.APR,CashRequests.RepaymentPeriod,CashRequests.ScorePoints,CashRequests.ExpirianRating,CashRequests.AnualTurnover,CashRequests.InterestRate,CashRequests.UseSetupFee,CashRequests.EmailSendingBanned,CashRequests.LoanTypeId,CashRequests.UnderwriterComment,CashRequests.HasLoans,CashRequests.LoanTemplate,CashRequests.IsLoanTypeSelectionAllowed,CashRequests.DiscountPlanId,CashRequests.LoanSourceID,CashRequests.OfferStart,CashRequests.OfferValidUntil,CashRequests.IsCustomerRepaymentPeriodSelectionAllowed, #tmp.MonthlyTurnover 
FROM CashRequests, #tmp
WHERE #tmp.CustumerId = CashRequests.IdCustomer

DROP TABLE #tmp

END
GO
/****** Object:  UserDefinedFunction [dbo].[fn_GetCustomerAdress]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[fn_GetCustomerAdress] (@CustId int)
/*
Функция возвращает адреса кастомера
Author: Oleg Zemskyi
Date Created: 29.09.2012
*/
RETURNS @CustAdress TABLE
(
	Line1 VARCHAR (255), 
	Line2 VARCHAR (255),
	Line3 VARCHAR (255),
	Line4 VARCHAR (255),
	Line5 VARCHAR (255),
	Line6 VARCHAR (255),
	Line1Prev VARCHAR (255), 
	Line2Prev VARCHAR (255),
	Line3Prev VARCHAR (255),
	Line4Prev VARCHAR (255),
	Line5Prev VARCHAR (255),
	Line6Prev VARCHAR (255)	
)
AS BEGIN

DECLARE
@Line1 VARCHAR (255), 
@Line2 VARCHAR (255),
@Line3 VARCHAR (255),
@Line4 VARCHAR (255),
@Line5 VARCHAR (255),
@Line6 VARCHAR (255),
@Line1Prev VARCHAR (255), 
@Line2Prev VARCHAR (255),
@Line3Prev VARCHAR (255),
@Line4Prev VARCHAR (255),
@Line5Prev VARCHAR (255),
@Line6Prev VARCHAR (255)

select 
@line1=ca.Line1, @Line2=ca.Line2, @Line3 =ca.Line3,@Line4 =ca.Town , @Line5 =ca.County, @Line6 =ca.Postcode
from CustomerAddress ca
where ca.addressType= '1'
AND ca.CustomerId = @CustId AND ca.Line1 is NOT NULL

select 
@Line1Prev =ca.Line1, @Line2Prev =ca.Line2, 
@Line3Prev =ca.Line3, @Line4Prev =ca.Town, 
@Line5Prev =ca.County, @Line6Prev =ca.Postcode
from CustomerAddress ca
WHERE ca.addressType = '2'
AND ca.CustomerId = @CustId AND ca.Line1 is NOT NULL


INSERT into @CustAdress
(
	Line1,
	Line2,
	Line3,
	Line4,
	Line5,
	Line6,
	Line1Prev,
	Line2Prev,
	Line3Prev,
	Line4Prev,
	Line5Prev,
	Line6Prev
)
VALUES
(
	@Line1, 
@Line2,
@Line3,
@Line4,
@Line5,
@Line6,
@Line1Prev, 
@Line2Prev,
@Line3Prev,
@Line4Prev,
@Line5Prev,
@Line6Prev
)        
RETURN
end

GO
/****** Object:  UserDefinedFunction [dbo].[fnGetNodesFromPatch]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Script Date: 23.10.2007
-- IN: @Path - xml path, for exampl: application/sate
-- OUT: return table consist rrom Nodes xml
CREATE FUNCTION [dbo].[fnGetNodesFromPatch](@Path nvarchar(1000))
RETURNS @retNodes TABLE
(	
	NodeID int IDENTITY(1,1) NOT NULL
  ,	Name nvarchar(255) NOT NULL
)
AS
BEGIN
	DECLARE 
			@nameNode nvarchar(255),
			@indexStart int,
			@indexFinish int;

	
	SET @nameNode = ''
	SET @indexStart = 1
	SET	@indexFinish = 0	
	
	-- delete last '/'
	SET @indexFinish = CHARINDEX('/', @Path, LEN(@Path) - 1)
	WHILE (@indexFinish <> 0)
	BEGIN
		SET @Path = SUBSTRING(@Path, 1, LEN(@Path) - 1) 
		SET @indexFinish = CHARINDEX('/', @Path, LEN(@Path) - 1)
	END	
	
	SET @indexFinish = CHARINDEX('/', @Path)
	
	-- delete first '/'
	WHILE(@indexFinish = 1)
	BEGIN
		SET @Path = SUBSTRING(@Path, @indexStart + 1, LEN(@Path) - 1)		
		SET @indexFinish = CHARINDEX('/', @Path, @indexStart)
	END;
	-- gets nodes
	WHILE (@indexStart <> 0)
	BEGIN
		IF (@indexFinish = 0)		
			SET @nameNode = SUBSTRING(@Path, @indexStart, LEN(@Path) - (@indexStart - 1))		
		ELSE
			SET @nameNode = SUBSTRING(@Path, @indexStart, @indexFinish - @indexStart)		
		
		IF (@indexFinish = 0)
			SET @indexStart = 0;
		ELSE
		BEGIN
			SET @indexStart = @indexFinish + 1
			SET @indexFinish = CHARINDEX('/', @Path, @indexStart)				
		END

		INSERT INTO @retNodes
		VALUES (@nameNode)		
	END;
	RETURN;	
END

GO
/****** Object:  UserDefinedFunction [dbo].[fnPacnetBalance]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[fnPacnetBalance] ()
RETURNS @ResultSet TABLE (
	[Id]             INT,
	[PacnetBalance]  DECIMAL(18, 4),
	[ReservedAmount] DECIMAL(18, 4),
	[Date]           DATETIME,
	[Adjusted]       DECIMAL(18, 4),
	[Loans]          DECIMAL(18, 4)
) AS
BEGIN
	DECLARE @reservedAmount DECIMAL(18, 4)
	DECLARE @pacNet         DECIMAL(18, 4)
	DECLARE @lastUpdate     DATETIME
	DECLARE @loans          DECIMAL(18, 4)

	SELECT
		@reservedAmount = SUM(c.CreditSum)
	FROM
		Customer c
	WHERE
		c.CreditSum > 0
		AND
		c.CreditResult = 'Approved'
		AND
		c.Status = 'Approved'
		AND
		c.ValidFor >= GETUTCDATE()
		AND
		c.ApplyForLoan <= GETUTCDATE()
		AND
		c.IsTest = 0

	SELECT TOP(1)
		@pacNet = pb.CurrentBalance,
		@lastUpdate = pb.Date
	FROM
		PacNetBalance pb
	ORDER BY
		pb.Date DESC,
		pb.Id DESC

	SELECT
		@loans = ISNULL(SUM(l.LoanAmount - l.SetupFee), 0)
	FROM
		Loan l
	WHERE
		l.Date > DATEADD(DAY, 1, @lastUpdate)

	INSERT @ResultSet (Id, PacnetBalance, ReservedAmount, Date, Loans, Adjusted)
		VALUES (1, ISNULL(@pacNet, 0), @reservedAmount, @lastUpdate, @loans, (ISNULL(@pacNet, 0) - @loans));

	RETURN;
END

GO

/****** Object:  UserDefinedFunction [dbo].[GetExpensesPayPalTransactionsByPayer]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
* 2012-10-08 O.Zemskyi Removing duplicates
* 
*/
CREATE FUNCTION [dbo].[GetExpensesPayPalTransactionsByPayer]
(	
  @marketplaceId int,
  @payer NVARCHAR (255)
)
RETURNS @Expenses TABLE
(
	Payer NVARCHAR (255),
	M1 float, 
	M3 float,
	M6 float,
	M12 float,
	M15 float,
	M18 float,
	M24 float,
	M24Plus float
)
AS BEGIN 
	
	DECLARE @min_created DATETIME;
	DECLARE @datediff int;
	
	SELECT @min_created=MIN(Created) FROM MP_PayPalTransactionItem WHERE Payer=@payer
	SELECT @datediff =DATEDIFF(mm,@min_created,GETDATE())
		
	DECLARE
		@M1 float, @M3 float, @M6 float, @M12 float, @M15 float, @M18 float, @M24 float, @M24Plus float
		
		IF (@datediff>=1)
		begin
		SELECT TOP 1 @M1 = Income FROM GetExpensesPayPalTransactionsByPayerAndRange (@payer, @marketplaceId, DATEADD( month , -1, GETDATE()), GETDATE())
		end
		
		IF (@datediff>1)
		begin			
		SELECT TOP 1 @M3 = Income FROM GetExpensesPayPalTransactionsByPayerAndRange (@payer, @marketplaceId, DATEADD( month , -3, GETDATE()), GETDATE())
		END
		
		IF (@datediff>3)
		begin
		SELECT TOP 1 @M6 = Income FROM GetExpensesPayPalTransactionsByPayerAndRange (@payer, @marketplaceId, DATEADD( month , -6, GETDATE()), GETDATE())
		END
		
		IF (@datediff>6)
		begin
		SELECT TOP 1 @M12 = Income FROM GetExpensesPayPalTransactionsByPayerAndRange (@payer, @marketplaceId, DATEADD( month , -12, GETDATE()), GETDATE())
		END
		
		IF (@datediff>12)
		begin
		SELECT TOP 1 @M15 = Income FROM GetExpensesPayPalTransactionsByPayerAndRange (@payer, @marketplaceId, DATEADD( month , -15, GETDATE()), GETDATE())
		END
		
		IF (@datediff>15)
		begin
		SELECT TOP 1 @M18 = Income FROM GetExpensesPayPalTransactionsByPayerAndRange (@payer, @marketplaceId, DATEADD( month , -18, GETDATE()), GETDATE())
		END
		
		IF (@datediff>18)
		begin
		SELECT TOP 1 @M24 = Income FROM GetExpensesPayPalTransactionsByPayerAndRange (@payer, @marketplaceId, DATEADD( month , -24, GETDATE()), GETDATE())
		END
		
		IF (@datediff>=24)
		begin
		SELECT TOP 1 @M24Plus = Income FROM GetExpensesPayPalTransactionsByPayerAndRange (@payer, @marketplaceId, '1900-01-01 00:00:00.000', GETDATE())
		END
		
		INSERT INTO @Expenses (Payer, M1, M3 , M6 , M12, M15, M18, M24, M24Plus) VALUES (@payer, @M1, @M3, @M6, @M12, @M15, @M18, @M24, @M24Plus)

RETURN
end

GO
/****** Object:  UserDefinedFunction [dbo].[GetIncomePayPalTransactionsByPayer]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
* 2012-10-08 O.Zemskyi Removing duplicates
* 
*/
CREATE FUNCTION [dbo].[GetIncomePayPalTransactionsByPayer]
(	
  @marketplaceId int,
  @payer nvarchar(255)
)
RETURNS @Incomes TABLE
(
	Payer NVARCHAR(255),
	M1 float, 
	M3 float,
	M6 float,
	M12 float,
	M15 float,
	M18 float,
	M24 float,
	M24Plus float
)
AS BEGIN 

	DECLARE @min_created DATETIME;
	DECLARE @datediff int;
	
	SELECT @min_created=MIN(Created) FROM MP_PayPalTransactionItem WHERE Payer=@payer
	SELECT @datediff =DATEDIFF(mm,@min_created,GETDATE())
		
	DECLARE
		@M1 FLOAT, @M3 FLOAT, @M6 FLOAT, @M12 FLOAT, @M15 FLOAT, @M18 FLOAT, @M24 FLOAT, @M24Plus float
		
		IF (@datediff>=1)
		begin
		SELECT TOP 1 @M1 = Income FROM GetIncomePayPalTransactionsByPayerAndRange (@payer, @marketplaceId, DATEADD( month , -1, GETDATE()), GETDATE())
		end
		
		IF (@datediff>1)
		begin			
		SELECT TOP 1 @M3 = Income FROM GetIncomePayPalTransactionsByPayerAndRange (@payer, @marketplaceId, DATEADD( month , -3, GETDATE()), GETDATE())
		END
		
		IF (@datediff>3)
		begin
		SELECT TOP 1 @M6 = Income FROM GetIncomePayPalTransactionsByPayerAndRange (@payer, @marketplaceId, DATEADD( month , -6, GETDATE()), GETDATE())
		END
		
		IF (@datediff>6)
		begin
		SELECT TOP 1 @M12 = Income FROM GetIncomePayPalTransactionsByPayerAndRange (@payer, @marketplaceId, DATEADD( month , -12, GETDATE()), GETDATE())
		END
		
		IF (@datediff>12)
		begin
		SELECT TOP 1 @M15 = Income FROM GetIncomePayPalTransactionsByPayerAndRange (@payer, @marketplaceId, DATEADD( month , -15, GETDATE()), GETDATE())
		END
		
		IF (@datediff>15)
		begin
		SELECT TOP 1 @M18 = Income FROM GetIncomePayPalTransactionsByPayerAndRange (@payer, @marketplaceId, DATEADD( month , -18, GETDATE()), GETDATE())
		END
		
		IF (@datediff>18)
		begin
		SELECT TOP 1 @M24 = Income FROM GetIncomePayPalTransactionsByPayerAndRange (@payer, @marketplaceId, DATEADD( month , -24, GETDATE()), GETDATE())
		END
		
		IF (@datediff>=24)
		begin
		SELECT TOP 1 @M24Plus = Income FROM GetIncomePayPalTransactionsByPayerAndRange (@payer, @marketplaceId, '1900-01-01 00:00:00.000', GETDATE())
		END
		
		INSERT INTO @Incomes (Payer, M1, M3 , M6 , M12, M15, M18, M24, M24Plus) VALUES (@payer, @M1, @M3 , @M6 , @M12, @M15, @M18, @M24, @M24Plus)

RETURN
end

GO
/****** Object:  UserDefinedFunction [dbo].[GetMarketPlaceStatus]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create FUNCTION [dbo].[GetMarketPlaceStatus]
(	
  @marketplaceId INT, @customerid INT
)
RETURNS NVARCHAR(16)
AS BEGIN
	DECLARE @status NVARCHAR(64)
SELECT @status =
(
	SELECT ISNULL(
		(select 
			CASE
				WHEN( SELECT COUNT(*) from [MP_CustomerMarketPlace] mp where (mp.updateError is not null or mp.updateError = '') and mp.CustomerId = @customerId and marketplaceId = @marketplaceId) > 0 then 'Error'
				WHEN( SELECT COUNT(*) from [MP_CustomerMarketPlace] mp where (UpdatingStart is not null and UpdatingEnd is null) and mp.CustomerId = @customerId and marketplaceId = @marketplaceId) > 0 then 'Updating'
				WHEN( SELECT COUNT(*) from [MP_CustomerMarketPlace] mp where (UpdatingStart is not null and UpdatingEnd is not null) and mp.CustomerId = @customerId and marketplaceId = @marketplaceId) > 0 then 'Completed'
			END), 
		'N/A')
)
RETURN @status
end

GO
/****** Object:  UserDefinedFunction [dbo].[GetMarketPlaceStatusByName]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create FUNCTION [dbo].[GetMarketPlaceStatusByName]
(	
  @marketplaceName NVARCHAR(255), @customerid INT
)
RETURNS NVARCHAR(16)
AS BEGIN
	DECLARE @status NVARCHAR(64),
			@marketplaceId INT
	DECLARE @mt TABLE (
	MarketPlaceId INT,
	CustomerId INT,
	STATUS nvarchar (64),
	DisplayName NVARCHAR (64)
	)
	
	SELECT @marketplaceId = Id FROM MP_MarketplaceType WHERE Name = @marketplaceName

	SELECT @status =
(
	SELECT ISNULL(
		(select 
			CASE
				WHEN( SELECT COUNT(*) from [MP_CustomerMarketPlace] mp where (mp.updateError is not null or mp.updateError = '') and mp.CustomerId = @customerId and marketplaceId = @marketplaceId) > 0 then 'Error'
				WHEN( SELECT COUNT(*) from [MP_CustomerMarketPlace] mp where (UpdatingStart is not null and UpdatingEnd is null) and mp.CustomerId = @customerId and marketplaceId = @marketplaceId) > 0 then 'Updating'
				WHEN( SELECT COUNT(*) from [MP_CustomerMarketPlace] mp where (UpdatingStart is not null and UpdatingEnd is not null) and mp.CustomerId = @customerId and marketplaceId = @marketplaceId) > 0 then 'Completed'
			END), 
		'N/A')
)
	RETURN @status
end

GO
/****** Object:  UserDefinedFunction [dbo].[GetTableFromList]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetTableFromList] (@List ntext, @Delimiter varchar(10)=',')  
RETURNS @tbl TABLE (Item nvarchar(max) NOT NULL) AS
BEGIN
  DECLARE
    @idxb int, @idxe int, 
    @item nvarchar(max),
    @lend int, @lenl int, @i int
  SET @lend=DATALENGTH(@Delimiter)
  SET @lenl=DATALENGTH(@List) 
  SET @idxb=1
  WHILE SUBSTRING(@List,@idxb,@lend)=@Delimiter AND @idxb<@lenl-@lend+1 SET @idxb=@idxb+@lend
  SET @idxe=@idxb
  WHILE @idxb<=@lenl AND @idxe<=@lenl
  BEGIN
    IF SUBSTRING(@List,@idxe+1,@lend)=@Delimiter 
    BEGIN
      SET @item=SUBSTRING(@List,@idxb,@idxe-@idxb+1)
      INSERT INTO @tbl (Item) VALUES (@item)
      SET @idxb=@idxe+@lend+1
      WHILE SUBSTRING(@List,@idxb,@lend)=@Delimiter AND @idxb<@lenl-@lend+1 SET @idxb=@idxb+@lend
      SET @idxe=@idxb
    END
    ELSE IF @idxe=@lenl
    BEGIN
      SET @item=SUBSTRING(@List,@idxb,@idxe-@idxb+1)
      IF @item<>@Delimiter  
        INSERT INTO @tbl (Item) VALUES (@item)
      RETURN
    END
    ELSE
    BEGIN
      SET @idxe=@idxe+1
    END 
  END
  RETURN
END

GO
/****** Object:  UserDefinedFunction [dbo].[GetTotalExpensesPayPalTransactions]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
* 2012-10-08 O.Zemskyi Removing duplicates
* 2012-10-19 O.Zemskyi 24Mplus = total transactions
* 
*/
CREATE FUNCTION [dbo].[GetTotalExpensesPayPalTransactions]
(	
  @marketplaceId int
)
RETURNS @Expenses TABLE
(
	Payer nvarchar(255),
	M1 float, 
	M3 float,
	M6 float,
	M12 float,
	M15 float,
	M18 float,
	M24 float,
	M24Plus float
)
AS BEGIN 
	DECLARE @min_created DATETIME;
	DECLARE @datediff int;
	
	SELECT @min_created=MIN(pi.Created) FROM [dbo].MP_PayPalTransactionItem pi, [dbo].MP_PayPalTransaction pt
		Where Status = 'Completed' AND Type = 'Payment' AND
			pi.TransactionId = pt.Id AND pt.CustomerMarketPlaceId = @marketplaceId AND
			pi.NetAmountAmount > 0  
			
	SELECT @datediff =DATEDIFF(mm,@min_created,GETDATE())
	--SELECT @datediff
		
	DECLARE
		@M1 float, @M3 float, @M6 float, @M12 float, @M15 float, @M18 float, @M24 float, @M24Plus float
		
		IF (@datediff>=1)
		begin
		SELECT TOP 1 @M1 = Income FROM GetExpensesPayPalTransactionsByRange (@marketplaceId, DATEADD( month , -1, GETDATE()), GETDATE())
		end
		
		IF (@datediff>1)
		begin			
		SELECT TOP 1 @M3 = Income FROM GetExpensesPayPalTransactionsByRange (@marketplaceId, DATEADD( month , -3, GETDATE()), GETDATE())
		END
		
		IF (@datediff>=3)
		begin
		SELECT TOP 1 @M6 = Income FROM GetExpensesPayPalTransactionsByRange (@marketplaceId, DATEADD( month , -6, GETDATE()), GETDATE())
		END
		
		IF (@datediff>=6)
		begin
		SELECT TOP 1 @M12 = Income FROM GetExpensesPayPalTransactionsByRange (@marketplaceId, DATEADD( month , -12, GETDATE()), GETDATE())
		END
		
		IF (@datediff>=12)
		begin
		SELECT TOP 1 @M15 = Income FROM GetExpensesPayPalTransactionsByRange (@marketplaceId, DATEADD( month , -15, GETDATE()), GETDATE())
		END
		
		IF (@datediff>=15)
		begin
		SELECT TOP 1 @M18 = Income FROM GetExpensesPayPalTransactionsByRange (@marketplaceId, DATEADD( month , -18, GETDATE()), GETDATE())
		END
		
		IF (@datediff>=18)
		begin
		SELECT TOP 1 @M24 = Income FROM GetExpensesPayPalTransactionsByRange (@marketplaceId, DATEADD( month , -24, GETDATE()), GETDATE())
		END
		/*
		IF (@datediff>=24)
		begin
		SELECT TOP 1 @M24Plus = Income FROM GetExpensesPayPalTransactionsByRange (@marketplaceId, '1900-01-01 00:00:00.000', GETDATE())
		END
		*/
		SELECT TOP 1 @M24Plus = Income FROM GetExpensesPayPalTransactionsByRange (@marketplaceId, '1900-01-01 00:00:00.000', GETDATE())				
		INSERT INTO @Expenses (Payer, M1, M3 , M6 , M12, M15, M18, M24, M24Plus) VALUES ('Expenses', @M1, @M3 , @M6 , @M12, @M15, @M18, @M24, @M24Plus)

RETURN
end

GO
/****** Object:  UserDefinedFunction [dbo].[GetTotalIncomePayPalTransactions]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
* 2012-10-08 O.Zemskyi Removing duplicates
* 
*/
CREATE FUNCTION [dbo].[GetTotalIncomePayPalTransactions]
(	
  @marketplaceId int
)
RETURNS @Expenses TABLE
(
	Payer nvarchar(255),
	M1 float, 
	M3 float,
	M6 float,
	M12 float,
	M15 float,
	M18 float,
	M24 float,
	M24Plus float
)
AS BEGIN
	
	DECLARE @min_created DATETIME;
	DECLARE @datediff int;
	
	SELECT @min_created=MIN(pi.Created) FROM [dbo].MP_PayPalTransactionItem pi, [dbo].MP_PayPalTransaction pt
		Where Status = 'Completed' AND Type = 'Payment' AND
			pi.TransactionId = pt.Id AND pt.CustomerMarketPlaceId = @marketplaceId AND
			pi.NetAmountAmount > 0  
			
	SELECT @datediff =DATEDIFF(mm,@min_created,GETDATE())
		
	DECLARE
		@M1 float, @M3 float, @M6 float, @M12 float, @M15 float, @M18 float, @M24 float, @M24Plus float
		
		IF (@datediff>=1)
		begin
		SELECT TOP 1 @M1 = Income FROM GetIncomePayPalTransactionsByRange (@marketplaceId, DATEADD( month , -1, GETDATE()), GETDATE())
		end
		
		IF (@datediff>1)
		begin			
		SELECT TOP 1 @M3 = Income FROM GetIncomePayPalTransactionsByRange (@marketplaceId, DATEADD( month , -3, GETDATE()), GETDATE())
		END
		
		IF (@datediff>3)
		begin
		SELECT TOP 1 @M6 = Income FROM GetIncomePayPalTransactionsByRange (@marketplaceId, DATEADD( month , -6, GETDATE()), GETDATE())
		END
		
		IF (@datediff>6)
		begin
		SELECT TOP 1 @M12 = Income FROM GetIncomePayPalTransactionsByRange (@marketplaceId, DATEADD( month , -12, GETDATE()), GETDATE())
		END
		
		IF (@datediff>12)
		begin
		SELECT TOP 1 @M15 = Income FROM GetIncomePayPalTransactionsByRange (@marketplaceId, DATEADD( month , -15, GETDATE()), GETDATE())
		END
		
		IF (@datediff>15)
		begin
		SELECT TOP 1 @M18 = Income FROM GetIncomePayPalTransactionsByRange (@marketplaceId, DATEADD( month , -18, GETDATE()), GETDATE())
		END
		
		IF (@datediff>18)
		begin
		SELECT TOP 1 @M24 = Income FROM GetIncomePayPalTransactionsByRange (@marketplaceId, DATEADD( month , -24, GETDATE()), GETDATE())
		END
		
		IF (@datediff>25)
		begin
		SELECT TOP 1 @M24Plus = Income FROM GetIncomePayPalTransactionsByRange (@marketplaceId, '1900-01-01 00:00:00.000', GETDATE())
		END
		
		
		INSERT INTO @Expenses (Payer, M1, M3 , M6 , M12, M15, M18, M24, M24Plus) VALUES ('Income', @M1, @M3 , @M6 , @M12, @M15, @M18, @M24, @M24Plus)
RETURN
end

GO
/****** Object:  UserDefinedFunction [dbo].[GetTotalTransactionsPayPalTransactions]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
* 2012-10-08 O.Zemskyi Removing duplicates
* 2012-10-19 O.Zemskyi 24Mplus = total transactions
* 
*/
CREATE FUNCTION [dbo].[GetTotalTransactionsPayPalTransactions]
(	
  @marketplaceId int
)
RETURNS @Expenses TABLE
(
	Payer nvarchar(255),
	M1 float, 
	M3 float,
	M6 float,
	M12 float,
	M15 float,
	M18 float,
	M24 float,
	M24Plus float
)
AS BEGIN 
	DECLARE @min_created DATETIME;
	DECLARE @datediff int;
	
	SELECT @min_created=MIN(pi.Created) FROM [dbo].MP_PayPalTransactionItem pi, [dbo].MP_PayPalTransaction pt
		Where Status = 'Completed' AND Type = 'Payment' AND
			pi.TransactionId = pt.Id AND pt.CustomerMarketPlaceId = @marketplaceId AND
			pi.NetAmountAmount > 0  
			
	SELECT @datediff =DATEDIFF(mm,@min_created,GETDATE())
		
	DECLARE
		@M1 float, @M3 float, @M6 float, @M12 float, @M15 float, @M18 float, @M24 float, @M24Plus float
		
		IF (@datediff>=1)
		begin
		SELECT TOP 1 @M1 = Income FROM GetTransactionsCountPayPalTransactionsByRange (@marketplaceId, DATEADD( month , -1, GETDATE()), GETDATE())
		end
		
		IF (@datediff>1)
		begin			
		SELECT TOP 1 @M3 = Income FROM GetTransactionsCountPayPalTransactionsByRange (@marketplaceId, DATEADD( month , -3, GETDATE()), GETDATE())
		END
		
		IF (@datediff>=3)
		begin
		SELECT TOP 1 @M6 = Income FROM GetTransactionsCountPayPalTransactionsByRange (@marketplaceId, DATEADD( month , -6, GETDATE()), GETDATE())
		END
		
		IF (@datediff>=6)
		begin
		SELECT TOP 1 @M12 = Income FROM GetTransactionsCountPayPalTransactionsByRange (@marketplaceId, DATEADD( month , -12, GETDATE()), GETDATE())
		END
		
		IF (@datediff>=12)
		begin
		SELECT TOP 1 @M15 = Income FROM GetTransactionsCountPayPalTransactionsByRange (@marketplaceId, DATEADD( month , -15, GETDATE()), GETDATE())
		END
		
		IF (@datediff>=15)
		begin
		SELECT TOP 1 @M18 = Income FROM GetTransactionsCountPayPalTransactionsByRange (@marketplaceId, DATEADD( month , -18, GETDATE()), GETDATE())
		END
		
		IF (@datediff>=18)
		begin
		SELECT TOP 1 @M24 = Income FROM GetTransactionsCountPayPalTransactionsByRange (@marketplaceId, DATEADD( month , -24, GETDATE()), GETDATE())
		END
		
		/*
		IF (@datediff>=24)
		begin
		SELECT TOP 1 @M24Plus = Income FROM GetTransactionsCountPayPalTransactionsByRange (@marketplaceId, '1900-01-01 00:00:00.000', GETDATE())
		END
		*/
		SELECT TOP 1 @M24Plus = Income FROM GetTransactionsCountPayPalTransactionsByRange (@marketplaceId, '1900-01-01 00:00:00.000', GETDATE())		
		INSERT INTO @Expenses (Payer, M1, M3 , M6 , M12, M15, M18, M24, M24Plus) VALUES ('Transactions', @M1, @M3 , @M6 , @M12, @M15, @M18, @M24, @M24Plus)

RETURN
end

GO
/****** Object:  UserDefinedFunction [dbo].[IntToTime]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Oleg Zemskyi
-- Create date: 29.11.2012
-- Description:	Convert seconds to min/hours
-- =============================================

CREATE FUNCTION [dbo].[IntToTime]
(	
  @sec int
)
RETURNS VARCHAR (max) 
AS
BEGIN
	DECLARE @res VARCHAR (max)
SELECT @res= RIGHT((@sec / 60), 10) + ':' + 
 right('0' + rtrim(convert(char(2), @sec % 60)),2)
 RETURN @res
end

GO
/****** Object:  UserDefinedFunction [dbo].[MP_GetCurrencyRate]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Vitaly Kotov
-- Create date: 03.05.2012
-- Description:	Return Currency rate
-- =============================================
CREATE FUNCTION [dbo].[MP_GetCurrencyRate]
(
	@cCurrencyName nvarchar(50),
	@dDate datetime
)
RETURNS DECIMAL(18,8)
AS
BEGIN

    IF @cCurrencyName IS NULL
       RETURN 1
       
	DECLARE @ret decimal(18,8)	
	
	SELECT @ret = h.Price
	FROM MP_CurrencyRateHistory h
		LEFT JOIN MP_Currency c on h.CurrencyId = c.Id
	WHERE c.Name LIKE @cCurrencyName
	and h.Updated = 
	(
		SELECT MAX(h1.Updated)
		FROM MP_CurrencyRateHistory h1
		WHERE h1.CurrencyId = c.Id
			AND h1.Updated <= @dDate
	); 
	
	IF @ret IS NULL
	  SELECT @ret = c.Price
	  FROM MP_Currency c 
	  WHERE c.Name LIKE @cCurrencyName 
	  	
	 RETURN @ret

END

GO
/****** Object:  UserDefinedFunction [dbo].[MP_GetCurrencyRate1]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[MP_GetCurrencyRate1]
(
	@cCurrencyName nvarchar(50),
	@dDate datetime
)
RETURNS @retTable TABLE
(
   Rate DECIMAL(18,8)
)
AS
BEGIN

	DECLARE @ret decimal(18,8)	

    IF @cCurrencyName IS NULL
       SET @ret = 1
    ELSE
      BEGIN
			
		SELECT @ret = h.Price
		FROM MP_CurrencyRateHistory h
			LEFT JOIN MP_Currency c on h.CurrencyId = c.Id
		WHERE c.Name LIKE @cCurrencyName
		and h.Updated = 
		(
			SELECT MAX(h1.Updated)
			FROM MP_CurrencyRateHistory h1
			WHERE h1.CurrencyId = c.Id
				AND h1.Updated <= @dDate
		); 
		
		IF @ret IS NULL
		  SELECT @ret = c.Price
		  FROM MP_Currency c 
		  WHERE c.Name LIKE @cCurrencyName 
	  	END
	  	
	 INSERT INTO @retTable(Rate)
		VALUES (@ret)  	
	 RETURN;

END

GO
/****** Object:  UserDefinedFunction [dbo].[MP_List]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[MP_List] (@CustId int)
RETURNS NVARCHAR (max)
AS
BEGIN
	DECLARE @res NVARCHAR(max)
	SET @res=(
SELECT (cast(a.counter AS varchar(255)) + ' '+mmt.Name + ',') from
(
SELECT marketPlaceId, count(marketPlaceId) AS 'Counter' FROM MP_CustomerMarketPlace
WHERE CustomerId=@CustId
group BY marketPlaceId
) a, MP_MarketplaceType mmt
WHERE a.marketPlaceId=mmt.Id
FOR xml PATH('')
)
RETURN @res	
END

GO
/****** Object:  UserDefinedFunction [dbo].[udfCustomerMarketPlaces]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[udfCustomerMarketPlaces](@CustomerID INT)
RETURNS NVARCHAR(4000)
AS
BEGIN
	DECLARE @out NVARCHAR(4000)

	SET @out = ''

	SELECT
		@out = @out +
			(CASE @out WHEN '' THEN '' ELSE ', ' END) +
			(CASE COUNT(DISTINCT m.Id) WHEN 1 THEN '' ELSE CONVERT(NVARCHAR, COUNT(DISTINCT m.Id)) + ' ' END) +
			t.Name
	FROM
		MP_CustomerMarketPlace m
		INNER JOIN MP_MarketplaceType t ON m.MarketPlaceId = t.Id
	WHERE
		m.CustomerId = @CustomerID
	GROUP BY
		t.Name
	ORDER BY
		t.Name

	RETURN @out
END

GO
/****** Object:  UserDefinedFunction [dbo].[udfEarnedInterest]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[udfEarnedInterest](
	@DateStart DATETIME,
	@DateEnd DATETIME
)
RETURNS @earned_interest TABLE (
	LoanID INT NOT NULL,
	EarnedInterest DECIMAL(18, 4) NOT NULL
)
AS
BEGIN
	SELECT
		@DateStart = CONVERT(DATE, @DateStart),
		@DateEnd = CONVERT(DATE, @DateEnd)

	DECLARE @loans LoanIdListTable

	INSERT INTO @loans
	SELECT DISTINCT
		Id
	FROM
		Loan
	WHERE
		Date < @DateEnd
	UNION
	SELECT DISTINCT
		LoanId
	FROM
		LoanSchedule
	WHERE
		@DateStart <= Date

	INSERT INTO @earned_interest
	SELECT
		LoanID,
		EarnedInterest
	FROM
		dbo.udfEarnedInterestForLoans(@DateStart, @DateEnd, @loans)

	RETURN
END

GO
/****** Object:  UserDefinedFunction [dbo].[udfEarnedInterestForLoans]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[udfEarnedInterestForLoans](
	@DateStart DATETIME,
	@DateEnd DATETIME,
	@loan_ids LoanIdListTable READONLY
)
RETURNS @earned_interest TABLE (
	LoanID INT NOT NULL,
	EarnedInterest DECIMAL(18, 4) NOT NULL
)
AS
BEGIN
	--------------------------------------------------------
	--
	-- Declarations.
	--
	--------------------------------------------------------

	-- Relevant loans only.
	DECLARE @loans TABLE (
		LoanID INT NOT NULL,
		CustomerID INT NULL,
		IssueDate DATE NULL,
		LoanAmount DECIMAL(18, 4) NULL
	)

	-- Loan schedule entries for relevant loans. Seqnum is always from 1 to N with step 1.
	DECLARE @sched TABLE (
		Seqnum INT NOT NULL,
		LoanID INT NOT NULL,
		Date DATE NOT NULL,
		Rate DECIMAL(18, 4) NOT NULL
	)

	-- Last scheduled payment for each loan. Used to calculate a rate for those who
	-- didn't pay on time.
	DECLARE @last_sched TABLE (
		Seqnum INT NOT NULL,
		LoanID INT NOT NULL
	)

	-- Interest rate by period (from issuing a loan until the last payment date).
	DECLARE @rates TABLE (
		SeqnumStart INT NOT NULL,
		SeqnumEnd INT NOT NULL,
		LoanID INT NOT NULL,
		DateStart DATE NOT NULL,
		RateStart DECIMAL(18, 4) NULL,
		DateEnd DATE NOT NULL,
		RateEnd DECIMAL(18, 4) NOT NULL
	)

	-- Principal and interest rate of specific loan on specific date.
	-- Interest rate is divided by period length.
	-- I.e. for every row: principal * rate / periodlen = interest we earn for that loan on that day.
	DECLARE @daily TABLE (
		LoanID INT NOT NULL,
		Date DATETIME NOT NULL,
		Principal DECIMAL(18, 4) NOT NULL,
		InterestRate DECIMAL(18, 4) NULL,
		PeriodLen DECIMAL(18, 4) NULL
	)

	--------------------------------------------------------
	--
	-- Ok, let's g o to work!
	-- Who the fuck are you?!
	-- The workaholic!
	--
	--                    2 unlimited
	--                    Workaholic
	--
	--------------------------------------------------------

	--------------------------------------------------------
	--
	-- Retrieving relevant loans (id only).
	--
	--------------------------------------------------------
	
	INSERT INTO @loans(LoanID)
	SELECT DISTINCT
		LoanID
	FROM
		@loan_ids

	--------------------------------------------------------
	--
	-- Setting dates.
	--
	--------------------------------------------------------

	IF @DateStart IS NULL
		SELECT
			@DateStart = CONVERT(DATE, MIN(l.Date))
		FROM
			Loan l
			INNER JOIN @loans rl ON l.Id = rl.LoanID

	IF @DateEnd IS NULL
		SET @DateEnd = CONVERT(DATE, DATEADD(day, 1, GETDATE()))

	--------------------------------------------------------
	--
	-- Loading loan amount for relevant loans.
	--
	--------------------------------------------------------

	UPDATE @loans SET
		LoanAmount = l.LoanAmount,
		IssueDate = CONVERT(DATE, l.Date),
		CustomerID = l.CustomerId
	FROM
		@loans rl
		INNER JOIN Loan l ON rl.LoanID = l.Id
		INNER JOIN Customer c
			ON l.CustomerId = c.Id
			AND c.IsTest = 0

	DELETE FROM
		@loans
	WHERE
		CustomerID IS NULL

	--------------------------------------------------------
	--
	-- Normalising payment list: setting Seqnum to 1..N.
	--
	--------------------------------------------------------

	INSERT INTO @sched
	SELECT
		RANK() OVER (PARTITION BY s.LoanId ORDER BY s.LoanId, s.Date),
		l.LoanID,
		CAST(s.Date AS DATE),
		s.InterestRate
	FROM
		LoanSchedule s
		INNER JOIN @loans l ON s.LoanID = l.LoanId

	--------------------------------------------------------
	--
	-- Filling last scheduled payment.
	--
	--------------------------------------------------------

	INSERT INTO @last_sched
	SELECT
		MAX(Seqnum),
		LoanID
	FROM
		@sched
	GROUP BY
		LoanID

	--------------------------------------------------------
	--
	-- Loading interest rates for period from the first
	-- payment till the last payment.
	--
	--------------------------------------------------------

	INSERT INTO @rates
	SELECT
		l1.Seqnum,
		l2.Seqnum,
		l1.LoanID,
		CAST(l1.Date AS DATE),
		l1.Rate,
		CAST(l2.Date AS DATE),
		l2.Rate
	FROM
		@sched l1
		LEFT JOIN @sched l2
			ON l1.LoanId = l2.LoanId
			AND l1.Seqnum = l2.Seqnum - 1
	WHERE
		l2.Date IS NOT NULL

	--------------------------------------------------------
	--
	-- Loading interest rates for period from loan issue
	-- till the first payment.
	--
	--------------------------------------------------------

	INSERT INTO @rates
	SELECT
		0,
		1,
		l.LoanID,
		CAST(ol.Date AS DATE),
		NULL,
		r.DateStart,
		r.RateStart
	FROM
		@loans l
		INNER JOIN Loan ol ON l.LoanID = ol.Id
		INNER JOIN @rates r ON l.LoanID = r.LoanId AND r.SeqnumStart = 1

	--------------------------------------------------------
	--
	-- At this point:
	--
	-- @loans
	--    Contains relevant loans.
	--
	-- @sched
	--    Contains normalised payment schedule for
	--    relevant loans.
	--
	-- @rates
	--    Each row contains interest rate of specific loan
	--    during specific period (start date inclusive,
	--    end date exclusive).
	--
	-- @daily
	--    Not used so far.
	--    Now it's time to use it.
	--
	--------------------------------------------------------

	--------------------------------------------------------
	--
	-- Filling initial daily data.
	--
	--------------------------------------------------------

	;
	-- This semicolon is vital because otherwise the WITH statement
	-- is considered as a part of the previous statement rather than
	-- as a beginning of the new statement.

	WITH days AS (
		SELECT
			CAST(@DateStart AS DATETIME) AS TheDate
		
		UNION ALL
		
		SELECT
			TheDate + 1
		FROM
			Days
		WHERE
			TheDate + 1 < @DateEnd
	) INSERT INTO @daily(LoanID, Date, Principal)
	SELECT
		l.LoanID,
		CONVERT(DATE, d.TheDate),
		l.LoanAmount
	FROM
		days d
		INNER JOIN @loans l ON l.IssueDate < d.TheDate
	OPTION
		(MAXRECURSION 0)

	--------------------------------------------------------
	--
	-- Updating principal with customer payments.
	--
	--------------------------------------------------------

	UPDATE @daily SET
		Principal = d.Principal - ISNULL((
			SELECT SUM(t.LoanRepayment)
			FROM LoanTransaction t
			WHERE d.LoanID = t.LoanId
			AND t.LoanRepayment > 0
			AND t.Status = 'Done'
			AND t.Type = 'PaypointTransaction'
			AND d.Date > CONVERT(DATE, t.PostDate)
		), 0)
	FROM
		@daily d
	
	DELETE FROM
		@daily
	WHERE
		Principal = 0

	--------------------------------------------------------
	--
	-- Setting daily interest rate.
	--
	--------------------------------------------------------

	UPDATE @daily SET
		InterestRate = r.RateEnd,
		PeriodLen = DATEDIFF(day, r.DateStart, r.DateEnd)
	FROM
		@daily d
		INNER JOIN @rates r ON d.LoanID = r.LoanId
	WHERE
		r.DateStart <= d.Date AND d.Date < r.DateEnd

	--------------------------------------------------------

	UPDATE @daily SET
		InterestRate = r.RateEnd,
		PeriodLen = DATEDIFF(day, r.DateStart, r.DateEnd)
	FROM
		@daily d
		INNER JOIN @last_sched l ON d.LoanID = l.LoanID
		INNER JOIN @rates r
			ON l.LoanID = r.LoanID
			AND l.Seqnum = r.SeqnumEnd
	WHERE
		 d.Date >= r.DateEnd

	--------------------------------------------------------
	--
	-- Building result.
	--
	--------------------------------------------------------
	
	INSERT INTO @earned_interest
	SELECT
		LoanID,
		SUM(Principal * InterestRate / PeriodLen) AS EarnedInterest
	FROM
		@daily
	GROUP BY
		LoanID

	RETURN
END

GO
/****** Object:  UserDefinedFunction [dbo].[udfLoanFeesAndCharges]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[udfLoanFeesAndCharges](
	@DateStart DATETIME,
	@DateEnd DATETIME
)
RETURNS @output TABLE (
	LoanID INT NOT NULL,
	Fees DECIMAL(18, 2) NOT NULL,
	Charges DECIMAL(18, 2) NOT NULL
)
AS
BEGIN
	SELECT
		@DateStart = CONVERT(DATE, @DateStart),
		@DateEnd = CONVERT(DATE, @DateEnd)

	------------------------------------------------------------------------------

	DECLARE @Fees TABLE (
		LoanID INT NOT NULL,
		Fees DECIMAL(18, 2) NOT NULL
	)

	------------------------------------------------------------------------------

	DECLARE @Charges TABLE (
		LoanID INT NOT NULL,
		Charges DECIMAL(18, 2) NOT NULL
	)

	------------------------------------------------------------------------------

	DECLARE @SetupFee TABLE (
		LoanID INT NOT NULL,
		SetupFee DECIMAL(18, 2) NOT NULL
	)

	------------------------------------------------------------------------------

	INSERT INTO @Fees
	SELECT
		t.LoanId,
		SUM(t.Fees)
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
	WHERE
		t.Type = 'PaypointTransaction'
		AND
		t.Status = 'Done'
		AND
		@DateStart <= t.PostDate AND t.PostDate < @DateEnd
	GROUP BY
		t.LoanID
	
	------------------------------------------------------------------------------

	INSERT INTO @Charges
	SELECT
		c.LoanId,
		CONVERT(DECIMAL(18, 2),
			SUM(CASE
				WHEN ISNULL(AmountPaid, 0) > 0 THEN
					CASE WHEN ISNULL(AmountPaid, 0) < ISNULL(Amount, 0) THEN ISNULL(AmountPaid, 0) ELSE Amount END
				ELSE 0
			END)
		)
	FROM
		LoanCharges c
		INNER JOIN Loan l ON c.LoanId = l.Id
	WHERE
		@DateStart <= c.Date AND c.Date < @DateEnd
	GROUP BY
		c.LoanID

	------------------------------------------------------------------------------

	INSERT INTO @SetupFee
	SELECT
		t.LoanId,
		SUM(t.Fees)
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
	WHERE
		t.Type = 'PacnetTransaction'
		AND
		t.Status = 'Done'
		AND
		@DateStart <= t.PostDate AND t.PostDate < @DateEnd
	GROUP BY
		t.LoanID

	------------------------------------------------------------------------------

	INSERT INTO @output
	SELECT
		ISNULL(f.LoanID, ISNULL(c.LoanID, s.LoanID)),
		ISNULL(f.Fees, 0) + ISNULL(s.SetupFee, 0),
		ISNULL(c.Charges, 0) + ISNULL(s.SetupFee, 0)
	FROM
		@Fees f
		FULL OUTER JOIN @Charges c ON f.LoanID = c.LoanID
		FULL OUTER JOIN @SetupFee s
			ON f.LoanID = s.LoanID
			OR c.LoanID = s.LoanID
	WHERE
		ISNULL(f.Fees, 0) + ISNULL(s.SetupFee, 0) > 0
		OR
		ISNULL(c.Charges, 0) + ISNULL(s.SetupFee, 0) > 0
	
	RETURN
END

GO
/****** Object:  UserDefinedFunction [dbo].[udfPaymentMethod]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[udfPaymentMethod](
	@Description NVARCHAR(64)
)
RETURNS NVARCHAR(64)
AS
BEGIN
	IF @Description IS NULL
		RETURN NULL
		
	DECLARE @Prefix NVARCHAR(64) = 'Manual payment method: '

	IF @Description NOT LIKE @Prefix + '%'
		RETURN NULL

	DECLARE @PrefixLen INT = LEN(@Prefix)

	DECLARE @CommaPos INT = CHARINDEX(',', @Description, @PrefixLen + 1)

	IF @CommaPos = 0
		RETURN NULL

	RETURN SUBSTRING(@Description, @PrefixLen + 2, @CommaPos - @PrefixLen - 2)
END

GO
/****** Object:  Table [dbo].[AllowedEmail]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AllowedEmail](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AllowedEmail] [nvarchar](250) NOT NULL,
 CONSTRAINT [PK_AllowedEmail] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

/****** Object:  Table [dbo].[ApprovalsWithoutAML]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ApprovalsWithoutAML](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerId] [int] NOT NULL,
	[Username] [nvarchar](100) NULL,
	[Timestamp] [datetime] NULL,
	[DoNotShowAgain] [bit] NULL,
 CONSTRAINT [PK_ApprovalsWithoutAML] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Askville]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Askville](
	[Guid] [nvarchar](200) NULL,
	[MarketPlaceId] [int] NULL,
	[isPassed] [bit] NULL,
	[Status] [nvarchar](200) NULL,
	[SendStatus] [nvarchar](50) NULL,
	[MessageBody] [nvarchar](max) NULL,
	[CreationDate] [datetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[AutoresponderLog]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AutoresponderLog](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Email] [nvarchar](300) NOT NULL,
	[Name] [nvarchar](300) NULL,
	[DateOfAutoResponse] [datetime] NULL,
 CONSTRAINT [PK_AutoresponderLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[AvailableFunds]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AvailableFunds](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Date] [datetime] NOT NULL,
	[Value] [decimal](18, 4) NOT NULL,
 CONSTRAINT [PK_AvailableFunds] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[BasicInterestRate]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BasicInterestRate](
	[FromScore] [int] NOT NULL,
	[ToScore] [int] NOT NULL,
	[LoanIntrestBase] [decimal](18, 4) NOT NULL
) ON [PRIMARY]

GO

/****** Object:  Table [dbo].[Bug]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Bug](
	[Id] [int] NOT NULL,
	[CustomerId] [int] NULL,
	[Type] [nvarchar](200) NULL,
	[State] [nvarchar](200) NULL,
	[MarketPlaceId] [int] NULL,
	[DateOpened] [datetime] NULL,
	[DateClosed] [datetime] NULL,
	[TextOpened] [nvarchar](2000) NULL,
	[TextClosed] [nvarchar](2000) NULL,
	[UnderwriterOpenedId] [int] NULL,
	[UnderwriterClosedId] [int] NULL,
	[CreditBureauDirectorId] [int] NULL,
 CONSTRAINT [PK_Bug] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Business]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Business](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
	[Address] [nvarchar](4000) NOT NULL,
 CONSTRAINT [PK_Business] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[BusinessEntity]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BusinessEntity](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](25) NOT NULL,
	[Description] [nvarchar](1024) NULL,
	[Review] [nvarchar](1024) NOT NULL,
	[IsDeleted] [int] NULL,
	[CreationDate] [datetime] NOT NULL,
	[TerminationDate] [datetime] NULL,
	[UserId] [int] NOT NULL,
	[Document] [nvarchar](max) NOT NULL,
	[SignedDocument] [nvarchar](max) NULL,
	[SignedDocumentDelete] [nvarchar](max) NULL,
	[ItemVersion] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_BusinessEntity] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[BusinessEntity_NodeRel]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BusinessEntity_NodeRel](
	[Id] [int] NOT NULL,
	[NodeId] [int] NOT NULL,
	[BusinessEntityId] [int] NOT NULL,
 CONSTRAINT [PK_BusinessEntity_NodeRel] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[BusinessEntity_StrategyRel]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BusinessEntity_StrategyRel](
	[Id] [int] NOT NULL,
	[StrategyId] [int] NOT NULL,
	[BusinessEntityId] [int] NOT NULL,
 CONSTRAINT [PK_BusinessEntity_StrategyRel] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[CaisFlags]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CaisFlags](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FlagSetting] [nvarchar](20) NULL,
	[Description] [nvarchar](50) NULL,
	[ValidForRecordType] [nvarchar](50) NULL,
	[Comment] [nvarchar](max) NULL,
 CONSTRAINT [PK_CaisFlags] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[CaisReportsHistory]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[CaisReportsHistory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Date] [datetime] NULL,
	[FileName] [nvarchar](300) NULL,
	[Type] [int] NULL,
	[OfItems] [int] NULL,
	[GoodUsers] [int] NULL,
	[UploadStatus] [int] NULL,
	[DirName] [nvarchar](400) NULL,
	[Defaults] [int] NULL,
	[FileData] [varbinary](max) NULL,
 CONSTRAINT [PK_CaisReportsHistory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[CardInfo]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CardInfo](
	[Id] [int] NOT NULL,
	[Bank] [nvarchar](1000) NULL,
	[BankBIC] [nvarchar](200) NULL,
	[Branch] [nvarchar](1000) NULL,
	[BranchBIC] [nvarchar](200) NULL,
	[ContactAddressLine1] [nvarchar](200) NULL,
	[ContactAddressLine2] [nvarchar](200) NULL,
	[ContactPostTown] [nvarchar](200) NULL,
	[ContactPostcode] [nvarchar](200) NULL,
	[ContactPhone] [nvarchar](200) NULL,
	[ContactFax] [nvarchar](200) NULL,
	[FasterPaymentsSupported] [bit] NULL,
	[CHAPSSupported] [bit] NULL,
	[SortCode] [nvarchar](20) NULL,
	[IBAN] [nvarchar](200) NULL,
	[IsDirectDebitCapable] [bit] NULL,
	[StatusInformation] [nvarchar](200) NULL,
	[CustomerId] [int] NULL,
	[BankAccount] [nvarchar](8) NULL,
	[BWAResult] [nvarchar](100) NULL,
	[BankAccountType] [nchar](150) NULL,
 CONSTRAINT [PK_CardInfo] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[CashRequests]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CashRequests](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[IdCustomer] [int] NOT NULL,
	[IdUnderwriter] [int] NULL,
	[CreationDate] [datetime] NULL,
	[SystemDecision] [nvarchar](50) NULL,
	[UnderwriterDecision] [nvarchar](50) NULL,
	[SystemDecisionDate] [datetime] NULL,
	[UnderwriterDecisionDate] [datetime] NULL,
	[EscalatedDate] [datetime] NULL,
	[SystemCalculatedSum] [int] NULL,
	[ManagerApprovedSum] [int] NULL,
	[MedalType] [nvarchar](50) NULL,
	[EscalationReason] [nvarchar](200) NULL,
	[APR] [decimal](18, 0) NULL,
	[RepaymentPeriod] [int] NOT NULL,
	[ScorePoints] [numeric](8, 3) NULL,
	[ExpirianRating] [int] NULL,
	[AnualTurnover] [int] NULL,
	[InterestRate] [decimal](18, 4) NOT NULL,
	[UseSetupFee] [int] NOT NULL,
	[EmailSendingBanned] [bit] NULL,
	[LoanTypeId] [int] NULL,
	[UnderwriterComment] [nvarchar](400) NULL,
	[HasLoans] [bit] NULL,
	[LoanTemplate] [nvarchar](max) NULL,
	[IsLoanTypeSelectionAllowed] [int] NULL,
	[DiscountPlanId] [int] NULL,
	[LoanSourceID] [int] NOT NULL,
	[OfferStart] [datetime] NULL,
	[OfferValidUntil] [datetime] NULL,
	[IsCustomerRepaymentPeriodSelectionAllowed] [bit] NOT NULL,
 CONSTRAINT [PK_CasheRequests] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Commands]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Commands](
	[Id] [int] NOT NULL,
	[Position] [int] NULL,
	[ListId] [nchar](32) NULL,
	[Type] [nvarchar](128) NOT NULL,
	[SecAppId] [int] NULL,
	[UserId] [int] NULL,
	[AppId] [int] NULL,
	[ParamsXml] [nvarchar](max) NULL,
	[Outparams] [nvarchar](max) NULL,
	[Outlet] [nvarchar](64) NULL,
	[Status] [int] NULL,
	[ControlName] [nvarchar](1024) NULL,
	[OutletName] [nvarchar](1024) NULL,
	[FormName] [nvarchar](1024) NULL,
	[ItemsToBeSigned] [nvarchar](max) NULL,
	[SignatureRequired] [bit] NULL,
	[AttachDescription] [nvarchar](255) NULL,
	[AttachDocType] [nvarchar](255) NULL,
	[AttachFileName] [nvarchar](255) NULL,
	[AttachControlName] [nvarchar](255) NULL,
	[AttachBody] [varbinary](max) NULL,
	[StrategyName] [nvarchar](512) NULL,
	[NodeName] [nvarchar](512) NULL,
 CONSTRAINT [PK_Commands] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[CommandsList]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CommandsList](
	[Id] [nchar](32) NOT NULL,
	[UserId] [int] NULL,
	[AppId] [bigint] NULL,
	[SecAppId] [int] NULL,
	[Description] [nvarchar](max) NULL,
	[CreatedOn] [datetime] NULL,
 CONSTRAINT [PK_CommandsList] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[CompanyEmployeeCount]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CompanyEmployeeCount](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[EmployeeCount] [int] NOT NULL,
	[TopEarningEmployeeCount] [int] NOT NULL,
	[BottomEarningEmployeeCount] [int] NOT NULL,
	[EmployeeCountChange] [int] NOT NULL,
	[TotalMonthlySalary] [decimal](18, 0) NOT NULL,
 CONSTRAINT [PK_CompanyEmployeeCount] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ConfigurationVariables]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ConfigurationVariables](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
	[Value] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_ConfigurationVariables] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Control_History]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Control_History](
	[HISROTYID] [bigint] IDENTITY(1,1) NOT NULL,
	[APPLICATIONID] [bigint] NOT NULL,
	[STRATEGYID] [int] NOT NULL,
	[USERID] [int] NOT NULL,
	[NODEID] [int] NOT NULL,
	[CHANGESTIME] [datetime] NOT NULL,
	[CONTROLNAME] [nvarchar](max) NOT NULL,
	[CONTROLVALUE] [nvarchar](max) NULL,
	[CURRENTNODEPOSTFIX] [nvarchar](max) NOT NULL,
	[SECURITYAPPID] [bigint] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

/****** Object:  Table [dbo].[CRMActions]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CRMActions](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NULL,
 CONSTRAINT [PK_CRMActions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[CRMStatuses]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CRMStatuses](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NULL,
 CONSTRAINT [PK_CRMStatuses] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Customer]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Customer](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](128) NOT NULL,
	[CreditResult] [nvarchar](max) NULL,
	[CreditSum] [decimal](18, 0) NULL,
	[GreetingMailSent] [int] NULL,
	[GreetingMailSentDate] [datetime] NULL,
	[Status] [nvarchar](250) NULL,
	[AccountNumber] [nvarchar](8) NULL,
	[SortCode] [nvarchar](8) NULL,
	[FirstName] [nvarchar](250) NULL,
	[MiddleInitial] [nvarchar](250) NULL,
	[Surname] [nvarchar](250) NULL,
	[DateOfBirth] [datetime] NULL,
	[TimeAtAddress] [int] NULL,
	[ResidentialStatus] [nvarchar](250) NULL,
	[LimitedCompanyNumber] [nvarchar](255) NULL,
	[LimitedCompanyName] [nvarchar](250) NULL,
	[LimitedTimeAtAddress] [int] NULL,
	[LimitedConsentToSearch] [bit] NULL,
	[NonLimitedCompanyName] [nvarchar](250) NULL,
	[NonLimitedTimeInBusiness] [nvarchar](250) NULL,
	[NonLimitedTimeAtAddress] [int] NULL,
	[NonLimitedConsentToSearch] [bit] NULL,
	[ApplyForLoan] [datetime] NULL,
	[MedalType] [nvarchar](50) NULL,
	[PayPointTransactionId] [nvarchar](250) NULL,
	[DateEscalated] [datetime] NULL,
	[UnderwriterName] [varchar](200) NULL,
	[ManagerName] [varchar](200) NULL,
	[EscalationReason] [varchar](200) NULL,
	[DateApproved] [datetime] NULL,
	[ApplyCount] [int] NULL,
	[RejectedReason] [varchar](200) NULL,
	[Gender] [char](1) NULL,
	[MartialStatus] [nvarchar](50) NULL,
	[TypeOfBusiness] [nvarchar](50) NULL,
	[SystemDecision] [nvarchar](50) NULL,
	[CreditCardNo] [nvarchar](50) NULL,
	[DaytimePhone] [nvarchar](50) NULL,
	[MobilePhone] [nvarchar](50) NULL,
	[LimitedBusinessPhone] [nvarchar](50) NULL,
	[NonLimitedBusinessPhone] [nvarchar](50) NULL,
	[Fullname] [nvarchar](250) NULL,
	[LimitedRefNum] [nvarchar](250) NULL,
	[NonLimitedRefNum] [nvarchar](250) NULL,
	[OverallTurnOver] [decimal](18, 0) NULL,
	[WebSiteTurnOver] [decimal](18, 0) NULL,
	[BWAResult] [nvarchar](100) NULL,
	[AMLResult] [nvarchar](100) NULL,
	[Fraud] [bit] NULL,
	[Eliminated] [bit] NULL,
	[RefNumber] [nvarchar](8) NULL,
	[PayPointErrorsCount] [int] NULL,
	[SetupFee] [decimal](18, 0) NULL,
	[Comments] [nvarchar](max) NULL,
	[Details] [nvarchar](max) NULL,
	[ValidFor] [datetime] NULL,
	[CollectionStatus] [int] NOT NULL,
	[ApprovedReason] [nchar](200) NULL,
	[ReferenceSource] [nvarchar](200) NULL,
	[EmailState] [nvarchar](100) NULL,
	[IsTest] [bit] NULL,
	[CurrentDebitCard] [int] NULL,
	[BankAccountType] [nvarchar](50) NULL,
	[BankAccountValidationInvalidAttempts] [int] NULL,
	[CollectionDescription] [nvarchar](50) NULL,
	[WizardStep] [int] NULL,
	[LastStartedMainStrategyId] [int] NULL,
	[LastStartedMainStrategyEndTime] [datetime] NULL,
	[PendingStatus] [nvarchar](50) NULL,
	[DateRejected] [datetime] NULL,
	[IsLoanTypeSelectionAllowed] [int] NULL,
	[ABTesting] [nvarchar](512) NULL,
	[NumApproves] [int] NOT NULL,
	[NumRejects] [int] NOT NULL,
	[SystemCalculatedSum] [decimal](18, 4) NOT NULL,
	[ManagerApprovedSum] [decimal](18, 4) NOT NULL,
	[FirstLoanDate] [datetime] NULL,
	[LastLoanDate] [datetime] NULL,
	[AmountTaken] [decimal](18, 4) NOT NULL,
	[LastLoanAmount] [decimal](18, 4) NOT NULL,
	[TotalPrincipalRepaid] [decimal](18, 4) NOT NULL,
	[LastStatus] [nvarchar](100) NULL,
	[AvoidAutomaticDescison] [bit] NOT NULL,
	[FraudStatus] [int] NULL,
	[FinancialAccounts] [int] NOT NULL,
	[IsWasLate] [bit] NULL,
	[IsOffline] [bit] NOT NULL,
	[PromoCode] [varchar](30) NULL,
	[PropertyOwnedByCompany] [bit] NULL,
	[YearsInCompany] [nvarchar](50) NULL,
	[RentMonthsLeft] [nvarchar](50) NULL,
	[MonthlyStatusEnabled] [bit] NULL,
 CONSTRAINT [PK_Customer] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[CustomerAddress]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[CustomerAddress](
	[addressId] [int] IDENTITY(1,1) NOT NULL,
	[addressType] [int] NULL,
	[id] [varchar](50) NULL,
	[Organisation] [varchar](200) NULL,
	[Line1] [varchar](200) NULL,
	[Line2] [varchar](200) NULL,
	[Line3] [varchar](200) NULL,
	[Town] [varchar](200) NULL,
	[County] [varchar](200) NULL,
	[Postcode] [varchar](200) NULL,
	[Country] [varchar](200) NULL,
	[Rawpostcode] [varchar](200) NULL,
	[Deliverypointsuffix] [varchar](200) NULL,
	[Nohouseholds] [varchar](200) NULL,
	[Smallorg] [varchar](200) NULL,
	[Pobox] [varchar](200) NULL,
	[Mailsortcode] [varchar](200) NULL,
	[Udprn] [varchar](200) NULL,
	[CustomerId] [int] NULL,
	[DirectorId] [int] NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[CustomerInviteFriend]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerInviteFriend](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Created] [datetime] NOT NULL,
	[CustomerId] [int] NOT NULL,
	[InviteFriendSource] [nvarchar](50) NULL,
	[InvitedByFriendSource] [nvarchar](50) NULL,
 CONSTRAINT [PK_CustomerInviteFriend] PRIMARY KEY CLUSTERED 
(
	[CustomerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[CustomerLoyaltyProgram]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerLoyaltyProgram](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[CustomerID] [int] NOT NULL,
	[CustomerMarketPlaceID] [int] NULL,
	[LoanID] [int] NULL,
	[LoanScheduleID] [int] NULL,
	[ActionID] [int] NOT NULL,
	[ActionDate] [datetime] NOT NULL,
	[EarnedPoints] [numeric](29, 0) NOT NULL,
 CONSTRAINT [PK_CustomerLoyaltyProgram] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[CustomerReason]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerReason](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Reason] [nvarchar](300) NOT NULL,
	ReasonType INT,
 CONSTRAINT [PK_CustomerReason] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[CustomerRelations]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[CustomerRelations](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerId] [int] NOT NULL,
	[UserName] [nvarchar](100) NULL,
	[Incoming] [bit] NULL,
	[ActionId] [int] NULL,
	[StatusId] [int] NULL,
	[Comment] [varchar](1000) NULL,
	[Timestamp] [datetime] NULL,
 CONSTRAINT [PK_CustomerRelations] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[CustomerRequestedLoan]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerRequestedLoan](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Created] [datetime] NOT NULL,
	[CustomerId] [int] NOT NULL,
	[Amount] [decimal](18, 0) NULL,
	[ReasonId] [int] NULL,
	[OtherReason] [nvarchar](300) NULL,
	[SourceOfRepaymentId] [int] NULL,
	[OtherSourceOfRepayment] [nvarchar](300) NULL,
 CONSTRAINT [PK_CustomerRequestedLoan] PRIMARY KEY CLUSTERED 
(
	[CustomerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[CustomerScoringResult]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerScoringResult](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerId] [int] NULL,
	[AC_Parameters] [nvarchar](max) NULL,
	[AC_Descriptors] [nvarchar](max) NULL,
	[Result_Weights] [nvarchar](max) NULL,
	[Result_MAXPossiblePoints] [nvarchar](max) NULL,
	[Medal] [nvarchar](20) NULL,
	[ScorePoints] [numeric](8, 3) NULL,
	[ScoreResult] [numeric](8, 3) NULL,
	[ScoreDate] [datetime] NULL,
 CONSTRAINT [PK_CustomerScoringResult] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[CustomerSession]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerSession](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerId] [int] NOT NULL,
	[StartSession] [datetime] NOT NULL,
	[Ip] [nvarchar](50) NOT NULL,
	[IsPasswdOk] [bit] NOT NULL,
	[ErrorMessage] [nvarchar](50) NULL,
 CONSTRAINT [PK_CustomerSession] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[CustomerSourceOfRepayment]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerSourceOfRepayment](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SourceOfRepayment] [nvarchar](300) NOT NULL,
 CONSTRAINT [PK_CustomerSourceOfRepayment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[CustomerStatuses]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerStatuses](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](100) NULL,
	[IsEnabled] [bit] NOT NULL,
	[IsWarning] [bit] NOT NULL,
 CONSTRAINT [PK_CustomerStatuses] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[CustomerStatusHistory]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerStatusHistory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Username] [nvarchar](100) NULL,
	[TimeStamp] [datetime] NULL,
	[CustomerId] [int] NULL,
	[PreviousStatus] [int] NULL,
	[NewStatus] [int] NULL,
 CONSTRAINT [PK_CustomerStatusHistory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[DataSource_KeyData]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DataSource_KeyData](
	[KeyValueId] [int] IDENTITY(1,1) NOT NULL,
	[RequestId] [int] NULL,
	[KeyNameId] [int] NULL,
	[Value] [nvarchar](max) NULL,
 CONSTRAINT [PK_DataSource_KeyData] PRIMARY KEY CLUSTERED 
(
	[KeyValueId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[DataSource_KeyFields]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DataSource_KeyFields](
	[KeyNameId] [int] IDENTITY(1,1) NOT NULL,
	[KeyName] [nvarchar](512) NULL,
 CONSTRAINT [PK_DataSource_KeyFields] PRIMARY KEY CLUSTERED 
(
	[KeyNameId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[DataSource_Requests]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DataSource_Requests](
	[RequestId] [int] IDENTITY(1,1) NOT NULL,
	[ApplicationId] [int] NULL,
	[Request] [nvarchar](max) NULL,
	[CreationDate] [datetime] NULL,
 CONSTRAINT [PK_DataSource_Requests] PRIMARY KEY CLUSTERED 
(
	[RequestId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[DataSource_Responses]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DataSource_Responses](
	[RequestId] [int] NOT NULL,
	[Response] [nvarchar](max) NULL,
	[CreationDate] [datetime] NULL,
 CONSTRAINT [PK_DataSource_Responses_1] PRIMARY KEY CLUSTERED 
(
	[RequestId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[DataSource_Sources]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DataSource_Sources](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](500) NOT NULL,
	[Type] [nvarchar](20) NOT NULL,
	[Document] [nvarchar](max) NOT NULL,
	[SignedDocument] [nvarchar](max) NULL,
	[UserId] [int] NULL,
	[IsDeleted] [int] NULL,
	[CreationDate] [datetime] NULL,
	[TerminationDate] [datetime] NULL,
	[DisplayName] [nvarchar](255) NULL,
	[SignedDocumentDelete] [nvarchar](max) NULL,
 CONSTRAINT [PK_DATASOURCE_SOURCES] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_DATASOURCE_SOURCES] UNIQUE NONCLUSTERED 
(
	[Name] ASC,
	[IsDeleted] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[DataSource_StrategyRel]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DataSource_StrategyRel](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DataSourceName] [nvarchar](1024) NOT NULL,
	[StrategyId] [int] NOT NULL,
 CONSTRAINT [PK_DATASOURCE_STRATEGYREL] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[DbString]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DbString](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Key] [nvarchar](250) NOT NULL,
	[Value] [nvarchar](4000) NOT NULL,
 CONSTRAINT [PK_DbString] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[DecisionHistory]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DecisionHistory](
	[Id] [int] NOT NULL,
	[Action] [nvarchar](50) NOT NULL,
	[Date] [datetime] NOT NULL,
	[Comment] [nvarchar](2000) NULL,
	[UnderwriterId] [int] NOT NULL,
	[CustomerId] [int] NOT NULL,
	[CashRequestId] [int] NULL,
	[LoanTypeId] [int] NULL,
 CONSTRAINT [PK_DecisionHistory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Director]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Director](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerId] [int] NULL,
	[Name] [nvarchar](512) NOT NULL,
	[DateOfBirth] [datetime] NULL,
	[Middle] [nvarchar](512) NULL,
	[Surname] [nvarchar](512) NULL,
	[Gender] [char](1) NULL,
	[Email] [nvarchar](128) NOT NULL,
	[Phone] [nvarchar](50) NOT NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[DiscountPlan]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DiscountPlan](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](512) NULL,
	[ValuesStr] [nvarchar](2048) NULL,
	[IsDefault] [bit] NULL,
 CONSTRAINT [PK_DiscountPlan] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[EkmConnectorConfigs]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[EkmConnectorConfigs](
	[CfgKey] [varchar](100) NOT NULL,
	[CfgValue] [varchar](100) NULL,
 CONSTRAINT [PK_EkmConnectorConfigs] PRIMARY KEY CLUSTERED 
(
	[CfgKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[EmailAccount]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmailAccount](
	[Id] [int] NOT NULL,
	[IsDeleted] [int] NULL,
	[Type] [nvarchar](256) NOT NULL,
	[Name] [nvarchar](1024) NULL,
	[Description] [nvarchar](1024) NULL,
	[EmailFrom] [nvarchar](1024) NULL,
	[ServerAddress] [nvarchar](1024) NULL,
	[SmtpServerAddress] [nvarchar](1024) NULL,
	[Port] [int] NULL,
	[UserName] [nvarchar](1024) NULL,
	[Password] [nvarchar](1024) NULL,
	[EncryptionType] [nvarchar](64) NULL,
	[RequireAuthenication] [bit] NULL,
	[SignedDocument] [nvarchar](max) NULL,
	[SignedDocumentDelete] [nvarchar](max) NULL,
	[TerminationDate] [datetime] NULL,
	[StartDate] [datetime] NULL,
	[CreatorUserId] [int] NULL,
 CONSTRAINT [PK_EmailAccount] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[EmailConfirmationRequest]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmailConfirmationRequest](
	[Id] [uniqueidentifier] NOT NULL,
	[CustomerId] [int] NULL,
	[Date] [datetime] NOT NULL,
	[State] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_EmailConfirmationRequest] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[EntityLink]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EntityLink](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[SeriaId] [int] NOT NULL,
	[EntityType] [nvarchar](100) NOT NULL,
	[EntityId] [bigint] NOT NULL,
	[UserId] [int] NOT NULL,
	[LinksDoc] [nvarchar](max) NOT NULL,
	[SignedDoc] [nvarchar](max) NULL,
	[IsDeleted] [bit] NULL,
	[IsApproved] [bit] NULL,
 CONSTRAINT [PK_EntityLink] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ExclusiveApplication]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExclusiveApplication](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ApplicationId] [bigint] NOT NULL,
 CONSTRAINT [PK_ExclusiveApplication] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ExperianAccountStatuses]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ExperianAccountStatuses](
	[Id] [varchar](3) NOT NULL,
	[Status] [varchar](10) NULL,
	[DetailedStatus] [varchar](100) NULL,
 CONSTRAINT [PK_ExperianAccountStatuses] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ExperianAccountTypes]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ExperianAccountTypes](
	[Id] [varchar](3) NOT NULL,
	[Name] [varchar](100) NULL,
 CONSTRAINT [PK_ExperianAccountTypes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ExperianConsentAgreement]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExperianConsentAgreement](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Template] [nvarchar](max) NOT NULL,
	[CustomerId] [int] NULL,
	[FilePath] [nvarchar](400) NULL,
 CONSTRAINT [PK_ExperianConsentAgreement] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ExperianDataAgentConfigs]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ExperianDataAgentConfigs](
	[CfgKey] [varchar](100) NOT NULL,
	[CfgValue] [varchar](100) NULL,
 CONSTRAINT [PK_ExperianDataAgentConfigs] PRIMARY KEY CLUSTERED 
(
	[CfgKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ExperianDefaultAccount]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExperianDefaultAccount](
	[Id] [int] NOT NULL,
	[CustomerId] [int] NOT NULL,
	[DateAdded] [datetime] NOT NULL,
	[AccountType] [nvarchar](100) NULL,
	[Date] [datetime] NULL,
	[DelinquencyType] [nvarchar](100) NULL,
	[ServiceLogId] [bigint] NULL,
	[Balance] [int] NULL,
	[CurrentDefBalance] [int] NULL,
 CONSTRAINT [PK_ExperianDefaultAccounts] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Export_OperationJournal]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Export_OperationJournal](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[ActionDateTime] [datetime] NOT NULL,
	[SignedDocument] [ntext] NULL,
	[OperationType] [nvarchar](6) NOT NULL,
	[ContentType] [nvarchar](10) NOT NULL,
	[BinaryBody] [varbinary](max) NOT NULL,
	[ContentName] [nvarchar](255) NOT NULL,
	[JournalType] [nvarchar](50) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Export_Results]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Export_Results](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FileName] [nvarchar](3000) NULL,
	[BinaryBody] [varbinary](max) NULL,
	[FileType] [int] NULL,
	[CreationDate] [datetime] NULL,
	[SourceTemplateId] [int] NULL,
	[ApplicationId] [bigint] NULL,
	[Status] [int] NULL,
	[StatusMode] [int] NULL,
	[NodeName] [nvarchar](500) NOT NULL,
	[SignedDocumentId] [bigint] NULL,
 CONSTRAINT [PK_Export_Results] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Export_TemplateNodeRel]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Export_TemplateNodeRel](
	[TemplateId] [int] NULL,
	[NodeId] [int] NULL,
	[OutputType] [int] NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Export_TemplatesList]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Export_TemplatesList](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FileName] [nvarchar](448) NULL,
	[Description] [nvarchar](1024) NULL,
	[VariablesXml] [nvarchar](max) NULL,
	[UploadDate] [datetime] NULL,
	[IsDeleted] [int] NULL,
	[BinaryBody] [varbinary](max) NULL,
	[ExceptionType] [int] NULL,
	[UserId] [int] NULL,
	[DeleterUserId] [int] NULL,
	[DisplayName] [nvarchar](max) NULL,
	[TerminationDate] [datetime] NULL,
	[SignedDocument] [ntext] NULL,
	[DelSignedDocument] [ntext] NULL,
 CONSTRAINT [PK_Export_TemplatesList] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_Export_TemplatesList] UNIQUE NONCLUSTERED 
(
	[FileName] ASC,
	[IsDeleted] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Export_TemplateStratRel]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Export_TemplateStratRel](
	[TemplateId] [int] NULL,
	[StrategyId] [int] NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[EzbobMailNodeAttachRelation]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EzbobMailNodeAttachRelation](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ExportId] [int] NULL,
	[ToField] [nvarchar](200) NULL,
 CONSTRAINT [PK_EzbobMailNodeAttachRelation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Filter]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Filter](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FilterType] [nvarchar](256) NULL,
	[Status] [nvarchar](256) NULL,
	[Statuses] [nvarchar](2048) NULL,
	[States] [nvarchar](2048) NULL,
	[Name] [nvarchar](256) NULL,
 CONSTRAINT [PK_Filter] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[FraudAddress]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FraudAddress](
	[Id] [int] NOT NULL,
	[Postcode] [nvarchar](50) NULL,
	[Line1] [nvarchar](200) NULL,
	[Line2] [nvarchar](200) NULL,
	[Line3] [nvarchar](200) NULL,
	[Town] [nvarchar](200) NULL,
	[County] [nvarchar](200) NULL,
	[FraudUserId] [int] NOT NULL,
 CONSTRAINT [PK_FraudAddress] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[FraudBankAccount]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FraudBankAccount](
	[Id] [int] NOT NULL,
	[BankAccount] [nvarchar](50) NULL,
	[SortCode] [nvarchar](50) NULL,
	[FraudUserId] [int] NOT NULL,
 CONSTRAINT [PK_FraudBankAccount] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[FraudCompany]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FraudCompany](
	[Id] [int] NOT NULL,
	[CompanyName] [nvarchar](200) NULL,
	[RegistrationNumber] [nvarchar](50) NULL,
	[FraudUserId] [int] NOT NULL,
 CONSTRAINT [PK_FraudCompany] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[FraudDetection]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FraudDetection](
	[Id] [int] NOT NULL,
	[CurrentCustomerId] [int] NOT NULL,
	[InternalCustomerId] [int] NULL,
	[ExternalUserId] [int] NULL,
	[CurrentField] [nvarchar](200) NOT NULL,
	[CompareField] [nvarchar](200) NOT NULL,
	[Value] [nvarchar](500) NOT NULL,
	[DateOfCheck] [datetime] NULL,
	[Concurrence] [nvarchar](250) NULL,
 CONSTRAINT [PK_FraudDetection] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[FraudEmail]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FraudEmail](
	[Id] [int] NOT NULL,
	[Email] [nvarchar](250) NULL,
	[FraudUserId] [int] NOT NULL,
 CONSTRAINT [PK_FraudEmail] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[FraudEmailDomain]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FraudEmailDomain](
	[Id] [int] NOT NULL,
	[EmailDomain] [nvarchar](250) NULL,
	[FraudUserId] [int] NOT NULL,
 CONSTRAINT [PK_FraudEmailDomain] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[FraudPhone]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FraudPhone](
	[Id] [int] NOT NULL,
	[PhoneNumber] [nvarchar](50) NULL,
	[FraudUserId] [int] NOT NULL,
 CONSTRAINT [PK_FraudPhone] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[FraudShop]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FraudShop](
	[Id] [int] NOT NULL,
	[Type] [int] NOT NULL,
	[Name] [nvarchar](200) NULL,
	[FraudUserId] [int] NOT NULL,
 CONSTRAINT [PK_FraudShop] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[FraudUser]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FraudUser](
	[FirstName] [nvarchar](100) NULL,
	[LastName] [nvarchar](100) NULL,
	[Id] [int] NOT NULL,
 CONSTRAINT [PK_FraudUser] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[hibernate_unique_key]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[hibernate_unique_key](
	[next_hi] [int] NULL,
	[InventoryItemIdSeed] [int] NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[IncrementalUpdateLog]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[IncrementalUpdateLog](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[HostName] [nvarchar](50) NOT NULL,
	[AccountName] [nvarchar](50) NOT NULL,
	[ActionDate] [datetime] NOT NULL,
	[FileName] [nvarchar](200) NOT NULL,
	[FileBody] [nvarchar](max) NOT NULL,
	[CheckSum] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_IncrementalUpdateLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Loan]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Loan](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Date] [datetime] NOT NULL,
	[LoanAmount] [numeric](18, 0) NOT NULL,
	[Status] [nvarchar](50) NOT NULL,
	[Balance] [numeric](18, 2) NULL,
	[CustomerId] [int] NULL,
	[DateClosed] [datetime] NULL,
	[Repayments] [numeric](18, 2) NULL,
	[RepaymentsNum] [int] NULL,
	[OnTime] [numeric](18, 0) NULL,
	[OnTimeNum] [int] NULL,
	[Late30] [numeric](18, 2) NULL,
	[Late30Num] [int] NULL,
	[Late60] [numeric](18, 2) NULL,
	[Late60Num] [int] NULL,
	[Late90] [numeric](18, 2) NULL,
	[Late90Num] [int] NULL,
	[PastDues] [numeric](18, 2) NULL,
	[PastDuesNum] [int] NULL,
	[NextRepayment] [numeric](18, 2) NULL,
	[Position] [int] NULL,
	[Interest] [numeric](18, 2) NULL,
	[PaymentStatus] [nvarchar](50) NULL,
	[RequestCashId] [bigint] NULL,
	[RefNum] [char](11) NULL,
	[IsDefaulted] [int] NULL,
	[Late90Plus] [numeric](18, 2) NULL,
	[Late90PlusNum] [numeric](18, 0) NULL,
	[MaxDelinquencyDays] [int] NULL,
	[Principal] [decimal](18, 2) NULL,
	[InterestRate] [decimal](18, 4) NOT NULL,
	[APR] [decimal](18, 4) NULL,
	[SetupFee] [decimal](18, 4) NULL,
	[Fees] [decimal](18, 4) NULL,
	[AgreementModel] [nvarchar](max) NULL,
	[InterestPaid] [decimal](18, 4) NULL,
	[FeesPaid] [decimal](18, 4) NULL,
	[LastReportedCAISStatus] [nvarchar](50) NULL,
	[LastReportedCAISStatusDate] [datetime] NULL,
	[LoanTypeId] [int] NULL,
	[Modified] [bit] NULL,
	[LastRecalculation] [datetime] NULL,
	[InterestDue] [decimal](18, 4) NULL,
	[Is14DaysLate] [bit] NOT NULL,
	[LoanSourceID] [int] NOT NULL,
	[LoanLegalId] [int] NULL,
 CONSTRAINT [PK_Loan] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[LoanAgreement]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoanAgreement](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Template] [nvarchar](max) NOT NULL,
	[LoanId] [int] NULL,
	[FilePath] [nvarchar](400) NULL,
	[TemplateId] [int] NULL,
 CONSTRAINT [PK_LoanAgreement] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[LoanAgreementTemplate]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoanAgreementTemplate](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Template] [nvarchar](max) NULL,
	[TemplateType] [int] NOT NULL,
 CONSTRAINT [PK_LoanAgreementTemplate] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[LoanChangesHistory]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoanChangesHistory](
	[Id] [int] NOT NULL,
	[Date] [datetime] NOT NULL,
	[LoanId] [int] NOT NULL,
	[Data] [nvarchar](max) NOT NULL,
	[UserId] [int] NOT NULL,
 CONSTRAINT [PK_LoanChangesHistory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[LoanCharges]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoanCharges](
	[Id] [int] NOT NULL,
	[Amount] [decimal](18, 4) NOT NULL,
	[LoanId] [int] NOT NULL,
	[ConfigurationVariableId] [int] NULL,
	[Date] [datetime] NULL,
	[InstallmentId] [int] NULL,
	[AmountPaid] [decimal](18, 4) NULL,
	[State] [nvarchar](50) NULL,
	[Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_LoanCharges] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[LoanHistory]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoanHistory](
	[Id] [int] NOT NULL,
	[Date] [datetime] NOT NULL,
	[Status] [nchar](50) NOT NULL,
	[Balance] [decimal](18, 4) NOT NULL,
	[Interest] [decimal](18, 4) NOT NULL,
	[Principal] [decimal](18, 4) NOT NULL,
	[Fees] [decimal](18, 4) NOT NULL,
	[LoanId] [int] NULL,
	[ExpectedPrincipal] [decimal](18, 4) NULL,
	[ExpectedInterest] [decimal](18, 4) NULL,
	[ExpectedFees] [decimal](18, 4) NULL,
	[ExpectedAmountDue] [decimal](18, 4) NULL,
 CONSTRAINT [PK_LoanHistory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[LoanInterestFreeze]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoanInterestFreeze](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LoanId] [int] NOT NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
	[InterestRate] [decimal](18, 4) NOT NULL,
	[ActivationDate] [datetime] NOT NULL,
	[DeactivationDate] [datetime] NULL,
 CONSTRAINT [PK_LoanInterestFreeze] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[LoanLegal]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoanLegal](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Created] [datetime] NOT NULL,
	[CashRequestsId] [bigint] NOT NULL,
	[CreditActAgreementAgreed] [bit] NOT NULL,
	[PreContractAgreementAgreed] [bit] NOT NULL,
	[PrivateCompanyLoanAgreementAgreed] [bit] NOT NULL,
	[GuarantyAgreementAgreed] [bit] NOT NULL,
	[EUAgreementAgreed] [bit] NOT NULL,
	[LoanId] [int] NULL,
 CONSTRAINT [PK_LoanLegal] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[LoanOfferMultiplier]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoanOfferMultiplier](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[StartScore] [int] NULL,
	[EndScore] [int] NULL,
	[Multiplier] [numeric](10, 2) NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[LoanOptions]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoanOptions](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LoanId] [int] NULL,
	[AutoPayment] [bit] NULL,
	[ReductionFee] [bit] NULL,
	[LatePaymentNotification] [bit] NULL,
	[CaisAccountStatus] [nvarchar](50) NULL,
	[StopSendingEmails] [bit] NULL,
	[ManualCaisFlag] [nvarchar](20) NULL,
 CONSTRAINT [PK_CustomerOptions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[LoanId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[LoanId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[LoanId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[LoanSchedule]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoanSchedule](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Date] [datetime] NOT NULL,
	[RepaymentAmount] [numeric](18, 2) NOT NULL,
	[Interest] [numeric](18, 2) NOT NULL,
	[Status] [nvarchar](50) NOT NULL,
	[LateCharges] [numeric](18, 2) NOT NULL,
	[AmountDue] [numeric](18, 2) NOT NULL,
	[LoanId] [int] NOT NULL,
	[Position] [int] NULL,
	[Principal] [numeric](18, 2) NULL,
	[Balance] [decimal](18, 2) NULL,
	[LoanRepayment] [decimal](18, 2) NULL,
	[Delinquency] [int] NULL,
	[Fees] [decimal](18, 4) NULL,
	[TwoDaysDueMailSent] [bit] NULL,
	[TwoWeeksDueMailSent] [bit] NULL,
	[InterestPaid] [decimal](18, 4) NULL,
	[FeesPaid] [decimal](18, 4) NULL,
	[InterestRate] [numeric](18, 7) NULL,
	[FiveDaysDueMailSent] [bit] NULL,
	[CustomInstallmentDate] [date] NULL,
 CONSTRAINT [PK_LoanSchedule] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[LoanScheduleTransaction]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoanScheduleTransaction](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[LoanID] [int] NOT NULL,
	[ScheduleID] [int] NOT NULL,
	[TransactionID] [int] NOT NULL,
	[Date] [datetime] NOT NULL,
	[PrincipalDelta] [numeric](18, 2) NOT NULL,
	[FeesDelta] [numeric](18, 2) NOT NULL,
	[InterestDelta] [numeric](18, 2) NOT NULL,
	[StatusBefore] [nvarchar](50) NOT NULL,
	[StatusAfter] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_LoanScheduleTransaction] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[LoanScheduleTransactionBackFilled]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoanScheduleTransactionBackFilled](
	[LoanScheduleTransactionID] [bigint] NOT NULL,
	[TimestampCounter] [timestamp] NOT NULL,
	[IsBad] [bit] NOT NULL,
	[Step] [int] NOT NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[LoanSource]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoanSource](
	[LoanSourceID] [int] NOT NULL,
	[LoanSourceName] [nvarchar](50) NOT NULL,
	[MaxInterest] [numeric](18, 2) NULL,
	[TimestampCounter] [timestamp] NOT NULL,
	[DefaultRepaymentPeriod] [int] NULL,
	[IsCustomerRepaymentPeriodSelectionAllowed] [bit] NOT NULL,
	MaxEmployeeCount INT NULL,
	MaxAnnualTurnover DECIMAL(18, 2) NULL,
	IsDefault BIT NOT NULL CONSTRAINT DF_LoanSource_Default DEFAULT (0),
	AlertOnCustomerReasonType INT NULL
 CONSTRAINT [PK_LoanSource] PRIMARY KEY CLUSTERED 
(
	[LoanSourceID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[LoanTransaction]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[LoanTransaction](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Type] [nvarchar](100) NOT NULL,
	[PostDate] [datetime] NOT NULL,
	[Amount] [numeric](18, 2) NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[LoanId] [int] NOT NULL,
	[Status] [nvarchar](50) NULL,
	[TrackingNumber] [nvarchar](100) NULL,
	[PacnetStatus] [nvarchar](1000) NULL,
	[PaypointId] [nvarchar](1000) NULL,
	[IP] [nvarchar](100) NULL,
	[Principal] [numeric](18, 2) NULL,
	[Interest] [numeric](18, 2) NULL,
	[Fees] [numeric](18, 2) NULL,
	[Balance] [numeric](18, 2) NULL,
	[RefNumber] [nchar](14) NULL,
	[LoanRepayment] [numeric](18, 4) NULL,
	[Rollover] [numeric](18, 4) NULL,
	[InterestOnly] [bit] NULL,
	[Reconciliation] [varchar](10) NOT NULL,
	[LoanTransactionMethodId] [int] NOT NULL,
 CONSTRAINT [PK_LoanTransaction] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[LoanTransactionMethod]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoanTransactionMethod](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](64) NOT NULL,
	[DisplaySort] [int] NOT NULL,
 CONSTRAINT [PK_LoanTransactionMethod] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[LoanType]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoanType](
	[Id] [int] NOT NULL,
	[Type] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](250) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[IsDefault] [bit] NOT NULL,
	[RepaymentPeriod] [int] NULL,
 CONSTRAINT [PK_LoanType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[LockedObject]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LockedObject](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Type] [nvarchar](100) NOT NULL,
	[TimeoutPeriod] [int] NOT NULL,
	[LastUpdateTime] [datetime] NOT NULL,
 CONSTRAINT [PK_LockedObject] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Log_ServiceAction]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Log_ServiceAction](
	[LogServiceActionId] [int] IDENTITY(1,1) NOT NULL,
	[Command] [nvarchar](255) NULL,
	[ApplicationId] [int] NULL,
	[Request] [ntext] NULL,
	[Response] [ntext] NULL,
	[DateTime] [datetime] NULL,
	[UserHost] [nvarchar](255) NULL,
 CONSTRAINT [PK_Log_ServiceAction] PRIMARY KEY CLUSTERED 
(
	[LogServiceActionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Log_TraceLog]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Log_TraceLog](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ApplicationId] [int] NULL,
	[Type] [tinyint] NULL,
	[Code] [int] NULL,
	[Message] [ntext] NULL,
	[Data] [ntext] NULL,
	[InsertDate] [datetime] NULL,
	[ThreadId] [nvarchar](50) NULL,
 CONSTRAINT [PK_Log_TraceLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Log4Net]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Log4Net](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Date] [datetime] NOT NULL,
	[Thread] [nvarchar](16) NOT NULL,
	[AppID] [nvarchar](20) NULL,
	[Level] [nvarchar](16) NOT NULL,
	[Logger] [nvarchar](256) NOT NULL,
	[Message] [nvarchar](max) NOT NULL,
	[Exception] [nvarchar](max) NULL,
 CONSTRAINT [PK_Log4Net] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[LoyaltyProgramActions]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoyaltyProgramActions](
	[ActionID] [int] NOT NULL,
	[ActionName] [nvarchar](20) NOT NULL,
	[ActionDescription] [nvarchar](256) NOT NULL,
	[Cost] [int] NOT NULL,
	[ActionTypeID] [int] NOT NULL,
 CONSTRAINT [PK_LoyaltyProgramActions] PRIMARY KEY CLUSTERED 
(
	[ActionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UNQ_LoyaltyProgramActionName] UNIQUE NONCLUSTERED 
(
	[ActionName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[LoyaltyProgramActionTypes]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoyaltyProgramActionTypes](
	[ActionTypeID] [int] NOT NULL,
	[ActionTypeName] [nvarchar](256) NOT NULL,
 CONSTRAINT [PK_LoyaltyProgramActionTypes] PRIMARY KEY CLUSTERED 
(
	[ActionTypeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UNQ_LoyaltyProgramActionTypes] UNIQUE NONCLUSTERED 
(
	[ActionTypeName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MailTemplateRelation]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MailTemplateRelation](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[InternalTemplateName] [nvarchar](200) NOT NULL,
	[MandrillTemplateId] [int] NOT NULL,
 CONSTRAINT [UNIQUE_InternalTemplateName] UNIQUE NONCLUSTERED 
(
	[InternalTemplateName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MandrillTemplate]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MandrillTemplate](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[NAME] [nvarchar](200) NULL,
 CONSTRAINT [PK_MandrillTemplate] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MC_CampaignClicks]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MC_CampaignClicks](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Date] [datetime] NULL,
	[Title] [nvarchar](300) NULL,
	[Url] [nvarchar](300) NULL,
	[EmailsSent] [int] NULL,
	[Clicks] [int] NULL,
	[Email] [nvarchar](300) NULL,
 CONSTRAINT [PK_MC_CampaignClicks] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Medals]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Medals](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Medal] [nvarchar](50) NOT NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MonitoredSites]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[MonitoredSites](
	[Site] [varchar](300) NOT NULL,
 CONSTRAINT [PK_MonitoredSites] PRIMARY KEY CLUSTERED 
(
	[Site] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[MP_AlertDocument]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[MP_AlertDocument](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DocName] [nvarchar](500) NULL,
	[UploadDate] [datetime] NULL,
	[UserId] [int] NULL,
	[CustomerId] [int] NULL,
	[Description] [nvarchar](max) NULL,
	[BinaryBody] [varbinary](max) NULL,
 CONSTRAINT [PK_MP_AlertDocument] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[MP_AmazonFeedback]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_AmazonFeedback](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerMarketPlaceId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[UserRaining] [float] NULL,
	[CustomerMarketPlaceUpdatingHistoryRecordId] [int] NULL,
 CONSTRAINT [PK_MP_AmazonFeedback] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_AmazonFeedbackItem]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_AmazonFeedbackItem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AmazonFeedbackId] [int] NOT NULL,
	[TimePeriodId] [int] NOT NULL,
	[Count] [int] NULL,
	[Negative] [int] NULL,
	[Positive] [int] NULL,
	[Neutral] [int] NULL,
 CONSTRAINT [PK_MP_AmazonFeedbackItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_AmazonMarketplaceType]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_AmazonMarketplaceType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[MarketplaceId] [nvarchar](20) NOT NULL,
	[Country] [nvarchar](50) NULL,
	[Domain] [nvarchar](50) NULL,
 CONSTRAINT [PK_MP_AmazonMarketplaceType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_AmazonOrder]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_AmazonOrder](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerMarketPlaceId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[CustomerMarketPlaceUpdatingHistoryRecordId] [int] NULL,
 CONSTRAINT [PK_MP_AmazonOrder] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_AmazonOrderItem]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_AmazonOrderItem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AmazonOrderId] [int] NOT NULL,
	[OrderId] [nvarchar](128) NULL,
	[OrderItemId] [nvarchar](128) NULL,
	[PurchaseDate] [datetime] NULL,
	[PaymentsDate] [datetime] NULL,
	[BayerEmail] [nvarchar](128) NULL,
	[BayerName] [nvarchar](128) NULL,
	[BayerPhone] [nvarchar](128) NULL,
	[Sku] [nvarchar](128) NULL,
	[ProductName] [nvarchar](256) NULL,
	[QuantityPurchased] [int] NULL,
	[Currency] [nvarchar](50) NULL,
	[ItemPrice] [float] NULL,
	[ItemTax] [float] NULL,
	[RecipientName] [nvarchar](128) NULL,
	[SalesChennel] [nvarchar](128) NULL,
	[ShipStreet] [nvarchar](128) NULL,
	[ShipStreet1] [nvarchar](128) NULL,
	[ShipStreet2] [nvarchar](128) NULL,
	[ShipCityName] [nvarchar](128) NULL,
	[ShipStateOrProvince] [nvarchar](128) NULL,
	[ShipCountryName] [nvarchar](128) NULL,
	[ShipPostalCode] [nvarchar](50) NULL,
	[ShipPhone] [nvarchar](128) NULL,
	[ShipRecipient] [nvarchar](128) NULL,
	[ShipingPrice] [float] NULL,
	[ShipingTax] [float] NULL,
	[ShipServiceLevel] [nvarchar](128) NULL,
	[DeliveryStartDate] [datetime] NULL,
	[DeliveryEndDate] [datetime] NULL,
	[DeliveryTimeZone] [nvarchar](128) NULL,
	[DeliveryInstructions] [nvarchar](128) NULL,
 CONSTRAINT [PK_MP_AmazonOrderItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_AmazonOrderItem2]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_AmazonOrderItem2](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AmazonOrderId] [int] NULL,
	[OrderId] [nvarchar](50) NULL,
	[SellerOrderId] [nvarchar](50) NULL,
	[PurchaseDate] [datetime] NULL,
	[LastUpdateDate] [datetime] NULL,
	[OrderStatus] [nvarchar](50) NULL,
	[OrderTotalCurrency] [nvarchar](50) NULL,
	[OrderTotal] [decimal](18, 8) NULL,
	[NumberOfItemsShipped] [int] NULL,
	[NumberOfItemsUnshipped] [int] NULL,
 CONSTRAINT [PK_MP_AmazonOrderItem2] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_AmazonOrderItem2Backup]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_AmazonOrderItem2Backup](
	[Id] [int] NOT NULL,
	[AmazonOrderId] [int] NULL,
	[OrderId] [nvarchar](50) NULL,
	[SellerOrderId] [nvarchar](50) NULL,
	[PurchaseDate] [datetime] NULL,
	[LastUpdateDate] [datetime] NULL,
	[OrderStatus] [nvarchar](50) NULL,
	[FulfillmentChannel] [nvarchar](50) NULL,
	[SalesChannel] [nvarchar](50) NULL,
	[OrderChannel] [nvarchar](50) NULL,
	[ShipServiceLevel] [nvarchar](50) NULL,
	[OrderTotalCurrency] [nvarchar](50) NULL,
	[OrderTotal] [decimal](18, 8) NULL,
	[PaymentMethod] [nvarchar](50) NULL,
	[BuyerName] [nvarchar](128) NULL,
	[ShipmentServiceLevelCategory] [nvarchar](50) NULL,
	[BuyerEmail] [nvarchar](128) NULL,
	[NumberOfItemsShipped] [int] NULL,
	[NumberOfItemsUnshipped] [int] NULL,
	[MarketplaceId] [nvarchar](50) NULL,
	[ShipAddress1] [nvarchar](128) NULL,
	[ShipAddress2] [nvarchar](128) NULL,
	[ShipAddress3] [nvarchar](128) NULL,
	[ShipCity] [nvarchar](50) NULL,
	[ShipCountryCode] [nvarchar](50) NULL,
	[ShipCounty] [nvarchar](50) NULL,
	[ShipDistrict] [nvarchar](50) NULL,
	[ShipName] [nvarchar](50) NULL,
	[ShipPhone] [nvarchar](50) NULL,
	[PostalCode] [nvarchar](50) NULL,
	[StateOrRegion] [nvarchar](50) NULL,
 CONSTRAINT [PK_MP_AmazonOrderItem2Backup] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_AmazonOrderItem2Payment]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_AmazonOrderItem2Payment](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderItem2Id] [int] NULL,
	[SubPaymentMethod] [nvarchar](50) NULL,
	[MoneyInfoCurrency] [nvarchar](50) NULL,
	[MoneyInfoAmount] [decimal](18, 8) NULL,
 CONSTRAINT [PK_MP_AmazonOrderItem2Payment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_AmazonOrderItemDetail]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_AmazonOrderItemDetail](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderItem2Id] [int] NULL,
	[SellerSKU] [nvarchar](50) NULL,
	[AmazonOrderItemId] [nvarchar](50) NULL,
	[ASIN] [nvarchar](50) NULL,
	[CODFeeCurrency] [nvarchar](10) NULL,
	[CODFeePrice] [float] NULL,
	[CODFeeDiscountCurrency] [nvarchar](10) NULL,
	[CODFeeDiscountPrice] [float] NULL,
	[GiftMessageText] [nvarchar](max) NULL,
	[GiftWrapLevel] [nvarchar](50) NULL,
	[GiftWrapPriceCurrency] [nvarchar](10) NULL,
	[GiftWrapPrice] [float] NULL,
	[GiftWrapTaxCurrency] [nvarchar](10) NULL,
	[GiftWrapTaxPrice] [float] NULL,
	[ItemPriceCurrency] [nvarchar](10) NULL,
	[ItemPrice] [float] NULL,
	[ItemTaxCurrency] [nvarchar](10) NULL,
	[ItemTaxPrice] [float] NULL,
	[PromotionDiscountCurrency] [nvarchar](10) NULL,
	[PromotionDiscountPrice] [float] NULL,
	[QuantityOrdered] [int] NULL,
	[QuantityShipped] [int] NULL,
	[ShippingDiscountCurrency] [nvarchar](10) NULL,
	[ShippingDiscountPrice] [float] NULL,
	[ShippingPriceCurrency] [nvarchar](10) NULL,
	[ShippingPrice] [float] NULL,
	[ShippingTaxCurrency] [nvarchar](10) NULL,
	[ShippingTaxPrice] [float] NULL,
	[Title] [nvarchar](max) NULL,
 CONSTRAINT [PK_MP_AmazonOrderItemDetail] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_AmazonOrderItemDetailCatgory]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_AmazonOrderItemDetailCatgory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AmazonOrderItemDetailId] [int] NOT NULL,
	[EbayAmazonCategoryId] [int] NOT NULL,
 CONSTRAINT [PK_MP_AmazonOrderItemDetailCatgory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_AnalyisisFunction]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_AnalyisisFunction](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[MarketPlaceId] [int] NOT NULL,
	[ValueTypeId] [int] NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
	[InternalId] [uniqueidentifier] NOT NULL,
	[Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_AnalisisFunction] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_MP_AnalyisisFunctionInternalId] UNIQUE NONCLUSTERED 
(
	[InternalId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_AnalyisisFunctionValues]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_AnalyisisFunctionValues](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Updated] [datetime] NOT NULL,
	[CustomerMarketPlaceId] [int] NOT NULL,
	[AnalyisisFunctionId] [int] NOT NULL,
	[AnalysisFunctionTimePeriodId] [int] NOT NULL,
	[ValueString] [nvarchar](max) NULL,
	[ValueInt] [int] NULL,
	[ValueFloat] [float] NULL,
	[ValueDate] [datetime] NULL,
	[Value] [nvarchar](max) NULL,
	[ValueBoolean] [bit] NULL,
	[ValueXml] [nvarchar](max) NULL,
	[CustomerMarketPlaceUpdatingHistoryRecordId] [int] NULL,
 CONSTRAINT [PK_AnalyisisFunctionValues] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_AnalysisFunctionTimePeriod]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_AnalysisFunctionTimePeriod](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[InternalId] [uniqueidentifier] NOT NULL,
	[Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_AnalysisFunctionTimePeriod] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_MP_AnalysisFunctionTimePeriodInternalId] UNIQUE NONCLUSTERED 
(
	[InternalId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_ChannelGrabberOrder]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_ChannelGrabberOrder](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerMarketPlaceId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[CustomerMarketPlaceUpdatingHistoryRecordId] [int] NULL,
 CONSTRAINT [PK_MP_ChannelGrabberOrder] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_ChannelGrabberOrderItem]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_ChannelGrabberOrderItem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderId] [int] NULL,
	[NativeOrderId] [nvarchar](300) NULL,
	[TotalCost] [numeric](18, 2) NULL,
	[CurrencyCode] [nvarchar](3) NULL,
	[PaymentDate] [datetime] NULL,
	[PurchaseDate] [datetime] NULL,
	[OrderStatus] [nvarchar](300) NULL,
	[IsExpense] [int] NOT NULL,
 CONSTRAINT [PK_MP_ChannelGrabberOrderItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_Currency]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_Currency](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](10) NOT NULL,
	[Price] [decimal](18, 8) NULL,
	[LastUpdated] [datetime] NULL,
 CONSTRAINT [PK_MP_Currency] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_CurrencyRateHistory]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_CurrencyRateHistory](
	[Id] [bigint] NOT NULL,
	[CurrencyId] [int] NOT NULL,
	[Price] [decimal](18, 8) NULL,
	[Updated] [datetime] NOT NULL,
 CONSTRAINT [PK_MP_CurrencyRateHistory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_CustomerMarketPlace]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[MP_CustomerMarketPlace](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[MarketPlaceId] [int] NOT NULL,
	[CustomerId] [int] NOT NULL,
	[SecurityData] [varbinary](max) NOT NULL,
	[DisplayName] [nvarchar](512) NULL,
	[Created] [datetime] NULL,
	[Updated] [datetime] NULL,
	[UpdatingStart] [datetime] NULL,
	[UpdatingEnd] [datetime] NULL,
	[EliminationPassed] [bit] NULL,
	[Warning] [nvarchar](max) NULL,
	[UpdateError] [nvarchar](max) NULL,
	[UpdatingTimePassInSeconds]  AS (datediff(second,[UpdatingStart],[UpdatingEnd])),
	[TokenExpired] [int] NOT NULL,
	[OriginationDate] [datetime] NULL,
	[Disabled] [bit] NULL,
	[AmazonMarketPlaceId] [int] NULL,
 CONSTRAINT [PK_CustomerMarketPlace] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[MP_CustomerMarketplaceUpdatingActionLog]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_CustomerMarketplaceUpdatingActionLog](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CustomerMarketplaceUpdatingHistoryRecordId] [int] NULL,
	[UpdatingStart] [datetime] NULL,
	[UpdatingEnd] [datetime] NULL,
	[ActionName] [nvarchar](128) NULL,
	[ControlValueName] [nvarchar](128) NULL,
	[ControlValue] [nvarchar](max) NULL,
	[Error] [nvarchar](max) NULL,
	[UpdatingTimePassInSeconds]  AS (datediff(second,[UpdatingStart],[UpdatingEnd])),
	[ElapsedAggregateData] [bigint] NULL,
	[ElapsedRetrieveDataFromDatabase] [bigint] NULL,
	[ElapsedRetrieveDataFromExternalService] [bigint] NULL,
	[ElapsedStoreAggregatedData] [bigint] NULL,
	[ElapsedStoreDataToDatabase] [bigint] NULL,
 CONSTRAINT [PK_MP_CustomerMarketPlaceUpdatingActionLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_CustomerMarketplaceUpdatingCounter]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_CustomerMarketplaceUpdatingCounter](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CustomerMarketplaceUpdatingActionLogId] [bigint] NOT NULL,
	[Created] [datetime] NOT NULL,
	[Method] [nvarchar](256) NULL,
	[Details] [nvarchar](256) NULL,
 CONSTRAINT [PK_MP_CustomerMarketplaceUpdatingCounter] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_CustomerMarketPlaceUpdatingHistory]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_CustomerMarketPlaceUpdatingHistory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerMarketPlaceId] [int] NOT NULL,
	[UpdatingStart] [datetime] NOT NULL,
	[UpdatingEnd] [datetime] NULL,
	[Error] [nvarchar](max) NULL,
	[UpdatingTimePassInSeconds]  AS (datediff(second,[UpdatingStart],[UpdatingEnd])),
 CONSTRAINT [PK_MP_CustomerMarketPlaceUpdatingHistory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_EbayAmazonCategory]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_EbayAmazonCategory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[MarketplaceTypeId] [int] NOT NULL,
	[ParentId] [int] NULL,
	[ServiceCategoryId] [nvarchar](128) NOT NULL,
	[Name] [nvarchar](256) NULL,
	[IsVirtual] [bit] NULL,
 CONSTRAINT [PK_MP_EbayAmazonCategory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_EbayAmazonInventory]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_EbayAmazonInventory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerMarketPlaceId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[AmazonUseAFN] [bit] NULL,
	[CustomerMarketPlaceUpdatingHistoryRecordId] [int] NULL,
 CONSTRAINT [PK_MP_Inventory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_EbayAmazonInventoryItem]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_EbayAmazonInventoryItem](
	[Id] [int] NOT NULL,
	[InventoryId] [int] NOT NULL,
	[BidCount] [int] NULL,
	[Sku] [nvarchar](128) NULL,
	[Price] [float] NULL,
	[Quantity] [int] NULL,
	[ItemId] [nvarchar](128) NULL,
	[Currency] [nvarchar](10) NULL,
 CONSTRAINT [PK_MP_InventoryItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_EbayExternalTransaction]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_EbayExternalTransaction](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderItemId] [int] NULL,
	[TransactionID] [nvarchar](128) NULL,
	[TransactionTime] [datetime] NULL,
	[FeeOrCreditCurrency] [nvarchar](50) NULL,
	[FeeOrCreditPrice] [float] NULL,
	[PaymentOrRefundACurrency] [nvarchar](50) NULL,
	[PaymentOrRefundAPrice] [float] NULL,
 CONSTRAINT [PK_MP_EbayExternalTransaction] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_EbayFeedback]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_EbayFeedback](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerMarketPlaceId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[RepeatBuyerCount] [int] NULL,
	[RepeatBuyerPercent] [float] NULL,
	[TransactionPercent] [float] NULL,
	[UniqueBuyerCount] [int] NULL,
	[UniqueNegativeCount] [int] NULL,
	[UniquePositiveCount] [int] NULL,
	[UniqueNeutralCount] [int] NULL,
	[CustomerMarketPlaceUpdatingHistoryRecordId] [int] NULL,
 CONSTRAINT [PK_MP_EbayFeedback] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_EbayFeedbackItem]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_EbayFeedbackItem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EbayFeedbackId] [int] NOT NULL,
	[TimePeriodId] [int] NOT NULL,
	[Negative] [int] NULL,
	[Positive] [int] NULL,
	[Neutral] [int] NULL,
 CONSTRAINT [PK_MP_EbayFeedbackItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_EbayOrder]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_EbayOrder](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerMarketPlaceId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[CustomerMarketPlaceUpdatingHistoryRecordId] [int] NULL,
 CONSTRAINT [PK_MP_Order] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_EbayOrderItem]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_EbayOrderItem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderId] [int] NOT NULL,
	[AdjustmentAmount] [float] NULL,
	[AdjustmentCurrency] [nvarchar](50) NULL,
	[AmountPaidAmount] [float] NULL,
	[AmountPaidCurrency] [nvarchar](50) NULL,
	[SubTotalAmount] [float] NULL,
	[SubTotalCurrency] [nvarchar](50) NULL,
	[TotalAmount] [float] NULL,
	[TotalCurrency] [nvarchar](50) NULL,
	[PaymentStatus] [nvarchar](50) NULL,
	[PaymentMethod] [nvarchar](50) NULL,
	[CheckoutStatus] [nvarchar](50) NULL,
	[OrderStatus] [nvarchar](50) NULL,
	[PaymentHoldStatus] [nvarchar](50) NULL,
	[PaymentMethodsList] [nvarchar](256) NULL,
	[CreatedTime] [datetime] NULL,
	[PaymentTime] [datetime] NULL,
	[ShippedTime] [datetime] NULL,
	[BuyerName] [nvarchar](128) NULL,
	[ShippingAddressId] [int] NULL,
 CONSTRAINT [PK_MP_OrderItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_EBayOrderItemDetail]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_EBayOrderItemDetail](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ItemId] [nvarchar](128) NOT NULL,
	[PrimaryCategoryId] [int] NULL,
	[SecondaryCategoryId] [int] NULL,
	[FreeAddedCategoryId] [int] NULL,
	[Title] [nvarchar](max) NULL,
 CONSTRAINT [PK_MP_EBayOrderItemInfo] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_EbayRaitingItem]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_EbayRaitingItem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EbayFeedbackId] [int] NOT NULL,
	[TimePeriodId] [int] NOT NULL,
	[CommunicationCount] [int] NULL,
	[Communication] [float] NULL,
	[ItemAsDescribedCount] [int] NULL,
	[ItemAsDescribed] [float] NULL,
	[ShippingTimeCount] [int] NULL,
	[ShippingTime] [float] NULL,
	[ShippingAndHandlingChargesCount] [int] NULL,
	[ShippingAndHandlingCharges] [float] NULL,
 CONSTRAINT [PK_MP_EbayRaitingItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_EbayTransaction]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_EbayTransaction](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderItemId] [int] NOT NULL,
	[CreatedDate] [datetime] NULL,
	[QuantityPurchased] [int] NULL,
	[PaymentHoldStatus] [nvarchar](50) NULL,
	[PaymentMethodUsed] [nvarchar](50) NULL,
	[Price] [float] NULL,
	[PriceCurrency] [nvarchar](50) NULL,
	[ItemID] [nvarchar](128) NULL,
	[ItemPrivateNotes] [nvarchar](max) NULL,
	[ItemSellerInventoryID] [nvarchar](128) NULL,
	[ItemSKU] [nvarchar](128) NULL,
	[eBayTransactionId] [nvarchar](128) NULL,
	[ItemInfoId] [int] NULL,
 CONSTRAINT [PK_MP_EbayTransaction] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_EbayUserAccountData]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_EbayUserAccountData](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerMarketPlaceId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[PaymentMethod] [nvarchar](50) NULL,
	[PastDue] [bit] NULL,
	[CurrentBalance] [float] NULL,
	[CreditCardModifyDate] [datetime] NULL,
	[CreditCardInfo] [nvarchar](max) NULL,
	[CreditCardExpiration] [datetime] NULL,
	[BankModifyDate] [datetime] NULL,
	[AccountState] [nvarchar](50) NULL,
	[AmountPastDueCurrency] [nvarchar](50) NULL,
	[AmountPastDueAmount] [float] NULL,
	[BankAccountInfo] [nvarchar](max) NULL,
	[AccountId] [nvarchar](max) NULL,
	[Currency] [nvarchar](50) NULL,
	[CustomerMarketPlaceUpdatingHistoryRecordId] [int] NULL,
 CONSTRAINT [PK_MP_EbayUserAccountData] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_EbayUserAdditionalAccountData]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_EbayUserAdditionalAccountData](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EbayUserAccountDataId] [int] NOT NULL,
	[Currency] [nvarchar](50) NULL,
	[AccountCode] [nvarchar](256) NULL,
	[Balance] [float] NULL,
 CONSTRAINT [PK_MP_EbayUserAdditionalAccountData] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_EbayUserAddressData]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_EbayUserAddressData](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AddressID] [nvarchar](256) NULL,
	[AddressOwner] [nvarchar](256) NULL,
	[AddressRecordType] [nvarchar](256) NULL,
	[AddressStatus] [nvarchar](256) NULL,
	[AddressUsage] [nvarchar](256) NULL,
	[CityName] [nvarchar](256) NULL,
	[CompanyName] [nvarchar](256) NULL,
	[CountryCode] [nvarchar](256) NULL,
	[CountryName] [nvarchar](256) NULL,
	[County] [nvarchar](256) NULL,
	[ExternalAddressID] [nvarchar](256) NULL,
	[FirstName] [nvarchar](256) NULL,
	[InternationalName] [nvarchar](256) NULL,
	[InternationalStateAndCity] [nvarchar](256) NULL,
	[InternationalStreet] [nvarchar](256) NULL,
	[LastName] [nvarchar](256) NULL,
	[Name] [nvarchar](256) NULL,
	[Phone] [nvarchar](256) NULL,
	[Phone2] [nvarchar](256) NULL,
	[Phone2AreaOrCityCode] [nvarchar](256) NULL,
	[Phone2CountryCode] [nvarchar](256) NULL,
	[Phone2CountryPrefix] [nvarchar](256) NULL,
	[Phone2LocalNumber] [nvarchar](256) NULL,
	[PhoneAreaOrCityCode] [nvarchar](256) NULL,
	[PhoneCountryCode] [nvarchar](256) NULL,
	[PhoneCountryCodePrefix] [nvarchar](256) NULL,
	[PhoneLocalNumber] [nvarchar](256) NULL,
	[PostalCode] [nvarchar](256) NULL,
	[StateOrProvince] [nvarchar](256) NULL,
	[Street] [nvarchar](256) NULL,
	[Street1] [nvarchar](256) NULL,
	[Street2] [nvarchar](256) NULL,
 CONSTRAINT [PK_MP_EbayUserAddressData] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_EbayUserData]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_EbayUserData](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerMarketPlaceId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[UserID] [nvarchar](256) NULL,
	[BillingEmail] [nvarchar](256) NULL,
	[eBayGoodStanding] [bit] NULL,
	[EIASToken] [nvarchar](256) NULL,
	[EMail] [nvarchar](256) NULL,
	[FeedbackPrivate] [bit] NULL,
	[FeedbackScore] [int] NULL,
	[FeedbackRatingStar] [nvarchar](50) NULL,
	[IdVerified] [bit] NULL,
	[NewUser] [bit] NULL,
	[PayPalAccountStatus] [nvarchar](50) NULL,
	[PayPalAccountType] [nvarchar](50) NULL,
	[QualifiesForSelling] [bit] NULL,
	[RegistrationAddressId] [int] NULL,
	[RegistrationDate] [datetime] NULL,
	[SellerInfoQualifiesForB2BVAT] [bit] NULL,
	[SellerInfoSellerBusinessType] [nvarchar](50) NULL,
	[SellerInfoSellerPaymentAddressId] [int] NULL,
	[SellerInfoStoreOwner] [bit] NULL,
	[SellerInfoStoreSite] [nvarchar](max) NULL,
	[SellerInfoStoreURL] [nvarchar](max) NULL,
	[SellerInfoTopRatedSeller] [bit] NULL,
	[SellerInfoTopRatedProgram] [nvarchar](max) NULL,
	[Site] [nvarchar](max) NULL,
	[SkypeID] [nvarchar](256) NULL,
	[IDChanged] [bit] NULL,
	[IDLastChanged] [datetime] NULL,
	[CustomerMarketPlaceUpdatingHistoryRecordId] [int] NULL,
 CONSTRAINT [PK_MP_EbayUserData] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_EkmOrder]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_EkmOrder](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerMarketPlaceId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[CustomerMarketPlaceUpdatingHistoryRecordId] [int] NULL,
 CONSTRAINT [PK_MP_EkmOrder] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_EkmOrderItem]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_EkmOrderItem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderId] [int] NULL,
	[OrderNumber] [nvarchar](300) NULL,
	[CustomerId] [int] NULL,
	[CompanyName] [nvarchar](300) NULL,
	[FirstName] [nvarchar](300) NULL,
	[LastName] [nvarchar](300) NULL,
	[EmailAddress] [nvarchar](300) NULL,
	[TotalCost] [numeric](18, 2) NULL,
	[OrderDate] [datetime] NULL,
	[OrderDateIso] [datetime] NULL,
	[OrderStatus] [nvarchar](300) NULL,
	[OrderStatusColour] [nvarchar](300) NULL,
 CONSTRAINT [PK_MP_EkmOrderItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_ExperianBankCache]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_ExperianBankCache](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[KeyData] [nvarchar](500) NULL,
	[LastUpdateDate] [datetime] NULL,
	[Data] [nvarchar](max) NULL,
	[ServiceLogId] [bigint] NULL,
 CONSTRAINT [PK_MP_ExperianBankCache] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_ExperianDataCache]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_ExperianDataCache](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](500) NULL,
	[Surname] [nvarchar](500) NULL,
	[PostCode] [nvarchar](500) NULL,
	[BirthDate] [datetime] NULL,
	[LastUpdateDate] [datetime] NULL,
	[JsonPacket] [nvarchar](max) NULL,
	[JsonPacketInput] [nvarchar](max) NULL,
	[ExperianError] [nvarchar](max) NULL,
	[ExperianScore] [int] NULL,
	[ExperianResult] [nvarchar](500) NULL,
	[ExperianWarning] [nvarchar](max) NULL,
	[ExperianReject] [nvarchar](max) NULL,
	[CompanyRefNumber] [nvarchar](50) NULL,
	[CustomerId] [bigint] NULL,
	[DirectorId] [bigint] NULL,
 CONSTRAINT [PK_MP_ExperianDataCache] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_FreeAgentCompany]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_FreeAgentCompany](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RequestId] [int] NOT NULL,
	[url] [nvarchar](250) NULL,
	[name] [nvarchar](250) NULL,
	[subdomain] [nvarchar](250) NULL,
	[type] [nvarchar](250) NULL,
	[currency] [nvarchar](250) NULL,
	[mileage_units] [nvarchar](250) NULL,
	[company_start_date] [datetime] NULL,
	[freeagent_start_date] [datetime] NULL,
	[first_accounting_year_end] [datetime] NULL,
	[company_registration_number] [int] NULL,
	[sales_tax_registration_status] [nvarchar](250) NULL,
	[sales_tax_registration_number] [int] NULL,
 CONSTRAINT [PK_MP_FreeAgentCompany] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_FreeAgentExpense]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_FreeAgentExpense](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RequestId] [int] NOT NULL,
	[CategoryId] [int] NOT NULL,
	[url] [nvarchar](250) NULL,
	[username] [nvarchar](250) NULL,
	[category] [nvarchar](250) NULL,
	[dated_on] [datetime] NULL,
	[currency] [nvarchar](10) NULL,
	[gross_value] [numeric](18, 2) NULL,
	[native_gross_value] [numeric](18, 2) NULL,
	[sales_tax_rate] [numeric](18, 2) NULL,
	[sales_tax_value] [numeric](18, 2) NULL,
	[native_sales_tax_value] [numeric](18, 2) NULL,
	[description] [nvarchar](250) NULL,
	[manual_sales_tax_amount] [numeric](18, 2) NULL,
	[updated_at] [datetime] NULL,
	[created_at] [datetime] NULL,
	[attachment_url] [nvarchar](250) NULL,
	[attachment_content_src] [nvarchar](1000) NULL,
	[attachment_content_type] [nvarchar](250) NULL,
	[attachment_file_name] [nvarchar](250) NULL,
	[attachment_file_size] [int] NULL,
	[attachment_description] [nvarchar](250) NULL,
 CONSTRAINT [PK_MP_FreeAgentExpense] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_FreeAgentExpenseCategory]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_FreeAgentExpenseCategory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[url] [nvarchar](250) NULL,
	[description] [nvarchar](250) NULL,
	[nominal_code] [nvarchar](250) NULL,
	[allowable_for_tax] [bit] NULL,
	[tax_reporting_name] [nvarchar](250) NULL,
	[auto_sales_tax_rate] [nvarchar](250) NULL,
	[category_group] [nvarchar](250) NULL,
 CONSTRAINT [PK_MP_FreeAgentExpenseCategory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_MP_FreeAgentExpenseCategory_url] UNIQUE NONCLUSTERED 
(
	[url] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_FreeAgentInvoice]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_FreeAgentInvoice](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RequestId] [int] NOT NULL,
	[url] [nvarchar](250) NULL,
	[contact] [nvarchar](250) NULL,
	[dated_on] [datetime] NULL,
	[due_on] [datetime] NULL,
	[reference] [nvarchar](250) NULL,
	[currency] [nvarchar](10) NULL,
	[exchange_rate] [numeric](18, 4) NULL,
	[net_value] [numeric](18, 2) NULL,
	[total_value] [numeric](18, 2) NULL,
	[paid_value] [numeric](18, 2) NULL,
	[due_value] [numeric](18, 2) NULL,
	[status] [nvarchar](250) NULL,
	[omit_header] [bit] NULL,
	[payment_terms_in_days] [int] NULL,
	[paid_on] [datetime] NULL,
 CONSTRAINT [PK_MP_FreeAgentInvoice] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_FreeAgentInvoiceItem]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_FreeAgentInvoiceItem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[InvoiceId] [int] NOT NULL,
	[url] [nvarchar](250) NULL,
	[position] [int] NULL,
	[description] [nvarchar](250) NULL,
	[item_type] [nvarchar](250) NULL,
	[price] [numeric](18, 2) NULL,
	[quantity] [numeric](18, 2) NULL,
	[category] [nvarchar](250) NULL,
 CONSTRAINT [PK_MP_FreeAgentInvoiceItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_FreeAgentRequest]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_FreeAgentRequest](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerMarketPlaceId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[CustomerMarketPlaceUpdatingHistoryRecordId] [int] NULL,
 CONSTRAINT [PK_MP_FreeAgentRequest] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_FreeAgentUsers]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_FreeAgentUsers](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RequestId] [int] NOT NULL,
	[url] [nvarchar](250) NULL,
	[first_name] [nvarchar](250) NULL,
	[last_name] [nvarchar](250) NULL,
	[email] [nvarchar](250) NULL,
	[role] [nvarchar](250) NULL,
	[permission_level] [int] NULL,
	[opening_mileage] [numeric](18, 2) NULL,
	[updated_at] [datetime] NULL,
	[created_at] [datetime] NULL,
 CONSTRAINT [PK_MP_FreeAgentUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_MarketplaceGroup]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_MarketplaceGroup](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
 CONSTRAINT [PK_MP_MarketplaceGroup] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_MarketplaceType]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_MarketplaceType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[InternalId] [uniqueidentifier] NOT NULL,
	[Description] [nvarchar](max) NULL,
	[ActiveWizardOnline] [bit] NULL,
	[ActiveDashboardOnline] [bit] NULL,
	[ActiveWizardOffline] [bit] NULL,
	[ActiveDashboardOffline] [bit] NULL,
	[PriorityOnline] [int] NULL,
	[PriorityOffline] [int] NULL,
	[GroupId] [int] NULL,
	[Ribbon] [nvarchar](50) NULL,
	[MandatoryOnline] [bit] NULL,
	[MandatoryOffline] [bit] NULL,
 CONSTRAINT [PK_MarketPlace] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_MP_MarketPlaceInternalId] UNIQUE NONCLUSTERED 
(
	[InternalId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_PayPalAggregationFormula]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_PayPalAggregationFormula](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FormulaNum] [int] NOT NULL,
	[FormulaName] [nvarchar](300) NOT NULL,
	[Type] [nvarchar](300) NOT NULL,
	[Status] [nvarchar](300) NOT NULL,
	[Positive] [bit] NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_PayPalPersonalInfo]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_PayPalPersonalInfo](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerMarketPlaceId] [int] NOT NULL,
	[Updated] [datetime] NOT NULL,
	[FirstName] [nvarchar](max) NULL,
	[LastName] [nvarchar](max) NULL,
	[EMail] [nvarchar](max) NULL,
	[FullName] [nvarchar](max) NULL,
	[BusinessName] [nvarchar](max) NULL,
	[Country] [nvarchar](max) NULL,
	[PlayerId] [nvarchar](max) NULL,
	[DateOfBirth] [datetime] NULL,
	[Postcode] [nvarchar](max) NULL,
	[Street1] [nvarchar](max) NULL,
	[Street2] [nvarchar](max) NULL,
	[City] [nvarchar](max) NULL,
	[State] [nvarchar](max) NULL,
	[Phone] [nvarchar](max) NULL,
 CONSTRAINT [PK_MP_PersonalInfo] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [U_MP_PersonalInfoCustomerMarketPlace] UNIQUE NONCLUSTERED 
(
	[CustomerMarketPlaceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_PayPalTransaction]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_PayPalTransaction](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerMarketPlaceId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[CustomerMarketPlaceUpdatingHistoryRecordId] [int] NULL,
 CONSTRAINT [PK_MP_Transaction] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_PayPalTransactionItem]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_PayPalTransactionItem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TransactionId] [int] NOT NULL,
	[Created] [datetime] NULL,
	[FeeAmountCurrency] [nvarchar](50) NULL,
	[FeeAmountAmount] [float] NULL,
	[GrossAmountCurrency] [nvarchar](50) NULL,
	[GrossAmountAmount] [float] NULL,
	[NetAmountCurrency] [nvarchar](50) NULL,
	[NetAmountAmount] [float] NULL,
	[TimeZone] [nvarchar](128) NULL,
	[Type] [nvarchar](128) NULL,
	[Status] [nvarchar](128) NULL,
	[Payer] [nvarchar](128) NULL,
	[PayerDisplayName] [nvarchar](128) NULL,
	[PayPalTransactionId] [nvarchar](128) NULL,
 CONSTRAINT [PK_MP_TransactionItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_PayPalTransactionItem2]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_PayPalTransactionItem2](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TransactionId] [int] NOT NULL,
	[Created] [datetime] NULL,
	[CurrencyId] [int] NULL,
	[FeeAmount] [float] NULL,
	[GrossAmount] [float] NULL,
	[NetAmount] [float] NULL,
	[TimeZone] [nvarchar](128) NULL,
	[Type] [nvarchar](128) NULL,
	[Status] [nvarchar](128) NULL,
	[PayPalTransactionId] [nvarchar](128) NULL,
 CONSTRAINT [PK_MP_TransactionItem2] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_PayPointOrder]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_PayPointOrder](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerMarketPlaceId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[CustomerMarketPlaceUpdatingHistoryRecordId] [int] NULL,
 CONSTRAINT [PK_MP_PayPointOrder] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_PayPointOrderItem]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[MP_PayPointOrderItem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderId] [int] NULL,
	[acquirer] [varchar](300) NULL,
	[amount] [decimal](18, 0) NULL,
	[auth_code] [varchar](300) NULL,
	[authorised] [varchar](300) NULL,
	[card_type] [varchar](300) NULL,
	[cid] [varchar](300) NULL,
	[classType] [varchar](300) NULL,
	[company_no] [varchar](300) NULL,
	[country] [varchar](300) NULL,
	[currency] [varchar](300) NULL,
	[cv2avs] [varchar](300) NULL,
	[date] [datetime] NULL,
	[deferred] [varchar](300) NULL,
	[emvValue] [varchar](300) NULL,
	[ExpiryDate] [datetime] NULL,
	[fraud_code] [varchar](300) NULL,
	[FraudScore] [varchar](300) NULL,
	[ip] [varchar](300) NULL,
	[lastfive] [varchar](300) NULL,
	[merchant_no] [varchar](300) NULL,
	[message] [varchar](300) NULL,
	[MessageType] [varchar](300) NULL,
	[mid] [varchar](300) NULL,
	[name] [varchar](300) NULL,
	[options] [varchar](300) NULL,
	[start_date] [datetime] NULL,
	[status] [varchar](300) NULL,
	[tid] [varchar](300) NULL,
	[trans_id] [varchar](300) NULL,
 CONSTRAINT [PK_MP_PayPointOrderItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[MP_PlayOrder]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_PlayOrder](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerMarketPlaceId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[CustomerMarketPlaceUpdatingHistoryRecordId] [int] NULL,
 CONSTRAINT [PK_MP_PlayOrder] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_PlayOrderItem]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_PlayOrderItem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderId] [int] NULL,
	[NativeOrderId] [nvarchar](300) NULL,
	[TotalCost] [numeric](18, 2) NULL,
	[CurrencyCode] [nvarchar](3) NULL,
	[PaymentDate] [datetime] NULL,
	[PurchaseDate] [datetime] NULL,
	[OrderStatus] [nvarchar](300) NULL,
 CONSTRAINT [PK_MP_PlayOrderItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_RtiTaxMonthEntries]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_RtiTaxMonthEntries](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RecordId] [int] NOT NULL,
	[DateStart] [datetime] NOT NULL,
	[DateEnd] [datetime] NOT NULL,
	[AmountPaid] [decimal](18, 2) NOT NULL,
	[AmountDue] [decimal](18, 2) NOT NULL,
	[CurrencyCode] [nvarchar](3) NOT NULL,
 CONSTRAINT [PK_RtiTaxMonthEntries] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_RtiTaxMonthRecords]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_RtiTaxMonthRecords](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerMarketPlaceId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[CustomerMarketPlaceUpdatingHistoryRecordId] [int] NOT NULL,
 CONSTRAINT [PK_RtiTaxMonthRecords] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_SageExpenditure]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_SageExpenditure](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RequestId] [int] NOT NULL,
	[SageId] [int] NOT NULL,
	[date] [datetime] NULL,
	[invoice_date] [datetime] NULL,
	[amount] [numeric](18, 2) NULL,
	[tax_amount] [numeric](18, 2) NULL,
	[gross_amount] [numeric](18, 2) NULL,
	[tax_percentage_rate] [numeric](18, 2) NULL,
	[TaxCodeId] [int] NULL,
	[tax_scheme_period_id] [int] NULL,
	[reference] [nvarchar](250) NULL,
	[ContactId] [int] NULL,
	[SourceId] [int] NULL,
	[DestinationId] [int] NULL,
	[PaymentMethodId] [int] NULL,
	[voided] [bit] NULL,
	[lock_version] [int] NULL,
 CONSTRAINT [PK_MP_SageExpenditure] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_SageIncome]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_SageIncome](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RequestId] [int] NOT NULL,
	[SageId] [int] NOT NULL,
	[date] [datetime] NULL,
	[invoice_date] [datetime] NULL,
	[amount] [numeric](18, 2) NULL,
	[tax_amount] [numeric](18, 2) NULL,
	[gross_amount] [numeric](18, 2) NULL,
	[tax_percentage_rate] [numeric](18, 2) NULL,
	[TaxCodeId] [int] NULL,
	[tax_scheme_period_id] [int] NULL,
	[reference] [nvarchar](250) NULL,
	[ContactId] [int] NULL,
	[SourceId] [int] NULL,
	[DestinationId] [int] NULL,
	[PaymentMethodId] [int] NULL,
	[voided] [bit] NULL,
	[lock_version] [int] NULL,
 CONSTRAINT [PK_MP_SageIncome] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_SagePaymentStatus]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_SagePaymentStatus](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SageId] [int] NOT NULL,
	[name] [nvarchar](250) NULL,
 CONSTRAINT [PK_MP_SagePaymentStatus] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_SagePurchaseInvoice]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_SagePurchaseInvoice](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RequestId] [int] NOT NULL,
	[SageId] [int] NOT NULL,
	[StatusId] [int] NULL,
	[due_date] [datetime] NULL,
	[date] [datetime] NULL,
	[void_reason] [nvarchar](250) NULL,
	[outstanding_amount] [numeric](18, 2) NULL,
	[total_net_amount] [numeric](18, 2) NULL,
	[total_tax_amount] [numeric](18, 2) NULL,
	[tax_scheme_period_id] [int] NULL,
	[ContactId] [int] NULL,
	[contact_name] [nvarchar](250) NULL,
	[main_address] [nvarchar](250) NULL,
	[delivery_address] [nvarchar](250) NULL,
	[delivery_address_same_as_main] [bit] NULL,
	[reference] [nvarchar](250) NULL,
	[notes] [nvarchar](250) NULL,
	[terms_and_conditions] [nvarchar](250) NULL,
	[lock_version] [int] NULL,
 CONSTRAINT [PK_MP_SagePurchaseInvoice] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_SagePurchaseInvoiceItem]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_SagePurchaseInvoiceItem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PurchaseInvoiceId] [int] NOT NULL,
	[SageId] [int] NOT NULL,
	[description] [nvarchar](250) NULL,
	[quantity] [numeric](18, 2) NULL,
	[unit_price] [numeric](18, 2) NULL,
	[net_amount] [numeric](18, 2) NULL,
	[tax_amount] [numeric](18, 2) NULL,
	[TaxCodeId] [int] NULL,
	[tax_rate_percentage] [numeric](18, 2) NULL,
	[unit_price_includes_tax] [bit] NULL,
	[LedgerAccountId] [int] NULL,
	[product_code] [nvarchar](250) NULL,
	[ProductId] [int] NULL,
	[ServiceId] [int] NULL,
	[lock_version] [int] NULL,
 CONSTRAINT [PK_MP_SagePurchaseInvoiceItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_SageRequest]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_SageRequest](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerMarketPlaceId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[CustomerMarketPlaceUpdatingHistoryRecordId] [int] NULL,
 CONSTRAINT [PK_MP_SageRequest] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_SageSalesInvoice]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_SageSalesInvoice](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RequestId] [int] NOT NULL,
	[SageId] [int] NOT NULL,
	[invoice_number] [nvarchar](250) NULL,
	[StatusId] [int] NULL,
	[due_date] [datetime] NULL,
	[date] [datetime] NULL,
	[void_reason] [nvarchar](250) NULL,
	[outstanding_amount] [numeric](18, 2) NULL,
	[total_net_amount] [numeric](18, 2) NULL,
	[total_tax_amount] [numeric](18, 2) NULL,
	[tax_scheme_period_id] [int] NULL,
	[carriage] [numeric](18, 2) NULL,
	[CarriageTaxCodeId] [int] NULL,
	[carriage_tax_rate_percentage] [numeric](18, 2) NULL,
	[ContactId] [int] NULL,
	[contact_name] [nvarchar](250) NULL,
	[main_address] [nvarchar](250) NULL,
	[delivery_address] [nvarchar](250) NULL,
	[delivery_address_same_as_main] [bit] NULL,
	[reference] [nvarchar](250) NULL,
	[notes] [nvarchar](250) NULL,
	[terms_and_conditions] [nvarchar](250) NULL,
	[lock_version] [int] NULL,
 CONSTRAINT [PK_MP_SageSalesInvoice] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_SageSalesInvoiceItem]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_SageSalesInvoiceItem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[InvoiceId] [int] NOT NULL,
	[SageId] [int] NOT NULL,
	[description] [nvarchar](250) NULL,
	[quantity] [numeric](18, 2) NULL,
	[unit_price] [numeric](18, 2) NULL,
	[net_amount] [numeric](18, 2) NULL,
	[tax_amount] [numeric](18, 2) NULL,
	[TaxCodeId] [int] NULL,
	[tax_rate_percentage] [numeric](18, 2) NULL,
	[unit_price_includes_tax] [bit] NULL,
	[LedgerAccountId] [int] NULL,
	[product_code] [nvarchar](250) NULL,
	[ProductId] [int] NULL,
	[ServiceId] [int] NULL,
	[lock_version] [int] NULL,
 CONSTRAINT [PK_MP_SageSalesInvoiceItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_ServiceLog]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_ServiceLog](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ServiceType] [nvarchar](500) NULL,
	[InsertDate] [datetime] NULL,
	[RequestData] [nvarchar](max) NULL,
	[ResponseData] [nvarchar](max) NULL,
	[CustomerId] [bigint] NULL,
	[DirectorId] [int] NULL,
 CONSTRAINT [PK_MP_ServiceLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_TeraPeakCategory]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_TeraPeakCategory](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](1000) NOT NULL,
	[FullName] [nvarchar](max) NOT NULL,
	[Level] [int] NOT NULL,
	[ParentCategoryID] [int] NOT NULL,
 CONSTRAINT [PK_MP_TeraPeakCategory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_TeraPeakCategoryStatistics]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_TeraPeakCategoryStatistics](
	[Id] [int] NOT NULL,
	[Listings] [int] NOT NULL,
	[Successful] [int] NOT NULL,
	[ItemsSold] [int] NOT NULL,
	[Revenue] [decimal](18, 4) NOT NULL,
	[SuccessRate] [decimal](18, 4) NOT NULL,
	[OrderItemId] [int] NOT NULL,
	[CategoryId] [int] NOT NULL,
 CONSTRAINT [PK_MP_TeraPeakCategoryStatistics] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_TeraPeakOrder]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_TeraPeakOrder](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerMarketPlaceId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[LastOrderItemEndDate] [datetime] NULL,
	[CustomerMarketPlaceUpdatingHistoryRecordId] [int] NULL,
 CONSTRAINT [PK_MP_TeraPeakOrder] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_TeraPeakOrderItem]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_TeraPeakOrderItem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TeraPeakOrderId] [int] NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
	[Revenue] [float] NULL,
	[Listings] [int] NULL,
	[Transactions] [int] NULL,
	[Successful] [int] NULL,
	[Bids] [int] NULL,
	[ItemsOffered] [int] NULL,
	[ItemsSold] [int] NULL,
	[AverageSellersPerDay] [int] NULL,
	[SuccessRate] [float] NULL,
	[RangeMarker] [int] NOT NULL,
 CONSTRAINT [PK_MP_TeraPeakOrderItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_ValueType]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_ValueType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[InternalId] [uniqueidentifier] NOT NULL,
	[Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_ValueType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_ValueTypeInternalId] UNIQUE NONCLUSTERED 
(
	[InternalId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_VatReturnEntries]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_VatReturnEntries](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RecordId] [int] NOT NULL,
	[NameId] [int] NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[CurrencyCode] [nvarchar](3) NOT NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_VatReturnEntryNames]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_VatReturnEntryNames](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](512) NOT NULL,
 CONSTRAINT [PK_VatReturnEntryNames] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_VatReturnRecords]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_VatReturnRecords](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerMarketPlaceId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[CustomerMarketPlaceUpdatingHistoryRecordId] [int] NOT NULL,
	[Period] [nvarchar](256) NOT NULL,
	[DateFrom] [datetime] NOT NULL,
	[DateTo] [datetime] NOT NULL,
	[DateDue] [datetime] NOT NULL,
	[BusinessId] [int] NOT NULL,
	[RegistrationNo] [bigint] NOT NULL,
 CONSTRAINT [PK_VatReturnRecords] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_VolusionOrder]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_VolusionOrder](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerMarketPlaceId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[CustomerMarketPlaceUpdatingHistoryRecordId] [int] NULL,
 CONSTRAINT [PK_MP_VolusionOrder] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_VolusionOrderItem]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_VolusionOrderItem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderId] [int] NULL,
	[NativeOrderId] [nvarchar](300) NULL,
	[TotalCost] [numeric](18, 2) NULL,
	[CurrencyCode] [nvarchar](3) NULL,
	[PaymentDate] [datetime] NULL,
	[PurchaseDate] [datetime] NULL,
	[OrderStatus] [nvarchar](300) NULL,
 CONSTRAINT [PK_MP_VolusionOrderItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_WhiteList]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_WhiteList](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](500) NOT NULL,
	[MarketPlaceTypeGuid] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_MP_WhiteList] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_YodleeOrder]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_YodleeOrder](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerMarketPlaceId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[CustomerMarketPlaceUpdatingHistoryRecordId] [int] NULL,
 CONSTRAINT [PK_MP_YodleeOrder] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_YodleeOrderItem]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_YodleeOrderItem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderId] [int] NOT NULL,
	[isSeidFromDataSource] [int] NULL,
	[isSeidFromDataSourceSpecified] [bit] NULL,
	[isSeidMod] [int] NULL,
	[isSeidModSpecified] [bit] NULL,
	[acctTypeId] [int] NULL,
	[acctTypeIdSpecified] [bit] NULL,
	[acctType] [nvarchar](300) NULL,
	[localizedAcctType] [nvarchar](300) NULL,
	[srcElementId] [nvarchar](300) NULL,
	[individualInformationId] [int] NULL,
	[individualInformationIdSpecified] [bit] NULL,
	[bankAccountId] [int] NULL,
	[bankAccountIdSpecified] [bit] NULL,
	[customName] [nvarchar](300) NULL,
	[customDescription] [nvarchar](300) NULL,
	[isDeleted] [int] NULL,
	[isDeletedSpecified] [bit] NULL,
	[lastUpdated] [int] NULL,
	[lastUpdatedSpecified] [bit] NULL,
	[hasDetails] [int] NULL,
	[hasDetailsSpecified] [bit] NULL,
	[interestRate] [float] NULL,
	[interestRateSpecified] [bit] NULL,
	[accountNumber] [nvarchar](300) NULL,
	[link] [nvarchar](300) NULL,
	[accountHolder] [nvarchar](300) NULL,
	[tranListToDate] [datetime] NULL,
	[tranListFromDate] [datetime] NULL,
	[availableBalance] [float] NULL,
	[availableBalanceCurrency] [nvarchar](3) NULL,
	[currentBalance] [float] NULL,
	[currentBalanceCurrency] [nvarchar](3) NULL,
	[interestEarnedYtd] [float] NULL,
	[interestEarnedYtdCurrency] [nvarchar](3) NULL,
	[prevYrInterest] [float] NULL,
	[prevYrInterestCurrency] [nvarchar](3) NULL,
	[overdraftProtection] [float] NULL,
	[overdraftProtectionCurrency] [nvarchar](3) NULL,
	[term] [nvarchar](300) NULL,
	[accountName] [nvarchar](300) NULL,
	[annualPercentYield] [float] NULL,
	[annualPercentYieldSpecified] [bit] NULL,
	[routingNumber] [nvarchar](300) NULL,
	[maturityDate] [datetime] NULL,
	[asOfDate] [datetime] NULL,
	[accountNicknameAtSrcSite] [nvarchar](300) NULL,
	[isPaperlessStmtOn] [int] NULL,
	[isPaperlessStmtOnSpecified] [bit] NULL,
	[siteAccountStatusSpecified] [bit] NULL,
	[created] [int] NULL,
	[createdSpecified] [bit] NULL,
	[nomineeName] [nvarchar](300) NULL,
	[secondaryAccountHolderName] [nvarchar](300) NULL,
	[accountOpenDate] [datetime] NULL,
	[accountCloseDate] [datetime] NULL,
	[maturityAmount] [float] NULL,
	[maturityAmountCurrency] [nvarchar](3) NULL,
	[taxesWithheldYtd] [float] NULL,
	[taxesWithheldYtdCurrency] [nvarchar](3) NULL,
	[taxesPaidYtd] [float] NULL,
	[taxesPaidYtdCurrency] [nvarchar](3) NULL,
	[budgetBalance] [float] NULL,
	[budgetBalanceCurrency] [nvarchar](3) NULL,
	[straightBalance] [float] NULL,
	[straightBalanceCurrency] [nvarchar](3) NULL,
	[accountClassificationSpecified] [bit] NULL,
	[siteAccountStatus] [nvarchar](50) NULL,
	[accountClassification] [nvarchar](50) NULL,
	[itemAccountId] [bigint] NULL,
 CONSTRAINT [PK_MP_YodleeOrderItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_YodleeOrderItemBankTransaction]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_YodleeOrderItemBankTransaction](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderItemId] [int] NOT NULL,
	[isSeidFromDataSource] [int] NULL,
	[isSeidFromDataSourceSpecified] [bit] NULL,
	[isSeidMod] [int] NULL,
	[isSeidModSpecified] [bit] NULL,
	[srcElementId] [nvarchar](300) NULL,
	[transactionTypeId] [int] NULL,
	[transactionTypeIdSpecified] [bit] NULL,
	[transactionType] [nvarchar](300) NULL,
	[localizedTransactionType] [nvarchar](300) NULL,
	[transactionStatusId] [int] NULL,
	[transactionStatusIdSpecified] [bit] NULL,
	[transactionStatus] [nvarchar](300) NULL,
	[localizedTransactionStatus] [nvarchar](300) NULL,
	[transactionBaseTypeId] [int] NULL,
	[transactionBaseTypeIdSpecified] [bit] NULL,
	[transactionBaseType] [nvarchar](300) NULL,
	[localizedTransactionBaseType] [nvarchar](300) NULL,
	[categoryId] [int] NULL,
	[categoryIdSpecified] [bit] NULL,
	[bankTransactionId] [int] NULL,
	[bankTransactionIdSpecified] [bit] NULL,
	[bankAccountId] [int] NULL,
	[bankAccountIdSpecified] [bit] NULL,
	[bankStatementId] [int] NULL,
	[bankStatementIdSpecified] [bit] NULL,
	[isDeleted] [int] NULL,
	[isDeletedSpecified] [bit] NULL,
	[lastUpdated] [int] NULL,
	[lastUpdatedSpecified] [bit] NULL,
	[hasDetails] [int] NULL,
	[hasDetailsSpecified] [bit] NULL,
	[transactionId] [nvarchar](300) NULL,
	[transactionCategoryId] [nvarchar](300) NULL,
	[siteCategoryType] [nvarchar](300) NULL,
	[siteCategory] [nvarchar](300) NULL,
	[classUpdationSource] [nvarchar](300) NULL,
	[lastCategorised] [nvarchar](300) NULL,
	[transactionDate] [datetime] NULL,
	[isReimbursable] [int] NULL,
	[isReimbursableSpecified] [bit] NULL,
	[mcCode] [nvarchar](300) NULL,
	[prevLastCategorised] [int] NULL,
	[prevLastCategorisedSpecified] [bit] NULL,
	[naicsCode] [nvarchar](300) NULL,
	[runningBalance] [float] NULL,
	[runningBalanceCurrency] [nvarchar](3) NULL,
	[userDescription] [nvarchar](300) NULL,
	[customCategoryId] [int] NULL,
	[customCategoryIdSpecified] [bit] NULL,
	[memo] [nvarchar](300) NULL,
	[parentId] [int] NULL,
	[parentIdSpecified] [bit] NULL,
	[isOlbUserDesc] [int] NULL,
	[isOlbUserDescSpecified] [bit] NULL,
	[categorisationSourceId] [nvarchar](300) NULL,
	[plainTextDescription] [nvarchar](300) NULL,
	[splitType] [nvarchar](300) NULL,
	[categoryLevelId] [int] NULL,
	[categoryLevelIdSpecified] [bit] NULL,
	[calcRunningBalance] [float] NULL,
	[calcRunningBalanceCurrency] [nvarchar](3) NULL,
	[category] [nvarchar](300) NULL,
	[link] [nvarchar](300) NULL,
	[postDate] [datetime] NULL,
	[prevTransactionCategoryId] [int] NULL,
	[prevTransactionCategoryIdSpecified] [bit] NULL,
	[isBusinessExpense] [int] NULL,
	[isBusinessExpenseSpecified] [bit] NULL,
	[descriptionViewPref] [int] NULL,
	[descriptionViewPrefSpecified] [bit] NULL,
	[prevCategorisationSourceId] [int] NULL,
	[prevCategorisationSourceIdSpecified] [bit] NULL,
	[transactionAmount] [float] NULL,
	[transactionAmountCurrency] [nvarchar](3) NULL,
	[transactionPostingOrder] [int] NULL,
	[transactionPostingOrderSpecified] [bit] NULL,
	[checkNumber] [nvarchar](300) NULL,
	[description] [nvarchar](300) NULL,
	[isTaxDeductible] [int] NULL,
	[isTaxDeductibleSpecified] [bit] NULL,
	[isMedicalExpense] [int] NULL,
	[isMedicalExpenseSpecified] [bit] NULL,
	[categorizationKeyword] [nvarchar](300) NULL,
	[sourceTransactionType] [nvarchar](300) NULL,
 CONSTRAINT [PK_MP_YodleeOrderItemBankTransaction] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_YodleeSearchWords]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_YodleeSearchWords](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SearchWords] [nvarchar](300) NOT NULL,
 CONSTRAINT [PK_MP_YodleeSearchWords] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MP_YodleeTransactionCategories]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_YodleeTransactionCategories](
	[CategoryId] [nvarchar](300) NOT NULL,
	[Name] [nvarchar](300) NOT NULL,
	[Type] [nvarchar](300) NOT NULL,
 CONSTRAINT [PK_MP_YodleeTransactionCategories] PRIMARY KEY CLUSTERED 
(
	[CategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[PacnetAgentConfigs]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[PacnetAgentConfigs](
	[CfgKey] [varchar](100) NOT NULL,
	[CfgValue] [varchar](100) NULL,
 CONSTRAINT [PK_PacnetAgentConfigs] PRIMARY KEY CLUSTERED 
(
	[CfgKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[PacNetBalance]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PacNetBalance](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Date] [datetime] NULL,
	[Amount] [float] NULL,
	[Fees] [float] NULL,
	[CurrentBalance] [float] NULL,
	[IsCredit] [bit] NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[PacNetManualBalance]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[PacNetManualBalance](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Username] [varchar](100) NULL,
	[Amount] [int] NOT NULL,
	[Date] [datetime] NULL,
	[Enabled] [bit] NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[PacnetPaypointServiceLog]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PacnetPaypointServiceLog](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CustomerId] [bigint] NULL,
	[InsertDate] [datetime] NULL,
	[RequestType] [nvarchar](max) NULL,
	[Status] [nvarchar](50) NULL,
	[ErrorMessage] [nvarchar](max) NULL,
 CONSTRAINT [PK_PacnetServiceLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[PaymentRollover]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[PaymentRollover](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LoanScheduleId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[CreatorName] [nvarchar](256) NULL,
	[Payment] [decimal](18, 0) NOT NULL,
	[PaymentDueDate] [datetime] NULL,
	[PaymentNewDate] [datetime] NULL,
	[ExpiryDate] [datetime] NULL,
	[CustomerConfirmationDate] [datetime] NULL,
	[PaidPaymentAmount] [decimal](18, 0) NULL,
	[Status] [varchar](50) NULL,
	[MounthCount] [int] NOT NULL,
 CONSTRAINT [PK_MP_PaymentRollover] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[PayPointBalance]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[PayPointBalance](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[acquirer] [varchar](300) NULL,
	[amount] [decimal](18, 2) NULL,
	[auth_code] [varchar](300) NULL,
	[authorised] [varchar](300) NULL,
	[card_type] [varchar](300) NULL,
	[cid] [varchar](300) NULL,
	[_class] [varchar](300) NULL,
	[company_no] [varchar](300) NULL,
	[country] [varchar](300) NULL,
	[currency] [varchar](300) NULL,
	[cv2avs] [varchar](300) NULL,
	[date] [datetime] NULL,
	[deferred] [varchar](300) NULL,
	[emvValue] [varchar](300) NULL,
	[fraud_code] [varchar](300) NULL,
	[FraudScore] [varchar](300) NULL,
	[ip] [varchar](300) NULL,
	[lastfive] [varchar](300) NULL,
	[merchant_no] [varchar](300) NULL,
	[message] [varchar](300) NULL,
	[MessageType] [varchar](300) NULL,
	[mid] [varchar](300) NULL,
	[name] [varchar](300) NULL,
	[options] [varchar](300) NULL,
	[status] [varchar](300) NULL,
	[tid] [varchar](300) NULL,
	[trans_id] [varchar](300) NULL,
 CONSTRAINT [PK_PayPointBalance] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[PayPointCard]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PayPointCard](
	[Id] [int] NOT NULL,
	[CustomerId] [int] NOT NULL,
	[DateAdded] [datetime] NOT NULL,
	[TransactionId] [nvarchar](250) NULL,
	[CardNo] [nvarchar](50) NULL,
	[ExpireDate] [datetime] NULL,
	[ExpireDateString] [nvarchar](50) NULL,
	[CardHolder] [nvarchar](150) NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[PersonalInfoHistory]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PersonalInfoHistory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerId] [int] NOT NULL,
	[FieldName] [nvarchar](50) NULL,
	[OldValue] [nvarchar](100) NULL,
	[NewValue] [nvarchar](100) NULL,
	[DateModifed] [datetime] NULL,
	[AddressId] [nvarchar](100) NULL,
 CONSTRAINT [PK_PersonalInfoEditHistory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[PostcodeServiceLog]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PostcodeServiceLog](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CustomerId] [bigint] NULL,
	[InsertDate] [datetime] NULL,
	[RequestType] [nvarchar](200) NULL,
	[RequestData] [nvarchar](max) NULL,
	[ResponseData] [nvarchar](max) NULL,
	[Status] [nvarchar](200) NULL,
	[ErrorMessage] [nvarchar](max) NULL,
 CONSTRAINT [PK_PostcodeServiceLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ReportArgumentNames]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReportArgumentNames](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](64) NOT NULL,
 CONSTRAINT [PK_ReportArgumentNames] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ReportArguments]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReportArguments](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ReportArgumentNameId] [int] NOT NULL,
	[ReportId] [int] NOT NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ReportScheduler]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReportScheduler](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Type] [nvarchar](200) NULL,
	[Title] [nvarchar](200) NULL,
	[StoredProcedure] [nvarchar](200) NULL,
	[IsDaily] [bit] NULL,
	[IsWeekly] [bit] NULL,
	[IsMonthly] [bit] NULL,
	[Header] [nvarchar](300) NULL,
	[Fields] [nvarchar](300) NULL,
	[ToEmail] [nvarchar](300) NULL,
	[IsMonthToDate] [bit] NOT NULL,
 CONSTRAINT [PK_ReportScheduler] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ReportsUsersMap]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReportsUsersMap](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NULL,
	[ReportID] [int] NULL,
 CONSTRAINT [PK_ReportsUsersMap] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ReportUsers]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ReportUsers](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserName] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Password] [varbinary](30) NULL,
	[Salt] [varbinary](30) NULL,
	[IsAdmin] [bit] NOT NULL,
 CONSTRAINT [PK_ReportUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ScoringModel]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ScoringModel](
	[Id] [int] NOT NULL,
	[Guid] [nvarchar](1024) NULL,
	[DisplayName] [nvarchar](1024) NULL,
	[UserId] [int] NULL,
	[ModelTypeName] [nvarchar](250) NULL,
	[CreationDate] [datetime] NULL,
	[IsDeleted] [int] NULL,
	[TerminationDate] [datetime] NULL,
	[Description] [nvarchar](1024) NULL,
	[Cutoffpoint] [decimal](5, 5) NULL,
	[AllowWeightsEdit] [int] NULL,
	[AllowSaveResults] [int] NULL,
	[PmmlFile] [nvarchar](max) NULL,
	[SignedDocument] [nvarchar](max) NULL,
	[SignedDocumentDelete] [nvarchar](max) NULL,
 CONSTRAINT [PK_Models] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Security_AccountLog]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Security_AccountLog](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EventDate] [datetime] NULL,
	[EventType] [int] NULL,
	[UserId] [int] NULL,
	[Data] [nvarchar](2048) NULL,
 CONSTRAINT [PK_Security_AccountLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Security_Application]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Security_Application](
	[ApplicationId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
	[Description] [nvarchar](256) NULL,
	[ApplicationType] [tinyint] NULL,
 CONSTRAINT [PK_Security_Application] PRIMARY KEY CLUSTERED 
(
	[ApplicationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Security_Branch]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Security_Branch](
	[BranchId] [int] NOT NULL,
	[Name] [nvarchar](255) NULL,
	[Description] [ntext] NULL,
	[Identifier] [nvarchar](255) NULL,
	[CreationDate]  AS (getdate()),
	[ModifyDate]  AS (getdate()),
 CONSTRAINT [PK_SECURITY_BRANCH] PRIMARY KEY CLUSTERED 
(
	[BranchId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Security_log4net]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Security_log4net](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Message] [nvarchar](max) NULL,
	[EventDate] [datetime] NULL,
	[EventJournal] [int] NULL,
	[EventType] [int] NULL,
	[UserName] [nvarchar](1024) NULL,
	[Level] [nvarchar](50) NULL,
	[Thread] [nvarchar](16) NULL,
	[AppID] [nvarchar](20) NULL,
	[Logger] [nvarchar](256) NULL,
	[Exception] [nvarchar](max) NULL,
 CONSTRAINT [PK_Security_log4net] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Security_Permission]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Security_Permission](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](250) NOT NULL,
	[Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_Permission] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Security_Question]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Security_Question](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [varchar](200) NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Security_Role]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Security_Role](
	[RoleId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](255) NULL,
 CONSTRAINT [PK_Security_Role] PRIMARY KEY CLUSTERED 
(
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Security_RoleAppRel]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Security_RoleAppRel](
	[RoleId] [int] NOT NULL,
	[AppId] [int] NOT NULL,
 CONSTRAINT [PK_Security_RoleAppRel] PRIMARY KEY CLUSTERED 
(
	[RoleId] ASC,
	[AppId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Security_RolePermissionRel]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Security_RolePermissionRel](
	[RoleId] [int] NOT NULL,
	[PermissionId] [int] NOT NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Security_Session]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Security_Session](
	[UserId] [int] NOT NULL,
	[AppId] [int] NOT NULL,
	[State] [tinyint] NOT NULL,
	[SessionId] [nvarchar](32) NOT NULL,
	[CreationDate] [datetime] NOT NULL,
	[LastAccessTime] [datetime] NOT NULL,
	[HostAddress] [nvarchar](max) NULL,
 CONSTRAINT [PK_Security_Session] PRIMARY KEY CLUSTERED 
(
	[SessionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Security_User]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Security_User](
	[UserId] [int] IDENTITY(1,1) NOT NULL,
	[UserName] [nvarchar](250) NOT NULL,
	[FullName] [nvarchar](250) NOT NULL,
	[Password] [nvarchar](200) NULL,
	[CreationDate] [datetime] NOT NULL,
	[IsDeleted] [int] NOT NULL,
	[EMail] [nvarchar](255) NULL,
	[CreateUserId] [int] NULL,
	[DeletionDate] [datetime] NULL,
	[DeleteUserId] [int] NULL,
	[BranchId] [int] NOT NULL,
	[PassSetTime] [datetime] NULL,
	[LoginFailedCount] [int] NULL,
	[DisableDate] [datetime] NULL,
	[LastBadLogin] [datetime] NULL,
	[PassExpPeriod] [bigint] NULL,
	[ForcePassChange] [int] NULL,
	[DisablePassChange] [int] NULL,
	[DeleteId] [int] NULL,
	[CertificateThumbprint] [nvarchar](40) NULL,
	[DomainUserName] [nvarchar](250) NULL,
	[SecurityQuestion1Id] [bigint] NULL,
	[SecurityAnswer1] [varchar](200) NULL,
	[IsPasswordRestored] [bit] NULL,
 CONSTRAINT [PK_Security_User] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_SECURITY_USER] UNIQUE NONCLUSTERED 
(
	[UserName] ASC,
	[DeleteId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Security_UserRoleRelation]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Security_UserRoleRelation](
	[UserId] [int] NOT NULL,
	[RoleId] [int] NOT NULL,
 CONSTRAINT [PK_Security_UserRoleRelation] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ServiceRegistration]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ServiceRegistration](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[key] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_ServiceRegistration] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

/****** Object:  Table [dbo].[SingleRunApplication]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SingleRunApplication](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ApplicationExecutionTypeId] [bigint] NOT NULL,
 CONSTRAINT [PK_SingleRunApplication] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SiteAnalytics]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SiteAnalytics](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Date] [datetime] NULL,
	[SiteAnalyticsCode] [int] NULL,
	[SiteAnalyticsValue] [int] NULL,
 CONSTRAINT [PK_SiteAnalytics] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SiteAnalyticsCodes]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SiteAnalyticsCodes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](300) NOT NULL,
	[Description] [nvarchar](300) NULL,
 CONSTRAINT [PK_SiteAnalyticsCodes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Strategy_CalendarRelation]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Strategy_CalendarRelation](
	[StrategyId] [int] NOT NULL,
	[CalendarId] [int] NOT NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Strategy_CustomerUpdateHistory]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Strategy_CustomerUpdateHistory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerId] [int] NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Strategy_Embededrel]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Strategy_Embededrel](
	[StrategyId] [int] NULL,
	[EmbStrategyId] [int] NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Strategy_MarketPlaceUpdateHistory]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Strategy_MarketPlaceUpdateHistory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[MarketPlaceId] [int] NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Strategy_Node]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Strategy_Node](
	[NodeId] [int] IDENTITY(1,1) NOT NULL,
	[GroupId] [tinyint] NOT NULL,
	[Name] [nvarchar](512) NOT NULL,
	[DisplayName] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[ExecutionDuration] [bigint] NULL,
	[Icon] [image] NOT NULL,
	[IsDeleted] [int] NOT NULL,
	[ApplicationId] [int] NULL,
	[IsHardReaction] [bit] NOT NULL,
	[ContainsPrint] [int] NULL,
	[CustomUrl] [nvarchar](4000) NULL,
	[StartDate] [datetime] NULL,
	[Guid] [nvarchar](256) NULL,
	[CreatorUserId] [int] NOT NULL,
	[DeleterUserId] [int] NULL,
	[NodeComment] [nvarchar](1024) NULL,
	[TerminationDate] [datetime] NULL,
	[NDX] [image] NOT NULL,
	[SignedDocument] [ntext] NULL,
	[SignedDocumentDelete] [ntext] NULL,
 CONSTRAINT [PK_Strategy_Node] PRIMARY KEY CLUSTERED 
(
	[NodeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Strategy_NodeGroup]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Strategy_NodeGroup](
	[NodeGroupId] [tinyint] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Strategy_NodeGroup] PRIMARY KEY CLUSTERED 
(
	[NodeGroupId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Strategy_NodeStrategyRel]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Strategy_NodeStrategyRel](
	[StrategyId] [int] NOT NULL,
	[NodeId] [int] NOT NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Strategy_ParameterType]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Strategy_ParameterType](
	[ParamTypeId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](500) NULL,
 CONSTRAINT [PK_Strategy_ParameterType] PRIMARY KEY CLUSTERED 
(
	[ParamTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_Strategy_ParameterType] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Strategy_PublicName]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Strategy_PublicName](
	[PUBLICNAMEID] [int] IDENTITY(1,1) NOT NULL,
	[NAME] [nvarchar](255) NOT NULL,
	[ISSTOPPED] [int] NULL,
	[IsDeleted] [int] NOT NULL,
	[DeleterUserId] [int] NULL,
	[TerminationDate] [datetime] NULL,
	[SignedDocumentDelete] [ntext] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Strategy_PublicRel]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Strategy_PublicRel](
	[PUBLICID] [int] NULL,
	[STRATEGYID] [int] NULL,
	[PERCENT] [float] NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Strategy_PublicSign]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Strategy_PublicSign](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[StrategyPublicId] [int] NOT NULL,
	[StrategyId] [int] NOT NULL,
	[CreationDate] [datetime] NOT NULL,
	[Data] [ntext] NOT NULL,
	[Action] [nvarchar](7) NOT NULL,
	[SignedDocument] [ntext] NOT NULL,
	[UserId] [int] NOT NULL,
	[AllData] [ntext] NOT NULL,
 CONSTRAINT [PK_Strategy_PublicSign] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Strategy_Schedule]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Strategy_Schedule](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
	[StrategyId] [int] NOT NULL,
	[ScheduleType] [int] NOT NULL,
	[ExecutionType] [int] NOT NULL,
	[ScheduleMask] [nvarchar](512) NOT NULL,
	[NextRun] [datetime] NOT NULL,
	[CreatorUserId] [int] NULL,
	[IsPaused] [bit] NULL,
 CONSTRAINT [PK_Strategy_Schedule] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

/****** Object:  Table [dbo].[Strategy_Schemas]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Strategy_Schemas](
	[StrategyId] [int] NOT NULL,
	[BinaryData] [varbinary](max) NULL,
 CONSTRAINT [PK_Strategy_Schemas] PRIMARY KEY CLUSTERED 
(
	[StrategyId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Strategy_Strategy]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Strategy_Strategy](
	[StrategyId] [int] IDENTITY(30,1) NOT NULL,
	[CurrentVersionId] [int] NULL,
	[Name] [nvarchar](400) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[IsEmbeddingAllowed] [bit] NOT NULL,
	[XML] [ntext] NOT NULL,
	[IsDeleted] [int] NOT NULL,
	[UserId] [int] NULL,
	[AuthorId] [int] NOT NULL,
	[State] [tinyint] NOT NULL,
	[SubState] [tinyint] NOT NULL,
	[CreationDate] [datetime] NOT NULL,
	[Icon] [image] NULL,
	[ExecutionDuration] [bigint] NULL,
	[LastUpdateDate] [datetime] NULL,
	[StrategyType] [nvarchar](255) NOT NULL,
	[DisplayName] [nvarchar](350) NULL,
	[TermDate] [datetime] NULL,
	[SignedDocument] [ntext] NULL,
	[IsMigrationSupported] [bit] NULL,
	[SignedDocumentDelete] [ntext] NULL,
	[InDbFormat] [varbinary](max) NULL,
 CONSTRAINT [PK_Strategy_Strategy] PRIMARY KEY CLUSTERED 
(
	[StrategyId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_Strategy_Strategy] UNIQUE NONCLUSTERED 
(
	[Name] ASC,
	[IsDeleted] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Strategy_StrategyParameter]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Strategy_StrategyParameter](
	[StratParamId] [int] IDENTITY(1,1) NOT NULL,
	[TypeId] [int] NOT NULL,
	[OwnerId] [int] NOT NULL,
	[Name] [nvarchar](128) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[IsInput] [bit] NOT NULL,
	[IsOutput] [bit] NOT NULL,
 CONSTRAINT [PK_Strategy_StrategyParameter] PRIMARY KEY CLUSTERED 
(
	[StratParamId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_Strategy_StrategyParameter] UNIQUE NONCLUSTERED 
(
	[Name] ASC,
	[OwnerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[StrategyAccountRel]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StrategyAccountRel](
	[AccountId] [int] NOT NULL,
	[StrategyId] [int] NOT NULL,
 CONSTRAINT [PK_StrategyAccountRel] PRIMARY KEY CLUSTERED 
(
	[AccountId] ASC,
	[StrategyId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


/****** Object:  Table [dbo].[SupportAgentConfigs]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[SupportAgentConfigs](
	[CfgKey] [varchar](100) NOT NULL,
	[CfgValue] [varchar](100) NULL,
 CONSTRAINT [PK_SupportAgentConfigs] PRIMARY KEY CLUSTERED 
(
	[CfgKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[SV_ReportingInfo]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SV_ReportingInfo](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[PathDir] [nvarchar](max) NOT NULL,
	[UserId] [int] NOT NULL,
	[CreationDate] [datetime] NOT NULL,
	[CreatingState] [int] NOT NULL,
	[ErrorMessage] [nvarchar](1000) NULL,
	[TimeCreationCube] [bigint] NOT NULL,
 CONSTRAINT [PK_DirectoryInfo] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SystemCalendar_BaseRelation]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SystemCalendar_BaseRelation](
	[CalendarId] [int] NOT NULL,
	[BaseCalendarId] [int] NOT NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SystemCalendar_Calendar]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SystemCalendar_Calendar](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IsDeleted] [int] NULL,
	[IsBase] [bit] NOT NULL,
	[DisplayName] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](255) NULL,
	[UserId] [int] NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[TerminationDate] [datetime] NULL,
	[SignedDocument] [nvarchar](max) NULL,
	[SignedDocumentDelete] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[IsDeleted] ASC,
	[DisplayName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SystemCalendar_Day]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SystemCalendar_Day](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DayOfWeek] [nvarchar](255) NOT NULL,
	[IsWorkDay] [bit] NOT NULL,
	[BeginsAt] [datetime] NULL,
	[Duration] [int] NULL,
	[LunchBeginsAt] [datetime] NULL,
	[LunchDuration] [int] NULL,
	[HostCalendarId] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SystemCalendar_Entry]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SystemCalendar_Entry](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EntryDate] [datetime] NOT NULL,
	[Description] [nvarchar](255) NOT NULL,
	[EntryMode] [nvarchar](255) NOT NULL,
	[HostCalendarId] [int] NULL,
	[HostEntryId] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

/****** Object:  Table [dbo].[TestCustomer]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TestCustomer](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Pattern] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_TestCustomer] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[UnderwriterRecentCustomers]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UnderwriterRecentCustomers](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserName] [nvarchar](500) NOT NULL,
	[CustomerId] [int] NOT NULL,
 CONSTRAINT [PK_UnderwriterRecentCustomers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[YodleeAccounts]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[YodleeAccounts](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerId] [int] NULL,
	[BankId] [int] NULL,
	[Username] [nvarchar](300) NULL,
	[Password] [nvarchar](300) NULL,
	[CreationDate] [datetime] NULL,
 CONSTRAINT [PK_YodleeAccounts] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[YodleeBanks]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[YodleeBanks](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](300) NULL,
	[ContentServiceId] [int] NOT NULL,
	[ParentBank] [nvarchar](100) NOT NULL,
	[Active] [bit] NOT NULL,
	[Image] [bit] NOT NULL,
 CONSTRAINT [PK_YodleeBanks] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[yuly_CustomerLoansSummary]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[yuly_CustomerLoansSummary](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerId] [int] NULL,
	[TotalBalance] [int] NULL,
	[NumOfLoans] [int] NULL
) ON [PRIMARY]

GO

/****** Object:  UserDefinedFunction [dbo].[GetLatePaymentsGrouped]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetLatePaymentsGrouped]
(	
 	
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT res.LoanId, SUM(res.AmountDue) LatePaymentsAmount FROM 
	(SELECT LoanId, AmountDue 
		FROM LoanSchedule
			Where Status = 'Late') as res
	Group by LoanId
)

GO
/****** Object:  UserDefinedFunction [dbo].[GetLoanLatePaymentsGrouped]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetLoanLatePaymentsGrouped]
(	
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT dbo.Loan.RequestCashId, SUM(lp.LatePaymentsAmount) LatePaymentsAmount FROM 
	dbo.Loan LEFT OUTER JOIN GetLatePaymentsGrouped() as lp on lp.LoanId = dbo.Loan.Id
	Group by dbo.Loan.RequestCashId
)

GO
/****** Object:  View [dbo].[MinLoanSchedule]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create view [dbo].[MinLoanSchedule]
as 
select MIN(ls.date) as lsdate, l.Id
FROM LoanSchedule ls 
  LEFT OUTER JOIN dbo.Loan AS l ON l.Id = ls.LoanId
where ls.Status = 'Late'
GROUP BY l.Id

GO
/****** Object:  View [dbo].[vw_NotClose]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create VIEW [dbo].[vw_NotClose]
AS
SELECT l.Id AS loanID
, l.CustomerId
, l.Date AS StartDate
, CASE WHEN cs.Name != 'Default' THEN ISNULL(l.DateClosed, 0)
	ELSE max(csh.Timestamp) END AS DateClose
, ISNULL(l.MaxDelinquencyDays, 0) as MaxDelinquencyDays
, l.RepaymentsNum AS RepaymentPeriod
, l.Balance AS CurrentBalance
, c.Gender
, c.FirstName
, c.MiddleInitial
, c.Surname
, c.RefNumber
, ca.Line1
, ca.Line2
, ca.Line3
, ca.Town
, ca.County
, ca.Postcode
, c.DateOfBirth
, ld.lsdate as  MinLSDate
, LoanAmount.am AS LoanAmount
, LoanAmount.SceduledRepayments AS SceduledRepayments
, c.TypeOfBusiness as CompanyType
, c.LimitedRefNum
, c.NonLimitedRefNum
, c.CreditResult as CustomerState
, c.SortCode
, l.IsDefaulted
, lo.CaisAccountStatus
, convert(INT, cs.IsEnabled) AS CustomerStatusIsEnabled
, c.MartialStatus
, lo.ManualCaisFlag
FROM         
(
  SELECT 
	  SUM(l.LoanAmount) AS am	  
     , l.Id
     , COUNT(ls.Id) as SceduledRepayments
 FROM dbo.LoanSchedule AS ls 
  LEFT OUTER JOIN dbo.Loan AS l ON l.Id = ls.LoanId
    GROUP BY l.Id
 ) AS LoanAmount

 LEFT OUTER JOIN dbo.Loan AS l ON l.Id = LoanAmount.Id 
 LEFT OUTER JOIN MinLoanSchedule as ld ON ld.Id = LoanAmount.Id 
 LEFT OUTER JOIN dbo.Customer AS c ON c.Id = l.CustomerId 
LEFT OUTER JOIN dbo.CustomerAddress AS ca ON ca.customerId = c.Id
LEFT OUTER JOIN dbo.LoanOptions AS lo ON lo.LoanId = l.Id
LEFT JOIN CustomerStatuses AS cs ON cs.Id = c.CollectionStatus
LEFT OUTER JOIN dbo.LoanTransaction AS lt ON lt.LoanId = l.Id AND lt.Status='Done' AND lt.Type = 'PaypointTransaction'
LEFT OUTER JOIN CustomerStatusHistory AS csh ON csh.CustomerId = c.Id
WHERE c.IsTest <> 1 and 
 (
 ((c.TypeOfBusiness = 'PShip3P' OR c.TypeOfBusiness = 'SoleTrader') AND ca.addressType = 5) OR 
 ((c.TypeOfBusiness = 'Limited' OR c.TypeOfBusiness = 'PShip' OR c.TypeOfBusiness = 'LLP')and  (ca.addressType = 3)) OR 
  (c.TypeOfBusiness = 'Entrepreneur' and  (ca.addressType = 1))
 )
 GROUP BY l.Id, l.CustomerId, l.[Date], l.DateClosed, l.MaxDelinquencyDays, l.RepaymentsNum, l.Balance, c.Gender, c.FirstName, c.MiddleInitial, c.Surname, c.RefNumber, ca.Line1, ca.Line2, ca.Line3, ca.Town, ca.County
 , ca.Postcode, c.DateOfBirth, ld.lsdate, LoanAmount.am, LoanAmount.SceduledRepayments, c.TypeOfBusiness, c.LimitedRefNum, c.NonLimitedRefNum, c.CreditResult
 , c.SortCode, l.IsDefaulted, lo.CaisAccountStatus, cs.IsEnabled, c.CollectionStatus, c.MartialStatus, lo.ManualCaisFlag, cs.Name

GO
/****** Object:  View [dbo].[vw_collection]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vw_collection]
AS
select distinct (CustomerID), ISNULL(MAX (MaxDelinquencyDays), 0) as MaxDelinquencyDays,
ISNULL(MAX (DateClose),0) as DateClosed
from vw_NotClose
group by CustomerID

GO
/****** Object:  View [dbo].[vw_CollectionFinal]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vw_CollectionFinal]
AS
SELECT DISTINCT CustomerId, MIN(StartDate) AS StartDate
FROM         (SELECT     loanID, CustomerId, StartDate, DateClose, MaxDelinquencyDays, RepaymentPeriod, CurrentBalance, Gender, FirstName, MiddleInitial, 
                                              Surname, RefNumber, Line1, Line2, Line3, Town, County, Postcode, DateOfBirth, LoanAmount,
                                              CompanyType, LimitedRefNum, NonLimitedRefNum, CustomerState, SortCode
                       FROM          dbo.vw_NotClose
                       WHERE      (loanID IN
                                                  (SELECT DISTINCT nc.loanID
                                                    FROM          dbo.vw_NotClose AS nc INNER JOIN
                                                                           dbo.vw_collection AS vc ON nc.DateClose = vc.DateClosed AND nc.MaxDelinquencyDays = vc.MaxDelinquencyDays))) 
                      AS a
GROUP BY CustomerId

GO
/****** Object:  UserDefinedFunction [dbo].[GetAmazonReviews]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetAmazonReviews]()
RETURNS TABLE 
AS
RETURN 
(
select cmp.CustomerId,
	 (afi.Negative + afi.Positive + afi.Neutral) as reviews,
	 rating=
	 case when (afi.Negative + afi.Positive + afi.Neutral) = 0
	 then 0
	 else (afi.Positive*100)/(afi.Negative + afi.Positive + afi.Neutral)
	 end
	 from (select m.CustomerMarketPlaceId, af.Id from  dbo.MP_AmazonFeedback as af INNER JOIN (select MAX(Created) Created, CustomerMarketPlaceId CustomerMarketPlaceId from dbo.MP_AmazonFeedback GROUP BY CustomerMarketPlaceId) as m
ON af.Created = m.Created and af.CustomerMarketPlaceId = m.CustomerMarketPlaceId) as af
Left outer join dbo.MP_AmazonFeedbackItem as afi on
af.Id = afi.AmazonFeedbackId and afi.TimePeriodId = 4
Left outer join dbo.MP_CustomerMarketPlace as cmp on 
cmp.Id = af.CustomerMarketPlaceId
)

GO
/****** Object:  UserDefinedFunction [dbo].[GetApprovedGrouped]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetApprovedGrouped]
(	
	@dateStart DateTime, 
	@dateEnd DateTime
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT res.IdUnderwriter, SUM(res.SystemCalculatedSum) ApprovedAmount, Count(res.IdUnderwriter) Approved FROM 
	(SELECT IdUnderwriter, SystemCalculatedSum 
		FROM CashRequests 
			Where SystemDecisionDate >= @dateStart
				AND UnderwriterDecisionDate <= @dateEnd
				AND UnderwriterDecision = 'Approved') as res
	Group by IdUnderwriter
)

GO
/****** Object:  UserDefinedFunction [dbo].[GetApprovedGroupedByMedal]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetApprovedGroupedByMedal]
(	
	@dateStart DateTime, 
	@dateEnd DateTime
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT res.Medal, SUM(res.SystemCalculatedSum) ApprovedAmount, Count(res.Medal) Approved FROM 
	(SELECT MedalType as Medal, SystemCalculatedSum 
		FROM CashRequests 
			Where SystemDecisionDate >= @dateStart
				AND UnderwriterDecisionDate <= @dateEnd
				AND UnderwriterDecision = 'Approved') as res
	Group by Medal
)

GO
/****** Object:  UserDefinedFunction [dbo].[GetBiggestExpensesPayPalTransactions]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetBiggestExpensesPayPalTransactions]
(	
  @marketpalceId int
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT TOP 5 *
	FROM
	(SELECT 
		Payer,
		Sum(pi.NetAmountAmount) Income24Plus
		FROM [dbo].MP_PayPalTransactionItem pi, [dbo].MP_PayPalTransaction pt
		Where Status = 'Completed' AND Type = 'Payment' AND
			pi.TransactionId = pt.Id AND pt.CustomerMarketPlaceId = @marketpalceId
			AND Payer != '' AND pi.NetAmountAmount < 0
		Group BY Payer) as tr
		ORDER BY tr.Income24Plus
)

GO
/****** Object:  UserDefinedFunction [dbo].[GetBiggestIncomePayPalTransactions]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetBiggestIncomePayPalTransactions]
(	
  @marketpalceId int
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT TOP 5 *
	FROM
	(SELECT 
		Payer,
		Sum(pi.NetAmountAmount) Income24Plus
		FROM [dbo].MP_PayPalTransactionItem pi, [dbo].MP_PayPalTransaction pt
		Where Status = 'Completed' AND Type = 'Payment' AND
			pi.TransactionId = pt.Id AND pt.CustomerMarketPlaceId = @marketpalceId
			AND Payer != '' AND pi.NetAmountAmount > 0
		Group BY Payer) as tr
		ORDER BY tr.Income24Plus DESC
)

GO
/****** Object:  UserDefinedFunction [dbo].[GetCustomerMarketplaceUpdatingCounters]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
2012-11-01 O.Zemskyi Customer Marketplace Updating Counters
*/
CREATE FUNCTION [dbo].[GetCustomerMarketplaceUpdatingCounters]
(	
  @marketplaceId int
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT c.Method, COUNT(*) AS Counter
  FROM MP_CustomerMarketplaceUpdatingCounter c
  left join MP_CustomerMarketplaceUpdatingActionLog l on l.Id = c.CustomerMarketplaceUpdatingActionLogId
  left join MP_CustomerMarketPlaceUpdatingHistory h on l.CustomerMarketplaceUpdatingHistoryRecordId = h.Id
  where h.CustomerMarketPlaceId = @marketplaceId--149
  group by c.method	
)

GO
/****** Object:  UserDefinedFunction [dbo].[GetEbayReviews]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetEbayReviews]()
RETURNS TABLE 
AS
RETURN 
(
select cmp.CustomerId,
	 (afi.Negative + afi.Positive + afi.Neutral) as reviews,
	 rating=
	 case when (afi.Negative + afi.Positive + afi.Neutral) = 0
	 then 0
	 else (afi.Positive*100)/(afi.Negative + afi.Positive + afi.Neutral)
	 end
	 from (select m.CustomerMarketPlaceId, af.Id from  dbo.MP_EbayFeedback as af INNER JOIN (select MAX(Created) Created, CustomerMarketPlaceId CustomerMarketPlaceId from dbo.MP_EbayFeedback GROUP BY CustomerMarketPlaceId) as m
ON af.Created = m.Created and af.CustomerMarketPlaceId = m.CustomerMarketPlaceId) as af
Left outer join dbo.MP_EbayFeedbackItem as afi on
af.Id = afi.EbayFeedbackId and afi.TimePeriodId = 4
Left outer join dbo.MP_CustomerMarketPlace as cmp on 
cmp.Id = af.CustomerMarketPlaceId
)

GO
/****** Object:  UserDefinedFunction [dbo].[GetEscalatedGrouped]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetEscalatedGrouped]
(	
	@dateStart DateTime, 
	@dateEnd DateTime
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT res.IdUnderwriter, SUM(res.SystemCalculatedSum) EscalatedAmount, Count(res.IdUnderwriter) Escalated FROM 
	(SELECT IdUnderwriter, SystemCalculatedSum 
		FROM CashRequests 
			Where SystemDecisionDate >= @dateStart
				AND UnderwriterDecisionDate <= @dateEnd
				AND UnderwriterDecision = 'Escalated') as res
	Group by IdUnderwriter
)

GO
/****** Object:  UserDefinedFunction [dbo].[GetEscalatedGroupedByMedal]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetEscalatedGroupedByMedal]
(	
	@dateStart DateTime, 
	@dateEnd DateTime
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT res.Medal, SUM(res.SystemCalculatedSum) EscalatedAmount, Count(res.Medal) Escalated FROM 
	(SELECT MedalType as Medal, SystemCalculatedSum 
		FROM CashRequests 
			Where SystemDecisionDate >= @dateStart
				AND UnderwriterDecisionDate <= @dateEnd
				AND UnderwriterDecision = 'Escalated') as res
	Group by Medal
)

GO
/****** Object:  UserDefinedFunction [dbo].[GetExpensesPayPalTransactionsByPayerAndRange]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetExpensesPayPalTransactionsByPayerAndRange]
(	
  @payer nvarchar(255),
  @marketplaceId int,
  @startDate DateTime,
  @endDate DateTime
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT 
		ISNULL( -Sum(pi.NetAmountAmount), 0) Income
		FROM [dbo].MP_PayPalTransactionItem pi, [dbo].MP_PayPalTransaction pt
		Where Status = 'Completed' AND Type = 'Payment' AND
			pi.TransactionId = pt.Id AND pi.Payer = @payer AND pt.CustomerMarketPlaceId = @marketplaceId AND
			pi.NetAmountAmount < 0 AND pi.Created BETWEEN @startDate AND @endDate 
		Group BY Payer
)

GO
/****** Object:  UserDefinedFunction [dbo].[GetExpensesPayPalTransactionsByRange]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
2012-10-08 O.Zemskyi Change status an set NetAmountAmount > 0
*/
CREATE FUNCTION [dbo].[GetExpensesPayPalTransactionsByRange]
(	
  @marketplaceId int,
  @startDate DateTime,
  @endDate DateTime
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT 
		ISNULL(Sum(pi.NetAmountAmount), 0) Income
		FROM [dbo].MP_PayPalTransactionItem pi, [dbo].MP_PayPalTransaction pt
		Where Status = 'Completed' AND Type = 'Transfer' AND
			pi.TransactionId = pt.Id AND pt.CustomerMarketPlaceId = @marketplaceId AND
			pi.NetAmountAmount > 0 AND pi.Created BETWEEN @startDate AND @endDate 
		Group BY pt.CustomerMarketPlaceId
)

GO
/****** Object:  UserDefinedFunction [dbo].[GetFullyEarlyPaid]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetFullyEarlyPaid]
(	
	@date DateTime
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT ls.Date, ls.AmountDue, ls.LoanRepayment, ls.loanid FROM dbo.LoanSchedule as ls
	LEFT OUTER JOIN dbo.LoanTransaction as lt ON lt.LoanId = ls.LoanId
	Where ls.Date > @date 
			AND ls.Date < DATEADD(day, 1, @date ) 
			AND lt.Status = 'Done' 
			AND lt.PostDate < @date
			AND ls.AmountDue = 0
			AND ls.LoanRepayment > 0
)

GO
/****** Object:  UserDefinedFunction [dbo].[GetFullyPaidOnTime]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetFullyPaidOnTime]
(	
	@date DateTime
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT ls.Date, ls.AmountDue, ls.LoanRepayment, ls.loanid FROM dbo.LoanSchedule as ls
	LEFT OUTER JOIN dbo.LoanTransaction as lt ON lt.LoanId = ls.LoanId
	Where ls.Date > @date 
			AND ls.Date < DATEADD(day, 1, @date ) 
			AND lt.Status = 'Done' 
			AND lt.PostDate > @date
			AND lt.PostDate < DATEADD(day, 1, @date )
			AND ls.AmountDue = 0
			AND ls.LoanRepayment > 0
)

GO
/****** Object:  UserDefinedFunction [dbo].[GetHighSideGrouped]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetHighSideGrouped]
(	
	@dateStart DateTime, 
	@dateEnd DateTime
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT res.IdUnderwriter, SUM(res.SystemCalculatedSum) HighSideAmount, Count(res.IdUnderwriter) HighSide FROM 
	(SELECT IdUnderwriter, SystemCalculatedSum 
		FROM CashRequests 
			Where SystemDecisionDate >= @dateStart
				AND UnderwriterDecisionDate <= @dateEnd
				AND UnderwriterDecision = 'Approved' 
				AND SystemDecision != 'Approve' ) as res
	Group by IdUnderwriter
)

GO
/****** Object:  UserDefinedFunction [dbo].[GetHighSideGroupedByMedal]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetHighSideGroupedByMedal]
(	
	@dateStart DateTime, 
	@dateEnd DateTime
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT res.Medal, SUM(res.SystemCalculatedSum) HighSideAmount, Count(res.Medal) HighSide FROM 
	(SELECT MedalType as Medal, SystemCalculatedSum 
		FROM CashRequests 
			Where SystemDecisionDate >= @dateStart
				AND UnderwriterDecisionDate <= @dateEnd
				AND UnderwriterDecision = 'Approved' 
				AND SystemDecision != 'Approve' ) as res
	Group by Medal
)

GO
/****** Object:  UserDefinedFunction [dbo].[GetId_MaxUpdated]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetId_MaxUpdated] 
(	
	-- Add the parameters for the function here
	@CustomerId int
)
RETURNS TABLE 
AS
RETURN 
(
select  h.CustomerMarketPlaceId, h.UpdatingStart as Updated, h.Id as UpdateHistoryId
from MP_CustomerMarketPlaceUpdatingHistory h 
inner join 
(
	select CustomerMarketPlaceId, MAX(UpdatingStart) as MaxUpdatedDate
	from MP_CustomerMarketPlaceUpdatingHistory
	where CustomerMarketPlaceId in 
		(
		select id 
		from MP_CustomerMarketPlace
		where CustomerId = @CustomerId
		--and EliminationPassed = 1
		)
	and UpdatingStart is not null
	and UpdatingEnd is not null
	and ERROR is null

	group by CustomerMarketPlaceId

) maxHistory on h.CustomerMarketPlaceId = maxHistory.CustomerMarketPlaceId 
and h.UpdatingStart = maxHistory.MaxUpdatedDate
)

GO
/****** Object:  UserDefinedFunction [dbo].[GetIncomePayPalTransactionsByPayerAndRange]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetIncomePayPalTransactionsByPayerAndRange]
(	
  @payer nvarchar(255),
  @marketplaceId int,
  @startDate DateTime,
  @endDate DateTime
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT 
		ISNULL( Sum(pi.NetAmountAmount), 0) Income
		FROM [dbo].MP_PayPalTransactionItem pi, [dbo].MP_PayPalTransaction pt
		Where Status = 'Completed' AND Type = 'Payment' AND
			pi.TransactionId = pt.Id AND pi.Payer = @payer AND pt.CustomerMarketPlaceId = @marketplaceId AND
			pi.NetAmountAmount > 0 AND pi.Created BETWEEN @startDate AND @endDate 
		Group BY Payer
)

GO
/****** Object:  UserDefinedFunction [dbo].[GetIncomePayPalTransactionsByRange]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetIncomePayPalTransactionsByRange]
(	
  @marketplaceId int,
  @startDate DateTime,
  @endDate DateTime
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT 
		ISNULL( Sum(pi.NetAmountAmount), 0) Income
		FROM [dbo].MP_PayPalTransactionItem pi, [dbo].MP_PayPalTransaction pt
		Where Status = 'Completed' AND Type = 'Payment' AND
			pi.TransactionId = pt.Id AND pt.CustomerMarketPlaceId = @marketplaceId AND
			pi.NetAmountAmount > 0 AND pi.Created BETWEEN @startDate AND @endDate 
		Group BY pt.CustomerMarketPlaceId
)

GO
/****** Object:  UserDefinedFunction [dbo].[GetLatePayments]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetLatePayments]
(	
  @date DateTime	
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT res.LoanId, SUM(res.AmountDue) LatePaymentsAmount FROM 
	(SELECT LoanId, AmountDue 
		FROM LoanSchedule
			Where Status = 'Late' AND 
			Date > @date and
			Date < DATEADD(day, 1, @date )) as res
	Group by LoanId
)

GO
/****** Object:  UserDefinedFunction [dbo].[GetLoanByRequestCashId]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetLoanByRequestCashId]
(	
)
RETURNS TABLE 
AS
RETURN 
(
	Select dbo.Loan.RequestCashId, Sum(Late30) as Late30Amount, 
Sum(Late30Num) as Late30, 
Sum(Late60) as Late60Amount, 
Sum(Late60Num) as Late60, 
Sum(Late90) as Late90Amount, 
Sum(Late90Num) as Late90, 
Sum(OnTime) as PaidAmount, 
Sum(OnTimeNum) as Paid, 
Sum(PastDues) as DefaultsAmount, 
Sum(PastDuesNum) as Defaults,
Sum(Balance) as Exposure
FROM dbo.Loan
Where dbo.Loan.RequestCashId IS NOT NULL 
Group By(dbo.Loan.RequestCashId) 
)

GO
/****** Object:  UserDefinedFunction [dbo].[GetLowSideGrouped]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetLowSideGrouped]
(	
	@dateStart DateTime, 
	@dateEnd DateTime
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT res.IdUnderwriter, SUM(res.SystemCalculatedSum) LowSideAmount, Count(res.IdUnderwriter) LowSided FROM 
	(SELECT IdUnderwriter, SystemCalculatedSum 
		FROM CashRequests 
			Where SystemDecisionDate >= @dateStart
				AND UnderwriterDecisionDate <= @dateEnd
				AND UnderwriterDecision = 'Rejected' 
				AND SystemDecision != 'Reject' ) as res
	Group by IdUnderwriter
)

GO
/****** Object:  UserDefinedFunction [dbo].[GetLowSideGroupedByMedal]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetLowSideGroupedByMedal]
(	
	@dateStart DateTime, 
	@dateEnd DateTime
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT res.Medal, SUM(res.SystemCalculatedSum) LowSideAmount, Count(res.Medal) LowSided FROM 
	(SELECT MedalType as Medal, SystemCalculatedSum 
		FROM CashRequests 
			Where SystemDecisionDate >= @dateStart
				AND UnderwriterDecisionDate <= @dateEnd
				AND UnderwriterDecision = 'Rejected' 
				AND SystemDecision != 'Reject' ) as res
	Group by Medal
)

GO

/****** Object:  UserDefinedFunction [dbo].[GetNotPaid]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetNotPaid]
(	
	@date DateTime
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT ls.Date, ls.AmountDue, ls.LoanRepayment, ls.loanid FROM dbo.LoanSchedule as ls
	Where ls.Date > @date 
			AND ls.Date < DATEADD(day, 1, @date ) 
			AND ls.Date >= @date
			AND ls.AmountDue > 0
			AND ls.LoanRepayment = 0
)

GO
/****** Object:  UserDefinedFunction [dbo].[GetNumOfDefaultAccounts]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Oleg Zemskyi
-- Create date: 27.06.2013
-- Description:	Get count from [ExperianDefaultAccount]
-- =============================================
CREATE FUNCTION [dbo].[GetNumOfDefaultAccounts]
(
	@CustomerId INT, @Months INT, @Amount int
)
RETURNS TABLE 
AS
RETURN 
(
	select COUNT(eda.Id) as NumOfDefaultAccounts
	FROM [ExperianDefaultAccount] eda
	where eda.CustomerId = @CustomerId and
	eda.date > dateadd(MM, -@Months, getdate()) and Balance > @Amount
	and eda.[DateAdded] = (select max(eda1.DateAdded) as maxdate FROM [ExperianDefaultAccount] eda1
								  where eda1.CustomerId = @CustomerId 
					       )
)

GO
/****** Object:  UserDefinedFunction [dbo].[GetOpenCreditLineByMedal]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetOpenCreditLineByMedal]
(
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT UPPER(MedalType) Medal, SUM(SystemCalculatedSum - l.loanAmount) as OpenCreditLine from CashRequests 
LEFT OUTER JOIN 
(SELECT RequestCashId, SUM(LoanAmount) as loanAmount FROM Loan
WHERE RequestCashId IS NOT NULL
Group by RequestCashId) as l ON CashRequests.Id = l.RequestCashId
WHERE UnderwriterDecisionDate > (GetDate() - 24)
Group BY UPPER(MedalType)
)

GO
/****** Object:  UserDefinedFunction [dbo].[GetOpenCreditLineByUnderwriter]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetOpenCreditLineByUnderwriter]
()
RETURNS TABLE 
AS
RETURN 
(
	SELECT IdUnderwriter, SUM(SystemCalculatedSum - l.loanAmount) as OpenCreditLine from CashRequests 
LEFT OUTER JOIN 
(SELECT RequestCashId, SUM(LoanAmount) as loanAmount FROM Loan
WHERE RequestCashId IS NOT NULL
Group by RequestCashId) as l ON CashRequests.Id = l.RequestCashId
WHERE UnderwriterDecisionDate > (GetDate() - 24)
Group BY IdUnderwriter
)

GO
/****** Object:  UserDefinedFunction [dbo].[GetPartialEarlyPaid]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetPartialEarlyPaid]
(	
	@date DateTime
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT ls.Date, ls.AmountDue, ls.LoanRepayment, ls.loanid FROM dbo.LoanSchedule as ls
	LEFT OUTER JOIN dbo.LoanTransaction as lt ON lt.LoanId = ls.LoanId
	Where ls.Date > @date 
			AND ls.Date < DATEADD(day, 1, @date ) 
			AND lt.Status = 'Done' 
			AND lt.PostDate < @date
			AND ls.AmountDue > 0
			AND ls.LoanRepayment > 0
)

GO
/****** Object:  UserDefinedFunction [dbo].[GetPartialPaidOnTime]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetPartialPaidOnTime]
(	
	@date DateTime
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT ls.Date, ls.AmountDue, ls.LoanRepayment,  ls.loanid FROM dbo.LoanSchedule as ls
	LEFT OUTER JOIN dbo.LoanTransaction as lt ON lt.LoanId = ls.LoanId
	Where ls.Date > @date 
			AND ls.Date < DATEADD(day, 1, @date ) 
			AND lt.Status = 'Done' 
			AND lt.PostDate > @date
			AND lt.PostDate < DATEADD(day, 1, @date )
			AND ls.AmountDue > 0
			AND ls.LoanRepayment > 0
)

GO
/****** Object:  UserDefinedFunction [dbo].[GetRejectedGrouped]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetRejectedGrouped]
(	
	@dateStart DateTime, 
	@dateEnd DateTime
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT res.IdUnderwriter, SUM(res.SystemCalculatedSum) RejectedAmount, Count(res.IdUnderwriter) Rejected FROM 
	(SELECT IdUnderwriter, SystemCalculatedSum 
		FROM CashRequests 
			Where SystemDecisionDate >= @dateStart
				AND UnderwriterDecisionDate <= @dateEnd
				AND UnderwriterDecision = 'Rejected') as res
	Group by IdUnderwriter
)

GO
/****** Object:  UserDefinedFunction [dbo].[GetRejectedGroupedByMedal]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetRejectedGroupedByMedal]
(	
	@dateStart DateTime, 
	@dateEnd DateTime
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT res.Medal, SUM(res.SystemCalculatedSum) RejectedAmount, Count(res.Medal) Rejected FROM 
	(SELECT MedalType as Medal, SystemCalculatedSum 
		FROM CashRequests 
			Where SystemDecisionDate >= @dateStart
				AND UnderwriterDecisionDate <= @dateEnd
				AND UnderwriterDecision = 'Rejected') as res
	Group by Medal
)

GO
/****** Object:  UserDefinedFunction [dbo].[GetStoresCount]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetStoresCount]
(	
	@storeType int,
	@dateStart DateTime, 
	@dateEnd DateTime
)
RETURNS TABLE 
AS
RETURN 
(
	Select Count(MP_CustomerMarketPlace.Id) StoresCount, CustomerId FROM
	(SELECT Id
		FROM dbo.MP_CustomerMarketPlace
			Where MarketPlaceId = @storeType
	) as m LEFT OUTER JOIN 
	(SELECT CustomerMarketPlaceId
		FROM dbo.MP_CustomerMarketPlaceUpdatingHistory
			Where UpdatingEnd <= @dateEnd 
			AND UpdatingStart >= @dateStart
		GROUP BY CustomerMarketPlaceId
	) as um ON um.CustomerMarketPlaceId = m.Id
	LEFT OUTER JOIN MP_CustomerMarketPlace ON
	MP_CustomerMarketPlace.Id = m.Id
	Group By CustomerId
)

GO
/****** Object:  UserDefinedFunction [dbo].[GetTransactionsCountPayPalTransactionsByRange]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
19.10.2012 O.Zemskyi Поменял условие отбора транзакций с pi.NetAmountAmount < 0 на pi.NetAmountAmount > 0
*/
CREATE FUNCTION [dbo].[GetTransactionsCountPayPalTransactionsByRange]
(	
  @marketplaceId int,
  @startDate DateTime,
  @endDate DateTime
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT 
		Count(pi.NetAmountAmount) Income
		FROM [dbo].MP_PayPalTransactionItem pi, [dbo].MP_PayPalTransaction pt
		Where Status = 'Completed' AND Type = 'Payment' AND
			pi.TransactionId = pt.Id AND pt.CustomerMarketPlaceId = @marketplaceId AND
			pi.NetAmountAmount > 0 AND pi.Created BETWEEN @startDate AND @endDate 
		Group BY pt.CustomerMarketPlaceId
)

GO


/****** Object:  View [dbo].[CustomerLoyaltyProgramPoints]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[CustomerLoyaltyProgramPoints]
AS
SELECT
	CustomerID,
	SUM(EarnedPoints) AS EarnedPoints,
	MAX(ActionDate) AS LastActionDate
FROM
	CustomerLoyaltyProgram
GROUP BY
	CustomerID

GO


/****** Object:  View [dbo].[LoyaltyProgramCheckedAccounts]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[LoyaltyProgramCheckedAccounts]
AS
SELECT
	clp.CustomerMarketPlaceID
FROM
	CustomerLoyaltyProgram clp
	INNER JOIN LoyaltyProgramActions a ON clp.ActionID = a.ActionID
WHERE
	a.ActionName = 'ACCOUNTCHECKED'

GO
/****** Object:  View [dbo].[Security_vUser]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[Security_vUser]
AS
SELECT     u.UserId, u.UserName, u.FullName, u.EMail, u.IsDeleted, s.SessionCreationDate, u.CreationDate, u.PassExpPeriod, u.ForcePassChange, 
           u.DisablePassChange, u.CertificateThumbprint
FROM         Security_User AS u LEFT OUTER JOIN
                          (SELECT    UserId, MAX(CreationDate) AS SessionCreationDate
                            FROM          Security_Session
                            GROUP BY UserId) AS s ON s.UserId = u.UserId

GO


/****** Object:  View [dbo].[vPacnetBalance]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create VIEW [dbo].[vPacnetBalance]
as

select * from fnPacnetBalance()

GO
/****** Object:  View [dbo].[vw_LoansAmountByDay]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vw_LoansAmountByDay]
as
SELECT ll.Date, sum(ll.LoanAmount) as Amount
  FROM (
	SELECT DATEADD(D, 0, DATEDIFF(D, 0, Date)) as Date, l.LoanAmount
	  FROM [dbo].[Loan] l
  ) ll
  group by Date

GO
/****** Object:  View [dbo].[vw_UpdateUsersMarketPlaces]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vw_UpdateUsersMarketPlaces]
as
SELECT c.Id AS 'CustomerId', c.RefNumber, c.Fullname,  
cmp.DisplayName AS 'MarketplaceName', 
mt.name AS 'TypeOfStore',
cmpuhal.UpdatingStart,cmpuhal.UpdatingEnd, --cmpuh.[Error],--cmpuh.UpdatingTimePassInSeconds,
cmpuhal.ActionName, cmpuhal.ControlValueName,cmpuhal.ControlValue,
[Result]=
CASE
WHEN cmpuhal.[Error] IS NULL
THEN 'Passed'
ELSE
cmpuhal.[Error]
END,	
cmpuhal.UpdatingTimePassInSeconds 
from dbo.MP_CustomerMarketPlace cmp
RIGHT JOIN dbo.MP_CustomerMarketPlaceUpdatingHistory cmpuh
ON cmpuh.CustomerMarketPlaceId = cmp.Id
RIGHT join dbo.MP_MarketplaceType as mt
on cmp.MarketPlaceId=mt.Id
right JOIN Customer c
ON c.Id = cmp.CustomerId
RIGHT JOIN MP_CustomerMarketplaceUpdatingActionLog cmpuhal 
ON cmpuhal.CustomerMarketplaceUpdatingHistoryRecordId = cmpuh.Id
--ORDER BY cmpuhal.UpdatingStart asc

GO
/****** Object:  View [dbo].[vw_UsersMarketPlaces]    Script Date: 04-Nov-13 5:03:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create VIEW [dbo].[vw_UsersMarketPlaces]
as
SELECT
c.Id 'Customer ID', 
c.RefNumber 'RefNumber',
FullName=
CASE WHEN c.FullName is NULL
THEN 'Wizard is not finished'
ELSE c.FullName
END,  
cmp.DisplayName 'Marketplace name', 
mt.name 'Type of store', 
cmp.UpdatingStart 'Time Start', 
cmp.UpdatingEnd 'Time End',
datediff(mi, cmp.UpdatingStart, cmp.UpdatingEnd) 'Time in min',
Result=
CASE WHEN cmp.UpdateError is not NULL
THEN 'passed'
ELSE 'not passed'
end
  FROM Customer as c RIGHT join MP_CustomerMarketPlace as cmp on c.Id=cmp.CustomerId
  RIGHT join MP_MarketplaceType as mt
on cmp.MarketPlaceId=mt.Id
AND cmp.UpdatingEnd IS NOT NULL

GO

/****** Object:  Index [IX_AutoresponderLog_Email]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_AutoresponderLog_Email] ON [dbo].[AutoresponderLog]
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IDX_Business]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IDX_Business] ON [dbo].[Business]
(
	[Name] ASC,
	[Address] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_CashRequests_IDCust]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_CashRequests_IDCust] ON [dbo].[CashRequests]
(
	[Id] ASC,
	[IdCustomer] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_ConfigurationVariables]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_ConfigurationVariables] ON [dbo].[ConfigurationVariables]
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_CRMActions]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_CRMActions] ON [dbo].[CRMActions]
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_CRMStatusesName]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_CRMStatusesName] ON [dbo].[CRMStatuses]
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_Customer_Fill]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_Customer_Fill] ON [dbo].[Customer]
(
	[WizardStep] ASC,
	[IsTest] ASC
)
INCLUDE ( 	[Id],
	[CreditResult],
	[FirstName],
	[MiddleInitial],
	[Surname],
	[DateOfBirth],
	[TimeAtAddress],
	[ResidentialStatus],
	[Gender],
	[MartialStatus],
	[TypeOfBusiness],
	[DaytimePhone],
	[MobilePhone],
	[Fullname],
	[OverallTurnOver],
	[WebSiteTurnOver]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_Customer_RefNumber]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_Customer_RefNumber] ON [dbo].[Customer]
(
	[RefNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_CustomerAddress_ID]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_CustomerAddress_ID] ON [dbo].[CustomerAddress]
(
	[addressId] ASC,
	[addressType] ASC
)
INCLUDE ( 	[id],
	[Organisation],
	[Line1],
	[Line2],
	[Line3],
	[Town],
	[County],
	[Postcode],
	[Country],
	[Rawpostcode],
	[Deliverypointsuffix],
	[Nohouseholds],
	[Smallorg],
	[Pobox],
	[Mailsortcode],
	[Udprn]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_CustomerLoyaltyProgramCustomerId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_CustomerLoyaltyProgramCustomerId] ON [dbo].[CustomerLoyaltyProgram]
(
	[CustomerID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_CustomerReason]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_CustomerReason] ON [dbo].[CustomerReason]
(
	[Reason] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_CustomerSourceOfRepayment]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_CustomerSourceOfRepayment] ON [dbo].[CustomerSourceOfRepayment]
(
	[SourceOfRepayment] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_1]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_1] ON [dbo].[DataSource_Sources]
(
	[IsDeleted] ASC,
	[DisplayName] ASC,
	[TerminationDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_DbString_Key]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_DbString_Key] ON [dbo].[DbString]
(
	[Key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_ServiceLogId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_ServiceLogId] ON [dbo].[ExperianDefaultAccount]
(
	[ServiceLogId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Export_Results_FType]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_Export_Results_FType] ON [dbo].[Export_Results]
(
	[FileType] ASC,
	[ApplicationId] ASC
)
INCLUDE ( 	[SourceTemplateId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_LOAN_CustId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_LOAN_CustId] ON [dbo].[Loan]
(
	[CustomerId] ASC,
	[Balance] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_LoanAgreement_Loan]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_LoanAgreement_Loan] ON [dbo].[LoanAgreement]
(
	[LoanId] ASC
)
INCLUDE ( 	[Name],
	[Template],
	[FilePath]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
/****** Object:  Index [IX_LoanHistory_Date]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_LoanHistory_Date] ON [dbo].[LoanHistory]
(
	[Date] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_LoanSchedule_Date]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_LoanSchedule_Date] ON [dbo].[LoanSchedule]
(
	[Status] ASC
)
INCLUDE ( 	[Date]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_LoanSchedule_LoanId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_LoanSchedule_LoanId] ON [dbo].[LoanSchedule]
(
	[Status] ASC
)
INCLUDE ( 	[Date],
	[LoanId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_LoanSchedule_LoanId_Date]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_LoanSchedule_LoanId_Date] ON [dbo].[LoanSchedule]
(
	[LoanId] ASC,
	[Status] ASC
)
INCLUDE ( 	[Date],
	[LoanRepayment]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_LT_ID_TYPE]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_LT_ID_TYPE] ON [dbo].[LoanTransaction]
(
	[Id] ASC,
	[Type] ASC,
	[Status] ASC
)
INCLUDE ( 	[LoanId],
	[LoanRepayment]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_AMO2_MarketPlaceId_OS]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_AMO2_MarketPlaceId_OS] ON [dbo].[MP_AmazonOrderItem2]
(
	[OrderStatus] ASC,
	[Id] ASC,
	[AmazonOrderId] ASC,
	[PurchaseDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_AMO2_MarketPlaceId_PD]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_AMO2_MarketPlaceId_PD] ON [dbo].[MP_AmazonOrderItem2]
(
	[AmazonOrderId] ASC,
	[PurchaseDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_AMO2_MarketPlaceId_PD2]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_AMO2_MarketPlaceId_PD2] ON [dbo].[MP_AmazonOrderItem2]
(
	[AmazonOrderId] ASC,
	[PurchaseDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_MP_AmazonOrderItem2_AOId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_AmazonOrderItem2_AOId] ON [dbo].[MP_AmazonOrderItem2]
(
	[AmazonOrderId] ASC,
	[Id] ASC,
	[OrderId] ASC,
	[SellerOrderId] ASC,
	[PurchaseDate] ASC,
	[LastUpdateDate] ASC,
	[OrderStatus] ASC,
	[OrderTotalCurrency] ASC,
	[OrderTotal] ASC,
	[NumberOfItemsShipped] ASC,
	[NumberOfItemsUnshipped] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_MP_AmazonOrderItemDetail_ASIN]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_AmazonOrderItemDetail_ASIN] ON [dbo].[MP_AmazonOrderItemDetail]
(
	[ASIN] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_MP_AnalyisisFunctionName]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_MP_AnalyisisFunctionName] ON [dbo].[MP_AnalyisisFunction]
(
	[Name] ASC,
	[MarketPlaceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_AnalyisisFunctionCreated]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_AnalyisisFunctionCreated] ON [dbo].[MP_AnalyisisFunctionValues]
(
	[Updated] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_AnalyisisFunctionValues]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_AnalyisisFunctionValues] ON [dbo].[MP_AnalyisisFunctionValues]
(
	[AnalyisisFunctionId] ASC,
	[AnalysisFunctionTimePeriodId] ASC,
	[CustomerMarketPlaceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_AnalyisisFunctionValues_AFI]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_AnalyisisFunctionValues_AFI] ON [dbo].[MP_AnalyisisFunctionValues]
(
	[AnalyisisFunctionId] ASC
)
INCLUDE ( 	[AnalysisFunctionTimePeriodId],
	[ValueFloat],
	[CustomerMarketPlaceUpdatingHistoryRecordId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_AnalyisisFunctionValues_AFTPI]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_AnalyisisFunctionValues_AFTPI] ON [dbo].[MP_AnalyisisFunctionValues]
(
	[AnalysisFunctionTimePeriodId] ASC
)
INCLUDE ( 	[AnalyisisFunctionId],
	[ValueInt],
	[CustomerMarketPlaceUpdatingHistoryRecordId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_AnalyisisFunctionValues_Include]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_AnalyisisFunctionValues_Include] ON [dbo].[MP_AnalyisisFunctionValues]
(
	[AnalyisisFunctionId] ASC
)
INCLUDE ( 	[AnalysisFunctionTimePeriodId],
	[ValueInt],
	[ValueFloat],
	[CustomerMarketPlaceUpdatingHistoryRecordId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_AnalyisisFunctionValues2]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_AnalyisisFunctionValues2] ON [dbo].[MP_AnalyisisFunctionValues]
(
	[CustomerMarketPlaceId] ASC,
	[AnalyisisFunctionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [MP_AnalyisisFunctionValues_AFTP]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [MP_AnalyisisFunctionValues_AFTP] ON [dbo].[MP_AnalyisisFunctionValues]
(
	[AnalysisFunctionTimePeriodId] ASC
)
INCLUDE ( 	[AnalyisisFunctionId],
	[ValueInt],
	[CustomerMarketPlaceUpdatingHistoryRecordId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_MP_AnalysisFunctionTimePeriod]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_MP_AnalysisFunctionTimePeriod] ON [dbo].[MP_AnalysisFunctionTimePeriod]
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_ChannelGrabberOrderCustomerMarketPlaceId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_ChannelGrabberOrderCustomerMarketPlaceId] ON [dbo].[MP_ChannelGrabberOrder]
(
	[CustomerMarketPlaceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_ChannelGrabberOrderItemOrderId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_ChannelGrabberOrderItemOrderId] ON [dbo].[MP_ChannelGrabberOrderItem]
(
	[OrderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_MP_CurrencyName]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_MP_CurrencyName] ON [dbo].[MP_Currency]
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_CurrencyRateHistory_CurId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_CurrencyRateHistory_CurId] ON [dbo].[MP_CurrencyRateHistory]
(
	[CurrencyId] ASC
)
INCLUDE ( 	[Id],
	[Price],
	[Updated]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_CurrencyRateHistory_UpdateId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_CurrencyRateHistory_UpdateId] ON [dbo].[MP_CurrencyRateHistory]
(
	[CurrencyId] ASC,
	[Updated] ASC
)
INCLUDE ( 	[Price]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_MP_CustomerMarketPlace]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_CustomerMarketPlace] ON [dbo].[MP_CustomerMarketPlace]
(
	[CustomerId] ASC
)
INCLUDE ( 	[MarketPlaceId],
	[DisplayName],
	[EliminationPassed],
	[UpdateError]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_CustomerMarketPlace_CUstId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_CustomerMarketPlace_CUstId] ON [dbo].[MP_CustomerMarketPlace]
(
	[CustomerId] ASC,
	[UpdatingEnd] ASC
)
INCLUDE ( 	[UpdatingStart],
	[MarketPlaceId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_CustomerMarketPlaceUpdatingHistory_DateStart]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_CustomerMarketPlaceUpdatingHistory_DateStart] ON [dbo].[MP_CustomerMarketPlaceUpdatingHistory]
(
	[UpdatingStart] ASC
)
INCLUDE ( 	[CustomerMarketPlaceId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_MP_EbayAmazonCategory]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_EbayAmazonCategory] ON [dbo].[MP_EbayAmazonCategory]
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_EbayAmazonInventoryCustomerMarketPlaceId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_EbayAmazonInventoryCustomerMarketPlaceId] ON [dbo].[MP_EbayAmazonInventory]
(
	[CustomerMarketPlaceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MPEbayAmazonInventoryCreatedIncludeUMI]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MPEbayAmazonInventoryCreatedIncludeUMI] ON [dbo].[MP_EbayAmazonInventory]
(
	[Created] DESC
)
INCLUDE ( 	[CustomerMarketPlaceId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_MP_EbayAmazonInventoryItem_Id]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_EbayAmazonInventoryItem_Id] ON [dbo].[MP_EbayAmazonInventoryItem]
(
	[InventoryId] ASC
)
INCLUDE ( 	[Price],
	[Quantity],
	[Currency]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_MP_EbayAmazonInventoryItemCurrency]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_EbayAmazonInventoryItemCurrency] ON [dbo].[MP_EbayAmazonInventoryItem]
(
	[Currency] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_EbayAmazonInventoryItemInventoryId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_EbayAmazonInventoryItemInventoryId] ON [dbo].[MP_EbayAmazonInventoryItem]
(
	[InventoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_EbayAmazonInventoryItemPrice]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_EbayAmazonInventoryItemPrice] ON [dbo].[MP_EbayAmazonInventoryItem]
(
	[Price] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_EbayAmazonInventoryItemQuantity]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_EbayAmazonInventoryItemQuantity] ON [dbo].[MP_EbayAmazonInventoryItem]
(
	[Quantity] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_EbayFeedbackCreated]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_EbayFeedbackCreated] ON [dbo].[MP_EbayFeedback]
(
	[Created] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_EbayFeedbackCreatedDateIncludeMUI]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_EbayFeedbackCreatedDateIncludeMUI] ON [dbo].[MP_EbayFeedback]
(
	[Created] DESC
)
INCLUDE ( 	[CustomerMarketPlaceId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_EbayFeedbackCustomerMarketPlaceId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_EbayFeedbackCustomerMarketPlaceId] ON [dbo].[MP_EbayFeedback]
(
	[CustomerMarketPlaceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_EbayFeedbackItemEbayFeedbackId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_EbayFeedbackItemEbayFeedbackId] ON [dbo].[MP_EbayFeedbackItem]
(
	[EbayFeedbackId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_EbayOrderCustomerMarketPlaceId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_EbayOrderCustomerMarketPlaceId] ON [dbo].[MP_EbayOrder]
(
	[CustomerMarketPlaceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_EbayOrderItemOrderId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_EbayOrderItemOrderId] ON [dbo].[MP_EbayOrderItem]
(
	[OrderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_OrderItemCreatedTime]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_OrderItemCreatedTime] ON [dbo].[MP_EbayOrderItem]
(
	[CreatedTime] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_MP_OrderItemOrderStatus]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_OrderItemOrderStatus] ON [dbo].[MP_EbayOrderItem]
(
	[OrderStatus] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_EbayTransaction_OItId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_EbayTransaction_OItId] ON [dbo].[MP_EbayTransaction]
(
	[OrderItemId] ASC
)
INCLUDE ( 	[ItemInfoId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_EbayUserAccountDataCreatedDateIncludeMUI]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_EbayUserAccountDataCreatedDateIncludeMUI] ON [dbo].[MP_EbayUserAccountData]
(
	[Created] DESC
)
INCLUDE ( 	[CustomerMarketPlaceId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_EbayUserData_CreatedDateIncludeUMI]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_EbayUserData_CreatedDateIncludeUMI] ON [dbo].[MP_EbayUserData]
(
	[Created] DESC
)
INCLUDE ( 	[CustomerMarketPlaceId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_EkmOrderCustomerMarketPlaceId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_EkmOrderCustomerMarketPlaceId] ON [dbo].[MP_EkmOrder]
(
	[CustomerMarketPlaceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_EkmOrderItemOrderId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_EkmOrderItemOrderId] ON [dbo].[MP_EkmOrderItem]
(
	[OrderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_FreeAgentCompanyRequestId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_FreeAgentCompanyRequestId] ON [dbo].[MP_FreeAgentCompany]
(
	[RequestId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_FreeAgentExpenseCategoryId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_FreeAgentExpenseCategoryId] ON [dbo].[MP_FreeAgentExpense]
(
	[CategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_FreeAgentExpenseRequestId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_FreeAgentExpenseRequestId] ON [dbo].[MP_FreeAgentExpense]
(
	[RequestId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_MP_FreeAgentExpenseCategoryurl]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_FreeAgentExpenseCategoryurl] ON [dbo].[MP_FreeAgentExpenseCategory]
(
	[url] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_PK_MP_FreeAgentInvoiceRequestId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_PK_MP_FreeAgentInvoiceRequestId] ON [dbo].[MP_FreeAgentInvoice]
(
	[RequestId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_FreeAgentInvoiceItemInvoiceId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_FreeAgentInvoiceItemInvoiceId] ON [dbo].[MP_FreeAgentInvoiceItem]
(
	[InvoiceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_FreeAgentRequestCustomerMarketPlaceId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_FreeAgentRequestCustomerMarketPlaceId] ON [dbo].[MP_FreeAgentRequest]
(
	[CustomerMarketPlaceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_FreeAgentUsersRequestId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_FreeAgentUsersRequestId] ON [dbo].[MP_FreeAgentUsers]
(
	[RequestId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_ID_MPId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_ID_MPId] ON [dbo].[MP_PayPalTransaction]
(
	[Id] ASC,
	[CustomerMarketPlaceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_MP_PayPalTransactionItem_Payer]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_PayPalTransactionItem_Payer] ON [dbo].[MP_PayPalTransactionItem]
(
	[Payer] ASC
)
INCLUDE ( 	[TransactionId],
	[Created],
	[NetAmountAmount],
	[Type],
	[Status]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_MP_PayPalTransactionItem_Type]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_PayPalTransactionItem_Type] ON [dbo].[MP_PayPalTransactionItem]
(
	[TransactionId] ASC,
	[Type] ASC,
	[Status] ASC,
	[NetAmountAmount] ASC
)
INCLUDE ( 	[Payer],
	[Created],
	[FeeAmountCurrency],
	[FeeAmountAmount],
	[GrossAmountCurrency],
	[GrossAmountAmount],
	[NetAmountCurrency],
	[TimeZone],
	[PayerDisplayName],
	[PayPalTransactionId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [MP_PayPalTransactionItem_TI]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [MP_PayPalTransactionItem_TI] ON [dbo].[MP_PayPalTransactionItem]
(
	[TransactionId] ASC
)
INCLUDE ( 	[Id],
	[Created],
	[FeeAmountCurrency],
	[FeeAmountAmount],
	[GrossAmountCurrency],
	[GrossAmountAmount],
	[NetAmountCurrency],
	[NetAmountAmount],
	[TimeZone],
	[Type],
	[Status],
	[Payer],
	[PayerDisplayName],
	[PayPalTransactionId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_PayPalTransactionItem2_Type]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_PayPalTransactionItem2_Type] ON [dbo].[MP_PayPalTransactionItem2]
(
	[TransactionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [MP_PayPalTransactionItem2_TI]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [MP_PayPalTransactionItem2_TI] ON [dbo].[MP_PayPalTransactionItem2]
(
	[Created] ASC,
	[Type] ASC,
	[Status] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_EkmOrderCustomerMarketPlaceId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_EkmOrderCustomerMarketPlaceId] ON [dbo].[MP_PayPointOrder]
(
	[CustomerMarketPlaceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_PlayOrderCustomerMarketPlaceId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_PlayOrderCustomerMarketPlaceId] ON [dbo].[MP_PlayOrder]
(
	[CustomerMarketPlaceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_PlayOrderItemOrderId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_PlayOrderItemOrderId] ON [dbo].[MP_PlayOrderItem]
(
	[OrderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_SageExpenditureRequestId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_SageExpenditureRequestId] ON [dbo].[MP_SageExpenditure]
(
	[RequestId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_SageIncomeRequestId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_SageIncomeRequestId] ON [dbo].[MP_SageIncome]
(
	[RequestId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_SagePurchaseInvoiceRequestId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_SagePurchaseInvoiceRequestId] ON [dbo].[MP_SagePurchaseInvoice]
(
	[RequestId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_SagePurchaseInvoiceItemPurchaseInvoiceId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_SagePurchaseInvoiceItemPurchaseInvoiceId] ON [dbo].[MP_SagePurchaseInvoiceItem]
(
	[PurchaseInvoiceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_SageRequestCustomerMarketPlaceId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_SageRequestCustomerMarketPlaceId] ON [dbo].[MP_SageRequest]
(
	[CustomerMarketPlaceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_SageSalesInvoiceRequestId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_SageSalesInvoiceRequestId] ON [dbo].[MP_SageSalesInvoice]
(
	[RequestId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_SageSalesInvoiceItemInvoiceId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_SageSalesInvoiceItemInvoiceId] ON [dbo].[MP_SageSalesInvoiceItem]
(
	[InvoiceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_MP_ServiceLog_CustomerId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_ServiceLog_CustomerId] ON [dbo].[MP_ServiceLog]
(
	[CustomerId] ASC
)
INCLUDE ( 	[ServiceType],
	[InsertDate],
	[RequestData],
	[ResponseData]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [MP_ServiceLog_CustId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [MP_ServiceLog_CustId] ON [dbo].[MP_ServiceLog]
(
	[CustomerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IDX_VatReturnEntryNames]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IDX_VatReturnEntryNames] ON [dbo].[MP_VatReturnEntryNames]
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_VolusionOrderCustomerMarketPlaceId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_VolusionOrderCustomerMarketPlaceId] ON [dbo].[MP_VolusionOrder]
(
	[CustomerMarketPlaceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_VolusionOrderItemOrderId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_VolusionOrderItemOrderId] ON [dbo].[MP_VolusionOrderItem]
(
	[OrderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_YodleeOrderCustomerMarketPlaceId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_YodleeOrderCustomerMarketPlaceId] ON [dbo].[MP_YodleeOrder]
(
	[CustomerMarketPlaceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_YodleeOrderItemOrderId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_YodleeOrderItemOrderId] ON [dbo].[MP_YodleeOrderItem]
(
	[OrderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_MP_YodleeOrderItemBankTransactionOrderItemId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_YodleeOrderItemBankTransactionOrderItemId] ON [dbo].[MP_YodleeOrderItemBankTransaction]
(
	[OrderItemId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_MP_YodleeSearchWords]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_YodleeSearchWords] ON [dbo].[MP_YodleeSearchWords]
(
	[SearchWords] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [PacnetPaypointServiceLog_CustId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [PacnetPaypointServiceLog_CustId] ON [dbo].[PacnetPaypointServiceLog]
(
	[CustomerId] ASC
)
INCLUDE ( 	[InsertDate],
	[RequestType],
	[Status],
	[ErrorMessage]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [PostcodeServiceLog_CustId]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE NONCLUSTERED INDEX [PostcodeServiceLog_CustId] ON [dbo].[PostcodeServiceLog]
(
	[CustomerId] ASC
)
INCLUDE ( 	[ErrorMessage],
	[InsertDate],
	[RequestData],
	[RequestType],
	[ResponseData],
	[Status]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IDX_ReportArgumentName]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IDX_ReportArgumentName] ON [dbo].[ReportArgumentNames]
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IDX_ReportArgument]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IDX_ReportArgument] ON [dbo].[ReportArguments]
(
	[ReportArgumentNameId] ASC,
	[ReportId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
GO
/****** Object:  Index [IX_Strategy_Node]    Script Date: 04-Nov-13 5:03:46 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Strategy_Node] ON [dbo].[Strategy_Node]
(
	[Guid] ASC,
	[IsDeleted] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO

ALTER TABLE [dbo].[CashRequests] ADD  CONSTRAINT [DF_CashRequests_IsLoanType]  DEFAULT ((0)) FOR [IsLoanTypeSelectionAllowed]
GO
ALTER TABLE [dbo].[CashRequests] ADD  CONSTRAINT [DF_CashRequest_SourceID]  DEFAULT ((1)) FOR [LoanSourceID]
GO
ALTER TABLE [dbo].[CashRequests] ADD  CONSTRAINT [DF_CashRequests_Icrpsa]  DEFAULT ((1)) FOR [IsCustomerRepaymentPeriodSelectionAllowed]
GO
ALTER TABLE [dbo].[CompanyEmployeeCount] ADD  CONSTRAINT [DF_CompanyEmployeeCount_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[CompanyEmployeeCount] ADD  DEFAULT ((0)) FOR [TotalMonthlySalary]
GO
ALTER TABLE [dbo].[Control_History] ADD  CONSTRAINT [DF_CONTROL_HISTORY_CHANGESTIME]  DEFAULT (getdate()) FOR [CHANGESTIME]
GO
ALTER TABLE [dbo].[Customer] ADD  CONSTRAINT [DF_Customer_IsLoanType]  DEFAULT ((0)) FOR [IsLoanTypeSelectionAllowed]
GO
ALTER TABLE [dbo].[Customer] ADD  CONSTRAINT [DF_Customer_NumApproves]  DEFAULT ((0)) FOR [NumApproves]
GO
ALTER TABLE [dbo].[Customer] ADD  CONSTRAINT [DF_Customer_NumRejects]  DEFAULT ((0)) FOR [NumRejects]
GO
ALTER TABLE [dbo].[Customer] ADD  CONSTRAINT [DF_Customer_SystemCalculatedSum]  DEFAULT ((0)) FOR [SystemCalculatedSum]
GO
ALTER TABLE [dbo].[Customer] ADD  CONSTRAINT [DF_Customer_ManagerApprovedSum]  DEFAULT ((0)) FOR [ManagerApprovedSum]
GO
ALTER TABLE [dbo].[Customer] ADD  CONSTRAINT [DF_Customer_AmountTaken]  DEFAULT ((0)) FOR [AmountTaken]
GO
ALTER TABLE [dbo].[Customer] ADD  CONSTRAINT [DF_Customer_LastLoanAmount]  DEFAULT ((0)) FOR [LastLoanAmount]
GO
ALTER TABLE [dbo].[Customer] ADD  CONSTRAINT [DF_Customer_TotalPrincipalRepaid]  DEFAULT ((0)) FOR [TotalPrincipalRepaid]
GO
ALTER TABLE [dbo].[Customer] ADD  CONSTRAINT [DF_Customer_LastStatus]  DEFAULT ('N/A') FOR [LastStatus]
GO
ALTER TABLE [dbo].[Customer] ADD  CONSTRAINT [DF_Customer_AvoidAutomaticDescison]  DEFAULT ((0)) FOR [AvoidAutomaticDescison]
GO
ALTER TABLE [dbo].[Customer] ADD  CONSTRAINT [DF_Customer_FinancialAccounts]  DEFAULT ((0)) FOR [FinancialAccounts]
GO
ALTER TABLE [dbo].[Customer] ADD  CONSTRAINT [DF_Customer_IsOffline]  DEFAULT ((0)) FOR [IsOffline]
GO
ALTER TABLE [dbo].[CustomerInviteFriend] ADD  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[CustomerLoyaltyProgram] ADD  CONSTRAINT [DF_CustomerLoyaltyProgramDate]  DEFAULT (getdate()) FOR [ActionDate]
GO
ALTER TABLE [dbo].[CustomerRequestedLoan] ADD  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[CustomerScoringResult] ADD  DEFAULT (getdate()) FOR [ScoreDate]
GO
ALTER TABLE [dbo].[CustomerStatuses] ADD  DEFAULT ((0)) FOR [IsEnabled]
GO
ALTER TABLE [dbo].[CustomerStatuses] ADD  DEFAULT ((0)) FOR [IsWarning]
GO
ALTER TABLE [dbo].[DataSource_Requests] ADD  CONSTRAINT [DF_DataSource_Requests_CreationDate]  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[DataSource_Responses] ADD  CONSTRAINT [DF_DataSource_Responses_CreationDate]  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[Director] ADD  CONSTRAINT [DF_Director_Email]  DEFAULT ('') FOR [Email]
GO
ALTER TABLE [dbo].[Director] ADD  CONSTRAINT [DF_Director_Phone]  DEFAULT ('') FOR [Phone]
GO
ALTER TABLE [dbo].[EmailAccount] ADD  CONSTRAINT [DF_EmailAccount_StartDate]  DEFAULT (getdate()) FOR [StartDate]
GO
ALTER TABLE [dbo].[Export_Results] ADD  CONSTRAINT [DF_Export_Results_CreationDate]  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[Export_TemplatesList] ADD  CONSTRAINT [DF_Export_TemplatesList_UploadDate]  DEFAULT (getdate()) FOR [UploadDate]
GO
ALTER TABLE [dbo].[Loan] ADD  CONSTRAINT [DF_Loan_InterestRate]  DEFAULT ((0.06)) FOR [InterestRate]
GO
ALTER TABLE [dbo].[Loan] ADD  DEFAULT ((0)) FOR [Is14DaysLate]
GO
ALTER TABLE [dbo].[Loan] ADD  CONSTRAINT [DF_Loan_SourceID]  DEFAULT ((1)) FOR [LoanSourceID]
GO
ALTER TABLE [dbo].[LoanAgreementTemplate] ADD  DEFAULT ((1)) FOR [TemplateType]
GO
ALTER TABLE [dbo].[LoanInterestFreeze] ADD  CONSTRAINT [DF_LoanInterestFreeze_Active]  DEFAULT (getdate()) FOR [ActivationDate]
GO
ALTER TABLE [dbo].[LoanLegal] ADD  DEFAULT (getutcdate()) FOR [Created]
GO
ALTER TABLE [dbo].[LoanLegal] ADD  DEFAULT ((0)) FOR [CreditActAgreementAgreed]
GO
ALTER TABLE [dbo].[LoanLegal] ADD  DEFAULT ((0)) FOR [PreContractAgreementAgreed]
GO
ALTER TABLE [dbo].[LoanLegal] ADD  DEFAULT ((0)) FOR [PrivateCompanyLoanAgreementAgreed]
GO
ALTER TABLE [dbo].[LoanLegal] ADD  DEFAULT ((0)) FOR [GuarantyAgreementAgreed]
GO
ALTER TABLE [dbo].[LoanLegal] ADD  DEFAULT ((0)) FOR [EUAgreementAgreed]
GO
ALTER TABLE [dbo].[LoanScheduleTransaction] ADD  CONSTRAINT [DF_LoanScheduleTransaction]  DEFAULT (getdate()) FOR [Date]
GO
ALTER TABLE [dbo].[LoanScheduleTransactionBackFilled] ADD  CONSTRAINT [DF_LoanScheduleTransactionBackFilled_IsBad]  DEFAULT ((0)) FOR [IsBad]
GO
ALTER TABLE [dbo].[LoanSource] ADD  CONSTRAINT [DF_LoanSource_Icrpsa]  DEFAULT ((1)) FOR [IsCustomerRepaymentPeriodSelectionAllowed]
GO
ALTER TABLE [dbo].[LoanTransaction] ADD  CONSTRAINT [DF_LoanTran_Recon]  DEFAULT ('not tested') FOR [Reconciliation]
GO
ALTER TABLE [dbo].[LoanTransaction] ADD  CONSTRAINT [DF_LoanTransaction_MethodId]  DEFAULT ((0)) FOR [LoanTransactionMethodId]
GO
ALTER TABLE [dbo].[Log_ServiceAction] ADD  CONSTRAINT [DF_Log_ServiceAction_DateTime]  DEFAULT (getdate()) FOR [DateTime]
GO
ALTER TABLE [dbo].[MP_AnalyisisFunction] ADD  CONSTRAINT [DF_AnalyisisFunction_InternalId]  DEFAULT (newid()) FOR [InternalId]
GO
ALTER TABLE [dbo].[MP_AnalysisFunctionTimePeriod] ADD  CONSTRAINT [DF_AnalysisFunctionTimePeriod_InternalId]  DEFAULT (newid()) FOR [InternalId]
GO
ALTER TABLE [dbo].[MP_ChannelGrabberOrderItem] ADD  CONSTRAINT [DF_ChannelGrabberOrderItem_Expense]  DEFAULT ((0)) FOR [IsExpense]
GO
ALTER TABLE [dbo].[MP_CustomerMarketPlace] ADD  CONSTRAINT [DF_MP_CustomerMarketPlace_TokenExpired]  DEFAULT ((0)) FOR [TokenExpired]
GO
ALTER TABLE [dbo].[MP_MarketplaceType] ADD  CONSTRAINT [DF_MarketPlace_InternalId]  DEFAULT (newid()) FOR [InternalId]
GO
ALTER TABLE [dbo].[MP_TeraPeakOrderItem] ADD  CONSTRAINT [DF_MP_TeraPeakOrderItem_RangeMarket]  DEFAULT ((0)) FOR [RangeMarker]
GO
ALTER TABLE [dbo].[MP_ValueType] ADD  CONSTRAINT [DF_ValueType_InternalId]  DEFAULT (newid()) FOR [InternalId]
GO
ALTER TABLE [dbo].[MP_VatReturnRecords] ADD  CONSTRAINT [DF_VatReturnRecord_RegNo]  DEFAULT ((0)) FOR [RegistrationNo]
GO
ALTER TABLE [dbo].[PacNetBalance] ADD  DEFAULT ((0)) FOR [IsCredit]
GO
ALTER TABLE [dbo].[PacNetManualBalance] ADD  DEFAULT ((1)) FOR [Enabled]
GO
ALTER TABLE [dbo].[PaymentRollover] ADD  DEFAULT ((1)) FOR [MounthCount]
GO
ALTER TABLE [dbo].[ReportScheduler] ADD  CONSTRAINT [DF_RptSchedule_MonthToDate]  DEFAULT ((0)) FOR [IsMonthToDate]
GO
ALTER TABLE [dbo].[ReportUsers] ADD  CONSTRAINT [DF_ReportUsersPIsAdmin]  DEFAULT ((0)) FOR [IsAdmin]
GO
ALTER TABLE [dbo].[Security_AccountLog] ADD  CONSTRAINT [DF_Security_AccountLog_EventDate]  DEFAULT (getdate()) FOR [EventDate]
GO
ALTER TABLE [dbo].[Security_log4net] ADD  CONSTRAINT [DF_Security_log4net_EventDate]  DEFAULT (getdate()) FOR [EventDate]
GO
ALTER TABLE [dbo].[Security_Session] ADD  CONSTRAINT [DF_AppSession_State]  DEFAULT ((0)) FOR [State]
GO
ALTER TABLE [dbo].[Security_Session] ADD  CONSTRAINT [DF_AppSession_CreationDate]  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[Security_User] ADD  CONSTRAINT [DF_AppUser_CreationDate]  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[Security_User] ADD  CONSTRAINT [DF_AppUser_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Security_User] ADD  CONSTRAINT [DF_Security_User_PassSetTime]  DEFAULT (getdate()) FOR [PassSetTime]
GO
ALTER TABLE [dbo].[Strategy_Node] ADD  CONSTRAINT [DF_Strategy_Node_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Strategy_Node] ADD  DEFAULT ((0)) FOR [IsHardReaction]
GO
ALTER TABLE [dbo].[Strategy_Node] ADD  CONSTRAINT [DF_Strategy_Node_StartDate]  DEFAULT (getdate()) FOR [StartDate]
GO
ALTER TABLE [dbo].[Strategy_PublicName] ADD  CONSTRAINT [DF_PublicName_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Strategy_PublicSign] ADD  CONSTRAINT [DF_Strategy_PublicSign_CreationDate]  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[Strategy_Schedule] ADD  DEFAULT ((0)) FOR [ScheduleType]
GO
ALTER TABLE [dbo].[Strategy_Schedule] ADD  DEFAULT ((0)) FOR [ExecutionType]
GO
ALTER TABLE [dbo].[Strategy_ScheduleParam] ADD  CONSTRAINT [DF_ScheduleParam_CreationDate]  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[Strategy_Strategy] ADD  CONSTRAINT [DF_Strategy_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Strategy_Strategy] ADD  CONSTRAINT [DF_Strategy_State]  DEFAULT ((0)) FOR [State]
GO
ALTER TABLE [dbo].[Strategy_Strategy] ADD  CONSTRAINT [DF_Strategy_SubState]  DEFAULT ((1)) FOR [SubState]
GO
ALTER TABLE [dbo].[Strategy_Strategy] ADD  CONSTRAINT [DF_Strategy_CreationDate]  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[Strategy_Strategy] ADD  CONSTRAINT [DF_Strategy_Strategy_LastUpdateDate]  DEFAULT (getdate()) FOR [LastUpdateDate]
GO

ALTER TABLE [dbo].[YodleeBanks] ADD  DEFAULT ('abc') FOR [ParentBank]
GO
ALTER TABLE [dbo].[YodleeBanks] ADD  DEFAULT ((1)) FOR [Active]
GO
ALTER TABLE [dbo].[YodleeBanks] ADD  CONSTRAINT [DF_YodleeBanks_Image]  DEFAULT ((1)) FOR [Image]
GO

ALTER TABLE [dbo].[BusinessEntity_NodeRel]  WITH CHECK ADD  CONSTRAINT [FK_BE_Node] FOREIGN KEY([NodeId])
REFERENCES [dbo].[Strategy_Node] ([NodeId])
GO
ALTER TABLE [dbo].[BusinessEntity_NodeRel] CHECK CONSTRAINT [FK_BE_Node]
GO
ALTER TABLE [dbo].[BusinessEntity_NodeRel]  WITH CHECK ADD  CONSTRAINT [FK_BE_Node_BE] FOREIGN KEY([BusinessEntityId])
REFERENCES [dbo].[BusinessEntity] ([Id])
GO
ALTER TABLE [dbo].[BusinessEntity_NodeRel] CHECK CONSTRAINT [FK_BE_Node_BE]
GO
ALTER TABLE [dbo].[BusinessEntity_StrategyRel]  WITH CHECK ADD  CONSTRAINT [FK_BE_Strategy] FOREIGN KEY([BusinessEntityId])
REFERENCES [dbo].[BusinessEntity] ([Id])
GO
ALTER TABLE [dbo].[BusinessEntity_StrategyRel] CHECK CONSTRAINT [FK_BE_Strategy]
GO
ALTER TABLE [dbo].[BusinessEntity_StrategyRel]  WITH CHECK ADD  CONSTRAINT [FK_BE_Strategy_BE] FOREIGN KEY([StrategyId])
REFERENCES [dbo].[Strategy_Strategy] ([StrategyId])
GO
ALTER TABLE [dbo].[BusinessEntity_StrategyRel] CHECK CONSTRAINT [FK_BE_Strategy_BE]
GO
ALTER TABLE [dbo].[CardInfo]  WITH NOCHECK ADD  CONSTRAINT [FK_CardInfo_CardInfo] FOREIGN KEY([Id])
REFERENCES [dbo].[CardInfo] ([Id])
GO
ALTER TABLE [dbo].[CardInfo] CHECK CONSTRAINT [FK_CardInfo_CardInfo]
GO
ALTER TABLE [dbo].[CashRequests]  WITH CHECK ADD  CONSTRAINT [FK_CashRequest_SourceID] FOREIGN KEY([LoanSourceID])
REFERENCES [dbo].[LoanSource] ([LoanSourceID])
GO
ALTER TABLE [dbo].[CashRequests] CHECK CONSTRAINT [FK_CashRequest_SourceID]
GO
ALTER TABLE [dbo].[CashRequests]  WITH CHECK ADD  CONSTRAINT [FK_CashRequests_DiscountPlan] FOREIGN KEY([DiscountPlanId])
REFERENCES [dbo].[DiscountPlan] ([Id])
GO
ALTER TABLE [dbo].[CashRequests] CHECK CONSTRAINT [FK_CashRequests_DiscountPlan]
GO
ALTER TABLE [dbo].[CompanyEmployeeCount]  WITH CHECK ADD  CONSTRAINT [FK_CompanyEmployeeCount_Customer] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customer] ([Id])
GO
ALTER TABLE [dbo].[CompanyEmployeeCount] CHECK CONSTRAINT [FK_CompanyEmployeeCount_Customer]
GO
ALTER TABLE [dbo].[Customer]  WITH NOCHECK ADD  CONSTRAINT [FK_Customer_CardInfo] FOREIGN KEY([CurrentDebitCard])
REFERENCES [dbo].[CardInfo] ([Id])
GO
ALTER TABLE [dbo].[Customer] CHECK CONSTRAINT [FK_Customer_CardInfo]
GO
ALTER TABLE [dbo].[Customer]  WITH CHECK ADD  CONSTRAINT [FK_Customer_CustomerStatuses] FOREIGN KEY([CollectionStatus])
REFERENCES [dbo].[CustomerStatuses] ([Id])
GO
ALTER TABLE [dbo].[Customer] CHECK CONSTRAINT [FK_Customer_CustomerStatuses]
GO
ALTER TABLE [dbo].[CustomerInviteFriend]  WITH CHECK ADD  CONSTRAINT [FK_CustomerInviteFriend_Customer] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customer] ([Id])
GO
ALTER TABLE [dbo].[CustomerInviteFriend] CHECK CONSTRAINT [FK_CustomerInviteFriend_Customer]
GO
ALTER TABLE [dbo].[CustomerLoyaltyProgram]  WITH CHECK ADD  CONSTRAINT [FK_CustomerLoyaltyProgram] FOREIGN KEY([CustomerID])
REFERENCES [dbo].[Customer] ([Id])
GO
ALTER TABLE [dbo].[CustomerLoyaltyProgram] CHECK CONSTRAINT [FK_CustomerLoyaltyProgram]
GO
ALTER TABLE [dbo].[CustomerLoyaltyProgram]  WITH CHECK ADD  CONSTRAINT [FK_CustomerLoyaltyProgramAction] FOREIGN KEY([ActionID])
REFERENCES [dbo].[LoyaltyProgramActions] ([ActionID])
GO
ALTER TABLE [dbo].[CustomerLoyaltyProgram] CHECK CONSTRAINT [FK_CustomerLoyaltyProgramAction]
GO
ALTER TABLE [dbo].[CustomerLoyaltyProgram]  WITH CHECK ADD  CONSTRAINT [FK_CustomerLoyaltyProgramMP] FOREIGN KEY([CustomerMarketPlaceID])
REFERENCES [dbo].[MP_CustomerMarketPlace] ([Id])
GO
ALTER TABLE [dbo].[CustomerLoyaltyProgram] CHECK CONSTRAINT [FK_CustomerLoyaltyProgramMP]
GO
ALTER TABLE [dbo].[CustomerRelations]  WITH CHECK ADD  CONSTRAINT [FK_CustomerRelations_CRMActions] FOREIGN KEY([ActionId])
REFERENCES [dbo].[CRMActions] ([Id])
GO
ALTER TABLE [dbo].[CustomerRelations] CHECK CONSTRAINT [FK_CustomerRelations_CRMActions]
GO
ALTER TABLE [dbo].[CustomerRelations]  WITH CHECK ADD  CONSTRAINT [FK_CustomerRelations_CRMStatuses] FOREIGN KEY([StatusId])
REFERENCES [dbo].[CRMStatuses] ([Id])
GO
ALTER TABLE [dbo].[CustomerRelations] CHECK CONSTRAINT [FK_CustomerRelations_CRMStatuses]
GO
ALTER TABLE [dbo].[CustomerRequestedLoan]  WITH CHECK ADD  CONSTRAINT [FK_CustomerRequestedLoan_Customer] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customer] ([Id])
GO
ALTER TABLE [dbo].[CustomerRequestedLoan] CHECK CONSTRAINT [FK_CustomerRequestedLoan_Customer]
GO
ALTER TABLE [dbo].[CustomerRequestedLoan]  WITH CHECK ADD  CONSTRAINT [FK_CustomerRequestedLoan_CustomerReason] FOREIGN KEY([ReasonId])
REFERENCES [dbo].[CustomerReason] ([Id])
GO
ALTER TABLE [dbo].[CustomerRequestedLoan] CHECK CONSTRAINT [FK_CustomerRequestedLoan_CustomerReason]
GO
ALTER TABLE [dbo].[CustomerRequestedLoan]  WITH CHECK ADD  CONSTRAINT [FK_CustomerRequestedLoan_CustomerSourceOfRepayment] FOREIGN KEY([SourceOfRepaymentId])
REFERENCES [dbo].[CustomerSourceOfRepayment] ([Id])
GO
ALTER TABLE [dbo].[CustomerRequestedLoan] CHECK CONSTRAINT [FK_CustomerRequestedLoan_CustomerSourceOfRepayment]
GO
ALTER TABLE [dbo].[DataSource_StrategyRel]  WITH NOCHECK ADD  CONSTRAINT [FK_DSSTR_STRATEGY_STRATEGYID] FOREIGN KEY([StrategyId])
REFERENCES [dbo].[Strategy_Strategy] ([StrategyId])
GO
ALTER TABLE [dbo].[DataSource_StrategyRel] CHECK CONSTRAINT [FK_DSSTR_STRATEGY_STRATEGYID]
GO
ALTER TABLE [dbo].[ExperianConsentAgreement]  WITH CHECK ADD  CONSTRAINT [FK_ExperianConsentAgreement_Customer] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customer] ([Id])
GO
ALTER TABLE [dbo].[ExperianConsentAgreement] CHECK CONSTRAINT [FK_ExperianConsentAgreement_Customer]
GO
ALTER TABLE [dbo].[ExperianDefaultAccount]  WITH CHECK ADD  CONSTRAINT [FK_ExperianDefaultAccount_MP_ServiceLog] FOREIGN KEY([ServiceLogId])
REFERENCES [dbo].[MP_ServiceLog] ([Id])
GO
ALTER TABLE [dbo].[ExperianDefaultAccount] CHECK CONSTRAINT [FK_ExperianDefaultAccount_MP_ServiceLog]
GO
ALTER TABLE [dbo].[FraudAddress]  WITH CHECK ADD  CONSTRAINT [FK_FraudAddress_FraudUser] FOREIGN KEY([FraudUserId])
REFERENCES [dbo].[FraudUser] ([Id])
GO
ALTER TABLE [dbo].[FraudAddress] CHECK CONSTRAINT [FK_FraudAddress_FraudUser]
GO
ALTER TABLE [dbo].[FraudBankAccount]  WITH CHECK ADD  CONSTRAINT [FK_FraudBankAccount_FraudUser] FOREIGN KEY([FraudUserId])
REFERENCES [dbo].[FraudUser] ([Id])
GO
ALTER TABLE [dbo].[FraudBankAccount] CHECK CONSTRAINT [FK_FraudBankAccount_FraudUser]
GO
ALTER TABLE [dbo].[FraudCompany]  WITH CHECK ADD  CONSTRAINT [FK_FraudCompany_FraudUser] FOREIGN KEY([FraudUserId])
REFERENCES [dbo].[FraudUser] ([Id])
GO
ALTER TABLE [dbo].[FraudCompany] CHECK CONSTRAINT [FK_FraudCompany_FraudUser]
GO
ALTER TABLE [dbo].[FraudDetection]  WITH CHECK ADD  CONSTRAINT [FK_FraudDetection_Customer] FOREIGN KEY([CurrentCustomerId])
REFERENCES [dbo].[Customer] ([Id])
GO
ALTER TABLE [dbo].[FraudDetection] CHECK CONSTRAINT [FK_FraudDetection_Customer]
GO
ALTER TABLE [dbo].[FraudDetection]  WITH CHECK ADD  CONSTRAINT [FK_FraudDetection_Customer1] FOREIGN KEY([InternalCustomerId])
REFERENCES [dbo].[Customer] ([Id])
GO
ALTER TABLE [dbo].[FraudDetection] CHECK CONSTRAINT [FK_FraudDetection_Customer1]
GO
ALTER TABLE [dbo].[FraudDetection]  WITH CHECK ADD  CONSTRAINT [FK_FraudDetection_FraudUser] FOREIGN KEY([ExternalUserId])
REFERENCES [dbo].[FraudUser] ([Id])
GO
ALTER TABLE [dbo].[FraudDetection] CHECK CONSTRAINT [FK_FraudDetection_FraudUser]
GO
ALTER TABLE [dbo].[FraudEmail]  WITH CHECK ADD  CONSTRAINT [FK_FraudEmail_FraudUser] FOREIGN KEY([FraudUserId])
REFERENCES [dbo].[FraudUser] ([Id])
GO
ALTER TABLE [dbo].[FraudEmail] CHECK CONSTRAINT [FK_FraudEmail_FraudUser]
GO
ALTER TABLE [dbo].[FraudEmailDomain]  WITH CHECK ADD  CONSTRAINT [FK_FraudEmailDomain_FraudUser] FOREIGN KEY([FraudUserId])
REFERENCES [dbo].[FraudUser] ([Id])
GO
ALTER TABLE [dbo].[FraudEmailDomain] CHECK CONSTRAINT [FK_FraudEmailDomain_FraudUser]
GO
ALTER TABLE [dbo].[FraudPhone]  WITH CHECK ADD  CONSTRAINT [FK_FraudPhone_FraudPhone] FOREIGN KEY([FraudUserId])
REFERENCES [dbo].[FraudUser] ([Id])
GO
ALTER TABLE [dbo].[FraudPhone] CHECK CONSTRAINT [FK_FraudPhone_FraudPhone]
GO
ALTER TABLE [dbo].[FraudShop]  WITH CHECK ADD  CONSTRAINT [FK_FraudShop_FraudUser] FOREIGN KEY([FraudUserId])
REFERENCES [dbo].[FraudUser] ([Id])
GO
ALTER TABLE [dbo].[FraudShop] CHECK CONSTRAINT [FK_FraudShop_FraudUser]
GO
ALTER TABLE [dbo].[Loan]  WITH CHECK ADD  CONSTRAINT [FK_Loan_SourceID] FOREIGN KEY([LoanSourceID])
REFERENCES [dbo].[LoanSource] ([LoanSourceID])
GO
ALTER TABLE [dbo].[Loan] CHECK CONSTRAINT [FK_Loan_SourceID]
GO
ALTER TABLE [dbo].[LoanAgreement]  WITH CHECK ADD  CONSTRAINT [FK_LoanAgreement_LoanAgreementTemplate] FOREIGN KEY([TemplateId])
REFERENCES [dbo].[LoanAgreementTemplate] ([Id])
GO
ALTER TABLE [dbo].[LoanAgreement] CHECK CONSTRAINT [FK_LoanAgreement_LoanAgreementTemplate]
GO
ALTER TABLE [dbo].[LoanInterestFreeze]  WITH CHECK ADD  CONSTRAINT [FK_LoanInterestFreeze] FOREIGN KEY([LoanId])
REFERENCES [dbo].[Loan] ([Id])
GO
ALTER TABLE [dbo].[LoanInterestFreeze] CHECK CONSTRAINT [FK_LoanInterestFreeze]
GO
ALTER TABLE [dbo].[LoanLegal]  WITH CHECK ADD  CONSTRAINT [FK_LoanLegal_CashRequests] FOREIGN KEY([CashRequestsId])
REFERENCES [dbo].[CashRequests] ([Id])
GO
ALTER TABLE [dbo].[LoanLegal] CHECK CONSTRAINT [FK_LoanLegal_CashRequests]
GO
ALTER TABLE [dbo].[LoanOptions]  WITH CHECK ADD  CONSTRAINT [FK_LoanOptions_Loan] FOREIGN KEY([LoanId])
REFERENCES [dbo].[Loan] ([Id])
GO
ALTER TABLE [dbo].[LoanOptions] CHECK CONSTRAINT [FK_LoanOptions_Loan]
GO
ALTER TABLE [dbo].[LoanScheduleTransaction]  WITH CHECK ADD  CONSTRAINT [FK_LoanST_Loan] FOREIGN KEY([LoanID])
REFERENCES [dbo].[Loan] ([Id])
GO
ALTER TABLE [dbo].[LoanScheduleTransaction] CHECK CONSTRAINT [FK_LoanST_Loan]
GO
ALTER TABLE [dbo].[LoanScheduleTransaction]  WITH CHECK ADD  CONSTRAINT [FK_LoanST_Schedule] FOREIGN KEY([ScheduleID])
REFERENCES [dbo].[LoanSchedule] ([Id])
GO
ALTER TABLE [dbo].[LoanScheduleTransaction] CHECK CONSTRAINT [FK_LoanST_Schedule]
GO
ALTER TABLE [dbo].[LoanScheduleTransaction]  WITH CHECK ADD  CONSTRAINT [FK_LoanST_Transaction] FOREIGN KEY([TransactionID])
REFERENCES [dbo].[LoanTransaction] ([Id])
GO
ALTER TABLE [dbo].[LoanScheduleTransaction] CHECK CONSTRAINT [FK_LoanST_Transaction]
GO
ALTER TABLE [dbo].[LoanTransaction]  WITH CHECK ADD  CONSTRAINT [FK_LoanTransaction_MethodId] FOREIGN KEY([LoanTransactionMethodId])
REFERENCES [dbo].[LoanTransactionMethod] ([Id])
GO
ALTER TABLE [dbo].[LoanTransaction] CHECK CONSTRAINT [FK_LoanTransaction_MethodId]
GO
ALTER TABLE [dbo].[LoyaltyProgramActions]  WITH CHECK ADD  CONSTRAINT [FK_LoyaltyProgramActionType] FOREIGN KEY([ActionTypeID])
REFERENCES [dbo].[LoyaltyProgramActionTypes] ([ActionTypeID])
GO
ALTER TABLE [dbo].[LoyaltyProgramActions] CHECK CONSTRAINT [FK_LoyaltyProgramActionType]
GO
ALTER TABLE [dbo].[MailTemplateRelation]  WITH CHECK ADD  CONSTRAINT [FK_MailTemplateRelation_MandrillTemplateId] FOREIGN KEY([MandrillTemplateId])
REFERENCES [dbo].[MandrillTemplate] ([Id])
GO
ALTER TABLE [dbo].[MailTemplateRelation] CHECK CONSTRAINT [FK_MailTemplateRelation_MandrillTemplateId]
GO
ALTER TABLE [dbo].[MP_AmazonFeedback]  WITH CHECK ADD  CONSTRAINT [FK_MP_AmazonFeedback_MP_CustomerMarketPlace] FOREIGN KEY([CustomerMarketPlaceId])
REFERENCES [dbo].[MP_CustomerMarketPlace] ([Id])
GO
ALTER TABLE [dbo].[MP_AmazonFeedback] CHECK CONSTRAINT [FK_MP_AmazonFeedback_MP_CustomerMarketPlace]
GO
ALTER TABLE [dbo].[MP_AmazonFeedbackItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_AmazonFeedbackItem_MP_AmazonFeedback] FOREIGN KEY([AmazonFeedbackId])
REFERENCES [dbo].[MP_AmazonFeedback] ([Id])
GO
ALTER TABLE [dbo].[MP_AmazonFeedbackItem] CHECK CONSTRAINT [FK_MP_AmazonFeedbackItem_MP_AmazonFeedback]
GO
ALTER TABLE [dbo].[MP_AmazonFeedbackItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_AmazonFeedbackItem_MP_AnalysisFunctionTimePeriod] FOREIGN KEY([TimePeriodId])
REFERENCES [dbo].[MP_AnalysisFunctionTimePeriod] ([Id])
GO
ALTER TABLE [dbo].[MP_AmazonFeedbackItem] CHECK CONSTRAINT [FK_MP_AmazonFeedbackItem_MP_AnalysisFunctionTimePeriod]
GO
ALTER TABLE [dbo].[MP_AmazonOrder]  WITH CHECK ADD  CONSTRAINT [FK_AmazonOrder_CustomerMarketPlace] FOREIGN KEY([CustomerMarketPlaceId])
REFERENCES [dbo].[MP_CustomerMarketPlace] ([Id])
GO
ALTER TABLE [dbo].[MP_AmazonOrder] CHECK CONSTRAINT [FK_AmazonOrder_CustomerMarketPlace]
GO
ALTER TABLE [dbo].[MP_AmazonOrderItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_AmazonOrderItem_MP_AmazonOrder] FOREIGN KEY([AmazonOrderId])
REFERENCES [dbo].[MP_AmazonOrder] ([Id])
GO
ALTER TABLE [dbo].[MP_AmazonOrderItem] CHECK CONSTRAINT [FK_MP_AmazonOrderItem_MP_AmazonOrder]
GO
ALTER TABLE [dbo].[MP_AmazonOrderItem2]  WITH CHECK ADD  CONSTRAINT [FK_MP_AmazonOrderItem2_MP_AmazonOrder] FOREIGN KEY([AmazonOrderId])
REFERENCES [dbo].[MP_AmazonOrder] ([Id])
GO
ALTER TABLE [dbo].[MP_AmazonOrderItem2] CHECK CONSTRAINT [FK_MP_AmazonOrderItem2_MP_AmazonOrder]
GO
ALTER TABLE [dbo].[MP_AmazonOrderItem2Payment]  WITH CHECK ADD  CONSTRAINT [FK_MP_AmazonOrderItem2Payment_MP_AmazonOrderItem2] FOREIGN KEY([OrderItem2Id])
REFERENCES [dbo].[MP_AmazonOrderItem2] ([Id])
GO
ALTER TABLE [dbo].[MP_AmazonOrderItem2Payment] CHECK CONSTRAINT [FK_MP_AmazonOrderItem2Payment_MP_AmazonOrderItem2]
GO
ALTER TABLE [dbo].[MP_AmazonOrderItemDetail]  WITH CHECK ADD  CONSTRAINT [FK_MP_AmazonOrderItemDetail_MP_AmazonOrderItem2] FOREIGN KEY([OrderItem2Id])
REFERENCES [dbo].[MP_AmazonOrderItem2] ([Id])
GO
ALTER TABLE [dbo].[MP_AmazonOrderItemDetail] CHECK CONSTRAINT [FK_MP_AmazonOrderItemDetail_MP_AmazonOrderItem2]
GO
ALTER TABLE [dbo].[MP_AmazonOrderItemDetailCatgory]  WITH CHECK ADD  CONSTRAINT [FK_MP_AmazonOrderItemDetailCatgory_MP_AmazonOrderItemDetail] FOREIGN KEY([AmazonOrderItemDetailId])
REFERENCES [dbo].[MP_AmazonOrderItemDetail] ([Id])
GO
ALTER TABLE [dbo].[MP_AmazonOrderItemDetailCatgory] CHECK CONSTRAINT [FK_MP_AmazonOrderItemDetailCatgory_MP_AmazonOrderItemDetail]
GO
ALTER TABLE [dbo].[MP_AmazonOrderItemDetailCatgory]  WITH CHECK ADD  CONSTRAINT [FK_MP_AmazonOrderItemDetailCatgory_MP_EbayAmazonCategory] FOREIGN KEY([EbayAmazonCategoryId])
REFERENCES [dbo].[MP_EbayAmazonCategory] ([Id])
GO
ALTER TABLE [dbo].[MP_AmazonOrderItemDetailCatgory] CHECK CONSTRAINT [FK_MP_AmazonOrderItemDetailCatgory_MP_EbayAmazonCategory]
GO
ALTER TABLE [dbo].[MP_AnalyisisFunction]  WITH CHECK ADD  CONSTRAINT [FK_AnalyisisFunction_MarketPlace] FOREIGN KEY([MarketPlaceId])
REFERENCES [dbo].[MP_MarketplaceType] ([Id])
GO
ALTER TABLE [dbo].[MP_AnalyisisFunction] CHECK CONSTRAINT [FK_AnalyisisFunction_MarketPlace]
GO
ALTER TABLE [dbo].[MP_AnalyisisFunction]  WITH CHECK ADD  CONSTRAINT [FK_AnalyisisFunction_ValueType] FOREIGN KEY([ValueTypeId])
REFERENCES [dbo].[MP_ValueType] ([Id])
GO
ALTER TABLE [dbo].[MP_AnalyisisFunction] CHECK CONSTRAINT [FK_AnalyisisFunction_ValueType]
GO
ALTER TABLE [dbo].[MP_AnalyisisFunctionValues]  WITH CHECK ADD  CONSTRAINT [FK_AnalyisisFunctionValues_AnalyisisFunction] FOREIGN KEY([AnalyisisFunctionId])
REFERENCES [dbo].[MP_AnalyisisFunction] ([Id])
GO
ALTER TABLE [dbo].[MP_AnalyisisFunctionValues] CHECK CONSTRAINT [FK_AnalyisisFunctionValues_AnalyisisFunction]
GO
ALTER TABLE [dbo].[MP_AnalyisisFunctionValues]  WITH CHECK ADD  CONSTRAINT [FK_AnalyisisFunctionValues_AnalysisFunctionTimePeriod] FOREIGN KEY([AnalysisFunctionTimePeriodId])
REFERENCES [dbo].[MP_AnalysisFunctionTimePeriod] ([Id])
GO
ALTER TABLE [dbo].[MP_AnalyisisFunctionValues] CHECK CONSTRAINT [FK_AnalyisisFunctionValues_AnalysisFunctionTimePeriod]
GO
ALTER TABLE [dbo].[MP_AnalyisisFunctionValues]  WITH CHECK ADD  CONSTRAINT [FK_MP_AnalyisisFunctionValues_MP_CustomerMarketPlace] FOREIGN KEY([CustomerMarketPlaceId])
REFERENCES [dbo].[MP_CustomerMarketPlace] ([Id])
GO
ALTER TABLE [dbo].[MP_AnalyisisFunctionValues] CHECK CONSTRAINT [FK_MP_AnalyisisFunctionValues_MP_CustomerMarketPlace]
GO
ALTER TABLE [dbo].[MP_ChannelGrabberOrder]  WITH CHECK ADD  CONSTRAINT [FK_MP_ChannelGrabberOrder_MP_CustomerMarketPlace] FOREIGN KEY([CustomerMarketPlaceId])
REFERENCES [dbo].[MP_CustomerMarketPlace] ([Id])
GO
ALTER TABLE [dbo].[MP_ChannelGrabberOrder] CHECK CONSTRAINT [FK_MP_ChannelGrabberOrder_MP_CustomerMarketPlace]
GO
ALTER TABLE [dbo].[MP_ChannelGrabberOrderItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_ChannelGrabberOrderItem_MP_Order] FOREIGN KEY([OrderId])
REFERENCES [dbo].[MP_ChannelGrabberOrder] ([Id])
GO
ALTER TABLE [dbo].[MP_ChannelGrabberOrderItem] CHECK CONSTRAINT [FK_MP_ChannelGrabberOrderItem_MP_Order]
GO
ALTER TABLE [dbo].[MP_CurrencyRateHistory]  WITH CHECK ADD  CONSTRAINT [FK_MP_CurrencyRateHistory_MP_Currency] FOREIGN KEY([CurrencyId])
REFERENCES [dbo].[MP_Currency] ([Id])
GO
ALTER TABLE [dbo].[MP_CurrencyRateHistory] CHECK CONSTRAINT [FK_MP_CurrencyRateHistory_MP_Currency]
GO
ALTER TABLE [dbo].[MP_CustomerMarketPlace]  WITH CHECK ADD  CONSTRAINT [FK_CustomerMarketPlace_Customer] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customer] ([Id])
GO
ALTER TABLE [dbo].[MP_CustomerMarketPlace] CHECK CONSTRAINT [FK_CustomerMarketPlace_Customer]
GO
ALTER TABLE [dbo].[MP_CustomerMarketPlace]  WITH CHECK ADD  CONSTRAINT [FK_CustomerMarketPlace_MarketPlace] FOREIGN KEY([MarketPlaceId])
REFERENCES [dbo].[MP_MarketplaceType] ([Id])
GO
ALTER TABLE [dbo].[MP_CustomerMarketPlace] CHECK CONSTRAINT [FK_CustomerMarketPlace_MarketPlace]
GO
ALTER TABLE [dbo].[MP_CustomerMarketPlace]  WITH CHECK ADD  CONSTRAINT [FK_MP_CustomerMarketPlace_MP_AmazonMarketplaceType] FOREIGN KEY([AmazonMarketPlaceId])
REFERENCES [dbo].[MP_AmazonMarketplaceType] ([Id])
GO
ALTER TABLE [dbo].[MP_CustomerMarketPlace] CHECK CONSTRAINT [FK_MP_CustomerMarketPlace_MP_AmazonMarketplaceType]
GO
ALTER TABLE [dbo].[MP_CustomerMarketplaceUpdatingActionLog]  WITH CHECK ADD  CONSTRAINT [FK_MP_CustomerMarketplaceUpdatingActionLog_MP_CustomerMarketPlaceUpdatingHistory] FOREIGN KEY([CustomerMarketplaceUpdatingHistoryRecordId])
REFERENCES [dbo].[MP_CustomerMarketPlaceUpdatingHistory] ([Id])
GO
ALTER TABLE [dbo].[MP_CustomerMarketplaceUpdatingActionLog] CHECK CONSTRAINT [FK_MP_CustomerMarketplaceUpdatingActionLog_MP_CustomerMarketPlaceUpdatingHistory]
GO
ALTER TABLE [dbo].[MP_CustomerMarketplaceUpdatingCounter]  WITH CHECK ADD  CONSTRAINT [FK_MP_CustomerMarketplaceUpdatingCounter_MP_CustomerMarketplaceUpdatingActionLog] FOREIGN KEY([CustomerMarketplaceUpdatingActionLogId])
REFERENCES [dbo].[MP_CustomerMarketplaceUpdatingActionLog] ([Id])
GO
ALTER TABLE [dbo].[MP_CustomerMarketplaceUpdatingCounter] CHECK CONSTRAINT [FK_MP_CustomerMarketplaceUpdatingCounter_MP_CustomerMarketplaceUpdatingActionLog]
GO
ALTER TABLE [dbo].[MP_CustomerMarketPlaceUpdatingHistory]  WITH CHECK ADD  CONSTRAINT [FK_MP_CustomerMarketPlaceUpdatingHistory_MP_CustomerMarketPlace] FOREIGN KEY([CustomerMarketPlaceId])
REFERENCES [dbo].[MP_CustomerMarketPlace] ([Id])
GO
ALTER TABLE [dbo].[MP_CustomerMarketPlaceUpdatingHistory] CHECK CONSTRAINT [FK_MP_CustomerMarketPlaceUpdatingHistory_MP_CustomerMarketPlace]
GO
ALTER TABLE [dbo].[MP_EbayAmazonCategory]  WITH CHECK ADD  CONSTRAINT [FK_MP_EbayAmazonCategory_MP_EbayAmazonCategory] FOREIGN KEY([ParentId])
REFERENCES [dbo].[MP_EbayAmazonCategory] ([Id])
GO
ALTER TABLE [dbo].[MP_EbayAmazonCategory] CHECK CONSTRAINT [FK_MP_EbayAmazonCategory_MP_EbayAmazonCategory]
GO
ALTER TABLE [dbo].[MP_EbayAmazonCategory]  WITH CHECK ADD  CONSTRAINT [FK_MP_EbayAmazonCategory_MP_MarketplaceType] FOREIGN KEY([MarketplaceTypeId])
REFERENCES [dbo].[MP_MarketplaceType] ([Id])
GO
ALTER TABLE [dbo].[MP_EbayAmazonCategory] CHECK CONSTRAINT [FK_MP_EbayAmazonCategory_MP_MarketplaceType]
GO
ALTER TABLE [dbo].[MP_EbayAmazonInventory]  WITH CHECK ADD  CONSTRAINT [FK_MP_Inventory_MP_CustomerMarketPlace] FOREIGN KEY([CustomerMarketPlaceId])
REFERENCES [dbo].[MP_CustomerMarketPlace] ([Id])
GO
ALTER TABLE [dbo].[MP_EbayAmazonInventory] CHECK CONSTRAINT [FK_MP_Inventory_MP_CustomerMarketPlace]
GO
ALTER TABLE [dbo].[MP_EbayAmazonInventoryItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_InventoryItem_MP_Inventory] FOREIGN KEY([InventoryId])
REFERENCES [dbo].[MP_EbayAmazonInventory] ([Id])
GO
ALTER TABLE [dbo].[MP_EbayAmazonInventoryItem] CHECK CONSTRAINT [FK_MP_InventoryItem_MP_Inventory]
GO
ALTER TABLE [dbo].[MP_EbayExternalTransaction]  WITH CHECK ADD  CONSTRAINT [FK_MP_EbayExternalTransaction_MP_EbayOrderItem] FOREIGN KEY([OrderItemId])
REFERENCES [dbo].[MP_EbayOrderItem] ([Id])
GO
ALTER TABLE [dbo].[MP_EbayExternalTransaction] CHECK CONSTRAINT [FK_MP_EbayExternalTransaction_MP_EbayOrderItem]
GO
ALTER TABLE [dbo].[MP_EbayFeedbackItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_EbayFeedbackItem_MP_AnalysisFunctionTimePeriod] FOREIGN KEY([TimePeriodId])
REFERENCES [dbo].[MP_AnalysisFunctionTimePeriod] ([Id])
GO
ALTER TABLE [dbo].[MP_EbayFeedbackItem] CHECK CONSTRAINT [FK_MP_EbayFeedbackItem_MP_AnalysisFunctionTimePeriod]
GO
ALTER TABLE [dbo].[MP_EbayFeedbackItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_EbayFeedbackItem_MP_EbayFeedback] FOREIGN KEY([EbayFeedbackId])
REFERENCES [dbo].[MP_EbayFeedback] ([Id])
GO
ALTER TABLE [dbo].[MP_EbayFeedbackItem] CHECK CONSTRAINT [FK_MP_EbayFeedbackItem_MP_EbayFeedback]
GO
ALTER TABLE [dbo].[MP_EbayOrder]  WITH CHECK ADD  CONSTRAINT [FK_MP_Order_MP_CustomerMarketPlace] FOREIGN KEY([CustomerMarketPlaceId])
REFERENCES [dbo].[MP_CustomerMarketPlace] ([Id])
GO
ALTER TABLE [dbo].[MP_EbayOrder] CHECK CONSTRAINT [FK_MP_Order_MP_CustomerMarketPlace]
GO
ALTER TABLE [dbo].[MP_EbayOrderItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_OrderItem_MP_EbayUserAddressData] FOREIGN KEY([ShippingAddressId])
REFERENCES [dbo].[MP_EbayUserAddressData] ([Id])
GO
ALTER TABLE [dbo].[MP_EbayOrderItem] CHECK CONSTRAINT [FK_MP_OrderItem_MP_EbayUserAddressData]
GO
ALTER TABLE [dbo].[MP_EbayOrderItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_OrderItem_MP_Order] FOREIGN KEY([OrderId])
REFERENCES [dbo].[MP_EbayOrder] ([Id])
GO
ALTER TABLE [dbo].[MP_EbayOrderItem] CHECK CONSTRAINT [FK_MP_OrderItem_MP_Order]
GO
ALTER TABLE [dbo].[MP_EBayOrderItemDetail]  WITH CHECK ADD  CONSTRAINT [FK_MP_EBayOrderItemDetail_MP_EbayAmazonCategory] FOREIGN KEY([SecondaryCategoryId])
REFERENCES [dbo].[MP_EbayAmazonCategory] ([Id])
GO
ALTER TABLE [dbo].[MP_EBayOrderItemDetail] CHECK CONSTRAINT [FK_MP_EBayOrderItemDetail_MP_EbayAmazonCategory]
GO
ALTER TABLE [dbo].[MP_EBayOrderItemDetail]  WITH CHECK ADD  CONSTRAINT [FK_MP_EBayOrderItemInfo_MP_EbayCategory_FreeAddedCategory] FOREIGN KEY([FreeAddedCategoryId])
REFERENCES [dbo].[MP_EbayAmazonCategory] ([Id])
GO
ALTER TABLE [dbo].[MP_EBayOrderItemDetail] CHECK CONSTRAINT [FK_MP_EBayOrderItemInfo_MP_EbayCategory_FreeAddedCategory]
GO
ALTER TABLE [dbo].[MP_EBayOrderItemDetail]  WITH CHECK ADD  CONSTRAINT [FK_MP_EBayOrderItemInfo_MP_EbayCategory_Primary] FOREIGN KEY([PrimaryCategoryId])
REFERENCES [dbo].[MP_EbayAmazonCategory] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[MP_EBayOrderItemDetail] CHECK CONSTRAINT [FK_MP_EBayOrderItemInfo_MP_EbayCategory_Primary]
GO
ALTER TABLE [dbo].[MP_EBayOrderItemDetail]  WITH CHECK ADD  CONSTRAINT [FK_MP_EBayOrderItemInfo_MP_EbayCategory_Secondary] FOREIGN KEY([SecondaryCategoryId])
REFERENCES [dbo].[MP_EbayAmazonCategory] ([Id])
GO
ALTER TABLE [dbo].[MP_EBayOrderItemDetail] CHECK CONSTRAINT [FK_MP_EBayOrderItemInfo_MP_EbayCategory_Secondary]
GO
ALTER TABLE [dbo].[MP_EbayRaitingItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_EbayRaitingItem_MP_AnalysisFunctionTimePeriod] FOREIGN KEY([TimePeriodId])
REFERENCES [dbo].[MP_AnalysisFunctionTimePeriod] ([Id])
GO
ALTER TABLE [dbo].[MP_EbayRaitingItem] CHECK CONSTRAINT [FK_MP_EbayRaitingItem_MP_AnalysisFunctionTimePeriod]
GO
ALTER TABLE [dbo].[MP_EbayRaitingItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_EbayRaitingItem_MP_EbayFeedback] FOREIGN KEY([EbayFeedbackId])
REFERENCES [dbo].[MP_EbayFeedback] ([Id])
GO
ALTER TABLE [dbo].[MP_EbayRaitingItem] CHECK CONSTRAINT [FK_MP_EbayRaitingItem_MP_EbayFeedback]
GO
ALTER TABLE [dbo].[MP_EbayTransaction]  WITH CHECK ADD  CONSTRAINT [FK_MP_EbayTransaction_MP_EBayOrderItemInfo] FOREIGN KEY([ItemInfoId])
REFERENCES [dbo].[MP_EBayOrderItemDetail] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[MP_EbayTransaction] CHECK CONSTRAINT [FK_MP_EbayTransaction_MP_EBayOrderItemInfo]
GO
ALTER TABLE [dbo].[MP_EbayTransaction]  WITH CHECK ADD  CONSTRAINT [FK_MP_EbayTransaction_MP_OrderItem] FOREIGN KEY([OrderItemId])
REFERENCES [dbo].[MP_EbayOrderItem] ([Id])
GO
ALTER TABLE [dbo].[MP_EbayTransaction] CHECK CONSTRAINT [FK_MP_EbayTransaction_MP_OrderItem]
GO
ALTER TABLE [dbo].[MP_EbayUserAccountData]  WITH CHECK ADD  CONSTRAINT [FK_MP_EbayUserAccountData_MP_EbayUserAccountData] FOREIGN KEY([CustomerMarketPlaceId])
REFERENCES [dbo].[MP_CustomerMarketPlace] ([Id])
GO
ALTER TABLE [dbo].[MP_EbayUserAccountData] CHECK CONSTRAINT [FK_MP_EbayUserAccountData_MP_EbayUserAccountData]
GO
ALTER TABLE [dbo].[MP_EbayUserAdditionalAccountData]  WITH CHECK ADD  CONSTRAINT [FK_MP_EbayUserAdditionalAccountData_MP_EbayUserAccountData] FOREIGN KEY([EbayUserAccountDataId])
REFERENCES [dbo].[MP_EbayUserAccountData] ([Id])
GO
ALTER TABLE [dbo].[MP_EbayUserAdditionalAccountData] CHECK CONSTRAINT [FK_MP_EbayUserAdditionalAccountData_MP_EbayUserAccountData]
GO
ALTER TABLE [dbo].[MP_EbayUserData]  WITH CHECK ADD  CONSTRAINT [FK_MP_EbayUserData_MP_CustomerMarketPlace] FOREIGN KEY([CustomerMarketPlaceId])
REFERENCES [dbo].[MP_CustomerMarketPlace] ([Id])
GO
ALTER TABLE [dbo].[MP_EbayUserData] CHECK CONSTRAINT [FK_MP_EbayUserData_MP_CustomerMarketPlace]
GO
ALTER TABLE [dbo].[MP_EbayUserData]  WITH CHECK ADD  CONSTRAINT [FK_MP_EbayUserData_MP_EbayUserAddressData] FOREIGN KEY([RegistrationAddressId])
REFERENCES [dbo].[MP_EbayUserAddressData] ([Id])
GO
ALTER TABLE [dbo].[MP_EbayUserData] CHECK CONSTRAINT [FK_MP_EbayUserData_MP_EbayUserAddressData]
GO
ALTER TABLE [dbo].[MP_EbayUserData]  WITH CHECK ADD  CONSTRAINT [FK_MP_EbayUserData_MP_EbayUserAddressData1] FOREIGN KEY([SellerInfoSellerPaymentAddressId])
REFERENCES [dbo].[MP_EbayUserAddressData] ([Id])
GO
ALTER TABLE [dbo].[MP_EbayUserData] CHECK CONSTRAINT [FK_MP_EbayUserData_MP_EbayUserAddressData1]
GO
ALTER TABLE [dbo].[MP_EkmOrder]  WITH CHECK ADD  CONSTRAINT [FK_MP_EkmOrder_MP_CustomerMarketPlace] FOREIGN KEY([CustomerMarketPlaceId])
REFERENCES [dbo].[MP_CustomerMarketPlace] ([Id])
GO
ALTER TABLE [dbo].[MP_EkmOrder] CHECK CONSTRAINT [FK_MP_EkmOrder_MP_CustomerMarketPlace]
GO
ALTER TABLE [dbo].[MP_EkmOrderItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_EkmOrderItem_MP_Order] FOREIGN KEY([OrderId])
REFERENCES [dbo].[MP_EkmOrder] ([Id])
GO
ALTER TABLE [dbo].[MP_EkmOrderItem] CHECK CONSTRAINT [FK_MP_EkmOrderItem_MP_Order]
GO
ALTER TABLE [dbo].[MP_FreeAgentCompany]  WITH CHECK ADD  CONSTRAINT [FK_MP_FreeAgentCompany_MP_FreeAgentRequest] FOREIGN KEY([RequestId])
REFERENCES [dbo].[MP_FreeAgentRequest] ([Id])
GO
ALTER TABLE [dbo].[MP_FreeAgentCompany] CHECK CONSTRAINT [FK_MP_FreeAgentCompany_MP_FreeAgentRequest]
GO
ALTER TABLE [dbo].[MP_FreeAgentExpense]  WITH CHECK ADD  CONSTRAINT [FK_MP_FreeAgentExpense_MP_FreeAgentExpenseCategory] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[MP_FreeAgentExpenseCategory] ([Id])
GO
ALTER TABLE [dbo].[MP_FreeAgentExpense] CHECK CONSTRAINT [FK_MP_FreeAgentExpense_MP_FreeAgentExpenseCategory]
GO
ALTER TABLE [dbo].[MP_FreeAgentExpense]  WITH CHECK ADD  CONSTRAINT [FK_MP_FreeAgentExpense_MP_FreeAgentRequest] FOREIGN KEY([RequestId])
REFERENCES [dbo].[MP_FreeAgentRequest] ([Id])
GO
ALTER TABLE [dbo].[MP_FreeAgentExpense] CHECK CONSTRAINT [FK_MP_FreeAgentExpense_MP_FreeAgentRequest]
GO
ALTER TABLE [dbo].[MP_FreeAgentInvoice]  WITH CHECK ADD  CONSTRAINT [FK_MP_FreeAgentInvoice_MP_FreeAgentRequest] FOREIGN KEY([RequestId])
REFERENCES [dbo].[MP_FreeAgentRequest] ([Id])
GO
ALTER TABLE [dbo].[MP_FreeAgentInvoice] CHECK CONSTRAINT [FK_MP_FreeAgentInvoice_MP_FreeAgentRequest]
GO
ALTER TABLE [dbo].[MP_FreeAgentInvoiceItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_FreeAgentInvoiceItem_MP_FreeAgentInvoice] FOREIGN KEY([InvoiceId])
REFERENCES [dbo].[MP_FreeAgentInvoice] ([Id])
GO
ALTER TABLE [dbo].[MP_FreeAgentInvoiceItem] CHECK CONSTRAINT [FK_MP_FreeAgentInvoiceItem_MP_FreeAgentInvoice]
GO
ALTER TABLE [dbo].[MP_FreeAgentRequest]  WITH CHECK ADD  CONSTRAINT [FK_MP_FreeAgentRequest_MP_CustomerMarketPlace] FOREIGN KEY([CustomerMarketPlaceId])
REFERENCES [dbo].[MP_CustomerMarketPlace] ([Id])
GO
ALTER TABLE [dbo].[MP_FreeAgentRequest] CHECK CONSTRAINT [FK_MP_FreeAgentRequest_MP_CustomerMarketPlace]
GO
ALTER TABLE [dbo].[MP_FreeAgentUsers]  WITH CHECK ADD  CONSTRAINT [FK_MP_FreeAgentUsers_MP_FreeAgentRequest] FOREIGN KEY([RequestId])
REFERENCES [dbo].[MP_FreeAgentRequest] ([Id])
GO
ALTER TABLE [dbo].[MP_FreeAgentUsers] CHECK CONSTRAINT [FK_MP_FreeAgentUsers_MP_FreeAgentRequest]
GO
ALTER TABLE [dbo].[MP_MarketplaceType]  WITH CHECK ADD  CONSTRAINT [FK_MP_MarketplaceType_MP_MarketplaceGroup] FOREIGN KEY([GroupId])
REFERENCES [dbo].[MP_MarketplaceGroup] ([Id])
GO
ALTER TABLE [dbo].[MP_MarketplaceType] CHECK CONSTRAINT [FK_MP_MarketplaceType_MP_MarketplaceGroup]
GO
ALTER TABLE [dbo].[MP_PayPalPersonalInfo]  WITH CHECK ADD  CONSTRAINT [FK_MP_PersonalInfoItem_MP_CustomerMarketPlace] FOREIGN KEY([CustomerMarketPlaceId])
REFERENCES [dbo].[MP_CustomerMarketPlace] ([Id])
GO
ALTER TABLE [dbo].[MP_PayPalPersonalInfo] CHECK CONSTRAINT [FK_MP_PersonalInfoItem_MP_CustomerMarketPlace]
GO
ALTER TABLE [dbo].[MP_PayPalTransaction]  WITH CHECK ADD  CONSTRAINT [FK_MP_Transaction_MP_CustomerMarketPlace] FOREIGN KEY([CustomerMarketPlaceId])
REFERENCES [dbo].[MP_CustomerMarketPlace] ([Id])
GO
ALTER TABLE [dbo].[MP_PayPalTransaction] CHECK CONSTRAINT [FK_MP_Transaction_MP_CustomerMarketPlace]
GO
ALTER TABLE [dbo].[MP_PayPalTransactionItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_TransactionItem_MP_Transaction] FOREIGN KEY([TransactionId])
REFERENCES [dbo].[MP_PayPalTransaction] ([Id])
GO
ALTER TABLE [dbo].[MP_PayPalTransactionItem] CHECK CONSTRAINT [FK_MP_TransactionItem_MP_Transaction]
GO
ALTER TABLE [dbo].[MP_PayPalTransactionItem2]  WITH CHECK ADD  CONSTRAINT [FK_MP_TransactionItem2_MP_Currency] FOREIGN KEY([CurrencyId])
REFERENCES [dbo].[MP_Currency] ([Id])
GO
ALTER TABLE [dbo].[MP_PayPalTransactionItem2] CHECK CONSTRAINT [FK_MP_TransactionItem2_MP_Currency]
GO
ALTER TABLE [dbo].[MP_PayPalTransactionItem2]  WITH CHECK ADD  CONSTRAINT [FK_MP_TransactionItem2_MP_Transaction] FOREIGN KEY([TransactionId])
REFERENCES [dbo].[MP_PayPalTransaction] ([Id])
GO
ALTER TABLE [dbo].[MP_PayPalTransactionItem2] CHECK CONSTRAINT [FK_MP_TransactionItem2_MP_Transaction]
GO
ALTER TABLE [dbo].[MP_PayPointOrder]  WITH CHECK ADD  CONSTRAINT [FK_MP_PayPointOrder_MP_CustomerMarketPlace] FOREIGN KEY([CustomerMarketPlaceId])
REFERENCES [dbo].[MP_CustomerMarketPlace] ([Id])
GO
ALTER TABLE [dbo].[MP_PayPointOrder] CHECK CONSTRAINT [FK_MP_PayPointOrder_MP_CustomerMarketPlace]
GO
ALTER TABLE [dbo].[MP_PayPointOrderItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_PayPointOrderItem_MP_PayPointOrder] FOREIGN KEY([OrderId])
REFERENCES [dbo].[MP_PayPointOrder] ([Id])
GO
ALTER TABLE [dbo].[MP_PayPointOrderItem] CHECK CONSTRAINT [FK_MP_PayPointOrderItem_MP_PayPointOrder]
GO
ALTER TABLE [dbo].[MP_PlayOrder]  WITH CHECK ADD  CONSTRAINT [FK_MP_PlayOrder_MP_CustomerMarketPlace] FOREIGN KEY([CustomerMarketPlaceId])
REFERENCES [dbo].[MP_CustomerMarketPlace] ([Id])
GO
ALTER TABLE [dbo].[MP_PlayOrder] CHECK CONSTRAINT [FK_MP_PlayOrder_MP_CustomerMarketPlace]
GO
ALTER TABLE [dbo].[MP_PlayOrderItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_PlayOrderItem_MP_Order] FOREIGN KEY([OrderId])
REFERENCES [dbo].[MP_PlayOrder] ([Id])
GO
ALTER TABLE [dbo].[MP_PlayOrderItem] CHECK CONSTRAINT [FK_MP_PlayOrderItem_MP_Order]
GO
ALTER TABLE [dbo].[MP_RtiTaxMonthEntries]  WITH CHECK ADD  CONSTRAINT [FK_RtiTaxMonthEntries_Record] FOREIGN KEY([RecordId])
REFERENCES [dbo].[MP_RtiTaxMonthRecords] ([Id])
GO
ALTER TABLE [dbo].[MP_RtiTaxMonthEntries] CHECK CONSTRAINT [FK_RtiTaxMonthEntries_Record]
GO
ALTER TABLE [dbo].[MP_RtiTaxMonthRecords]  WITH CHECK ADD  CONSTRAINT [FK_RtiTaxMonthRecord_MP] FOREIGN KEY([CustomerMarketPlaceId])
REFERENCES [dbo].[MP_CustomerMarketPlace] ([Id])
GO
ALTER TABLE [dbo].[MP_RtiTaxMonthRecords] CHECK CONSTRAINT [FK_RtiTaxMonthRecord_MP]
GO
ALTER TABLE [dbo].[MP_SageExpenditure]  WITH CHECK ADD  CONSTRAINT [FK_MP_SageExpenditure_MP_SageRequest] FOREIGN KEY([RequestId])
REFERENCES [dbo].[MP_SageRequest] ([Id])
GO
ALTER TABLE [dbo].[MP_SageExpenditure] CHECK CONSTRAINT [FK_MP_SageExpenditure_MP_SageRequest]
GO
ALTER TABLE [dbo].[MP_SageIncome]  WITH CHECK ADD  CONSTRAINT [FK_MP_SageIncome_MP_SageRequest] FOREIGN KEY([RequestId])
REFERENCES [dbo].[MP_SageRequest] ([Id])
GO
ALTER TABLE [dbo].[MP_SageIncome] CHECK CONSTRAINT [FK_MP_SageIncome_MP_SageRequest]
GO
ALTER TABLE [dbo].[MP_SagePurchaseInvoice]  WITH CHECK ADD  CONSTRAINT [FK_MP_SagePurchaseInvoice_MP_SageRequest] FOREIGN KEY([RequestId])
REFERENCES [dbo].[MP_SageRequest] ([Id])
GO
ALTER TABLE [dbo].[MP_SagePurchaseInvoice] CHECK CONSTRAINT [FK_MP_SagePurchaseInvoice_MP_SageRequest]
GO
ALTER TABLE [dbo].[MP_SagePurchaseInvoiceItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_SagePurchaseInvoiceItem_MP_SagePurchaseInvoice] FOREIGN KEY([PurchaseInvoiceId])
REFERENCES [dbo].[MP_SagePurchaseInvoice] ([Id])
GO
ALTER TABLE [dbo].[MP_SagePurchaseInvoiceItem] CHECK CONSTRAINT [FK_MP_SagePurchaseInvoiceItem_MP_SagePurchaseInvoice]
GO
ALTER TABLE [dbo].[MP_SageRequest]  WITH CHECK ADD  CONSTRAINT [FK_MP_SageRequest_MP_CustomerMarketPlace] FOREIGN KEY([CustomerMarketPlaceId])
REFERENCES [dbo].[MP_CustomerMarketPlace] ([Id])
GO
ALTER TABLE [dbo].[MP_SageRequest] CHECK CONSTRAINT [FK_MP_SageRequest_MP_CustomerMarketPlace]
GO
ALTER TABLE [dbo].[MP_SageSalesInvoice]  WITH CHECK ADD  CONSTRAINT [FK_MP_SageSalesInvoice_MP_SageRequest] FOREIGN KEY([RequestId])
REFERENCES [dbo].[MP_SageRequest] ([Id])
GO
ALTER TABLE [dbo].[MP_SageSalesInvoice] CHECK CONSTRAINT [FK_MP_SageSalesInvoice_MP_SageRequest]
GO
ALTER TABLE [dbo].[MP_SageSalesInvoiceItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_SageSalesInvoiceItem_MP_SageInvoice] FOREIGN KEY([InvoiceId])
REFERENCES [dbo].[MP_SageSalesInvoice] ([Id])
GO
ALTER TABLE [dbo].[MP_SageSalesInvoiceItem] CHECK CONSTRAINT [FK_MP_SageSalesInvoiceItem_MP_SageInvoice]
GO
ALTER TABLE [dbo].[MP_SageSalesInvoiceItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_SageSalesInvoiceItem_MP_SageSalesInvoice] FOREIGN KEY([InvoiceId])
REFERENCES [dbo].[MP_SageSalesInvoice] ([Id])
GO
ALTER TABLE [dbo].[MP_SageSalesInvoiceItem] CHECK CONSTRAINT [FK_MP_SageSalesInvoiceItem_MP_SageSalesInvoice]
GO
ALTER TABLE [dbo].[MP_TeraPeakCategoryStatistics]  WITH CHECK ADD  CONSTRAINT [FK_MP_TeraPeakCategoryStatistics_MP_TeraPeakCategory] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[MP_TeraPeakCategory] ([Id])
GO
ALTER TABLE [dbo].[MP_TeraPeakCategoryStatistics] CHECK CONSTRAINT [FK_MP_TeraPeakCategoryStatistics_MP_TeraPeakCategory]
GO
ALTER TABLE [dbo].[MP_TeraPeakOrder]  WITH CHECK ADD  CONSTRAINT [FK_MP_TeraPeakOrder_MP_CustomerMarketPlaceUpdatingHistory] FOREIGN KEY([CustomerMarketPlaceUpdatingHistoryRecordId])
REFERENCES [dbo].[MP_CustomerMarketPlaceUpdatingHistory] ([Id])
GO
ALTER TABLE [dbo].[MP_TeraPeakOrder] CHECK CONSTRAINT [FK_MP_TeraPeakOrder_MP_CustomerMarketPlaceUpdatingHistory]
GO
ALTER TABLE [dbo].[MP_TeraPeakOrder]  WITH CHECK ADD  CONSTRAINT [FK_TeraPeakOrder_CustomerMarketPlace] FOREIGN KEY([CustomerMarketPlaceId])
REFERENCES [dbo].[MP_CustomerMarketPlace] ([Id])
GO
ALTER TABLE [dbo].[MP_TeraPeakOrder] CHECK CONSTRAINT [FK_TeraPeakOrder_CustomerMarketPlace]
GO
ALTER TABLE [dbo].[MP_TeraPeakOrderItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_TeraPeakOrderItem_MP_TeraPeakOrder] FOREIGN KEY([TeraPeakOrderId])
REFERENCES [dbo].[MP_TeraPeakOrder] ([Id])
GO
ALTER TABLE [dbo].[MP_TeraPeakOrderItem] CHECK CONSTRAINT [FK_MP_TeraPeakOrderItem_MP_TeraPeakOrder]
GO
ALTER TABLE [dbo].[MP_VatReturnRecords]  WITH CHECK ADD  CONSTRAINT [FK_VatReturn_Business] FOREIGN KEY([BusinessId])
REFERENCES [dbo].[Business] ([Id])
GO
ALTER TABLE [dbo].[MP_VatReturnRecords] CHECK CONSTRAINT [FK_VatReturn_Business]
GO
ALTER TABLE [dbo].[MP_VatReturnRecords]  WITH CHECK ADD  CONSTRAINT [FK_VatReturn_MarketPlace] FOREIGN KEY([CustomerMarketPlaceId])
REFERENCES [dbo].[MP_CustomerMarketPlace] ([Id])
GO
ALTER TABLE [dbo].[MP_VatReturnRecords] CHECK CONSTRAINT [FK_VatReturn_MarketPlace]
GO
ALTER TABLE [dbo].[MP_VolusionOrder]  WITH CHECK ADD  CONSTRAINT [FK_MP_VolusionOrder_MP_CustomerMarketPlace] FOREIGN KEY([CustomerMarketPlaceId])
REFERENCES [dbo].[MP_CustomerMarketPlace] ([Id])
GO
ALTER TABLE [dbo].[MP_VolusionOrder] CHECK CONSTRAINT [FK_MP_VolusionOrder_MP_CustomerMarketPlace]
GO
ALTER TABLE [dbo].[MP_VolusionOrderItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_VolusionOrderItem_MP_Order] FOREIGN KEY([OrderId])
REFERENCES [dbo].[MP_VolusionOrder] ([Id])
GO
ALTER TABLE [dbo].[MP_VolusionOrderItem] CHECK CONSTRAINT [FK_MP_VolusionOrderItem_MP_Order]
GO
ALTER TABLE [dbo].[MP_YodleeOrder]  WITH CHECK ADD  CONSTRAINT [FK_MP_YodleeOrder_MP_CustomerMarketPlace] FOREIGN KEY([CustomerMarketPlaceId])
REFERENCES [dbo].[MP_CustomerMarketPlace] ([Id])
GO
ALTER TABLE [dbo].[MP_YodleeOrder] CHECK CONSTRAINT [FK_MP_YodleeOrder_MP_CustomerMarketPlace]
GO
ALTER TABLE [dbo].[MP_YodleeOrderItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_YodleeOrderItem_MP_YodleeOrder] FOREIGN KEY([OrderId])
REFERENCES [dbo].[MP_YodleeOrder] ([Id])
GO
ALTER TABLE [dbo].[MP_YodleeOrderItem] CHECK CONSTRAINT [FK_MP_YodleeOrderItem_MP_YodleeOrder]
GO
ALTER TABLE [dbo].[MP_YodleeOrderItemBankTransaction]  WITH CHECK ADD  CONSTRAINT [FK_MP_YodleeOrderItemBankTransaction_MP_YodleeOrderItem] FOREIGN KEY([OrderItemId])
REFERENCES [dbo].[MP_YodleeOrderItem] ([Id])
GO
ALTER TABLE [dbo].[MP_YodleeOrderItemBankTransaction] CHECK CONSTRAINT [FK_MP_YodleeOrderItemBankTransaction_MP_YodleeOrderItem]
GO
ALTER TABLE [dbo].[MP_YodleeOrderItemBankTransaction]  WITH CHECK ADD  CONSTRAINT [FK_MP_YodleeOrderItemBankTransaction_MP_YodleeTransactionCategories] FOREIGN KEY([transactionCategoryId])
REFERENCES [dbo].[MP_YodleeTransactionCategories] ([CategoryId])
GO
ALTER TABLE [dbo].[MP_YodleeOrderItemBankTransaction] CHECK CONSTRAINT [FK_MP_YodleeOrderItemBankTransaction_MP_YodleeTransactionCategories]
GO
ALTER TABLE [dbo].[ReportArguments]  WITH CHECK ADD  CONSTRAINT [FK_ReportArgument_Name] FOREIGN KEY([ReportArgumentNameId])
REFERENCES [dbo].[ReportArgumentNames] ([Id])
GO
ALTER TABLE [dbo].[ReportArguments] CHECK CONSTRAINT [FK_ReportArgument_Name]
GO
ALTER TABLE [dbo].[ReportArguments]  WITH CHECK ADD  CONSTRAINT [FK_ReportArgument_Report] FOREIGN KEY([ReportId])
REFERENCES [dbo].[ReportScheduler] ([Id])
GO
ALTER TABLE [dbo].[ReportArguments] CHECK CONSTRAINT [FK_ReportArgument_Report]
GO
ALTER TABLE [dbo].[ReportsUsersMap]  WITH CHECK ADD  CONSTRAINT [FK_ReportScheduler_ReportID] FOREIGN KEY([ReportID])
REFERENCES [dbo].[ReportScheduler] ([Id])
GO
ALTER TABLE [dbo].[ReportsUsersMap] CHECK CONSTRAINT [FK_ReportScheduler_ReportID]
GO
ALTER TABLE [dbo].[ReportsUsersMap]  WITH CHECK ADD  CONSTRAINT [FK_ReportUsers_UserID] FOREIGN KEY([UserID])
REFERENCES [dbo].[ReportUsers] ([Id])
GO
ALTER TABLE [dbo].[ReportsUsersMap] CHECK CONSTRAINT [FK_ReportUsers_UserID]
GO
ALTER TABLE [dbo].[Security_RoleAppRel]  WITH NOCHECK ADD  CONSTRAINT [FK_RoleAppRel_Application] FOREIGN KEY([AppId])
REFERENCES [dbo].[Security_Application] ([ApplicationId])
GO
ALTER TABLE [dbo].[Security_RoleAppRel] CHECK CONSTRAINT [FK_RoleAppRel_Application]
GO
ALTER TABLE [dbo].[Security_RoleAppRel]  WITH NOCHECK ADD  CONSTRAINT [FK_RoleAppRel_Role] FOREIGN KEY([RoleId])
REFERENCES [dbo].[Security_Role] ([RoleId])
GO
ALTER TABLE [dbo].[Security_RoleAppRel] CHECK CONSTRAINT [FK_RoleAppRel_Role]
GO
ALTER TABLE [dbo].[Security_Session]  WITH NOCHECK ADD  CONSTRAINT [FK_Security_Session_Security_Application] FOREIGN KEY([AppId])
REFERENCES [dbo].[Security_Application] ([ApplicationId])
GO
ALTER TABLE [dbo].[Security_Session] CHECK CONSTRAINT [FK_Security_Session_Security_Application]
GO
ALTER TABLE [dbo].[Security_Session]  WITH CHECK ADD  CONSTRAINT [FK_Security_Session_Security_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[Security_User] ([UserId])
GO
ALTER TABLE [dbo].[Security_Session] CHECK CONSTRAINT [FK_Security_Session_Security_User]
GO
ALTER TABLE [dbo].[Security_User]  WITH NOCHECK ADD  CONSTRAINT [FK_Security_User_Security_User] FOREIGN KEY([CreateUserId])
REFERENCES [dbo].[Security_User] ([UserId])
GO
ALTER TABLE [dbo].[Security_User] CHECK CONSTRAINT [FK_Security_User_Security_User]
GO
ALTER TABLE [dbo].[Security_User]  WITH NOCHECK ADD  CONSTRAINT [FK_Security_User_Security_User1] FOREIGN KEY([DeleteUserId])
REFERENCES [dbo].[Security_User] ([UserId])
GO
ALTER TABLE [dbo].[Security_User] CHECK CONSTRAINT [FK_Security_User_Security_User1]
GO
ALTER TABLE [dbo].[Security_UserRoleRelation]  WITH NOCHECK ADD  CONSTRAINT [FK_Security_UserRoleRelation_Security_Role] FOREIGN KEY([RoleId])
REFERENCES [dbo].[Security_Role] ([RoleId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Security_UserRoleRelation] CHECK CONSTRAINT [FK_Security_UserRoleRelation_Security_Role]
GO
ALTER TABLE [dbo].[Security_UserRoleRelation]  WITH NOCHECK ADD  CONSTRAINT [FK_Security_UserRoleRelation_Security_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[Security_User] ([UserId])
GO
ALTER TABLE [dbo].[Security_UserRoleRelation] CHECK CONSTRAINT [FK_Security_UserRoleRelation_Security_User]
GO
ALTER TABLE [dbo].[SiteAnalytics]  WITH CHECK ADD  CONSTRAINT [FK_PK_SiteAnalyticsCodesId] FOREIGN KEY([SiteAnalyticsCode])
REFERENCES [dbo].[SiteAnalyticsCodes] ([Id])
GO
ALTER TABLE [dbo].[SiteAnalytics] CHECK CONSTRAINT [FK_PK_SiteAnalyticsCodesId]
GO
ALTER TABLE [dbo].[Strategy_CalendarRelation]  WITH CHECK ADD  CONSTRAINT [FKEA3BC91D4A086D8C] FOREIGN KEY([CalendarId])
REFERENCES [dbo].[SystemCalendar_Calendar] ([Id])
GO
ALTER TABLE [dbo].[Strategy_CalendarRelation] CHECK CONSTRAINT [FKEA3BC91D4A086D8C]
GO
ALTER TABLE [dbo].[Strategy_CalendarRelation]  WITH CHECK ADD  CONSTRAINT [FKEA3BC91DC083FE87] FOREIGN KEY([StrategyId])
REFERENCES [dbo].[Strategy_Strategy] ([StrategyId])
GO
ALTER TABLE [dbo].[Strategy_CalendarRelation] CHECK CONSTRAINT [FKEA3BC91DC083FE87]
GO
ALTER TABLE [dbo].[Strategy_Node]  WITH NOCHECK ADD  CONSTRAINT [FK_Strategy_Node_Security_Application] FOREIGN KEY([ApplicationId])
REFERENCES [dbo].[Security_Application] ([ApplicationId])
GO
ALTER TABLE [dbo].[Strategy_Node] CHECK CONSTRAINT [FK_Strategy_Node_Security_Application]
GO
ALTER TABLE [dbo].[Strategy_Node]  WITH NOCHECK ADD  CONSTRAINT [FK_Strategy_Node_Strategy_NodeGroup] FOREIGN KEY([GroupId])
REFERENCES [dbo].[Strategy_NodeGroup] ([NodeGroupId])
GO
ALTER TABLE [dbo].[Strategy_Node] CHECK CONSTRAINT [FK_Strategy_Node_Strategy_NodeGroup]
GO
ALTER TABLE [dbo].[Strategy_NodeStrategyRel]  WITH CHECK ADD  CONSTRAINT [FK_Strategy_NodeStrategyRel_Strategy_Node] FOREIGN KEY([NodeId])
REFERENCES [dbo].[Strategy_Node] ([NodeId])
GO
ALTER TABLE [dbo].[Strategy_NodeStrategyRel] CHECK CONSTRAINT [FK_Strategy_NodeStrategyRel_Strategy_Node]
GO
ALTER TABLE [dbo].[Strategy_NodeStrategyRel]  WITH CHECK ADD  CONSTRAINT [FK_Strategy_NodeStrategyRel_Strategy_Strategy] FOREIGN KEY([StrategyId])
REFERENCES [dbo].[Strategy_Strategy] ([StrategyId])
GO
ALTER TABLE [dbo].[Strategy_NodeStrategyRel] CHECK CONSTRAINT [FK_Strategy_NodeStrategyRel_Strategy_Strategy]
GO
ALTER TABLE [dbo].[Strategy_Schedule]  WITH NOCHECK ADD  CONSTRAINT [FK_Schedule_StrategyId] FOREIGN KEY([StrategyId])
REFERENCES [dbo].[Strategy_Strategy] ([StrategyId])
GO
ALTER TABLE [dbo].[Strategy_Schedule] CHECK CONSTRAINT [FK_Schedule_StrategyId]
GO
ALTER TABLE [dbo].[Strategy_Strategy]  WITH CHECK ADD  CONSTRAINT [FK_Strategy_Strategy_Security_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[Security_User] ([UserId])
GO
ALTER TABLE [dbo].[Strategy_Strategy] CHECK CONSTRAINT [FK_Strategy_Strategy_Security_User]
GO
ALTER TABLE [dbo].[Strategy_Strategy]  WITH CHECK ADD  CONSTRAINT [FK_Strategy_Strategy_Security_User1] FOREIGN KEY([AuthorId])
REFERENCES [dbo].[Security_User] ([UserId])
GO
ALTER TABLE [dbo].[Strategy_Strategy] CHECK CONSTRAINT [FK_Strategy_Strategy_Security_User1]
GO
ALTER TABLE [dbo].[Strategy_StrategyParameter]  WITH NOCHECK ADD  CONSTRAINT [FK_Strategy_StrategyParameter_Strategy_ParameterType] FOREIGN KEY([TypeId])
REFERENCES [dbo].[Strategy_ParameterType] ([ParamTypeId])
GO
ALTER TABLE [dbo].[Strategy_StrategyParameter] CHECK CONSTRAINT [FK_Strategy_StrategyParameter_Strategy_ParameterType]
GO
ALTER TABLE [dbo].[Strategy_StrategyParameter]  WITH NOCHECK ADD  CONSTRAINT [FK_Strategy_StrategyParameter_Strategy_Strategy] FOREIGN KEY([OwnerId])
REFERENCES [dbo].[Strategy_Strategy] ([StrategyId])
GO
ALTER TABLE [dbo].[Strategy_StrategyParameter] CHECK CONSTRAINT [FK_Strategy_StrategyParameter_Strategy_Strategy]
GO

ALTER TABLE [dbo].[SV_ReportingInfo]  WITH CHECK ADD  CONSTRAINT [FK_SV_ReportingInfo_Security_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[Security_User] ([UserId])
GO
ALTER TABLE [dbo].[SV_ReportingInfo] CHECK CONSTRAINT [FK_SV_ReportingInfo_Security_User]
GO
ALTER TABLE [dbo].[SystemCalendar_BaseRelation]  WITH CHECK ADD  CONSTRAINT [FK9107F6204A086D8C] FOREIGN KEY([CalendarId])
REFERENCES [dbo].[SystemCalendar_Calendar] ([Id])
GO
ALTER TABLE [dbo].[SystemCalendar_BaseRelation] CHECK CONSTRAINT [FK9107F6204A086D8C]
GO
ALTER TABLE [dbo].[SystemCalendar_BaseRelation]  WITH CHECK ADD  CONSTRAINT [FK9107F620ADD7AFD3] FOREIGN KEY([BaseCalendarId])
REFERENCES [dbo].[SystemCalendar_Calendar] ([Id])
GO
ALTER TABLE [dbo].[SystemCalendar_BaseRelation] CHECK CONSTRAINT [FK9107F620ADD7AFD3]
GO
ALTER TABLE [dbo].[SystemCalendar_Day]  WITH CHECK ADD  CONSTRAINT [FKAFEECA076E0ADFD1] FOREIGN KEY([HostCalendarId])
REFERENCES [dbo].[SystemCalendar_Calendar] ([Id])
GO
ALTER TABLE [dbo].[SystemCalendar_Day] CHECK CONSTRAINT [FKAFEECA076E0ADFD1]
GO
ALTER TABLE [dbo].[SystemCalendar_Entry]  WITH CHECK ADD  CONSTRAINT [FK5B28FB9C177ADF22] FOREIGN KEY([HostEntryId])
REFERENCES [dbo].[SystemCalendar_Entry] ([Id])
GO
ALTER TABLE [dbo].[SystemCalendar_Entry] CHECK CONSTRAINT [FK5B28FB9C177ADF22]
GO
ALTER TABLE [dbo].[SystemCalendar_Entry]  WITH CHECK ADD  CONSTRAINT [FK5B28FB9C6E0ADFD1] FOREIGN KEY([HostCalendarId])
REFERENCES [dbo].[SystemCalendar_Calendar] ([Id])
GO
ALTER TABLE [dbo].[SystemCalendar_Entry] CHECK CONSTRAINT [FK5B28FB9C6E0ADFD1]
GO

ALTER TABLE [dbo].[YodleeAccounts]  WITH CHECK ADD  CONSTRAINT [FK_YodleeAccounts_Customer] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customer] ([Id])
GO
ALTER TABLE [dbo].[YodleeAccounts] CHECK CONSTRAINT [FK_YodleeAccounts_Customer]
GO
ALTER TABLE [dbo].[YodleeAccounts]  WITH CHECK ADD  CONSTRAINT [FK_YodleeAccounts_YodleeBanks] FOREIGN KEY([BankId])
REFERENCES [dbo].[YodleeBanks] ([Id])
GO
ALTER TABLE [dbo].[YodleeAccounts] CHECK CONSTRAINT [FK_YodleeAccounts_YodleeBanks]
GO
ALTER TABLE [dbo].[LoanSource]  WITH CHECK ADD  CONSTRAINT [CHK_LoanSource] CHECK  (([LoanSourceID]>(0)))
GO
ALTER TABLE [dbo].[LoanSource] CHECK CONSTRAINT [CHK_LoanSource]
GO
ALTER TABLE [dbo].[LoanSource]  WITH CHECK ADD  CONSTRAINT [CHK_LoanSource_MaxInterest] CHECK  (([MaxInterest] IS NULL OR [MaxInterest]>(0)))
GO
ALTER TABLE [dbo].[LoanSource] CHECK CONSTRAINT [CHK_LoanSource_MaxInterest]
GO
ALTER TABLE [dbo].[LoanSource]  WITH CHECK ADD  CONSTRAINT [CHK_LoanSource_Name] CHECK  (([LoanSourceName]<>''))
GO
ALTER TABLE [dbo].[LoanSource] CHECK CONSTRAINT [CHK_LoanSource_Name]
GO
ALTER TABLE [dbo].[LoyaltyProgramActions]  WITH CHECK ADD  CONSTRAINT [CHK_LoyaltyProgramActions] CHECK  (([ActionID]>(0) AND ltrim(rtrim([ActionName]))<>'' AND ltrim(rtrim([ActionDescription]))<>''))
GO
ALTER TABLE [dbo].[LoyaltyProgramActions] CHECK CONSTRAINT [CHK_LoyaltyProgramActions]
GO
ALTER TABLE [dbo].[LoyaltyProgramActionTypes]  WITH CHECK ADD  CONSTRAINT [CHK_LoyaltyProgramActionTypes] CHECK  (([ActionTypeID]>(0) AND ltrim(rtrim([ActionTypeName]))<>''))
GO
ALTER TABLE [dbo].[LoyaltyProgramActionTypes] CHECK CONSTRAINT [CHK_LoyaltyProgramActionTypes]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 - Full Range
1- Partial Filled
2 -Temporary' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'MP_TeraPeakOrderItem', @level2type=N'COLUMN',@level2name=N'RangeMarker'
GO

IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TR_AccountCheckedLoyalty]'))
DROP TRIGGER [dbo].[TR_AccountCheckedLoyalty]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TRIGGER TR_AccountCheckedLoyalty
ON MP_AnalyisisFunctionValues
FOR INSERT
AS
BEGIN
	SET NOCOUNT ON

	SELECT
		v.CustomerMarketPlaceId,
		MAX(
			(((((CAST(YEAR(v.Updated) AS BIGINT)
			) * 100 + CAST(MONTH(v.Updated) AS BIGINT)
			) * 100 + CAST(DAY(v.Updated) AS BIGINT)
			) * 100 + CAST(DATEPART(hour, v.Updated) AS BIGINT)
			) * 100 + CAST(DATEPART(minute, v.Updated) AS BIGINT)
			) * 100 + CAST(DATEPART(second, v.Updated) AS BIGINT)
		) AS MaxMeasureCode
	INTO
		#m
	FROM
		inserted v
		INNER JOIN MP_AnalyisisFunction f ON f.Id = v.AnalyisisFunctionId AND f.Name = 'TotalSumOfOrders'
		LEFT JOIN LoyaltyProgramCheckedAccounts lp
			ON v.CustomerMarketPlaceId = lp.CustomerMarketPlaceID
	WHERE
		v.ValueFloat > 0
		AND
		lp.CustomerMarketPlaceID IS NULL
	GROUP BY
		v.CustomerMarketPlaceId

	INSERT INTO CustomerLoyaltyProgram (CustomerID, CustomerMarketPlaceID, ActionID, EarnedPoints)
	SELECT
		mp.CustomerId,
		#m.CustomerMarketPlaceId,
		a.ActionID,
		a.Cost
	FROM
		#m
		INNER JOIN MP_CustomerMarketPlace mp ON #m.CustomerMarketPlaceId = mp.Id
		INNER JOIN LoyaltyProgramActions a ON a.ActionName = 'ACCOUNTCHECKED'
	
	DROP TABLE #m
	
	SET NOCOUNT OFF
END
GO
IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TR_CustomerLinkAccountLoyalty]'))
DROP TRIGGER [dbo].[TR_CustomerLinkAccountLoyalty]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TRIGGER TR_CustomerLinkAccountLoyalty
ON MP_CustomerMarketPlace
FOR INSERT
AS
BEGIN
	SET NOCOUNT ON

	INSERT INTO CustomerLoyaltyProgram (CustomerID, CustomerMarketPlaceID, ActionID, EarnedPoints)
	SELECT
		c.CustomerId,
		c.Id,
		a.ActionID,
		a.Cost
	FROM
		inserted c
		INNER JOIN LoyaltyProgramActions a ON a.ActionName = 'LINKACCOUNT'

	SET NOCOUNT OFF
END
GO
IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TR_RepaymentLoyalty]'))
DROP TRIGGER [dbo].[TR_RepaymentLoyalty]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TRIGGER TR_RepaymentLoyalty
ON LoanSchedule
FOR UPDATE
AS
BEGIN
	SET NOCOUNT ON

	INSERT INTO CustomerLoyaltyProgram (CustomerID, LoanID, LoanScheduleID, ActionID, EarnedPoints)
	SELECT
		l.CustomerId,
		l.Id,
		i.Id,
		a.ActionID,
		a.Cost * CAST(d.LoanRepayment - i.LoanRepayment AS NUMERIC(29, 0))
	FROM
		deleted d
		INNER JOIN inserted i ON d.Id = i.id
		INNER JOIN Loan l ON d.LoanId = l.Id
		INNER JOIN LoyaltyProgramActions a ON a.ActionName = 'REPAYMENT'
	WHERE
		d.Status != 'Late'
		AND
		i.Status != 'Late'
	
	SET NOCOUNT OFF
END
GO
IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TR_CustomerPersonalInfoLoyalty]'))
DROP TRIGGER [dbo].[TR_CustomerPersonalInfoLoyalty]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TRIGGER TR_CustomerPersonalInfoLoyalty
ON Customer
FOR UPDATE
AS
BEGIN
	SET NOCOUNT ON

	INSERT INTO CustomerLoyaltyProgram (CustomerID, ActionID, EarnedPoints)
	SELECT
		c.Id,
		a.ActionID,
		a.Cost
	FROM
		inserted c
		INNER JOIN deleted d ON c.Id = d.Id AND c.WizardStep = 4 AND d.WizardStep != 4
		INNER JOIN LoyaltyProgramActions a ON a.ActionName = 'PERSONALINFO'

	SET NOCOUNT OFF
END
GO

IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TR_CustomerSignupLoyalty]'))
DROP TRIGGER [dbo].[TR_CustomerSignupLoyalty]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TRIGGER TR_CustomerSignupLoyalty
ON Customer
FOR INSERT
AS
BEGIN
	SET NOCOUNT ON

	INSERT INTO CustomerLoyaltyProgram (CustomerID, ActionID, EarnedPoints)
	SELECT
		c.Id,
		a.ActionID,
		a.Cost
	FROM
		inserted c
		INNER JOIN LoyaltyProgramActions a ON a.ActionName = 'SIGNUP'

	SET NOCOUNT OFF
END
GO
IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TR_LoanTakenLoyalty]'))
DROP TRIGGER [dbo].[TR_LoanTakenLoyalty]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TRIGGER TR_LoanTakenLoyalty
ON Loan
FOR INSERT
AS
BEGIN
	SET NOCOUNT ON

	INSERT INTO CustomerLoyaltyProgram (CustomerID, LoanID, ActionID, EarnedPoints)
	SELECT
		CustomerId,
		Id,
		a.ActionID,
		a.Cost * l.LoanAmount
	FROM
		inserted l
		INNER JOIN LoyaltyProgramActions a ON a.ActionName = 'LOAN'
	
	SET NOCOUNT OFF
END
GO
IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TR_LoanTransactionMethod]'))
DROP TRIGGER [dbo].[TR_LoanTransactionMethod]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TRIGGER TR_LoanTransactionMethod
ON LoanTransaction
FOR INSERT, UPDATE
AS
BEGIN
	SET NOCOUNT ON

	------------------------------------------------------------------------------

	UPDATE LoanTransaction SET
		LoanTransactionMethodId = m.Id
	FROM
		inserted i,
		LoanTransaction t,
		LoanTransactionMethod m
	WHERE
		i.Id = t.Id
		AND
		t.Type = 'PacnetTransaction'
		AND
		m.Name = 'Pacnet'

	------------------------------------------------------------------------------

	UPDATE LoanTransaction SET
		LoanTransactionMethodId = m.Id
	FROM
		inserted i,
		LoanTransaction t,
		LoanTransactionMethod m
	WHERE
		i.Id = t.Id
		AND
		t.Type = 'PaypointTransaction'
		AND
		t.PaypointId IS NOT NULL
		AND
		t.PaypointId NOT LIKE '--- manual ---'
		AND
		m.Name = 'Auto'

	------------------------------------------------------------------------------

	UPDATE LoanTransaction SET
		LoanTransactionMethodId = ISNULL(m.Id, 0)
	FROM
		inserted i,
		LoanTransaction t,
		LoanTransactionMethod m
	WHERE
		i.Id = t.Id
		AND
		t.Type = 'PaypointTransaction'
		AND
		t.PaypointId IS NOT NULL
		AND
		t.PaypointId LIKE '--- manual ---'
		AND
		m.Name = dbo.udfPaymentMethod(t.Description)

	------------------------------------------------------------------------------

	UPDATE LoanTransaction SET
		LoanTransactionMethodId = m.Id
	FROM
		inserted i,
		LoanTransaction t,
		LoanTransactionMethod m
	WHERE
		i.Id = t.Id
		AND
		t.Type = 'PaypointTransaction'
		AND
		t.PaypointId IS NOT NULL
		AND
		t.PaypointId LIKE '--- manual ---'
		AND
		t.LoanTransactionMethodId = 0
		AND
		m.Name = 'Manual'

	------------------------------------------------------------------------------

	SET NOCOUNT OFF
END
GO

USE [master]
GO
ALTER DATABASE [ezbob] SET  READ_WRITE 
GO
