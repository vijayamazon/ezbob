IF (SELECT COUNT(*) FROM Security_Role s WHERE s.Name = 'Collector')  = 0 BEGIN

	declare @roleId INT
	DECLARE @permissionId INT = (SELECT sp.Id FROM Security_Permission sp WHERE NAME ='EditLoanDetails')
 
	INSERT INTO Security_Role(Name,[Description]) VALUES ('Collector', 'Collector - Allow change only Loans')
	SET @roleId = @@IDENTITY

	INSERT INTO Security_RolePermissionRel VALUES (@roleId, @permissionId)
END 
