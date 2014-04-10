IF OBJECT_ID('BrokerCheckCustomerRelevance') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerCheckCustomerRelevance AS SELECT 1')
GO

ALTER PROCEDURE BrokerCheckCustomerRelevance
@CustomerID INT,
@CustomerEmail NVARCHAR(128),
@SourceRef NVARCHAR(200)
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @BrokerID INT
	DECLARE @BrokerLeadID INT
	DECLARE @IsTest BIT

	------------------------------------------------------------------------------

	SELECT
		@BrokerLeadID = BrokerLeadID,
		@BrokerID = BrokerID
	FROM
		BrokerLeads
	WHERE
		Email = @CustomerEmail

	------------------------------------------------------------------------------

	IF @BrokerID IS NOT NULL
	BEGIN
		SELECT
			@IsTest = IsTest
		FROM
			Broker
		WHERE
			BrokerID = @BrokerID

		--------------------------------------------------------------------------

		UPDATE Customer SET
			BrokerID = @BrokerID,
			IsTest = CASE @IsTest WHEN 1 THEN 1 ELSE IsTest END
		WHERE
			Id = @CustomerID

		--------------------------------------------------------------------------

		UPDATE BrokerLeads SET
			CustomerID = @CustomerID
		WHERE
			BrokerLeadID = @BrokerLeadID
			AND
			CustomerID IS NULL

		--------------------------------------------------------------------------

		RETURN
	END

	------------------------------------------------------------------------------

	SELECT
		@BrokerID = BrokerID,
		@IsTest = IsTest
	FROM
		Broker
	WHERE
		SourceRef = @SourceRef

	------------------------------------------------------------------------------

	IF @BrokerID IS NOT NULL
	BEGIN
		UPDATE Customer SET
			BrokerID = @BrokerID,
			IsTest = CASE @IsTest WHEN 1 THEN 1 ELSE IsTest END
		WHERE
			Id = @CustomerID
	END

	------------------------------------------------------------------------------
END
GO
