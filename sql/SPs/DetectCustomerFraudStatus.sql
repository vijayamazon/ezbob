IF OBJECT_ID('DetectCustomerFraudStatus') IS NULL
	EXECUTE('CREATE PROCEDURE DetectCustomerFraudStatus AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE DetectCustomerFraudStatus
@CustomerID INT,
@Now DATETIME,
@FraudStatus INT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	SET @FraudStatus = NULL

	------------------------------------------------------------------------------

	DECLARE @FraudRqID INT

	------------------------------------------------------------------------------

	IF @Now IS NOT NULL
	BEGIN
		SELECT TOP 1
			@FraudRqID = r.Id
		FROM
			FraudRequest r
		WHERE
			r.CustomerId = @CustomerID
			AND
			r.CheckDate < @Now
		ORDER BY
			r.CheckDate DESC

		IF EXISTS (SELECT * FROM FraudDetection WHERE FraudRequestId = @FraudRqID)
			SET @FraudStatus = 2 -- Fraud suspect
	END

	------------------------------------------------------------------------------

	IF @FraudStatus IS NULL
	BEGIN
		SELECT
			@FraudStatus = c.FraudStatus
		FROM
			Customer c
		WHERE
			c.Id = @CustomerID
	END

	------------------------------------------------------------------------------

	IF @FraudStatus IS NULL
		SET @FraudStatus = 3 -- Under investigation
END
GO
