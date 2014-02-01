ALTER PROCEDURE AV_GetExperianScore
 @CustomerId INT 
AS
BEGIN
	SET NOCOUNT ON
	SELECT TOP 1 JsonPacket, ExperianScore 
	FROM MP_ExperianDataCache 
	WHERE CustomerId=@CustomerId AND ExperianScore IS NOT NULL AND DirectorId=0
	ORDER BY LastUpdateDate DESC
SET NOCOUNT OFF
END

GO

