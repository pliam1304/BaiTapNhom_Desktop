using Avalonia;
using Avalonia.Controls;

namespace EduPath.Avalonia.Behaviors
{
    public static class ClassBinding
    {
        public static readonly AttachedProperty<string?> ValueProperty =
            AvaloniaProperty.RegisterAttached<StyledElement, string?>(
                "Value", typeof(ClassBinding));

        static ClassBinding()
        {
            ValueProperty.Changed.AddClassHandler<StyledElement>(OnValueChanged);
        }

        public static void SetValue(StyledElement element, string? value) =>
            element.SetValue(ValueProperty, value);

        public static string? GetValue(StyledElement element) =>
            element.GetValue(ValueProperty);

        private static void OnValueChanged(StyledElement element, AvaloniaPropertyChangedEventArgs e)
        {
            element.Classes.Clear();
            if (e.NewValue is string value && !string.IsNullOrWhiteSpace(value))
            {
                foreach (var name in value.Split(' ', System.StringSplitOptions.RemoveEmptyEntries))
                {
                    element.Classes.Add(name);
                }
            }
        }
    }
}