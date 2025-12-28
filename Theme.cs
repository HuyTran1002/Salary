using System;
using System.Drawing;
using System.Windows.Forms;

namespace SalaryCalculator
{
    public static class Theme
    {
        private static readonly Color Background = Color.FromArgb(15, 23, 32); // very dark
        private static readonly Color PanelBackground = Color.FromArgb(23, 31, 40);
        private static readonly Color Accent = Color.FromArgb(0, 181, 204); // cyan-teal
        private static readonly Color AccentDark = Color.FromArgb(0, 150, 170);
        private static readonly Color TextPrimary = Color.FromArgb(230, 230, 235);
        private static readonly Color TextSecondary = Color.FromArgb(180, 190, 200);

        // Gaming theme colors
        private static readonly Color GameBackground = Color.FromArgb(8, 8, 12);
        private static readonly Color GamePanel = Color.FromArgb(18, 18, 24);
        private static readonly Color GameAccent = Color.FromArgb(255, 45, 85); // neon red-pink
        private static readonly Color GameAccent2 = Color.FromArgb(120, 40, 255); // neon purple

        public static void ApplyModernTheme(Form form)
        {
            try
            {
                form.SuspendLayout();
                form.BackColor = Background;
                form.ForeColor = TextPrimary;
                form.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);

                ApplyToControls(form.Controls);

                // Tweak DataGridViews at form level
                foreach (Control c in FindControlsOfType<DataGridView>(form))
                {
                    StyleDataGridView((DataGridView)c);
                }

                // Tweak progress bars
                foreach (Control c in FindControlsOfType<ProgressBar>(form))
                {
                    c.ForeColor = Accent;
                    c.BackColor = PanelBackground;
                }

                form.ResumeLayout();
            }
            catch { }
        }

        public static void ApplyGamingTheme(Form form)
        {
            try
            {
                form.SuspendLayout();
                form.BackColor = GameBackground;
                form.ForeColor = TextPrimary;
                form.Font = new Font("Orbitron", 9F, FontStyle.Regular, GraphicsUnit.Point);

                ApplyGamingToControls(form.Controls);

                foreach (Control c in FindControlsOfType<DataGridView>(form))
                {
                    StyleDataGridView((DataGridView)c);
                }

                // Start subtle animated gradient on form using a Timer
                var timer = new Timer();
                timer.Interval = 80;
                int phase = 0;
                timer.Tick += (s, e) => {
                    phase = (phase + 1) % 360;
                    int r = (int)(8 + 12 * Math.Abs(Math.Sin(phase * Math.PI / 180)));
                    int g = (int)(8 + 8 * Math.Abs(Math.Cos(phase * Math.PI / 180)));
                    int b = 16;
                    form.BackColor = Color.FromArgb(r, g, b);
                };
                timer.Start();

                form.ResumeLayout();
            }
            catch { }
        }

