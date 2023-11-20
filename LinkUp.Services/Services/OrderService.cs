using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace LinkUp.Shopify.Services
{
    public static class OrderService
    {
        public static IRestResponse GetOrder(string shopId, string token, string orderId)
        {
            var path = $"admin/api/2023-01/orders/{orderId}.json";
            var response = Helper.NewRequestToken(shopId, path, token, Method.GET);

            return response;
        }
        public static IRestResponse CreateOrder(string shopId, string token, dynamic order)
        {
            var path = $"admin/api/2023-01/orders.json";
            var json = new
            {
                order = order
            };

            var response = Helper.NewRequestToken(shopId, path, token, Method.POST, Helper.RemoveEmojis(JsonConvert.SerializeObject(json)));

            return response;
        }
        public static IRestResponse CreateDraftOrder(string shopId, string token, dynamic order)
        {
            var path = $"admin/api/2023-01/draft_orders.json";
            var json = new
            {
                draft_order = order
            };
            var response = Helper.NewRequestToken(shopId, path, token, Method.POST, Helper.RemoveEmojis(JsonConvert.SerializeObject(json)));

            return response;
        }

        public static IRestResponse CompleteDraftOrder(string shopId, string token, string orderId)
        {
            var path = $"admin/api/2023-01/draft_orders/{orderId}/complete.json";
            var json = new
            {
                draft_order = new
                {
                    id = orderId
                }
            };
            var response = Helper.NewRequestToken(shopId, path, token, Method.PUT, Helper.RemoveEmojis(JsonConvert.SerializeObject(json)));

            return response;
        }
    }
}
