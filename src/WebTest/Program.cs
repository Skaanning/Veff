using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace WebTest
{
    public static class Program
    {
        public static void Main(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls("https://localhost:5555/");
                })
                .Build()
                .Run();
    }
}