using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spyweex_client_wpf
{
    public class Response
    {

        public Response()
        {
            headers = new Dictionary<string, string>();
        }

        public int StatusCode;

        public int Tag;

        public string Action;

        public int WxhtpVersion;

        public Dictionary<string, string> headers;

        public byte[] content;

        public override string ToString()
        {
            return $"StatusCode={StatusCode}; Tag={Tag.ToString()}; Action={Action}; WxhtpVersion={WxhtpVersion.ToString()}; Headers={headers.Values.ToArray()}; Content={content}";
        }
    }
}
