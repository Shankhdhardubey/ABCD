
using DataModelMigration.Common;
using DataModelMigration.Service;

namespace DataModelMigration.Scripts.MigrationScript
{
    /// <summary>
    /// 
    /// </summary>
    public class InitialMigration
    {
        /// <summary>
        /// Adding source database as linked server to target
        /// Ref : https://www.sqlshack.com/create-configure-drop-sql-server-linked-server-using-transact-sql/
        /// </summary>

public string Policy_binder { get; set; }= $@"----------/**/----------
            insert into GH_Policy_binder 
             select* from HIUAGH_Policy_Binder";

        public string Create_Linked_Server { get; set; } = $@"----------/*Adding source database as linked server to target*/----------
if not exists (select * from sys.servers where name = N'{AppConfiguration.DataBase_Source_Server}')
begin
    EXEC sp_addlinkedserver 
    @server = N'{AppConfiguration.DataBase_Source_Server}',   
    @srvproduct=N'SQL Server'
end
";

        /// <summary>
        /// Giving user access to the linked server
        /// </summary>
        public string Add_User_Credentials_To_Linked_Server { get; set; } = $@"----------/*Giving user access to the linked server*/----------
EXEC sp_addlinkedsrvlogin   
@rmtsrvname = N'{AppConfiguration.DataBase_Source_Server}',
@locallogin = NULL ,
@useself = N'False',
@rmtuser = '{AppConfiguration.DataBase_Source_Credentials_UserName}',
@rmtpassword = '{AppConfiguration.DataBase_Source_Credentials_Password}'

EXEC sp_serveroption '{AppConfiguration.DataBase_Source_Server}', 'DATA ACCESS', TRUE
";
      
