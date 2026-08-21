namespace EduPath.WinForms.Common
{
    /// <summary>Bảng màu lấy đúng theo mockup HTML gốc (#09213e navy, #f4bd3c/#e8a91f gold...).</summary>
    public static class UiTheme
    {
        public static readonly Color Navy = ColorTranslator.FromHtml("#09213e");
        public static readonly Color NavyLight = ColorTranslator.FromHtml("#173d65");
        public static readonly Color Gold = ColorTranslator.FromHtml("#e8a91f");
        public static readonly Color GoldText = ColorTranslator.FromHtml("#f4bd3c");
        public static readonly Color Background = ColorTranslator.FromHtml("#eef2f7");
        public static readonly Color CardBackground = Color.White;
        public static readonly Color Border = ColorTranslator.FromHtml("#dce5ef");
        public static readonly Color TextMuted = ColorTranslator.FromHtml("#64748b");
        public static readonly Color TextDark = ColorTranslator.FromHtml("#17233b");
        public static readonly Color BadgeGreenBg = ColorTranslator.FromHtml("#e9f7f1");
        public static readonly Color BadgeGreenText = ColorTranslator.FromHtml("#167255");
        public static readonly Color BadgeWarnBg = ColorTranslator.FromHtml("#fff4d8");
        public static readonly Color BadgeWarnText = ColorTranslator.FromHtml("#9d6800");
        public static readonly Color BadgeOffBg = ColorTranslator.FromHtml("#f2f4f7");
        public static readonly Color BadgeOffText = ColorTranslator.FromHtml("#667085");

        public static readonly Font FontBase = new("Segoe UI", 9F);
        public static readonly Font FontHeading = new("Segoe UI", 14F, FontStyle.Bold);
        public static readonly Font FontBrand = new("Segoe UI", 15F, FontStyle.Bold);

        public static Button MakeYellowButton(string text)
        {
            var btn = new Button
            {
                Text = text,
                BackColor = Gold,
                ForeColor = Navy,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                AutoSize = true,
                Padding = new Padding(10, 6, 10, 6),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        public static Button MakeOutlineButton(string text)
        {
            var btn = new Button
            {
                Text = text,
                BackColor = Color.White,
                ForeColor = TextDark,
                FlatStyle = FlatStyle.Flat,
                AutoSize = true,
                Padding = new Padding(8, 5, 8, 5),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = ColorTranslator.FromHtml("#94a3b8");
            btn.FlatAppearance.BorderSize = 1;
            return btn;
        }

        public static Label MakeBadge(string text, string kind = "ok")
        {
            var (bg, fg) = kind switch
            {
                "warn" => (BadgeWarnBg, BadgeWarnText),
                "off" => (BadgeOffBg, BadgeOffText),
                _ => (BadgeGreenBg, BadgeGreenText)
            };
            return new Label
            {
                Text = text,
                BackColor = bg,
                ForeColor = fg,
                AutoSize = true,
                Padding = new Padding(8, 3, 8, 3),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold)
            };
        }
    }
}
