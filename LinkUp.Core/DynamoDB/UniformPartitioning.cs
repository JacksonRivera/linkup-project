using System;
using System.Collections.Generic;
using System.Text;

namespace LinkUp.Core.AWS.DynamoDB
{
    public class UniformPartitioning : System.Attribute
    {
        /// <summary>
        /// Número de particiones
        /// </summary>
        public int Partitions { get; set; }

        public UniformPartitioning(int partitions)
        {
            Partitions = partitions;
        }

        public int GetRandomPartition()
        {
            var rnd = new Random();
            int partition = rnd.Next(1, Partitions + 1);

            return partition;
        }
        public int GetPartition(string input)
        {
            var product = 1;

            foreach (var c in input)
            {
                int unicode = c;
                product *= c;
            }

            var partition = Math.Abs((product % Partitions) + 1);

            return partition;
        }
        public string GetNewPartitionKey(string partitionKey, string input)
        {
            var partition = GetPartition(input);

            return $"{partitionKey}#{partition}";
        }
        public List<string> GetPartitions(string partitionKey)
        {
            var partitionKeys = new List<string>();

            for (int i = 1; i <= Partitions; i++)
            {
                partitionKeys.Add($"{partitionKey}#{i}");
            }

            return partitionKeys;
        }
    }
}
