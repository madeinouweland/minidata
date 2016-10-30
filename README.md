# minidata
The simplest and fastest way to serialize UWP data to text files

## No database setup. 

Entities will be saved as xyx.minidata in `ApplicationData.Current.LocalFolder`

```
var db = new MiniData.Database();
var emp = new Employee { Name = "Loek" };
await db.Save(emp);
```

Employee.minidata:

```
1|Loek
```
