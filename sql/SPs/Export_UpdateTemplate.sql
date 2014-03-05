IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Export_UpdateTemplate]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Export_UpdateTemplate]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Export_UpdateTemplate] 
	(@pId             INT,
    @pFileName       NVARCHAR(MAX),
    @pDescription    NVARCHAR(MAX),
    @pExceptionType  INT,
    @pBinaryBody     VARBINARY(MAX),
    @pVariablesXml   NVARCHAR(MAX),
    @pUserId         INT,
    @pDisplayName    NVARCHAR(MAX),
    @pSignedDocument ntext)
AS
BEGIN
	IF @pId IS NULL
    BEGIN
        IF NOT EXISTS(
               SELECT *
               FROM   Export_TemplatesList etl
               WHERE  UPPER(etl.[FileName]) = UPPER(@pFileName) AND etl.IsDeleted IS NULL
           )
        BEGIN
        	UPDATE export_templateslist SET terminationdate = GETDATE()
        	WHERE upper(displayname) = upper(@pDisplayName) and terminationdate is null;
        	
            INSERT INTO export_templateslist
              (
                FILENAME,
                DESCRIPTION,
                binarybody,
                variablesxml,
                exceptiontype,
                userId,
                DisplayName,
                SignedDocument
              )
            VALUES
              (
                @pFileName,
                @pDescription,
                @pBinaryBody,
                @pVariablesXml,
                @pExceptionType,
                @pUserId,
                @pDisplayName,
                @pSignedDocument
              );
              SELECT @@IDENTITY;
        END
        ELSE
        	RAISERROR( 'dublicated_name', 16, 1);
        	SELECT NULL;
        	RETURN;

    END
    ELSE
    BEGIN
        UPDATE export_templateslist
        SET    DESCRIPTION = @pDescription,
               exceptiontype = @pExceptionType
        WHERE  id = @pId;
	SELECT @pId;
    END
END
GO
