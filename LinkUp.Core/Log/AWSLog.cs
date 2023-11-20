using System;
using System.Collections.Generic;
using System.Text;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using LinkUp.Core.AWS.Business;

namespace LinkUp.Core.AWS.Log
{
	public class AWSLog
	{
        public static AmazonCloudWatchLogsClient Client;
        public AWSLog()
        {
            var awsAccessKeyId = Environment.GetEnvironmentVariable("awsAccessKeyId", EnvironmentVariableTarget.Process);
            var awsSecretAccessKey = Environment.GetEnvironmentVariable("awsSecretAccessKey", EnvironmentVariableTarget.Process);
            var awsRegionName = Environment.GetEnvironmentVariable("awsRegionName", EnvironmentVariableTarget.Process);

            Client = new AmazonCloudWatchLogsClient(awsAccessKeyId, awsSecretAccessKey, Helper.GetRegion(awsRegionName));
            
        }

        public AWSLog(string awsAccessKeyIdName, string awsSecretAccessKeyName)
        {
            var awsAccessKeyId = Environment.GetEnvironmentVariable(awsAccessKeyIdName, EnvironmentVariableTarget.Process);
            var awsSecretAccessKey = Environment.GetEnvironmentVariable(awsSecretAccessKeyName, EnvironmentVariableTarget.Process);

            Client = new AmazonCloudWatchLogsClient(awsAccessKeyId, awsSecretAccessKey, Helper.GetRegion());
        }

        public AWSLog(string awsAccessKeyId, string awsSecretAccessKey, string awsRegionName)
        {
            Client = new AmazonCloudWatchLogsClient(awsAccessKeyId, awsSecretAccessKey, Helper.GetRegion(awsRegionName));
        }


        public void LogMessage(string logGroupName, string logStreamName, string message, string prefix = "", string suffix = "")
        {
           
            var parts = SplitByByteSize(message, 262000);

            foreach (var msg in parts)
            {
                var request = new PutLogEventsRequest
                {
                    LogGroupName = logGroupName,
                    LogStreamName = logStreamName,
                    LogEvents = new List<InputLogEvent>
                    {
                        new InputLogEvent
                        {
                            Timestamp = DateTime.Now,
                            Message =$"{prefix}{msg}{suffix}"
                        }
                    }
                };

                var result = Client.PutLogEventsAsync(request).Result;
            }
        }

        public static IEnumerable<string> SplitByByteSize(string str, int maxByteSize)
        {
            Encoding encoding = Encoding.UTF8;
            int byteCount = 0;
            int startIndex = 0;

            for (int i = 0; i < str.Length; i++)
            {
                byteCount += encoding.GetByteCount(str[i].ToString());

                if (byteCount > maxByteSize)
                {
                    yield return str.Substring(startIndex, i - startIndex);
                    startIndex = i;
                    byteCount = encoding.GetByteCount(str[i].ToString());
                }
            }

            if (startIndex < str.Length)
            {
                yield return str.Substring(startIndex);
            }
        }

    }
}

