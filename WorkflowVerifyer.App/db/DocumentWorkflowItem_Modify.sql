IF OBJECT_ID(N'dbo.DocumentWorkflowItem_Modify', N'P') IS NULL
	EXEC ('CREATE PROC dbo.DocumentWorkflowItem_Modify AS SET NOCOUNT ON;')
GO

ALTER PROCEDURE DocumentWorkflowItem_Modify

@a_DocumentWorkflowItemID int,
@a_DocumentationAnalystID int = NULL,
@a_ComplianceAnalystID int = NULL,
-- @a_CompanyID int = NULL,
-- @a_CompanyCertificateID int = NULL,
@a_Notes nvarchar(500) = NULL,
@a_DocumentWorkflowStatusID int = NULL

AS

BEGIN
    UPDATE      [DocumentWorkflowItem]
    SET         [DocumentationAnalystID] = @a_DocumentationAnalystID
                , [ComplianceAnalystID] = @a_ComplianceAnalystID
                , [Notes] = @a_Notes
                , [DocumentWorkflowStatusID] = @a_DocumentWorkflowStatusID       
    WHERE DocumentWorkflowItemID = @a_DocumentWorkflowItemID
    SELECT @a_DocumentWorkflowItemID
END