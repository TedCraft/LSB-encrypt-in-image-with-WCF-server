using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Drawing;
using System.Text;

namespace Server
{
    // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени интерфейса "IServiceLSB" в коде и файле конфигурации.
    [ServiceContract(CallbackContract = typeof(IServiceLSBCallback))]
    public interface IServiceLSB
    {
        [OperationContract]
        int Connect(string name);

        [OperationContract]
        void Disconnect(int id);

        [OperationContract]
        string SendImg(string msg, byte[] img, int from, int to);
    }

    [ServiceContract]
    public interface IServiceLSBCallback
    {
        [OperationContract(IsOneWay = true)]
        void ImgCallback(string msg, byte[] image);
    }
}
