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
            var files = new[] {
                @"d:\downloads\npp.7.7.Installer.exe",
                @"d:\downloads\AxureRP-Setup.exe",
                @"d:\downloads\ChromeStandaloneSetup64.exe",
                @"d:\downloads\FiddlerSetup-5.0.20173.50948.exe",
                @"d:\downloads\innosetup-qsp-6.0.2.exe",
                @"d:\downloads\ndp48-x86-x64-allos-chs.exe",
                @"d:\downloads\Simplified Chinese Pin-Yin Conversion Library Document.chm",
                @"d:\downloads\XamlpadX 4.0.exe",
                @"d:\downloads\ZXing.Net.0.16.4.0.zip",
                @"d:\downloads\LINQPad5Setup.exe",
                @"d:\downloads\depends22_x64.zip"
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
