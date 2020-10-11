using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;


namespace DataLayer
 { 
     public static class AppSetting
     { 
         public static string GetConnectionString()
         {

             string root = Directory.GetCurrentDirectory();
            return new ConfigurationBuilder()
                .AddJsonFile(Path.Combine("C:\\Users\\taj174\\source\\repos\\NCSApi\\NCSApi\\", "appsettings.json"), optional: false)
                .AddJsonFile(Path.Combine("C:\\Users\\taj174\\source\\repos\\NCSApi\\NCSApi\\", "appsettings.Development.json"), optional: false)
                .Build().GetSection("ConnectionString").GetValue<string>("DefaultConnectionString");
         }         
    }
}
