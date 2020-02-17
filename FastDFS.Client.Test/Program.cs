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
                "1.jpg",
                "2.jpg",
                "3.jpg",
            };

            ClientGlobal.InitFromFile("fdfs_client.json");
            Console.WriteLine(ClientGlobal.configInfo());

            var trackerClient = new TrackerClient();
            var trackerServer = trackerClient.getConnection();
            var storageClient = new StorageClient(trackerServer, null);

            var fileIds = new List<string[]>();
            Console.WriteLine($"Total files to be upload: {files.Length}");
            foreach (var file in files)
            {
                var metaData = new NameValueCollection
                {
                    ["filename"] = file
                };
                var result = storageClient.upload_file(file, null, metaData);
                if (result != null)
                {
                    fileIds.Add(result);

                    var fileId = string.Join("/", result);
                    Console.WriteLine($"Upload success. file: {file}, file id: {fileId}");
                }
                else
                {
                    Console.WriteLine($"Failed to upload file: {file}");
                }
            }

            foreach (var fileId in fileIds)
            {
                var fileData = storageClient.download_file(fileId[0], fileId[1]);
                using (var fs = System.IO.File.Create(System.IO.Path.GetFileName(fileId[1])))
                {
                    fs.Write(fileData, 0, fileData.Length);
                }
            }


            Console.ReadLine();
        }
    }
}
