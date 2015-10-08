IF OBJECT_ID('LoadSaleContractResult') IS NULL
	EXECUTE('CREATE PROCEDURE LoadSaleContractResult AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadSaleContractResult
@CustomerId INT,
@AliMemberId BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	IF (SELECT Id from dbo.Customer where Id = @CustomerId)  IS NULL
	BEGIN
		SELECT NULL AS aId, @AliMemberId as aliMemberId;
		RETURN;
	END

	IF (SELECT Id from dbo.AlibabaBuyer where AliId = @AliMemberId) IS NULL
	BEGIN
		SELECT NULL AS aliMemberId, @CustomerId as aId ;
		RETURN;
	END

	IF (SELECT CustomerId from dbo.AlibabaBuyer where AliId = @AliMemberId) <> @CustomerId
	BEGIN
		SELECT NULL AS aliMemberId, NULL as aId;  -- mismatch CustomerId and Ali memberID
		RETURN;
	END	
	
	SELECT @AliMemberId AS aliMemberId, @CustomerId as aId;
	
END
GO


