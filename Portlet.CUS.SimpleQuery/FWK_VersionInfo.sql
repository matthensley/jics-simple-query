begin tran 
use ICS_NET; 
Declare @ApplicationName varchar(255) = 'CUS Simple Query Portlet'; 
Declare @VersionNumber varchar(255) = '3.2.0'; 
Declare @VersionDate DateTime = '2012-05-8 9:49:00'; 
if exists (select * from [FWK_VersionInfo] where [ApplicationName] = @ApplicationName) 
begin 
   update [FWK_VersionInfo] set VersionNumber = @VersionNumber, VersionDate = @VersionDate 
   where [ApplicationName] = @ApplicationName 
end 
else 
begin 
   insert [FWK_VersionInfo]  (ApplicationName, VersionNumber, VersionDate) 
   values (@ApplicationName,@VersionNumber,@VersionDate ) 
end 
commit tran 
