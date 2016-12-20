using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Data;

namespace KnockerWPF.Infrastructure
{
    public class UrlRegexValidator
    {
        static UrlRegexValidator()
        {
            RegexTextProperty = DependencyProperty.RegisterAttached(
                "RegexText",
                typeof(string),
                typeof(UrlRegexValidator),
                new UIPropertyMetadata(null, OnAttachedPropertyChanged));

            ErrorMessageProperty = DependencyProperty.RegisterAttached(
                "ErrorMessage",
                typeof(string),
                typeof(UrlRegexValidator),
                new UIPropertyMetadata(null, OnAttachedPropertyChanged));
        }

        public static readonly DependencyProperty ErrorMessageProperty;

        public static string GetErrorMessage(TextBox textBox)
        {
            return textBox.GetValue(ErrorMessageProperty) as string;
        }

        public static void SetErrorMessage(TextBox textBox, string value)
        {
            textBox.SetValue(ErrorMessageProperty, value);
        }

        public static readonly DependencyProperty RegexTextProperty;

        public static string GetRegexText(TextBox textBox)
        {
            return textBox.GetValue(RegexTextProperty) as string;
        }

        public static void SetRegexText(TextBox textBox, string value)
        {
            textBox.SetValue(RegexTextProperty, value);
        }

        static void OnAttachedPropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            TextBox textBox = depObj as TextBox;
            if (textBox == null)
                throw new InvalidOperationException(
                    "The RegexValidator can only be used with a TextBox.");

            VerifyRegexValidationRule(textBox);
        }

        static PointAddressValidationRule GetRegexValidationRuleForTextBox(TextBox textBox)
        {
            if (!textBox.IsInitialized)
            {
                EventHandler callback = null;
                callback = delegate
                {
                    textBox.Initialized -= callback;
                    VerifyRegexValidationRule(textBox);
                };
                textBox.Initialized += callback;
                return null;
            }

            BindingExpression expression = textBox.GetBindingExpression(TextBox.TextProperty);
            if (expression == null)
                throw new InvalidOperationException(
                    "The TextBox's Text property must be bound for the RegexValidator to validate it.");

            Binding binding = expression.ParentBinding;
            if (binding == null)
                throw new ApplicationException(
                    "Unexpected situation: the TextBox.Text binding expression has no parent binding.");

            PointAddressValidationRule regexRule = null;
            foreach (ValidationRule rule in binding.ValidationRules)
            {
                if (rule is PointAddressValidationRule)
                {
                    if (regexRule == null)
                        regexRule = rule as PointAddressValidationRule;
                    else
                        throw new InvalidOperationException(
                            "There should not be more than one RegexValidationRule in a Binding's ValidationRules.");
                }
            }

            if (regexRule == null)
            {
                regexRule = new PointAddressValidationRule();
                binding.ValidationRules.Add(regexRule);
            }

            return regexRule;
        }

        static void VerifyRegexValidationRule(TextBox textBox)
        {
            PointAddressValidationRule regexRule = GetRegexValidationRuleForTextBox(textBox);
            if (regexRule != null)
            {
                //regexRule.RegexText =
                //    textBox.GetValue(UrlRegexValidator.RegexTextProperty) as string;

                regexRule.ErrorMessage =
                    textBox.GetValue(UrlRegexValidator.ErrorMessageProperty) as string;
            }
        }

    }
}
