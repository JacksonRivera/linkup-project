using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using RestSharp;
using LinkUp.Shopify.Entities;
using GraphQL.Common.Request;
using GraphQL.Client;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using GraphQL.Common.Response;
using LinkUp.Core.AWS.DynamoDB;

namespace LinkUp.Shopify.Services
{
    public class SubscriptionService
    {
        public void GetDataAsync(IServiceProvider serviceProvider, string appId, string shopId)
        {
            var repo = new RepositoryAWS<Shop>(serviceProvider);
            var shop = repo.Get(appId, shopId);

            if (shop != null)
            {

                CreateSubscription(shop);

            }
        }
        public void CreateSubscription(Shop shop)
        {
            var query = new GraphQLRequest
            {
                Query = @"
                        mutation {
                            appSubscriptionCreate(name: ""Basic"", lineItems: [{plan: {appRecurringPricingDetails: {price: {amount: 7.99, currencyCode: USD}}}}] , returnUrl: ""https://shopify-discountapp.azurewebsites.net/Index"", test: true, trialDays: 7) {
                                appSubscription {
                                  id
                                }
                                confirmationUrl
                                userErrors {
                                  field
                                  message
                                }
                            }                            
                        }"
            };
            var graphQLClient = new GraphQLClient($"https://{shop.RowKey}/admin/api/2023-01/graphql.json");

            graphQLClient.DefaultRequestHeaders.Add("X-Shopify-Access-Token", shop.Token);

            var graphQLResponse = graphQLClient.PostAsync(query);
            var content = graphQLResponse.Result.Data;
            var id = content.appSubscriptionCreate.appSubscription.id;
            var url = content.appSubscriptionCreate.confirmationUrl;
            var errors = content.appSubscriptionCreate.userErrors;

            shop.SubscriptionId = id != null ? id.Value : null;
            shop.SubscriptionUrl = url != null ? url.Value : null;
        }

        public void Test()
        {
            var query = new GraphQLRequest
            {
                Query = @"
                        query {
                              productVariants(first: 1, query: ""barcode:SKU002"") {
                                edges {
                                        node {
                                              id
                                             }
                                      }
                               }
                          }"
            };
            var graphQLClient = new GraphQLClient($"https://LinkUp-test.myshopify.com/admin/api/2023-01/graphql.json");

            graphQLClient.DefaultRequestHeaders.Add("X-Shopify-Access-Token", "a3500b028093a8f542dc45d5ef90d3e0");

            var graphQLResponse = graphQLClient.PostAsync(query);
            var content = graphQLResponse.Result.Data;
            var id = content.appSubscriptionCreate.appSubscription.id;
            var url = content.appSubscriptionCreate.confirmationUrl;
            var errors = content.appSubscriptionCreate.userErrors;
        }
        public SubscriptionStatus GetStatus(IServiceProvider serviceProvider, string appId, string shopId)
        {
            var repo = new RepositoryAWS<Shop>(serviceProvider);
            var shop = repo.Get(appId, shopId);

            if (shop != null)
            {
                var id = shop.SubscriptionId.Split("/")[4] ?? shop.SubscriptionId;
                var client = new RestClient($"https://{shopId}");
                var request = new RestRequest("/admin/api/2023-01/recurring_application_charges/" + id + ".json");

                request.AddHeader("X-Shopify-Access-Token", shop.Token);

                var response = client.Get(request);
                var content = response.Content;
                var jObject = JObject.Parse(content);
                var status = (string)jObject["recurring_application_charge"]["status"];

                return new SubscriptionStatus() { Status = status, SubscriptionId = id, Url = shop.SubscriptionUrl };
            }
            else
            {
                return null;
            }
        }
    }
}
