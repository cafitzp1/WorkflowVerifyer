IF OBJECT_ID(N'dbo.WorkflowVerification_Add', N'P') IS NULL
	EXEC ('CREATE PROC dbo.WorkflowVerification_Add AS SET NOCOUNT ON;')
GO

ALTER PROCEDURE WorkflowVerification_Add

@a_RunTime datetime,
@a_Summary varchar(50)

AS

BEGIN
	INSERT INTO WorkflowVerification(RunTime, Summary)
	VALUES(@a_RunTime, @a_Summary)
END