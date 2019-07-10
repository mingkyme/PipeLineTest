using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO.Pipes;
using System.IO;
using System.ComponentModel;

namespace PipeLineTest
{
    
    public partial class ServerTest : Window
    {
        static string Separator = "\r\n";
        NamedPipeServerStream pipeFromWPF;
        StreamReader reader;
        StreamWriter writer;
        BackgroundWorker connectBackgroundWorker;
        BackgroundWorker receiveBackgroundWorker;
        BackgroundWorker sendBackgroundWorker;
        public ServerTest()
        {
            InitializeComponent();
            pipeFromWPF = new NamedPipeServerStream("wpfPIPE");
            connectBackgroundWorker = new BackgroundWorker();
            receiveBackgroundWorker = new BackgroundWorker();
            sendBackgroundWorker = new BackgroundWorker();

            connectBackgroundWorker.DoWork += ConnectBackgroundWorker_DoWork;
            receiveBackgroundWorker.DoWork += ReceiveBackgroundWorker_DoWork;
            sendBackgroundWorker.DoWork += SendBackgroundWorker_DoWork;

            connectBackgroundWorker.RunWorkerAsync();
            receiveBackgroundWorker.RunWorkerAsync();
            sendBackgroundWorker.RunWorkerAsync();
        }
        // NamedPipeLine 연결
        private void ConnectBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                try
                {
                    if(!pipeFromWPF.IsConnected)
                    {
                        print("Try to connect");
                        pipeFromWPF.WaitForConnection();
                        print("Connection success");
                    }
                }
                catch(Exception ex)
                {
                    pipeFromWPF.Disconnect();
                    print(ex.ToString());
                }
            }
        }

        // NamedPipeLine 통해 받아서 receiveQueue에 넣기
        private void ReceiveBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                try
                {
                    if (pipeFromWPF.IsConnected)
                    {
                        reader = new StreamReader(pipeFromWPF);
                        List<char> list = new List<char>();
                        if (reader.Peek() > 0)
                        {
                            while (true)
                            {
                                var temp = reader.Read();
                                if (temp != -1)
                                {
                                    list.Add((char)temp);
                                }
                                else
                                {
                                    var fullString = new string(list.ToArray());
                                    print(fullString);
                                   
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    // 연결이 끊어졌을 때
                    connectBackgroundWorker.RunWorkerAsync();
                }
                catch
                {

                }
            }
        }

        // NamedPipeLine 통해 sendQueue의 내용 넣기
        private void SendBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                try
                {
                    if (pipeFromWPF.IsConnected)
                    {
                        if (sendQueue.Count > 0)
                        {
                            writer = new StreamWriter(pipeFromWPF);
                            if (pipeFromWPF.IsConnected)
                            {
                                var temp = sendQueue.Dequeue();
                                writer.Write(temp.ToString());
                                writer.Flush();

                                pipeFromWPF.WaitForPipeDrain();
                                pipeFromWPF.Dispose();
                                print("전송 성공");
                            }
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    connectBackgroundWorker.RunWorkerAsync();
                    // 연결이 끊어졌을 때
                }
                catch
                {

                }
            }
        }

        

        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
            pipeFromWPF.WaitForConnection();

            StreamReader reader = new StreamReader(pipeFromWPF);
            StreamWriter writer = new StreamWriter(pipeFromWPF);

            List<char> list = new List<char>();
            //TODO 비동기 변경
            while (true)
            {
                int temp = reader.Read();
                if(temp != -1)
                {
                    list.Add((char)temp);
                }
                else
                {
                    var fullString = new string(list.ToArray());
                    Console.WriteLine(fullString.Split(new string[] { Separator }, StringSplitOptions.None)[0] ); // COMMAND
                    Console.WriteLine(fullString.Split(new string[] { Separator }, StringSplitOptions.None)[1]); // MESSAGE

                    break;
                }
                
                
            }
            pipeFromWPF.Close();
        }

        private void print(string v)
        {
            Console.WriteLine(v);
        }
        Queue<CommunicationMessage> sendQueue = new Queue<CommunicationMessage>();
        Queue<CommunicationMessage> receiveQueue = new Queue<CommunicationMessage>();
        
        
        // receiveQueue가 들어오면 행동
        private void ActionReceiveQueue()
        {
            if(receiveQueue.Count > 0)
            {
                var temp = receiveQueue.Dequeue();
                print(temp.Message);
            }
        }
        // sendQueue에 넣기
        private void ActionSendQueue()
        { }
        // Unity <=> WPF 간의 통신 형식
        public class CommunicationMessage
        {
            public string command;
            public string Command
            {
                get
                {
                    return command;
                }
                set
                {
                    command = value;
                }
            }
            public string message;
            public string Message
            {
                get
                {
                    return message;
                }
                set
                {
                    message = value;
                }
            }

            public CommunicationMessage() { }
            public CommunicationMessage(string _Command, string _Message)
            {
                command = _Command;
                message = _Message;
            }
            public CommunicationMessage(byte[] buffer)
            {
                string rawString = Encoding.UTF8.GetString(buffer);
                command = rawString.Split(new string[] { Separator }, StringSplitOptions.None)[0];
                try
                {
                    message = rawString.Substring(command.Length + Separator.Length);
                }
                catch (Exception)
                {
                    // Message가 존재하지 않은 경우
                }

            }
            public byte[] ToByteArray()
            {
                return Encoding.UTF8.GetBytes(command + Environment.NewLine + Environment.NewLine + message);
            }

            override public string ToString()
            {
                return command + Environment.NewLine + message;
            }
        }
    }
}