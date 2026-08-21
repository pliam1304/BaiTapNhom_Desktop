namespace EduPath.WinForms.Common
{
    /// <summary>Tương đương hàm `grid()` trong mockup JS: tạo bảng dữ liệu đồng bộ style toàn app.</summary>
    public static class GridHelper
    {
        public static DataGridView MakeGrid()
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = UiTheme.FontBase
            };
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#eef3f8");
            grid.ColumnHeadersDefaultCellStyle.ForeColor = ColorTranslator.FromHtml("#45566c");
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grid.ColumnHeadersHeight = 34;
            grid.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#dbeafe");
            grid.DefaultCellStyle.SelectionForeColor = UiTheme.TextDark;
            grid.RowTemplate.Height = 32;
            return grid;
        }

        public static Panel MakeCard()
        {
            return new Panel
            {
                BackColor = Color.White,
                Padding = new Padding(14),
                Margin = new Padding(0, 0, 0, 12),
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        public static Panel MakeKpiCard(string label, string value, Color? valueColor = null)
        {
            var card = MakeCard();
            card.Width = 230;
            card.Height = 85;

            var lbl = new Label { Text = label, ForeColor = UiTheme.TextMuted, Font = new Font("Segoe UI", 8F), AutoSize = true, Location = new Point(14, 12) };
            var val = new Label { Text = value, ForeColor = valueColor ?? UiTheme.TextDark, Font = new Font("Segoe UI", 18F, FontStyle.Bold), AutoSize = true, Location = new Point(14, 32) };
            card.Controls.Add(lbl);
            card.Controls.Add(val);
            return card;
        }
    }
}
