using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Windows.Controls;

namespace KnockerWPF.Infrastructure
{
    public class PointAddressValidationRule : ValidationRule
    {
        private string _errorMessage;
        private RegexOptions _regexOptions = RegexOptions.None;
        private string _regexText;
        private PointAddressType _addrType;

        private Regex _rxUrlValidator = new Regex(@"^http(s)?://([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$");
        private Regex _rxIpValidator = new Regex(@"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$");

        #region Constructors

        public PointAddressValidationRule()
        {
        }

        //public PointAddressValidationRule(string regexText)
        //{
        //    this.RegexText = regexText;
        //}

        //public PointAddressValidationRule(string regexText, string errorMessage)
        //    : this(regexText)
        //{
        //    this.RegexOptions = _regexOptions;
        //}

        //public PointAddressValidationRule(string regexText, string errorMessage, RegexOptions regexOptions)
        //    : this(regexText)
        //{
        //    this.RegexOptions = regexOptions;
        //}

        #endregion

        #region Props

        public string ErrorMessage
        {
            get { return this._errorMessage; }
            set { this._errorMessage = value; }
        }

        public RegexOptions RegexOptions
        {
            get { return _regexOptions; }
            set { _regexOptions = value; }
        }

        //public string RegexText
        //{
        //    get { return regexText; }
        //    set { regexText = value; }
        //}

        public PointAddressType AddressType
        {
            get { return _addrType; }
            set { _addrType = value; }
        }

        #endregion

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            //ValidationResult result = ValidationResult.ValidResult;

            switch (_addrType)
            {
                case PointAddressType.Both:
                    {
                        string input = (string)value;
                        bool is_valid = _rxIpValidator.IsMatch(input) ? true : _rxUrlValidator.IsMatch(input);
                        return new ValidationResult(is_valid, is_valid ? null : "Invalid point address format");
                    }
                case PointAddressType.URL:
                    {
                            string input = (string)value;
                            bool is_valid = _rxUrlValidator.IsMatch(input);
                            return new ValidationResult(is_valid, is_valid ? null : "Invalid URL address format");
                        
                    }
                case PointAddressType.IP:
                    {
                        string input = (string)value;
                        bool is_valid = _rxIpValidator.IsMatch(input);
                        return new ValidationResult(is_valid, is_valid ? null : "Invalid IP address format");
                    }
                default:
                    return new ValidationResult(false, "Address type is not selected");
            }
            //if (!string.IsNullOrEmpty(RegexText))
            //{
            //    string text = value as string ?? String.Empty;

            //    if (!Regex.IsMatch(text, this.RegexText, this.RegexOptions))
            //        result = new ValidationResult(false, this.ErrorMessage);
            //}

            //return result;
        }
    }

    public enum PointAddressType
    {
        Both = 0,
        URL = 1,
        IP = 2
    }
}
