using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ImageDbConnection
{
    public class RecognizedImage
    {
        [Key]
        public int Id { get; set; }
        public string Path { get; set; }
        public string Class { get; set; }
        public virtual ImageBytes Bytes { get; set; }
        public string Checksum { get; set; }
        public int Counter { get; set; }

    }

    public class ImageBytes
    {
        [Key]
        public int Id { get; set; }
        public byte[] ImageBlob { get; set; }
    }

}
