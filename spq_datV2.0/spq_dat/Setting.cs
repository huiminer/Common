using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;

namespace spq_dat
{
    public partial class Setting : Form
    {
        public Setting()
        {
            InitializeComponent();
        }

        private string operatorName = "ÊýÏÔ";
        public string Operator
        {
            get { return operatorName; }
            set { operatorName = value; }
        }

        private string type = "Ç§·Ö³ß";
        public string Type
        {
            get { return type; }
            set { type = value; }
        }

        private string port = "COM1";
        public string Port
        {
            get { return port; }
            set { port = value; }
        }

        private int baud = 9600;
        public int Baud
        {
            get { return baud; }
            set { baud = value; }
        }

        private Parity parity = Parity.None;
        public Parity Parity
        {
            get { return parity; }
            set { parity = value; }
        }

        private int dataBits = 8;
        public int DataBits
        {
            get { return dataBits; }
            set { dataBits = value; }
        }

        private StopBits stopBits = StopBits.One;
        public StopBits StopBits
        {
            get { return stopBits; }
            set { stopBits = value; }
        }

        private float std = 10.0f;
        public float STD
        {
            get { return std; }
            set { std = value; }
        }

        private float usl = 0.005f;
        public float USL
        {
            get { return usl; }
            set { usl = value; }
        }

        private float lsl = -0.005f;
        public float LSL
        {
            get { return lsl; }
            set { lsl = value; }
        }

        private int interval = 500;
        public int Interval
        {
            get { return interval; }
            set { interval = value; }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Operator = txtOperator.Text.Trim();
            if (radioButton2.Checked)
            {
                Type = "Ç§·Ö³ß";
            }
            else
            {
                Type = "¿¨³ß";
            }
            STD = Convert.ToSingle(txtSTD.Text.Trim());
            USL = Convert.ToSingle(txtUSL.Text.Trim());
            LSL = Convert.ToSingle(txtLSL.Text.Trim());
            Port = cbPort.SelectedItem.ToString();
            Baud = Convert.ToInt32(cbBaud.SelectedItem);
            Parity = (Parity)(cbParity.SelectedIndex);
            DataBits = Convert.ToInt32(cbDataBits.SelectedItem);
            Interval = Convert.ToInt32(txtInterval.Text.ToString()); 
            if (cbStopBits.SelectedText.Equals("1.5"))
            {
                StopBits = StopBits.OnePointFive;
            }
            else
            {
                StopBits = (StopBits)Convert.ToInt32(cbStopBits.SelectedItem);
            }
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void Setting_Shown(object sender, EventArgs e)
        {
            txtOperator.Text = Operator.ToString();
            if (Type.Equals("Ç§·Ö³ß"))
            {
                radioButton2.Checked = true;
            }
            else
            {
                radioButton1.Checked = true;
            }
            txtSTD.Text = STD.ToString();
            txtUSL.Text = USL.ToString();
            txtLSL.Text = LSL.ToString();
            cbPort.SelectedItem = Port.ToString();
            cbBaud.SelectedItem = Baud.ToString();
            cbParity.SelectedIndex = Convert.ToInt32(Parity);
            cbDataBits.SelectedItem = DataBits.ToString();
            txtInterval.Text = Interval.ToString(); ;
            if (StopBits == StopBits.OnePointFive)
            {
                cbStopBits.SelectedItem = "1.5";
            }
            else
            {
                cbStopBits.SelectedItem = Convert.ToInt32(StopBits).ToString();
            }
        }
    }
}