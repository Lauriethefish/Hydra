﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using TestUtils;

namespace Hydra.Http11.Tests
{
    [TestClass]
    public class HttpWriterTests
    {
        private readonly MemoryStream stream;
        private readonly HttpWriter writer;

        public HttpWriterTests()
        {
            stream = new();
            writer = new(PipeWriter.Create(stream));
        }

        [TestMethod]
        public async Task Write()
        {
            string body = "{ \"name\": \"Hydra\" }";
            writer.WriteStatusLine(new(200, "OK"));
            writer.WriteHeader(new("Content-Type", "application/json; charset=utf-8"));
            writer.WriteHeader(new("Content-Length", body.Length.ToString()));
            await writer.Send(body.AsStream());

            string expected =
                "HTTP/1.1 200 OK\r\n" +
                "Content-Type: application/json; charset=utf-8\r\n" +
                $"Content-Length: {body.Length}\r\n" +
                "\r\n" + body;

            stream.Position = 0;
            Assert.AreEqual(expected, stream.AsText(Encoding.UTF8));
        }
    }
}