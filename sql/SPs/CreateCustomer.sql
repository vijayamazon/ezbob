SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('CreateCustomer') IS NULL
	EXECUTE('CREATE PROCEDURE CreateCustomer AS SELECT 1')
GO

ALTER PROCEDURE CreateCustomer
@CustomerID INT,
@UserName NVARCHAR(128),
@OriginID INT,
@Status NVARCHAR(250),
@RefNumber NVARCHAR(8),
@WizardStep INT,
@CollectionStatus INT,
@IsTest BIT,
@WhiteLabelID INT,
@MobilePhone NVARCHAR(50),
@MobilePhoneVerified BIT,
@FirstName NVARCHAR(250),
@LastName NVARCHAR(250),
@TrustPilotStatusID INT,
@GreetingMailSentDate DATETIME,
@ABTesting NVARCHAR(512),
@FirstVisitTime NVARCHAR(64),
@ReferenceSource NVARCHAR(1000),
@GoogleCookie NVARCHAR(300),
@AlibabaID NVARCHAR(300),
@IsAlibaba BIT
AS
BEGIN
	------------------------------------------------------------------------------
	--
	-- CONST
	--
	------------------------------------------------------------------------------

	DECLARE @IsOffline BIT = NULL

	------------------------------------------------------------------------------
	--
	-- Detect IsTest if needed.
	--
	------------------------------------------------------------------------------

	IF @IsTest IS NULL
	BEGIN
		IF EXISTS (SELECT * FROM TestCustomer WHERE @UserName LIKE '%' + LOWER(Pattern) + '%')
			SET @IsTest = 1
		ELSE
			SET @IsTest = 0
	END

	------------------------------------------------------------------------------
	--
	-- Detect VIP
	--
	------------------------------------------------------------------------------

	DECLARE @Vip BIT = 0

	------------------------------------------------------------------------------

	IF EXISTS (SELECT * FROM VipRequest WHERE LOWER(Email) = @UserName)
		SET @Vip = 1

	------------------------------------------------------------------------------
	--
	-- Validate white label (can be NULL).
	--
	------------------------------------------------------------------------------

	SET @WhiteLabelID = (SELECT TOP 1 Id FROM WhiteLabelProvider WHERE Id = @WhiteLabelID)

	------------------------------------------------------------------------------
	--
	-- Detect broker by white label (can be NULL).
	--
	------------------------------------------------------------------------------

	DECLARE @BrokerID INT = NULL

	IF @WhiteLabelID IS NOT NULL
		SET @BrokerID = (SELECT TOP 1 BrokerID FROM Broker WHERE WhiteLabelId = @WhiteLabelID)

	------------------------------------------------------------------------------
	--
	-- This is what this stored procedure is all about.
	--
	------------------------------------------------------------------------------

	INSERT INTO Customer (
		Id, Name, OriginID, Status, RefNumber, WizardStep,
		CollectionStatus, IsTest, MobilePhone, MobilePhoneVerified,
		FirstName, Surname, TrustPilotStatusID, GreetingMailSentDate,
		ABTesting, FirstVisitTime, ReferenceSource, GoogleCookie,
		AlibabaId, IsAlibaba, IsOffline, Vip, WhiteLabelId, BrokerID,
		PropertyStatusId
	) VALUES (
		@CustomerID, @UserName, @OriginID, @Status, @RefNumber, @WizardStep,
		@CollectionStatus, @IsTest, @MobilePhone, @MobilePhoneVerified,
		@FirstName, @LastName, @TrustPilotStatusID, @GreetingMailSentDate,
		@ABTesting, @FirstVisitTime, @ReferenceSource, @GoogleCookie,
		@AlibabaID, @IsAlibaba, @IsOffline, @Vip, @WhiteLabelID, @BrokerID,
		NULL
	)
END
GO
