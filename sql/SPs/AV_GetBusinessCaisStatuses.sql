IF OBJECT_ID('AV_GetBusinessCaisStatuses') IS NULL
	EXECUTE('CREATE PROCEDURE AV_GetBusinessCaisStatuses AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE AV_GetBusinessCaisStatuses
	@CustomerId INT 
AS
BEGIN

	DECLARE @ExperianLtd INT = (SELECT isnull(max(e.ExperianLtdID), 0) 
								FROM Customer c 
								LEFT JOIN Company co ON c.CompanyId = co.Id 
								LEFT JOIN ExperianLtd e ON co.ExperianRefNum = e.RegisteredNumber
								WHERE c.Id = @CustomerId)
	
	SELECT CAST(c.DefaultBalance AS INT) AS  CurrentDefBalance, CAST(c.CurrentBalance AS INT) Balance, c.CAISLastUpdatedDate LastUpdatedDate, c.AccountStatusLast12AccountStatuses AccountStatusCodes
	FROM ExperianLtdDL97 c
	WHERE c.ExperianLtdID=@ExperianLtd
END

GO