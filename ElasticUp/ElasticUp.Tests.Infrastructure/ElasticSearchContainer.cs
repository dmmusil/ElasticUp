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

        private ElasticSearchContainer(Version version)
        {
            StartElasticSearch(version);
        }

        public void WaitUntilElasticOperational()
        {
            SpinWait.SpinUntil(() => IsElasticSearchUpAndRunning().Result);
            Thread.Sleep(1000);
        }

        public void Dispose()
        {
            StopContainer();
        }

        private static void StopContainer()
        {
            Process.Start(Command("docker", "rm -f elastic"))?.WaitForExit();
        }

        private static ProcessStartInfo Command(string filename, string args) => new ProcessStartInfo(filename, args)
            { CreateNoWindow = true, UseShellExecute = false };
        private static void StartElasticSearch(Version version)
        {
            Process.Start(Command("docker", $"pull elasticsearch:{version.ToString(3)}"))?.WaitForExit();
            Process.Start(Command("docker", $"run -d -p 9200:9200 --name elastic elasticsearch:{version.ToString(3)}"))
                ?.WaitForExit();
        }

        private static async Task<bool> IsElasticSearchUpAndRunning()
        {
            const string endpointUrl = "http://localhost:9200/_cluster/health?wait_for_status=yellow&timeout=30s";

            using (var request = new HttpRequestMessage(HttpMethod.Get, endpointUrl))
            {
                try
                {
                    var response = await HttpClient.SendAsync(request);
                    return response.IsSuccessStatusCode;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static ElasticSearchContainer Start(Version version) => new ElasticSearchContainer(version);
    }
}
