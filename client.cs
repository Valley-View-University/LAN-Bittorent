using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
public partial class FormClient : Form
{
    public FormClient()
    {
        InitializeComponent();
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        label1.Text = IPAddressCheck();
        label2.Text = GetFQDN();
        label3.Text = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

    }

    public void msg(string mesg)
    {
        lstProgress.Items.Add(">> " + mesg);
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


    private void button1_Click(object sender, EventArgs e)
    {
        string message = textBox1.Text;
    try
    {
        // Create a TcpClient.
        // Note, for this client to work you need to have a TcpServer 
        // connected to the same address as specified by the server, port
        // combination.
        Int32 port = 1337;
        string IPAddr = textBox2.Text;
        TcpClient client = new TcpClient(IPAddr, port); //Unsure of IP to use.

        // Translate the passed message into ASCII and store it as a Byte array.
        Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

        // Get a client stream for reading and writing.
        //  Stream stream = client.GetStream();

        NetworkStream stream = client.GetStream();

        // Send the message to the connected TcpServer. 
        stream.Write(data, 0, data.Length);

        //lstProgress.Items.Add(String.Format("Sent: {0}", message));

        // Receive the TcpServer.response.

        // Buffer to store the response bytes.
        data = new Byte[256];

        // String to store the response ASCII representation.
        String responseData = String.Empty;

        // Read the first batch of the TcpServer response bytes.
            Int32 bytes = stream.Read(data, 0, data.Length);
            responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
            if (message == "file")
            {
                lstProgress.Items.Add(String.Format("{0}", responseData));
                fPath.getPath = (String.Format("{0}", responseData));
                label4.Text = UNCPathing.GetUNCPath(fPath.getPath);
            }
            else if(message == "nama")
            {
                lstProgress.Items.Add(String.Format("{0}", responseData));
                fPath.getNama = (String.Format("{0}", responseData));
            }

        // Close everything.
        stream.Close();
        client.Close();
    }
    catch (ArgumentNullException an)
    {
        lstProgress.Items.Add(String.Format("ArgumentNullException: {0}", an));
    }
    catch (SocketException se)
    {
        lstProgress.Items.Add(String.Format("SocketException: {0}", se));
    }
    }

    private void button2_Click(object sender, EventArgs e)
    {
        using (NetworkShareAccesser.Access(GetFQDN(), IPAddressCheck(), "arif.hidayatullah28@gmail.com", "971364825135win8"))
        {

            File.Copy("\\\\"+label1.Text+"\\TestFolder\\"+fPath.getNama+"", @"D:\movie\"+ fPath.getNama+"", true);

        }
    }

    private void button3_Click(object sender, EventArgs e)
    {
        string connString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=\\ARIF-PC\aaaaaaaaaa\MsAccess.accdb; Jet OLEDB:Database Password=dbase;";

        string cmdText = "SELECT kode, deskripsi, stok, Harga FROM [Barang] ORDER BY kode DESC";

        OleDbConnection conn = new OleDbConnection(connString);
        OleDbCommand cmd = new OleDbCommand(cmdText, conn);

        try
        {
            conn.Open();

            cmd.CommandType = CommandType.Text;
            OleDbDataAdapter da = new OleDbDataAdapter(cmd);
            DataTable barang = new DataTable();
            da.Fill(barang);
            dataGridView1.DataSource = barang;
        }
        finally
        {
            conn.Close();
        }
    }
}
}

