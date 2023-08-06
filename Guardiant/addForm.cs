using System;
using System.Data.SQLite;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Guardiant
{
    public partial class addForm : Form
    {
        private readonly PassControl passwordControl;
        public addForm(PassControl passControl)
        {
            InitializeComponent();
            this.passwordControl = passControl;
        }
        SQLiteConnection connection = new SQLiteConnection("Data Source=Data/guardiantdb.db");
        private const string EncryptionKey = "Cpt7527++Opt2673";
        private void addForm_Load(object sender, EventArgs e)
        {

        }
        private static byte[] EncryptString(string plainText, string key)
        {
            byte[] encrypted;
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.IV = new byte[16]; // IV sıfırlanmış olarak kullanılmaktadır.

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        byte[] bytesToEncrypt = Encoding.UTF8.GetBytes(plainText);
                        csEncrypt.Write(bytesToEncrypt, 0, bytesToEncrypt.Length);
                    }
                    encrypted = msEncrypt.ToArray();
                }
            }
            return encrypted;
        }
        public static class SQLiteHelper
        {
            private const string DllName = "sqlite3"; // SQLite C API DLL dosyasının adı

            // SQLite veritabanını açan ve şifreleyen C API fonksiyonları
            [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern int sqlite3_key(IntPtr db, byte[] key, int keyLength);

            [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern int sqlite3_rekey(IntPtr db, byte[] newKey, int newKeyLength);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            string add = "INSERT INTO guarded (website, username, password, email, buton) VALUES (@website, @username, @password, @email, @buton)";
            SQLiteCommand adding = new SQLiteCommand(add, connection);
            adding.Parameters.AddWithValue("@website", textBox1.Text);
            adding.Parameters.AddWithValue("@username", textBox2.Text);
            adding.Parameters.AddWithValue("@password", EncryptString(textBox3.Text, EncryptionKey));
            adding.Parameters.AddWithValue("@email", textBox4.Text);
            adding.Parameters.AddWithValue("@buton", "Copy");
            connection.Open();
            adding.ExecuteNonQuery();
            connection.Close();
            MessageBox.Show("Successfully Added");

            passwordControl.refresh();
        }
    }
}