        /// <summary>
        /// SplitString_DataMigration function added in the Target database and removed after use
        /// </summary>
        public string Split_String { get; set; } = $@"----------/*User defined string split function */----------
CREATE FUNCTION SplitString_DataMigration
(    
      @Input NVARCHAR(MAX),
      @Character CHAR(1)
)
RETURNS @Output TABLE (
      value NVARCHAR(1000)
)
AS
BEGIN
      DECLARE @StartIndex INT, @EndIndex INT
      SET @StartIndex = 1
      IF SUBSTRING(@Input, LEN(@Input) - 1, LEN(@Input)) <> @Character
      BEGIN
            SET @Input = @Input + @Character
      END
      WHILE CHARINDEX(@Character, @Input) > 0
      BEGIN
            SET @EndIndex = CHARINDEX(@Character, @Input)
            INSERT INTO @Output(value)
            SELECT SUBSTRING(@Input, @StartIndex, @EndIndex - 1)           
            SET @Input = SUBSTRING(@Input, @EndIndex + 1, LEN(@Input))
      END
      RETURN
END
";
        /// <summary>
        /// 
        /// </summary>
//        public string Get_Source_version { get; set; } = $@"----------/**/----------
// Declare @valSourceStart VARCHAR(100)
// Declare @valSourceEnd VARCHAR(100)
// Declare @valSouredata VARCHAR(100)
// Declare @lenSourceStart INT
// Declare @lenSourceEnd INT
// Declare @lenSouredata INT
// Declare @FinalResult VARCHAR(100)
// Declare @cntSourceStart INT
// Declare @cntSouredata INT
// Set @valSourceStart = '{AppConfiguration.DataBaseVersion_Source_Start}'
// Set @valSourceEnd = '{AppConfiguration.DataBaseVersion_Source_End}'

//set @valSouredata  = (select PCI.latest_client_version from {AppConstants.SourceDatabaseWithServer}[PMProduct_Client_Install]  AS PCI 
//join {AppConstants.SourceDatabaseWithServer}[PMProduct] as PMP ON PMP.pmproduct_id = PCI.pmproduct_id 
//where PMP.[description] in ('Back-office','Pure Back-office'))

//set @cntSourceStart = (SELECT Count(value) FROM {AppConstants.TargetDatabaseWithServer}SplitString_DataMigration(@valSourceStart, '.'))
//set @cntSouredata = (SELECT Count(value) FROM {AppConstants.TargetDatabaseWithServer}SplitString_DataMigration(@valSouredata, '.'))
//if(@cntSourceStart <> @cntSouredata)
//BEGIN
//	set @valSouredata =  @valSouredata + '.0'
//END
//Set @lenSourceStart = LEN(@valSourceStart)
//Set @lenSourceEnd = LEN(@valSourceEnd)
//Set @lenSouredata = LEN(@valSouredata)
//-------------------------------------------------------------------------------------
//if(@lenSourceStart = @lenSouredata AND @lenSourceEnd =  @lenSouredata)
//BEGIN
//	DECLARE @VALUE1 TABLE(ID INT IDENTITY(1,1),VER_VALUE VARCHAR(10))
//	INSERT INTO @VALUE1
//	SELECT value FROM SplitString_DataMigration(@valSouredata, '.') 

//	DECLARE @VALUE2 TABLE(ID INT IDENTITY(1,1),VER_VALUE VARCHAR(10))
//	INSERT INTO @VALUE2
//	SELECT  value FROM SplitString_DataMigration(@valSourceStart, '.') 

//	DECLARE @COUNT INT=0
//	DECLARE @INDEX INT=1
//	SELECT @COUNT=ISNULL(COUNT(*),0) FROM @VALUE1

//	DECLARE @RESULT INT=0

//	WHILE(@INDEX<=@COUNT)
//	BEGIN

//	DECLARE @VAL1 INT
//	SELECT @VAL1=CAST(VER_VALUE AS INT) FROM @VALUE1 WHERE ID=@INDEX

//	DECLARE @VAL2 INT
//	SELECT @VAL2=CAST(VER_VALUE AS INT) FROM @VALUE2 WHERE ID=@INDEX

//	IF(@VAL1>=@VAL2)
//		SET @RESULT=1
//	ELSE
//		SET @RESULT=-1

//	IF(@RESULT=1)
//		SET @INDEX=@INDEX+1
//	ELSE
//		SET @INDEX=@COUNT+1
//	END
//--------------------------------------------------------------
//	DECLARE @VALUE11 TABLE(ID INT IDENTITY(1,1),VER_VALUE VARCHAR(10))
//	INSERT INTO @VALUE11
//	SELECT value FROM {AppConstants.TargetDatabaseWithServer}SplitString_DataMigration(@valSouredata, '.') 

//	DECLARE @VALUE22 TABLE(ID INT IDENTITY(1,1),VER_VALUE VARCHAR(10))
//	INSERT INTO @VALUE22
//	SELECT  value FROM {AppConstants.TargetDatabaseWithServer}SplitString_DataMigration(@valSourceEnd, '.') 

//	DECLARE @COUNT1 INT=0
//	DECLARE @INDEX1 INT=1
//	SELECT @COUNT1=ISNULL(COUNT(*),0) FROM @VALUE11

//	DECLARE @RESULT1 INT=0

//	WHILE(@INDEX1<=@COUNT1)
//	BEGIN

//	DECLARE @VAL11 INT
//	SELECT @VAL11=CAST(VER_VALUE AS INT) FROM @VALUE11 WHERE ID=@INDEX1

//	DECLARE @VAL22 INT
//	SELECT @VAL22=CAST(VER_VALUE AS INT) FROM @VALUE22 WHERE ID=@INDEX1

//    IF(@VAL11<=@VAL22)
//		SET @RESULT1=1
//	ELSE
//		SET @RESULT1=-1

//	IF(@RESULT1=1)
//		SET @INDEX1=@INDEX1+1
//	ELSE
//		SET @INDEX1=@COUNT1+1
//	END
//------------------------------------------------------------------------------
//	IF(@RESULT = 1 AND  @RESULT1 = 1)
//		SET @FinalResult = '1'
//	ELSE
//    BEGIN
//		SET @FinalResult = '0'
//        DROP FUNCTION SplitString_DataMigration
//    END
//END
//ELSE
//BEGIN
//	SET @FinalResult = '0'
//    DROP FUNCTION SplitString_DataMigration
//END
//	SELECT @FinalResult AS Script_Log
//";

//        /// <summary>
//        /// 
//        /// </summary>
//        public string Get_Target_version { get; set; } = $@"----------/**/----------
// Declare @valTargetStart VARCHAR(100)
// Declare @valTargetEnd VARCHAR(100)
// Declare @valTargetData VARCHAR(100)
// Declare @lenTargetStart INT
// Declare @lenTargetEnd INT
// Declare @lenTargetData INT
// Declare @FinalResult VARCHAR(100)
// Set @valTargetStart = '{AppConfiguration.DataBaseVersion_Target_Start}'
// Set @valTargetEnd = '{AppConfiguration.DataBaseVersion_Target_End}'

//set @valTargetData  = (select PCI.latest_client_version from {AppConstants.TargetDatabaseWithServer}[PMProduct_Client_Install]  AS PCI 
//join {AppConstants.TargetDatabaseWithServer}[PMProduct] as PMP ON PMP.pmproduct_id = PCI.pmproduct_id 
//where PMP.[description] in ('Back-office','Pure Back-office'))

//Set @lenTargetStart = LEN(@valTargetStart)
//Set @lenTargetEnd = LEN(@valTargetEnd)
//Set @lenTargetData = LEN(@valTargetData)
//-------------------------------------------------------------------------------------
//if(@lenTargetStart = @lenTargetData AND @lenTargetEnd =  @lenTargetData)
//BEGIN
//	DECLARE @VALUE1 TABLE(ID INT IDENTITY(1,1),VER_VALUE VARCHAR(10))
//	INSERT INTO @VALUE1
//	SELECT value FROM {AppConstants.TargetDatabaseWithServer}SplitString_DataMigration(@valTargetData, '.') 

//	DECLARE @VALUE2 TABLE(ID INT IDENTITY(1,1),VER_VALUE VARCHAR(10))
//	INSERT INTO @VALUE2
//	SELECT  value FROM {AppConstants.TargetDatabaseWithServer}SplitString_DataMigration(@valTargetStart, '.') 

//	DECLARE @COUNT INT=0
//	DECLARE @INDEX INT=1
//	SELECT @COUNT=ISNULL(COUNT(*),0) FROM @VALUE1

//	DECLARE @RESULT INT=0

//	WHILE(@INDEX<=@COUNT)
//	BEGIN

//	DECLARE @VAL1 INT
//	SELECT @VAL1=CAST(VER_VALUE AS INT) FROM @VALUE1 WHERE ID=@INDEX

//	DECLARE @VAL2 INT
//	SELECT @VAL2=CAST(VER_VALUE AS INT) FROM @VALUE2 WHERE ID=@INDEX

//	IF(@VAL1>=@VAL2)
//		SET @RESULT=1
//	ELSE
//		SET @RESULT=-1

//	IF(@RESULT=1)
//		SET @INDEX=@INDEX+1
//	ELSE
//		SET @INDEX=@COUNT+1
//	END
//--------------------------------------------------------------
//	DECLARE @VALUE11 TABLE(ID INT IDENTITY(1,1),VER_VALUE VARCHAR(10))
//	INSERT INTO @VALUE11
//	SELECT value FROM {AppConstants.TargetDatabaseWithServer}SplitString_DataMigration(@valTargetData, '.') 

//	DECLARE @VALUE22 TABLE(ID INT IDENTITY(1,1),VER_VALUE VARCHAR(10))
//	INSERT INTO @VALUE22
//	SELECT  value FROM {AppConstants.TargetDatabaseWithServer}SplitString_DataMigration(@valTargetEnd, '.') 

//	DECLARE @COUNT1 INT=0
//	DECLARE @INDEX1 INT=1
//	SELECT @COUNT1=ISNULL(COUNT(*),0) FROM @VALUE11

//	DECLARE @RESULT1 INT=0

//	WHILE(@INDEX1<=@COUNT1)
//	BEGIN

//	DECLARE @VAL11 INT
//	SELECT @VAL11=CAST(VER_VALUE AS INT) FROM @VALUE11 WHERE ID=@INDEX1

//	DECLARE @VAL22 INT
//	SELECT @VAL22=CAST(VER_VALUE AS INT) FROM @VALUE22 WHERE ID=@INDEX1

//	IF(@VAL11<=@VAL22)
//		SET @RESULT1=1
//	ELSE
//		SET @RESULT1=-1

//	IF(@RESULT1=1)
//		SET @INDEX1=@INDEX1+1
//	ELSE
//		SET @INDEX1=@COUNT1+1
//	END
//------------------------------------------------------------------------------
//	IF(@RESULT = 1 AND @RESULT1 = 1)
//		SET @FinalResult = '1'
//	ELSE
//		SET @FinalResult = '0'
//END
//ELSE
//BEGIN
//	SET @FinalResult = '0'
//END
//	SELECT @FinalResult AS Script_Log
//  IF EXISTS(
//    SELECT* FROM sysobjects WHERE id = object_id(N'SplitString_DataMigration')
//    AND xtype IN (N'FN', N'IF', N'TF')
//)
//DROP FUNCTION SplitString_DataMigration
//";

