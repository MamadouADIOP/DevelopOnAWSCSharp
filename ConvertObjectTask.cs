using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

namespace S3Operations
{
    class ConvertObjectTask
    {
        public async Task Run()
        {
            Console.WriteLine("\nStart of convert object task");

            Console.WriteLine("\nReading configuration for bucket name...");
            var configSettings = ConfigSettingsReader<S3ConfigSettings>.Read("S3");

            try
            {
                using var s3Client = new AmazonS3Client();

                // Get the object from Amazon S3
                Console.WriteLine("\nGetting the CSV object from the Amazon S3 bucket");
                var csvData = await GetCSVFile(s3Client,
                                               configSettings.BucketName,
                                               $"{configSettings.ObjectName}{configSettings.SourceFileExtension}");

                // Convert the object to the new format
                Console.WriteLine("\nConverting CSV string to JSON...");
                var jsonData = ConvertToJSON(csvData);

                // Uploaded the converted object to Amazon S3
                Console.WriteLine("\nCreating the new JSON object in Amazon S3...");
                await CreateObject(s3Client,
                                   configSettings.BucketName,
                                   $"{configSettings.ObjectName}{configSettings.ProcessedFileExtension}",
                                   jsonData,
                                   configSettings.ProcessedContentType,
                                   new Dictionary<string, string>
                                   {
                                       { configSettings.MetadataKey, configSettings.MetadataValue }
                                   });
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }

            Console.WriteLine("\nEnd of convert object task");
        }

        async Task<string> GetCSVFile(IAmazonS3 s3Client, string bucketName, string name)
        {
            string contents = null;

            // Start TODO 6: Download the file contents to the
            // contents object so that it can be decoded to a string.
            var getObjectRequest = new GetObjectRequest { BucketName = bucketName, Key = name};
            var response = await s3Client.GetObjectAsync(getObjectRequest);
            Console.WriteLine($"The returned object {name} etag is {response.ETag}");
            using (var reader =new StreamReader(response.ResponseStream))
            {
                contents =  await reader.ReadToEndAsync();
            }
            return contents;
        }

        async Task CreateObject(IAmazonS3 s3Client,
                                string bucketName,
                                string name,
                                string data,
                                string contentType,
                                IDictionary<string, string> metadata)
        {
            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = name,
                ContentBody = data,
                ContentType = contentType
            };
            
            foreach (var key in metadata.Keys)
            {
                request.Metadata.Add(key, metadata[key]);
            }           
            // Start TODO 7: Create an Amazon S3 object with the converted data
            var response = await s3Client.PutObjectAsync(request);
            var bytes = Encoding.UTF8.GetBytes(data);
            var hashString = MD5HashComputer.CalculateMD5Hash(bytes);
            if (response.ETag.Trim('"') != hashString)
            {
                Console.WriteLine($"File  upload failed.Etag Received {response.ETag}. Etag Sent {hashString}");
            }
            // End TODO 7

            Console.WriteLine("Successfully created object");
        }

        string ConvertToJSON(string csv)
        {
            // simple conversion, appropriate to the lab data

            var rows = csv.Split('\n');
            var headers = rows[0].Split(',');

            var data = new List<Dictionary<string, string>>();

            for (var row = 1; row < rows.Length; row++)
            {
                var dict = new Dictionary<string, string>();

                var rowData = rows[row].Split(',');

                var col = 0;
                foreach (var header in headers)
                {
                    dict.Add(header, rowData[col++]);
                }

                data.Add(dict);
            }

            var jsonData = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            return jsonData;
        }
    }
}
