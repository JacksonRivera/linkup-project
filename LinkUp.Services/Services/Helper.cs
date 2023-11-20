using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace LinkUp.Shopify.Services
{
    public class Helper
    {
        public static IRestResponse NewRequest(string server, string path, Method method, Dictionary<string, string> headers = null, string json = null)
        {
            var webClient = new RestClient($"{server}");
            var request = new RestRequest(path);

            request.Method = method;

            if (headers != null)
            {
                foreach (var item in headers)
                {
                    request.AddHeader(item.Key, item.Value);
                }
            }

            switch (method)
            {
                case Method.GET:
                    return webClient.Get(request);
                case Method.POST:
                    request.AddHeader("Content-Type", "application/json");
                    request.AddJsonBody(json);
                    return webClient.Post(request);
                case Method.PUT:
                    request.AddHeader("Content-Type", "application/json");
                    request.AddJsonBody(json);
                    return webClient.Put(request);
                case Method.DELETE:
                    return webClient.Delete(request);
                case Method.HEAD:
                    break;
                case Method.OPTIONS:
                    break;
                case Method.PATCH:
                    break;
                case Method.MERGE:
                    break;
                case Method.COPY:
                    break;
                default:
                    break;
            }


            return null;
        }

        public static IRestResponse NewRequestToken(string host, string path, string token, Method method, string json = null)
        {
            var webClient = new RestClient(host.Contains("http") ? host : $"https://{host}");
            var request = new RestRequest(path);

            request.Method = method;
            request.AddHeader("X-Shopify-Access-Token", token);

            switch (method)
            {
                case Method.GET:
                    return webClient.Get(request);
                case Method.POST:
                    request.AddHeader("Content-Type", "application/json");
                    request.AddJsonBody(json);
                    return webClient.Post(request);
                case Method.PUT:
                    request.AddHeader("Content-Type", "application/json");
                    request.AddJsonBody(json);
                    return webClient.Put(request);
                case Method.DELETE:
                    return webClient.Delete(request);
                case Method.HEAD:
                    break;
                case Method.OPTIONS:
                    break;
                case Method.PATCH:
                    break;
                case Method.MERGE:
                    break;
                case Method.COPY:
                    break;
                default:
                    break;
            }


            return null;
        }

        public static IRestResponse NewRequestBearerToken(string host, string path, string token, Method method, string json = null)
        {
            var webClient = new RestClient(host.Contains("http") ? host : $"https://{host}");
            var request = new RestRequest(path);

            request.Method = method;
            webClient.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(token, "Bearer");

            switch (method)
            {
                case Method.GET:
                    return webClient.Get(request);
                case Method.POST:
                    request.AddHeader("Content-Type", "application/json");
                    request.AddJsonBody(json);
                    return webClient.Post(request);
                case Method.PUT:
                    request.AddHeader("Content-Type", "application/json");
                    request.AddJsonBody(json);
                    return webClient.Put(request);
                case Method.DELETE:
                    return webClient.Delete(request);
                case Method.HEAD:
                    break;
                case Method.OPTIONS:
                    break;
                case Method.PATCH:
                    break;
                case Method.MERGE:
                    break;
                case Method.COPY:
                    break;
                default:
                    break;
            }


            return null;
        }

        public static IRestResponse NewRequestBasic(string host, string path, string user, string password, Method method, string json = null)
        {

            var webClient = new RestClient($"https://{user}:{password}@{host}");
            var request = new RestRequest(path);

            request.Method = method;
            webClient.Authenticator = new HttpBasicAuthenticator(user, password);

            switch (method)
            {
                case Method.GET:
                    return webClient.Get(request);
                case Method.POST:
                    request.AddHeader("Content-Type", "application/json");
                    request.AddJsonBody(json);
                    return webClient.Post(request);
                case Method.PUT:
                    request.AddHeader("Content-Type", "application/json");
                    request.AddJsonBody(json);
                    return webClient.Put(request);
                case Method.DELETE:
                    request.AddHeader("Content-Type", "application/json");
                    return webClient.Delete(request);
                case Method.HEAD:
                    break;
                case Method.OPTIONS:
                    break;
                case Method.PATCH:
                    break;
                case Method.MERGE:
                    break;
                case Method.COPY:
                    break;
                default:
                    break;
            }


            return null;
        }

        public static IRestResponse NewRequestBasic2(string host, string path, string user, string password, Method method, string json = null)
        {
            var webClient = new RestClient($"{host}");
            var request = new RestRequest(path);

            request.Method = method;
            webClient.Authenticator = new HttpBasicAuthenticator(user, password);

            switch (method)
            {
                case Method.GET:
                    return webClient.Get(request);
                case Method.POST:
                    request.AddHeader("Content-Type", "application/json");
                    request.AddJsonBody(json);
                    return webClient.Post(request);
                case Method.PUT:
                    request.AddHeader("Content-Type", "application/json");
                    request.AddJsonBody(json);
                    return webClient.Put(request);
                case Method.DELETE:
                    request.AddHeader("Content-Type", "application/json");
                    return webClient.Delete(request);
                case Method.HEAD:
                    break;
                case Method.OPTIONS:
                    break;
                case Method.PATCH:
                    break;
                case Method.MERGE:
                    break;
                case Method.COPY:
                    break;
                default:
                    break;
            }


            return null;
        }

        public static string RemoveEmojis(string input)
        {
            // Expresión regular para encontrar emojis
            var emojiPattern = @"\p{Cs}";

            // Remover emojis utilizando la expresión regular
            var result = Regex.Replace(input, emojiPattern, string.Empty);

            return result;
        }


    }
}
