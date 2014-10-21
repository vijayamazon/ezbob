SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'CustomerLeadFieldNames')
BEGIN
	CREATE TABLE CustomerLeadFieldNames (
		LinkID BIGINT IDENTITY(1, 1) NOT NULL,
		UiControlName NVARCHAR(255),
		LeadDatumFieldName NVARCHAR(255),
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_CustomerLeadFieldNames PRIMARY KEY (LinkID),
		CONSTRAINT UC_CustomerLeadFieldNames UNIQUE (UiControlName)
	)
END
GO

CREATE TABLE #t (
	UiControlName NVARCHAR(255),
	LeadDatumFieldName NVARCHAR(255)
)

INSERT INTO #t (UiControlName, LeadDatumFieldName) VALUES
	('signup:email', 'email'),
	('signup:mobile-phone', 'mobile-phone')

INSERT INTO CustomerLeadFieldNames (UiControlName, LeadDatumFieldName)
SELECT
	#t.UiControlName,
	#t.LeadDatumFieldName
FROM
	#t
	LEFT JOIN CustomerLeadFieldNames l ON #t.UiControlName = l.UiControlName
WHERE
	l.LinkID IS NULL

DROP TABLE #t
GO
