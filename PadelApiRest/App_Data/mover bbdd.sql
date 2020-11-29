ALTER DATABASE reservations_db   
    MODIFY FILE ( NAME = reservations_db,   
                  FILENAME = 'D:\Desarrollo\PadelApiRest\PadelApiRest\App_Data\reservations_db.mdf');  
GO
 
ALTER DATABASE reservations_db   
    MODIFY FILE ( NAME = reservations_db_log,   
                  FILENAME = 'D:\Desarrollo\PadelApiRest\PadelApiRest\App_Data\reservations_db_log.ldf');  
GO