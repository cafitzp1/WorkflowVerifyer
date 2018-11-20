IF OBJECT_ID(N'dbo.WorkflowVerification_GetLatest', N'P') IS NULL
	EXEC ('CREATE PROC dbo.WorkflowVerification_GetLatest AS SET NOCOUNT ON;')
GO

ALTER PROCEDURE WorkflowVerification_GetLatest

AS

SELECT TOP 1 *
FROM WorkflowVerification
ORDER BY WorkflowVerification.RunTime DESC;