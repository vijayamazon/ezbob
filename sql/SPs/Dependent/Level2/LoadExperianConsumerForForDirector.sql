IF OBJECT_ID('LoadExperianConsumerForDirector') IS NULL
	EXECUTE('CREATE PROCEDURE LoadExperianConsumerForDirector AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadExperianConsumerForDirector
@DirectorId INT
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @ServiceLogID BIGINT
	DECLARE @InsertDate DATETIME
	------------------------------------------------------------------------------

	SELECT TOP 1
		@ServiceLogID = Id, @InsertDate = InsertDate
	FROM
		MP_ServiceLog l
	WHERE
		l.DirectorId = @DirectorId
		AND
		l.ServiceType = 'Consumer Request'
	ORDER BY
		l.InsertDate DESC,
		l.Id DESC
		
	IF @ServiceLogId IS NULL
	BEGIN
		SELECT TOP 1
		   @ServiceLogID=l.Id, @InsertDate = l.InsertDate
		FROM
		 Director d 
		 INNER JOIN CustomerAddress a ON a.DirectorId = d.Id AND a.addressType=1
		 INNER JOIN MP_ServiceLog l on
		  l.Firstname = d.Name AND
		  l.Surname = d.Surname AND 
		  l.DateOfBirth = d.DateOfBirth AND
		  l.Postcode = a.Postcode AND
		  l.ServiceType = 'Consumer Request'
		  
		  WHERE
		   d.id=@DirectorId
		  ORDER BY
		   l.InsertDate DESC,
		   l.Id DESC
	END
	
	------------------------------------------------------------------------------

	EXECUTE LoadFullExperianConsumer @ServiceLogID, @InsertDate
END
GO
