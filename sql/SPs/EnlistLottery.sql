IF OBJECT_ID('EnlistLottery') IS NULL
	EXECUTE('CREATE PROCEDURE EnlistLottery AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE EnlistLottery
@CustomerID INT,
@UniqueID UNIQUEIDENTIFIER,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @BrokerID INT
	DECLARE @UserID INT = @CustomerID
	DECLARE @LotteryID BIGINT = 0
	DECLARE @CfgLotteryName NVARCHAR(256) = 'LotteryForCustomers'

	DECLARE @Email NVARCHAR(255)
	DECLARE @ContactName NVARCHAR(255)

	DECLARE @LoanCount INT = 1 -- This SP is called only when customer takes the first loan.

	------------------------------------------------------------------------------

	SELECT
		@UserID = Id,
		@BrokerID = BrokerID,
		@Email = Name,
		@ContactName = FirstName
	FROM
		Customer c
	WHERE
		c.Id = @CustomerID

	------------------------------------------------------------------------------

	IF @UserID != @CustomerID -- wrong customer id specified
		RETURN

	------------------------------------------------------------------------------

	IF @BrokerID > 0
	BEGIN
		SET @UserID = @BrokerID
		SET @CfgLotteryName = 'LotteryForBrokers'

		SELECT
			@Email = ContactEmail,
			@ContactName = ContactName
		FROM
			Broker
		WHERE
			BrokerID = @UserID

		SELECT
			@LoanCount = COUNT(*)
		FROM
			Loan l
			INNER JOIN Customer c
				ON l.CustomerId = c.Id
				AND c.BrokerID = @BrokerID
	END

	------------------------------------------------------------------------------

	IF @LoanCount != 1
		RETURN

	------------------------------------------------------------------------------

	BEGIN TRY
		SELECT
			@LotteryID = CONVERT(BIGINT, v.Value)
		FROM
			ConfigurationVariables v
		WHERE
			v.Name = @CfgLotteryName
	END TRY
	BEGIN CATCH
		RETURN -- configuration is not valid lottery id
	END CATCH

	------------------------------------------------------------------------------

	IF NOT EXISTS (SELECT * FROM Lotteries WHERE LotteryID = @LotteryID)
		RETURN

	------------------------------------------------------------------------------

	SET TRANSACTION ISOLATION LEVEL SERIALIZABLE

	BEGIN TRANSACTION

	------------------------------------------------------------------------------

	IF EXISTS (SELECT * FROM LotteryPlayers WHERE LotteryID = LotteryID AND UserID = @UserID)
	BEGIN
		COMMIT TRANSACTION
		RETURN
	END

	INSERT INTO LotteryPlayers (UniqueID, UserID, LotteryID, StatusID, InsertionTime)
		VALUES (@UniqueID, @UserID, @LotteryID, 1, @Now)

	------------------------------------------------------------------------------

	COMMIT TRANSACTION

	------------------------------------------------------------------------------

	SELECT
		Email = @Email,
		ContactName = @ContactName,
		TemplateName = l.MailTemplateName
	FROM
		Lotteries l
	WHERE
		l.LotteryID = @LotteryID
END
GO
