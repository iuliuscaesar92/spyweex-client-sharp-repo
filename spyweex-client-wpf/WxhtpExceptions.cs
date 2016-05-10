using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spyweex_client_wpf
{
    internal class WxhtpExceptions
    {

    }

    [Serializable()]
    internal class EmptyCollectionException : Exception
    {
        public EmptyCollectionException() : base() { }
        public EmptyCollectionException(string message) : base(message) { }
        public EmptyCollectionException(string message, System.Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client. 
        protected EmptyCollectionException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
        { }
    }
}
