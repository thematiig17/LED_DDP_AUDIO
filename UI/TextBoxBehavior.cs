using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace LED_DDP_DRIVER.UI
{
    public static class TextBoxBehavior
    {
        public static readonly DependencyProperty AutoScrollToEndProperty =
            DependencyProperty.RegisterAttached(
                "AutoScrollToEnd",
                typeof(bool),
                typeof(TextBoxBehavior),
                new PropertyMetadata(false, OnAutoScrollToEndChanged));

        public static bool GetAutoScrollToEnd(DependencyObject obj) => (bool)obj.GetValue(AutoScrollToEndProperty);
        public static void SetAutoScrollToEnd(DependencyObject obj, bool value) => obj.SetValue(AutoScrollToEndProperty, value);
        private static void OnAutoScrollToEndChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                textBox.TextChanged -= TextBox_TextChanged;

                if ((bool)e.NewValue)
                {
                    textBox.TextChanged += TextBox_TextChanged;
                    textBox.ScrollToEnd();
                }
            }
        }

        private static void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.ScrollToEnd();
            }
        }
    }
}