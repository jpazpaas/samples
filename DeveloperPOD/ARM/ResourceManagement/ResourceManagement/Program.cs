using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Resources.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ResourceManagement
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                // Authenticate
                var credentials = new InteractiveBrowserCredential();

                // List all resource groups and resources
                var subscriptionId = "AZURE_SUBSCRIPTION_ID";
                await RunSample(credentials, subscriptionId);
                Console.WriteLine("Please press any key.");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static async Task RunSample(TokenCredential credential, string subscriptionId)
        {
            var resourceClient = new ResourcesManagementClient(subscriptionId, credential);
            var resourceGroups = resourceClient.ResourceGroups;
            var resources = resourceClient.Resources;

            // Display the list of resourceGroup
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("Resource group: ");
            Console.WriteLine("------------------------------------");
            var resourceGroupList = resourceGroups.List().ToList();
            foreach (var rGroup in resourceGroupList)
            {
                Console.WriteLine(rGroup.Name);
            }

            // Display the list of resources
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("Resource: ");
            Console.WriteLine("-----------------------------------");
            var resourceList = resources.List().ToList();
            foreach (var r in resourceList)
            {
                Console.WriteLine(r.Type + " " + r.Name);
            }
        }
    }

}
