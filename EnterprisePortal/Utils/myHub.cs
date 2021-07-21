using EnterprisePortal.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace EnterprisePortal.Utils
{
    [HubName("chathub")]
    public class MyHub : Hub
    {
        private readonly PortalModel _db = new PortalModel();

        public void Notify(string message, int msgId)
        {
            Clients.OthersInGroup(msgId.ToString()).notify(message);
        }

        public void Welcome(string message)
        {
            Clients.Caller.welcomeMsg(message);
        }

        public void Join(int msgId)
        {
            Groups.Add(Context.ConnectionId, msgId.ToString());
        }

        public void SendMsg(string message, int userId, int msgId)
        {
            DateTime msgTime = DateTime.Now;
            Clients.Group(msgId.ToString()).sendMsgBack(message, userId, msgTime.ToString("HH:mm"));

            Message newMessage = new Message
            {
                MsgListId = msgId,
                SenderId = userId,
                Text = message,
                Time = msgTime
            };
            _db.Messages.Add(newMessage);
            _db.SaveChanges();
        }
    }
}