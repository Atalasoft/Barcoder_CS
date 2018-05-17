using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace BarcodeDemo
{
    public partial class ResultForm : Form
    {
        public ResultForm(string resultText)
        {
            InitializeComponent();
            textBox1.Text = resultText;
        }
        public ResultForm()
        {
            InitializeComponent();
            textBox1.Text = "No results";
        }
    }
}
