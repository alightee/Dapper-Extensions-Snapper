# Introduction

Snapper is a small library that creates a wrapper over Dapper and provides CRUD functionality over database tables in order to help developers to create more complex SQL queries.
The target of this library is to simplify and facilitate queries for more complex relational data models.

This library is working for SQL and MySQL Connections.
# Features
* CRUD functionalities over tables from database 
* Automatic mapping of table names for CRUD operations
* Creation of efficient advanced queries in a lightweight library
* Composite Primary Key support
* Query result caching
* DI Registration

# Installation
Install-Package Dapper.Extensions.Snapper
# How to use
##### Database Connection Manager:
Each connection in order to be used must extend **DatabaseConnectionManager\<DBConnectionType>** where DBConnectionType can be SQL or MySQL connection. Method NewConnection has to be override and will contain the creation of the new connection

Example:

*  For SQL Connection:

```csharp  
public SqlConnectionManager: DatabaseConnectionManager<SqlConnection>
{
    protected override SqlConnection NewConnection()
    {
        var connection = new SqlConnection(ConnectionString);
        connection.Open();
        return connection;
    }
    public SqlConnectionManager(IHttpContextAccessor contextAccessor) : base(contextAccessor, connValue)
    {
    	SqlMapperExtensions.GetDatabaseType = conn => "SqlConnection";
    }
}
```
   
* For MySQLConnection:

```csharp 
public MySqlConnectionManager: DatabaseConnectionManager<MySqlConnection>
{
    protected override MySqlConnection NewConnection()
    {
        var connection = new MySqlConnection(ConnectionString);
        connection.Open();
        return connection;
    }
    public MySqlConnectionManager(IHttpContextAccessor contextAccessor) : base(contextAccessor, connValue)
    {
    	SqlMapperExtensions.GetDatabaseType = conn => "MySqlConnection";
    }
}
 ```


##### Dependency Injection Registration: 
IHttpContextAccessor has to be registered in Startup.cs:
```csharp 
services.AddHttpContextAccessor();
```

Snapper services that will be used has to be registered in Startup.cs:
* For SQL Connection:

```csharp 
services.RegisterSqlConnectionManager<SqlConnectionManager>();
services.RegisterSnapperServicesForSql();
```
* For MySQL Connection:

```csharp 
services.RegisterMySqlConnectionManager<MySqlConnectionManager>();
services.RegisterSnapperServicesForMySql();
```
##### Models:

Each model class must inherit **SnapperDatabaseTableModel** and must have a primary key (simple/composite):
```csharp
[Table("tableName")]
public class TableName : SnapperDatabaseTableModel
{
    public long TableColumn1 { get; set; }
    public string TableColumn2 { get; set; }
    public string TableColumn3 { get; set; }
    [Write(false)]
    public ForeignKeyModel ForeignKeyModel { get; set; }
}
  
```  
##### Example:
    
 * Retrive table items:
 ```csharp 
 private ISmartRepository<Item> ItemRepository { get; }

public ItemService(ISmartRepository<Item> itemRepository)
{
	  ItemRepository = itemRepository;
}

public List<Item> GetAllItems()
{
      var items = ItemRepository.GetWhere(x => x.Field == 1, new QueryExecutionOptions { ResultLimit = 100 });
      return items;
}
  ```
 * Update table items:
  ```csharp 
  private ISmartRepository<Item> ItemRepository { get; }

public ItemService(ISmartRepository<Item> itemRepository)
{
	   ItemRepository = itemRepository;
}

public List<Item> UpdateItem()
{
      var items = ItemRepository.GetWhere(x => x.Field == 1, new QueryExecutionOptions { ResultLimit = 100 });
      items[0].Field = items[0].Field + "Updated";
      ItemRepository.AddUpdateDeleteWhere(x => x.Field == 1, items);
      return items;
}
```
Same method is used for Add, Delete and Update table items with where clauses. 