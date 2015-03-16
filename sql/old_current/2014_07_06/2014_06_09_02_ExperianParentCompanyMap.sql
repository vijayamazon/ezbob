IF OBJECT_ID('MP_ExperianParentCompanyMap') IS NULL
BEGIN
CREATE TABLE MP_ExperianParentCompanyMap
(
	 Id INT NOT NULL IDENTITY(1,1)
	,ExperianRefNum NVARCHAR(15)
	,ExperianParentRefNum NVARCHAR(15)
	,CONSTRAINT PK_MP_ExperianParentCompanyMap PRIMARY KEY (Id)
)

END	
GO
