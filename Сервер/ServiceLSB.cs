using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Server
{
    // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени класса "ServiceLSB" в коде и файле конфигурации.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ServiceLSB : IServiceLSB
    {
        List<ServerUser> users = new List<ServerUser>();
        int nextId = 1;
        public int Connect(string name)
        {
            ServerUser user = new ServerUser()
            {
                Id = nextId,
                Name = name,
                Context = OperationContext.Current
            };
            nextId++;
            users.Add(user);
            Console.WriteLine(DateTime.Now.ToShortTimeString() + ": Пользователь " + user.Name + " с id " + user.Id + " подключён!\n");

            return user.Id;
        }

        public void Disconnect(int id)
        {
            var user = users.FirstOrDefault(i => i.Id == id);
            if(user != null)
            {
                users.Remove(user);
            }
            Console.WriteLine(DateTime.Now.ToShortTimeString() + ": Пользователь " + user.Name + " с id " + user.Id + " отключён!\n");
        }

        public string SendImg(string msg, byte[] img, int from, int to)
        {
            var userFrom = users.FirstOrDefault(i => i.Id == from);
            var userTo = users.FirstOrDefault(i => i.Id == to);
            if (userTo != null && userFrom != null)
            {
                if (msg == "") msg = "Сообщение не указано.";
                string answer = DateTime.Now.ToShortTimeString() + ": Пользователь " + userFrom.Name + " отправил вам изображение с сообщением:\n\n" + msg + "\n\nПринять изображение?";

                Console.WriteLine(DateTime.Now.ToShortTimeString() + ": Пользователь " + userFrom.Name + " с id " + userFrom.Id + 
                    " отправил сообщение Пользователю " + userTo.Name + " с id " + userTo.Id + ":\n" + msg + "\n");
                userTo.Context.GetCallbackChannel<IServiceLSBCallback>().ImgCallback(answer, img);
                return "Сообщение отправлено!";
            }
            else {
                return "Пользователь с id " + to.ToString() + " не существует";
            }
        }
    }
}
