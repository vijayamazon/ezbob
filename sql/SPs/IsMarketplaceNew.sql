SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('IsMarketplaceNew') IS NULL
	EXECUTE('CREATE PROCEDURE IsMarketplaceNew AS SELECT 1')
GO

ALTER PROCEDURE IsMarketplaceNew
@MpID INT
AS
BEGIN
	DECLARE @CustomerID INT = NULL
	DECLARE @Created DATETIME

	SELECT
		@CustomerID = m.CustomerId,
		@Created = m.Created
	FROM
		MP_CustomerMarketPlace m
	WHERE
		m.Id = @MpID

	IF @CustomerID IS NULL
	BEGIN
		SELECT IsNew = CONVERT(BIT, 0)
		RETURN
	END

	DECLARE @LastCashRequestCreated DATETIME =(
		SELECT
			MAX(r.CreationDate)
		FROM
			CashRequests r
		WHERE
			r.IdCustomer = @CustomerID
	)

	IF @LastCashRequestCreated IS NULL
	BEGIN
		SELECT IsNew = CONVERT(BIT, 0)
		RETURN
	END

	SELECT IsNew = CONVERT(BIT, CASE WHEN @Created > @LastCashRequestCreated THEN 1 ELSE 0 END)
END
GO
