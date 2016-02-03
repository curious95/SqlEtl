# SqlEtl

Easy to move on-premises SQL Server data to to another database, Azure SQL Database or Amazon SQL RDS

Syntax:
SqlEtl.exe /source:"Server=<SERVER>;Initial Catalog=<SOURCEDB>;integrated security=true" /destination:"Server=<SERVER>;Initial Catalog=<TARGETDB>;integrated security=true" /batchsize:50  /createobjects:true

Help:
 

Sql ETL Utility Version 1.0.0.0

Usage: SqlEtl.exe <parameters>
   Example: /source:"source" /destination:"destinatin" /batchsize:50 /resumeonerror:true
   
 Parameters:
 
  /source
  
    Connection string for data source
    
  /destination
  
    Connection string for destination
 
  /CreateObjects
  
    destination is empty database, so create all tables, indexes, storedproc and views on destination
    
  /resumeonerror
  
    resume if any error occurs due to missing primary keys
 
  /batchsize
  
    number of rows per transfer
 
 /skip
  
    comma separated ist of tables needs to be skipped
 
  /retrycount
  
    try number of times to retry on any failure
 
  /retryinterval
  
    seconds to be waited before retrying any failure
 
  /customkeydefinitions
  
    moving data needs clustered index but if none is presented you can fake the keys here for example       tablename~key1,key2 in a separate file and provide full path to the file here
 
  /?
 
  /h
 
    Displays this help text

Things to note:  At this time this utility will support only dbo schema.



