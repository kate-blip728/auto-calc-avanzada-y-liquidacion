using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TodoAMano
{
    public partial class MainForm
    {
        // Efectos específicos de la calculadora y los conversores.
        private bool calculatorSoundsEnabled1554 = true;
        private CheckBox settingsCalculatorSounds1554;
        private Timer pendingCalculatorSoundTimer1554;
        private string pendingCalculatorSound1554 = "";

        private void LoadCalculatorSoundSettings1554(Dictionary<string, object> data)
        {
            if (data == null) return;
            object value;
            if (data.TryGetValue("sonidosCalculadora", out value))
            {
                try { calculatorSoundsEnabled1554 = Convert.ToBoolean(value); }
                catch { calculatorSoundsEnabled1554 = true; }
            }
        }

        private void SaveCalculatorSoundSettings1554(Dictionary<string, object> data)
        {
            if (data != null) data["sonidosCalculadora"] = calculatorSoundsEnabled1554;
        }

        private void AddCalculatorSoundSettings1554(TableLayoutPanel layout)
        {
            settingsCalculatorSounds1554 = new CheckBox();
            settingsCalculatorSounds1554.AutoSize = true;
            settingsCalculatorSounds1554.Text = "Reproducir sonidos al escribir y calcular en Calculadora y utilidades";
            settingsCalculatorSounds1554.AccessibleName = "Sonidos de la calculadora";
            settingsCalculatorSounds1554.AccessibleDescription = "Distingue números, decimales, operaciones, borrado, igual, resultado y conversiones. No afecta a los demás sonidos de Navaja Suiza.";
            settingsCalculatorSounds1554.Checked = calculatorSoundsEnabled1554;
            AddFullRow(layout, settingsCalculatorSounds1554, false);
        }

        private void ApplyCalculatorSoundSettings1554()
        {
            if (settingsCalculatorSounds1554 != null)
                calculatorSoundsEnabled1554 = settingsCalculatorSounds1554.Checked;
        }

        private void RefreshCalculatorSoundSettings1554()
        {
            if (settingsCalculatorSounds1554 != null)
                settingsCalculatorSounds1554.Checked = calculatorSoundsEnabled1554;
        }

        private void ResetCalculatorSoundSettings1554()
        {
            if (settingsCalculatorSounds1554 != null) settingsCalculatorSounds1554.Checked = true;
        }

        private void PlayCalculatorSound1554(string name)
        {
            if (!calculatorSoundsEnabled1554 || !soundsEnabled || string.IsNullOrWhiteSpace(name)) return;
            PlayNamedSound(name);
        }

        private void ScheduleCalculatorSound1554(string name, int delayMilliseconds)
        {
            if (!calculatorSoundsEnabled1554 || !soundsEnabled) return;
            pendingCalculatorSound1554 = name;
            if (pendingCalculatorSoundTimer1554 == null)
            {
                pendingCalculatorSoundTimer1554 = new Timer();
                pendingCalculatorSoundTimer1554.Tick += delegate
                {
                    pendingCalculatorSoundTimer1554.Stop();
                    string sound = pendingCalculatorSound1554;
                    pendingCalculatorSound1554 = "";
                    if (sound.Length > 0) PlayCalculatorSound1554(sound);
                };
            }
            pendingCalculatorSoundTimer1554.Stop();
            pendingCalculatorSoundTimer1554.Interval = Math.Max(20, delayMilliseconds);
            pendingCalculatorSoundTimer1554.Start();
        }

        private void AttachCalculatorTextBox1554(TextBox box, Action enterAction)
        {
            if (box == null) return;
            box.KeyPress += delegate(object sender, KeyPressEventArgs e)
            {
                if (char.IsDigit(e.KeyChar)) PlayCalculatorSound1554("calc_numero");
                else if (e.KeyChar == ',' || e.KeyChar == '.') PlayCalculatorSound1554("calc_decimal");
                else if (e.KeyChar == '+' || e.KeyChar == '-' || e.KeyChar == '*' || e.KeyChar == '/' || e.KeyChar == '%' || e.KeyChar == '(' || e.KeyChar == ')' || e.KeyChar == ';')
                    PlayCalculatorSound1554("calc_operacion");
            };
            box.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete)
                    PlayCalculatorSound1554("calc_borrar");
                else if (e.KeyCode == Keys.Enter && enterAction != null)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    enterAction();
                }
            };
        }

        private void RunCalculator1554()
        {
            PlayCalculatorSound1554("calc_igual");
            Calculate();
        }

        private void ClearCalculator1554()
        {
            PlayCalculatorSound1554("calc_borrar");
            if (calculadoraInput != null) calculadoraInput.Clear();
            if (calculadoraResultado != null) calculadoraResultado.Clear();
            if (calculadoraInput != null) calculadoraInput.Focus();
            SetStatus("Campos limpios.");
        }

        private void RunUnitConversion1554()
        {
            PlayCalculatorSound1554("calc_igual");
            ConvertUnits1545();
        }

        private void RunCurrencyConversion1554(bool forceRefresh)
        {
            PlayCalculatorSound1554("calc_igual");
            ConvertCurrencies1545(forceRefresh);
        }
    }
}
