using System;
using System.Collections.Generic;
using LinkUp.Core.Storage;
using Newtonsoft.Json.Linq;
using RestSharp;
using LinkUp.Shopify.Entities;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Newtonsoft.Json;
using System.Net;
using LinkUp.Core.AWS.DynamoDB;

namespace LinkUp.Shopify.Services
{
    public class ShopService
    {
        public List<Shop> GetShops(IServiceProvider serviceProvider, string appId)
        {
            var repo = new RepositoryAWS<Shop>(serviceProvider);
            var shops = repo.Get(appId).Result;

            return shops;
        }

        public Shop GetShop(IServiceProvider serviceProvider, string appId, string shopId)
        {
            var repo = new RepositoryAWS<Shop>(serviceProvider);
            var shop = repo.Get(appId, shopId);

            return shop;
        }

        public Shop CreateShop(IServiceProvider serviceProvider, Shop shop)
        {
            var repo = new RepositoryAWS<Shop>(serviceProvider);
            var oldShop = GetShop(serviceProvider, shop.PartitionKey, shop.RowKey);

            if (oldShop == null)
            {
                shop.State = Guid.NewGuid().ToString();
                shop.CreationDate = DateTime.Now;
                repo.Save(shop);

                return shop;
            }
            else
            {
                return oldShop;
            }

        }

        public Shop Install(IServiceProvider serviceProvider, IConfiguration configuration, string appId, string shopId, string code)
        {
            var repo = new RepositoryAWS<Shop>(serviceProvider);
            var shop = GetShop(serviceProvider, appId, shopId);

            if (shop != null)
            {
                if (shop.Installed == false)
                {
                    shop.Code = code;
                    shop.Token = GetToken(serviceProvider, shop);

                    if (shop.Token != null)
                    {
                        shop.InstalationDate = DateTime.Now;
                        shop.Installed = true;

                        var connectionString = configuration.GetSection("StorageSettings:StorageConnectionString").Value;

                        try
                        {
                            var name = shop.RowKey.Split(".")[0];
                        }
                        catch (Exception)
                        {
                            shop.SubscriptionErros += " -- Error al crear las colas";
                        }

                        repo.Save(shop);

                        //CreateWebHook(shop.RowKey, shop.Token, "orders/create");
                        //CreateWebHook(shop.RowKey, shop.Token, "orders/updated");
                        //CreateCarrierService(shop.RowKey, shop.Token);
                    }
                }

                return shop;
            }
            else
            {
                return null;
            }
        }

        private dynamic CreateCarrierService(string origin, string token)
        {
            var client = new RestClient($"https://{origin}");
            var request = new RestRequest($"/admin/api/2023-01/carrier_services.json");

            request.AddHeader("X-Shopify-Access-Token", token);
            request.AddHeader("Content-Type", "application/json");

            try
            {
                var metafield = new List<string>();

                metafield.Add("Order notification");

                var newCarrierService = new
                {
                    carrier_service = new
                    {
                        name = "ENVÍO ESTÁNDAR",
                        callback_url = "https://shippify.LinkUp.com/api/CarrierService/GetRates",
                        service_discovery = true
                    }
                };

                request.AddJsonBody(JsonConvert.SerializeObject(newCarrierService));

                var response = client.Post(request);
                var content = response.Content;
                var jObject = JObject.Parse(content);

                return response.StatusCode == HttpStatusCode.OK ? jObject : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string GetToken(IServiceProvider serviceProvider, Shop shop)
        {
            var repo = new RepositoryAWS<App>(serviceProvider);
            var app = repo.Get("Shopify", shop.PartitionKey);
            if (app != null)
            {
                var client = new RestClient($"https://{shop.RowKey}");
                var request = new RestRequest("admin/oauth/access_token");
                var oauth = new
                {
                    client_id = app.RowKey,
                    client_secret = app.Key,
                    code = shop.Code
                };

                request.AddObject(oauth);

                var response = client.Post(request);
                var content = response.Content; // raw content as string
                var jObject = JObject.Parse(content);

                return (string)jObject["access_token"];
            }
            else
            {
                return null;
            }
        }

        public dynamic GetCurrentUser(IServiceProvider serviceProvider, string appId, string shopId)
        {
            var repo = new RepositoryAWS<Shop>(serviceProvider);
            var shop = repo.Get(appId, shopId);

            if (shop != null)
            {
                var client = new RestClient($"https://{shopId}");
                var request = new RestRequest("/admin/api/2023-01/users/current.json");
                request.AddHeader("X-Shopify-Access-Token", shop.Token);

                var response = client.Get(request);
                var content = response.Content;
                //var jObject = JObject.Parse(content);

                return content;
            }
            else
            {
                return null;
            }
        }

        public dynamic CreateWebHook(string origin, string token, string topic = "orders/paid")
        {
            DeleteWebHooks(origin, token);

            var client = new RestClient($"https://{origin}");
            var request = new RestRequest($"/admin/api/2023-01/webhooks.json");

            request.AddHeader("X-Shopify-Access-Token", token);
            request.AddHeader("Content-Type", "application/json");

            try
            {
                var metafield = new List<string>();

                metafield.Add("Order notification");

                var newWebhook = new
                {
                    webhook = new
                    {
                        topic = topic,
                        address = "https://shippify.LinkUp.com/api/Order/NewOrder",
                        format = "json",
                        metafield_namespaces = metafield,
                        private_metafield_namespaces = metafield
                    }
                };

                request.AddJsonBody(JsonConvert.SerializeObject(newWebhook));

                var response = client.Post(request);
                var content = response.Content;
                var jObject = JObject.Parse(content);

                return response.StatusCode == HttpStatusCode.OK ? jObject : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void DeleteWebHooks(string origin, string token)
        {
            try
            {
                var webhooks = GetWebHooks(origin, token);

                if (webhooks != null)
                {
                    foreach (var item in webhooks["webhooks"])
                    {
                        var client = new RestClient($"https://{origin}");
                        var request = new RestRequest($"/admin/api/2023-01/webhooks/{(string)item["id"]}.json");

                        request.AddHeader("X-Shopify-Access-Token", token);

                        try
                        {
                            var response = client.Delete(request);
                            var content = response.Content;
                            var jObject = JObject.Parse(content);

                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }


        private JObject GetWebHooks(string origin, string token)
        {
            var client = new RestClient($"https://{origin}");
            var request = new RestRequest($"/admin/api/2023-01/webhooks.json");

            request.AddHeader("X-Shopify-Access-Token", token);

            try
            {
                var response = client.Get(request);
                var content = response.Content;
                var jObject = JObject.Parse(content);

                return jObject;
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
