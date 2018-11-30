IF OBJECT_ID(N'dbo.DocumentWorkflowItem_GetLatestForClient', N'P') IS NULL
	EXEC ('CREATE PROC dbo.DocumentWorkflowItem_GetLatestForClient AS SET NOCOUNT ON;')
GO

ALTER PROCEDURE DocumentWorkflowItem_GetLatestForClient

@a_ClientID int,
@a_LastRunTime datetime

AS

SELECT		dwi.[DocumentWorkflowItemID]
			, dwi.[ClientID]
			, dwi.[EmailDate]
			, dwi.[EmailToAddress]
			, dwi.[EmailFromAddress]
			, dwi.[EmailSubject]
			, dwi.[DocumentationAnalystID]
			, dwi.[ComplianceAnalystID]
			, dwi.[CompanyID]
			, dwi.[CompanyCertificateID]
			, dwi.[Notes]
			, dwi.[DocumentWorkflowStatusID]
			, cf.[FileName]
			, cf.[FileSize]
FROM		dbo.[DocumentWorkflowItem] as dwi
LEFT JOIN	dbo.[CertusFile] as cf ON dwi.[CertusFileID] = cf.[CertusFileID]
WHERE		dwi.[ClientID] = @a_ClientID
AND			dwi.[EmailDate] > @a_LastRunTime
AND			dwi.[DocumentWorkflowStatusID] <= 3
ORDER BY	dwi.[DocumentWorkflowItemID] DESC;