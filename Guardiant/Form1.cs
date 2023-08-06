using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace Guardiant
{
    public partial class Form1 : Form
    {
        public static string loginusername;
        public static string loginpassword;
        public static int sayac = 3;
        public Form1()
        {
            InitializeComponent();
        }
        SQLiteConnection connection = new SQLiteConnection("Data Source=Data/guardiantdb.db");
        private void Form1_Load(object sender, EventArgs e)
        {
            label5.Visible = false;
            connection.Open();
            SQLiteCommand username = new SQLiteCommand();
            username.CommandText = "SELECT username FROM users";
            username.Connection = connection;
            username.CommandType = CommandType.Text;

            SQLiteDataReader uname;
            uname = username.ExecuteReader();
            while (uname.Read())
            {
                loginusername = uname["username"].ToString();
            }

            SQLiteCommand password = new SQLiteCommand();
            password.CommandText = "SELECT password FROM users";
            password.Connection = connection;
            password.CommandType = CommandType.Text;

            SQLiteDataReader passwd;
            passwd = password.ExecuteReader();
            while (passwd.Read())
            {
                loginpassword = passwd["password"].ToString();
            }
            connection.Close();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
        private void PassControl_VisibleChanged(object sender, EventArgs e)
        {
            PassControl passControl = new PassControl();
            // İkinciForm'un VisibleChanged olayında tetiklenen işleyici, form yeniden görünür hale geldiğinde çalışacak.
            // Burada TextBox1'in görünürlüğünü yeniden ayarlayacağız.
            if (passControl.Visible)
            {
                passControl.minimal();
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {

            if (textBox1.Text == loginusername || textBox2.Text == loginpassword)
            {
                panel3.Controls.Clear();
                PassControl pwControl = new PassControl(); // "Belgeler" User Control'ünü oluştur
                pwControl.Dock = DockStyle.Fill; // User Control'ü Panel kontrolünün tamamını kaplayacak şekilde boyutlandır
                panel3.Controls.Add(pwControl); // User Control'ü Panel kontrolüne ekle 

            }
            else
            {
                label5.Visible = true;
                sayac--;
                if (sayac > 0)
                {
                    label5.Text = "Wrong Password ! (Login Attemp=" + sayac + ")";
                }
                else
                {
                    MessageBox.Show("Too many incorrect login attempts. The program is closing !");
                    this.Dispose();
                }
            }

        }
        private void button1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            PassControl passControl = new PassControl();
            passControl.minimal();
            this.Hide();
            notifyIcon1.Visible = true;
            notifyIcon1.Text = "Guardiant";
            notifyIcon1.Icon = new Icon("Data/favico.ico");
            notifyIcon1.Click += NotifyIcon1_Click;
            notifyIcon1.ShowBalloonTip(3000, "Guardiant", "Still Working and locked", ToolTipIcon.Info);
        }

        private void NotifyIcon1_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }
    }
}
