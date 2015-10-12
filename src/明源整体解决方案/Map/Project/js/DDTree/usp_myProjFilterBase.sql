<<<<<<< HEAD
Ôªø/*ÁâπÂà´ËØ¥ÊòéÔºöÊèê‰æõ‰∏§ÁßçËøîÂõûÊñπÂºè„ÄÇ  
ËøîÂõûÊï∞ÊçÆÈõÜÔºåÁî®‰∫éÂú®.net‰ª£Á†ÅËÆøÈóÆÁªìÊûúÈõÜÔºåÈúÄË¶ÅÂÖ∂ÂÆÉÂ≠òÂÇ®ËøáÁ®ãÂåÖË£ÖÔºå‰æãÂ¶Çusp_myProjFilterBaseDT  
ËøîÂõûÊ∏∏Ê†áÔºåÁî®‰∫éÂú®Â≠òÂÇ®ËøáÁ®ãÁõ¥Êé•ËÆøÈóÆÁªìÊûúÈõÜÔºå‰æãÂ¶Çusp_myContractFilter  
ÂéüÂõ†ÔºöÂ≠òÂÇ®ËøáÁ®ã‰∏çÂÖÅËÆ∏inert into execÂµåÂ•ó‰ΩøÁî®Ôºå.net‰∏çËÉΩË∞ÉÁî®Â∏¶Ê∏∏Ê†áËæìÂá∫ÁöÑÂ≠òÂÇ®ËøáÁ®ã  
*/  
  
--ÂäüËÉΩÔºöËé∑ÂèñÁî®Êà∑ÁöÑÈ°πÁõÆËøáÊª§ÁªìÊûúÈõÜ  
--ÂèÇÊï∞Ôºö  
--  @chvUserGUID  ÂΩìÂâçÁî®Êà∑ÁöÑGUID  
--  @chvApplication  Â∫îÁî®Á≥ªÁªü‰ª£Á†Å  
--  @chvBUGUID   ÂÖ¨Âè∏GUIDÔºåÂ¶ÇÊûú‰∏∫Á©∫Âàô‰∏çÂä†ÂÖ¨Âè∏ËøáÊª§  
--  @blnisUserFilter ÊòØÂê¶Âè†Âä†Áî®Êà∑Ëá™Â∑±ÂÆö‰πâÁöÑÈ°πÁõÆËøáÊª§  
--  @chvResultType  ËøîÂõûÁ±ªÂûã„ÄÇ‚Äútable‚ÄùÔºàËøîÂõûÊï∞ÊçÆÈõÜÔºâ„ÄÅ‚Äúcursor‚ÄùÔºàËøîÂõûÊ∏∏Ê†áÔºâ  
--  @curResult   ËøîÂõûÁöÑÊ∏∏Ê†á  
=======

/*Ãÿ±Àµ√˜£∫Ã·π©¡Ω÷÷∑µªÿ∑Ω Ω°£  
∑µªÿ ˝æ›ºØ£¨”√”⁄‘⁄.net¥˙¬Î∑√Œ Ω·π˚ºØ£¨–Ë“™∆‰À¸¥Ê¥¢π˝≥Ã∞¸◊∞£¨¿˝»Áusp_myProjFilterBaseDT  
∑µªÿ”Œ±Í£¨”√”⁄‘⁄¥Ê¥¢π˝≥Ã÷±Ω”∑√Œ Ω·π˚ºØ£¨¿˝»Áusp_myContractFilter  
‘≠“Ú£∫¥Ê¥¢π˝≥Ã≤ª‘ –Ìinert into exec«∂Ã◊ π”√£¨.net≤ªƒ‹µ˜”√¥¯”Œ±Í ‰≥ˆµƒ¥Ê¥¢π˝≥Ã  
*/  
  
--π¶ƒ‹£∫ªÒ»°”√ªßµƒœÓƒøπ˝¬ÀΩ·π˚ºØ  
--≤Œ ˝£∫  
--  @chvUserGUID  µ±«∞”√ªßµƒGUID  
--  @chvApplication  ”¶”√œµÕ≥¥˙¬Î  
--  @chvBUGUID   π´ÀæGUID£¨»Áπ˚Œ™ø’‘Ú≤ªº”π´Àæπ˝¬À  
--  @blnisUserFilter  «∑Òµ˛º””√ªß◊‘º∫∂®“ÂµƒœÓƒøπ˝¬À  
--  @chvResultType  ∑µªÿ¿‡–Õ°£°∞table°±£®∑µªÿ ˝æ›ºØ£©°¢°∞cursor°±£®∑µªÿ”Œ±Í£©  
--  @curResult   ∑µªÿµƒ”Œ±Í  
>>>>>>> origin/ddtree
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
  
<<<<<<< HEAD
 --1„ÄÅËé∑ÂèñÂéüÂßãÁöÑÈ°πÁõÆÊùÉÈôêÈõÜÂêà  
 --#dtAllObject ÊâÄÊúâÊï∞ÊçÆÂØπË±°  
 --#dtUO   ÂΩìÂâçÁî®Êà∑Â∑≤ÊúâÁöÑÊï∞ÊçÆÂØπË±°ÊùÉÈôê  
=======
 --1°¢ªÒ»°‘≠ ºµƒœÓƒø»®œﬁºØ∫œ  
 --#dtAllObject À˘”– ˝æ›∂‘œÛ  
 --#dtUO   µ±«∞”√ªß“—”–µƒ ˝æ›∂‘œÛ»®œﬁ  
>>>>>>> origin/ddtree
   
    DECLARE @chvSQL VARCHAR(8000) ,  
        @chvFilt VARCHAR(MAX) ,  
        @bIsAdmin BIT              
  
   
<<<<<<< HEAD
    --Âä†ËΩΩÊéàÊùÉÂØπË±°ÂÆö‰πâ  
=======
    --º”‘ÿ ⁄»®∂‘œÛ∂®“Â  
>>>>>>> origin/ddtree
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
<<<<<<< HEAD
   --SET @chvErrorInfo = 'id‰∏∫‚Äúproject‚ÄùÁöÑÊï∞ÊçÆÂØπË±°Ê≤°ÊúâÂÆö‰πâÔºÅ'  
            GOTO RETURN_NULL  
        END  
  
    --Ëé∑ÂèñÊï∞ÊçÆÊ∫êÔºåÁõÆÂâçÂè™ÊîØÊåÅ SQL  
=======
   --SET @chvErrorInfo = 'idŒ™°∞project°±µƒ ˝æ›∂‘œÛ√ª”–∂®“Â£°'  
            GOTO RETURN_NULL  
        END  
  
    --ªÒ»° ˝æ›‘¥£¨ƒø«∞÷ª÷ß≥÷ SQL  
>>>>>>> origin/ddtree
    IF @chvDataType = 'SQL'   
        BEGIN  
            SET @chvDataSource = @chvDataSource  
        END  
    ELSE   
        BEGIN  
<<<<<<< HEAD
   --SET @chvErrorInfo = 'id‰∏∫‚Äúproject‚ÄùÁöÑÊï∞ÊçÆÂØπË±°ÂÆö‰πâÈîôËØØÔºÅ'  
            GOTO RETURN_NULL  
        END  
    
 --ÂàõÂª∫#dtAllObjectÔºàÊâÄÊúâÊï∞ÊçÆÂØπË±°Ôºâ‰∏¥Êó∂Ë°®  
=======
   --SET @chvErrorInfo = 'idŒ™°∞project°±µƒ ˝æ›∂‘œÛ∂®“Â¥ÌŒÛ£°'  
            GOTO RETURN_NULL  
        END  
    
 --¥¥Ω®#dtAllObject£®À˘”– ˝æ›∂‘œÛ£©¡Ÿ ±±Ì  
>>>>>>> origin/ddtree
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
   
<<<<<<< HEAD
 --Â≠òÂÇ®ËøáÁ®ã‰∏≠Êó†Ê≥ïÂÆûÊó∂‰øÆÊîπË°®ÁªìÊûÑ   
=======
 --¥Ê¥¢π˝≥Ã÷–Œﬁ∑® µ ±–ﬁ∏ƒ±ÌΩ·ππ   
>>>>>>> origin/ddtree
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
<<<<<<< HEAD
    WHERE   UserGUID = @chvUserGUID --ÊòØÂê¶ÁÆ°ÁêÜÂëòÔºà1ÔºöÊòØ„ÄÅ0ÔºöÂê¶Ôºâ   
  
  
 --Ê†πÊçÆÁî®Êà∑ÁöÑÊùÉÈôêËøõË°åËøáÊª§  
    IF @bIsAdmin = 1   
        BEGIN  
   --ÁÆ°ÁêÜÂëòÔºåÊã•ÊúâÊâÄÊúâÁöÑÊï∞ÊçÆÊùÉÈôê  
=======
    WHERE   UserGUID = @chvUserGUID -- «∑Òπ‹¿Ì‘±£®1£∫ «°¢0£∫∑Ò£©   
  
  
 --∏˘æ›”√ªßµƒ»®œﬁΩ¯––π˝¬À  
    IF @bIsAdmin = 1   
        BEGIN  
   --π‹¿Ì‘±£¨”µ”–À˘”–µƒ ˝æ›»®œﬁ  
>>>>>>> origin/ddtree
            UPDATE  #dtAllObject  
            SET     _isallowopr = 1   
        END  
    ELSE   
        BEGIN  
<<<<<<< HEAD
   --ÊôÆÈÄöÁî®Êà∑ÔºåÂè™ËÉΩÊéàÂ∑≤ÊúâÁöÑÊï∞ÊçÆÊùÉÈôê     
=======
   --∆’Õ®”√ªß£¨÷ªƒ‹ ⁄“—”–µƒ ˝æ›»®œﬁ     
>>>>>>> origin/ddtree
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
     
<<<<<<< HEAD
            IF @@ROWCOUNT > 0    --myUserObjectÂèØËÉΩÂ≠òÂú®ÂûÉÂúæÊï∞ÊçÆ  
                BEGIN  
     --Êú¨Á∫ßÂèä‰∏ãÁ∫ß  
=======
            IF @@ROWCOUNT > 0    --myUserObjectø…ƒ‹¥Ê‘⁄¿¨ª¯ ˝æ›  
                BEGIN  
     --±æº∂º∞œ¬º∂  
>>>>>>> origin/ddtree
                    UPDATE  a  
                    SET     a._isallowopr = 1  
                    FROM    #dtAllObject a  
                            INNER JOIN #dtUO b ON a._hierarchycode + '.' LIKE b._hierarchycode  
                                                  + '.%'  
                END     
      
            DROP TABLE #dtUO     
        END  
  

<<<<<<< HEAD
   --Ê†πÊçÆDataRightsDTTypeÁ±ªÂûãËøõË°åËøáÊª§  
=======
   --∏˘æ›DataRightsDTType¿‡–ÕΩ¯––π˝¬À  
>>>>>>> origin/ddtree
    SET @chvFilt = ''  
	
	Create table #tmpBUGUID
	(
		BUGUID UNIQUEIDENTIFIER 
	)

    IF @bIsAdmin = 1  
<<<<<<< HEAD
        --Â¶ÇÊûúÊòØÁÆ°ÁêÜÂëòÔºåÂè™Âá∫Áî®Êà∑ÊâÄÂ±ûÂÖ¨Âè∏ÁöÑ Deep ÊùÉÈôêÔºå‰∏çËÄÉËôëDataRightsDTType„ÄÇ  
=======
        --»Áπ˚ «π‹¿Ì‘±£¨÷ª≥ˆ”√ªßÀ˘ Ùπ´Àæµƒ Deep »®œﬁ£¨≤ªøº¬«DataRightsDTType°£  
>>>>>>> origin/ddtree
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
<<<<<<< HEAD
				--Èöè‰æø‰∏Ä‰∏™GUID,ÈÅøÂÖçÊä•Èîô  
                SET @guid = '''A9FB9CB3-5A7C-41F9-8518-31494E56243C'''   
			
			--Â¶ÇÊûúÊòØÁÆ°ÁêÜÂëòÔºåÂè™Âá∫Áî®Êà∑ÊâÄÂ±ûÂÖ¨Âè∏ÁöÑ Deep ÊùÉÈôêÔºå‰∏çËÄÉËôëDataRightsDTType„ÄÇ  
=======
				--ÀÊ±„“ª∏ˆGUID,±‹√‚±®¥Ì  
                SET @guid = '''A9FB9CB3-5A7C-41F9-8518-31494E56243C'''   
			
			--»Áπ˚ «π‹¿Ì‘±£¨÷ª≥ˆ”√ªßÀ˘ Ùπ´Àæµƒ Deep »®œﬁ£¨≤ªøº¬«DataRightsDTType°£  
>>>>>>> origin/ddtree
            SET @chvFilt = '_buguid not in (' + @guid + ')'  
            */
            SET @chvFilt ='_buguid not in(SELECT BUGUID FROM #tmpBUGUID)'
        END  
    ELSE   
        BEGIN   
            SET @chvFilt = '_isallowopr = ''0'''  
        END  
 
<<<<<<< HEAD
 --Â¶ÇÊûúÈùûÂÆ¢ÊúçÁ≥ªÁªü(0102)ÔºåÈúÄË¶ÅËøáÊª§ÊéâÂÖ±‰∫´È°πÁõÆÊï∞ÊçÆ      
=======
 --»Áπ˚∑«øÕ∑˛œµÕ≥(0102)£¨–Ë“™π˝¬ÀµÙπ≤œÌœÓƒø ˝æ›      
>>>>>>> origin/ddtree
    IF @chvApplication <> ''  
        AND @chvApplication <> '0102'   
        SET @chvFilt = @chvFilt + ' OR _isshare=1'   
   
