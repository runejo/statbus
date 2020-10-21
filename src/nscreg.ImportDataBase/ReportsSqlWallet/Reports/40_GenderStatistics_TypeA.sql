BEGIN /* INPUT PARAMETERS from report body */
	DECLARE @InStatusId NVARCHAR(MAX) = $StatusId,
			@InStatUnitType NVARCHAR(MAX) = $StatUnitType,
			@InPersonTypeId NVARCHAR(MAX) = $PersonTypeId,
			@InCurrentYear NVARCHAR(MAX) = YEAR(GETDATE()) - 1,
      @OblastLevel INT = dbo.GetOblastLevel();
END

/* checking if temporary table exists and deleting it if it is true */
IF (OBJECT_ID('tempdb..#tempTableForPivot') IS NOT NULL)
BEGIN DROP TABLE #tempTableForPivot END

/*
	table with count of employees of new stat units
	for each Sex, ActivityCategory with level = 1, and Oblast(region with level = 2)
*/
CREATE TABLE #tempTableForPivot
(
	Count NVARCHAR(MAX) NULL,
	Sex TINYINT NULL,
	Name NVARCHAR(MAX) NULL,
	NameOblast NVARCHAR(MAX) NULL
);

/* table where ActivityCategories linked to the greatest ancestor */
WITH ActivityCategoriesHierarchyCTE(Id,ParentId,Name,DesiredLevel) AS(
	SELECT
		Id,
		ParentId,
		Name,
		DesiredLevel
	FROM v_ActivityCategoriesHierarchy
	WHERE DesiredLevel=1
),
/* table where regions linked to their ancestor - oblast(region with level = 2) and superregion with Id = 1(level = 1) linked to itself */
RegionsHierarchyCTE AS(
	SELECT
		Id,
		ParentId,
		Name,
		RegionLevel,
		DesiredLevel
	FROM v_Regions
	/*
		If there no Country level in database, edit WHERE condition below from:
		DesiredLevel = 2 OR Id = 1
		To:
		DesiredLevel = 1
	*/
	WHERE DesiredLevel = 2 OR Id = 1
),
/* table with needed fields for previous states of stat units that were active in given dateperiod */
StatisticalUnitHistoryCTE AS (
	SELECT
		RegId,
    RegistrationDate,
    LiqDate,
		ParentId,
		AddressId,
		UnitStatusId,
		Discriminator,
		ROW_NUMBER() over (partition by ParentId order by StartPeriod desc) AS RowNumber
	FROM StatisticalUnitHistory
	WHERE DATEPART(YEAR,StartPeriod) < @InCurrentYear
),
/* list with all stat units linked to their primary ActivityCategory that were active in given dateperiod and have required StatUnitType */
ResultTableCTE AS
(
	SELECT
		IIF(DATEPART(YEAR,su.RegistrationDate) < @InCurrentYear AND DATEPART(YEAR,su.StartPeriod) < @InCurrentYear,su.RegId,suhCTE.RegId) AS RegId,
		IIF(DATEPART(YEAR,su.RegistrationDate) < @InCurrentYear AND DATEPART(YEAR,su.StartPeriod) < @InCurrentYear,a.ActivityCategoryId,ah.ActivityCategoryId) AS ActivityCategoryId,
		IIF(DATEPART(YEAR,su.RegistrationDate) < @InCurrentYear AND DATEPART(YEAR,su.StartPeriod) < @InCurrentYear,su.AddressId,suhCTE.AddressId) AS AddressId,
		IIF(DATEPART(YEAR,su.RegistrationDate) < @InCurrentYear AND DATEPART(YEAR,su.StartPeriod) < @InCurrentYear,psu.Person_Id, psuh.Person_Id) AS PersonId,
		IIF(DATEPART(YEAR,su.RegistrationDate) < @InCurrentYear AND DATEPART(YEAR,su.StartPeriod) < @InCurrentYear,su.RegistrationDate,suhCTE.RegistrationDate) AS RegistrationDate,
		IIF(DATEPART(YEAR,su.RegistrationDate) < @InCurrentYear AND DATEPART(YEAR,su.StartPeriod) < @InCurrentYear,su.UnitStatusId,suhCTE.UnitStatusId) AS UnitStatusId,
		IIF(DATEPART(YEAR,su.RegistrationDate) < @InCurrentYear AND DATEPART(YEAR,su.StartPeriod) < @InCurrentYear,su.LiqDate,suhCTE.LiqDate) AS LiqDate
	FROM dbo.StatisticalUnits AS su
		LEFT JOIN dbo.ActivityStatisticalUnits asu ON asu.Unit_Id = su.RegId
		LEFT JOIN dbo.Activities a ON a.Id = asu.Activity_Id
		LEFT JOIN dbo.PersonStatisticalUnits psu ON psu.Unit_Id = su.RegId

		LEFT JOIN StatisticalUnitHistoryCTE suhCTE ON suhCTE.ParentId = su.RegId and suhCTE.RowNumber = 1
		LEFT JOIN dbo.ActivityStatisticalUnitHistory asuh ON asuh.Unit_Id = suhCTE.RegId
		LEFT JOIN dbo.Activities ah ON ah.Id = asuh.Activity_Id
		LEFT JOIN dbo.PersonStatisticalUnitHistory psuh ON psuh.Unit_Id = su.RegId
	WHERE (@InStatUnitType ='All' OR @InStatUnitType = IIF(DATEPART(YEAR,su.RegistrationDate) < @InCurrentYear AND DATEPART(YEAR,su.StartPeriod) < @InCurrentYear,su.Discriminator,suhCTE.Discriminator))
			AND (@InStatusId = 0 OR @InStatusId = IIF(DATEPART(YEAR,su.RegistrationDate) < @InCurrentYear AND DATEPART(YEAR,su.StartPeriod) < @InCurrentYear,su.UnitStatusId,suhCTE.UnitStatusId))
			AND (@InPersonTypeId = 0 OR @InPersonTypeId = IIF(DATEPART(YEAR,su.RegistrationDate) < @InCurrentYear AND DATEPART(YEAR,su.StartPeriod) < @InCurrentYear,psu.PersonTypeId,psuh.PersonTypeId))
			AND IIF(DATEPART(YEAR,su.RegistrationDate) < @InCurrentYear AND DATEPART(YEAR,su.StartPeriod) < @InCurrentYear,a.Activity_Type,ah.Activity_Type) = 1
),
/* list of stat units linked to their oblast(region with level = 2) */
ResultTableCTE2 AS
(
	SELECT
		r.RegId,
		r.PersonId,
		p.Sex,
		ac.ParentId AS ActivityCategoryId,
		tr.Name AS RegionParentName,
		tr.ParentId AS RegionParentId
	FROM ResultTableCTE AS r
		LEFT JOIN ActivityCategoriesHierarchyCTE AS ac ON ac.Id = r.ActivityCategoryId
		LEFT JOIN dbo.Address AS addr ON addr.Address_id = r.AddressId
		INNER JOIN RegionsHierarchyCTE AS tr ON tr.Id = addr.Region_id
		INNER JOIN dbo.Persons AS p ON r.PersonId = p.Id
	WHERE DATEPART(YEAR, r.RegistrationDate) < @InCurrentYear AND r.PersonId IS NOT NULL
)

