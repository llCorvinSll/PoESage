using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace PoESage
{
    public class Program
    {
        static void Main(string[] args)
        {
            
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseUrls("http://0.0.0.0:8443")
                .Build();
            
            host.Run();
        }
    }
}