using System;
using Amazon.DynamoDBv2.DataModel;
using LinkUp.Core.AWS.Business;
using LinkUp.Core.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace LinkUp.Shopify.Entities
{
    /// <summary>
    /// PartitionKey=  Shopify
    /// RowKey= AppId
    /// </summary>
    [DynamoDBTable("Shopify_App_Apps")]
    public class App : DynamoDBTableEntity
    {
        public string Key { get; set; }
        public string Name { get; set; }
    }
}
