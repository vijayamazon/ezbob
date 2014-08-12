IF OBJECT_ID('GetCompanyHistory') IS NULL
	EXECUTE('CREATE PROCEDURE GetCompanyHistory AS SELECT 1')
GO

ALTER PROCEDURE GetCompanyHistory
	(@RefNumber NVARCHAR(50))
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT 
		h.ServiceLogId,
		h.[Date],
		h.Score,
		h.CaisBalance AS Balance
	FROM 
		MP_ExperianHistory h
	WHERE 
		h.CompanyRefNum = @RefNumber 
END
GO