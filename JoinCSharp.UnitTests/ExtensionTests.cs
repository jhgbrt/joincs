using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JoinCSharp.UnitTests
{
    [TestClass]
    public class ExtensionTests
    {
        [TestMethod]
        public void IsBelow_FileInFolder_True()
        {
            var fileInfo = new FileInfo(@"C:\Users\Joe\tmp.txt");
            var root = new DirectoryInfo(@"C:\Users\Joe");
            Assert.IsTrue(fileInfo.SitsBelow(root));
        }
        [TestMethod]
        public void IsBelow_FileInFolderBelow_True()
        {
            var fileInfo = new FileInfo(@"C:\Users\Joe\tmp.txt");
            var root = new DirectoryInfo(@"C:\Users");
            Assert.IsTrue(fileInfo.SitsBelow(root));
        }
        [TestMethod]
        public void IsBelow_FileInRootFolderBelow_True()
        {
            var fileInfo = new FileInfo(@"C:\Users\Joe\tmp.txt");
            var root = new DirectoryInfo(@"C:\");
            Assert.IsTrue(fileInfo.SitsBelow(root));
        }
        [TestMethod]
        public void IsBelow_FileOtherFolderBelow_False()
        {
            var fileInfo = new FileInfo(@"C:\Users\Joe\tmp.txt");
            var root = new DirectoryInfo(@"C:\Users\Jane");
            Assert.IsFalse(fileInfo.SitsBelow(root));
        }

        [TestMethod]
        public void Except_FiltersFileInSubfolders()
        {
            var input = new[]
            {
                @"C:\A\AA\AAA.txt",
                @"C:\A\AB\AAB.txt",
                @"C:\A\AC\AAC.txt",
                @"C:\A\AD\AAD.txt",
                @"C:\A\AD\AAE.txt",
                @"C:\A\AE\AAF.txt",
            }.Select(s => new FileInfo(s));

            var rootDir = new DirectoryInfo(@"C:\A");
            var subdirs = new[] { "AB", "AD"};

            var result = input.Except(rootDir, subdirs);

            var expected = new[]
            {
                @"C:\A\AA\AAA.txt",
                @"C:\A\AC\AAC.txt",
                @"C:\A\AE\AAF.txt",
            };

            CollectionAssert.AreEqual(expected, result.Select(f => f.FullName).ToArray());
        }

        [TestMethod]
        public void WriteLine_WritesNameToTextWriter()
        {
            var fileInfos = new[] {@"C:\A\B\C.txt"}.Select(s => new FileInfo(s));
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                fileInfos = fileInfos.WriteLine(writer).ToList();
            }
            Assert.AreEqual(@"Processing: C:\A\B\C.txt" + Environment.NewLine, sb.ToString());
        }
    }
}