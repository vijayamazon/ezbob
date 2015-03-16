IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Perks]') AND type in (N'U'))
BEGIN
CREATE TABLE Perks(
	 Id INT NOT NULL IDENTITY(1,1)
	,ValidFrom DATETIME NOT NULL
	,ValidUntil DATETIME NOT NULL
	,Active BIT NOT NULL DEFAULT(0)
	,PerkHtml NVARCHAR(MAX) NOT NULL
	,CONSTRAINT PK_Perks PRIMARY KEY (Id))

	INSERT INTO Perks (ValidFrom,ValidUntil,Active,PerkHtml) VALUES ('2013-12-20', '2013-12-31', 1, '<div style=''width=100%;height=100%;''><a href=''https://google.com'' target=''_blank''><img src=''https://www.google.co.il/images/srpr/logo11w.png'' title=''google'' alt=''google'' style=''width=100%;height=100%;''/></a></div>')
END 	

