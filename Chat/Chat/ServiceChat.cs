using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;


namespace Chat
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ServiceChat : IServiceChat
    {
        List<ServerUser> users = new List<ServerUser>();

        static int nextId = 1;

        public int Connect(string name)
        {
            ServerUser user = new ServerUser()
            {
                Id = nextId,
                Name = name,
                operationContext = OperationContext.Current
            };
            nextId++;
            SendMssage(user.Name + ": подключился к чату", nextId);
            users.Add(user);

            return user.Id;
        }

        public void Disconnect(int id)
        {
            var user = users.FirstOrDefault(x => x.Id == id);
            if (user != null)
            {
                users.Remove(user);
                SendMssage(user.Name + ": покинул чату", 0);
            }
        }

        public void SendMssage(string message, int id)
        {
            if (users.Count > 0)
            {
                string answer = "(" + DateTime.Now.ToShortTimeString() + ") ";

                var userSend = users.FirstOrDefault(x => x.Id == id);

                if (userSend != null)
                {
                    answer += userSend.Name + ": ";
                }

                answer += message;

                foreach (var user in users)
                {
                    user.operationContext.GetCallbackChannel<IServerChatCallback>().MessageCollback(answer);
                }
            }
        }
    }
}
