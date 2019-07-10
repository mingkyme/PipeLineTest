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

namespace PipeLineTest
{
    public partial class ClientTest : Window
    {
        NamedPipeClientStream client;
        public ClientTest()
        {
            InitializeComponent();

            client = new NamedPipeClientStream("WPF Test");
            client.ConnectAsync();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            var text = "GoodJob";
            var buffer = Encoding.UTF8.GetBytes(text);
            client.Write(buffer, 0, buffer.Length);
        }
    }
}
