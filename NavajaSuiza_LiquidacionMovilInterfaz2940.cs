using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;

namespace TodoAMano
{
    public partial class MainForm
    {
        private Form mobileManager2940;
        private ListBox mobileInventoryList2940;
        private ListBox mobileScratchList2940;
        private TextBox mobileStatus2940;
        private ComboBox mobileProvider2940;
        private CheckBox mobileAutoSyncCheck2940;

        private TextBox mobileInvProduct2940;
        private TextBox mobileInvDraw2940;
        private TextBox mobileInvNumber2940;
        private TextBox mobileInvSeries2940;
        private NumericUpDown mobileInvQuantity2940;

        private TextBox mobileScratchProduct2940;
        private TextBox mobileScratchNumber2940;
        private NumericUpDown mobileScratchInitial2940;
        private NumericUpDown mobileScratchRemaining2940;

        private sealed class MobileListItem2940
        {
            public Dictionary<string, object> Data;
            public string Text;
            public override string ToString() { return Text ?? ""; }
        }

        private void ShowLiquidacionMovil2940()
        {
            EnsureLiquidacionMovilData2940();
            if (mobileManager2940 != null && !mobileManager2940.IsDisposed)
            {
                RefreshLiquidacionMovilUi2940();
                mobileManager2940.Show();
                mobileManager2940.BringToFront();
                mobileManager2940.Activate();
                return;
            }

            mobileManager2940 = new EscapeForm();
            mobileManager2940.Text = "Inventario, rascas y sincronización móvil — Navaja Suiza";
            mobileManager2940.StartPosition = FormStartPosition.CenterParent;
            mobileManager2940.Width = 900;
            mobileManager2940.Height = 680;
            mobileManager2940.MinimumSize = new System.Drawing.Size(680, 500);

            TabControl tabs = new TabControl();
            tabs.Dock = DockStyle.Fill;
            tabs.AccessibleName = "Secciones de Liquidación móvil";
            tabs.TabPages.Add(BuildMobileInventoryTab2940());
            tabs.TabPages.Add(BuildMobileScratchTab2940());
            tabs.TabPages.Add(BuildMobileSyncTab2940());

            mobileManager2940.Controls.Add(tabs);
            mobileManager2940.FormClosed += delegate
            {
                mobileManager2940 = null;
                mobileInventoryList2940 = null;
                mobileScratchList2940 = null;
                mobileStatus2940 = null;
                mobileProvider2940 = null;
                mobileAutoSyncCheck2940 = null;
            };
            mobileManager2940.Shown += delegate
            {
                RefreshLiquidacionMovilUi2940();
                if (mobileInventoryList2940 != null) mobileInventoryList2940.Focus();
            };
            mobileManager2940.Show(this);
        }

        private TabPage BuildMobileInventoryTab2940()
        {
            TabPage page = new TabPage("Inventario de números");
            TableLayoutPanel layout = BaseLayout();

            TextBox intro = ResultBox();
            intro.ReadOnly = true;
            intro.Height = 70;
            intro.Text = "Inventario compartido con Navaja Liquidación del móvil. Los cambios se guardan localmente y, si la sincronización automática está activada, se envían a la nube.";
            AddResultRow(layout, "Uso:", intro);

            mobileInvProduct2940 = AddLabeledText(layout, "Producto:", false);
            mobileInvDraw2940 = AddLabeledText(layout, "Sorteo o fecha:", false);
            mobileInvNumber2940 = AddLabeledText(layout, "Número:", false);
            mobileInvSeries2940 = AddLabeledText(layout, "Serie, opcional:", false);

            Label quantityLabel = new Label();
            quantityLabel.Text = "Cantidad:";
            quantityLabel.AutoSize = true;
            mobileInvQuantity2940 = new NumericUpDown();
            mobileInvQuantity2940.Minimum = 1;
            mobileInvQuantity2940.Maximum = 100000;
            mobileInvQuantity2940.Value = 1;
            mobileInvQuantity2940.AccessibleName = "Cantidad de números";
            mobileInvQuantity2940.Dock = DockStyle.Top;
            int qr = layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.Controls.Add(quantityLabel, 0, qr);
            layout.Controls.Add(mobileInvQuantity2940, 1, qr);

            FlowLayoutPanel add = ButtonRow();
            add.Controls.Add(NewButton("Guardar y añadir otro", delegate { AddMobileInventory2940(); }));
            AddFullRow(layout, add, false);

            mobileInventoryList2940 = new ListBox();
            mobileInventoryList2940.Dock = DockStyle.Fill;
            mobileInventoryList2940.Height = 230;
            mobileInventoryList2940.AccessibleName = "Números del inventario móvil";
            mobileInventoryList2940.AccessibleDescription = "Lista de números. Usa los botones siguientes para cambiar el estado o eliminar.";
            AddFullRow(layout, mobileInventoryList2940, true);

            FlowLayoutPanel actions = ButtonRow();
            actions.Controls.Add(NewButton("Marcar disponible", delegate { ChangeMobileInventoryState2940("disponible"); }));
            actions.Controls.Add(NewButton("Marcar vendido", delegate { ChangeMobileInventoryState2940("vendido"); }));
            actions.Controls.Add(NewButton("Marcar devuelto", delegate { ChangeMobileInventoryState2940("devuelto"); }));
            actions.Controls.Add(NewButton("Marcar anulado", delegate { ChangeMobileInventoryState2940("anulado"); }));
            actions.Controls.Add(NewButton("Eliminar", delegate { DeleteMobileInventory2940(); }));
            AddFullRow(layout, actions, false);

            page.Controls.Add(layout);
            return page;
        }

