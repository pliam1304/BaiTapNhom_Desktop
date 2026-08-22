using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;

namespace EduPath.Avalonia.Views
{
    public partial class StudentDashboardView : UserControl
    {
        public StudentDashboardView()
        {
            InitializeComponent();
            Loaded += (_, _) => AnimateKpiCards();
        }

        /// <summary>Gắn class "in" cho từng thẻ KPI theo độ trễ tăng dần để tạo hiệu ứng xuất hiện so le.</summary>
        private void AnimateKpiCards()
        {
            var row = this.FindControl<UniformGrid>("KpiRow");
            if (row is null) return;

            int i = 0;
            foreach (var child in row.Children)
            {
                if (child is not Border border) continue;
                var delay = TimeSpan.FromMilliseconds(70 * i++);
                DispatcherTimer.RunOnce(() => border.Classes.Add("in"), delay);
            }
        }
    }
}