<<<<<<< HEAD
 --Ê†πÊçÆÊï∞ÊçÆÁ±ªÂûãËøáÊª§  
   SET @chvFilt = @chvFilt + ' OR _sourcetype <> ''È°πÁõÆ'''  
  
	--Âà†Èô§‰ª•‰∏äÊù°‰ª∂ÊâÄËøáÊª§ÁöÑËÆ∞ÂΩï 
=======
 --∏˘æ› ˝æ›¿‡–Õπ˝¬À  
   SET @chvFilt = @chvFilt + ' OR _sourcetype <> ''œÓƒø'''  
  
	--…æ≥˝“‘…œÃıº˛À˘π˝¬Àµƒº«¬º 
>>>>>>> origin/ddtree
	SET @chvSQL = 'DELETE #dtAllObject WHERE ' + @chvFilt  
    
    EXEC (@chvSQL)  
   
<<<<<<< HEAD
 --2„ÄÅÂØπÂéüÂßãÁöÑÈ°πÁõÆÊùÉÈôêÈõÜÂêàËøõË°åËøáÊª§  
 --Âä†ÂÖ¨Âè∏ËøáÊª§  
=======
 --2°¢∂‘‘≠ ºµƒœÓƒø»®œﬁºØ∫œΩ¯––π˝¬À  
 --º”π´Àæπ˝¬À  
>>>>>>> origin/ddtree
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
    
    
<<<<<<< HEAD
 --Â¶ÇÊûúÂêØÁî®ÁöÑ‚ÄúÈ°πÁõÆÂ≠êÁ≥ªÁªüËøáÊª§‚ÄùÔºåÂàôÈúÄË¶ÅËøáÊª§ÂΩìÂâçÁ≥ªÁªü‰∏çÂèØÁî®ÁöÑÈ°πÁõÆ  
=======
 --»Áπ˚∆Ù”√µƒ°∞œÓƒø◊”œµÕ≥π˝¬À°±£¨‘Ú–Ë“™π˝¬Àµ±«∞œµÕ≥≤ªø…”√µƒœÓƒø  
>>>>>>> origin/ddtree
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
   
   
<<<<<<< HEAD
 --Âè†Âä†Áî®Êà∑Ëá™Â∑±ÂÆö‰πâÁöÑÈ°πÁõÆËøáÊª§  
=======
 --µ˛º””√ªß◊‘º∫∂®“ÂµƒœÓƒøπ˝¬À  
>>>>>>> origin/ddtree
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
  
  
<<<<<<< HEAD
 --ÊàêÂäü:ËæìÂá∫ÁªìÊûúÈõÜ  
=======
 --≥…π¶: ‰≥ˆΩ·π˚ºØ  
>>>>>>> origin/ddtree
    DROP TABLE #Temp  
    EXEC sp_xml_removedocument @hDoc  
  
    SET NOCOUNT OFF  
    IF @chvResultType = 'table'   
        BEGIN  
<<<<<<< HEAD
   --ËøîÂõûÁªìÊûúÈõÜ  
=======
   --∑µªÿΩ·π˚ºØ  
>>>>>>> origin/ddtree
            SELECT  ProjGUID  
            FROM    #dtDataRights  
        END  
    ELSE   
        BEGIN  
<<<<<<< HEAD
   --ËæìÂá∫Ê∏∏Ê†á  
=======
   -- ‰≥ˆ”Œ±Í  
>>>>>>> origin/ddtree
            SET @curResult = CURSOR FORWARD_ONLY STATIC FOR   
    SELECT ProjGUID FROM #dtDataRights  
            OPEN @curResult  
        END  
    RETURN 1  
   
<<<<<<< HEAD
 --Â§±Ë¥•:ËøîÂõûÁ©∫Êï∞ÊçÆÈõÜ  
=======
 -- ß∞‹:∑µªÿø’ ˝æ›ºØ  
>>>>>>> origin/ddtree
    RETURN_NULL:  
    IF NOT OBJECT_ID('tempdb..#b') IS NULL   
        DROP TABLE #Temp  
  
    EXEC sp_xml_removedocument @hDoc  
  
    SET NOCOUNT OFF  
    IF @chvResultType = 'table'   
        BEGIN  
<<<<<<< HEAD
   --ËøîÂõûÁªìÊûúÈõÜ  
=======
   --∑µªÿΩ·π˚ºØ  
>>>>>>> origin/ddtree
            SELECT  NULL AS ProjGUID  
            WHERE   1 = 2  
        END  
    ELSE   
        BEGIN  
<<<<<<< HEAD
   --ËæìÂá∫Ê∏∏Ê†á  
=======
   -- ‰≥ˆ”Œ±Í  
>>>>>>> origin/ddtree
            SET @curResult = CURSOR FORWARD_ONLY STATIC FOR   
    SELECT NULL AS ProjGUID WHERE 1=2  
            OPEN @curResult  
        END  
    RETURN 0


GO
