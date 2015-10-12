
/*特别说明：提供两种返回方式。  
返回数据集，用于在.net代码访问结果集，需要其它存储过程包装，例如usp_myProjFilterBaseDT  
返回游标，用于在存储过程直接访问结果集，例如usp_myContractFilter  
原因：存储过程不允许inert into exec嵌套使用，.net不能调用带游标输出的存储过程  
*/  
  
--功能：获取用户的项目过滤结果集  
--参数：  
--  @chvUserGUID  当前用户的GUID  
--  @chvApplication  应用系统代码  
--  @chvBUGUID   公司GUID，如果为空则不加公司过滤  
--  @blnisUserFilter 是否叠加用户自己定义的项目过滤  
--  @chvResultType  返回类型。“table”（返回数据集）、“cursor”（返回游标）  
--  @curResult   返回的游标  
ALTER   PROCEDURE [dbo].[usp_myProjFilterBase]  
    (  
      @chvUserGUID NVARCHAR(40) ,  
      @chvApplication NVARCHAR(16) = '0101' ,  
      @chvBUGUID NVARCHAR(40) = '' ,  
      @blnisUserFilter BIT = 1 ,  
      @chvResultType VARCHAR(10) = 'table' ,  
      @curResult CURSOR VARYING OUTPUT  
   
    )  
