IF object_id('CampaignMail') IS NULL
BEGIN
	CREATE TABLE CampaignMail(
	    Id INT NOT NULL IDENTITY(1,1)
	   ,Name NVARCHAR(300) NOT NULL
	   ,IsCampaign BIT NOT NULL -- if 1 campaign name else sourceref
	   ,CONSTRAINT PK_CampaignMail PRIMARY KEY (Id)
	)
END
GO
