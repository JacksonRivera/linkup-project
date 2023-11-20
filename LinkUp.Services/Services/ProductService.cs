using System;
using System.Collections.Generic;
using LinkUp.Core.Storage;
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
using LinkUp.Core.AWS.DynamoDB;

namespace LinkUp.Shopify.Services
{
    public class ProductService
    {
        public dynamic GetVariantByBarcode(string store, string barcode, string token)
        {
            var filter = $"barcode:{barcode}";
            var query = new GraphQLRequest
            {
                Query = @"
                        query {
                              productVariants(first: 1, query: """ + filter + @""") {
                                edges {
                                        node {
                                              id
                                             }
                                      }
                               }
                          }"
            };
            var graphQLClient = new GraphQLClient($"https://{store}/admin/api/2023-01/graphql.json");
            graphQLClient.DefaultRequestHeaders.Add("X-Shopify-Access-Token", token);

            var graphQLResponse = graphQLClient.PostAsync(query);
            var content = graphQLResponse.Result.Data;
            var variants = content.productVariants.edges;

            try
            {
                string id = content.productVariants.edges[0] != null ? content.productVariants.edges[0].node.id : "";
                id = id.Split("/")[4];

                return new { Id = id, Variants = variants, Response = graphQLResponse.Result };
            }
            catch (Exception)
            {
                return new { Id = "null", Variants = variants, Response = graphQLResponse.Result };
            }
        }

        public string GetVariantIdByBarcode(string store, string barcode, string token)
        {
            var filter = $"barcode:{barcode}";
            var query = new GraphQLRequest
            {
                Query = @"
                        query {
                              productVariants(first: 1, query: """ + filter + @""") {
                                edges {
                                        node {
                                              id
                                             }
                                      }
                               }
                          }"
            };
            var graphQLClient = new GraphQLClient($"https://{store}/admin/api/2023-01/graphql.json");
            graphQLClient.DefaultRequestHeaders.Add("X-Shopify-Access-Token", token);

            var graphQLResponse = graphQLClient.PostAsync(query);
            var content = graphQLResponse.Result.Data;
            var variants = content.productVariants.edges;

            try
            {
                string id = content.productVariants.edges[0] != null ? content.productVariants.edges[0].node.id : "";
                //id = id.Split("/")[4];

                return id;
            }
            catch (Exception)
            {
                return "Error";
            }
        }

        public string GetinventoryItemId(string store, string sku, string token)
        {
            var inventoryItemId = GetinventoryItemIdBySKU(store, sku, token);

            return inventoryItemId;
        }
        /// <summary>
        /// obtiene productId, variantId, inventoryItemId
        /// </summary>
        /// <param name="store"></param>
        /// <param name="barcode"></param>
        /// <param name="sku"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public JObject GetIds(string store, string sku, string sku2, string token)
        {
            var idsSku = GetIdsBySKU(store, sku, token);

            if ((string)idsSku["Status"] == "Error")
            {
                idsSku = GetIdsByBarcode(store, sku2, token);
                idsSku["ItemId"] = "";
                idsSku["VariantId"] = "";

            }

            return idsSku;
        }

        public string GetinventoryItemIdByBarcode(string store, string barcode, string token)
        {
            var filter = $"barcode:{barcode}";
            var query = new GraphQLRequest
            {
                Query = @"
                        query {
                              productVariants(first: 1, query: """ + filter + @""") {
                                edges {
                                        node {
                                              id
                                                 inventoryItem {
                                                    id
                                                   }
                                             }
                                      }
                               }
                          }"
            };

            var graphQLClient = new GraphQLClient($"https://{store}/admin/api/2023-01/graphql.json");
            graphQLClient.DefaultRequestHeaders.Add("X-Shopify-Access-Token", token);

            var graphQLResponse = graphQLClient.PostAsync(query);
            var content = graphQLResponse.Result.Data;
            var variants = content.productVariants.edges;

            try
            {
                string id = content.productVariants.edges[0] != null ? content.productVariants.edges[0].node.inventoryItem.id : "";
                //id = id.Split("/")[4];

                return id;
            }
            catch (Exception)
            {
                return "Error";
            }
        }

