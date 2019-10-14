UPDATE DBO.Profiles
SET favoriteMap='Milky Way'
WHERE CAST((id) AS int) % 3 = 0
GO
UPDATE DBO.Profiles
SET favoriteMap='Andromeda'
WHERE CAST((id) AS int) % 3 = 1
GO
UPDATE DBO.Profiles
SET favoriteMap='Pinwheel'
WHERE CAST((id) AS int) % 3 = 2
GO 