/* filling temporary table by all ActivityCategories with level=1 and number of employees in new stat units from ResultTableCTE linked to them as string, not number */
INSERT INTO #tempTableForPivot
SELECT
	STR(COUNT(rt.PersonId)) AS Count,
	rt.Sex,
	ac.Name,
	rt.RegionParentName + IIF(rt.Sex = 1, '1', '2') as NameOblast
FROM dbo.ActivityCategories as ac
	LEFT JOIN ResultTableCTE2 AS rt ON ac.Id = rt.ActivityCategoryId
	WHERE ac.ActivityCategoryLevel = 1
	GROUP BY ac.Name, rt.RegionParentName, rt.Sex

/*
	list of regions with level=2, that will be columns in report
	for select statement with replacing NULL values with zeroes as string
*/
DECLARE @colswithISNULL as NVARCHAR(MAX) = STUFF((SELECT distinct ', STR(ISNULL(' + QUOTENAME(Name + '1') + ', ''0''))  AS ' + QUOTENAME(Name + '1') + ', STR(ISNULL(' + QUOTENAME(Name + '2') + ', ''0''))  AS ' + QUOTENAME(Name + '2')
				/* set RegionLevel = 1 if there is no Country level at Regions tree */
				FROM dbo.Regions  WHERE RegionLevel = @OblastLevel
				FOR XML PATH(''), TYPE
				).value('.', 'NVARCHAR(MAX)')
			,1,2,'');

