using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using RestSharp;
using LinkUp.Shopify.Entities;
using GraphQL.Client;
using GraphQL.Common.Request;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LinkUp.Shopify.Services
{
    public class CustomerService
    {
        public JObject GetCustomer(string origin, string token, string customerId)
        {
            var customer = new RestClient($"https://{origin}");
            var request = new RestRequest($"/admin/api/2023-01/customers/{customerId}.json");

            request.AddHeader("X-Shopify-Access-Token", token);

            try
            {
                var response = customer.Get(request);
                var content = response.Content;
                var jObject = JObject.Parse(content);

                return response.StatusCode == HttpStatusCode.OK ? jObject : null;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public IRestResponse AddTag(string shopId, string token, string customerId, string newTag, List<string> removeTags = null)
        {
            var customer = GetCustomer(shopId, token, customerId);

            if (customer != null)
            {
                var tags = (string)customer["customer"]["tags"];

                tags = tags == null ? "" : tags;

                if (tags.Contains(newTag) == false)
                {
                    tags = string.IsNullOrEmpty(tags) ? newTag : $"{tags}, {newTag}";

                    if (removeTags != null)
                    {
                        var list = Regex.Replace(tags, @"\s", "").Split(",");
                        var tagList =  new List<string>();

                        foreach (var item in list)
                        {
                            if (removeTags.Find(x => x == item) == null) {
                                tagList.Add(item);
                            }
                        }

                        tags = string.Join(",", tagList);
                    }

                    var path = $"admin/api/2023-01/customers/{customerId}.json";
                    dynamic customerJson = new ExpandoObject();

                    customerJson.id = customerId;
                    customerJson.tags = tags;

                    var json = new
                    {
                        customer = customerJson
                    };
                    var response = Helper.NewRequestToken(shopId, path, token, Method.PUT, JsonConvert.SerializeObject(json));

                    return response;
                }
            }

            return null;
        }
        ///data-pending, data-complite
        public dynamic AddTags(string shopId, string token, string customerId, List<string> newTags)
        {
            var customer = GetCustomer(shopId, token, customerId);

            if (customer != null)
            {
                var tags = (string)customer["customer"]["tags"];

                tags = tags == null ? "" : tags;

                foreach (var newTag in newTags)
                {
                    if (tags.Contains(newTag) == false)
                    {
                        tags = string.IsNullOrEmpty(tags) ? newTag : $"{tags}, {newTag}";
                    }
                }

                var path = $"admin/api/2023-01/customers/{customerId}.json";
                dynamic customerJson = new ExpandoObject();

                customerJson.id = customerId;
                customerJson.tags = tags;

                var json = new
                {
                    customer = customerJson
                };

                var response = Helper.NewRequestToken(shopId, path, token, Method.PUT, JsonConvert.SerializeObject(json));

                return response;
            }

            return null;
        }
        ///data-pending, data-complite
        public dynamic AddTagsIf(string shopId, string token, string customerId, Dictionary<string, List<string>> newTags)
        {
            var customer = GetCustomer(shopId, token, customerId);

            if (customer != null)
            {
                var tags = (string)customer["customer"]["tags"];

                tags = tags == null ? "" : tags;

                foreach (var newTag in newTags)
                {
                    if (tags.Contains(newTag.Key) == false)
                    {
                        var isValid = true;

                        foreach (var item in newTag.Value)
                        {
                            if (tags.Contains(item) != false)
                            {
                                isValid = false;
                            }
                        }
                        if (isValid)
                        {
                            tags = string.IsNullOrEmpty(tags) ? newTag.Key : $"{tags}, {newTag.Key}";
                        }
                    }
                }

                var path = $"admin/api/2023-01/customers/{customerId}.json";
                dynamic customerJson = new ExpandoObject();

                customerJson.id = customerId;
                customerJson.tags = tags;

                var json = new
                {
                    customer = customerJson
                };

                var response = Helper.NewRequestToken(shopId, path, token, Method.PUT, JsonConvert.SerializeObject(json));

                return response;
            }

            return null;
        }

        public string GetCustomerId(string store, string token, string email)
        {
            var customerResponse = GetCustomerByEmail(store, token, email);
            var customerId = string.Empty;

            try
            {
                if (customerResponse.StatusCode == HttpStatusCode.OK)
                {
                    var customer = JObject.Parse(customerResponse.Content);

                    customerId = (string)customer["customers"][0]["id"];
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


            return customerId;
        }

        public IRestResponse GetCustomerByEmail(string origin, string token, string email)
        {
            var path = $"admin/api/2023-01/customers/search.json?query={email}";

            try
            {
                if (string.IsNullOrEmpty(email.Trim()))
                {
                    return null;
                }

                var response = Helper.NewRequestToken(origin, path, token, Method.GET);

                return response;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public IRestResponse GetCustomerById(string origin, string token, string id)
        {
            var path = $"admin/api/2023-01/customers/{id}.json";

            try
            {
                var response = Helper.NewRequestToken(origin, path, token, Method.GET);

                return response;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private string GetNextPag(Parameter link)
        {
            var query = string.Empty;

            if (link != null)
            {

                var pages = link.Value.ToString().Split(",");

                try
                {
                    switch (pages.Count())
                    {
                        case 1:
                            if (pages[0].Contains("next"))
                            {
                                var url = new Uri(pages[0].Split(";")[0].Replace("<", "").Replace(">", ""));

                                query = url.Query;
                            }
                            break;
                        case 2:
                            if (pages[1].Contains("next"))
                            {
                                var url = new Uri(pages[1].Split(";")[0].Replace("<", "").Replace(">", ""));

                                query = url.Query;
                            }
                            break;
                    }
                }
                catch (Exception)
                {
                }
            }
            else
            {
                query = "?limit=250";
            }

            return query;
        }

        //public IRestResponse UpdateCustomerMetafields(string shopId, string token, string customerId, List<Metafield> metafields)
        //{
        //    var path = $"admin/api/2023-01/customers/{customerId}.json";
        //    var metafieldsJson = FormatMetafields(shopId, token, customerId, metafields);

        //    if (metafieldsJson.Count > 0)
        //    {
        //        var customer = new
        //        {
        //            customer = new
        //            {
        //                id = customerId,
        //                metafields = metafieldsJson
        //            }
        //        };
        //        var json = JsonConvert.SerializeObject(customer);

        //        var response = Helper.NewRequestToken(shopId, path, token, Method.PUT, json);

        //        return response;
        //    }

        //    return new RestResponse()
        //    {
        //        StatusCode = HttpStatusCode.OK,
        //        Content = ""
        //    };
        //}

        public IRestResponse AddCustomerTags(string shopId, string token, string customerId, List<string> tagsList)
        {
            try
            {
                var path = $"admin/api/2023-01/customers/{customerId}.json";
                var result = GetCustomerById(shopId, token, customerId);

                if (result.StatusCode == HttpStatusCode.OK)
                {
                    var customer = JObject.Parse(result.Content);

                    var tags = SetTags((string)customer["customer"]["tags"], tagsList);

                    if (!string.IsNullOrEmpty(tags))
                    {
                        var newCustomer = new
                        {
                            customer = new
                            {
                                id = customerId,
                                tags = tags
                            }
                        };
                        var json = JsonConvert.SerializeObject(newCustomer);
                        var response = UpdateCustomer(shopId, token, customerId, json);

                        return response;
                    }

                    return new RestResponse()
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = ""
                    };
                }

            }
            catch (Exception)
            {
            }

            return new RestResponse()
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = ""
            };
        }

        public string GetCustomerTags(string shopId, string token, string customerId)
        {
            var tags = string.Empty;

            try
            {
                var path = $"admin/api/2023-01/customers/{customerId}.json";
                var result = GetCustomerById(shopId, token, customerId);

                if (result.StatusCode == HttpStatusCode.OK)
                {
                    var customer = JObject.Parse(result.Content);

                    tags = (string)customer["customer"]["tags"];
                }
            }
            catch (Exception)
            {
            }

            return tags;
        }

        public string SetTags(string tags, List<string> tagsList)
        {
            var oldTags = tags.Replace(", ", ",").Split(",").ToList();

            foreach (var tag in tagsList)
            {
                if (oldTags.Find(x => x == tag) == null)
                {
                    oldTags.Add(tag);
                }
            }

            return String.Join(", ", oldTags.ToArray());

        }

        //public List<dynamic> FormatMetafields(string shopId, string token, string customerId, List<Metafield> metafields)
        //{
        //    var result = new List<dynamic>();
        //    var oldMetafields = GetCustomerMetafields(shopId, token, customerId);

        //    foreach (var item in metafields)
        //    {
        //        var metafieldJson = "{\"key\": \"" + item.Key + "\", \"value\": \"" + item.Value + "\", \"type\": \"" + item.Type + "\", \"namespace\": \"" + item.Namespace + "\"}";
        //        var oldMetafieldJson = oldMetafields.Find(x => x.Key == item.Key && x.Namespace == item.Namespace);

        //        if (oldMetafieldJson != null)
        //        {
        //            if (oldMetafieldJson.Value != item.Value)
        //            {
        //                metafieldJson = "{\"id\": \"" + oldMetafieldJson.Id + "\", \"value\": \"" + item.Value + "\"}";
        //            }
        //            else
        //            {
        //                metafieldJson = null;
        //            }
        //        }

        //        if (!string.IsNullOrEmpty(metafieldJson))
        //        {
        //            dynamic metafield = JsonConvert.DeserializeObject(metafieldJson);
        //            result.Add(metafield);
        //        }
        //    }

        //    return result;
        //}

        //public List<dynamic> FormatMetafields(string shopId, string token, string entity, string id, List<Metafield> metafields)
        //{
        //    var result = new List<dynamic>();
        //    var oldMetafields = GetGenericMetafields(shopId, token, entity, id);

        //    foreach (var item in metafields)
        //    {
        //        var metafieldJson = "{\"key\": \"" + item.Key + "\", \"value\": \"" + item.Value + "\", \"type\": \"" + item.Type + "\", \"namespace\": \"" + item.Namespace + "\"}";
        //        var oldMetafieldJson = oldMetafields.Find(x => x.Key == item.Key && x.Namespace == item.Namespace);

        //        if (oldMetafieldJson != null)
        //        {
        //            if (oldMetafieldJson.Value != item.Value)
        //            {
        //                metafieldJson = "{\"id\": \"" + oldMetafieldJson.Id + "\", \"value\": \"" + item.Value + "\"}";
        //            }
        //            else
        //            {
        //                metafieldJson = null;
        //            }
        //        }

        //        if (!string.IsNullOrEmpty(metafieldJson))
        //        {
        //            dynamic metafield = JsonConvert.DeserializeObject(metafieldJson);
        //            result.Add(metafield);
        //        }
        //    }

        //    return result;
        //}

        //public List<Metafield> GetCustomerMetafields(string store, string token, string customerId)
        //{
        //    var result = new List<Metafield>();

        //    try
        //    {
        //        var path = $"admin/api/2023-01/customers/{customerId}/metafields.json";
        //        var response = Helper.NewRequestToken(store, path, token, Method.GET);

        //        if (response.StatusCode == HttpStatusCode.OK)
        //        {
        //            var json = JObject.Parse(response.Content);

        //            foreach (var item in json["metafields"])
        //            {
        //                var metafield = new Metafield()
        //                {
        //                    Id = (string)item["id"],
        //                    Key = (string)item["key"],
        //                    Namespace = (string)item["namespace"],
        //                    Type = (string)item["type"],
        //                    Value = (string)item["value"]
        //                };

        //                result.Add(metafield);
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //    }

        //    return result;
        //}

        //public List<Metafield> GetGenericMetafields(string store, string token, string entity, string id)
        //{
        //    var result = new List<Metafield>();

        //    try
        //    {
        //        var path = $"admin/api/2023-01/{entity}/{id}/metafields.json";
        //        var response = Helper.NewRequestToken(store, path, token, Method.GET);

        //        if (response.StatusCode == HttpStatusCode.OK)
        //        {
        //            var json = JObject.Parse(response.Content);

        //            foreach (var item in json["metafields"])
        //            {
        //                var metafield = new Metafield()
        //                {
        //                    Id = (string)item["id"],
        //                    Key = (string)item["key"],
        //                    Namespace = (string)item["namespace"],
        //                    Type = (string)item["type"],
        //                    Value = (string)item["value"]
        //                };

        //                result.Add(metafield);
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //    }

        //    return result;
        //}

        public IRestResponse CreateCustomer(string shopId, string token, string customer)
        {
            var path = $"admin/api/2023-01/customers.json";
            var response = Helper.NewRequestToken(shopId, path, token, Method.POST, customer);

            return response;
        }

        public IRestResponse UpdateCustomer(string shopId, string token, string customerId, string customer)
        {
            var path = $"admin/api/2023-01/customers/{customerId}.json";
            var response = Helper.NewRequestToken(shopId, path, token, Method.PUT, customer);

            return response;
        }
    }
}
