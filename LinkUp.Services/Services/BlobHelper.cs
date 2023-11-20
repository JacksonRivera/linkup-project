using System;
using System.IO;
using System.Net;
using System.Text;

namespace LinkUp.Shopify.Services
{
    public static class BlobHelper
    {
        public static string Write(IServiceProvider serviceProvider, string container, string content)
        {
            var bm = new BlobManager(container, serviceProvider);

            var bytes = Encoding.UTF8.GetBytes(content);
            var ms = new MemoryStream();
            ms.Write(bytes, 0, bytes.Length);
            ms.Position = 0;

            var uri = bm.Upload(ms, ".json");

            return uri;
        }

        public static string Read(string uri)
        {
            var client = new WebClient();

            var bytes = client.DownloadData(uri);

            return Encoding.UTF8.GetString(bytes);

        }
    }

    
}
