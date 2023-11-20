using System;
using Amazon;

namespace LinkUp.Core.AWS.Business
{
	public static class Helper
	{
		
        public static RegionEndpoint GetRegion(string region = "")
        {
            switch (region)
            {

                case "us-east-1":
                    return RegionEndpoint.USEast1;
                case "us-west-1":
                    return RegionEndpoint.USWest1;
                case "us-west-2":
                    return RegionEndpoint.USWest2;
                case "af-south-1":
                    return RegionEndpoint.AFSouth1;
                case "ap-east-1":
                    return RegionEndpoint.APEast1;
                case "ap-southeast-3":
                    return RegionEndpoint.APSoutheast3;
                case "ap-south-1":
                    return RegionEndpoint.APSouth1;
                case "ap-northeast-3":
                    return RegionEndpoint.APNortheast3;
                case "ap-northeast-2":
                    return RegionEndpoint.APNortheast2;
                case "ap-southeast-1":
                    return RegionEndpoint.APSoutheast1;
                case "ap-southeast-2":
                    return RegionEndpoint.APSoutheast2;
                case "ap-northeast-1":
                    return RegionEndpoint.APNortheast1;
                case "ca-central-1":
                    return RegionEndpoint.CACentral1;
                case "eu-central-1":
                    return RegionEndpoint.EUCentral1;
                case "eu-west-1":
                    return RegionEndpoint.EUWest1;
                case "eu-west-2":
                    return RegionEndpoint.EUWest2;
                case "eu-south-1":
                    return RegionEndpoint.EUSouth1;
                case "eu-west-3":
                    return RegionEndpoint.EUWest3;
                case "eu-north-1":
                    return RegionEndpoint.EUNorth1;
                case "me-south-1":
                    return RegionEndpoint.MESouth1;
                case "sa-east-1":
                    return RegionEndpoint.SAEast1;
                case "us-east-2":
                    return RegionEndpoint.USEast2;
                default:
                    return RegionEndpoint.USEast1;
            }
        }
    }
}

