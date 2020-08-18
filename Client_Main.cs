using Module;
using System;
using System.Data.SqlClient;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ClientChat
{
    public partial class Client_Main : Form
    {
        Socket client_socket;
        string data;
        Thread receive_msg;
        public static string strDBCon;

        public Client_Main()
        {
            InitializeComponent();
        }

        private void Client_Main_Load(object sender, EventArgs e)
        {
            try
            {
                client_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client_socket.Connect(new IPEndPoint(IPAddress.Parse("192.168.10.207"), 7777));
                listBox1.Items.Add(String.Format("{0} 서버에 연결되었습니다.", client_socket.RemoteEndPoint.ToString()));


                receive_msg = new Thread(Receive_MSg);
                receive_msg.Start();
            }
            catch(Exception ex)
            {
                listBox1.Items.Add(ex.Message);
            }
        }

        private void Receive_MSg()
        {
            try
            {
                while (true)
                {
                    byte[] bytes = new byte[1024];

                    int bytesRec = client_socket.Receive(bytes);


                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);

                    if (data.IndexOf("<EOF>") == -1)
                    {
                        break;
                    }
                    else
                    {
                        data = data.Replace("<EOF>", "");
                        add_ListBox("[받은 메세지]" + data);
                        data = "";
                    }
                }
            }
            catch (Exception Ex)
            {
                if(client_socket.Connected == false)
                {
                    MessageBox.Show("서버 연결이 끊어졌습니다.");
                }
                else
                {
                    MessageBox.Show("접속종료되었습니다.");
                }
                
            }
        }

        private void btn_Send_Click(object sender, EventArgs e)
        {
            byte[] bytes = new byte[1024];
            byte[] msg = Encoding.ASCII.GetBytes(String.Format("{0}<EOF>", txt_Msg.Text));
            if(client_socket.Connected == false)
            {
                MessageBox.Show("서버에 연결되지 않았습니다.");
                txt_Msg.Clear();
                listBox1.Focus();
            }
            else
            {
                int byteSent = client_socket.Send(msg);
                listBox1.Items.Add("[보낸메세지]" + txt_Msg.Text);
                Chat_Log_Insert(CM_Main.GUBUN, CM_Main.S_ID, txt_Msg.Text);
                txt_Msg.Clear();
            }
            
        }        

        /// <summary>
        /// Thread(Background)에서 ListBox1에 메세지 띄울 때 ListBox위치 선택(띄울 메세지 추가) 
        /// </summary>
        /// <param name="temp"></param>
        private void add_ListBox(string temp)
        {
            try
            {
                if (listBox1.InvokeRequired)
                {
                    listBox1.Invoke((MethodInvoker)delegate ()
                    {
                        listBox1.Items.Add(temp);
                    });
                }
                else
                {
                    listBox1.Items.Add(temp);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void Chat_Log_Insert(string gubun, string id, string text)
        {
            SqlConnection conn = new SqlConnection(strDBCon);
            try
            {
                SqlCommand sqlCommand = new SqlCommand(strDBCon);
                sqlCommand.Connection = conn;
                sqlCommand.CommandText = "insert into ChatLog (Gubun, UserID, Text, DateTime) values (@param1, @param2, @param3, getdate())";
                sqlCommand.Parameters.AddWithValue("@param1", gubun);
                sqlCommand.Parameters.AddWithValue("@param2", id);
                sqlCommand.Parameters.AddWithValue("@param3", text);
                conn.Open();
                sqlCommand.ExecuteNonQuery();
                conn.Close();
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        /// <summary>
        /// 보낼 메세지 입력하고 엔터키 눌렀을 때 전송버튼 클릭 이벤트 발생
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txt_Msg_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btn_Send_Click(sender, e);
            }
        }

        private void Client_Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            client_socket.Close();
            Application.Exit();
        }
    }
}
