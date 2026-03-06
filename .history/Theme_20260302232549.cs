using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
using System.Media;

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
        private static readonly HashSet<IntPtr> AnimatedForms = new HashSet<IntPtr>();
        private static readonly HashSet<Button> SoundWiredButtons = new HashSet<Button>();

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
                Color bg = Color.FromArgb(244, 247, 252);
                Color card = Color.FromArgb(255, 255, 255);
                Color primary = Color.FromArgb(37, 99, 235);
                Color primaryHover = Color.FromArgb(29, 78, 216);
                Color danger = Color.FromArgb(220, 38, 38);
                Color neutral = Color.FromArgb(71, 85, 105);
                Color muted = Color.FromArgb(100, 116, 139);

                form.BackColor = bg;
                form.ForeColor = Color.FromArgb(15, 23, 42);
                form.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);

                ApplyEcommerceToControls(form.Controls, card, primary, primaryHover, danger, neutral, muted);

                foreach (Control c in FindControlsOfType<DataGridView>(form))
                {
                    StyleLightDataGridView((DataGridView)c);
                }

                EnableDoubleBuffer(form);
                form.ResumeLayout();
            }
            catch { }
        }

        public static void ApplyInfinityGlassTheme(Form form)
        {
            try
            {
                form.SuspendLayout();

                form.BackColor = Color.FromArgb(239, 247, 255);
                form.ForeColor = Color.FromArgb(28, 44, 78);
                form.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);

                ApplyInfinityGlassToControls(form.Controls);

                foreach (Control c in FindControlsOfType<DataGridView>(form))
                {
                    StyleInfinityGlassDataGridView((DataGridView)c);
                }

                ApplyInfinityBackdrop(form, 0);
                StartInfinityBackgroundAnimation(form);
                EnableDoubleBuffer(form);

                form.ResumeLayout();
            }
            catch { }
        }

        private static void ApplyEcommerceToControls(Control.ControlCollection controls, Color cardBg, Color primary, Color primaryHover, Color danger, Color neutral, Color muted)
        {
            foreach (Control ctrl in controls)
            {
                ApplySegoeUIFont(ctrl);

                if (ctrl is Panel pnl)
                {
                    pnl.BackColor = cardBg;
                    pnl.Padding = new Padding(12);
                    pnl.BorderStyle = BorderStyle.None;
                    EnableDoubleBuffer(pnl);
                }
                else if (ctrl is GroupBox gb)
                {
                    gb.BackColor = cardBg;
                    gb.ForeColor = Color.FromArgb(30, 41, 59);
                    gb.Font = new Font("Segoe UI", gb.Font.Size, FontStyle.Bold);
                }
                else if (ctrl is Button btn)
                {
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    btn.ForeColor = Color.White;
                    btn.Font = new Font("Segoe UI", Math.Max(9, btn.Font.Size), FontStyle.Bold);
                    btn.Height = Math.Max(btn.Height, 32);

                    Color buttonColor = primary;
                    if (btn.Text.IndexOf("ĐĂNG XUẤT", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        btn.Text.IndexOf("HỦY", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        btn.Text.IndexOf("XÓA", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        buttonColor = danger;
                    }
                    else if (btn.Text.Contains("✏️"))
                    {
                        buttonColor = neutral;
                    }

                    btn.BackColor = buttonColor;
                    AttachEcommerceHoverEffects(btn, buttonColor, primaryHover);
                    AttachCuteClickSound(btn);
                }
                else if (ctrl is Label lab)
                {
                    bool isHeading = lab.Font.Bold ||
                                     lab.Name.IndexOf("Title", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                     lab.Text.All(ch => !char.IsLetter(ch) || char.IsUpper(ch));
                    lab.ForeColor = isHeading ? Color.FromArgb(30, 41, 59) : muted;
                }
                else if (ctrl is TextBox tb)
                {
                    tb.BackColor = Color.White;
                    tb.ForeColor = Color.FromArgb(15, 23, 42);
                    tb.BorderStyle = BorderStyle.FixedSingle;
                }
                else if (ctrl is CheckBox cb)
                {
                    cb.ForeColor = Color.FromArgb(51, 65, 85);
                    cb.BackColor = cardBg;
                }
                else if (ctrl is ComboBox combo)
                {
                    combo.BackColor = Color.White;
                    combo.ForeColor = Color.FromArgb(15, 23, 42);
                    combo.FlatStyle = FlatStyle.Flat;
                }

                if (ctrl.HasChildren)
                    ApplyEcommerceToControls(ctrl.Controls, cardBg, primary, primaryHover, danger, neutral, muted);
            }
        }

        private static void AttachEcommerceHoverEffects(Button btn, Color normalColor, Color defaultHover)
        {
            if (btn.AccessibleName == "ecom-hover") return;
            btn.AccessibleName = "ecom-hover";

            Color hoverColor = defaultHover;
            if (normalColor.R > 180 && normalColor.G < 80)
            {
                hoverColor = ControlPaint.Dark(normalColor, 0.1f);
            }

            btn.MouseEnter += (s, e) => {
                btn.BackColor = hoverColor;
            };
            btn.MouseLeave += (s, e) => {
                btn.BackColor = normalColor;
            };
        }

        private static void ApplyInfinityGlassToControls(Control.ControlCollection controls)
        {
            foreach (Control ctrl in controls)
            {
                ApplySegoeUIFont(ctrl);

                if (ctrl is Panel panel)
                {
                    panel.BackColor = Color.Transparent;
                    panel.Padding = new Padding(Math.Max(panel.Padding.Left, 12), Math.Max(panel.Padding.Top, 12), Math.Max(panel.Padding.Right, 12), Math.Max(panel.Padding.Bottom, 12));
                    panel.BorderStyle = BorderStyle.None;
                    EnableDoubleBuffer(panel);
                }
                else if (ctrl is GroupBox gb)
                {
                    gb.BackColor = Color.FromArgb(242, 248, 255);
                    gb.ForeColor = Color.FromArgb(31, 56, 99);
                    gb.Font = new Font("Segoe UI", gb.Font.Size, FontStyle.Bold);
                }
                else if (ctrl is Button btn)
                {
                    bool isCompactIconButton = btn.Width <= 44 || btn.Text.Contains("✏") || btn.Text.Contains("🧮");

                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 1;
                    btn.FlatAppearance.BorderColor = Color.FromArgb(140, 180, 255);
                    btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 212, 255);
                    btn.ForeColor = Color.White;
                    btn.Font = isCompactIconButton
                        ? new Font("Segoe UI Emoji", Math.Max(10, btn.Font.Size), FontStyle.Regular)
                        : new Font("Segoe UI", Math.Max(9, btn.Font.Size), FontStyle.Bold);
                    btn.Cursor = Cursors.Hand;
                    btn.Padding = isCompactIconButton ? new Padding(0) : new Padding(10, 4, 10, 4);
                    if (!isCompactIconButton)
                    {
                        btn.Height = Math.Max(btn.Height, 32);
                    }

                    Color baseColor = Color.FromArgb(54, 168, 255);
                    Color hoverColor = Color.FromArgb(98, 190, 255);

                    if (btn.Text.IndexOf("ĐĂNG XUẤT", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        btn.Text.IndexOf("HỦY", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        baseColor = Color.FromArgb(128, 110, 214);
                        hoverColor = Color.FromArgb(148, 130, 230);
                    }
                    else if (btn.Text.IndexOf("TÍNH", StringComparison.OrdinalIgnoreCase) >= 0 ||
                             btn.Text.IndexOf("LƯU", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        baseColor = Color.FromArgb(0, 201, 190);
                        hoverColor = Color.FromArgb(48, 218, 205);
                    }
                    else if (btn.Text.Contains("✏️"))
                    {
                        baseColor = Color.FromArgb(106, 134, 255);
                        hoverColor = Color.FromArgb(128, 154, 255);
                    }

                    btn.BackColor = baseColor;
                    AttachGlassHoverEffects(btn, baseColor, hoverColor);
                    AttachCuteClickSound(btn);
                }
                else if (ctrl is Label label)
                {
                    bool isTitle = label.Font.Bold ||
                                   label.Name.IndexOf("Title", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                   label.Text.All(ch => !char.IsLetter(ch) || char.IsUpper(ch));
                    label.ForeColor = isTitle ? Color.FromArgb(34, 62, 110) : Color.FromArgb(74, 98, 150);
                    if (!string.IsNullOrWhiteSpace(label.Text) && label.Text.Contains("━━━━━━━━"))
                    {
                        label.ForeColor = Color.FromArgb(155, 174, 220);
                    }
                }
                else if (ctrl is TextBox tb)
                {
                    tb.BorderStyle = BorderStyle.FixedSingle;
                    tb.ForeColor = Color.FromArgb(33, 55, 94);
                    tb.BackColor = tb.ReadOnly
                        ? Color.FromArgb(227, 236, 250)
                        : Color.FromArgb(248, 252, 255);
                }
                else if (ctrl is CheckBox cb)
                {
                    cb.ForeColor = Color.FromArgb(48, 78, 128);
                    cb.BackColor = Color.Transparent;
                }
                else if (ctrl is ComboBox combo)
                {
                    combo.BackColor = Color.FromArgb(248, 252, 255);
                    combo.ForeColor = Color.FromArgb(33, 55, 94);
                    combo.FlatStyle = FlatStyle.Flat;
                }

                if (ctrl.HasChildren)
                {
                    ApplyInfinityGlassToControls(ctrl.Controls);
                }
            }
        }

        private static void AttachGlassHoverEffects(Button btn, Color normalColor, Color hoverColor)
        {
            if (btn.AccessibleDescription == "glass-hover") return;
            btn.AccessibleDescription = "glass-hover";

            bool isCompactIconButton = btn.Width <= 44 || btn.Text.Contains("✏") || btn.Text.Contains("🧮");
            Color normalBorder = btn.FlatAppearance.BorderColor;
            var hoverBorder = ControlPaint.Light(hoverColor, 0.2f);

            var transitionTimer = new Timer();
            transitionTimer.Interval = 16;
            float hoverProgress = 0f;
            bool isHovering = false;

            var sweepTimer = new Timer();
            sweepTimer.Interval = 16;
            int sweepX = -btn.Width;

            transitionTimer.Tick += (s, e) =>
            {
                if (btn.IsDisposed || !btn.IsHandleCreated)
                {
                    try { transitionTimer.Stop(); } catch { }
                    return;
                }

                float target = isHovering ? 1f : 0f;
                const float speed = 0.14f;

                if (hoverProgress < target)
                {
                    hoverProgress = Math.Min(target, hoverProgress + speed);
                }
                else if (hoverProgress > target)
                {
                    hoverProgress = Math.Max(target, hoverProgress - speed);
                }

                btn.BackColor = BlendColor(normalColor, hoverColor, hoverProgress);
                btn.Invalidate();

                if (Math.Abs(hoverProgress - target) < 0.001f)
                {
                    try { transitionTimer.Stop(); } catch { }
                }
            };

            sweepTimer.Tick += (s, e) =>
            {
                if (btn.IsDisposed || !btn.IsHandleCreated)
                {
                    try { sweepTimer.Stop(); } catch { }
                    return;
                }

                sweepX += Math.Max(5, btn.Width / 22);
                if (sweepX > btn.Width + btn.Width / 2)
                {
                    sweepX = -btn.Width / 2;
                }

                btn.Invalidate();
            };

            btn.Paint += (s, e) =>
            {
                if (isCompactIconButton || !isHovering || hoverProgress <= 0.01f) return;

                int stripeWidth = Math.Max(26, btn.Width / 4);
                int alphaStrong = (int)(72f * hoverProgress);
                int alphaSoft = (int)(36f * hoverProgress);

                var stripeRect = new Rectangle(sweepX, 0, stripeWidth, btn.Height);
                using (var stripeBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    stripeRect,
                    Color.FromArgb(0, 255, 255, 255),
                    Color.FromArgb(alphaStrong, 255, 255, 255),
                    0f))
                {
                    e.Graphics.FillRectangle(stripeBrush, stripeRect);
                }

                var glowRect = new Rectangle(Math.Max(0, sweepX - stripeWidth / 2), 0, stripeWidth, btn.Height);
                using (var glowBrush = new SolidBrush(Color.FromArgb(alphaSoft, 255, 255, 255)))
                {
                    e.Graphics.FillRectangle(glowBrush, glowRect);
                }
            };

            btn.MouseEnter += (s, e) =>
            {
                isHovering = true;
                btn.FlatAppearance.BorderColor = hoverBorder;
                if (!transitionTimer.Enabled)
                {
                    try { transitionTimer.Start(); } catch { }
                }

                if (!isCompactIconButton && !sweepTimer.Enabled)
                {
                    sweepX = -btn.Width / 2;
                    try { sweepTimer.Start(); } catch { }
                }
            };
            btn.MouseLeave += (s, e) =>
            {
                isHovering = false;
                btn.FlatAppearance.BorderColor = normalBorder;
                if (!transitionTimer.Enabled)
                {
                    try { transitionTimer.Start(); } catch { }
                }
                try { sweepTimer.Stop(); } catch { }
                btn.Invalidate();
            };

            btn.Disposed += (s, e) =>
            {
                try { transitionTimer.Stop(); } catch { }
                try { transitionTimer.Dispose(); } catch { }
                try { sweepTimer.Stop(); } catch { }
                try { sweepTimer.Dispose(); } catch { }
            };
        }

        private static Color BlendColor(Color from, Color to, float t)
        {
            if (t < 0f) t = 0f;
            if (t > 1f) t = 1f;

            int a = from.A + (int)((to.A - from.A) * t);
            int r = from.R + (int)((to.R - from.R) * t);
            int g = from.G + (int)((to.G - from.G) * t);
            int b = from.B + (int)((to.B - from.B) * t);

            return Color.FromArgb(a, r, g, b);
        }

        private static void StyleInfinityGlassDataGridView(DataGridView dgv)
        {
            dgv.EnableHeadersVisualStyles = false;
            dgv.BackgroundColor = Color.FromArgb(243, 249, 255);
            dgv.GridColor = Color.FromArgb(189, 211, 245);
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(106, 162, 255);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgv.ColumnHeadersHeight = 38;
            dgv.RowHeadersVisible = false;
            dgv.DefaultCellStyle.BackColor = Color.FromArgb(248, 252, 255);
            dgv.DefaultCellStyle.ForeColor = Color.FromArgb(40, 62, 104);
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(237, 246, 255);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(162, 196, 255);
            dgv.DefaultCellStyle.SelectionForeColor = Color.FromArgb(24, 47, 89);
        }

        private static void ApplyInfinityBackdrop(Form form, int phase)
        {
            try
            {
                int width = Math.Max(form.ClientSize.Width, 1);
                int height = Math.Max(form.ClientSize.Height, 1);

                var background = new Bitmap(width, height);
                using (var g = Graphics.FromImage(background))
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                    using (var baseGradient = new System.Drawing.Drawing2D.LinearGradientBrush(
                        new Rectangle(0, 0, width, height),
                        Color.FromArgb(255, 246, 252),
                        Color.FromArgb(230, 248, 255),
                        100f))
                    {
                        g.FillRectangle(baseGradient, 0, 0, width, height);
                    }

                    using (var sweep = new System.Drawing.Drawing2D.LinearGradientBrush(
                        new Rectangle(0, 0, width, height),
                        Color.FromArgb(78, 255, 184, 232),
                        Color.FromArgb(68, 176, 220, 255),
                        35f))
                    {
                        g.FillRectangle(sweep, 0, 0, width, height);
                    }

                    using (var sunlight = new System.Drawing.Drawing2D.PathGradientBrush(new Point[]
                    {
                        new Point(width / 3, height / 12),
                        new Point(width, 0),
                        new Point(width, height / 2),
                        new Point(width / 2, height / 2)
                    }))
                    {
                        sunlight.CenterColor = Color.FromArgb(96, 255, 255, 255);
                        sunlight.SurroundColors = new[] { Color.FromArgb(0, 255, 255, 255), Color.FromArgb(0, 255, 255, 255), Color.FromArgb(0, 255, 255, 255), Color.FromArgb(0, 255, 255, 255) };
                        g.FillRectangle(sunlight, new Rectangle(0, 0, width, height / 2 + 40));
                    }

                    int driftA = (int)(Math.Sin(phase * Math.PI / 180.0) * 26);
                    int driftB = (int)(Math.Cos((phase + 60) * Math.PI / 180.0) * 28);
                    int driftC = (int)(Math.Sin((phase + 130) * Math.PI / 180.0) * 22);

                    using (var orb1 = new SolidBrush(Color.FromArgb(66, 255, 182, 219)))
                    {
                        g.FillEllipse(orb1, -width / 8 + driftA, height / 7, width / 2, height / 2);
                    }

                    using (var orb2 = new SolidBrush(Color.FromArgb(72, 170, 220, 255)))
                    {
                        g.FillEllipse(orb2, width / 2 + driftB, -height / 6, width / 2, height / 2);
                    }

                    using (var orb3 = new SolidBrush(Color.FromArgb(64, 255, 196, 242)))
                    {
                        g.FillEllipse(orb3, width / 5 + driftC, height / 2, width * 2 / 3, height / 3);
                    }

                    using (var streakPen = new Pen(Color.FromArgb(58, 255, 255, 255), 1.5f))
                    {
                        int streakShift = (phase * 2) % Math.Max(width, 1);
                        g.DrawLine(streakPen, -width / 4 + streakShift, 0, width / 5 + streakShift, height);
                        g.DrawLine(streakPen, width / 4 + streakShift / 2, 0, width / 2 + streakShift / 2, height);
                    }

                    int cloudFloat = (int)(Math.Sin((phase + 40) * Math.PI / 180.0) * 10);
                    DrawCloud(g, width / 12 + driftA / 2, height / 10 + cloudFloat, width / 4, height / 9, Color.FromArgb(166, 255, 255, 255));
                    DrawCloud(g, width / 2 + driftB / 2, height / 6 - cloudFloat / 2, width / 4, height / 9, Color.FromArgb(154, 255, 255, 255));
                    DrawCloud(g, width - width / 3 + driftC / 2, height / 4 + cloudFloat / 2, width / 5, height / 10, Color.FromArgb(144, 255, 255, 255));
                    DrawCloud(g, width / 3 - driftB / 3, height / 3 + cloudFloat / 3, width / 5, height / 10, Color.FromArgb(132, 255, 255, 255));

                    int sparkleSeed = phase / 3;
                    for (int i = 0; i < 34; i++)
                    {
                        int sx = (int)((i * 167 + sparkleSeed * 23) % width);
                        int sy = (int)((i * 149 + sparkleSeed * 19) % height);
                        int alpha = 72 + (int)(94 * Math.Abs(Math.Sin((phase + i * 9) * Math.PI / 180.0)));

                        if (i % 3 == 0)
                        {
                            DrawTwinkle(g, sx, sy, 4 + (i % 4), Color.FromArgb(alpha, 255, 255, 255));
                        }
                        else
                        {
                            using (var sparkle = new SolidBrush(Color.FromArgb(alpha, 160, 195, 255)))
                            {
                                g.FillEllipse(sparkle, sx, sy, 2 + (i % 3), 2 + (i % 3));
                            }
                        }
                    }

                    for (int i = 0; i < 18; i++)
                    {
                        int hx = (int)((i * 173 + phase * 3 + 80) % Math.Max(width - 20, 1)) + 10;
                        int hy = (int)((i * 131 + phase * 2 + 60) % Math.Max(height - 40, 1)) + 20;
                        int heartSize = 8 + (i % 5);
                        int alpha = 46 + (int)(34 * Math.Abs(Math.Sin((phase + i * 24) * Math.PI / 180.0)));
                        Color heartColor = i % 2 == 0 ? Color.FromArgb(alpha, 255, 140, 196) : Color.FromArgb(alpha, 255, 168, 214);
                        DrawCuteHeart(g, hx, hy, heartSize, heartColor);
                    }

                    for (int i = 0; i < 22; i++)
                    {
                        int bx = (int)((i * 97 + phase * 3) % width);
                        int by = (int)((i * 83 + phase * 2) % height);
                        int bubbleSize = 16 + (i % 5) * 8;
                        using (var bubble = new SolidBrush(Color.FromArgb(30 + i % 4 * 10, 255, 255, 255)))
                        {
                            g.FillEllipse(bubble, bx, by, bubbleSize, bubbleSize);
                        }
                    }

                    using (var glowPen = new Pen(Color.FromArgb(48, 255, 181, 225), 2.2f))
                    {
                        int loop = (phase * 4) % Math.Max(width, 1);
                        g.DrawBezier(glowPen,
                            new Point(-80 + loop, height - height / 3),
                            new Point(width / 5 + loop / 2, height / 2),
                            new Point(width / 2 + loop / 3, height - height / 4),
                            new Point(width + 40, height / 3));
                    }
                }

                var oldImage = form.BackgroundImage;
                form.BackgroundImage = background;
                form.BackgroundImageLayout = ImageLayout.Stretch;
                if (oldImage != null)
                {
                    try { oldImage.Dispose(); } catch { }
                }
            }
            catch { }
        }

        private static void DrawCloud(Graphics g, int x, int y, int width, int height, Color color)
        {
            using (var brush = new SolidBrush(color))
            {
                g.FillEllipse(brush, x, y + height / 4, width / 2, height / 2);
                g.FillEllipse(brush, x + width / 4, y, width / 2, height / 2);
                g.FillEllipse(brush, x + width / 2, y + height / 5, width / 2, height / 2);
                g.FillRectangle(brush, x + width / 6, y + height / 3, width * 2 / 3, height / 2);
            }
        }

        private static void DrawCuteHeart(Graphics g, int centerX, int centerY, int size, Color color)
        {
            int radius = Math.Max(2, size / 2);
            using (var brush = new SolidBrush(color))
            {
                g.FillEllipse(brush, centerX - radius - radius / 2, centerY - radius, radius + 1, radius + 1);
                g.FillEllipse(brush, centerX - radius / 2, centerY - radius, radius + 1, radius + 1);
                Point[] triangle = new[]
                {
                    new Point(centerX - radius - radius / 2 + 1, centerY - radius / 2),
                    new Point(centerX + radius / 2 + 1, centerY - radius / 2),
                    new Point(centerX - radius / 2, centerY + radius + 1)
                };
                g.FillPolygon(brush, triangle);
            }
        }

        private static void DrawTwinkle(Graphics g, int x, int y, int size, Color color)
        {
            using (var pen = new Pen(color, 1.2f))
            {
                g.DrawLine(pen, x - size, y, x + size, y);
                g.DrawLine(pen, x, y - size, x, y + size);
            }
        }

        private static void StartInfinityBackgroundAnimation(Form form)
        {
            try
            {
                if (!form.IsHandleCreated)
                {
                    form.HandleCreated += (s, e) => StartInfinityBackgroundAnimation(form);
                    return;
                }

                if (AnimatedForms.Contains(form.Handle)) return;
                AnimatedForms.Add(form.Handle);

                int phase = 0;
                bool isUserDetailForm = form.GetType().Name == "UserDetailForm";
                int phaseStep = isUserDetailForm ? 1 : 3;
                var timer = new Timer();
                timer.Interval = isUserDetailForm ? 70 : 34;
                timer.Tick += (s, e) =>
                {
                    if (form.IsDisposed || !form.IsHandleCreated)
                    {
                        timer.Stop();
                        try { timer.Dispose(); } catch { }
                        return;
                    }

                    phase = (phase + phaseStep) % 360;
                    ApplyInfinityBackdrop(form, phase);
                };

                form.Disposed += (s, e) =>
                {
                    try { timer.Stop(); } catch { }
                    try { timer.Dispose(); } catch { }
                    try { AnimatedForms.Remove(form.Handle); } catch { }
                };

                timer.Start();
            }
            catch { }
        }

        private static void ApplySegoeUIFont(Control ctrl)
        {
            try
            {
                if (ctrl.Font == null) return;
                if (ctrl is DataGridView) return;
                if (ctrl.Font.FontFamily.Name.IndexOf("Segoe UI", StringComparison.OrdinalIgnoreCase) >= 0) return;
                ctrl.Font = new Font("Segoe UI", ctrl.Font.Size, ctrl.Font.Style, GraphicsUnit.Point);
            }
            catch { }
        }

        private static void EnableDoubleBuffer(Control control)
        {
            try
            {
                var property = typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                property?.SetValue(control, true, null);
            }
            catch { }
        }

        private static void StyleLightDataGridView(DataGridView dgv)
        {
            dgv.EnableHeadersVisualStyles = false;
            dgv.BackgroundColor = Color.White;
            dgv.GridColor = Color.FromArgb(226, 232, 240);
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgv.ColumnHeadersHeight = 36;
            dgv.RowHeadersVisible = false;
            dgv.DefaultCellStyle.BackColor = Color.White;
            dgv.DefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgv.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);
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
                    AttachCuteClickSound(btn);
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
                    AttachCuteClickSound(btn);
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

        private static void AttachCuteClickSound(Button btn)
        {
            if (btn == null) return;
            if (SoundWiredButtons.Contains(btn)) return;

            SoundWiredButtons.Add(btn);

            btn.Click += (s, e) =>
            {
                try
                {
                    SystemSounds.Asterisk.Play();
                }
                catch { }
            };

            btn.Disposed += (s, e) =>
            {
                try { SoundWiredButtons.Remove(btn); } catch { }
            };
        }
    }
}
