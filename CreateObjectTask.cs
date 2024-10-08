using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

namespace S3Operations
{
    class CreateObjectTask
    {
        public async Task Run()
        {
            Console.WriteLine("\nStart of create object task");

            Console.WriteLine("\nReading configuration for bucket name...");
            var configSettings = ConfigSettingsReader<S3ConfigSettings>.Read("S3");

            try
            {
                using var s3Client = new AmazonS3Client();

                // Create object in the Amazon S3 bucket
                await UploadObject(s3Client,
                                   configSettings.BucketName,
                                   $"{configSettings.ObjectName}{configSettings.SourceFileExtension}",
                                   configSettings.SourceContentType,
                                   new Dictionary<string, string>
                                   {
                                       { configSettings.MetadataKey, configSettings.MetadataValue }
                                   });
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }

            Console.WriteLine("\nEnd of create object task");
        }

        async Task UploadObject(IAmazonS3 s3Client,
                                string bucketName,
                                string name,
                                string contentType,
                                IDictionary<string, string> metadata)
        {
            Console.WriteLine("Creating object...");

            // Start TODO 5: create a object by transferring the file to the S3 bucket,
            // set the contentType of the file and add any metadata passed to this function.
            var filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), name);



            try
            {

                var putObjectRequest = new PutObjectRequest
                {
                    Key = name,
                    BucketName = bucketName,
                    ContentType = contentType,
                    FilePath = filePath,
                };
                foreach (var item in metadata)
                {
                    putObjectRequest.Metadata.Add(item.Key, item.Value);
                }

                var response = await s3Client.PutObjectAsync(putObjectRequest);
                Console.WriteLine($"{response.ETag}");
                var bytes = File.ReadAllBytes(filePath);
                var hashString = MD5HashComputer.CalculateMD5Hash(bytes);
                if (response.ETag.Trim('"') != hashString)
                {
                    Console.WriteLine($"File {filePath}  upload failed.Etag Received {response.ETag}. Etag Sent {hashString}");
                }
            }
            catch (AmazonS3Exception ex)
            {

                Console.WriteLine($"File {filePath}  upload to S3 failed. {ex.Message}");
            }


            // End TODO 5

            Console.WriteLine("Finished creating object");



        }


    }
}
