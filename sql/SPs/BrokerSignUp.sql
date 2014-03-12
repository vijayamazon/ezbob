IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BrokerSignUp]') AND TYPE IN (N'P', N'PC'))
	EXECUTE('CREATE PROCEDURE BrokerSignUp AS SELECT 1')
GO

SET ANSI_NULLS ON
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
@SourceRef NVARCHAR(255),
@EstimatedMonthlyClientAmount DECIMAL(18, 4),
@Password NVARCHAR(255)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ErrMsg NVARCHAR(255) = ''
	DECLARE @BrokerID INT
	DECLARE @UserID INT
	DECLARE @sBrokerID NVARCHAR(255)

	IF @ErrMsg = ''
	BEGIN
		SET @ErrMsg = dbo.udfCheckContactInfoUniqueness(@ContactEmail, @ContactMobile, DEFAULT, DEFAULT, DEFAULT)
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
			INSERT INTO Broker(FirmName, FirmRegNum, ContactName, ContactEmail, ContactMobile, ContactOtherPhone, SourceRef, EstimatedMonthlyClientAmount, Password, UserID)
				VALUES (@FirmName, @FirmRegNum, @ContactName, @ContactEmail, @ContactMobile, @ContactOtherPhone, CAST(NEWID() AS NVARCHAR(255)), @EstimatedMonthlyClientAmount, @Password, @UserID)
		END TRY
		BEGIN CATCH
			SET @ErrMsg = 'Failed to create a broker entry: ' + dbo.udfGetErrorMsg()

			DELETE FROM Security_User WHERE UserId = @UserID
		END CATCH
	END

	IF @ErrMsg = ''
	BEGIN
		SET @BrokerID = SCOPE_IDENTITY()
		SET @sBrokerID = CAST(@BrokerID AS NVARCHAR(255))

		UPDATE Broker SET
			SourceRef = SUBSTRING(@SourceRef, 1, 10 - LEN(@sBrokerID)) + @sBrokerID
		WHERE
			BrokerID = @BrokerID
	END

	SELECT @ErrMsg AS ErrorMsg
END
GO
