﻿using CommandClient;
using Server.ResponseMessages;
using System;
using System.Collections.Generic;

namespace Server.ClientService
{
    class ManageClientFriend : ServerResponds, IPrepareRespond
    {
        public event EventHandler<ClientEventArgs> ClientAddFriend;
        public event EventHandler<ClientEventArgs> ClientDeleteFriend;

        DataBaseManager db = DataBaseManager.Instance;
        EmailSender emailSender = EmailSender.Instance;

        SendMessageToNick sendToNick;

        //The collection of all clients logged into the room
        private List<Client> ListOfClientsOnline;

        public void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelList = null)
        {
            Client = client;
            Received = receive;
            ListOfClientsOnline = clientList;
            Send.strName = Received.strMessage2;
            sendToNick = new SendMessageToNick(ListOfClientsOnline, Send);
        }

        public void Execute()
        {
            prepareResponse();
            string type = Received.strMessage;
            string friendName = Received.strMessage2;

            db.bind("FriendName", friendName);
            Int64 friend_id = Int64.Parse(db.singleSelect("SELECT id_user FROM users WHERE login = @FriendName"));

            if (friend_id > 0)
            {
                if (type == "Yes")
                {
                    bool InserClientToDb = AddFriendToDb(Client.id, friend_id); // client add friend in db
                    bool InserFriendToDb = AddFriendToDb(friend_id, Client.id); // friend add client in db

                    if (InserClientToDb && InserFriendToDb)
                    {
                        OnClientAddFriend(Client.strName, friendName);
                        Send.strMessage = "Yes";
                        Respond();
                        sendToNick.Send = Send;
                        sendToNick.Send.strMessage2 = Client.strName;
                        sendToNick.Send.strName = friendName;
                        sendToNick.ResponseToNick();
                    }
                    else
                    {
                        Send.strMessage = "No";
                        Send.strMessage2 = friendName;
                        Respond();
                    }
                }
                else if (type == "Delete")
                {
                    bool userDeleteFriend = DeleteFriendToDb(Client.id, friend_id);
                    bool friendDeleteUser = DeleteFriendToDb(friend_id, Client.id);
                    if (userDeleteFriend && friendDeleteUser)
                    {
                        // So user delete friend, friend delete user
                        // Need to send to user and friend list of friends
                        Send.strMessage = "Delete";
                        //Send.strMessage2 = friendName;
                        //SendMessageToSomeone sendToSomeone = new SendMessageToSomeone(ListOfClientsOnline, Send);
                        //sendToSomeone.ResponseToSomeone();

                        Respond();
                        sendToNick.Send = Send;
                        sendToNick.Send.strMessage2 = Client.strName;
                        sendToNick.Send.strName = friendName;
                        sendToNick.ResponseToNick();

                        //when client get delete then  he will send to server list ask
                        OnClientDeleteFriend(Client.strName, friendName);
                    }
                }
                else if (type == "Add")
                {
                    sendToNick.Send = Send;
                    sendToNick.Send.strMessage2 = Client.strName;
                    sendToNick.Send.strName = friendName;
                    sendToNick.ResponseToNick();
                }
                else if (type == "No")
                {
                    // Friend type no: he dont want be as friend
                    Send.strMessage = "No";
                    sendToNick.Send = Send;
                    sendToNick.Send.strMessage2 = Client.strName;
                    sendToNick.Send.strName = friendName;
                    sendToNick.ResponseToNick();
                }
            }
            else
            {
                Send.strMessage = "There is no friend that you want to add.";
                Respond();
            }
        }

        // Used in ManageUserFriend function
        private bool AddFriendToDb(Int64 clientId, Int64 friendId)
        {
            db.bind(new string[] { "idUser", clientId.ToString(), "idFriend", friendId.ToString() });

            if (db.delUpdateInsertDb("INSERT INTO user_friend (id_user, id_friend) " + "VALUES (@idUser, @idFriend)") > 0)
                return true;
            else
                return false;

        }
        // Used in ManageUserFriend function
        private bool DeleteFriendToDb(Int64 clientId, Int64 friendId)
        {
            db.bind(new string[] { "idUser", clientId.ToString(), "idFriend", friendId.ToString() });

            if (db.delUpdateInsertDb("DELETE FROM user_friend WHERE id_user = @idUser AND id_friend = @idFriend") > 0)
                return true;
            else
                return false;
        }

        protected virtual void OnClientAddFriend(string ClientName, string ClientFriendName)
        {
            ClientAddFriend?.Invoke(this, new ClientEventArgs() { clientName = ClientName, clientFriendName = ClientFriendName });
        }

        protected virtual void OnClientDeleteFriend(string ClientName, string ClientFriendName)
        {
            ClientDeleteFriend?.Invoke(this, new ClientEventArgs() { clientName = ClientName, clientFriendName = ClientFriendName });
        }
    }
}