/* total sum of male persons for select statement */
DECLARE @totalMale AS NVARCHAR(MAX) = STUFF((SELECT distinct '+ ISNULL(CONVERT(INT, ' + QUOTENAME(Name + '1') + '), 0)'
				/* set RegionLevel = 1 if there is no Country level at Regions tree */
				FROM dbo.Regions  WHERE RegionLevel IN (@OblastLevel, @OblastLevel - 1)
				FOR XML PATH(''), TYPE
				).value('.', 'NVARCHAR(MAX)')
			,1,1,'')

/* total sum of female persons for select statement */
DECLARE @totalFemale AS NVARCHAR(MAX) = STUFF((SELECT distinct '+ISNULL(CONVERT(INT, ' + QUOTENAME(Name + '2') + '), 0)'
				/* set RegionLevel = 1 if there is no Country level at Regions tree */
				FROM dbo.Regions  WHERE RegionLevel IN (@OblastLevel, @OblastLevel - 1)
				FOR XML PATH(''), TYPE
				).value('.', 'NVARCHAR(MAX)')
			,1,1,'')

/* list of names of regions that were used in #tempTableForPivot */
DECLARE @namesRegionsForPivot AS NVARCHAR(MAX) = STUFF((SELECT distinct ',' + QUOTENAME(Name + '1') + ',' + QUOTENAME(Name + '2')
				/* set RegionLevel = 1 if there is no Country level at Regions tree */
				FROM dbo.Regions  WHERE RegionLevel IN (@OblastLevel, @OblastLevel - 1)
				FOR XML PATH(''), TYPE
				).value('.', 'NVARCHAR(MAX)')
			,1,1,'');

/* second line of headers, that will be used for naming columns as Male and Female */
DECLARE @maleFemaleLine AS NVARCHAR(MAX) = STUFF((SELECT distinct ', ''Male'' as ' + QUOTENAME(Name) + ', ''Female'' as ''                   '''
				/* set RegionLevel = 1 if there is no Country level at Regions tree */
				FROM dbo.Regions  WHERE RegionLevel = @OblastLevel
				FOR XML PATH(''), TYPE
				).value('.', 'NVARCHAR(MAX)')
			,1,1,'');

/*
	perform pivot on list of number of employees of each sex
	transforming names of regions to columns
	and uniting it with line of headers(maleFemaleLine)
*/
DECLARE @insertQuery AS NVARCHAR(MAX) = N'
SELECT '''' as Name, ''Male'' as Total, ''Female'' as ''    '', ' + @maleFemaleLine + '
UNION ALL
SELECT Name, STR(' + @totalMale + ') as [Total Male], STR(' + @totalFemale + ') as [Total Female], ' + @colswithISNULL + ' from
            (
				SELECT
					Count,
					Name,
					NameOblast
				FROM #tempTableForPivot
           ) SourceTable
            PIVOT
            (
                MAX(Count)
                FOR NameOblast IN (' + @namesRegionsForPivot + ')
            ) PivotTable order by Name'

execute(@insertQuery)
