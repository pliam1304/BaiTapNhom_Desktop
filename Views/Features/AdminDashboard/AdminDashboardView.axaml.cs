using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;

namespace EduPath.Avalonia.Views
{
    public partial class AdminDashboardView : UserControl
    {
        public AdminDashboardView()
        {
            InitializeComponent();
            Loaded += (_, _) => AnimateKpiCards();
        }

        private void AnimateKpiCards()
        {
            var row = this.FindControl<UniformGrid>("KpiRow");
            if (row is null) return;

            int i = 0;
            foreach (var child in row.Children)
            {
                if (child is not Border border) continue;
                var delay = TimeSpan.FromMilliseconds(60 * i++);
                DispatcherTimer.RunOnce(() => border.Classes.Add("in"), delay);
            }
        }
    }
}
