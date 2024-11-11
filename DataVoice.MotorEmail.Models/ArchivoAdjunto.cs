using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataVoice.MotorEmail.Models
{
    [Serializable]
    public class ArchivoAdjunto
    {
        public Stream FileContent { get; set; }
        public string FileName { get; set; }
        public string FileFullName { get; set; }
    }
}
