IF OBJECT_ID('WriteOffReasons') IS NULL 
BEGIN	
CREATE TABLE [dbo].[WriteOffReasons](
	[WriteOffReasonID] [INT] NOT NULL IDENTITY(1,1) ,
	[ReasonName] [nvarchar](100) NOT NULL,
	[TimestampCounter] rowversion NOT NULL,
 CONSTRAINT [PK_WriteOffReasons] PRIMARY KEY CLUSTERED ( [WriteOffReasonID] ASC)
) ;
END
GO

IF NOT EXISTS (SELECT * FROM WriteOffReasons WHERE ReasonName='FF Settlement')
BEGIN
	INSERT INTO WriteOffReasons (ReasonName) VALUES ('FF Settlement')
END	
GO

IF NOT EXISTS (SELECT * FROM WriteOffReasons WHERE ReasonName='Liquidation')
BEGIN
	INSERT INTO WriteOffReasons (ReasonName) VALUES ('Liquidation')
END	
GO

IF NOT EXISTS (SELECT * FROM WriteOffReasons WHERE ReasonName='Bankruptcy')
BEGIN
	INSERT INTO WriteOffReasons (ReasonName) VALUES ('Bankruptcy')
END	
GO

IF NOT EXISTS (SELECT * FROM WriteOffReasons WHERE ReasonName='Deceased')
BEGIN
	INSERT INTO WriteOffReasons (ReasonName) VALUES ('Deceased')
END	
GO

IF NOT EXISTS (SELECT * FROM WriteOffReasons WHERE ReasonName='UTC')
BEGIN
	INSERT INTO WriteOffReasons (ReasonName) VALUES ('UTC')
END	
GO

IF NOT EXISTS (SELECT * FROM WriteOffReasons WHERE ReasonName='Goneaway')
BEGIN
	INSERT INTO WriteOffReasons (ReasonName) VALUES ('Goneaway')
END	
GO

IF NOT EXISTS (SELECT * FROM WriteOffReasons WHERE ReasonName='Vulnerable')
BEGIN
	INSERT INTO WriteOffReasons (ReasonName) VALUES ('Vulnerable')
END	
GO

