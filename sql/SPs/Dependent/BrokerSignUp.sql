IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BrokerSignUp]') AND TYPE IN (N'P', N'PC'))
	EXECUTE('CREATE PROCEDURE BrokerSignUp AS SELECT 1')
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
@FirmWebSiteUrl NVARCHAR(255),
@EstimatedMonthlyApplicationCount INT,
@AgreedToTermsDate DATETIME,
@AgreedToPrivacyPolicyDate DATETIME,
@BrokerTermsID INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ErrMsg NVARCHAR(255) = ''
	DECLARE @BrokerID INT
	DECLARE @UserID INT
	DECLARE @SourceRef NVARCHAR(255)

	IF @BrokerTermsID = 0
		SET @BrokerTermsID = NULL

	IF @ErrMsg = ''
	BEGIN
		SET @ErrMsg = dbo.udfCheckContactInfoUniqueness(@ContactEmail, @ContactMobile, DEFAULT, DEFAULT, DEFAULT, DEFAULT)
	END

	IF @ErrMsg = ''
	BEGIN
		BEGIN TRY
			INSERT INTO Security_User (UserName, FullName, Email, BranchId)
				VALUES (@ContactEmail, @FirmName, @ContactEmail, 0)
		END TRY
		BEGIN CATCH
			SET @ErrMsg = 'Failed to create a user entry: ' + dbo.udfGetErrorMsg()
		END CATCH
	END

	IF @ErrMsg = ''
	BEGIN
		SET @UserID = SCOPE_IDENTITY()

		BEGIN TRY
			INSERT INTO Broker(
				FirmName, FirmRegNum, ContactName, ContactEmail, ContactMobile,
				ContactOtherPhone, SourceRef, EstimatedMonthlyClientAmount, Password, UserID,
				FirmWebSiteUrl, EstimatedMonthlyApplicationCount, AgreedToTermsDate, AgreedToPrivacyPolicyDate,
				BrokerTermsID
			) VALUES (
				@FirmName, @FirmRegNum, @ContactName, @ContactEmail, @ContactMobile,
				@ContactOtherPhone, @TempSourceRef, @EstimatedMonthlyClientAmount, @Password, @UserID,
				@FirmWebSiteUrl, @EstimatedMonthlyApplicationCount, @AgreedToTermsDate, @AgreedToPrivacyPolicyDate,
				@BrokerTermsID
			)
		END TRY
		BEGIN CATCH
			SET @ErrMsg = 'Failed to create a broker entry: ' + dbo.udfGetErrorMsg()

			DELETE FROM Security_User WHERE UserId = @UserID
		END CATCH
	END

	IF @ErrMsg = ''
	BEGIN
		SET @BrokerID = SCOPE_IDENTITY()

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
			DELETE FROM Security_User WHERE UserId = @UserID
		END CATCH
	END

	IF @ErrMsg = ''
		EXECUTE BrokerLoadOwnProperties @BrokerID = @BrokerID
	ELSE
		SELECT @ErrMsg AS ErrorMsg, @BrokerID AS BrokerID
END
GO
