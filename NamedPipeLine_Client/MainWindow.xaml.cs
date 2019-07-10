using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Pipes;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NamedPipeLine_Client
{
    public partial class MainWindow : Window
    {
        NamedPipeClientStream pipeFromWPF;
        NamedPipeClientStream pipeFromUnity;

        BackgroundWorker receiveBackgroundWorker = new BackgroundWorker();
        public MainWindow()
        {
            InitializeComponent();
            pipeFromWPF = new NamedPipeClientStream("pipeFromWPF");
            pipeFromWPF.ConnectAsync();
            pipeFromUnity = new NamedPipeClientStream("pipeFromUnity");
            pipeFromUnity.ConnectAsync();
            receiveBackgroundWorker.DoWork += receiveBackgroundWorker_DoWork;
            receiveBackgroundWorker.RunWorkerAsync();

        }

        private void receiveBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while(true)
            {
                if (pipeFromWPF.IsConnected)
                {
                    int len = pipeFromWPF.ReadByte() * 256 * 256;
                    len += pipeFromWPF.ReadByte() * 256;
                    len += pipeFromWPF.ReadByte();
                    // 데이터의 길이 구하기
                    byte[] receiveByteArray = new byte[len];
                    pipeFromWPF.Read(receiveByteArray, 0, len);
                    Console.WriteLine(Encoding.UTF8.GetString(receiveByteArray));
                    MessageBox.Show(Encoding.UTF8.GetString(receiveByteArray));
                    pipeFromWPF.Flush();
                }
            }
        }

        private void ClientButton_Click(object sender, RoutedEventArgs e)
        {
            if (pipeFromUnity.IsConnected)
            {
                string msg = "Hello World! from Unity";
                byte[] sendMsg = Encoding.UTF8.GetBytes(msg);
                int len = sendMsg.Length;
                pipeFromUnity.WriteByte((byte)(len / 256 / 256));
                pipeFromUnity.WriteByte((byte)(len / 256));
                pipeFromUnity.WriteByte((byte)(len % 256));
                pipeFromUnity.Write(sendMsg, 0, len);
                pipeFromUnity.Flush();
            }
        }
    }
}
