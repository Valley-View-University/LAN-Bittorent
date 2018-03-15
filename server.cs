using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServerClientProject
{
public partial class FormServer : Form
{
    private static Int32 port;
    private static string filePath;
    TcpListener server = new TcpListener(IPAddress.Any, port);

    public FormServer()
    {
        InitializeComponent();
    }

    private void FormServer_Load(object sender, EventArgs e)
    {
        File.WriteAllText("path.misc","");
        File.WriteAllText("nama.misc","");

        filePath = readPath();
        label1.Text = filePath;
        label2.Text = readNama();
        label3.Text = IPAddressCheck();
        label4.Text = UNCPathing.GetUNCPath(readPath());
        label5.Text = GetFQDN();

        bw.WorkerSupportsCancellation = true;
        bw.WorkerReportsProgress = true;
        bw.DoWork += new DoWorkEventHandler(bw_DoWork);
        bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);

        if (bw.IsBusy != true)
        {
            bw.RunWorkerAsync();
        }
    }

    private static string IPAddressCheck()
    {
        IPHostEntry IPAddr;
        IPAddr = Dns.GetHostEntry(GetFQDN());
        IPAddress ipString = null;

        foreach (IPAddress ip in IPAddr.AddressList)
        {
            if (IPAddress.TryParse(ip.ToString(), out ipString) && ip.AddressFamily == AddressFamily.InterNetwork)
            {
                break;
            }
        }
        return ipString.ToString();
    }

    public static string GetFQDN()
    {
        string domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
        string hostName = Dns.GetHostName();

        if (!hostName.EndsWith(domainName))  // if hostname does not already include domain name
        {
            hostName += "." + domainName;   // add the domain name part
        }

        return hostName;                    // return the fully qualified name
    }

    private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
        lstProgress.Items.Add(e.UserState);
    }

    private void bw_DoWork(object sender, DoWorkEventArgs e)
    {
        BackgroundWorker worker = sender as BackgroundWorker;

        if ((worker.CancellationPending == true))
        {
            e.Cancel = true;
        }
        else
        {
            try
            {
                // Set the TcpListener on port 1333.
                port = 1337;
                //IPAddress localAddr = IPAddress.Parse("127.0.0.1");
                TcpListener server = new TcpListener(IPAddress.Any, port);

                //label2.Text = IPAddressToString(ip);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String data = null;

                // Enter the listening loop.
                while (true)
                {
                    bw.ReportProgress(0, "Waiting for a connection... ");
                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();
                    bw.ReportProgress(0, "Connected!");

                    data = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        bw.ReportProgress(0, String.Format("Received: {0}", data));

                        if (data == "file")
                        {

                        // Process the data sent by the client.

                            data = String.Format("Request: {0}", data);


                            byte[] mssg = System.Text.Encoding.ASCII.GetBytes(label4.Text);

                            // Send back a response.
                            stream.Write(mssg, 0, mssg.Length);
                            bw.ReportProgress(0, String.Format("Sent: {0}", data));
                            bw.ReportProgress(0, String.Format("File path : {0}", label4.Text));
                        }
                        else if (data == "nama")
                        {
                            byte[] mssg = System.Text.Encoding.ASCII.GetBytes(readNama());
                            stream.Write(mssg, 0, mssg.Length);
                        }
                        else if (data == "ip")
                        {
                            byte[] mssg = System.Text.Encoding.ASCII.GetBytes(GetFQDN());
                            stream.Write(mssg, 0, mssg.Length);
                        }
                    }

                    // Shutdown and end connection
                    client.Close();
                }
            }
            catch (SocketException se)
            {
                bw.ReportProgress(0, String.Format("SocketException: {0}", se));
            }
        }
    }

    private void FormServer_FormClosing(object sender, FormClosingEventArgs e)
    {

    }

    private void button1_Click(object sender, EventArgs e)
    {
        openFileDialog1.ShowDialog();
        savePath(openFileDialog1.FileName.ToString());
        saveNama(openFileDialog1.SafeFileName.ToString());

        //folderBrowserDialog1.ShowDialog();
        //savePath(folderBrowserDialog1.SelectedPath.ToString());

        label1.Text = readPath();
        label2.Text = readNama();
        label4.Text = UNCPathing.GetUNCPath(readPath());
    }

    private void savePath(string fPath)
    {
        string sPath = "Path.misc";

        File.WriteAllText(sPath, fPath);
    }

    private string readPath()
    {
        string readText = File.ReadAllText("Path.misc");
        return readText;
    }

    private void saveNama(string nama)
    {
        string sNama = "Nama.misc";

        File.WriteAllText(sNama, nama);
    }

    private string readNama()
    {
        string readText = File.ReadAllText("Nama.misc");
        return readText;
    }
}
}

a