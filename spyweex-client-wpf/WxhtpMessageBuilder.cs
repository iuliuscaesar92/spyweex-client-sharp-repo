using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using spyweex_client_wpf.StaticStrings;

namespace spyweex_client_wpf
{
    internal enum StatusType
    {
        Ok = 200,
        Created = 201,
        Accepted = 202,
        NoContent = 204,
        MultipleChoices = 300,
        NotModified = 304,
        BadRequest = 400,
        Forbidden = 403,
        NotFound = 404,
        InternalServerError = 500,
        NotImplemented = 501,
        BadGateway = 502,
        ServiceUnavailable = 503
    };

    public class WxhtpMessageBuilder
    {
        private string _content = null;
        private string _remoteEndpoint = null;
        private string _method_type = null;
        private string _action_type = null;
        private List<string> _params = new List<string>();


        public WxhtpMessageBuilder(params string[] parameters)
        {
            _remoteEndpoint = parameters[0];
            _method_type = parameters[1];
            _action_type = parameters[2];
            foreach (var s in parameters.ToList().GetRange(3, parameters.Length - 3))
            {
                _params.Add(s.StartsWith("--") ? s.Remove(0, 2) : s);
            }
            build();
        }

        private void build()
        {
            buildFirstLine();
            buildHeaders();
            buildBody();
        }

        private void buildFirstLine()
        {
            _content += _method_type;
            _content += DELIMITERS.SPACE;
            _content += _action_type;
            _content += DELIMITERS.SPACE;
            _content += VERSION.SPY_VERSION;
            _content += DELIMITERS.NEWLINE;
        }

        private void buildHeaders()
        {
            _content += HEADER_TYPES.HOST;
            _content += DELIMITERS.SPACE;
            _content += _remoteEndpoint;
            _content += DELIMITERS.NEWLINE;
            _content += HEADER_TYPES.TAG;
            _content += DELIMITERS.SPACE;
            _content += generateTag().ToString();
            _content += DELIMITERS.NEWLINE;
            _content += HEADER_TYPES.USER_AGENT;
            _content += DELIMITERS.SPACE;
            _content += VERSION.STANDARD_AGENT;
            _content += DELIMITERS.NEWLINE;
            _content += HEADER_TYPES.CONTENT_TYPE;
            _content += DELIMITERS.SPACE;
            _content += "text/plain";
            _content += DELIMITERS.NEWLINE;
            _content += HEADER_TYPES.CONTENT_LENGTH;
            _content += DELIMITERS.SPACE;
            _content += string.Join("&", _params).Length.ToString();
            _content += DELIMITERS.DOUBLE_NEWLINE;
        }

        private void buildBody()
        {
            _content += string.Join("%%", _params);
        }

        public string getContent()
        {
            return _content;
        }

        public static int generateTag()
        {
            Random x = new Random();
            return x.Next(100000, 999999);
        }

    }
}
