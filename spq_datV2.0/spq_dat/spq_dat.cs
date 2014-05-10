using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Timers;

using ADOX;
using System.Data.OleDb;
using System.IO;

namespace spq_dat
{
    public partial class spq_dat : Form
    {
        private SerialPort seriaPort;
        private string portName;
        private int baudRate;
        private Parity parity;
        private int dataBit;
        private StopBits stopBit;
        private string operatorName;
        private string type;
        private float STD;
        private float USL;
        private float LSL;
        private string strComPortData;
        private  string readData;
        private int interval;
        private float sum = 0;
        private int NG1 = 0;
        private int NG2 = 0;
        private int firstKeyCode;
        private int secondKeyCode;
        private bool isWrap = false;
        private const int listMaxCount = 1000;

        public spq_dat()
        {
            InitializeComponent();
        }

        private void spq_dat_Load(object sender, EventArgs e)
        {
            seriaPort = new SerialPort();
            CheckForIllegalCrossThreadCalls = false;
            portName = "COM3";
            baudRate = 9600;
            parity = Parity.None;
            dataBit = 8;
            stopBit = StopBits.One;
            operatorName = "数显";
            type = "千分尺";
            STD = 10.0f;
            USL = 0.005f;
            LSL = -0.005f;
            txtBaud.Text = baudRate.ToString();
            txtPort.Text = portName.ToString();
            txtType.Text = type.ToString();
            txtValue.Text = "0.000";
            txtWrap.Text = "Err:——";
            interval = timer1.Interval;
            seriaPort.DataReceived += new SerialDataReceivedEventHandler(seriaPort_DataReceived);
            ResetAll();
        }

        private bool Verify(string value)
        {
            try
            {
                if (value != null)
                {
                    Convert.ToInt32(value);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!seriaPort.RtsEnable)
                {
                    return;
                }
                if (Verify(readData))
                {
                    txtValue.Text = (Convert.ToSingle(readData) / 1000).ToString("f3");
                    MeasureData md = new MeasureData();
                    md.Data = (Convert.ToSingle(readData) / 1000).ToString("f3");
                    md.Err = Convert.ToSingle((Convert.ToSingle(md.Data) - STD).ToString("f3"));
                    md.Number = lbView.Items.Count + 1;
                    md.Time = System.DateTime.Now.ToString();
                    if (lbView.Items.Count >= listMaxCount)
                    {
                        lbView.Items.RemoveAt(0);
                    }
                    lbView.Items.Add("   " + md.Number + "\t    " + md.Data + "\t    " + md.Err);
                    lbView.SelectedIndex = lbView.Items.Count - 1;
                    UpdataPara(Convert.ToSingle(md.Data), md.Err);
                    dataAndTime = md.Time + "\t" + md.Data + "\t"  + md.Err +  "\t" + firstKeyCode + "\t" + secondKeyCode;
                    if (isSaveData)
                    {
                        //SaveData("Data");
                        streamWriter.WriteLine(dataAndTime);
                    }
                }
            }
            catch (Exception ex)
            { 
                
            }
        }