        /// <summary>
        /// 
        /// </summary>
//        public string CheckIf_DataModel_Screen_Exists { get; set; } = $@"----------/**/----------
//DECLARE @logData VARCHAR(Max) = ''

//IF ((DB_ID('{AppConfiguration.DataBase_Temp_Name}') is null) or (SELECT 1 FROM {AppConfiguration.DataBase_Target_Name}.sys.tables WHERE name = 'GIS_Data_ModelMap') = 1)
//begin
//    select pbp.gis_data_model_id, pbp.code
//    into #TempTable
//    from {AppConstants.SourceDatabaseWithServer}[GIS_Data_Model] pbp
//    inner join {AppConstants.TargetDatabaseWithServer}[GIS_Data_Model] pip on pip.Code = pbp.code
//    where pbp.code not in ({AppConfiguration.BlackListDataModel}) 
//    and pbp.gis_data_model_id in 
//    (
//        select distinct(p.gis_data_model_id) from {AppConstants.SourceDatabaseWithServer}[GIS_Policy_Link] p
//    	inner join {AppConstants.SourceDatabaseWithServer}[Insurance_File] i on i.insurance_file_cnt = p.insurance_file_cnt 
//    	inner join {AppConstants.SourceDatabaseWithServer}[Risk_Code] rc on rc.risk_code_id = i.risk_code_id
//    	inner join {AppConstants.SourceDatabaseWithServer}[Risk_Group] rg on rc.risk_group_id = rg.risk_group_id
//    	inner join {AppConstants.SourceDatabaseWithServer}[GIS_Screen] gs on gs.gis_screen_id = rg.gis_screen_id and gs.gis_data_model_id = p.gis_data_model_id
//    )
    
//    select 
//    @logData = COALESCE(@logData + ', ', '')+ ' Datamodel (' + ltrim(rtrim(code)) + ') already exist in target database'
//    from #TempTable
    
//    select 
//    @logData = COALESCE(@logData + ', ', '')+ ' Screen (' + ltrim(rtrim(pbp_gs.code)) + ') already exist in target database'
//    from {AppConstants.SourceDatabaseWithServer}[GIS_Screen] pbp_gs
//    inner join {AppConstants.TargetDatabaseWithServer}[GIS_Screen] pip_gs on pip_gs.Code = pbp_gs.code
//    where pbp_gs.gis_data_model_id in (select gis_data_model_id from #TempTable)
    
//    drop table #TempTable
//end

//select @logData As Script_Log
//";

