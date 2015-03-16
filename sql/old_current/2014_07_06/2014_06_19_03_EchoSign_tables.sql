SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('Esignatures') IS NULL
BEGIN
	CREATE TABLE Esignatures (
		EsignatureID BIGINT IDENTITY(1, 1) NOT NULL,
		CustomerID INT NOT NULL,
		EsignTemplateID INT NOT NULL,
		DocumentKey NVARCHAR(255) NOT NULL,
		SendDate DATETIME NOT NULL,
		StatusID INT NOT NULL,
		SignedDocumentMimeType NVARCHAR(255) NULL,
		SignedDocument VARBINARY(MAX) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_Esignature PRIMARY KEY (EsignatureID),
		CONSTRAINT FK_Esignature_Customer FOREIGN KEY (CustomerID) REFERENCES Customer (Id),
		CONSTRAINT FK_Esignature_Template FOREIGN KEY (EsignTemplateID) REFERENCES EsignTemplates (EsignTemplateID),
		CONSTRAINT FK_Esignature_Status FOREIGN KEY (StatusID) REFERENCES EsignAgreementStatus (StatusID)
	)
END
GO

IF OBJECT_ID('Esigners') IS NULL
BEGIN
	CREATE TABLE Esigners (
		EsignerID BIGINT IDENTITY(1, 1) NOT NULL,
		EsignatureID BIGINT NOT NULL,
		DirectorID INT NULL,
		StatusID INT NOT NULL,
		SignDate DATETIME NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_Esigner PRIMARY KEY (EsignerID),
		CONSTRAINT FK_Esigner_Signature FOREIGN KEY (EsignatureID) REFERENCES Esignatures (EsignatureID),
		-- CONSTRAINT FK_Esigner_Director FOREIGN KEY (DirectorID) REFERENCES Director (id),
		CONSTRAINT FK_Esigner_Status FOREIGN KEY (StatusID) REFERENCES EsignUserAgreementStatus (StatusID)
	)
END
GO
