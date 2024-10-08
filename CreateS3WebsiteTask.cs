﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

namespace S3Operations
{
    class CreateS3WebsiteTask
    {
        public async Task Run()
        {
            Console.WriteLine("\nStart of create website task");

            Console.WriteLine("\nReading configuration for bucket name...");
            var configSettings = ConfigSettingsReader<S3ConfigSettings>.Read("S3");

            try
            {
                using var s3Client = new AmazonS3Client();

                // Upload html files
                Console.WriteLine("\nUploading files for the website...");
                await UploadWebsiteFiles(s3Client, configSettings.BucketName);

                // Enable web hosting
                Console.WriteLine("\nEnabling web hosting on the bucket...");
                await EnableWebHosting(s3Client, configSettings.BucketName);

                // Configure bucket policy
                Console.WriteLine("\nAdding a bucket policy to allow traffic from the internet...");
                await AllowAccessFromWeb(s3Client, configSettings.BucketName);

                Console.WriteLine("\nYou can access the website at:");
                Console.WriteLine($"http://{configSettings.BucketName}.s3-website-{s3Client.Config.RegionEndpoint.SystemName}.amazonaws.com");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }

            Console.WriteLine("\nEnd of create website task");
        }

        async Task UploadWebsiteFiles(IAmazonS3 s3Client, string bucketName)
        {
            var fileNames = GetFileList();
            foreach (var obj in fileNames)
            {
                var key = obj.Item1;
                var filename = System.IO.Path.Join(Environment.CurrentDirectory, "html/", key);
                var contentType = obj.Item2;
               
                Console.WriteLine($"Upload: html/{key} to s3://{bucketName}/{key}");
                // Start TODO 8: Upload file to the bucket
                var putObjectRequest = new PutObjectRequest { BucketName = bucketName, Key = key, ContentType = contentType, FilePath = filename };
                Console.WriteLine($"Upload: html/{key} to s3://{bucketName}/{key}");
                var response = await s3Client.PutObjectAsync(putObjectRequest);
                var bytes = File.ReadAllBytes(filename);
                var hashString = MD5HashComputer.CalculateMD5Hash(bytes);
               
                if (response.ETag.Trim('"') != hashString)
                {
                    Console.WriteLine($"File {filename}  upload failed.Etag Received {response.ETag}. Etag Sent {hashString}");
                }
                // End TODO 8
            }
        }

        async Task EnableWebHosting(IAmazonS3 s3Client, string bucketName)
        {
            // Start TODO 9: enable an Amazon S3 web hosting using the objects you uploaded in the last method
            // as the index and error document for the website.
            var websiteConfiguration = new WebsiteConfiguration { ErrorDocument = "error.html", IndexDocumentSuffix = "index.html" };
            var response = await s3Client.PutBucketWebsiteAsync(bucketName, websiteConfiguration);
             
            // End TODO 9
        }

        async Task AllowAccessFromWeb(IAmazonS3 s3Client, string bucketName)
        {
            var bucketPolicy = 
                "{\r"
                + "\"Version\": \"2012-10-17\",\r"
                + "\"Statement\": [{\r"
                +     "\"Effect\": \"Allow\",\r"
                +     "\"Principal\": \"*\",\r"
                +     "\"Action\": [\"s3:GetObject\"],\r"
                +     $"\"Resource\": \"arn:aws:s3:::{bucketName}/*\"\r"
                +   "}]\r"
                + "}";

            // Start TODO 10: Apply the provided bucket policy to the website bucket
            // to allow your objects to be accessed from the internet.
            var publicAccessBlockConfiguration = new PublicAccessBlockConfiguration { RestrictPublicBuckets = false, BlockPublicPolicy = false };
            var putPublicAccessBlockRequest = new PutPublicAccessBlockRequest { PublicAccessBlockConfiguration = publicAccessBlockConfiguration, BucketName = bucketName };
            
            var response = await s3Client.PutPublicAccessBlockAsync(putPublicAccessBlockRequest);
            var putBucketPolicyRequest = new PutBucketPolicyRequest { BucketName = bucketName, Policy = bucketPolicy };
            var response1 = await s3Client.PutBucketPolicyAsync(putBucketPolicyRequest);
            
            // End TODO 10
        }

        IEnumerable<Tuple<string, string>> GetFileList()
        {
            return new Tuple<string, string>[]
            {
                new Tuple<string, string>("404.png", "image/png"),
                new Tuple<string, string>("header.png", "image/png"),
                new Tuple<string, string>("error.html", "text/html"),
                new Tuple<string, string>("index.html", "text/html"),
                new Tuple<string, string>("styles.css", "text/css")
            };
        }
    }
}
