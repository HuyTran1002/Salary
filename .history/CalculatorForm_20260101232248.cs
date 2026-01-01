using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Media;
using System.Windows.Forms;

namespace SalaryCalculator
{
    // Professional calculator UI inspired by real calculators
    public class CalculatorForm : Form
    {
        private Label displayLabel;
        private string currentInput = "0";
        private string lastAnswer = "0";
        private string lastOperator = "";
        private double lastValue = 0;
        private bool newNumber = true;

        public CalculatorForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Máy Tính";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Width = 420;
            this.Height = 580;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(40, 40, 40);
            this.KeyPreview = true;
            this.KeyDown += CalculatorForm_KeyDown;

            // Display screen - tăng tương phản
            displayLabel = new Label();
            displayLabel.Text = "0";
            displayLabel.Font = new Font("Segoe UI", 36, FontStyle.Bold);
            displayLabel.ForeColor = Color.FromArgb(255, 140, 0);  // Màu cam
            displayLabel.BackColor = Color.Black;   // Nền đen tuyệt đối
            displayLabel.BorderStyle = BorderStyle.FixedSingle;
            displayLabel.TextAlign = ContentAlignment.MiddleRight;
            displayLabel.Location = new Point(10, 10);
            displayLabel.Size = new Size(380, 90);
            this.Controls.Add(displayLabel);

