IF OBJECT_ID('GetCompanySeniority') IS NULL
	EXECUTE('CREATE PROCEDURE GetCompanySeniority AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetCompanySeniority
@CustomerId INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT 
		eda.JsonPacket AS CompanyData
	FROM 
		MP_ExperianDataCache eda
		INNER JOIN Company co ON eda.CompanyRefNumber = co.ExperianRefNum
		INNER JOIN Customer cu ON co.Id = cu.CompanyId
	WHERE 
		cu.Id = @CustomerId		
END
GO
