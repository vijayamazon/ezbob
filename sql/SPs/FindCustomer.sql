SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('FindCustomer') IS NULL
	EXECUTE('CREATE PROCEDURE FindCustomer AS SELECT 1')
GO

ALTER PROCEDURE FindCustomer
@Ntoken INT,
@Stoken NVARCHAR(128)
AS
BEGIN
	SET @Stoken = '%' + @Stoken + '%'

	SELECT TOP 20
		CustomerID = c.Id,
		CustomerName = ISNULL(c.FullName, ''),
		Email = c.Name,
		Origin = o.Name
	FROM
		Customer c
		INNER JOIN CustomerOrigin o ON c.OriginID = o.CustomerOriginID
	WHERE
		(
			(@Ntoken > 0 AND c.Id = @Ntoken)
		) OR (
			(@Stoken != '%%') AND ((LOWER(c.Fullname) LIKE @Stoken) OR (LOWER(c.Name) LIKE @Stoken))
		)
	ORDER BY
		c.Fullname,
		c.Name,
		o.Name
END
GO