        /// <summary>
        /// Log migration version/system versions at start
        /// </summary>
//        public string Log_Migration_Version_Start { get; set; } = $@"----------/*Log migration version/system versions at start*/----------
//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Target_Name}.sys.tables WHERE name = 'Migration_Run')
//begin
//	CREATE TABLE {AppConstants.TargetDatabaseWithoutServer}[Migration_Run]
//	(
//		[Migration_Run_Id] [int] IDENTITY(1,1) NOT NULL,
//		[Start_Date] [datetime] NOT NULL,
//		[End_Date] [datetime] NULL,
//		CONSTRAINT [PK_Migration_Run] PRIMARY KEY CLUSTERED ([Migration_Run_Id] ASC)
//	)

//	CREATE TABLE {AppConstants.TargetDatabaseWithoutServer}[Migration_Run_Details]
//	(
//		[Migration_Run_Details_Id] [int] IDENTITY(1,1) NOT NULL,
//		[Migration_Run_Id] [int] NOT NULL,
//		[Configuration_Item] [nvarchar](100) NOT NULL,
//		[Configuration_Setting] [nvarchar](max) NOT NULL,
//		CONSTRAINT [PK_Migration_Run_Details] PRIMARY KEY CLUSTERED ([Migration_Run_Details_Id] ASC)
//	)

//	ALTER TABLE {AppConstants.TargetDatabaseWithoutServer}[Migration_Run_Details] WITH CHECK ADD CONSTRAINT [FK_Migration_Run_Details_Migration_Run] FOREIGN KEY([Migration_Run_Id])
//	REFERENCES {AppConstants.TargetDatabaseWithoutServer}[Migration_Run] ([Migration_Run_Id])
//end

//declare @migration_Run_Id int;
//insert into {AppConstants.TargetDatabaseWithServer}[Migration_Run] (Start_Date)  values (GetDate())
//set @migration_Run_Id = Scope_Identity()

//insert into {AppConstants.TargetDatabaseWithServer}[Migration_Run_Details] (Migration_Run_Id, Configuration_Item, Configuration_Setting) (select @migration_Run_Id as Migration_Run_Id, 'PB, ' + ltrim(rtrim(p.code)) as Configuration_Item, pc.required_server_version + ', ' + convert(varchar, pc.server_software_date, 121) as Configuration_Setting
//from {AppConstants.SourceDatabaseWithServer}[PMProduct_Client_Install] pc
//inner join {AppConstants.SourceDatabaseWithServer}[PMProduct] p on pc.pmproduct_id = p.pmproduct_id)

//insert into {AppConstants.TargetDatabaseWithServer}[Migration_Run_Details] (Migration_Run_Id, Configuration_Item, Configuration_Setting) (select @migration_Run_Id as Migration_Run_Id, 'SSPB, ' + ltrim(rtrim(p.code)) as Configuration_Item, pc.required_server_version + ', ' + convert(varchar, pc.server_software_date, 121) as Configuration_Setting
//from {AppConstants.TargetDatabaseWithServer}[PMProduct_Client_Install] pc
//inner join {AppConstants.TargetDatabaseWithServer}[PMProduct] p on pc.pmproduct_id = p.pmproduct_id)

//insert into {AppConstants.TargetDatabaseWithServer}[Migration_Run_Details] (Migration_Run_Id, Configuration_Item, Configuration_Setting) values (@migration_Run_Id, 'Migration Version', '{AppConfiguration.Version}')

//insert into {AppConstants.TargetDatabaseWithServer}[Migration_Run_Details] (Migration_Run_Id, Configuration_Item, Configuration_Setting) values (@migration_Run_Id, 'Migration Configuration', '{AppConstants.ConfigSettings}')

//declare @migrationMode nvarchar(max) = '';
//if ('{AppConfiguration.MigrateProductOnly}' = 'True')
//begin
//    set @migrationMode = '{AppConfiguration.MigrationMode_Product}';
//end
//else
//begin
//    if ('{AppConfiguration.MigrateConfigurationOnly}' = 'True')
//    begin
//        set @migrationMode = '{AppConfiguration.MigrationMode_Configuration}';
//    end
//    else
//    begin
//        set @migrationMode = '{AppConfiguration.MigrationMode_User}';
//    end
//end

//insert into {AppConstants.TargetDatabaseWithServer}[Migration_Run_Details] (Migration_Run_Id, Configuration_Item, Configuration_Setting) values (@migration_Run_Id, 'Migration Mode', @migrationMode)
//";

        /// <summary>
        /// Creating temp database at target for identity
        /// </summary>
//        public string Create_Temp_Database { get; set; } = $@"----------/*Creating temp database at target for identity*/----------
//IF (DB_ID('{AppConfiguration.DataBase_Temp_Name}') is null)
//begin
//    create database {AppConfiguration.DataBase_Temp_Name}
//end
//";

        /// <summary>
        /// Creating temp table TableDataCountBeforeMigration in temp database
        /// </summary>
//        public string Temp_TableDataCountBeforeMigration_Table { get; set; } = $@"----------/*Creating temp table TableDataCountBeforeMigration in temp database*/----------
//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'TableDataCountBeforeMigration')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}TableDataCountBeforeMigration  (ID INT IDENTITY(1,1),TABLE_NAME VARCHAR(100),COLUMN_NAME VARCHAR(100), COLUMN_MAX_VALUE INT, Migration_Run_Id int not null)
//end
//";

