using System.Windows.Forms;

namespace TodoAMano {
    // Escape follows the cancellation route; it never clicks an accept button.
    public class EscapeForm : Form {
        private MainForm SoundOwner() {
            Form owner=Owner;
            while(owner!=null) {MainForm main=owner as MainForm;if(main!=null)return main;owner=owner.Owner;}
            return null;
        }
        protected override void OnShown(System.EventArgs e) {base.OnShown(e);if(!(this is MainForm)) {var main=SoundOwner();if(main!=null)main.NotifySoundScreen(this,true);} }
        protected override void OnFormClosed(FormClosedEventArgs e) {var main=SoundOwner();base.OnFormClosed(e);if(!(this is MainForm) && main!=null)main.NotifySoundScreen(this,false);}
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
