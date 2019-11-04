using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Linq;

namespace JoinCSharp.UnitTests
{
    [TestClass]
    public class ArgsTests
    {
        [TestMethod]
        public void Args_OnlyInputDirectory_WhenExists_InputDirectoryIsSet()
        {
            var args = new Args(new[] { Environment.CurrentDirectory });
            Assert.AreEqual(Environment.CurrentDirectory, args.InputDirectory);
            Assert.IsTrue(string.IsNullOrEmpty(args.OutputFile));
        }
        [TestMethod]
        public void Args_OnlyInputDirectory_WhenExists_ErrorsIsEmpty()
        {
            var args = new Args(new[] { Environment.CurrentDirectory });
            Assert.IsFalse(args.Errors.Any());
        }
        [TestMethod]
        public void Args_OnlyInputDirectory_WhenNotExists_ErrorIsSet()
        {
            var args = new Args(new[] { "NonExistingDirectory" });
            Assert.AreEqual(1, args.Errors.Count());
        }
        [TestMethod]
        public void Args_InputDirectoryAndOutputFile_WhenExists_InputDirectoryIsSet()
        {
            var args = new Args(new[] { Environment.CurrentDirectory, "somefile.cs" });
            Assert.AreEqual(Environment.CurrentDirectory, args.InputDirectory);
            Assert.AreEqual("somefile.cs", args.OutputFile);
        }
        [TestMethod]
        public void Args_InputDirectoryAndOutputFile_WhenFileIsNotCsFile_Error()
        {
            var args = new Args(new[] { Environment.CurrentDirectory, "somefile.blah" });
            Assert.AreEqual(1, args.Errors.Count);
        }

        [TestMethod]
        public void Args_Preprocessor()
        {
            var args = new Args(new[] { Environment.CurrentDirectory, "somefile.cs", "DEBUG,NETFRAMEWORK" });
            Assert.AreEqual(0, args.Errors.Count);
            CollectionAssert.AreEqual(new[] { "DEBUG", "NETFRAMEWORK" }, args.PreprocessorDirectives);
        }
        [TestMethod]
        public void Args_DifferentOrder1()
        {
            var args = new Args(new[] { Environment.CurrentDirectory, "DEBUG,NETFRAMEWORK", "somefile.cs" });
            Assert.AreEqual(0, args.Errors.Count);
            CollectionAssert.AreEqual(new[] { "DEBUG", "NETFRAMEWORK" }, args.PreprocessorDirectives);
            Assert.AreEqual("somefile.cs", args.OutputFile);
            Assert.AreEqual(Environment.CurrentDirectory, args.InputDirectory);
        }
        [TestMethod]
        public void Args_DifferentOrder2()
        {
            var args = new Args(new[] { "DEBUG,NETFRAMEWORK", Environment.CurrentDirectory, "somefile.cs" });
            Assert.AreEqual(0, args.Errors.Count);
            CollectionAssert.AreEqual(new[] { "DEBUG", "NETFRAMEWORK" }, args.PreprocessorDirectives);
            Assert.AreEqual("somefile.cs", args.OutputFile);
            Assert.AreEqual(Environment.CurrentDirectory, args.InputDirectory);
        }
        [TestMethod]
        public void Args_DifferentOrder3()
        {
            var args = new Args(new[] { "DEBUG,NETFRAMEWORK", "somefile.cs", Environment.CurrentDirectory });
            Assert.AreEqual(0, args.Errors.Count);
            CollectionAssert.AreEqual(new[] { "DEBUG", "NETFRAMEWORK" }, args.PreprocessorDirectives);
            Assert.AreEqual("somefile.cs", args.OutputFile);
            Assert.AreEqual(Environment.CurrentDirectory, args.InputDirectory);
        }
        [TestMethod]
        public void Args_IgnoreAssemblyAttributes()
        {
            var args = new Args(new[] { "DEBUG,NETFRAMEWORK", "somefile.cs", Environment.CurrentDirectory, "--ignoreAssemblyAttributes" });
            Assert.AreEqual(0, args.Errors.Count);
            Assert.IsTrue(args.IgnoreAssemblyAttributes);
        }
    }
}