        public string GetinventoryItemIdBySKU(string store, string sku, string token)
        {
            var filter = $"sku:{sku}";
            var query = new GraphQLRequest
            {
                Query = @"
                        query {
                              productVariants(first: 1, query: """ + filter + @""") {
                                edges {
                                        node {
                                              id
                                                 inventoryItem {
                                                    id
                                                   }
                                             }
                                      }
                               }
                          }"
            };

            var graphQLClient = new GraphQLClient($"https://{store}/admin/api/2023-01/graphql.json");
            graphQLClient.DefaultRequestHeaders.Add("X-Shopify-Access-Token", token);

            var graphQLResponse = graphQLClient.PostAsync(query);
            var content = graphQLResponse.Result.Data;
            var variants = content.productVariants.edges;

            try
            {
                string id = content.productVariants.edges[0] != null ? content.productVariants.edges[0].node.inventoryItem.id : "";
                //id = id.Split("/")[4];

                return id;
            }
            catch (Exception)
            {
                return "Error";
            }
        }

        public JObject GetIdsByBarcode(string store, string barcode, string token)
        {
            var filter = $"barcode:{barcode}";
            var query = new GraphQLRequest
            {
                Query = @"
                        query {
                              productVariants(first: 1, query: """ + filter + @""") {
                                edges {
                                        node {
                                              id
                                              inventoryItem {
                                                  id
                                              }
                                              product {
                                                  id
                                              }
                                        }
                                      }
                               }
                          }"
            };

            var graphQLClient = new GraphQLClient($"https://{store}/admin/api/2023-01/graphql.json");
            graphQLClient.DefaultRequestHeaders.Add("X-Shopify-Access-Token", token);

            var graphQLResponse = graphQLClient.PostAsync(query);
            var content = graphQLResponse.Result.Data;
            var variants = content.productVariants.edges;

            try
            {
                string itemId = content.productVariants.edges[0] != null ? content.productVariants.edges[0].node.inventoryItem.id : "";
                string variantId = content.productVariants.edges[0] != null ? content.productVariants.edges[0].node.id : "";
                string productId = content.productVariants.edges[0] != null ? content.productVariants.edges[0].node.product.id : "";

                variantId = variantId.Split('/').LastOrDefault();
                productId = productId.Split('/').LastOrDefault();

                return JObject.Parse(JsonConvert.SerializeObject(new
                {
                    Status = "Ok",
                    ItemId = itemId,
                    VariantId = variantId,
                    ProductId = productId
                }));
            }
            catch (Exception)
            {
                return JObject.Parse(JsonConvert.SerializeObject(new
                {
                    Status = "Error",
                    ItemId = "",
                    VariantId = "",
                    ProductId = ""
                }));
            }
        }

        public JObject GetIdsBySKU(string store, string sku, string token)
        {
            var filter = $"sku:{sku}";
            var query = new GraphQLRequest
            {
                Query = @"
                        query {
                              productVariants(first: 1, query: """ + filter + @""") {
                                edges {
                                        node {
                                              id
                                              inventoryItem {
                                                  id
                                              }
                                              product {
                                                  id
                                              }
                                        }
                                }
                              }
                          }"
            };

            var graphQLClient = new GraphQLClient($"https://{store}/admin/api/2023-01/graphql.json");
            graphQLClient.DefaultRequestHeaders.Add("X-Shopify-Access-Token", token);

            var graphQLResponse = graphQLClient.PostAsync(query);
            var content = graphQLResponse.Result.Data;
            var variants = content.productVariants.edges;

            try
            {
                string itemId = content.productVariants.edges[0] != null ? content.productVariants.edges[0].node.inventoryItem.id : "";
                string variantId = content.productVariants.edges[0] != null ? content.productVariants.edges[0].node.id : "";
                string productId = content.productVariants.edges[0] != null ? content.productVariants.edges[0].node.product.id : "";

                variantId = variantId.Split('/').LastOrDefault();
                productId = productId.Split('/').LastOrDefault();

                return JObject.Parse(JsonConvert.SerializeObject(new
                {
                    Status = "Ok",
                    ItemId = itemId,
                    VariantId = variantId,
                    ProductId = productId
                }));
            }
            catch (Exception)
            {
                return JObject.Parse(JsonConvert.SerializeObject(new
                {
                    Status = "Error",
                    ItemId = "",
                    VariantId = "",
                    ProductId = ""
                }));
            }
        }

