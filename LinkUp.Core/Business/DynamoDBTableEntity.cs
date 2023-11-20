using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkUp.Core.AWS.Business
{
   public class DynamoDBTableEntity
    {
        [DynamoDBHashKey]
        public string PartitionKey { get; set; }
        [DynamoDBRangeKey]
        public string RowKey { get; set; }
        public string Id { get; set; }
    }
}
