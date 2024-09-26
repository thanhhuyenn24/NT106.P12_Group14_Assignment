using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Calculator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //Fields
        Double result = 0;
        string operation = string.Empty;
        string fstNum, secNum;
        bool enterValue = false;

        private void Math_Click(object sender, EventArgs e)
        {
            if (result != 0) BtnEquals.PerformClick();
            else result = Double.Parse(TxtDisplay1.Text);

            Button button = (Button)sender;
            operation=button.Text;
            enterValue = true;
            if (TxtDisplay1.Text != "0")
            {
                TxtDisplay2.Text = fstNum = $"{result}{operation}";
                TxtDisplay1.Text=string.Empty;
            }
        }

        private void BtnEquals_Click(object sender, EventArgs e)
        {
            secNum = TxtDisplay1.Text;
            TxtDisplay2.Text = $"{TxtDisplay2.Text}{TxtDisplay1.Text} = ";
            if (TxtDisplay1.Text=="0") TxtDisplay2.Text=string.Empty;
            switch (operation)
            {
                case "+":
                    TxtDisplay1.Text = (result + Double.Parse(TxtDisplay1.Text)).ToString();
                    richTextBox1.AppendText($"{fstNum}{secNum} = {TxtDisplay1.Text}\n");
                    break;
                case "-":
                    TxtDisplay1.Text = (result - Double.Parse(TxtDisplay1.Text)).ToString();
                    richTextBox1.AppendText($"{fstNum}{secNum} = {TxtDisplay1.Text}\n");
                    break;
                case "×":
                    TxtDisplay1.Text = (result * Double.Parse(TxtDisplay1.Text)).ToString();
                    richTextBox1.AppendText($"{fstNum}{secNum} = {TxtDisplay1.Text}\n");
                    break;
                case "÷":
                    TxtDisplay1.Text = (result / Double.Parse(TxtDisplay1.Text)).ToString();
                    if (TxtDisplay1.Text == "∞")
                    {
                        BtnC_Click(sender, e);
                        MessageBox.Show("Cannot divide by zero", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    richTextBox1.AppendText($"{fstNum}{secNum} = {TxtDisplay1.Text}\n");
                    break;
                default:
                    TxtDisplay2.Text = $"{TxtDisplay1.Text} =";
                    break;
            }
            result=Double.Parse(TxtDisplay1.Text);
            operation = string.Empty;
        }

        private void BtnHistory_Click(object sender, EventArgs e)
        {
            if (ClearHistory.Visible == false)
            {
                richTextBox1.Visible = true;
                ClearHistory.Visible = true;
            }
            else
            {
                richTextBox1.Visible = false;
                ClearHistory.Visible = false;
            }
        }

        private void ClearHistory_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
        }

        private void BtnBackSpace_Click(object sender, EventArgs e)
        {
            if (TxtDisplay1.Text.Length>0)
                TxtDisplay1.Text=TxtDisplay1.Text.Remove(TxtDisplay1.Text.Length - 1,1);
            if (TxtDisplay1.Text == string.Empty) TxtDisplay1.Text = "0";
        }

        private void BtnC_Click(object sender, EventArgs e)
        {
            TxtDisplay1.Text = "0";
            TxtDisplay2.Text = string.Empty;
            result = 0;
        }

        private void BtnCE_Click(object sender, EventArgs e)
        {
            TxtDisplay1.Text = "0";
        }

        private void OthersMath_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            operation=button.Text;
            switch (operation)
            {
                case "%":
                    TxtDisplay2.Text = $"{TxtDisplay1.Text}%";
                    TxtDisplay1.Text=Convert.ToString(Double.Parse(TxtDisplay1.Text)/Convert.ToDouble(100));
                    break;
                case "+/-":
                    TxtDisplay1.Text = Convert.ToString(Double.Parse(TxtDisplay1.Text) * -1);
                    break;
                case "1/x":
                    TxtDisplay2.Text = $"1/{TxtDisplay1.Text}";
                    TxtDisplay1.Text = Convert.ToString(1.0/Double.Parse(TxtDisplay1.Text));
                    if (TxtDisplay1.Text == "∞")
                    {
                        BtnC_Click(sender, e);
                        MessageBox.Show("Cannot divide by zero","Error",MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    break;
                case "√":
                    TxtDisplay2.Text = $"√({TxtDisplay1.Text})";
                    TxtDisplay1.Text = Convert.ToString(Math.Sqrt(Double.Parse(TxtDisplay1.Text)));
                    break;
                case "^2":
                    TxtDisplay2.Text = $"{TxtDisplay1.Text}^2";
                    TxtDisplay1.Text = Convert.ToString(Double.Parse(TxtDisplay1.Text) * Double.Parse(TxtDisplay1.Text));
                    break;
            }
            richTextBox1.AppendText($"{TxtDisplay2.Text} = {TxtDisplay1.Text}\n");
        }

      
        private void Num_Click(object sender, EventArgs e)
        {
            if (TxtDisplay1.Text == "0"||enterValue) TxtDisplay1.Text = string.Empty;
            enterValue = false;
            Button button = (Button)sender;
            if (button.Text == ".")
            {
                if (!TxtDisplay1.Text.Contains("."))
                    TxtDisplay1.Text=TxtDisplay1.Text + button.Text;
            }
            else TxtDisplay1.Text=TxtDisplay1.Text + button.Text;
            // Neu van ban hien tai la ".", them "0." thay vi chi them "."
            if (TxtDisplay1.Text == ".")
            {
                TxtDisplay1.Text = "0.";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("This is a useless button ദ്ദി(˵ •̀ ᴗ - ˵ ) ✧", "A usless caption");
        }

    }
}
