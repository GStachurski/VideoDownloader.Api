using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VideoDownloader.Api.Models
{
    public class PartialDownload : Download
    {
        public string Location { get; set; }        
    }
}
