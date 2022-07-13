using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Search.ObjectStore;
using Newtonsoft.Json;
using CryptoSchema;

namespace Company.Function
{
  public static class HttpTrigger1
  {
    private static string EnvironmentEndpoint = "http://objectstoremulti.prod.ch.binginternal.com:83/sds/ObjectStoreQuery/V1";
    private const string NamespaceName = "Presto";
    private const string TableName = "CryptoTokenSSD";

    static async Task Query(IClient<string, string> client)
    {
      var data = new List<KeyValuePair<string, string>>();
      data.Add(new KeyValuePair<string, string>("internal_token_id", "818a32ef5a8d88cf019da358ee7c5c91"));

      var keys = data.Select(e => e.Key);

      var values = await client.Read(keys).SendAsync();
      Console.WriteLine($"Read API succeeded");
    }

    [FunctionName("HttpTrigger1")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
        ILogger log)
    {

      var clientBuilder = Client.Builder<string, string>(
          environment: EnvironmentEndpoint,
          osNamespace: NamespaceName,
          osTable: TableName,
          timeout: TimeSpan.FromMilliseconds(2000),
          maxRetries: 0);

      using (var client = clientBuilder.Create())
      {
        log.LogInformation($"Client version is {client.ClientVersion}");

        await Query(client);
      }

      log.LogInformation("C# HTTP trigger function processed a request.");

      string name = req.Query["name"];

      string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
      dynamic data = JsonConvert.DeserializeObject(requestBody);
      name = name ?? data?.name;

      string responseMessage = string.IsNullOrEmpty(name)
          ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
          : $"Hello, {name}. This HTTP triggered function executed successfully.";

      return new OkObjectResult(responseMessage);
    }
  }
}
