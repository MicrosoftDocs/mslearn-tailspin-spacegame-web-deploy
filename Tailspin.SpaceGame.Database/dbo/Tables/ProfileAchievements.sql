
CREATE TABLE [dbo].[ProfileAchievements] (
    [profileId]         INT           NOT NULL,
    [achievementId]     INT           NOT NULL,
    PRIMARY KEY CLUSTERED ([profileId], [achievementId])
);

GO

ALTER TABLE [dbo].[ProfileAchievements] WITH NOCHECK
    ADD CONSTRAINT [FK_ProfileAchievements_Profiles] FOREIGN KEY ([profileId]) REFERENCES [dbo].[Profiles] ([id]);
GO
ALTER TABLE [dbo].[ProfileAchievements] WITH NOCHECK
    ADD CONSTRAINT [FK_ProfileAchievements_Achievements] FOREIGN KEY ([achievementId]) REFERENCES [dbo].[Achievements] ([id]);
GO