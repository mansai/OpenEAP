using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenEAP.Model
{
    public sealed class ImageDownloadProgressInfo
    {
        public double Percent { get; set; }

        public int ProcessedCount { get; set; }

        public int TotalCount { get; set; }
    }
}
