﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using WPWebSockets.Common;
using WPWebSockets.Server.WebSocket;

namespace WPWebSocketsCmd.Server
{
    /// <summary>
    /// the class that handles the client messages inbound.
    /// </summary>
    class WPSWebSocketService : WebSocketService
    {
        private readonly IWebSocketLogger _logger;
        
        public WPSWebSocketService(Stream stream, TcpClient tcpClient, string header, IWebSocketLogger logger,Int64 _uuid)
            : base(stream, tcpClient, header, true, logger,_uuid)
        {
            _logger = logger;
        }
        protected string ParseJSONFrame(string jsonString)
        {
            string value = "";
            value = WorkPackageCmd.ProcessJSON(jsonString);
            return value;
        }
        /// <summary>
        /// A simple method to parse the string for a specific pattern.
        /// it is currently only looking for OPEN, SELECT, KEYIN, and RD
        /// For specific WorkPackage commands they prefix with WPS then use
        /// ADD listName JSON array of data
        /// GET listName keyProperty.  this will  be sent out to the clients.
        /// </summary>
        /// <param name="unparsed"></param>
        protected string ParseTextFrame(string unparsed)
        {
            string response = "SUCCESS";
            string[] args = unparsed.Split('>');
            try
            {
                int i = 0;
               // for (int i = 0; i < args.Length; ++i)
                    switch (args[i].ToUpper())
                    {
                        case "USTN":
                            switch (args[i + 1].ToUpper())
                            {
                                case "OPEN":
                                    WorkPackageCmd.OpenFileCmd(args[i + 2], args[i + 3], false);
                                    break;
                                case "SELECT":
                                    WorkPackageCmd.SelectElementById(args[i + 2]);
                                    break;
                                case "KEYIN":
                                    WorkPackageCmd.SendKeyin(args[i + 2]);
                                    break;
                                case "RD":
                                    WorkPackageCmd.rdEquals(args[i + 2]);
                                    break;
                            }
                           break;

                        case "WPS":
                            switch (args[i + 1].ToUpper())
                            {
                                case "ADD":
                                    WorkPackageCmd.ProcessElementList(args[i + 2], args[i + 3], this);
                                    break;
                                case "GET":
                                    WorkPackageCmd.GetElementList(args[i + 2], args[i + 3]);
                                    break;
                                case "WORK":
                                    WorkPackageCmd.OpenForWork(args[i + 2]);
                                    break;
                                case "SHOW":
                                    // WorkPackageCmd.SendKeyin("itemset activate " + args[i + 1]);
                                    WorkPackageCmd.SendKeyin("itemset isolate " + args[i + 1]);
                                    break;
                                case "FILE":
                                    Send("Getting  File... ");
                                    response = WorkPackageCmd.GetFileFromWSGByGUID(args[i + 2], this, "");
                                    break;
                                case "MOVE":
                                    WorkPackageCmd.MoveToGroup(args[i + 2], args[i + 3], args[1 + 4], args[i + 5]);
                                    break;
                                case "REG":
                                    WorkPackageCmd.RegConnection(args[i + 2]);
                                    m_uuid = Int64.Parse(args[i + 2]);
                                    base.m_uuid = m_uuid;
                                    break;
                                case "JSON":
                                    WorkPackageCmd.ProcessIWPJSON(args[i + 2], this);
                                    break;
                                case "LOAD":
                                    if(args[i+2].Length>0)
                                        WorkPackageCmd.LoadIWPCommand(args[i + 2]);
                                    break;
                                case "QGET":
                                    string msg = WorkPackageCmd.GetFromMessageQueue();
                                    if(msg.Length>0)
                                        ParseTextFrame(msg);
                                    break;
                                case "QPUT":
                                    if(args[1+2].Length>0)
                                        WorkPackageCmd.AddToMessageQueue(args[i + 2]);
                                    break;
                                case "EXIT":
                                    WorkPackageCmd.CloseHost();
                                    break;
                                default:
                                    break;
                            }
                            break;

                        default:
                            {
                                response = unparsed;
                                break;
                            }
                    }
            }
            catch (Exception e)
            {
                Console.WriteLine("DEBUG EXCEPTION: " + e.Message);
            }

            return response;
        }
 /// <summary>
 /// this is the received message.  It should decode the message
 /// and reply.
 /// </summary>
 /// <param name="text"></param>
        protected override void OnTextFrame(string text)
        {
            //Send("message received");
            _logger.Information(this.GetType(), text);
            string response = "Server Response: " + ParseTextFrame(text);
            //base.Send(response);//send back an ack?
            List<WPWebSockets.Server.IService> conns = ServiceFactory.GetClients();
            
            base.Broadcast(response,conns,this);
          
           //base.Send(text);
            _logger.Information(this.GetType(),"", response);
            //parse the "command" could  be a json?
        }
        protected override void OnJSONFrame(string json)
        {
            string response = ParseJSONFrame(json);
        }
        /// <summary>
        /// override the dispose method.  cleans out the connection list.
        /// </summary>
        public override void Dispose()
        {
            List<WPWebSockets.Server.IService> conns = ServiceFactory.GetClients();

            conns.Remove(this);
            
            //base.Dispose();
        }
    }
}