        /// <summary>
        /// Adding data to temp table TableDataCountBeforeMigration in temp database
        /// </summary>
//        public string Temp_TableDataCountBeforeMigration { get; set; } = $@"----------/*Adding data to temp table TableDataCountBeforeMigration in temp database*/----------
//declare @migration_Run_Id int = (select max(migration_run_id) from {AppConstants.TargetDatabaseWithServer}[Migration_Run]);

//INSERT INTO {AppConstants.TempDatabaseWithoutServer}TableDataCountBeforeMigration(TABLE_NAME,COLUMN_NAME,COLUMN_MAX_VALUE, Migration_Run_Id)
//SELECT TABLE_NAME,COLUMN_NAME,0, @migration_Run_Id
//FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
//WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + QUOTENAME(CONSTRAINT_NAME)), 'IsPrimaryKey') = 1
//AND TABLE_NAME in
//(SELECT TABLE_NAME
//FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
//WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + QUOTENAME(CONSTRAINT_NAME)), 'IsPrimaryKey') = 1
//AND TABLE_NAME in(
// SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE  CONSTRAINT_TYPE = 'PRIMARY KEY' AND
//        TABLE_NAME in (SELECT TABLE_NAME FROM {AppConfiguration.DataBase_Target_Name}.INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE')
//     AND TABLE_SCHEMA ='dbo')  
//AND TABLE_SCHEMA = 'dbo' group by TABLE_NAME HAVING COUNT(COLUMN_NAME) = 1)
//AND TABLE_SCHEMA = 'dbo' AND TABLE_NAME NOT IN ( 'Script_Update','Unique_Number','wp_fields')

//SELECT * into #TempTable FROM {AppConstants.TempDatabaseWithServer}[TableDataCountBeforeMigration] where Migration_Run_Id = @migration_Run_Id
//WHILE EXISTS (SELECT * from #TempTable)
//BEGIN
//	DECLARE @QRY VARCHAR(MAX)
//	DECLARE @TABLE_NAME VARCHAR(MAX)
//	DECLARE @COLUMN_NAME VARCHAR(MAX)
//	declare @id int;
	
//	select @TABLE_NAME = TABLE_NAME, @COLUMN_NAME = COLUMN_NAME, @id = Id FROM #TempTable

//	SET @QRY='UPDATE {AppConstants.TempDatabaseWithServer}[TableDataCountBeforeMigration] SET COLUMN_MAX_VALUE= ISNULL((SELECT MAX('+@COLUMN_NAME+') FROM [' + @TABLE_NAME + ']), 0) WHERE ID = ' + CAST(@id AS varchar) 
//	EXEC(@QRY)
//	delete from #TempTable where id = @id
//END  
//drop table #TempTable 
//";

        /// <summary>
        /// 
        /// </summary>
//        public string CreateMigrationReport { get; set; } = $@"----------/**/----------
//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Target_Name}.sys.tables WHERE name = 'Migration_Report')
//begin
//    CREATE TABLE {AppConstants.TargetDatabaseWithoutServer}[Migration_Report](
//    	[Migration_Report_Id] [int] IDENTITY(1,1) NOT NULL,
//    	[Migration_Run_Id] [int] NOT NULL,
//    	[Process_Name] [nvarchar](200) NOT NULL,
//    	[PB_Table_Name] [nvarchar](100) NULL,
//    	[SSPBroking_Table_Name] [nvarchar](100) NOT NULL,
//    	[Category] [nvarchar](200) NULL,
//    	[PB_Rows] [bigint] NULL,
//    	[SSPBroking_Rows_Inserted] [bigint] NOT NULL,
//    	[SSPBroking_Rows_Updated] [bigint] NOT NULL,
//    	[Is_Filtered] [bit] NOT NULL,
//    	[Comment] [nvarchar](500) NULL,
//    	[Executed_On] [datetime] NOT NULL,
//     CONSTRAINT [PK_Migration_Report] PRIMARY KEY CLUSTERED 
//    (
//    	[Migration_Report_Id] ASC
//    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
//    ) ON [PRIMARY]
    
//    ALTER TABLE {AppConstants.TargetDatabaseWithoutServer}[Migration_Report]  WITH CHECK ADD  CONSTRAINT [FK_Migration_Report_Migration_Run] FOREIGN KEY([Migration_Run_Id])
//    REFERENCES {AppConstants.TargetDatabaseWithoutServer}[Migration_Run] ([Migration_Run_Id])
    
//    ALTER TABLE {AppConstants.TargetDatabaseWithoutServer}[Migration_Report] CHECK CONSTRAINT [FK_Migration_Report_Migration_Run]
//end
//";

        /// <summary>
        /// 
        /// </summary>
//        public string MigrationTable { get; set; } = $@"----------/**/----------
//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Target_Name}.sys.tables WHERE name = 'MigrationTable')
//begin
//    CREATE TABLE {AppConstants.TargetDatabaseWithoutServer}[MigrationTable](
//    	[MigrationTable_Id] [int] IDENTITY(1,1) NOT NULL,
//    	[Migration_Run_Id] [int] NOT NULL,
//    	[Table_Name] [nvarchar](100) NOT NULL,
//     CONSTRAINT [PK_MigrationTable] PRIMARY KEY CLUSTERED 
//    (
//    	[MigrationTable_Id] ASC
//    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
//    ) ON [PRIMARY]
    
//    ALTER TABLE {AppConstants.TargetDatabaseWithoutServer}[MigrationTable]  WITH CHECK ADD  CONSTRAINT [FK_MigrationTable_Migration_Run] FOREIGN KEY([Migration_Run_Id])
//    REFERENCES {AppConstants.TargetDatabaseWithoutServer}[Migration_Run] ([Migration_Run_Id])
    
//    ALTER TABLE {AppConstants.TargetDatabaseWithoutServer}[MigrationTable] CHECK CONSTRAINT [FK_MigrationTable_Migration_Run]
//end
//";

