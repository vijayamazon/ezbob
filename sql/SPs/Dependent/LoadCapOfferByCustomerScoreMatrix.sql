SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadCapOfferByCustomerScoreMatrix') IS NULL
	EXECUTE('CREATE PROCEDURE LoadCapOfferByCustomerScoreMatrix AS SELECT 1')
GO

ALTER PROCEDURE LoadCapOfferByCustomerScoreMatrix
@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @MatrixName NVARCHAR(64)

	IF EXISTS (SELECT 1 FROM CustomerAddress WHERE CustomerId = @CustomerID AND IsOwnerAccordingToLandRegistry = 1)
		SET @MatrixName = 'CapOfferByCustomerScoresOwner'
	ELSE
		SET @MatrixName = 'CapOfferByCustomerScoresHomeless'

	EXECUTE LoadMatrix @MatrixName
END
GO
