using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using System.Web.Script.Serialization;

namespace TodoAMano { public partial class MainForm {
    private sealed class DraftState {
        internal readonly List<SellerLine2825> Lines = new List<SellerLine2825>();
        internal string Note, CorrectionId;
        internal DateTime When;
        internal int Selected;
    }
    private readonly List<DraftState> draftUndo = new List<DraftState>();
    private readonly List<DraftState> draftRedo = new List<DraftState>();
    private Button draftUndoButton, draftRedoButton;
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
        if(calculatorTabs2824!=null && calculatorTabs2824.SelectedTab==calculatorSellerPage2825) {
            if(keyData==(Keys.Control|Keys.Alt|Keys.Z)) { UndoDraftChange(false); return true; }
            if(keyData==(Keys.Control|Keys.Alt|Keys.Y)) { UndoDraftChange(true); return true; }
        }
        return base.ProcessCmdKey(ref msg,keyData);
    }
    private void InstallDraftContext() {
        try {
            string path=Path.Combine(dataDir,"borrador-contexto.json");
            if(File.Exists(path)) {
                var data=new JavaScriptSerializer().Deserialize<Dictionary<string,string>>(File.ReadAllText(path));
                DateTime when;
                if(data.ContainsKey("when") && DateTime.TryParse(data["when"],CultureInfo.InvariantCulture,DateTimeStyles.RoundtripKind,out when)
                    && when>=sellerRecordDate2826.MinDate && when<=sellerRecordDate2826.MaxDate) sellerRecordDate2826.Value=when;
                if(data.ContainsKey("note")) sellerRecordNote2826.Text=data["note"];
            }
        } catch { AnnounceToScreenReader("No se pudieron recuperar la fecha y la nota del borrador."); }
        FormClosing += delegate { SaveDraftContext(); };
    }
    private void SaveDraftContext() {
        try { AtomicWriteCalculatorJson285(Path.Combine(dataDir,"borrador-contexto.json"),
            new JavaScriptSerializer().Serialize(new Dictionary<string,string> {
                {"note",sellerRecordNote2826.Text}, {"when",sellerRecordDate2826.Value.ToString("o",CultureInfo.InvariantCulture)} }));
        } catch(Exception ex) { AnnounceToScreenReader("No se pudieron guardar la fecha y la nota del borrador. "+ex.Message); }
    }
    private DraftState CaptureDraftState() {
        var state = new DraftState { Note = sellerRecordNote2826 == null ? "" : sellerRecordNote2826.Text,
            When = sellerRecordDate2826 == null ? DateTime.Now : sellerRecordDate2826.Value,
            CorrectionId = sellerCorrectionId2827, Selected = sellerRows2825 == null ? -1 : sellerRows2825.SelectedIndex };
        foreach(var line in sellerLines2825) state.Lines.Add(CloneSellerLine2826(line));
        return state;
    }
    private void RememberDraftChange() {
        draftUndo.Add(CaptureDraftState());
        if(draftUndo.Count > 50) draftUndo.RemoveAt(0);
        draftRedo.Clear(); UpdateDraftUndoButtons();
    }
    private void ClearDraftUndo() { draftUndo.Clear(); draftRedo.Clear(); UpdateDraftUndoButtons(); }
    private void UpdateDraftUndoButtons() {
        if(draftUndoButton != null) draftUndoButton.Enabled = draftUndo.Count > 0;
        if(draftRedoButton != null) draftRedoButton.Enabled = draftRedo.Count > 0;
    }
    private void ApplyDraftState(DraftState state) {
        sellerLines2825.Clear();
        foreach(var line in state.Lines) sellerLines2825.Add(CloneSellerLine2826(line));
        sellerCorrectionId2827 = state.CorrectionId;
        sellerRecordNote2826.Text = state.Note;
        sellerRecordDate2826.Value = state.When;
        UpdateSellerCorrectionStatus2827();
        RefreshSellerRows2825(state.Selected);
        ScheduleSellerDraftSave2930();
    }
    private void UndoDraftChange(bool redo) {
        var source = redo ? draftRedo : draftUndo;
        var target = redo ? draftUndo : draftRedo;
        if(source.Count == 0) { AnnounceToScreenReader(redo ? "No hay cambios para rehacer." : "No hay cambios para deshacer."); return; }
        target.Add(CaptureDraftState());
        var state = source[source.Count-1]; source.RemoveAt(source.Count-1);
        ApplyDraftState(state); UpdateDraftUndoButtons();
        AnnounceToScreenReader((redo ? "Cambio rehecho. " : "Cambio deshecho. ") + "Total " + SellerTotal2825().ToString("N2", SellerCulture2825) + " euros.");
    }
    private void AddDraftUndoButtons(FlowLayoutPanel row) {
        draftUndoButton = NewButton("Deshacer cambio (Ctrl+Alt+Z)", delegate { UndoDraftChange(false); });
        draftRedoButton = NewButton("Rehacer cambio (Ctrl+Alt+Y)", delegate { UndoDraftChange(true); });
        row.Controls.Add(draftUndoButton); row.Controls.Add(draftRedoButton); UpdateDraftUndoButtons();
    }
} }
