using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wpf
{
    public class BiffEventArgs : EventArgs
    {
        public string Path { get; set; }
        public BiffEventArgs(string path)
            : base()
        {
            Path = path;
        }
    }
}