        // E-commerce / Shopee-like warm theme
        public static void ApplyEcommerceTheme(Form form)
        {
            try
            {
                form.SuspendLayout();
                // Shopee-like palette: vibrant orange + white cards
                Color bg = Color.FromArgb(250, 250, 252);
                Color card = Color.FromArgb(255, 255, 255);
                Color primary = Color.FromArgb(255, 90, 0); // orange
                Color primaryDark = Color.FromArgb(220, 70, 0);
                Color muted = Color.FromArgb(102, 102, 102);

                form.BackColor = bg;
                form.ForeColor = Color.FromArgb(34,34,34);
                form.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);

                ApplyEcommerceToControls(form.Controls, card, primary, muted);

                foreach (Control c in FindControlsOfType<DataGridView>(form))
                {
                    StyleDataGridView((DataGridView)c);
                }

                form.ResumeLayout();
            }
            catch { }
        }

        private static void ApplyEcommerceToControls(Control.ControlCollection controls, Color cardBg, Color primary, Color muted)
        {
            foreach (Control ctrl in controls)
            {
                if (ctrl is Panel pnl)
                {
                    pnl.BackColor = cardBg;
                    pnl.Padding = new Padding(8);
                    pnl.BorderStyle = BorderStyle.None;
                }
                else if (ctrl is GroupBox gb)
                {
                    gb.BackColor = cardBg;
                    gb.ForeColor = Color.FromArgb(34,34,34);
                    gb.Font = new Font(gb.Font.FontFamily, gb.Font.Size, FontStyle.Bold);
                }
                else if (ctrl is Button btn)
                {
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    btn.BackColor = primary;
                    btn.ForeColor = Color.White;
                    btn.Font = new Font(btn.Font.FontFamily, Math.Max(9, btn.Font.Size), FontStyle.Bold);

                    // Add hover animation handlers (smooth color and slight scale)
                    AttachEcommerceHoverEffects(btn, primary);
                }
                else if (ctrl is Label lab)
                {
                    lab.ForeColor = Color.FromArgb(34,34,34);
                }
                else if (ctrl is TextBox tb)
                {
                    tb.BackColor = Color.White;
                    tb.ForeColor = Color.FromArgb(34,34,34);
                    tb.BorderStyle = BorderStyle.FixedSingle;
                }

                if (ctrl.HasChildren)
                    ApplyEcommerceToControls(ctrl.Controls, cardBg, primary, muted);
            }
        }

        private static void AttachEcommerceHoverEffects(Button btn, Color primary)
        {
            // avoid multiple attachments — use AccessibleName to mark attachment without clobbering Tag
            if (btn.AccessibleName == "ecom-hover") return;
            btn.AccessibleName = "ecom-hover";

            Color normal = btn.BackColor;
            Color hover = ControlPaint.Light(primary, 0.15f);
            float normalFont = btn.Font.Size;
            float hoverFont = normalFont + 1.5f;

            System.Windows.Forms.Timer animTimer = new System.Windows.Forms.Timer();
            animTimer.Interval = 15;
            float t = 0f; // 0..1 progress
            bool hovering = false;

            animTimer.Tick += (s, e) => {
                if (hovering) { t = Math.Min(1f, t + 0.12f); }
                else { t = Math.Max(0f, t - 0.12f); }

                // lerp color
                int r = (int)(normal.R + (hover.R - normal.R) * t);
                int g = (int)(normal.G + (hover.G - normal.G) * t);
                int b = (int)(normal.B + (hover.B - normal.B) * t);
                btn.BackColor = Color.FromArgb(r, g, b);

                // scale font by changing size (not perfect but visible)
                try
                {
                    btn.Font = new Font(btn.Font.FontFamily, normalFont + (hoverFont - normalFont) * t, btn.Font.Style);
                }
                catch { }

                // subtle shadow using FlatAppearance when fully hovered
                if (t > 0.6f)
                {
                    btn.FlatAppearance.BorderSize = 0;
                }

                if (t == 0f && !hovering)
                {
                    animTimer.Stop();
                }
            };

            btn.MouseEnter += (s, e) => {
                hovering = true;
                animTimer.Start();
            };
            btn.MouseLeave += (s, e) => {
                hovering = false;
                animTimer.Start();
            };
        }

        private static void ApplyToControls(Control.ControlCollection controls)
        {
            foreach (Control ctrl in controls)
            {
                if (ctrl is Panel || ctrl is GroupBox || ctrl is TableLayoutPanel)
                {
                    ctrl.BackColor = PanelBackground;
                    ctrl.ForeColor = TextPrimary;
                }
                else if (ctrl is Button btn)
                {
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    btn.BackColor = Accent;
                    btn.ForeColor = Color.White;
                    btn.Font = new Font(btn.Font.FontFamily, Math.Max(9, btn.Font.Size), FontStyle.Bold);
                }
                else if (ctrl is Label lab)
                {
                    lab.ForeColor = TextPrimary;
                    // smaller secondary color for non-bold labels
                    if (!lab.Font.Bold)
                        lab.ForeColor = TextSecondary;
                }
                else if (ctrl is TextBox tb)
                {
                    tb.BackColor = Color.FromArgb(28, 36, 44);
                    tb.ForeColor = TextPrimary;
                    tb.BorderStyle = BorderStyle.FixedSingle;
                }
                else if (ctrl is ComboBox cb)
                {
                    cb.BackColor = Color.FromArgb(28, 36, 44);
                    cb.ForeColor = TextPrimary;
                    cb.FlatStyle = FlatStyle.Flat;
                }
                else if (ctrl is CheckBox chk)
                {
                    chk.ForeColor = TextPrimary;
                    chk.BackColor = PanelBackground;
                }
                else if (ctrl is RadioButton rb)
                {
                    rb.ForeColor = TextPrimary;
                    rb.BackColor = PanelBackground;
                }

                // Recursively apply
                if (ctrl.HasChildren)
                    ApplyToControls(ctrl.Controls);
            }
        }

        private static void ApplyGamingToControls(Control.ControlCollection controls)
        {
            foreach (Control ctrl in controls)
            {
                if (ctrl is Panel || ctrl is GroupBox || ctrl is TableLayoutPanel)
                {
                    ctrl.BackColor = GamePanel;
                    ctrl.ForeColor = TextPrimary;
                }
                else if (ctrl is Button btn)
                {
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    btn.BackColor = GameAccent;
                    btn.ForeColor = Color.White;
                    btn.Font = new Font(btn.Font.FontFamily, Math.Max(9, btn.Font.Size), FontStyle.Bold);

                    // Add glow effect using shadow label behind button (best-effort)
                    var shadow = new Label();
                    shadow.Text = "";
                    shadow.BackColor = Color.Transparent;
                    shadow.AutoSize = false;
                    shadow.Size = btn.Size;
                    shadow.Location = new Point(btn.Location.X - 2, btn.Location.Y - 2);
                    shadow.Parent = btn.Parent;
                    shadow.SendToBack();
                }
                else if (ctrl is Label lab)
                {
                    lab.ForeColor = TextPrimary;
                    if (!lab.Font.Bold)
                        lab.ForeColor = TextSecondary;
                }
                else if (ctrl is TextBox tb)
                {
                    tb.BackColor = Color.FromArgb(12, 12, 18);
                    tb.ForeColor = TextPrimary;
                    tb.BorderStyle = BorderStyle.FixedSingle;
                }

                if (ctrl.HasChildren)
                    ApplyGamingToControls(ctrl.Controls);
            }
        }

        private static void StyleDataGridView(DataGridView dgv)
        {
            dgv.EnableHeadersVisualStyles = false;
            dgv.BackgroundColor = PanelBackground;
            dgv.GridColor = Color.FromArgb(40, 48, 58);
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(20, 30, 36);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = TextPrimary;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            // Prevent header wrapping and lock header height so titles stay on one line
            dgv.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgv.ColumnHeadersHeight = 36;
            dgv.RowHeadersVisible = false;
            dgv.DefaultCellStyle.BackColor = PanelBackground;
            dgv.DefaultCellStyle.ForeColor = TextPrimary;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(20, 26, 32);
            dgv.DefaultCellStyle.SelectionBackColor = AccentDark;
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
        }

        private static System.Collections.Generic.IEnumerable<Control> FindControlsOfType<T>(Control parent) where T : Control
        {
            var list = new System.Collections.Generic.List<Control>();
            foreach (Control c in parent.Controls)
            {
                if (c is T) list.Add(c);
                if (c.HasChildren)
                    list.AddRange(FindControlsOfType<T>(c));
            }
            return list;
        }
    }
}
