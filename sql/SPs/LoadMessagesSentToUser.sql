SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadMessagesSentToUser') IS NULL
	EXECUTE('CREATE PROCEDURE LoadMessagesSentToUser AS SELECT 1')
GO

ALTER PROCEDURE LoadMessagesSentToUser
@UserID INT
AS
BEGIN
	SELECT
		Id = CONVERT(NVARCHAR(200), e.Id),
		RawCreationDate = e.CreationDate,
		RawFileName = e.FileName,
		IsOwn = CONVERT(BIT, CASE WHEN r.UserID IS NULL THEN 0 ELSE 1 END)
	FROM
		EzbobMailNodeAttachRelation r
		INNER JOIN Export_Results e ON r.ExportId = e.Id AND e.FileType = 0
		INNER JOIN Customer c ON c.Id = @UserID
	WHERE
		r.UserID = @UserID
		OR
		(r.ToField = c.Name AND r.UserID IS NULL)

	UNION

	SELECT
		Id = a.Guid,
		RawCreationDate = a.CreationDate,
		RawFileName = '',
		IsOwn = CONVERT(BIT, 1)
	FROM
		Askville a
		INNER JOIN MP_CustomerMarketPlace m
			ON a.MarketPlaceId = m.Id
			AND m.CustomerId = @UserID

	ORDER BY
		2 DESC
END
GO
