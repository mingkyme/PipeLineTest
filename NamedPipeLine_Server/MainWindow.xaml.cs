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

namespace NamedPipeLine_Server
{
    public partial class MainWindow : Window
    {
        NamedPipeServerStream pipeFromWPF;
        NamedPipeServerStream pipeFromUnity;

        BackgroundWorker receiveBackgroundWorker = new BackgroundWorker();
        public MainWindow()
        {
            InitializeComponent();
            pipeFromWPF = new NamedPipeServerStream("pipeFromWPF");
            pipeFromWPF.WaitForConnectionAsync();
            pipeFromUnity = new NamedPipeServerStream("pipeFromUnity");
            pipeFromUnity.WaitForConnectionAsync();

            receiveBackgroundWorker.DoWork += ReceiveBackgroundWorker_DoWork;
            receiveBackgroundWorker.RunWorkerAsync();
        }

        private void ReceiveBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (pipeFromUnity.IsConnected)
                {
                    int len = pipeFromUnity.ReadByte() * 256 * 256;
                    len += pipeFromUnity.ReadByte() * 256;
                    len += pipeFromUnity.ReadByte();
                    // 데이터의 길이 구하기
                    byte[] receiveByteArray = new byte[len];
                    pipeFromUnity.Read(receiveByteArray, 0, len);
                    Console.WriteLine(Encoding.UTF8.GetString(receiveByteArray));
                    MessageBox.Show(Encoding.UTF8.GetString(receiveByteArray));
                    pipeFromUnity.Flush();
                }
            }
        }

        private void ServerButton_Click(object sender, RoutedEventArgs e)
        {
            if (pipeFromWPF.IsConnected)
            {
                string msg = "Hello World! from WPF";
                byte[] sendMsg = Encoding.UTF8.GetBytes(msg);
                int len = sendMsg.Length;
                pipeFromWPF.WriteByte((byte)(len / 256 / 256));
                pipeFromWPF.WriteByte((byte)(len / 256));
                pipeFromWPF.WriteByte((byte)(len % 256));
                pipeFromWPF.Write(sendMsg, 0, len);
                pipeFromWPF.Flush();
            }
        }
    }
}