        public List<string> GetProfuctsIdsBySKU(string store,  string token, string sku, string vendor)
        {
            var pIds = new List<string>();
            var filter = $"sku:{sku}";
            var query = new GraphQLRequest
            {
                Query = @"
                        query {
                              productVariants(first: 250, query: """ + filter + @""") {
                                edges {
                                        node {
                                              id
                                              product {
                                                  id
                                                  vendor
                                              }
                                        }
                                }
                              }
                          }"
            };

            var graphQLClient = new GraphQLClient($"https://{store}/admin/api/2023-01/graphql.json");
            graphQLClient.DefaultRequestHeaders.Add("X-Shopify-Access-Token", token);

            var graphQLResponse = graphQLClient.PostAsync(query);
            var content = graphQLResponse.Result.Data;

            try
            {
                foreach (var item in content.productVariants.edges)
                {
                    try
                    {
                        var included = true;
                        string productId = item.node.product.id;
                        string v = item.node.product.vendor;

                        if (vendor == v)
                        {
                            included = false;
                        }
                        if (included)
                        {
                            productId = productId.Split('/').LastOrDefault();
                            pIds.Add(productId);
                        }
                    }
                    catch (Exception)
                    {
                        //
                    }
                }
            }
            catch (Exception)
            {
                //
            }

            return pIds;
        }

        public dynamic GetInventoryItemByID(string store, string inventoryItemId, string token)
        {
            var query = new GraphQLRequest
            {
                Query = @"
                        query {
                              inventoryItem(id: """ + inventoryItemId + @""") {
                                id
                                inventoryLevels (first:1) {
                                  edges {
                                    node {
                                      id
                                      available
                                    }
                                  }
                                }
                              }
                        }"
            };

            var graphQLClient = new GraphQLClient($"https://{store}/admin/api/2023-01/graphql.json");
            graphQLClient.DefaultRequestHeaders.Add("X-Shopify-Access-Token", token);

            var graphQLResponse = graphQLClient.PostAsync(query);
            var content = graphQLResponse.Result.Data;

            try
            {
                string id = content.inventoryItem.inventoryLevels.edges[0].node.id;
                int available = content.inventoryItem.inventoryLevels.edges[0].node.available;

                return new { Id = id, Available = available };
            }
            catch (Exception)
            {
                return null;
            }
        }

