using System.Runtime.Serialization;
using System.ServiceModel;

namespace Server
{
    [DataContract]
    public class ServerUser
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public OperationContext Context { get; set; }
    }
}
