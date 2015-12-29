IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BrokerSignUp]') AND TYPE IN (N'P', N'PC'))
	EXECUTE('CREATE PROCEDURE BrokerSignUp AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE BrokerSignUp
@FirmName NVARCHAR(255),
@FirmRegNum NVARCHAR(255),
@ContactName NVARCHAR(255),
@ContactEmail NVARCHAR(255),
@ContactMobile NVARCHAR(255),
@ContactOtherPhone NVARCHAR(255),
@TempSourceRef NVARCHAR(255),
@EstimatedMonthlyClientAmount DECIMAL(18, 4),
@Password NVARCHAR(255),
@Salt NVARCHAR(255),
@CycleCount NVARCHAR(255),
@FirmWebSiteUrl NVARCHAR(255),
@EstimatedMonthlyApplicationCount INT,
@AgreedToTermsDate DATETIME,
@AgreedToPrivacyPolicyDate DATETIME,
@BrokerTermsID INT,
@ReferredBy NVARCHAR(255),
@FCARegistered BIT,
@LicenseNumber NVARCHAR(255),
@UiOriginID INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ErrMsg NVARCHAR(255) = ''
	DECLARE @BrokerID INT
	DECLARE @SourceRef NVARCHAR(255)
	DECLARE @IsTest BIT = 0
	DECLARE @IsAutoTest BIT = 0

	IF @BrokerTermsID = 0
		SET @BrokerTermsID = NULL

	IF @ErrMsg = ''
	BEGIN
		SET @ErrMsg = dbo.udfCheckContactInfoUniqueness(@ContactEmail, @UiOriginID, @ContactMobile, DEFAULT, DEFAULT, DEFAULT, DEFAULT)
	END

	IF @ErrMsg = ''
	BEGIN
		SET @ErrMsg = dbo.udfCheckExternalBrokerEmailCollissions(@ContactEmail)
	END

	IF @ErrMsg = ''
	BEGIN
		BEGIN TRY
			INSERT INTO Security_User (UserName, FullName, Email, BranchId, EzPassword, Salt, CycleCount, OriginID)
				VALUES (@ContactEmail, @FirmName, @ContactEmail, 0, @Password, @Salt, @CycleCount, @UiOriginID)
		END TRY
		BEGIN CATCH
			SET @ErrMsg = 'Failed to create a user entry: ' + dbo.udfGetErrorMsg()
		END CATCH
	END

	IF @ErrMsg = ''
	BEGIN
		SET @BrokerID = SCOPE_IDENTITY()

		SET @IsAutoTest = CASE (SELECT LOWER(ISNULL(LTRIM(RTRIM(Value)), '')) FROM ConfigurationVariables WHERE Name = 'AutomaticTestBrokerMark')
			WHEN '1' THEN 1
			WHEN 'true' THEN 1
			WHEN 'yes' THEN 1
			ELSE 0
		END

		IF @IsAutoTest = 1
		BEGIN
			IF EXISTS (SELECT * FROM TestCustomer WHERE LOWER(@ContactEmail) LIKE '%' + LOWER(ISNULL(LTRIM(RTRIM(Pattern)), '')))
				SET @IsTest = 1
			ELSE
				SET @IsTest = 0
		END

		BEGIN TRY
			INSERT INTO Broker(
				BrokerID, FirmName, FirmRegNum, ContactName, ContactEmail, ContactMobile,
				ContactOtherPhone, SourceRef, EstimatedMonthlyClientAmount, Password,
				FirmWebSiteUrl, EstimatedMonthlyApplicationCount, AgreedToTermsDate, AgreedToPrivacyPolicyDate,
				BrokerTermsID, IsTest, ReferredBy, FCARegistered, LicenseNumber, OriginID
			) VALUES (
				@BrokerID, @FirmName, @FirmRegNum, @ContactName, @ContactEmail, @ContactMobile,
				@ContactOtherPhone, @TempSourceRef, @EstimatedMonthlyClientAmount, 'not used',
				@FirmWebSiteUrl, @EstimatedMonthlyApplicationCount, @AgreedToTermsDate, @AgreedToPrivacyPolicyDate,
				@BrokerTermsID, @IsTest, @ReferredBy, @FCARegistered, @LicenseNumber, @UiOriginID
			)
		END TRY
		BEGIN CATCH
			SET @ErrMsg = 'Failed to create a broker entry: ' + dbo.udfGetErrorMsg()

			DELETE FROM Security_User WHERE UserId = @BrokerID
		END CATCH
	END

	IF @ErrMsg = ''
	BEGIN
		BEGIN TRY
			EXECUTE BrokerGenerateSourceRef @BrokerID, @SourceRef OUT
	
			UPDATE Broker SET
				SourceRef = @SourceRef
			WHERE
				BrokerID = @BrokerID
		END TRY
		BEGIN CATCH
			SET @ErrMsg = 'Failed to create a broker entry: ' + dbo.udfGetErrorMsg()

			DELETE FROM Broker WHERE BrokerID = @BrokerID
			DELETE FROM Security_User WHERE UserId = @BrokerID
		END CATCH
	END

	IF @ErrMsg = ''
		EXECUTE BrokerLoadOwnProperties @BrokerID = @BrokerID
	ELSE
		SELECT @ErrMsg AS ErrorMsg, @BrokerID AS BrokerID
END
GO
