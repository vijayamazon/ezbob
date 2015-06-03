IF OBJECT_ID('GetCompanyAddress') IS NULL
	EXECUTE('CREATE PROCEDURE GetCompanyAddress AS SELECT 1')
GO

ALTER PROCEDURE GetCompanyAddress
	(@CustomerId int)
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT 
		a.[addressId]
      ,a.[addressType]
      ,a.[id]
      ,a.[Organisation]
      ,a.[Line1]
      ,a.[Line2]
      ,a.[Line3]
      ,a.[Town]
      ,a.[County]
      ,a.[Postcode]
      ,a.[Country]
      ,a.[Rawpostcode]
      ,a.[Deliverypointsuffix]
      ,a.[Nohouseholds]
      ,a.[Smallorg]
      ,a.[Pobox]
      ,a.[Mailsortcode]
      ,a.[Udprn]
      ,a.[CustomerId]
      ,a.[DirectorId]
      ,a.[CompanyId]
      ,a.[IsOwnerAccordingToLandRegistry]
      ,a.[TimestampCounter]
	FROM 
		[ezbob].[dbo].[Customer] cu
	LEFT JOIN [ezbob].[dbo].[Company] co ON cu.[CompanyId]=co.[id]
	LEFT JOIN [ezbob].[dbo].[CustomerAddress] a ON a.[CustomerId]=cu.[id] AND a.[CompanyID]=co.[id]
	WHERE 
		a.[CustomerId] = @CustomerId AND
		(a.[addressType] = 3 OR	a.[addressType] = 5)
END
GO
