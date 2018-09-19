using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace MegaApp.Classes
{
    public class NumericPasswordBox : RadPasswordBox
    {
        private TextBox _passwordTextBox;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _passwordTextBox = GetTemplatePart<TextBox>("PART_PasswordTextBox");
          
            var inputScope = new InputScope();
            inputScope.Names.Add(new InputScopeName
            {
                NameValue = InputScopeNameValue.Number
            });
            _passwordTextBox.InputScope = inputScope;
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            _passwordTextBox.Focus();
        }
    }
}