        /// <summary>
        /// 
        /// </summary>
//        public string MigrationTableRange { get; set; } = $@"----------/**/----------
//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Target_Name}.sys.tables WHERE name = 'MigrationTableRange')
//begin
//    CREATE TABLE {AppConstants.TargetDatabaseWithoutServer}[MigrationTableRange](
//    	[MigrationTableRange_Id] [int] IDENTITY(1,1) NOT NULL,
//    	[MigrationTable_Id] [int] NOT NULL,
//    	[Start_Id] [int] NULL,
//    	[End_Id] [int] NOT NULL,
//     CONSTRAINT [PK_MigrationTableRange] PRIMARY KEY CLUSTERED 
//    (
//    	[MigrationTableRange_Id] ASC
//    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
//    ) ON [PRIMARY]
    
//    ALTER TABLE {AppConstants.TargetDatabaseWithoutServer}[MigrationTableRange]  WITH CHECK ADD  CONSTRAINT [FK_MigrationTableRanges_MigrationTable] FOREIGN KEY([MigrationTable_Id])
//    REFERENCES {AppConstants.TargetDatabaseWithoutServer}[MigrationTable] ([MigrationTable_Id])
    
//    ALTER TABLE {AppConstants.TargetDatabaseWithoutServer}[MigrationTableRange] CHECK CONSTRAINT [FK_MigrationTableRanges_MigrationTable]
//end
//";

        /// <summary>
        /// 
        /// </summary>
//        public string MigrationTableRowsTemp { get; set; } = $@"----------/**/----------
//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'MigrationTableRowsTemp')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[MigrationTableRowsTemp](
//        [MigrationTableRowsTemp_Id] [int] IDENTITY(1,1) NOT NULL, 
//        [Table_Name] [nvarchar](100) NOT NULL, 
//        [InitialRow_Xml] [xml] NULL,
//        [Migration_Run_Id] [int] NOT NULL, 
//    CONSTRAINT [PK_MigrationTableRowsTemp] PRIMARY KEY CLUSTERED ([MigrationTableRowsTemp_Id] ASC))
//end
//";

        /// <summary>
        /// 
        /// </summary>
//        public string MigrationTableRowsTempData { get; set; } = $@"----------/**/----------
//declare @migration_Run_Id int = (select max(migration_run_id) from {AppConstants.TargetDatabaseWithServer}[Migration_Run]);

//{new DataMigrationReport().ScriptForTablesWithComplexPrimaryKeyInitial()}
//";

        /// <summary>
        /// 
        /// </summary>
//        public string MigrationTableRows { get; set; } = $@"----------/**/----------
//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Target_Name}.sys.tables WHERE name = 'MigrationTableRows')
//begin
//    CREATE TABLE {AppConstants.TargetDatabaseWithoutServer}[MigrationTableRows](
//    	[MigrationTableRow_Id] [int] IDENTITY(1,1) NOT NULL,
//    	[MigrationTable_Id] [int] NOT NULL,
//    	[Row_Xml] [xml] NULL,
//     CONSTRAINT [PK_MigrationTableRows] PRIMARY KEY CLUSTERED 
//    (
//    	[MigrationTableRow_Id] ASC
//    )) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
    
//    ALTER TABLE {AppConstants.TargetDatabaseWithoutServer}[MigrationTableRows]  WITH CHECK ADD  CONSTRAINT [FK_MigrationTableRows_MigrationTable] FOREIGN KEY([MigrationTableRow_Id])
//    REFERENCES {AppConstants.TargetDatabaseWithoutServer}[MigrationTable] ([MigrationTable_Id])
    
//    ALTER TABLE {AppConstants.TargetDatabaseWithoutServer}[MigrationTableRows] CHECK CONSTRAINT [FK_MigrationTableRows_MigrationTable]
//end
//";

