using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FastDFS.Client;

namespace FastDFS.Client.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var files = new[]
            {
                ""
            };

            ClientGlobal.init("fdfs_client.json");
            Console.WriteLine(ClientGlobal.configInfo());

            var trackerClient = new TrackerClient();
            var trackerServer = trackerClient.getConnection();
            var storageClient = new StorageClient(trackerServer, null);

            Console.WriteLine($"Total files to be upload: {files.Length}");
            foreach (var file in files)
            {
                var metaData = new NameValueCollection
                {
                    ["filename"] = file
                };
                var fileId = string.Join("/", storageClient.upload_file(file, null, metaData));
                Console.WriteLine($"Upload success. file: {file}, file id: {fileId}");
            }
        }
    }
}