AS 
IF @chvBUGUID<>''
BEGIN  
SELECT @chvBUGUID='' FROM dbo.myBusinessUnit WHERE BUGUID=@chvBUGUID
AND ParentGUID IS NULL
END

    SET NOCOUNT ON  

    IF @chvApplication IS NULL   
        SET @chvApplication = '0101'  
    IF @chvBUGUID IS NULL   
        SET @chvBUGUID = ''  
    IF @blnisUserFilter IS NULL   
        SET @blnisUserFilter = 1  
  
    SET @chvUserGUID = REPLACE(@chvUserGUID, '''', '''''')              
    SET @chvApplication = REPLACE(@chvApplication, '''', '''''')              
 --SET @chvErrorInfo = ''  
  
 --1、获取原始的项目权限集合  
 --#dtAllObject 所有数据对象  
 --#dtUO   当前用户已有的数据对象权限  
   
    DECLARE @chvSQL VARCHAR(8000) ,  
        @chvFilt VARCHAR(MAX) ,  
        @bIsAdmin BIT              
  
   
    --加载授权对象定义  
    DECLARE @chvXML NVARCHAR(4000) ,  
        @hDoc INT ,  
        @chvXPath VARCHAR(200) ,  
        @chvDataType VARCHAR(50) ,  
        @chvDataSource VARCHAR(8000)   
   
    EXEC usp_myGetDataRightsXML @chvXML OUTPUT  
    EXEC sp_xml_preparedocument @hDoc OUTPUT, @chvXML  
   
    SET @chvXPath = '//object[@id=''project'']'   
  
    SELECT  @chvDataType = UPPER(datatype) ,  
            @chvDataSource = datasource  
    FROM    OPENXML (@hDoc,@chvXPath ,1)  
    WITH ( datatype VARCHAR(50) 'datatype',  
      datasource VARCHAR(8000) 'datasource' )  
    
    IF @@ROWCOUNT = 0   
        BEGIN  
   --SET @chvErrorInfo = 'id为“project”的数据对象没有定义！'  
            GOTO RETURN_NULL  
        END  
  
    --获取数据源，目前只支持 SQL  
    IF @chvDataType = 'SQL'   
        BEGIN  
            SET @chvDataSource = @chvDataSource  
        END  
    ELSE   
        BEGIN  
   --SET @chvErrorInfo = 'id为“project”的数据对象定义错误！'  
            GOTO RETURN_NULL  
        END  
    
 --创建#dtAllObject（所有数据对象）临时表  
    CREATE TABLE #dtAllObject  
        (  
          _guid UNIQUEIDENTIFIER ,  
          _name VARCHAR(1000) ,  
          _hierarchycode VARCHAR(1000) ,  
          _sourcetype VARCHAR(100) ,  
          _level INT ,  
          _buguid VARCHAR(40) ,  
          _isshare INT ,  

          _isallowopr VARCHAR(1) DEFAULT '0'  
        )  
   
 --存储过程中无法实时修改表结构   
    INSERT  INTO #dtAllObject  
            ( _guid ,  
              _name ,  
              _hierarchycode ,  
              _sourcetype ,  
              _level ,  
              _buguid ,  
              _isshare  
            )  
            EXEC ( @chvDataSource  
                )  
   
    SELECT  @bIsAdmin = IsAdmin  
    FROM    myUser  
    WHERE   UserGUID = @chvUserGUID --是否管理员（1：是、0：否）   
  
  
 --根据用户的权限进行过滤  
    IF @bIsAdmin = 1   
        BEGIN  
   --管理员，拥有所有的数据权限  
            UPDATE  #dtAllObject  
            SET     _isallowopr = 1   
        END  
    ELSE   
        BEGIN  
   --普通用户，只能授已有的数据权限     
            SELECT  *  
            INTO    #dtUO  
            FROM    (   
    --SELECT a.UserGUID,a.StationGUID,b.ObjectType,b.ObjectGUID,b.TableName,b.BUGUID,c._hierarchycode  
                      SELECT DISTINCT  
                                c._hierarchycode  
                      FROM      myStationUser a  
                                INNER JOIN myStationObject b ON a.stationguid = b.stationguid  
                                INNER JOIN #dtAllObject c ON c._guid = b.ObjectGUID  
                                                             AND c._buguid = b.BUGUID  
                      WHERE     ObjectType = 'project'  
                                AND UserGUID = @chvUserGUID  
                    ) a  
     
            IF @@ROWCOUNT > 0    --myUserObject可能存在垃圾数据  
                BEGIN  
     --本级及下级  
                    UPDATE  a  
                    SET     a._isallowopr = 1  
                    FROM    #dtAllObject a  
                            INNER JOIN #dtUO b ON a._hierarchycode + '.' LIKE b._hierarchycode  
                                                  + '.%'  
                END     
      
            DROP TABLE #dtUO     
        END  
  

   --根据DataRightsDTType类型进行过滤  
    SET @chvFilt = ''  
	
	Create table #tmpBUGUID
	(
		BUGUID UNIQUEIDENTIFIER 
	)

    IF @bIsAdmin = 1  
        --如果是管理员，只出用户所属公司的 Deep 权限，不考虑DataRightsDTType。  
        BEGIN  
            Insert Into #tmpBUGUID(BUGUID)
            SELECT    BUGUID  
                      FROM      myBusinessUnit  
                      WHERE     CHARINDEX(( SELECT  HierarchyCode + '.'  
                                            FROM    myBusinessUnit  
                                            WHERE   BUGUID = ( SELECT  
                                                              BUGUID  
                                                              FROM  
                                                              myUser  
                                                              WHERE  
                                                              UserGUID = ''  
                                                              + @chvUserGUID  
                                                              + ''  
                                                             )  
                                          ), HierarchyCode + '.') = 1  
            /*  
            DECLARE @guid VARCHAR(MAX)  
            SET @guid = ''''  
            SELECT  @guid = @guid + CONVERT(VARCHAR(40), a.BUGUID) + ''','''  
            FROM    ( SELECT    BUGUID  
                      FROM      myBusinessUnit  
                      WHERE     CHARINDEX(( SELECT  HierarchyCode + '.'  
                                            FROM    myBusinessUnit  
                                            WHERE   BUGUID = ( SELECT  
                                                              BUGUID  
                                                              FROM  
                                                              myUser  
                                                              WHERE  
                                                              UserGUID = ''  
                                                              + @chvUserGUID  
                                                              + ''  
                                                             )  
                                          ), HierarchyCode + '.') = 1  
                    ) AS a           
                      
            IF LEN(@guid) >= 2   
                SELECT  @guid = SUBSTRING(@guid, 1, LEN(@guid) - 2)        
            ELSE  
				--随便一个GUID,避免报错  
                SET @guid = '''A9FB9CB3-5A7C-41F9-8518-31494E56243C'''   
			
			--如果是管理员，只出用户所属公司的 Deep 权限，不考虑DataRightsDTType。  
            SET @chvFilt = '_buguid not in (' + @guid + ')'  
            */
            SET @chvFilt ='_buguid not in(SELECT BUGUID FROM #tmpBUGUID)'
        END  
    ELSE   
        BEGIN   
            SET @chvFilt = '_isallowopr = ''0'''  
        END  
 
 --如果非客服系统(0102)，需要过滤掉共享项目数据      
    IF @chvApplication <> ''  
        AND @chvApplication <> '0102'   
        SET @chvFilt = @chvFilt + ' OR _isshare=1'   
   
 --根据数据类型过滤  
   SET @chvFilt = @chvFilt + ' OR _sourcetype <> ''项目'''  
  
	--删除以上条件所过滤的记录 
	SET @chvSQL = 'DELETE #dtAllObject WHERE ' + @chvFilt  
    
    EXEC (@chvSQL)  
   
 --2、对原始的项目权限集合进行过滤  
 --加公司过滤  
    IF @chvBUGUID <> ''   
        DELETE  #dtAllObject  
        WHERE _buguid NOT IN (SELECT BUGUID FROM dbo.myBusinessUnit WHERE ParentGUID =@chvBUGUID OR BUGUID =@chvBUGUID)
  
    SELECT  _guid AS ProjGUID ,  
            _buguid AS BUGUID  
    INTO    #dtDataRights  
    FROM    #dtAllObject   
    
    
    IF @@ROWCOUNT = 0   
        GOTO RETURN_NULL  
  
    SELECT  *  
    INTO    #Temp  
    FROM    #dtDataRights  
    WHERE   1 = 2   
    
    
 --如果启用的“项目子系统过滤”，则需要过滤当前系统不可用的项目  
    IF EXISTS ( SELECT  1  
                FROM    myApplication  
                WHERE   ISNULL(IsApplySys, 0) = 1  
                        AND Application = @chvApplication )   
        BEGIN  
            IF @chvApplication = '0102'   
                BEGIN  
                    IF @chvBUGUID = ''   
                        INSERT  INTO #Temp  
                                SELECT  a.*  
                                FROM    #dtDataRights a  
                                        INNER JOIN ( SELECT ProjGUID ,  
                                                            BUGUID  
                                                     FROM   ek_Project  
                                                     WHERE  CHARINDEX(','  
                                                              + @chvApplication  
                                                              + ',',  
                                                              ',' + ApplySys  
                                                              + ',') > 0  
                       ) b ON a.ProjGUID = b.ProjGUID  
                                                          AND a.BUGUID = b.BUGUID  
                    ELSE
                        INSERT  INTO #Temp  
                                SELECT  a.*  
                                FROM    #dtDataRights a  
                                        INNER JOIN ( SELECT ProjGUID ,  
                                                            BUGUID  
                                                     FROM   ek_Project  
                                                     WHERE  CHARINDEX(','  
                                                              + @chvApplication  
                                                              + ',',  
                                                              ',' + ApplySys  
                                                              + ',') > 0  
                                                            AND BUGUID IN (SELECT BUGUID FROM dbo.e_myBusinessUnit WHERE ParentGUID =@chvBUGUID OR BUGUID = @chvBUGUID)
                                                   ) b ON a.ProjGUID = b.ProjGUID  
                                                          AND a.BUGUID = b.BUGUID  
  
  
                    DELETE  FROM #dtDataRights  
                    INSERT  INTO #dtDataRights  
                            SELECT  *  
                            FROM    #Temp  
                    TRUNCATE TABLE #Temp  
                    
                END  
      
            ELSE   
                BEGIN  
                    IF @chvBUGUID = ''   
                        INSERT  INTO #Temp  
                                SELECT  a.*  
                                FROM    #dtDataRights a  
                                        INNER JOIN ( SELECT ProjGUID ,  
                                                            BUGUID  
                                                     FROM   ep_Project  
                                                     WHERE  CHARINDEX(','  
                                                              + @chvApplication  
                                                              + ',',  
                                                              ',' + ApplySys  
                                                              + ',') > 0  
                                                   ) b ON a.ProjGUID = b.ProjGUID  
                                                          AND a.BUGUID = b.BUGUID  
                    ELSE   
                    
                        INSERT  INTO #Temp  
                                SELECT  a.*  
                                FROM    #dtDataRights a  
                                        INNER JOIN ( SELECT ProjGUID ,  
                                                            BUGUID  
                                                     FROM   ep_Project  
                                                     WHERE  CHARINDEX(','  
                                                              + @chvApplication  
                                                              + ',',  
                                                              ',' + ApplySys  
                                                              + ',') > 0  
                                                            AND BUGUID IN (SELECT BUGUID FROM dbo.e_myBusinessUnit WHERE ParentGUID =@chvBUGUID OR BUGUID = @chvBUGUID)
                                                   ) b ON a.ProjGUID = b.ProjGUID  
                                                          AND a.BUGUID = b.BUGUID  
  
                    DELETE  FROM #dtDataRights  
                    INSERT  INTO #dtDataRights  
                            SELECT  *  
                            FROM    #Temp  
                    TRUNCATE TABLE #Temp  
                END  
        END  
   
   
 --叠加用户自己定义的项目过滤  
    IF @blnisUserFilter = 1  
        AND EXISTS ( SELECT 1  
                     FROM   myUser  
                     WHERE  ISNULL(UserProject, '') <> '[ALL]'  
                            AND UserGUID = @chvUserGUID )   
        BEGIN  
            IF @chvBUGUID = ''   
                INSERT  INTO #Temp  
        SELECT  a.*  
                        FROM    #dtDataRights a  
                                INNER JOIN ( SELECT ProjGUID ,  
                                                    BUGUID  
                                             FROM   myUserProject  
                                             WHERE  UserGUID = @chvUserGUID  
                                           ) b ON a.ProjGUID = b.ProjGUID  
                                                  AND a.BUGUID = b.BUGUID  
            ELSE   
                INSERT  INTO #Temp  
                        SELECT  a.*  
                        FROM    #dtDataRights a  
                                INNER JOIN ( SELECT ProjGUID ,  
                                                    BUGUID  
                                             FROM   myUserProject  
                                             WHERE  UserGUID = @chvUserGUID  
                                                   AND BUGUID IN (SELECT BUGUID FROM dbo.e_myBusinessUnit WHERE ParentGUID =@chvBUGUID OR BUGUID = @chvBUGUID)
                                           ) b ON a.ProjGUID = b.ProjGUID  
                                                  AND a.BUGUID = b.BUGUID  
     
            DELETE  FROM #dtDataRights  
            INSERT  INTO #dtDataRights  
                    SELECT  *  
                    FROM    #Temp  
            TRUNCATE TABLE #Temp  
        END  
  
  
 --成功:输出结果集  
    DROP TABLE #Temp  
    EXEC sp_xml_removedocument @hDoc  
  
    SET NOCOUNT OFF  
    IF @chvResultType = 'table'   
        BEGIN  
   --返回结果集  
            SELECT  ProjGUID  
            FROM    #dtDataRights  
        END  
    ELSE   
        BEGIN  
   --输出游标  
            SET @curResult = CURSOR FORWARD_ONLY STATIC FOR   
    SELECT ProjGUID FROM #dtDataRights  
            OPEN @curResult  
        END  
    RETURN 1  
   
 --失败:返回空数据集  
    RETURN_NULL:  
    IF NOT OBJECT_ID('tempdb..#b') IS NULL   
        DROP TABLE #Temp  
  
    EXEC sp_xml_removedocument @hDoc  
  
    SET NOCOUNT OFF  
    IF @chvResultType = 'table'   
        BEGIN  
   --返回结果集  
            SELECT  NULL AS ProjGUID  
            WHERE   1 = 2  
        END  
    ELSE   
        BEGIN  
   --输出游标  
            SET @curResult = CURSOR FORWARD_ONLY STATIC FOR   
    SELECT NULL AS ProjGUID WHERE 1=2  
            OPEN @curResult  
        END  
    RETURN 0


GO
