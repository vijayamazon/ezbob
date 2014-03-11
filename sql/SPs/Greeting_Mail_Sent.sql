IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Greeting_Mail_Sent]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Greeting_Mail_Sent]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Greeting_Mail_Sent] 
	(@UserId int,
@GreetingMailSent int, 
@Now DATETIME)
AS
BEGIN
	declare @GreetingMailSentDate datetime  

set @GreetingMailSentDate = @Now

UPDATE [dbo].[Customer]
   SET [GreetingMailSent] = @GreetingMailSent, [GreetingMailSentDate] = @GreetingMailSentDate
 WHERE Id = @UserId

SET NOCOUNT ON;
SELECT @@IDENTITY
END
GO
