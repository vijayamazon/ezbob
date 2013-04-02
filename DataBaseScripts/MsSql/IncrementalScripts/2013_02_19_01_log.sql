CREATE TABLE dbo.Tmp_PacnetPaypointServiceLog
    (
    Id bigint NOT NULL IDENTITY (1, 1),
    CustomerId bigint NULL,
    InsertDate datetime NULL,
    RequestType nvarchar(MAX) NULL,
    Status nvarchar(50) NULL,
    ErrorMessage nvarchar(MAX) NULL
    )  ON [PRIMARY]
     TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE dbo.Tmp_PacnetPaypointServiceLog SET (LOCK_ESCALATION = TABLE)
GO

SET IDENTITY_INSERT dbo.Tmp_PacnetPaypointServiceLog ON
GO

IF EXISTS(SELECT * FROM dbo.PacnetPaypointServiceLog)
     EXEC('INSERT INTO dbo.Tmp_PacnetPaypointServiceLog (Id, CustomerId, InsertDate, RequestType, Status, ErrorMessage)
        SELECT Id, CustomerId, InsertDate, CONVERT(nvarchar(MAX), RequestType), Status, ErrorMessage FROM dbo.PacnetPaypointServiceLog WITH (HOLDLOCK TABLOCKX)')
GO

SET IDENTITY_INSERT dbo.Tmp_PacnetPaypointServiceLog OFF
GO

DROP TABLE dbo.PacnetPaypointServiceLog
GO

EXECUTE sp_rename N'dbo.Tmp_PacnetPaypointServiceLog', N'PacnetPaypointServiceLog', 'OBJECT' 
GO

ALTER TABLE dbo.PacnetPaypointServiceLog ADD CONSTRAINT
    PK_PacnetServiceLog PRIMARY KEY CLUSTERED 
    (
    Id
    ) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX PacnetPaypointServiceLog_CustId ON dbo.PacnetPaypointServiceLog
    (
    CustomerId
    ) INCLUDE (InsertDate, RequestType, Status, ErrorMessage) 
 WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO