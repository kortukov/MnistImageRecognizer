using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RecognizerDbConnection
{
    public class RecognizedImage
    {
        [Key]
        public int Id { get; set; }
        public string Class { get; set; }
        public byte[] Bytes { get; set; }
        public string Checksum { get; set; }
        public int Counter { get; set; }

    }

    //public class ImageBytes
    //{
    //    [Key]
    //    public int Id { get; set; }
    //    public byte[] ImageBlob { get; set; }
    //}
}
