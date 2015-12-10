UPDATE CollectionSmsTemplate SET IsActive = 0 WHERE Type = 'CollectionDay1to6'
UPDATE CollectionSmsTemplate SET IsActive = 0 WHERE Type = 'CollectionDay8to14'
UPDATE CollectionSmsTemplate SET IsActive = 0 WHERE Type = 'CollectionDay15'

UPDATE CollectionSmsTemplate SET Template = '{0}.Please call the Everline collections team on 02036677519 to discuss the status of your account.', Comment = '{0} - FirstName' WHERE Type = 'CollectionDay0' AND OriginID =	2
UPDATE CollectionSmsTemplate SET Template = '{0}.Please call the Everline collections team on 02036677519 to discuss the status of your account.', Comment = '{0} - FirstName' WHERE Type = 'CollectionDay7' AND OriginID =	2
UPDATE CollectionSmsTemplate SET Template = '{0}.Please call the Everline collections team on 02036677519 to discuss the status of your account.' WHERE Type = 'CollectionDay21' AND OriginID =	2
UPDATE CollectionSmsTemplate SET Template = '{0}.Please call the Everline collections team on 02036677519 to discuss the status of your account.' WHERE Type = 'CollectionDay31' AND OriginID =	2
UPDATE CollectionSmsTemplate SET Template = '{0}.Please call ezbob to discuss the status of your account as soon as possible on 02036677519', Comment = '{0} - FirstName' WHERE Type = 'CollectionDay0' AND OriginID = 1
UPDATE CollectionSmsTemplate SET Template = '{0}.Please call ezbob to discuss the status of your account as soon as possible on 02036677519', Comment = '{0} - FirstName' WHERE Type = 'CollectionDay7' AND OriginID = 1
UPDATE CollectionSmsTemplate SET Template = '{0}.Please call ezbob to discuss the status of your account as soon as possible on 02036677519' WHERE Type = 'CollectionDay21' AND OriginID = 1
UPDATE CollectionSmsTemplate SET Template = '{0}.Please call ezbob to discuss the status of your account as soon as possible on 02036677519' WHERE Type = 'CollectionDay31' AND OriginID = 1

BEGIN
	IF NOT EXISTS (SELECT 1 FROM CollectionSmsTemplate WHERE [Type]='CollectionDay3' AND OriginID=1)
	BEGIN
		INSERT INTO CollectionSmsTemplate (Type, IsActive, OriginID, Template, Comment) VALUES ('CollectionDay3', 1, 1, '{0}.Please call ezbob to discuss the status of your account as soon as possible on 02036677519','{0} - FirstName')
	END
END

BEGIN
	IF NOT EXISTS (SELECT 1 FROM CollectionSmsTemplate WHERE [Type]='CollectionDay3' AND OriginID=2)
	BEGIN
		INSERT INTO CollectionSmsTemplate (Type, IsActive, OriginID, Template, Comment) VALUES ('CollectionDay3', 1, 2, '{0}.Please call the Everline collections team on 02036677519 to discuss the status of your account.','{0} - FirstName')
	END
END

BEGIN
	IF NOT EXISTS (SELECT 1 FROM CollectionSmsTemplate WHERE [Type]='CollectionDay10' AND OriginID=1)
	BEGIN
		INSERT INTO CollectionSmsTemplate (Type, IsActive, OriginID, Template, Comment) VALUES ('CollectionDay10', 1, 1, '{0}.Please call ezbob to discuss the status of your account as soon as possible on 02036677519','{0} - FirstName')
	END
END

BEGIN
	IF NOT EXISTS (SELECT 1 FROM CollectionSmsTemplate WHERE [Type]='CollectionDay10' AND OriginID=2)
	BEGIN
		INSERT INTO CollectionSmsTemplate (Type, IsActive, OriginID, Template, Comment) VALUES ('CollectionDay10', 1, 2, '{0}.Please call the Everline collections team on 02036677519 to discuss the status of your account.','{0} - FirstName')
	END
END

BEGIN
	IF NOT EXISTS (SELECT 1 FROM CollectionSmsTemplate WHERE [Type]='CollectionDay13' AND OriginID=1)
	BEGIN
		INSERT INTO CollectionSmsTemplate (Type, IsActive, OriginID, Template, Comment) VALUES ('CollectionDay13', 1, 1, '{0}.Please call ezbob to discuss the status of your account as soon as possible on 02036677519','{0} - FirstName')
	END
END

BEGIN
	IF NOT EXISTS (SELECT 1 FROM CollectionSmsTemplate WHERE [Type]='CollectionDay13' AND OriginID=2)
	BEGIN
		INSERT INTO CollectionSmsTemplate (Type, IsActive, OriginID, Template, Comment) VALUES ('CollectionDay13', 1, 2, '{0}.Please call the Everline collections team on 02036677519 to discuss the status of your account.','{0} - FirstName')
	END
END

GO
