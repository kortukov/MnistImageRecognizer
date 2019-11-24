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
        public DbSet<RecognizedImage> RecognizedImages { get; set; }
        

        public void Clear()
        {
            RecognizedImages.RemoveRange(RecognizedImages);
            SaveChanges();
        }


        private string ComputeMD5Checksum(byte[] fileData)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] checkSum = md5.ComputeHash(fileData);
            string result = BitConverter.ToString(checkSum).Replace("-", String.Empty);
            return result;

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