        public JObject GetVariants(string origin, string token)
        {
            var client = new RestClient($"https://{origin}");
            var request = new RestRequest($"/admin/api/2023-01/variants.json");

            request.AddHeader("X-Shopify-Access-Token", token);

            try
            {
                var response = client.Get(request);
                var content = response.Content;
                var jObject = JObject.Parse(content);

                return response.StatusCode == HttpStatusCode.OK ? jObject : null;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public JObject GetVariant(string origin, string token, string variantId)
        {
            var client = new RestClient($"https://{origin}");
            var request = new RestRequest($"/admin/api/2023-01/variants/{variantId}.json");

            request.AddHeader("X-Shopify-Access-Token", token);

            try
            {
                var response = client.Get(request);
                var content = response.Content;
                var jObject = JObject.Parse(content);

                return response.StatusCode == HttpStatusCode.OK ? jObject : null;
            }
            catch (Exception e)
            {

                return null;
            }
        }

        public JObject GetInventoryItem(string origin, string token, string inventoryItemId)
        {
            var client = new RestClient($"https://{origin}");
            var request = new RestRequest($"/admin/api/2023-01/inventory_levels.json?inventory_item_ids={inventoryItemId}");

            request.AddHeader("X-Shopify-Access-Token", token);

            try
            {
                var response = client.Get(request);
                var content = response.Content;
                var jObject = JObject.Parse(content);

                return response.StatusCode == HttpStatusCode.OK ? jObject : null;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public JObject GetInventoryItem(string origin, string apiKey, string password, string inventoryItemId)
        {
            var encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(apiKey + ":" + password));
            var request = (HttpWebRequest)WebRequest.Create($"https://{apiKey}:{password}@{origin}/admin/api/2023-01/inventory_levels.json?inventory_item_ids={inventoryItemId}");
            request.Method = "GET";
            request.Headers.Add("Authorization", "Basic " + encoded);
            var response = request.GetResponse() as HttpWebResponse;
            var dataStream = response.GetResponseStream();
            var reader = new StreamReader(dataStream);
            var json = reader.ReadToEnd();
            var jObject = JObject.Parse(json);

            return jObject;
        }

        public dynamic UpdateInvetoryItem(string origin, string token, string inventoryItemId, string locationId, long available, int window)
        {
            var client = new RestClient($"https://{origin}");
            var request = new RestRequest($"/admin/api/2023-01/inventory_levels/set.json");

            request.AddHeader("X-Shopify-Access-Token", token);
            request.AddHeader("Content-Type", "application/json");

            try
            {
                var newInventory = new
                {
                    available = available > window ? available : 0,
                    inventory_item_id = inventoryItemId,
                    location_id = locationId
                };

                request.AddJsonBody(JsonConvert.SerializeObject(newInventory));

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

        public dynamic UpdateInvetoryItemGraph(string origin, string token, string inventoryLevelId, long oldAvailable, long available, int window)
        {
            var availableDelta = (available > window ? available : 0) - oldAvailable;

            if (availableDelta != 0)
            {
                var query = new GraphQLRequest
                {
                    Query = @"
                       mutation adjustInventoryLevelQuantity($inventoryAdjustQuantityInput: InventoryAdjustQuantityInput!) {
                          inventoryAdjustQuantity(input: $inventoryAdjustQuantityInput) {
                            inventoryLevel {
                              available
                            }
                            userErrors {
                              field
                              message
                            }
                          }
                        }",
                    Variables = new
                    {
                        inventoryAdjustQuantityInput = new
                        {
                            inventoryLevelId = inventoryLevelId,
                            availableDelta = availableDelta
                        }
                    }
                };

                var graphQLClient = new GraphQLClient($"https://{origin}/admin/api/2023-01/graphql.json");
                graphQLClient.DefaultRequestHeaders.Add("X-Shopify-Access-Token", token);

                var graphQLResponse = graphQLClient.PostAsync(query);
                var content = graphQLResponse.Result.Data;

                try
                {
                    return content.errors == null ? "Ok" : null;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return "Ok";
        }

        public dynamic UpdateInvetoryItem(string origin, string apiKey, string password, string inventoryItemId, string locationId, long available, int window)
        {
            var encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(apiKey + ":" + password));
            var request = (HttpWebRequest)WebRequest.Create($"https://{apiKey}:{password}@{origin}/admin/api/2023-01/inventory_levels/set.json");

            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", "Basic " + encoded);

            var newInventory = new
            {
                available = available > window ? available : 0,
                inventory_item_id = inventoryItemId,
                location_id = locationId
            };
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                var data = JsonConvert.SerializeObject(newInventory);
                streamWriter.Write(data);
            }
            var response = request.GetResponse() as HttpWebResponse;
            var dataStream = response.GetResponseStream();
            var reader = new StreamReader(dataStream);
            var json = reader.ReadToEnd();

            return json;
        }

        public dynamic GetProducts(IServiceProvider serviceProvider, string appId, string shopId)
        {
            var repo = new RepositoryAWS<Shop>(serviceProvider);
            var shop = repo.Get(appId, shopId);

            if (shop != null)
            {
                var client = new RestClient($"https://{shopId}");
                var request = new RestRequest("/admin/api/2023-01/products.json");
                request.AddHeader("X-Shopify-Access-Token", shop.Token);

                var response = client.Get(request);
                var content = response.Content;
                var jObject = JObject.Parse(content);

                return jObject;
            }
            else
            {
                return null;
            }
        }
        public IRestResponse GetProduct(string shopId, string token, string productId)
        {
            var path = $"/admin/api/2023-01/products/{productId}.json";
            var response = Helper.NewRequestToken(shopId, path, token, Method.GET);

            return response;

        }

        public dynamic GetCollections(IServiceProvider serviceProvider, string appId, string shopId)
        {
            var repo = new RepositoryAWS<Shop>(serviceProvider);
            var shop = repo.Get(appId, shopId);

            if (shop != null)
            {
                var client = new RestClient($"https://{shopId}");
                var request = new RestRequest("/admin/api/2023-01/custom_collections.json");
                var request2 = new RestRequest("/admin/api/2023-01/smart_collections.json");

                request.AddHeader("X-Shopify-Access-Token", shop.Token);
                request2.AddHeader("X-Shopify-Access-Token", shop.Token);

                var response = client.Get(request);
                var response2 = client.Get(request2);
                var content = response.Content;
                var content2 = response2.Content;

                return new
                {
                    smart = JObject.Parse(content2),
                    custom = JObject.Parse(content)
                };
            }
            else
            {
                return null;
            }
        }

        public dynamic GetProductByCollections(IServiceProvider serviceProvider, string appId, string shopId, string collectionId)
        {
            var repo = new RepositoryAWS<Shop>(serviceProvider);
            var shop = repo.Get(appId, shopId);

            if (shop != null)
            {
                var client = new RestClient($"https://{shopId}");
                var request = new RestRequest($"/admin/api/2023-01/products.json?collection_id={collectionId}");

                request.AddHeader("X-Shopify-Access-Token", shop.Token);

                var response = client.Get(request);
                var content = response.Content;

                return JObject.Parse(content);
            }
            else
            {
                return null;
            }
        }

        public static IRestResponse EditProduct(IServiceProvider serviceProvider, string appId, string shopId, string token, dynamic variant)
        {
            var client = new RestClient($"https://{shopId}");
            var request = new RestRequest($"admin/api/2023-01/variants/{variant.variant.id}.json");
            request.AddHeader("X-Shopify-Access-Token", token);

            request.AddJsonBody(variant);

            var response = client.Put(request);
            var content = response.Content; // raw content as string

            //return JObject.Parse(content);
            return response;
        }

        public dynamic EditProducts(IServiceProvider serviceProvider, string appId, string shopId, List<Product> products)
        {
            var repo = new RepositoryAWS<Shop>(serviceProvider);
            var shop = repo.Get(appId, shopId);
            var results = new List<string>();

            if (shop != null)
            {
                foreach (var product in products)
                {

                    var client = new RestClient($"https://{shopId}");
                    var request = new RestRequest($"admin/api/2023-01/variants/{product.RowKey}.json");
                    request.AddHeader("X-Shopify-Access-Token", shop.Token);

                    var variant = new
                    {
                        variant = new
                        {
                            id = product.RowKey,
                            compare_at_price = product.CurrentComparePrice,
                            price = product.CurrentPrice
                        }
                    };

                    request.AddJsonBody(variant);

                    var response = client.Put(request);
                    var content = response.Content; // raw content as string
                    var jObject = JObject.Parse(content);

                    results.Add(jObject.ToString());
                }

                return results;
            }
            else
            {
                return null;
            }
        }

        public IRestResponse CreateProduct(string shopId, string token, InventoryWrapper product)
        {
            var path = $"admin/api/2023-01/products.json";
            var variants = new List<dynamic>();

            foreach (var item in product.variants)
            {
                var variant = new
                {
                    title = item.title,
                    option1 = item.title,
                    barcode = item.barcode,
                    compare_at_price = item.compare_at_price + "",
                    inventory_quantity = item.inventory_quantity,
                    price = item.price + "",
                    requires_shipping = item.requires_shipping,
                    sku = item.sku,
                    taxable = item.taxable,
                    weight = item.weight,
                    weight_unit = item.weight_unit,
                };

                variants.Add(variant);
            }

            var json = new
            {
                product = new
                {
                    title = product.title,
                    body_html = product.body_html,
                    tags = product.tags.Split(","),
                    vendor = product.vendor,
                    product_type = product.product_type,
                    variants = variants
                }
            };
            var response = Helper.NewRequestToken(shopId, path, token, Method.POST, JsonConvert.SerializeObject(json));

            return response;
        }
        public string CreateProductPayment(string shopId, string token, string productIdBase, double payment)
        {
            var productBaseResponse = GetProduct(shopId, token, productIdBase);

            if (productBaseResponse.StatusCode == HttpStatusCode.OK)
            {
                var productBase = JObject.Parse(productBaseResponse.Content);
                var path = $"admin/api/2023-01/products.json";
                var variants = new List<dynamic>();
                dynamic productJson = new ExpandoObject();
                dynamic variant = new ExpandoObject();

                variant.title = (string)productBase["product"]["variants"][0]["title"];
                variant.option1 = (string)productBase["product"]["variants"][0]["option1"];
                variant.inventory_management = "shopify";
                variant.sku = (string)productBase["product"]["variants"][0]["sku"];
                variant.barcode = (string)productBase["product"]["variants"][0]["barcode"];
                variant.price = payment + "";
                variant.requires_shipping = false;
                variant.taxable = false;
                variant.inventory_policy = "continue";

                variants.Add(variant);

                productJson.title = (string)productBase["product"]["title"] + " - " + payment.ToString("C0");
                productJson.body_html = (string)productBase["product"]["body_html"];
                productJson.variants = variants;
                productJson.status = "active";
                productJson.product_type = "abono";

                var images = new List<dynamic>();

                images.Add(new { src = (string)productBase["product"]["image"]["src"] });
                productJson.images = images;

                var json = new
                {
                    product = productJson
                };
                var response = Helper.NewRequestToken(shopId, path, token, Method.POST, JsonConvert.SerializeObject(json));

                if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
                {

                    var newProduct = JObject.Parse(response.Content);

                    return (string)newProduct["product"]["variants"][0]["id"];
                }
            }

            return null;
        }

        public List<string> RemoveProductPayment(string shopId, string token, string sku, string vendor)
        {
            var ids = GetProfuctsIdsBySKU(shopId, token, sku, vendor);

            foreach (var id in ids)
            {
                var path = $"admin/api/2023-01/products/{id}.json";

                var response = Helper.NewRequestToken(shopId, path, token, Method.DELETE);

                if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
                {
                    //var newProduct = JObject.Parse(response.Content);
                }
            }

            return ids;
        }

        public IRestResponse CreateProductDraft(string shopId, string token, string title, JToken product)
        {
            var path = $"admin/api/2023-01/products.json";
            var variants = new List<dynamic>();
            dynamic productJson = new ExpandoObject();
            dynamic variant = new ExpandoObject();

            variant.title = title;
            variant.option1 = title;
            variant.inventory_management = "shopify";
            variant.sku = (string)product["Calculated_ParteSerie"];
            variant.barcode = (string)product["Part_PartNum"];

            if ((double)product["Calculated_Precio"] != null)
                variant.price = (double)product["Calculated_Precio"] + "";

            variants.Add(variant);

            productJson.title = (string)product["Part_PartDescription"];
            productJson.variants = variants;
            productJson.status = "draft";

            var images = new List<dynamic>();

            images.Add(new { src = (string)product["Calculated_Imagen"] });
            productJson.images = images;

            var json = new
            {
                product = productJson
            };
            var response = Helper.NewRequestToken(shopId, path, token, Method.POST, JsonConvert.SerializeObject(json));

            return response;
        }

        public IRestResponse UpdateProduct(string shopId, string token, string productId, JToken product)
        {
            var path = $"admin/api/2023-01/products/{productId}.json";
            var variants = new List<dynamic>();
            dynamic productJson = new ExpandoObject();

            productJson.id = productId;
            productJson.title = (string)product["Part_PartDescription"];

            var json = new
            {
                product = productJson
            };
            var response = Helper.NewRequestToken(shopId, path, token, Method.PUT, JsonConvert.SerializeObject(json));

            return response;
        }

        public IRestResponse CreateVariant(string shopId, string token, string productId, string title, JToken variant)
        {
            var path = $"/admin/api/2023-01/products/{productId}/variants.json";
            dynamic variantJson = new ExpandoObject();

            variantJson.title = $"{title}-{(string)variant["Calculated_ParteSerie"]}";
            variantJson.option1 = $"{title}-{(string)variant["Calculated_ParteSerie"]}";
            variantJson.inventory_management = "shopify";
            variantJson.sku = (string)variant["Calculated_ParteSerie"];
            variantJson.barcode = (string)variant["Part_PartNum"];
            variantJson.inventory_management = "shopify";

            if ((double)variant["Calculated_Precio"] != null)
                variantJson.price = (double)variant["Calculated_Precio"] + "";

            dynamic json = new
            {
                variant = variantJson
            };

            var response = Helper.NewRequestToken(shopId, path, token, Method.POST, JsonConvert.SerializeObject(json));

            return response;
        }

        public IRestResponse UpdateVariant(string shopId, string token, string variantId, string title, JToken variant)
        {
            var path = $"/admin/api/2023-01/variants/{variantId}.json";
            dynamic variantJson = new ExpandoObject();

            //variantJson.title = title;
            //variantJson.option1 = title;
            //variantJson.inventory_management = "shopify";
            //variantJson.sku = (string)variant["Calculated_ParteSerie"];
            //variantJson.barcode = (string)variant["Part_PartNum"];

            if ((double)variant["Calculated_Precio"] != null)
                variantJson.price = (double)variant["Calculated_Precio"] + "";

            dynamic json = new
            {
                variant = variantJson
            };

            var response = Helper.NewRequestToken(shopId, path, token, Method.PUT, JsonConvert.SerializeObject(json));
            return response;
        }
    }
}
