using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Guardiant
{
    public partial class unlock : Form
    {
        
        public static int sayac = 3;
        private readonly PassControl unlocking; 
        public unlock(PassControl unlocker)
        {
            InitializeComponent();
            this.unlocking = unlocker;
            textBox1.TextChanged += checktext;
            connection.Open();
        }
        SQLiteConnection connection = new SQLiteConnection("Data Source=Data/guardiantdb.db");
        private void checktext(object sender, EventArgs e)
        {
            string text = textBox1.Text;

            // Veritabanından yazıyı al ve kontrol et
            using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM users WHERE password=@pass", connection))
            {
                cmd.Parameters.AddWithValue("@pass", text);
                int count = Convert.ToInt32(cmd.ExecuteScalar());

                if (count > 0)
                {
                    
                    this.Close();
                }
                else
                {
                    
                    sayac--;
                    if (sayac > 0)
                    {
                        label2.Visible = true;
                    }
                    else
                    {
                        MessageBox.Show("Too many incorrect login attempts. The program is closing !");
                        this.Close();
                    }
                }
            }
        }
        private void unlock_Load(object sender, EventArgs e)
        {
            label2.Visible=false;
        }

        private void unlock_FormClosing(object sender, FormClosingEventArgs e)
        {
            connection.Close();
        }
    }
}
