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
    [HubName("chatHub")]
    public class myHub : Hub
    {
        private readonly PortalModel _db = new PortalModel();

        public void GetServerDateTime()
        {
            Clients.Caller.DisplayDateTime();
        }

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
            Clients.Group(msgId.ToString()).sendMsgBack(message, userId, msgId);

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

        public void Announce(string message)
        {
            Clients.All.Announce(message);
        }

        public void Broadcast(string name, string message)
        {
            Clients.All.showmessage(name, message);
        }

        //public string GetServerDateTime()
        //{
        //    return "於" + DateTime.Now.ToString("MM-dd hh:mm") + "送出。";
        //}

        public override Task OnConnected()
        {
            // Add your own code here.
            // For example: in a chat application, record the association between
            // the current connection ID and user name, and mark the user as online.
            // After the code in this method completes, the client is informed that
            // the connection is established; for example, in a JavaScript client,
            // the start().done callback is executed.

            //string name = Context.User.Identity.Name;
            //Groups.Add(Context.ConnectionId, name);
            Clients.All.Announce("有人上線了。");
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            // Add your own code here.
            // For example: in a chat application, mark the user as offline,
            // delete the association between the current connection id and user name.
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            // Add your own code here.
            // For example: in a chat application, you might have marked the
            // user as offline after a period of inactivity; in that case
            // mark the user as online again.
            return base.OnReconnected();
        }
    }
}