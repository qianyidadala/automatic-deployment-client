using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerMachineInfoShower_Client
{
    public class FileInfoEntity
    {
        public string LocalURL { get; set; }

        public string AbsolutionPath { get; set; }

        public string FileName { get; set; }

        public string FileSize { get; set; }
    }
}
