using System.Windows.Forms;

namespace RevitApiTemplate
{
    internal sealed class BundleOptionsForm : Form
    {
        private readonly CheckBox _createBundleFiles;
        private readonly CheckBox _useObfuscar;

        public bool CreateBundleFiles => _createBundleFiles.Checked;
        public bool UseObfuscar => _useObfuscar.Checked;

        public BundleOptionsForm()
        {
            Text = "Revit API Project Options";
            Width = 380;
            Height = 180;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            _createBundleFiles = new CheckBox
            {
                Text = "Create bundle files",
                Checked = true,
                AutoSize = true,
                Left = 20,
                Top = 20
            };

            _useObfuscar = new CheckBox
            {
                Text = "Use Obfuscar",
                Checked = true,
                AutoSize = true,
                Left = 20,
                Top = 50
            };

            var createButton = new Button
            {
                Text = "Create",
                DialogResult = DialogResult.OK,
                Left = 185,
                Top = 95,
                Width = 75
            };

            var cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Left = 270,
                Top = 95,
                Width = 75
            };

            Controls.Add(_createBundleFiles);
            Controls.Add(_useObfuscar);
            Controls.Add(createButton);
            Controls.Add(cancelButton);

            AcceptButton = createButton;
            CancelButton = cancelButton;
        }
    }
}
