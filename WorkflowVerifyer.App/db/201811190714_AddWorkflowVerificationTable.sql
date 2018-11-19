IF  NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WorkflowVerification]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[WorkflowVerification](
		[WorkflowVerificationID] [int] NOT NULL IDENTITY(1,1) PRIMARY KEY,
		[RunTime] [datetime] NOT NULL,
		[Summary] varchar(50) NOT NULL
	)
END