        void seriaPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                while (seriaPort.BytesToRead > 0)
                {
                    lock (this)
                    {
                        strComPortData = seriaPort.ReadExisting();//entire bytes in the port buffer
                        if (strComPortData.Length == 10)
                        {
                            readData = strComPortData.Substring(2, 6);
                            firstKeyCode = Convert.ToInt32(strComPortData.Substring(8, 1));
                            secondKeyCode = Convert.ToInt32(strComPortData.Substring(9, 1));
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (btnConnect.Text.Equals("连接设备"))
                {
                    seriaPort.Encoding = Encoding.ASCII;
                    seriaPort.PortName = portName;
                    seriaPort.BaudRate = baudRate;
                    seriaPort.Parity = parity;
                    seriaPort.DataBits = dataBit;
                    seriaPort.StopBits = stopBit;
                    seriaPort.Open();
                    btnConnect.Text = "断开设备";
                }
                else
                {
                    System.Threading.Thread.Sleep(100);
                    seriaPort.Close();      
                    seriaPort.RtsEnable = false;
                    this.timer1.Stop();
                    isSaveData = false;
                    if (streamWriter != null)
                    {
                        streamWriter.Close();
                    }
                    btnSaveStart.Text = "开始保存";
                    btnConnect.Text = "连接设备";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnReadData_Click(object sender, EventArgs e)
        {
            if (seriaPort.RtsEnable)
            {
                seriaPort.RtsEnable = false;
                btnReadData.Text = "读取数据";
                this.timer1.Stop();
            }
            else
            {
                seriaPort.RtsEnable = true;
                btnReadData.Text = "停止读取";
                this.timer1.Start();
            }
        }

        bool isSaveData = false;
        string dataAndTime = string.Empty;

        string myConnectionString = string.Empty;
        string startTime = string.Empty;
        //保存数据
        private void CreateDataTable()
        {
            startTime = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + "-" + DateTime.Now.Hour.ToString() + "-" + DateTime.Now.Minute.ToString() + "-" + DateTime.Now.Second.ToString();

            ADOX.Catalog catalog = new Catalog();
            string tableName = startTime;

            string myDataPath = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + "D:\\Data\\" + tableName + ".mdb";
            myConnectionString = myDataPath;
            catalog.Create(myDataPath + ";Jet OLEDB:Engine Type=5");

            ADODB.Connection connection = new ADODB.Connection();
            connection.Open(myDataPath, null, null, -1);
            catalog.ActiveConnection = connection;

            ADOX.Table table = new ADOX.Table();
            table.Name = "Data";
            ADOX.Column column = new ADOX.Column();
            column.ParentCatalog = catalog;
            column.Name = "Num_My";
            column.Type = DataTypeEnum.adInteger;
            column.DefinedSize = 9;
            column.Properties["AutoIncrement"].Value = true;
            table.Columns.Append(column, DataTypeEnum.adInteger, 12);
            table.Keys.Append("FirstTablePrimaryKey", KeyTypeEnum.adKeyPrimary, column, null, null);

            table.Columns.Append("Date", DataTypeEnum.adDate, 20);
            table.Columns.Append("Position", DataTypeEnum.adSingle, 9);

            catalog.Tables.Append(table);
        }

        private void SaveData(string tableName)
        {
            using (OleDbConnection connection = new OleDbConnection(myConnectionString))
            {
                try
                {
                    string insertSQL = "insert into " + tableName.ToString()
                    + "([Data],[Position]) values ('" + DateTime.Now.ToShortTimeString() + "','" + readData.ToString() + "')";

                    OleDbCommand command = new OleDbCommand(insertSQL);

                    // Set the Connection to the new OleDbConnection.
                    command.Connection = connection;

                    // Open the connection and execute the insert command.

                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {

                }
            }

        }

        //文本文件保存
        FileStream fileStream;
        StreamWriter streamWriter;
        //string filePath = "D:\\Data\\";

        public void CreateDataForTxt(string filePath, string data, bool isChecked)
        {
            if (File.Exists(filePath))
            {
                fileStream = new FileStream(filePath, FileMode.Append, FileAccess.Write);
            }
            else
            {
                fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            }
            
            if (isChecked)
            {
                streamWriter = new StreamWriter(fileStream, System.Text.Encoding.GetEncoding("gb2312"));
            }
            else
            {
                streamWriter = new StreamWriter(fileStream);
            }
            
            streamWriter.WriteLine(data);
        }

        private void btnSetting_Click(object sender, EventArgs e)
        {
            Setting setting = new Setting();
            setting.Operator = operatorName;
            setting.Type = type;
            setting.STD = STD;
            setting.USL = USL;
            setting.LSL = LSL;
            setting.Port = portName;
            setting.Baud = baudRate;
            setting.Parity = parity;
            setting.DataBits = dataBit;
            setting.StopBits = stopBit;
            setting.Interval = interval;
            if (setting.ShowDialog() == DialogResult.OK)
            {
                operatorName = setting.Operator;
                type = setting.Type;
                STD = setting.STD;
                USL = setting.USL;
                LSL = setting.LSL;
                portName = setting.Port;
                baudRate = setting.Baud;
                parity = setting.Parity;
                dataBit = setting.DataBits;
                stopBit = setting.StopBits;
                interval = setting.Interval;
                txtBaud.Text = baudRate.ToString();
                txtPort.Text = portName.ToString();
                txtType.Text = type.ToString();
                labelMAX.Text = STD.ToString("f3");
                labelMIN.Text = STD.ToString("f3");
                timer1.Interval = interval;
                labelOperator.Text = operatorName;
            }
        }

        private void btnErrView_Click(object sender, EventArgs e)
        {
            if (isWrap)
            {
                isWrap = false;
            }
            else
            {
                isWrap = true;
            }
        }

        private void btnResetAll_Click(object sender, EventArgs e)
        {
            lbView.Items.Clear();
            ResetAll();
        }

        private void ResetAll()
        {
            sum = 0;
            labelAVG.Text = (0).ToString("f3");
            labelP.Text = (0).ToString("f3");
            labelR.Text = (0).ToString("f3");
            labelTOL.Text = (0).ToString("f3");
            labelNum.Text = (0).ToString();
            labelMAX.Text = (STD).ToString("f3");
            labelMIN.Text = (STD).ToString("f3");
            NG1 = 0;
            NG2 = 0;
            labelNG1.Text = (0).ToString();
            labelNG2.Text = (0).ToString();
            labelSTD.Text = STD.ToString("f3");
            labelUSL.Text = USL.ToString("f3");
            labelLSL.Text = LSL.ToString("f3");
            labelOperator.Text = operatorName.ToString();
        }

        private void btnSaveStart_Click(object sender, EventArgs e)
        {
            if (!isSaveData)
            {
                SaveFileDialog filedialog = new SaveFileDialog();
                filedialog.CheckPathExists = true;
                filedialog.DefaultExt = "txt";
                filedialog.Filter = "文本文件(*.txt)|*.txt|所有文件(*.*)|*.*";
                startTime = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + "-" + DateTime.Now.Hour.ToString() + "-" + DateTime.Now.Minute.ToString() + "-" + DateTime.Now.Second.ToString();
                //string filePath = "D:\\Data\\" + startTime + ".txt";
                filedialog.FileName = startTime;
                if (filedialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = filedialog.FileName;
                    CreateDataForTxt(filePath, dataAndTime, false);
                    isSaveData = true;
                    btnSaveStart.Text = "停止保存";
                }
            }
            else
            {
                if (streamWriter != null)
                {
                    streamWriter.Close();
                }
                isSaveData = false;
                btnSaveStart.Text = "开始保存";
                seriaPort.RtsEnable = false;
                btnReadData.Text = "读取数据";
            }
        }

        private void UpdataPara(float value,float err)
        {
            labelSTD.Text = STD.ToString("f3");
            labelUSL.Text = USL.ToString("f3");
            labelLSL.Text = LSL.ToString("f3");
            labelNum.Text =lbView.Items.Count.ToString();
            if (value > Convert.ToSingle(labelMAX.Text))
            {
                labelMAX.Text = value.ToString("f3");
            }
            if (value < Convert.ToSingle(labelMIN.Text))
            {
                labelMIN.Text = value.ToString("f3");
            }
            if (err > 0 && err > USL)
            {
                NG1++;
            }
            if(err < 0 && err < LSL)
            {
                NG2++;
            }
            sum += value;
            labelAVG.Text = (sum / lbView.Items.Count).ToString("f3");
            labelTOL.Text = (USL- LSL).ToString("f3");
            labelP.Text = "";
            labelR.Text = (Convert.ToSingle(labelMAX.Text) - Convert.ToSingle(labelMIN.Text)).ToString("f3");
            labelNG1.Text = NG1.ToString() ;
            labelNG2.Text = NG2.ToString();
            if (isWrap)
            {
                txtWrap.Text = "ERR:" + (Convert.ToSingle(txtValue.Text) - STD).ToString("f3");
            }
            else
            {
                txtWrap.Text = "ERR: ------";
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            seriaPort.RtsEnable = false;
            System.Threading.Thread.Sleep(200);
            seriaPort.RtsEnable = true;
        }

        private void btnConnect_TextChanged(object sender, EventArgs e)
        {
            if (seriaPort.IsOpen)
            {
                btnSetting.Enabled = false;
                btnSaveStart.Enabled = true;
            }
            else
            {
                btnSetting.Enabled = true;
                btnSaveStart.Enabled = false;
                btnReadData.Enabled = false;
            }
        }

        private void btnSaveStart_TextChanged(object sender, EventArgs e)
        {
            if (isSaveData)
            {
                btnReadData.Enabled = true;
            }
            else
            {
                btnReadData.Enabled = false;
            }
        }

        private void btnView_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.CheckPathExists = true;
            openFile.DefaultExt = "txt";
            openFile.Filter = "文本文件(*.txt)|*.txt|所有文件(*.*)|*.*";
        }
    }
}