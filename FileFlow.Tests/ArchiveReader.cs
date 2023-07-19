using SharpCompress.Archives;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileFlow.Tests
{
    internal class ArchiveReader
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void RarTest()
        {
            string path = "C:/Tests/1/Soundpad-3.3.2.0-Rus-Repack.rar";
            
            using(var stream = File.OpenRead(path))
            {
                using(var archive = ArchiveFactory.Open(stream))
                {
                    foreach (IArchiveEntry item in archive.Entries)
                    {
                        Trace.WriteLine(item.Key);
                    }
                }
                //using(var reader = ReaderFactory.Open(stream))
                //{
                //    while (reader.MoveToNextEntry())
                //    {
                //        Trace.WriteLine(reader.Entry.Key);
                //    }
                //}
            }
        }
    }
}