        private TabPage BuildMobileScratchTab2940()
        {
            TabPage page = new TabPage("Rascas y libros");
            TableLayoutPanel layout = BaseLayout();

            TextBox intro = ResultBox();
            intro.ReadOnly = true;
            intro.Height = 75;
            intro.Text = "Registra los libros de rascas y su estado. Se distinguen recibido, pendiente de activar, activado, confirmado, agotado, devuelto y anulado. Activar y confirmar guardan también la fecha.";
            AddResultRow(layout, "Uso:", intro);

            mobileScratchProduct2940 = AddLabeledText(layout, "Producto:", false);
            mobileScratchNumber2940 = AddLabeledText(layout, "Número de libro:", false);

            Label initialLabel = new Label(); initialLabel.Text = "Cantidad inicial:"; initialLabel.AutoSize = true;
            mobileScratchInitial2940 = new NumericUpDown();
            mobileScratchInitial2940.Minimum = 1; mobileScratchInitial2940.Maximum = 100000; mobileScratchInitial2940.Value = 1;
            mobileScratchInitial2940.AccessibleName = "Cantidad inicial del libro"; mobileScratchInitial2940.Dock = DockStyle.Top;
            int ir = layout.RowCount++; layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.Controls.Add(initialLabel, 0, ir); layout.Controls.Add(mobileScratchInitial2940, 1, ir);

            Label remainingLabel = new Label(); remainingLabel.Text = "Cantidad restante:"; remainingLabel.AutoSize = true;
            mobileScratchRemaining2940 = new NumericUpDown();
            mobileScratchRemaining2940.Minimum = 0; mobileScratchRemaining2940.Maximum = 100000; mobileScratchRemaining2940.Value = 1;
            mobileScratchRemaining2940.AccessibleName = "Cantidad restante del libro"; mobileScratchRemaining2940.Dock = DockStyle.Top;
            int rr = layout.RowCount++; layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.Controls.Add(remainingLabel, 0, rr); layout.Controls.Add(mobileScratchRemaining2940, 1, rr);

            FlowLayoutPanel add = ButtonRow();
            add.Controls.Add(NewButton("Guardar libro pendiente de activar", delegate { AddMobileScratch2940(); }));
            AddFullRow(layout, add, false);

            mobileScratchList2940 = new ListBox();
            mobileScratchList2940.Dock = DockStyle.Fill;
            mobileScratchList2940.Height = 220;
            mobileScratchList2940.AccessibleName = "Libros de rascas";
            mobileScratchList2940.AccessibleDescription = "Lista de libros con estado y cantidad restante.";
            AddFullRow(layout, mobileScratchList2940, true);

            FlowLayoutPanel actions = ButtonRow();
            actions.Controls.Add(NewButton("Pendiente de activar", delegate { ChangeMobileScratchState2940("pendiente_activar"); }));
            actions.Controls.Add(NewButton("Activar", delegate { ChangeMobileScratchState2940("activado"); }));
            actions.Controls.Add(NewButton("Confirmar", delegate { ChangeMobileScratchState2940("confirmado"); }));
            actions.Controls.Add(NewButton("Vender uno", delegate { SellOneMobileScratch2940(); }));
            actions.Controls.Add(NewButton("Marcar agotado", delegate { ChangeMobileScratchState2940("agotado"); }));
            actions.Controls.Add(NewButton("Marcar devuelto", delegate { ChangeMobileScratchState2940("devuelto"); }));
            actions.Controls.Add(NewButton("Anular", delegate { ChangeMobileScratchState2940("anulado"); }));
            actions.Controls.Add(NewButton("Eliminar", delegate { DeleteMobileScratch2940(); }));
            AddFullRow(layout, actions, false);

            page.Controls.Add(layout);
            return page;
        }

