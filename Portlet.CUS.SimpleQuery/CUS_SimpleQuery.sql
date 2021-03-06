USE [ICS_NET]
GO

/****** Object:  Table [dbo].[CUS_SimpleQuerySetting]    Script Date: 01/23/2012 18:43:22 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[CUS_SimpleQuerySetting](
	[ID] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[PortletID] [uniqueidentifier] NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[Value] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_CUS_SimpleQuerySetting] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[CUS_SimpleQuerySetting] ADD  CONSTRAINT [DF_CUS_SimpleQuerySetting_ID]  DEFAULT (newid()) FOR [ID]
GO



