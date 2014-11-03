IF OBJECT_ID('SetCustomerManualAnnualizedRevenue') IS NULL
	EXECUTE('CREATE PROCEDURE SetCustomerManualAnnualizedRevenue AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE SetCustomerManualAnnualizedRevenue
@CustomerID INT,
@Revenue DECIMAL(18, 2),
@Comment NVARCHAR(255),
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	SET TRANSACTION ISOLATION LEVEL SERIALIZABLE

	BEGIN TRANSACTION

	UPDATE CustomerManualUwData SET
		IsActive = 0
	WHERE
		CustomerID = @CustomerID
		AND
		ISActive = 1

	INSERT INTO CustomerManualUwData (CustomerID, IsActive, EntryTime, AnnualizedRevenue, Comment)
		VALUES (@CustomerID, 1, @Now, @Revenue, @Comment)

	COMMIT TRANSACTION
END
GO
