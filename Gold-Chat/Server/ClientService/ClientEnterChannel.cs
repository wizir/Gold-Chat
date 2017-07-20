﻿using CommandClient;
using Server.ResponseMessages;
using System;
using System.Collections.Generic;

namespace Server.ClientService
{
    class ClientEnterChannel : ServerResponds, IPrepareRespond
    {
        //list of all channels
        private List<Channel> Channels;
        List<Client> ListOfClientsOnline;
        bool IsUserEnter = false;
        string ChannelName;

        DataBaseManager db = DataBaseManager.Instance;

        public void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelList = null)
        {
            Client = client;
            Received = receive;
            Channels = channelList;
            ListOfClientsOnline = clientList;
        }

        public void Execute()
        {
            prepareResponse();
            ChannelName = Received.strMessage;

            db.bind(new string[] { "channelName", ChannelName, "idUser", Client.id.ToString() });
            db.manySelect("SELECT uc.id_channel, c.welcome_message FROM channel c, user_channel uc WHERE uc.id_channel = c.id_channel AND c.channel_name = @channelName AND uc.id_user = @idUser");

            string[] respond = db.tableToRow();
            if (respond != null)
            {
                int id_channel_db = Int32.Parse(respond[0]);
                string motd = respond[1];

                Channel channel = ChannelGets.getChannelByName(Channels, ChannelName);

                if (channel != null)
                {
                    if (!channel.Users.Contains(Received.strName) && (!Client.enterChannels.Contains(ChannelName)))
                    {
                        channel.Users.Add(Received.strName);
                        Client.enterChannels.Add(ChannelName);

                        Send.strMessage2 = "enter";
                        Send.strMessage3 = motd;
                        IsUserEnter = true;
                    }
                    else
                    {
                        userWontJoinToServer("Cannot enter Because you already entered to ");
                    }
                }

                //OnClientEnterChannel(channelName, client.strName); //todo
            }
            else
            {
                userWontJoinToServer("Cannot Enter Because you not join to ");
            }
        }

        private void userWontJoinToServer(string serverMessage)
        {
            Send.strMessage2 = "deny";
            Send.strMessage3 = serverMessage + ChannelName;
            Respond();
        }

        public override void Respond()
        {
            if (IsUserEnter)
            {
                SendMessageToChannel sendToChannel = new SendMessageToChannel(Send, ListOfClientsOnline, ChannelName);
                sendToChannel.ResponseToChannel();
            }
            else
                base.Respond();
        }
    }
}
