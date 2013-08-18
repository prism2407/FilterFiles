using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCompare
{
    public class FileSearchCriteria
    {
        public string Directory
        {
            get;
            set;
        }

        public string FileCardExtensions
        {
            get;
            set;
        }

        public DateTime? DateStart
        {
            get;
            set;
        }

        public string SkipSubDirectories
        {
            get;
            set;
        }
    }
}
