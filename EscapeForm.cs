using System.Windows.Forms;

namespace TodoAMano {
    // Escape follows the cancellation route; it never clicks an accept button.
    public class EscapeForm : Form {
        protected override bool ProcessDialogKey(Keys keyData) {
            if (keyData == Keys.Escape) {
                if (Modal) DialogResult = DialogResult.Cancel;
                Close();
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }
    }
}
