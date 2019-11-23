using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace RecognizerDbConnection
{
    public class ImageContext : DbContext
    {

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer(@"Data Source=(localdb)\mssqllocaldb;Initial Catalog=StoreDB;");
        }
        //Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False
        public DbSet<RecognizedImage> RecognizedImages { get; set; }
        //public DbSet<ImageBytes> ImageBytesTable { get; set; }

        public RecognizedImage GetImageByPath(string imagePath)
        {
            var img_bytes = ReadBytesFromFile(imagePath);
            string checksum = ComputeMD5Checksum(img_bytes);
            var recognized_image = RecognizedImages.SingleOrDefault(image => image.Checksum == checksum);
            if (recognized_image != null)
            {
                var recognized_bytes = recognized_image.Bytes;
                if (recognized_bytes.SequenceEqual<byte>(img_bytes))
                {
                    recognized_image.Counter += 1;
                    SaveChanges();
                }
                else
                {
                    recognized_image = null;
                }

            }

            return recognized_image;

        }

        public void SaveRecognitionResult(string path, string recognized_class)
        {
            var img_bytes = ReadBytesFromFile(path);
            string checksum = ComputeMD5Checksum(img_bytes);
            var already_there = RecognizedImages.SingleOrDefault(image => image.Checksum == checksum);
            if (already_there != null)
                return;

           
            RecognizedImages.Add(new RecognizedImage()
            {
                Checksum = checksum,
                Class = recognized_class,
                Bytes = img_bytes,
                Counter = 0
            });
            SaveChanges();
        }

        //public Tuple<BlockingCollection<string>, string[]> GetRecognizedAndUnrecognizedImages(string[] imagePaths)
        //{
        //    var recognized_results = new BlockingCollection<string>();
        //    List<string> unrecognized_image_paths = new List<string>();
        //    foreach (var path in imagePaths)
        //    {
        //        RecognizedImage img = GetImageByPath(path);
        //        if (img != null)
        //        {
        //            recognized_results.Add(img.Path + '\n' + img.Class + '$' + img.Counter.ToString());
        //        }
        //        else
        //        {
        //            unrecognized_image_paths.Add(path);
        //        }
        //    }
        //    recognized_results.CompleteAdding();
        //    return Tuple.Create(recognized_results, unrecognized_image_paths.ToArray());
        //}

        public void Clear()
        {
            RecognizedImages.RemoveRange(RecognizedImages);
            //ImageBytesTable.RemoveRange(ImageBytesTable);
            SaveChanges();
        }


        private string ComputeMD5ChecksumFromPath(string path)
        {
            using (FileStream fs = System.IO.File.OpenRead(path))
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] fileData = new byte[fs.Length];
                fs.Read(fileData, 0, (int)fs.Length);
                byte[] checkSum = md5.ComputeHash(fileData);
                string result = BitConverter.ToString(checkSum).Replace("-", String.Empty);
                return result;
            }
        }

        private string ComputeMD5Checksum(byte[] fileData)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] checkSum = md5.ComputeHash(fileData);
            string result = BitConverter.ToString(checkSum).Replace("-", String.Empty);
            return result;

        }

        private byte[] ReadBytesFromFile(string path)
        {
            using (FileStream fs = File.OpenRead(path))
            {
                byte[] fileData = new byte[fs.Length];
                fs.Read(fileData, 0, (int)fs.Length);
                return fileData;
            }
        }

        private RecognizedImage TryGetImage(byte[] imageBytes)
        {
            string checksum = ComputeMD5Checksum(imageBytes);
            var foundImage = RecognizedImages.SingleOrDefault(image => image.Checksum == checksum);
            if (foundImage != null)
            {
                var recognized_bytes = foundImage.Bytes;
                if (recognized_bytes.SequenceEqual<byte>(imageBytes))
                {
                    
                    foundImage.Counter += 1;
                    SaveChanges();
                    return foundImage;
                }
                else
                {
                    return null;
                }
            }
            else
                return null;
        }

        public Tuple<string, string> TryGetRecognizedImage(byte[] imageBytes)
        {
            var foundImage = TryGetImage(imageBytes);
            if (foundImage != null)
            {
                return new Tuple<string, string>(foundImage.Class, foundImage.Counter.ToString());
            }
            else
            {
                return null;
            }

        }

        public void SaveRecognizedImage(byte[] imageBytes, string recognizedClass)
        {
            var foundImage = TryGetImage(imageBytes);
            if (foundImage != null)
                return;

            string checksum = ComputeMD5Checksum(imageBytes);

            RecognizedImages.Add(new RecognizedImage()
            {
                Checksum = checksum,
                Class = recognizedClass,
                Bytes = imageBytes,
                Counter = 0
            });
            SaveChanges();
        }

        public Dictionary<string, string> GetStatistics()
        {
            var dict = new Dictionary<string, string>();
            foreach (var image in RecognizedImages)
            {
                dict.Add("ImageId:" + image.Id.ToString(), image.Counter.ToString());
            }
            return dict;
        }

    }
}
