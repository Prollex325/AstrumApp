using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AstrumApp.Controls
{
    public partial class StepByStepControl : UserControl
    {
        private List<Border> segments = new List<Border>();
        private int currentStep = 0;
        private Color activeColor = Color.FromRgb(67, 141, 255);
        private Color inactiveColor = Color.FromRgb(30, 41, 54);

        public StepByStepControl()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                if (StepsCount > 0)
                {
                    InitializeSteps(StepsCount);
                }
            };
        }

        public static readonly DependencyProperty StepsCountProperty = DependencyProperty.Register(
            nameof(StepsCount),
            typeof(int),
            typeof(StepByStepControl),
            new PropertyMetadata(5)
        );

        public int StepsCount
        {
            get => (int)GetValue(StepsCountProperty);
            set => SetValue(StepsCountProperty, value);
        }

        public void SetCurrentStep(int step)
        {
            currentStep = step;
        }

        public void InitializeSteps(int totalSteps)
        {
            if (totalSteps <= 0)
                throw new ArgumentException("Total steps must be greater than zero.", nameof(totalSteps));
            
            StepGrid.Children.Clear();
            segments.Clear();
            for (int i = 0; i < totalSteps; i++)
            {
                Border border = AddSegment(i == 0); // Activate the first step
                segments.Add(border);
            }

            currentStep = 0;

            UpdateUI();
        }

        public void UpdateUI()
        {
            CurrentStepText.Text = $"{currentStep}";
            TotalStepsText.Text = $"{segments.Count}";

            Debug.WriteLine($"{segments.Count}");
            Debug.WriteLine($"");
        }

        public void NextStep(string hint)
        {
            Debug.WriteLine($"step {currentStep} count {segments.Count}");
            if (currentStep < segments.Count)
            {
                UpdateStep(currentStep + 1);
                HintText.Text = hint;
            }
        }

        public void BackStep(string hint)
        {
            if (currentStep > 0)
            {
                UpdateStep(currentStep - 1);
                HintText.Text = hint;
            }
        }

        private void UpdateStep(int step)
        {
            if (step < 0 || step > segments.Count)
                throw new ArgumentOutOfRangeException(nameof(step), "Step index is out of range.");

            for (int i = 0; i < segments.Count; i++)
            {
                segments[i].Background = new SolidColorBrush(i < step ? activeColor : inactiveColor);
            }
            currentStep = step;

            UpdateUI();
        }

        private Border AddSegment(bool isActive)
        {
            Color color = isActive ? activeColor : inactiveColor;
            var segment = new Border
            {
                Width = 60,
                Height = 6,
                CornerRadius = new CornerRadius(3),
                Background = new SolidColorBrush(color),
                Margin = new Thickness(3, 0, 3, 0)
            };

            StepGrid.Children.Add(segment);
            return segment;
        }
    }
}
