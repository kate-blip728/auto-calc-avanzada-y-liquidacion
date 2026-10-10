using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TodoAMano {
public partial class MainForm {
    private Timer repeatedCalculationTimer;
    private CheckBox repeatedCalculationEnabled;
    private string lastRepeatedCalculation = "";

    private static string RepeatedInputKey(string input) {
        // Preserve spaces inside numbers: "1 47" must never match "147".
        return Regex.Replace((input ?? "").Trim(), @"\s*([+*/()%;\-])\s*", "$1");
    }

    private void InstallRepeatedCalculation(TableLayoutPanel layout) {
        string preference = Path.Combine(dataDir, "desactivar_operaciones_repetidas.txt");
        repeatedCalculationEnabled = new CheckBox {
            Text = "Avisar automáticamente de operaciones ya realizadas",
            AutoSize = true, Checked = !File.Exists(preference)
        };
        AddFullRow(layout, repeatedCalculationEnabled, false);
        repeatedCalculationTimer = new Timer { Interval = 900 };
        repeatedCalculationTimer.Tick += delegate {
            repeatedCalculationTimer.Stop();
            if (calculadoraInput.Focused && calculatorTabs2824.SelectedTab == calculatorBasicPage2824)
                TryAnnounceRepeatedCalculation();
        };
        calculadoraInput.TextChanged += delegate {
            repeatedCalculationTimer.Stop();
            lastRepeatedCalculation = "";
            if (repeatedCalculationEnabled.Checked && calculadoraInput.Focused)
                repeatedCalculationTimer.Start();
        };
        calculadoraInput.Leave += delegate { repeatedCalculationTimer.Stop(); };
        calcTemplate.SelectedIndexChanged += delegate {
            repeatedCalculationTimer.Stop(); lastRepeatedCalculation = "";
        };
        repeatedCalculationEnabled.CheckedChanged += delegate {
            repeatedCalculationTimer.Stop(); lastRepeatedCalculation = "";
            if (repeatedCalculationEnabled.Checked) {
                if (File.Exists(preference)) File.Delete(preference);
            } else File.WriteAllText(preference, "1");
        };
        Disposed += delegate { repeatedCalculationTimer.Dispose(); };
    }

    private bool TryAnnounceRepeatedCalculation() {
        if (!repeatedCalculationEnabled.Checked || calculatorHistoryLoading2824) return false;
        string input = RepeatedInputKey(calculadoraInput.Text);
        string kind = "Calculadora: " + calcTemplate.Text;
        string key = kind + "\n" + input;
        if (input.Length == 0 || key == lastRepeatedCalculation) return false;
        foreach (CalculatorHistoryItem2824 item in calculatorHistory2824) {
            if (item.ReuseTarget != "basic" || item.Kind != kind ||
                RepeatedInputKey(item.Input) != input || !IsHistoryResultValid2824(item.Result)) continue;
            lastRepeatedCalculation = key;
            // Updating the display must not insert another history entry.
            calculatorHistoryLoading2824 = true;
            try { calculadoraResultado.Text = item.Result; }
            finally { calculatorHistoryLoading2824 = false; }
            PlayCalculatorSound1554("calc_historial");
            AnnounceToScreenReader("Operación ya realizada. Resultado: " + item.Result + ".");
            return true;
        }
        return false;
    }
}
}
