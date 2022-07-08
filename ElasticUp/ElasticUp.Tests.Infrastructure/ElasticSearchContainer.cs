using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Version = System.Version;

namespace ElasticUp.Tests.Infrastructure
{
    public class ElasticSearchContainer : IDisposable
    {
        private static readonly HttpClient HttpClient = new HttpClient();
        
        private ElasticSearchContainer()
        {
            StartElasticSearch();
        }

        public async Task WaitUntilElasticOperational()
        {
            while (!await IsElasticSearchUpAndRunning())
            {
                Thread.Sleep(1000);
            }
        }

        public void Dispose()
        {
            StopContainer();
        }

        private static void StopContainer()
        {
            Process.Start(Command("docker-compose", "rm -fs"))?.WaitForExit();
        }

        private static ProcessStartInfo Command(string filename, string args) => new ProcessStartInfo(filename, args)
            { CreateNoWindow = true, UseShellExecute = false };

        private static void StartElasticSearch()
        {
            var pull = Process.Start(Command("docker-compose", "up -d"));
            if (pull == null)
            {
                Console.WriteLine("Failed to locate Docker");
            }
            else
            {
                pull.WaitForExit();
                if (pull.ExitCode != 0)
                {
                    Console.WriteLine("Failed to pull image.");
                    throw new Exception();
                }
            }

            
        }

        private static async Task<bool> IsElasticSearchUpAndRunning()
        {
            const string endpointUrl = "http://127.0.0.1:9200/_cluster/health?wait_for_status=yellow&timeout=30s";
            using (var request = new HttpRequestMessage(HttpMethod.Get, endpointUrl))
            {
                try
                {
                    var response = await HttpClient.SendAsync(request);
                    Console.WriteLine(response.StatusCode);                    
                    return response.IsSuccessStatusCode;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return false;
                }
            }
        }

        public static ElasticSearchContainer Start() => new ElasticSearchContainer();
    }
}