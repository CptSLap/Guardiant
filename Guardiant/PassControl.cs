using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Guardiant
{

    public partial class PassControl : UserControl
    {
        private static PassControl instance; // Singleton deseni için örnek
        private const string EncryptionKey = "Cpt7527++Opt2673";
        public static PassControl Instance
        {
            get
            {
                if (instance == null)
                    instance = new PassControl();
                return instance;
            }
        }
        private int wrongAttempts = 0;
        private const int maxWrongAttempts = 3;
        private const int disableTimeInSeconds = 180; // 5 dakika = 5 * 60 saniye
        public static int sayac = 3;
        public PassControl()
        {
            InitializeComponent();

        }
        SQLiteConnection connection = new SQLiteConnection("Data Source=Data/guardiantdb.db");
        private void button1_Click(object sender, EventArgs e)
        {
            dataGridView1.Visible = false;
            button1.Visible = false;
            button4.Visible = true;
            button3.Enabled = false;
            button2.Enabled = false;
        }
        public void minimal()
        {
            dataGridView1.Visible = false;
            button1.Visible = false;
            button4.Visible = true;
            button3.Enabled = false;
            button2.Enabled = false;
        }
        private void timerDisable_Tick(object sender, EventArgs e)
        {
            // Timer'ın olayı tetiklendiğinde, TextBox'i tekrar kullanıma aç
            textBox1.Enabled = true;
            button3.Enabled = true;
            button2.Enabled = true;
            label2.Text = string.Empty;
            wrongAttempts = 0;
            timer1.Enabled = false; // Timer'ı durdur
        }
        private DataTable DecryptUserData(DataTable encryptedDataTable)
        {
            DataTable decryptedDataTable = new DataTable();

            // Şifresi çözülmüş sütunları eklemek için varolan sütunları kullanın
            foreach (DataColumn column in encryptedDataTable.Columns)
            {
                if (column.ColumnName == "password")
                {
                    // "Password" sütununu "DecryptedPassword" olarak ekleyin
                    decryptedDataTable.Columns.Add("password");
                }
                else
                {
                    // Diğer sütunları aynı şekilde ekleyin
                    decryptedDataTable.Columns.Add(column.ColumnName);
                }
            }

            foreach (DataRow row in encryptedDataTable.Rows)
            {
                DataRow decryptedRow = decryptedDataTable.NewRow();

                // "Password" sütununu çözerek "DecryptedPassword" sütununa ekleyin
                byte[] encryptedPassword = (byte[])row["password"];
                string decryptedPassword = DecryptString(encryptedPassword, EncryptionKey);
                decryptedRow["password"] = decryptedPassword;

                // Diğer sütunları da aynı şekilde ekleyin
                foreach (DataColumn column in encryptedDataTable.Columns)
                {
                    if (column.ColumnName != "password")
                    {
                        decryptedRow[column.ColumnName] = row[column.ColumnName];
                    }
                }

                decryptedDataTable.Rows.Add(decryptedRow);
            }

            return decryptedDataTable;
        }


        private static string DecryptString(byte[] cipherText, string key)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.IV = new byte[16]; // IV sıfırlanmış olarak kullanılmaktadır.

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
        public void refresh()
        {
            using (var connectionc = new SQLiteConnection(connection))
            {
                connection.Open();
                SQLiteDataAdapter adapter = new SQLiteDataAdapter("SELECT website, username, password, email, buton, id FROM guarded", connection);
                DataTable dataAdapter = new DataTable();
                adapter.Fill(dataAdapter);
                DataTable decryptedDataTable = DecryptUserData(dataAdapter);
                dataGridView1.Rows.Clear(); // İçerisinde varolan satırları temizle
                foreach (DataRow row in decryptedDataTable.Rows)
                {
                    dataGridView1.Rows.Add(row.ItemArray); // Satırı DataGridView'e ekle
                }
                connection.Close();
            }

        }
        private void PassControl_Load(object sender, EventArgs e)
        {
            button4.Visible = false;
            label2.Visible = false;
            textBox1.Visible = false;
            refresh();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            addForm addform = new addForm(this);
            addform.Show();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Butona tıklanıldığında işlem yapmak için kontrol edin
            if (e.ColumnIndex == dataGridView1.Columns["copybutton"].Index && e.RowIndex >= 0)
            {
                // 3. hücrenin içeriğini alın
                string valueToCopy = dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString();
                string copiedWebsite = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();

                // İşlem yapmak için valueToCopy değişkenini kullanabilirsiniz.
                // Örnek olarak Clipboard'a kopyalama işlemi yapabilirsiniz:
                Clipboard.SetText(valueToCopy);

                MessageBox.Show(copiedWebsite + " Password Copied");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure ?", "Warning !", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                foreach (DataGridViewRow idrow in dataGridView1.SelectedRows)
                {

                    string delete = "DELETE FROM guarded WHERE id=" + idrow.Cells["id"].Value + "";
                    SQLiteCommand command = new SQLiteCommand(delete, connection);
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                    refresh();
                }
            }
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
        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                string columnName = dataGridView1.Columns[e.ColumnIndex].Name;

                // Eğer değişiklik yapılan sütun "Password" ise işlem yap
                if (columnName == "password")
                {
                    int Secilen = Convert.ToInt32(row.Cells["id"].Value);
                    string newValue = row.Cells[columnName].Value.ToString();

                    // Şifreleme işlemi
                    byte[] encryptedValue = EncryptString(newValue, EncryptionKey);

                    // Veritabanına şifreli veriyi kaydet
                    using (var connectiond = new SQLiteConnection(connection))
                    {
                        connection.Open();
                        string updateQuery = $"UPDATE guarded SET password = @password WHERE id = @id";
                        SQLiteCommand cmd = new SQLiteCommand(updateQuery, connection);
                        cmd.Parameters.AddWithValue("@password", encryptedValue);
                        cmd.Parameters.AddWithValue("@id", Secilen);
                        cmd.ExecuteNonQuery();
                        connection.Close();
                    }
                }
                else
                {
                    string id = row.Cells[columnName].Value.ToString();
                    int Secilen = Convert.ToInt32(row.Cells["id"].Value);
                    connection.Open();
                    string updateQuery = "UPDATE guarded SET '" + columnName + "' = '" + id + "' WHERE  id = '" + Secilen + "'";
                    SQLiteCommand cmd = new SQLiteCommand(updateQuery, connection);
                    cmd.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }


        private void button4_Click(object sender, EventArgs e)
        {
            label2.Visible = true;
            textBox1.Visible = true;
            if (dataGridView1.Visible == true)
            {
                label2.Visible = false;
                textBox1.Visible = false;
            }
            else
            {
                label2.Visible = true;
                textBox1.Visible = true;
            }

        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true; // Enter tuşunun olayı işleme devam etmesini engelle
                string text = textBox1.Text;

                connection.Open();

                // Veritabanından yazıyı al ve kontrol et
                using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM users WHERE password=@pass", connection))
                {
                    cmd.Parameters.AddWithValue("@pass", text);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());

                    if (count > 0)
                    {
                        label2.Visible = false;
                        textBox1.Visible = false;
                        button4.Visible = false;
                        button1.Visible = true;
                        dataGridView1.Visible = true;
                        textBox1.Text = "";
                        button3.Enabled = true;
                        button2.Enabled = true;
                    }
                    else
                    {
                        wrongAttempts++;
                        if (wrongAttempts >= maxWrongAttempts)
                        {
                            // Belirtilen sayıda yanlış deneme yapıldı, TextBox'i devre dışı bırak
                            label2.Text = "Too many attemp. Wait 5 minutes";
                            textBox1.Enabled = false;
                            timer1.Interval = disableTimeInSeconds * 1000; // Timer aralığını milisaniye cinsinden ayarla
                            timer1.Enabled = true; // Timer'ı başlat
                        }
                        else
                        {
                            label2.Text = $"Wrong Password. Remaining attempt: {maxWrongAttempts - wrongAttempts}";
                        }
                    }
                }
                connection.Close();

            }
        }

        private void PassControl_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                minimal();
            }
        }
    }
}
