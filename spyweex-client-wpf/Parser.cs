using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;


namespace spyweex_client_wpf
{
    public class Parser
    {
        private MemoryStream memStream;
        private Response response = new Response();

        public Response getResponse()
        {
            return response;
        }

        public Parser(ref MemoryStream memoryStream)
        {
            memStream = memoryStream;
        }

        public Response tryParse()
        {

            StreamReader sr = new StreamReader(memStream);
            memStream.Seek(0, SeekOrigin.Begin);
            var result = sr.ReadToEnd();
            string[] WholeSplittedResult = result.Split(new string[] { "\r\n\r\n" }, 2, StringSplitOptions.RemoveEmptyEntries);
            string ResponseMessageHeaders = WholeSplittedResult[0];
            TextReader textReader = new StringReader(ResponseMessageHeaders);

            string statusLine = textReader.ReadLine();
            string[] splittedStatusLine = statusLine.Split(new char[] { ' ' }, 4, StringSplitOptions.RemoveEmptyEntries);
            Int32.TryParse(splittedStatusLine[1], out response.StatusCode);
            response.Action = splittedStatusLine[3];

            string line = String.Empty;
            while ((line = textReader.ReadLine()) != null)
            {
                line = line.Trim();
                response.headers.Add(
                    line.Split(new char[] { ':', ' ' }, StringSplitOptions.RemoveEmptyEntries)[0],
                    line.Split(new char[] { ':', ' ' }, StringSplitOptions.RemoveEmptyEntries)[1]
                    );
            }

            byte[] bytes = System.Text.ASCIIEncoding.ASCII.GetBytes(WholeSplittedResult[0]);
            int bytesBeforeContent = bytes.Length + 4;  // 4 bytes of \r\n\r\n removed before...
            memStream.Seek(bytesBeforeContent, SeekOrigin.Begin);
            BinaryReader br = new BinaryReader(memStream);
            byte mybyte;
            byte[] byteArray = new byte[memStream.Length - bytesBeforeContent];
            int counter = 0;
            // reading the content in bytes from stream
            while (memStream.Position < br.BaseStream.Length)
            {
                mybyte = br.ReadByte();
                byteArray[counter++] = mybyte;
            }
            response.content = new byte[byteArray.Length];
            byteArray.CopyTo(response.content, 0);

            return response;
        }
    }
}
