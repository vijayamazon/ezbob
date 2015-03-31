IF object_id('FraudIovation') IS NULL
BEGIN 
CREATE TABLE FraudIovation(
	FraudIovationID INT NOT NULL IDENTITY(1,1),
	CustomerID INT NOT NULL,
	Created DATETIME NOT NULL,
	Origin NVARCHAR(20),
	Result CHAR,
	Reason NVARCHAR(40),
	TrackingNumber NVARCHAR(40),
	Score NVARCHAR(10),
	Details NVARCHAR(MAX),
	CONSTRAINT PK_FraudIovation PRIMARY KEY (FraudIovationID),
	CONSTRAINT FK_FraudIovation_Customer FOREIGN KEY (CustomerID) REFERENCES Customer(Id)
)
END
GO