            // Button layout: 4 columns x 5 rows
            var table = new TableLayoutPanel();
            table.ColumnCount = 4;
            table.RowCount = 5;
            table.Width = 400;
            table.Height = 460;
            table.Location = new Point(10, 110);
            table.BackColor = Color.FromArgb(40, 40, 40);
            table.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            
            for (int c = 0; c < table.ColumnCount; c++)
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            for (int r = 0; r < table.RowCount; r++)
                table.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));

            this.Controls.Add(table);

            // Button factory with professional styling
            Button AddBtn(string text, int col, int row, int colSpan = 1, int rowSpan = 1, string tag = null, Color? bgColor = null)
            {
                var b = new Button();
                b.Dock = DockStyle.Fill;
                b.Text = text;
                b.Tag = tag ?? text;
                b.Font = new Font("Segoe UI", 16, FontStyle.Bold);
                b.ForeColor = Color.White;
                b.BackColor = bgColor ?? Color.FromArgb(80, 80, 80);
                b.FlatStyle = FlatStyle.Flat;
                b.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100);
                b.FlatAppearance.BorderSize = 1;
                b.Cursor = Cursors.Hand;
                b.Click += CalcButton_Click;
                
                b.MouseEnter += (s, e) => b.BackColor = bgColor.HasValue 
                    ? DarkenColor((Color)bgColor) 
                    : Color.FromArgb(100, 100, 100);
                b.MouseLeave += (s, e) => b.BackColor = bgColor ?? Color.FromArgb(80, 80, 80);
                
                table.Controls.Add(b, col, row);
                if (colSpan > 1) table.SetColumnSpan(b, colSpan);
                if (rowSpan > 1) table.SetRowSpan(b, rowSpan);
                return b;
            }

            // Color definitions
            Color acColor = Color.FromArgb(200, 50, 50);
            Color opColor = Color.FromArgb(200, 100, 50);
            Color eqColor = Color.FromArgb(50, 150, 50);

            // Row 0: AC, DEL, %, /
            AddBtn("AC", 0, 0, tag: "AC", bgColor: acColor);
            AddBtn("DEL", 1, 0, tag: "DEL", bgColor: acColor);
            AddBtn("%", 2, 0, tag: "%", bgColor: opColor);
            AddBtn("÷", 3, 0, tag: "/", bgColor: opColor);

            // Row 1: 7, 8, 9, ×
            AddBtn("7", 0, 1);
            AddBtn("8", 1, 1);
            AddBtn("9", 2, 1);
            AddBtn("×", 3, 1, tag: "*", bgColor: opColor);

            // Row 2: 4, 5, 6, −
            AddBtn("4", 0, 2);
            AddBtn("5", 1, 2);
            AddBtn("6", 2, 2);
            AddBtn("−", 3, 2, tag: "-", bgColor: opColor);

            // Row 3: 1, 2, 3, +
            AddBtn("1", 0, 3);
            AddBtn("2", 1, 3);
            AddBtn("3", 2, 3);
            AddBtn("+", 3, 3, tag: "+", bgColor: opColor);

            // Row 4: 0 (spans 2 cols), ., =
            AddBtn("0", 0, 4, colSpan: 2);
            AddBtn(".", 2, 4, tag: ".");
            AddBtn("=", 3, 4, tag: "=", bgColor: eqColor);

            try { Theme.ApplyEcommerceTheme(this); } catch { }
        }

        private Color DarkenColor(Color color)
        {
            return Color.FromArgb(
                Math.Max(0, color.R - 30),
                Math.Max(0, color.G - 30),
                Math.Max(0, color.B - 30)
            );
        }

        private void CalcButton_Click(object sender, EventArgs e)
        {
            if (!(sender is Button b)) return;
            string key = (b.Tag as string) ?? b.Text;

            if (key == "AC")
            {
                currentInput = "0";
                lastValue = 0;
                lastOperator = "";
                newNumber = true;
                UpdateDisplay();
                return;
            }

            if (key == "DEL")
            {
                if (currentInput.Length > 1)
                    currentInput = currentInput.Substring(0, currentInput.Length - 1);
                else
                    currentInput = "0";
                UpdateDisplay();
                return;
            }

            if (key == "=")
            {
                Calculate();
                newNumber = true;
                lastOperator = "";
                return;
            }

            // Handle operators: +, -, *, /
            if (key == "+" || key == "-" || key == "*" || key == "/")
            {
                if (!string.IsNullOrEmpty(lastOperator) && !newNumber)
                {
                    Calculate();
                }
                lastValue = double.Parse(currentInput, CultureInfo.InvariantCulture);
                lastOperator = key;
                newNumber = true;
                return;
            }

            // Handle percentage
            if (key == "%")
            {
                double val = double.Parse(currentInput, CultureInfo.InvariantCulture);
                val = val / 100;
                currentInput = val.ToString(CultureInfo.InvariantCulture);
                UpdateDisplay();
                return;
            }

            // Handle decimal point
            if (key == ".")
            {
                if (!currentInput.Contains("."))
                {
                    currentInput += ".";
                    newNumber = false;
                    UpdateDisplay();
                }
                return;
            }

            // Handle digit input
            if (char.IsDigit(key[0]))
            {
                if (newNumber)
                {
                    currentInput = key;
                    newNumber = false;
                }
                else
                {
                    currentInput += key;
                }
                UpdateDisplay();
                return;
            }
        }

        private void Calculate()
        {
            if (string.IsNullOrEmpty(lastOperator)) return;

            try
            {
                double current = double.Parse(currentInput, CultureInfo.InvariantCulture);
                double result = 0;

                switch (lastOperator)
                {
                    case "+": result = lastValue + current; break;
                    case "-": result = lastValue - current; break;
                    case "*": result = lastValue * current; break;
                    case "/": 
                        if (current == 0)
                        {
                            currentInput = "Lỗi";
                            UpdateDisplay();
                            return;
                        }
                        result = lastValue / current; 
                        break;
                }

                currentInput = result.ToString(CultureInfo.InvariantCulture);
                lastAnswer = currentInput;
                UpdateDisplay();
            }
            catch
            {
                currentInput = "Lỗi";
                UpdateDisplay();
            }
        }

        private void UpdateDisplay()
        {
            // Điều chỉnh font size dựa trên độ dài chữ
            float fontSize = 36;
            if (currentInput.Length > 18)
                fontSize = 18;
            else if (currentInput.Length > 15)
                fontSize = 24;
            else if (currentInput.Length > 12)
                fontSize = 28;
            else if (currentInput.Length > 10)
                fontSize = 32;
            
            displayLabel.Font = new Font("Segoe UI", fontSize, FontStyle.Bold);
            displayLabel.Text = currentInput;
        }

        private void CalculatorForm_KeyDown(object? sender, KeyEventArgs e)
        {
            string key = "";
            
            if (char.IsDigit((char)e.KeyCode))
                key = ((char)e.KeyCode).ToString();
            else if (e.KeyCode == Keys.Add)
                key = "+";
            else if (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.OemMinus)
                key = "-";
            else if (e.KeyCode == Keys.Multiply)
                key = "*";
            else if (e.KeyCode == Keys.Divide || e.KeyCode == Keys.OemQuestion)
                key = "/";
            else if (e.KeyCode == Keys.Decimal || e.KeyCode == Keys.OemPeriod)
                key = ".";
            else if (e.KeyCode == Keys.Return)
                key = "=";
            else if (e.KeyCode == Keys.Back)
                key = "DEL";
            else if (e.KeyCode == Keys.Delete)
                key = "AC";

            if (!string.IsNullOrEmpty(key))
            {
                // Trigger button click
                foreach (Control ctrl in ((TableLayoutPanel)this.Controls[1]).Controls)
                {
                    if (ctrl is Button btn && (btn.Tag as string) == key)
                    {
                        btn.PerformClick();
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        break;
                    }
                }
            }
        }
    }
}
