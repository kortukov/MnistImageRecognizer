using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace ImageDbConnection
{
    public class ImageContext : DbContext
    {
        public ImageContext()
            : base("DbConnection")
        { }

        public DbSet<RecognizedImage> RecognizedImages { get; set; }
        public DbSet<ImageBytes> ImageBytesTable { get; set; }

        public RecognizedImage GetImageByPath(string imagePath)
        {
            var img_bytes = ReadBytesFromFile(imagePath);
            string checksum = ComputeMD5Checksum(img_bytes);
            var recognized_image = RecognizedImages.SingleOrDefault(image => image.Checksum == checksum);
            if (recognized_image != null)
            {
                var recognized_bytes = recognized_image.Bytes.ImageBlob;
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

            var newBytes = new ImageBytes() { ImageBlob = img_bytes };
            ImageBytesTable.Add(newBytes);
            RecognizedImages.Add(new RecognizedImage() {
                Checksum = checksum, Class = recognized_class, Path = path, Bytes = newBytes, Counter=0
            });
            SaveChanges();
        }

        public Tuple<BlockingCollection<string>, string[]> GetRecognizedAndUnrecognizedImages(string[] imagePaths)
        {
            var recognized_results = new BlockingCollection<string>();
            List<string> unrecognized_image_paths = new List<string>();
            foreach (var path in imagePaths)
            {
                RecognizedImage img = GetImageByPath(path);
                if (img != null)
                {
                    recognized_results.Add(img.Path + '\n' + img.Class + '$' + img.Counter.ToString());
                }
                else
                {
                    unrecognized_image_paths.Add(path);
                }
            }
            recognized_results.CompleteAdding();
            return Tuple.Create(recognized_results, unrecognized_image_paths.ToArray());
        }

        public void Clear()
        {
            RecognizedImages.RemoveRange(RecognizedImages);
            ImageBytesTable.RemoveRange(ImageBytesTable);
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

        private string ComputeMD5Checksum(byte [] fileData)
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

    }
}