        private TabPage BuildMobileSyncTab2940()
        {
            TabPage page = new TabPage("Sincronización");
            TableLayoutPanel layout = BaseLayout();

            TextBox intro = ResultBox();
            intro.ReadOnly = true;
            intro.Height = 88;
            intro.Text = "Conectar Google Drive autoriza la cuenta, pero no confirma por sí solo que los datos se hayan sincronizado. Elige Google Drive y pulsa Sincronizar ahora para comprobarlo. El resultado se anuncia y queda en Estado; ahí también puedes consultar la última hora de sincronización. Si la sincronización automática está activada, Navaja vuelve a sincronizar periódicamente mientras está abierta.";
            AddResultRow(layout, "Información:", intro);

            mobileProvider2940 = AddLabeledCombo(layout, "Nube para Liquidación móvil:", new string[] { "Dropbox", "Google Drive" });
            if (!string.IsNullOrWhiteSpace(mobileSyncProvider2940)) mobileProvider2940.Text = mobileSyncProvider2940;
            mobileProvider2940.SelectedIndexChanged += delegate
            {
                mobileSyncProvider2940 = Convert.ToString(mobileProvider2940.SelectedItem);
                SaveLiquidacionMovilSettings2940();
                RefreshMobileSyncStatus2940();
            };

            mobileAutoSyncCheck2940 = new CheckBox();
            mobileAutoSyncCheck2940.AutoSize = true;
            mobileAutoSyncCheck2940.Text = "Sincronizar automáticamente";
            mobileAutoSyncCheck2940.Checked = mobileAutoSync2940;
            mobileAutoSyncCheck2940.AccessibleDescription = "Sincroniza después de cambios y periódicamente mientras Navaja está abierta.";
            mobileAutoSyncCheck2940.CheckedChanged += delegate
            {
                mobileAutoSync2940 = mobileAutoSyncCheck2940.Checked;
                SaveLiquidacionMovilSettings2940();
                if (mobileAutoSync2940) ScheduleLiquidacionMovilSync2940();
            };
            AddFullRow(layout, mobileAutoSyncCheck2940, false);

            FlowLayoutPanel buttons = ButtonRow();
            buttons.Controls.Add(NewButton("Sincronizar ahora", delegate { QueueLiquidacionMovilSync2940(true); }));
            buttons.Controls.Add(NewButton("Publicar actualización móvil", delegate { PublishLiquidacionMovilUpdate2940(); }));
            buttons.Controls.Add(NewButton("Abrir Nubes por API", delegate { ShowCloudHub2878(); }));
            buttons.Controls.Add(NewButton("Conectar Dropbox", delegate { ShowCloudBrowser2878("Dropbox"); }));
            buttons.Controls.Add(NewButton("Conectar Google Drive", delegate { ShowGoogleDriveModule2851(); }));
            AddFullRow(layout, buttons, false);

            mobileStatus2940 = ResultBox();
            mobileStatus2940.ReadOnly = true;
            mobileStatus2940.Height = 130;
            mobileStatus2940.AccessibleName = "Estado y resultado de la sincronización";
            AddResultRow(layout, "Estado:", mobileStatus2940);

            page.Controls.Add(layout);
            return page;
        }

