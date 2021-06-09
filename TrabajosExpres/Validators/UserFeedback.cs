
using FluentValidation.Results;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TrabajosExpres.Validators
{
    public class UserFeedback
    {
        private IList<Control> _controlsToBePaintedInGreen;
        private IList<Control> _controlsThatHaveBeenPaintedInRed;
        private IList<ValidationFailure> _validationFailures;

        public IList<Control> ControlsToBePaintedInGreen
        {
            private set { }
            get
            {
                return _controlsToBePaintedInGreen;
            }
        }

        public IList<Control> ControlsThatHaveBeenPaintedInRed
        {
            private set { }
            get
            {
                return _controlsThatHaveBeenPaintedInRed;
            }
        }

        public IList<ValidationFailure> ValidationFailures
        {
            private set { }
            get
            {
                return _validationFailures;
            }
        }

        public UserFeedback(DependencyObject dependencyObject, IList<ValidationFailure> validationFailures)
        {
            _controlsToBePaintedInGreen = FindVisualChildren(dependencyObject);
            _controlsThatHaveBeenPaintedInRed = new List<Control>();
            _validationFailures = validationFailures;
        }

        public void ShowFeedback()
        {
            ShowFeedbackInvalidFields();
            DeleteControlsThatHaveAlreadyBeenPainted();
            ShowFeedbackValidFields();
        }

        private void ShowFeedbackInvalidFields()
        {
            foreach (var control in _controlsToBePaintedInGreen)
            {
                foreach (ValidationFailure validationFailure in _validationFailures)
                {
                    string failureState = (string)validationFailure.CustomState;
                    if (control.Name.Equals(failureState))
                    {
                        control.BorderBrush = Brushes.Red;
                        _controlsThatHaveBeenPaintedInRed.Add(control);
                    }
                }
            }
        }

        private void DeleteControlsThatHaveAlreadyBeenPainted()
        {
            foreach (var control in _controlsThatHaveBeenPaintedInRed)
            {
                _controlsToBePaintedInGreen.Remove(control);
            }
        }

        private void ShowFeedbackValidFields()
        {
            foreach (var control in _controlsToBePaintedInGreen)
            {
                control.BorderBrush = Brushes.Green;
            }
        }

        private IList<Control> FindVisualChildren(DependencyObject dependencyObject)
        {
            IList<Control> controls = new List<Control>();
            if (dependencyObject != null)
            {
                int dependencyObjectsCount = VisualTreeHelper.GetChildrenCount(dependencyObject);
                for (int i = 0; i < dependencyObjectsCount; i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(dependencyObject, i);
                    if (child is TextBox || child is PasswordBox)
                    {
                        controls.Add(((Control)child));
                    }
                }
            }
            return controls;
        }
    }
}
