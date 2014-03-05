IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BrokerSignUp]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[BrokerSignUp]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BrokerSignUp] 
	(@FirmName NVARCHAR(255),
@FirmRegNum NVARCHAR(255),
@ContactName NVARCHAR(255),
@ContactEmail NVARCHAR(255),
@ContactMobile NVARCHAR(255),
@ContactOtherPhone NVARCHAR(255),
@SourceRef NVARCHAR(255),
@EstimatedMonthlyClientAmount DECIMAL(18, 4),
@Password NVARCHAR(255))
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ErrMsg NVARCHAR(255) = ''
	DECLARE @BrokerID INT
	DECLARE @sBrokerID NVARCHAR(255)

	IF @ErrMsg = ''
	BEGIN
		IF EXISTS (SELECT * FROM Security_User WHERE Email = @ContactEmail)
			SET @ErrMsg = 'There is already a customer with such email: ' + @ContactEmail
	END

	IF @ErrMsg = ''
	BEGIN
		IF EXISTS (SELECT * FROM Customer WHERE DaytimePhone = @ContactMobile OR MobilePhone = @ContactMobile)
			SET @ErrMsg = 'There is already a customer with such phone number: ' + @ContactMobile
	END

	IF @ErrMsg = ''
	BEGIN
		INSERT INTO Broker(FirmName, FirmRegNum, ContactName, ContactEmail, ContactMobile, ContactOtherPhone, SourceRef, EstimatedMonthlyClientAmount, Password)
			VALUES (@FirmName, @FirmRegNum, @ContactName, @ContactEmail, @ContactMobile, @ContactOtherPhone, CAST(NEWID() AS NVARCHAR(255)), @EstimatedMonthlyClientAmount, @Password)

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