        private void AddMobileInventory2940()
        {
            EnsureLiquidacionMovilData2940();
            string product = mobileInvProduct2940 == null ? "" : mobileInvProduct2940.Text.Trim();
            string number = mobileInvNumber2940 == null ? "" : mobileInvNumber2940.Text.Trim();
            if (product.Length == 0 || number.Length == 0)
            {
                PlayNamedSound("aviso");
                AnnounceToScreenReader("Indica el producto y el número.");
                if (number.Length == 0 && mobileInvNumber2940 != null) mobileInvNumber2940.Focus();
                return;
            }
            string draw = mobileInvDraw2940 == null ? "" : mobileInvDraw2940.Text.Trim();
            string series = mobileInvSeries2940 == null ? "" : mobileInvSeries2940.Text.Trim();

            foreach (Dictionary<string, object> existing in mobileInventory2940)
            {
                if (MobileDeleted2940(existing)) continue;
                if (string.Equals(MobileText2940(existing, "producto"), product, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(MobileText2940(existing, "numero"), number, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(MobileText2940(existing, "sorteo"), draw, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(MobileText2940(existing, "serie"), series, StringComparison.OrdinalIgnoreCase))
                {
                    PlayNamedSound("aviso");
                    AnnounceToScreenReader("Ese número ya está guardado para este producto y sorteo.");
                    mobileInvNumber2940.Focus();
                    return;
                }
            }

            Dictionary<string, object> item = new Dictionary<string, object>();
            item["id"] = Guid.NewGuid().ToString("N");
            item["producto"] = product;
            item["sorteo"] = draw;
            item["numero"] = number;
            item["serie"] = series;
            item["cantidad"] = mobileInvQuantity2940 == null ? 1 : (int)mobileInvQuantity2940.Value;
            item["estado"] = "disponible";
            item["nota"] = "";
            item["created_at"] = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
            item["deleted_at"] = null;
            StampMobileItem2940(item);
            mobileInventory2940.Insert(0, item);
            SaveLiquidacionMovilData2940();

            if (mobileInvNumber2940 != null) { mobileInvNumber2940.Clear(); mobileInvNumber2940.Focus(); }
            if (mobileInvSeries2940 != null) mobileInvSeries2940.Clear();
            if (mobileInvQuantity2940 != null) mobileInvQuantity2940.Value = 1;
            RefreshLiquidacionMovilUi2940();
            PlayCalculatorSound1554("calc_historial");
            AnnounceToScreenReader("Número " + number + " guardado. Preparado para el siguiente.");
            ScheduleLiquidacionMovilSync2940();
        }

        private Dictionary<string, object> SelectedMobile2940(ListBox list)
        {
            MobileListItem2940 wrapper = list == null ? null : list.SelectedItem as MobileListItem2940;
            return wrapper == null ? null : wrapper.Data;
        }

        private void ChangeMobileInventoryState2940(string state)
        {
            Dictionary<string, object> item = SelectedMobile2940(mobileInventoryList2940);
            if (item == null) { PlayNamedSound("aviso"); AnnounceToScreenReader("Selecciona un número."); return; }
            item["estado"] = state;
            StampMobileItem2940(item);
            SaveLiquidacionMovilData2940();
            RefreshLiquidacionMovilUi2940();
            PlayCalculatorSound1554("calc_historial");
            AnnounceToScreenReader("Número " + MobileText2940(item, "numero") + ": " + state + ".");
            ScheduleLiquidacionMovilSync2940();
        }

        private void DeleteMobileInventory2940()
        {
            Dictionary<string, object> item = SelectedMobile2940(mobileInventoryList2940);
            if (item == null) { PlayNamedSound("aviso"); return; }
            if (MessageBox.Show("¿Eliminar el número " + MobileText2940(item, "numero") + "?", "Eliminar número", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
            item["deleted_at"] = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
            StampMobileItem2940(item);
            SaveLiquidacionMovilData2940();
            RefreshLiquidacionMovilUi2940();
            PlayCalculatorSound1554("calc_historial");
            AnnounceToScreenReader("Número eliminado.");
            ScheduleLiquidacionMovilSync2940();
        }

        private void AddMobileScratch2940()
        {
            EnsureLiquidacionMovilData2940();
            string product = mobileScratchProduct2940 == null ? "" : mobileScratchProduct2940.Text.Trim();
            string number = mobileScratchNumber2940 == null ? "" : mobileScratchNumber2940.Text.Trim();
            if (product.Length == 0 || number.Length == 0)
            {
                PlayNamedSound("aviso");
                AnnounceToScreenReader("Indica el producto y el número de libro.");
                return;
            }
            foreach (Dictionary<string, object> existing in mobileScratchBooks2940)
            {
                if (MobileDeleted2940(existing)) continue;
                if (string.Equals(MobileText2940(existing, "producto"), product, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(MobileText2940(existing, "numero_libro"), number, StringComparison.OrdinalIgnoreCase))
                {
                    PlayNamedSound("aviso");
                    AnnounceToScreenReader("Ese libro ya está guardado para este producto.");
                    mobileScratchNumber2940.Focus();
                    return;
                }
            }

            int initial = mobileScratchInitial2940 == null ? 1 : (int)mobileScratchInitial2940.Value;
            int remaining = mobileScratchRemaining2940 == null ? initial : (int)mobileScratchRemaining2940.Value;
            if (remaining > initial) remaining = initial;

            Dictionary<string, object> item = new Dictionary<string, object>();
            item["id"] = Guid.NewGuid().ToString("N");
            item["producto"] = product;
            item["numero_libro"] = number;
            item["cantidad_inicial"] = initial;
            item["cantidad_restante"] = remaining;
            item["estado"] = "pendiente_activar";
            item["nota"] = "";
            item["created_at"] = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
            item["activated_at"] = null;
            item["confirmed_at"] = null;
            item["deleted_at"] = null;
            StampMobileItem2940(item);
            mobileScratchBooks2940.Insert(0, item);
            SaveLiquidacionMovilData2940();

            mobileScratchNumber2940.Clear();
            mobileScratchNumber2940.Focus();
            mobileScratchInitial2940.Value = 1;
            mobileScratchRemaining2940.Value = 1;
            RefreshLiquidacionMovilUi2940();
            PlayCalculatorSound1554("calc_historial");
            AnnounceToScreenReader("Libro " + number + " guardado como pendiente de activar.");
            ScheduleLiquidacionMovilSync2940();
        }

        private void ChangeMobileScratchState2940(string state)
        {
            Dictionary<string, object> item = SelectedMobile2940(mobileScratchList2940);
            if (item == null) { PlayNamedSound("aviso"); AnnounceToScreenReader("Selecciona un libro."); return; }
            string now = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
            item["estado"] = state;
            if (state == "activado" && string.IsNullOrWhiteSpace(MobileText2940(item, "activated_at"))) item["activated_at"] = now;
            if (state == "confirmado" && string.IsNullOrWhiteSpace(MobileText2940(item, "confirmed_at"))) item["confirmed_at"] = now;
            StampMobileItem2940(item);
            SaveLiquidacionMovilData2940();
            RefreshLiquidacionMovilUi2940();
            PlayCalculatorSound1554("calc_historial");
            AnnounceToScreenReader("Libro " + MobileText2940(item, "numero_libro") + ": " + MobileScratchStateLabel2940(state) + ".");
            ScheduleLiquidacionMovilSync2940();
        }

        private void SellOneMobileScratch2940()
        {
            Dictionary<string, object> item = SelectedMobile2940(mobileScratchList2940);
            if (item == null) { PlayNamedSound("aviso"); return; }
            int remaining = MobileInt2940(item, "cantidad_restante", 0);
            if (remaining <= 0) { PlayNamedSound("aviso"); AnnounceToScreenReader("Este libro ya no tiene rascas restantes."); return; }
            remaining--;
            item["cantidad_restante"] = remaining;
            if (remaining == 0) item["estado"] = "agotado";
            StampMobileItem2940(item);
            SaveLiquidacionMovilData2940();
            RefreshLiquidacionMovilUi2940();
            PlayCalculatorSound1554("calc_historial");
            AnnounceToScreenReader("Quedan " + remaining.ToString(CultureInfo.CurrentCulture) + " rascas en el libro " + MobileText2940(item, "numero_libro") + ".");
            ScheduleLiquidacionMovilSync2940();
        }

        private void DeleteMobileScratch2940()
        {
            Dictionary<string, object> item = SelectedMobile2940(mobileScratchList2940);
            if (item == null) { PlayNamedSound("aviso"); return; }
            if (MessageBox.Show("¿Eliminar el libro " + MobileText2940(item, "numero_libro") + "?", "Eliminar libro", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
            item["deleted_at"] = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
            StampMobileItem2940(item);
            SaveLiquidacionMovilData2940();
            RefreshLiquidacionMovilUi2940();
            PlayCalculatorSound1554("calc_historial");
            AnnounceToScreenReader("Libro eliminado.");
            ScheduleLiquidacionMovilSync2940();
        }

        private string MobileScratchStateLabel2940(string state)
        {
            if (state == "pendiente_activar") return "pendiente de activar";
            if (state == "activado") return "activado";
            if (state == "confirmado") return "confirmado";
            if (state == "agotado") return "agotado";
            if (state == "devuelto") return "devuelto";
            if (state == "anulado") return "anulado";
            return "recibido";
        }

        private void RefreshLiquidacionMovilUi2940()
        {
            if (InvokeRequired)
            {
                try { BeginInvoke((MethodInvoker)RefreshLiquidacionMovilUi2940); } catch { }
                return;
            }
            EnsureLiquidacionMovilData2940();

            if (mobileInventoryList2940 != null)
            {
                mobileInventoryList2940.BeginUpdate();
                try
                {
                    mobileInventoryList2940.Items.Clear();
                    foreach (Dictionary<string, object> item in mobileInventory2940)
                    {
                        if (MobileDeleted2940(item)) continue;
                        string text = MobileText2940(item, "numero") + " — " + MobileText2940(item, "producto") +
                            " — " + MobileText2940(item, "estado") + " — cantidad " + MobileInt2940(item, "cantidad", 1).ToString(CultureInfo.CurrentCulture);
                        mobileInventoryList2940.Items.Add(new MobileListItem2940 { Data = item, Text = text });
                    }
                    if (mobileInventoryList2940.Items.Count > 0) mobileInventoryList2940.SelectedIndex = 0;
                }
                finally { mobileInventoryList2940.EndUpdate(); }
            }

            if (mobileScratchList2940 != null)
            {
                mobileScratchList2940.BeginUpdate();
                try
                {
                    mobileScratchList2940.Items.Clear();
                    foreach (Dictionary<string, object> item in mobileScratchBooks2940)
                    {
                        if (MobileDeleted2940(item)) continue;
                        string text = MobileText2940(item, "producto") + " — libro " + MobileText2940(item, "numero_libro") +
                            " — " + MobileScratchStateLabel2940(MobileText2940(item, "estado")) +
                            " — quedan " + MobileInt2940(item, "cantidad_restante", 0).ToString(CultureInfo.CurrentCulture) +
                            " de " + MobileInt2940(item, "cantidad_inicial", 0).ToString(CultureInfo.CurrentCulture);
                        mobileScratchList2940.Items.Add(new MobileListItem2940 { Data = item, Text = text });
                    }
                    if (mobileScratchList2940.Items.Count > 0) mobileScratchList2940.SelectedIndex = 0;
                }
                finally { mobileScratchList2940.EndUpdate(); }
            }

            if (mobileProvider2940 != null && !string.IsNullOrWhiteSpace(mobileSyncProvider2940))
                mobileProvider2940.Text = mobileSyncProvider2940;
            if (mobileAutoSyncCheck2940 != null) mobileAutoSyncCheck2940.Checked = mobileAutoSync2940;
            RefreshMobileSyncStatus2940();
        }

        private void RefreshMobileSyncStatus2940()
        {
            RefreshSellerCloudStatus2962();
            if (mobileStatus2940 == null) return;
            string provider = string.IsNullOrWhiteSpace(mobileSyncProvider2940) ? "ninguna" : mobileSyncProvider2940;
            string connected = string.IsNullOrWhiteSpace(mobileSyncProvider2940)
                ? "sin seleccionar"
                : (LiquidacionMovilCloudReady2940(mobileSyncProvider2940) ? "conectada" : "no conectada");
            string last = mobileLastSync2940 == DateTime.MinValue ? "nunca" : mobileLastSync2940.ToString("dd/MM/yyyy HH:mm");
            mobileStatus2940.Text = "Nube: " + provider + ", " + connected + ". Sincronización automática: " +
                (mobileAutoSync2940 ? "activada" : "desactivada") + ". Última sincronización: " + last + ". " + mobileLastResult2962;
        }

        private void SetLiquidacionMovilStatus2940(string text, bool announce)
        {
            if (InvokeRequired)
            {
                try { BeginInvoke((MethodInvoker)delegate { SetLiquidacionMovilStatus2940(text, announce); }); } catch { }
                return;
            }
            mobileLastResult2962 = text ?? "";
            SaveLiquidacionMovilSettings2940();
            if (mobileStatus2940 != null) mobileStatus2940.Text = text ?? "";
            RefreshSellerCloudStatus2962();
            RefreshSellerSavedDetail2826();
            if (announce && !string.IsNullOrWhiteSpace(text)) AnnounceToScreenReader(text);
        }
    }
}