        /// <summary>
        /// 
        /// </summary>
//        public string Create_Party_Account_TempTable { get; set; } = $@"----------/**/----------
//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'CaptionMap')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[CaptionMap]([pb_caption_id] [int] NULL, [pi_caption_id] [int] NULL) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'GIS_Data_ModelMap')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[GIS_Data_ModelMap]([pb_GIS_Data_Model_id] [int] NULL, [pi_GIS_Data_Model_id] [bigint] NULL, [Is_Already_Added] [bit] NOT NULL DEFAULT ((0))) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'PartyMap')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[PartyMap]([pb_party_cnt] [int] NOT NULL, [pi_party_cnt] [bigint] NULL, [Is_Already_Added] [bit] NOT NULL DEFAULT ((0))) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'AccountMap')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[AccountMap]([pb_account_id] [int] NOT NULL,	[pi_account_id] [bigint] NULL) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'AddressMap')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[AddressMap]([pi_address_cnt] [bigint] NULL, [pb_address_cnt] [int] NOT NULL) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'ContactMap')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[ContactMap]([pb_contact_cnt] [int] NOT NULL, [pi_contact_cnt] [bigint] NULL) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'ElementMap')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[ElementMap]([pb_element_id] [int] NULL, [pi_element_id] [bigint] NULL, [Is_Already_Added] [bit] NOT NULL DEFAULT ((0))) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'NodeMap')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[NodeMap]([pb_node_id] [int] NOT NULL, [pi_node_id] [bigint] NULL) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'PeriodMap')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[PeriodMap]([pi_Period_id] [int] NOT NULL, [pb_Period_id] [int] NOT NULL) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'MappingMap')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[MappingMap]([pi_Mapping_id] [int] NOT NULL, [pb_Mapping_id] [int] NOT NULL) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'UserNameTemp')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[UserNameTemp]([username] [varchar](255) NULL) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'UserMap')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[UserMap]([pb_user_id] [smallint] NULL, [pi_user_id] [bigint] NULL, [Is_Already_Added] [bit] NOT NULL DEFAULT ((0))) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'GIS_SchemeMap')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[GIS_SchemeMap]([pb_GIS_Scheme_id] [int] NOT NULL, [pi_GIS_Scheme_id] [bigint] NULL) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'Insurer_Group_Rate_Map_PB')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[Insurer_Group_Rate_Map_PB]([party_cnt] [int] NOT NULL, [Scheme] [int] NOT NULL, [risk_group_id] [int] NOT NULL, [effective_date] [datetime] NOT NULL) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'Insurer_Section_Rate_Map_PB')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[Insurer_Section_Rate_Map_PB]([Party_Cnt] [int] NOT NULL, [Scheme] [int] NOT NULL, [Risk_Code_Id] [int] NOT NULL, [Risk_code_COB_Rating_Section_id] [int] NOT NULL, [Effective_Date] [datetime] NOT NULL) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'Insurer_Scheme_Rate_Map_PB')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[Insurer_Scheme_Rate_Map_PB]([party_cnt] [int] NOT NULL, [scheme] [int] NOT NULL, [effective_date] [datetime] NOT NULL) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'Insurer_Rate_Map_PB')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[Insurer_Rate_Map_PB]([party_cnt] [int] NOT NULL, [Scheme] [int] NOT NULL, [risk_code_id] [int] NOT NULL, [effective_date] [datetime] NOT NULL) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'Party_Insurer_Risk_Map_PB')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[Party_Insurer_Risk_Map_PB]([party_cnt] [int] NOT NULL, [risk_code_id] [int] NOT NULL) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'Party_Insurer_Crt_Map_PB')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[Party_Insurer_Crt_Map_PB]([parent_insurer_cnt] [int] NOT NULL, [lead_insurer_cnt] [int] NOT NULL) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'Agent_Rate_Map_PB')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[Agent_Rate_Map_PB]([party_cnt] [int] NOT NULL, [risk_code_id] [int] NOT NULL, [effective_date] [datetime] NOT NULL, [rate_type_ind] [int] NOT NULL) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'Agent_Group_Rate_Map_PB')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[Agent_Group_Rate_Map_PB]([party_cnt] [int] NOT NULL, [risk_group_id] [int] NOT NULL, [effective_date] [datetime] NOT NULL, [rate_type_ind] [int] NOT NULL) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'Party_Agent_Risk_Group_Map')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[Party_Agent_Risk_Group_Map]([pb_party_agent_risk_group_id] [int] NOT NULL, [pi_party_agent_risk_group_id] [bigint] NULL) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'Fee_AmountMap')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[Fee_AmountMap]([pb_fee_amount_id] [int] NOT NULL, [pi_fee_amount_id] [bigint] NULL) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'Risk_By_Source_Map_PB')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[Risk_By_Source_Map_PB]([risk_group_id] [int] NOT NULL, [source_id] [int] NOT NULL) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'GIS_Scheme_For_General_PolicyMap')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[GIS_Scheme_For_General_PolicyMap]([pb_gis_insurer_id] [int] NOT NULL, [pb_risk_group_id] [int] NULL, [pi_GIS_Scheme_id] [bigint] NULL) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'BankAccount_DefaultMap')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[BankAccount_DefaultMap]([pb_bankaccount_default_id] [int] NOT NULL, [pi_bankaccount_default_id] [bigint] NULL) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'BankAccountMap')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[BankAccountMap]([pb_BankAccount_id] [int] NOT NULL, [pi_BankAccount_id] [bigint] NULL) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'Department_Address_Usage_Map_PB')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[Department_Address_Usage_Map_PB]([department_id] [int] NOT NULL, [address_cnt] [int] NOT NULL) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'RiskGroup_RiskTypeGroup_Map')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[RiskGroup_RiskTypeGroup_Map]([pb_Risk_Group_Id] [int] NULL, [pi_Risk_Type_Group_Id] [int] NULL) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'RiskGroup_Product_Map')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[RiskGroup_Product_Map]([pb_Risk_Group_Id] [int] NULL, [pi_product_Id] [int] NULL) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'RiskCode_RiskType_Map')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[RiskCode_RiskType_Map]([pb_Risk_Code_Id] [int] NOT NULL, [pi_Risk_Type_Id] [bigint] NULL) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'GIS_User_Def_HeaderMap')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[GIS_User_Def_HeaderMap]([pb_GIS_User_Def_Header_id] [int] NULL, [pi_GIS_User_Def_Header_id] [bigint] NULL) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'GIS_ScreenMap')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[GIS_ScreenMap]([pb_GIS_Screen_id] [int] NULL, [pi_GIS_Screen_id] [bigint] NULL) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'Gis_ObjectMap')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[Gis_ObjectMap]([pb_gis_object_id] [int] NULL, [pi_gis_object_id] [int] NULL) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'Gis_PropertyMap')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[Gis_PropertyMap]([pb_GIS_Property_id] [int] NULL, [pi_GIS_Property_id] [int] NULL) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'GIS_List_TypeMap')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[GIS_List_TypeMap]([pb_GIS_List_Type_id] [int] NULL, [pi_GIS_List_Type_id] [bigint] NULL) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'GIS_User_Def_DetailMap')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[GIS_User_Def_DetailMap]([pb_GIS_User_Def_Detail_id] [int] NULL, [pi_GIS_User_Def_Detail_id] [bigint] NULL) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'GIS_Data_ModelScript')
//begin
//    create table {AppConstants.TempDatabaseWithoutServer}[GIS_Data_ModelScript] (data_model_id int, table_name nvarchar(max), parent int, is_table_exist bit, create_table nvarchar(max), alter_table nvarchar(max))
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'EndorsementMap')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[EndorsementMap]([pb_endorsement_id] [int] NULL, [pi_endorsement_id] [bigint] NULL) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'MigrationTableLogData')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[MigrationTableLogData]([Id] [int] IDENTITY(1,1) NOT NULL, [TableName] [nvarchar](100) NOT NULL, [LogData] [nvarchar](max) NOT NULL, [Migration_Run_Id] [int] NOT NULL) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'Pmwrk_Task_Map')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[Pmwrk_Task_Map]([pb_pmwrk_task_id] [int] NOT NULL, pi_pmwrk_task_id [int] NOT NULL) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'Not_Migrated_Party')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[Not_Migrated_Party]([pb_party_cnt] [int] Not Null, [shortname] [nvarchar](50) Not Null) ON [PRIMARY]
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'SQC_Policy_Test_Data')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[SQC_Policy_Test_Data]([RowNumber] [bigint] NOT NULL, [InsuranceFileCnt] [int] NOT NULL, [InsuranceRef] [varchar](60) NOT NULL, [SchemeDesc] [varchar](255) NOT NULL, [RatingSectionCode] [char](10) NOT NULL, [TransactionType] [varchar](4) NOT NULL, [EffectiveDate] [datetime] NOT NULL, [PolicyInceptionDate] [datetime] NOT NULL, [RiskTypeGroupCode] [char](10) NOT NULL, [AnalysisCode] [char](50) NULL, [ThirdParty] [varchar](100) NULL, [RiskTypeCode] [varchar](50) NOT NULL, [PolicyAddonId] [int] NULL, [TaxConfigurationResult] [bit] NULL, [CommissionConfigurationResult] [bit] NULL, [FeeConfigurationResult] [bit] NULL, [ThirdPartyConfigurationResult] [bit] NULL, [AddOnConfigurationResult] [bit] NULL, [AddonTaxConfigurationResult] [bit] NULL) ON [PRIMARY]
//    CREATE NONCLUSTERED INDEX RowNumber on {AppConstants.TempDatabaseWithoutServer}[SQC_Policy_Test_Data] (RowNumber)
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'DocumentUploadRequest')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[DocumentUploadRequest]([RowNumber] [bigint] NOT NULL IDENTITY(1,1) PRIMARY KEY, [DocNum] [int] NULL, [DocName] [varchar](50) NULL, [CreateDate] [datetime] NULL, [Zipped] [char](1) NULL, [PageName] [varchar](254) NULL, [Extension] [varchar](254) NULL, [UniqueId] [varchar](20) NULL, [FolderLevel] [tinyint] NULL, [DocumentName] [nvarchar](max) NULL, [Path] [varchar](6) NULL, [ClientId] [nvarchar](20) NULL, [PolicyVersionId] [nvarchar](20) NULL, [ParentFolderNum] INT NULL, [FolderNum] INT NULL, [FolderName] [nvarchar](max) NULL, [PartyCnt] INT NULL, [InsuranceFileCnt] INT NULL, [InsuranceFolderCnt] INT NULL, [IsUploaded] [bit] NULL, [ErrorMessage] [nvarchar](max) NULL, [ErrorFileUploaded] [bit] NULL)
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'SchemeNotExists')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[SchemeNotExists](RiskTypeGroup varchar(10), TransactionType varchar(10))
//end

//if not EXISTS (SELECT 1 FROM {AppConfiguration.DataBase_Temp_Name}.sys.tables WHERE name = 'RatingSectionNotExists')
//begin
//    CREATE TABLE {AppConstants.TempDatabaseWithoutServer}[RatingSectionNotExists](Risk_Type_ID int, code char(10))
//end
//";

        /// <summary>
        /// 
        /// </summary>
//        public string Create_Migrated_Insurance_File { get; set; } = $@"----------/**/----------
//DECLARE @bExists TINYINT
//EXECUTE @bExists = DDLExistsTable 'Migrated_Insurance_File'
//IF @bExists = 0
//BEGIN
//       CREATE TABLE {AppConstants.TargetDatabaseWithoutServer}[Migrated_Insurance_File]
//       (
//              Insurance_File_Cnt INTEGER NOT NULL,
//              Is_From_Event BIT NOT NULL
//       )
//	   EXEC DDLAddPrimaryKey 'Migrated_Insurance_File', 'Insurance_File_Cnt' 
//END
//";
    }
}
