
IDBO dbo = new DBO("dbdemo2", "demo", "Florin123");
await dbo.InitializeConnection();
await Task.Delay(5000);
dbo.Dispose();
Console.ReadLine();