using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ScriptDatabaseEditor
{
    /// <summary>
    /// Interaction logic for NumericUpDown.xaml
    /// </summary>
    public partial class NumericUpDown : UserControl
    {

        public int Value
        {
            get => (int)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
        public int MinimumValue
        {
            get => (int)GetValue(MinimumValueProperty);
            set => SetValue(MinimumValueProperty, value);
        }
        public int MaximumValue
        {
            get => (int)GetValue(MaximumValueProperty);
            set => SetValue(MaximumValueProperty, value);
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value",
                typeof(int),
                typeof(NumericUpDown),
                new PropertyMetadata(0, new PropertyChangedCallback(OnValuePropertyChanged)));
        public static readonly DependencyProperty MinimumValueProperty =
            DependencyProperty.Register(
                "MinimumValue",
                typeof(int),
                typeof(NumericUpDown),
                new PropertyMetadata(0, new PropertyChangedCallback(OnMinimumValuePropertyChanged)));
        public static readonly DependencyProperty MaximumValueProperty =
            DependencyProperty.Register(
                "MaximumValue",
                typeof(int),
                typeof(NumericUpDown),
                new PropertyMetadata(255, new PropertyChangedCallback(OnMaximumValuePropertyChanged)));

        public NumericUpDown()
        {
            InitializeComponent();
        }
        
        private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = d as NumericUpDown;
            int value = (int)e.NewValue;
            if (value < obj.MinimumValue)
                obj.Value = obj.MinimumValue;
            if (value > obj.MaximumValue)
                obj.Value = obj.MaximumValue;
            obj.NumTextBox.TextChanged -= obj.NumTextBox_TextChanged;
            obj.NumTextBox.Text = e.NewValue.ToString();
            obj.NumTextBox.TextChanged += obj.NumTextBox_TextChanged;
        }
        private static void OnMinimumValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = d as NumericUpDown;
            int min = (int)e.NewValue;
            if (obj.Value < min)
                obj.Value = min;
        }
        private static void OnMaximumValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = d as NumericUpDown;
            int max = (int)e.NewValue;
            if (obj.Value > max)
                obj.Value = max;
        }

        private void RepeatButtonUp_Click(object sender, RoutedEventArgs e)
        {
            ++Value;
        }
        private void RepeatButtonDown_Click(object sender, RoutedEventArgs e)
        {
            --Value;
        }

        private void NumTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(NumTextBox.Text, out int value))
                Value = value;
            else
                NumTextBox.Text = Value.ToString();
        }
    }
}
