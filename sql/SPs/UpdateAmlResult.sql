IF OBJECT_ID('UpdateAmlResult') IS NULL
	EXECUTE('CREATE PROCEDURE UpdateAmlResult AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UpdateAmlResult
@CustomerId INT,
@AmlResult NVARCHAR(100)
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE
		Customer
	SET
		AMLResult = @AmlResult
	WHERE
		Id = @CustomerId
END
GO
