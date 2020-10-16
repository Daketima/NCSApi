using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;


namespace DataLayer
 { 
     public static class AppSetting
     { 
         public static string GetConnectionString()
         {
        
            return new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(@"C:\DevApp\NCSApi", "appsettings.json"), optional: false)
                .AddJsonFile(Path.Combine(@"C:\DevApp\NCSApi", "appsettings.Development.json"), optional: false)
                .Build().GetSection("ConnectionString").GetValue<string>("DefaultConnectionString");

                // return new ConfigurationBuilder()
                // .AddJsonFile(Path.Combine(@"C:\DevApp\NCSApi", "appsettings.json"), optional: false)
                // .AddJsonFile(Path.Combine(@"C:\DevApp\NCSApi", "appsettings.Development.json"), optional: false)
                // .Build().GetSection("ConnectionString").GetValue<string>("DefaultConnectionString");
         }         
    }
}
