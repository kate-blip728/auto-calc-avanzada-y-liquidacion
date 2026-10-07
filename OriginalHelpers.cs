using System;
using System.Diagnostics;
using System.Web.Script.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Drawing;
namespace TodoAMano { public partial class MainForm {
        private TableLayoutPanel BaseLayout()
        {
            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.Padding = new Padding(12);
            layout.ColumnCount = 2;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.AutoScroll = true;
            return layout;
        }
        private TextBox AddLabeledText(TableLayoutPanel layout, string labelText, bool multiline)
        {
            int row = layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            Label label = new Label();
            label.Text = labelText;
            label.AutoSize = true;
            label.Anchor = AnchorStyles.Left;
            TextBox box = new TextBox();
            box.Dock = DockStyle.Top;
            box.Multiline = multiline;
            box.ScrollBars = multiline ? ScrollBars.Vertical : ScrollBars.None;
            box.AccessibleName = labelText.TrimEnd(':');
            label.UseMnemonic = true;
            layout.Controls.Add(label, 0, row);
            layout.Controls.Add(box, 1, row);
            label.TabStop = false;
            return box;
        }
        private ComboBox AddLabeledCombo(TableLayoutPanel layout, string labelText, string[] items)
        {
            int row = layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            Label label = new Label();
            label.Text = labelText;
            label.AutoSize = true;
            label.Anchor = AnchorStyles.Left;
            ComboBox combo = new ComboBox();
            combo.DropDownStyle = ComboBoxStyle.DropDownList;
            combo.Dock = DockStyle.Top;
            combo.AccessibleName = labelText.TrimEnd(':');
            combo.Items.AddRange(items);
            if (combo.Items.Count > 0) combo.SelectedIndex = 0;
            layout.Controls.Add(label, 0, row);
            layout.Controls.Add(combo, 1, row);
            return combo;
        }
        private void AddFullRow(TableLayoutPanel layout, Control control, bool fill)
        {
            int row = layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(fill ? SizeType.Percent : SizeType.AutoSize, fill ? 100 : 0));
            control.Dock = fill ? DockStyle.Fill : DockStyle.Top;
            layout.Controls.Add(control, 0, row);
            layout.SetColumnSpan(control, 2);
        }
        private void AddResultRow(TableLayoutPanel layout, string labelText, TextBoxBase box)
        {
            if (box != null)
            {
                string accessible = (labelText ?? "Resultado").Trim().TrimEnd(':');
                box.AccessibleName = accessible.Length > 0 ? accessible : "Resultado";
                System.Windows.Forms.RichTextBox rich = box as System.Windows.Forms.RichTextBox;
                box.AccessibleDescription = rich != null && rich.DetectUrls
                    ? "Cuadro donde se muestran los resultados. Sitúa el cursor en la línea de un enlace y pulsa Enter para abrirlo."
                    : "Cuadro donde se muestran los resultados de esta consulta.";
            }
            int labelRow = layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            Label label = new Label();
            label.Text = labelText;
            label.AutoSize = true;
            if (IsHelpLabel1602(labelText, box)) MarkHelpRow1602(label, box);
            layout.Controls.Add(label, 0, labelRow);
            layout.SetColumnSpan(label, 2);

            int resultRow = layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            box.Dock = DockStyle.Fill;
            layout.Controls.Add(box, 0, resultRow);
            layout.SetColumnSpan(box, 2);
        }
        private FlowLayoutPanel ButtonRow()
        {
            FlowLayoutPanel panel = new FlowLayoutPanel();
            panel.AutoSize = true;
            panel.WrapContents = true;
            return panel;
        }
        private Button NewButton(string text, EventHandler action)
        {
            Button button = new Button();
            button.Text = text;
            button.AutoSize = true;
            button.Click += action;
            return button;
        }
        private TextBox ResultBox()
        {
            TextBox box = new TextBox();
            box.Multiline = true;
            box.ReadOnly = true;
            box.ScrollBars = ScrollBars.Both;
            box.WordWrap = false;
            box.AcceptsReturn = true;
            box.AccessibleName = "Resultado";
            return box;
        }
        private void Calculate()
        {
            try
            {
                string input = calculadoraInput.Text.Trim();
                string mode = calcTemplate == null ? "Expresión libre" : calcTemplate.Text;
                double result;
                if (mode.StartsWith("Porcentaje"))
                {
                    double[] values = ParseNumberList(input, 2);
                    result = values[0] * values[1] / 100.0;
                }
                else if (mode.StartsWith("Descuento"))
                {
                    double[] values = ParseNumberList(input, 2);
                    result = values[0] * (1.0 - values[1] / 100.0);
                }
                else if (mode.StartsWith("Aumento"))
                {
                    double[] values = ParseNumberList(input, 2);
                    result = values[0] * (1.0 + values[1] / 100.0);
                }
                else if (mode.StartsWith("IVA"))
                {
                    double[] values = ParseNumberList(input, 2);
                    result = values[0] * (1.0 + values[1] / 100.0);
                }
                else if (mode.StartsWith("Media"))
                {
                    double[] values = ParseNumberList(input, 1);
                    result = 0;
                    foreach (double value in values) result += value;
                    result /= values.Length;
                }
                else if (mode.StartsWith("Regla de tres"))
                {
                    double[] values = ParseNumberList(input, 3);
                    if (values[0] == 0) throw new Exception("El primer valor no puede ser cero.");
                    result = values[1] * values[2] / values[0];
                }
                else if (mode.StartsWith("Raíz"))
                {
                    double[] values = ParseNumberList(input, 1);
                    if (values[0] < 0) throw new Exception("No se puede calcular una raíz real de un número negativo.");
                    result = Math.Sqrt(values[0]);
                }
                else if (mode.StartsWith("Potencia"))
                {
                    double[] values = ParseNumberList(input, 2);
                    result = Math.Pow(values[0], values[1]);
                }
                else
                {
                    string expression = input.Replace(',', '.');
                    if (!Regex.IsMatch(expression, @"^[0-9\s\.\+\-\*\/\(\)%]+$")) throw new Exception("La expresión contiene caracteres no permitidos.");
                    expression = Regex.Replace(expression, @"([0-9\.]+)%", "($1/100)");
                    System.Data.DataTable table = new System.Data.DataTable();
                    object value = table.Compute(expression, "");
                    result = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                }
                calculadoraResultado.Text = result.ToString("0.############", new CultureInfo("es-ES"));
                ScheduleCalculatorSound1554("calc_resultado", 125);
                SetStatus("Resultado listo.");
                AnnounceToScreenReader("Resultado: " + calculadoraResultado.Text + ".");
            }
            catch (Exception ex)
            {
                calculadoraResultado.Text = "No se pudo calcular: " + ex.Message;
                PlayNamedSound("error");
                SetStatus("Entrada no válida.");
                AnnounceToScreenReader(calculadoraResultado.Text);
            }
        }
        private void RollDice()
        {
            LoadDiceControlsFromNotation1532();
            TextBox target2824 = diceResult2824 != null ? diceResult2824 : calculadoraResultado;
            Match match = Regex.Match(dadosInput.Text.Trim(), @"^(\d+)d(\d+)([+-]\d+)?$", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                target2824.Text = "Formato no válido. Usa, por ejemplo, 3d6+2.";
                PlayNamedSound("error");
                AnnounceToScreenReader(target2824.Text);
                return;
            }
            int count = int.Parse(match.Groups[1].Value);
            int sides = int.Parse(match.Groups[2].Value);
            int modifier = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 0;
            if (count < 1 || count > 100 || sides < 2 || sides > 100000)
            {
                target2824.Text = "La cantidad o el número de caras está fuera del límite.";
                PlayNamedSound("error");
                AnnounceToScreenReader(target2824.Text);
                return;
            }
            PlayNamedSound("dados");
            List<int> rolls = new List<int>();
            int total = modifier;
            for (int i = 0; i < count; i++) { int roll = random.Next(1, sides + 1); rolls.Add(roll); total += roll; }
            target2824.Text = "Resultados: " + string.Join(", ", rolls.ConvertAll<string>(delegate(int n) { return n.ToString(); }).ToArray()) + ". Modificador: " + modifier + ". Total: " + total + ".";
            if (statusLabel != null) statusLabel.Text = "Tirada de dados realizada.";
            AddCalculatorHistory2824("Dados", dadosInput.Text, target2824.Text, "dice");
            AnnounceToScreenReader(target2824.Text);
            target2824.SelectionStart = 0;
            target2824.SelectionLength = 0;
            target2824.Focus();
        }
        private void CheckDate()
        {
            TextBox target2824 = simpleDateResult2824 != null ? simpleDateResult2824 : calculadoraResultado;
            DateTime date;
            if (!DateTime.TryParseExact(fechaInput.Text.Trim(), "dd/MM/yyyy", new CultureInfo("es-ES"), DateTimeStyles.None, out date))
            {
                target2824.Text = "La fecha no es válida. Usa día/mes/año.";
                PlayNamedSound("error");
                AnnounceToScreenReader(target2824.Text);
                return;
            }
            int difference = (date.Date - DateTime.Today).Days;
            string relation = difference == 0 ? "Es hoy." : difference > 0 ? "Faltan " + difference + " días." : "Fue hace " + Math.Abs(difference) + " días.";
            target2824.Text = date.ToString("dddd d 'de' MMMM 'de' yyyy", new CultureInfo("es-ES")) + ". " + relation;
            AddCalculatorHistory2824("Consulta de fecha", fechaInput.Text, target2824.Text, "simpledate");
            PlayCalculatorSound1554("calc_resultado");
            SetStatus("Fecha consultada.");
            AnnounceResults(target2824, target2824.Text);
        }
        private string DownloadText(string url)
        {
            try
            {
                using (WebClient client = NewWebClient()) return client.DownloadString(url);
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo consultar " + NetworkHost153(url) + ". " + DescribeNetworkException153(ex), ex);
            }
        }
        private string GetString(Dictionary<string, object> parent, string key)
        {
            if (parent == null) return "";
            object value;
            if (!parent.TryGetValue(key, out value) || value == null) return "";
            return Convert.ToString(value, CultureInfo.CurrentCulture) ?? "";
        }
        private IList GetList(Dictionary<string, object> parent, string key)
        {
            if (parent == null) return new ArrayList();
            object value;
            if (!parent.TryGetValue(key, out value)) return new ArrayList();
            IList list = value as IList;
            return list ?? new ArrayList();
        }
        private int ToInt(Dictionary<string, object> parent, string key)
        {
            try { return Convert.ToInt32(parent[key], CultureInfo.InvariantCulture); }
            catch { return -1; }
        }
        private void ApplyCalculatorTemplate()
        {
            if (calcTemplate == null || calculadoraInput == null) return;
            string mode = calcTemplate.Text;
            if (mode.StartsWith("Porcentaje")) calculadoraInput.Text = "15;200";
            else if (mode.StartsWith("Descuento")) calculadoraInput.Text = "100;20";
            else if (mode.StartsWith("Aumento")) calculadoraInput.Text = "100;10";
            else if (mode.StartsWith("IVA")) calculadoraInput.Text = "100;21";
            else if (mode.StartsWith("Media")) calculadoraInput.Text = "10;20;30";
            else if (mode.StartsWith("Regla de tres")) calculadoraInput.Text = "2;10;5";
            else if (mode.StartsWith("Raíz")) calculadoraInput.Text = "81";
            else if (mode.StartsWith("Potencia")) calculadoraInput.Text = "2;8";
            else calculadoraInput.Text = "(25 * 79,90) / 100";
        }
        private double[] ParseNumberList(string input, int minimum)
        {
            string normalized = input.Replace("\r", ";").Replace("\n", ";");
            string[] parts = normalized.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < minimum) throw new System.Exception("Escribe al menos " + minimum + " valores separados por punto y coma.");
            double[] values = new double[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                string number = parts[i].Trim().Replace(',', '.');
                if (!double.TryParse(number, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out values[i]))
                    throw new System.Exception("No se reconoce el número: " + parts[i].Trim());
            }
            return values;
        }
        private System.Windows.Forms.TableLayoutPanel BuildCalculatorKeypad()
        {
            System.Windows.Forms.TableLayoutPanel pad = new System.Windows.Forms.TableLayoutPanel();
            pad.AutoSize = true;
            pad.ColumnCount = 4;
            pad.RowCount = 5;
            string[,] labels = new string[,] {
                { "7", "8", "9", "Dividir" },
                { "4", "5", "6", "Multiplicar" },
                { "1", "2", "3", "Restar" },
                { "0", "Coma decimal", "Porcentaje", "Sumar" },
                { "Abrir paréntesis", "Cerrar paréntesis", "Borrar último", "Igual" }
            };
            for (int r = 0; r < labels.GetLength(0); r++)
            {
                for (int c = 0; c < labels.GetLength(1); c++)
                {
                    string label = labels[r, c];
                    System.Windows.Forms.Button button = new System.Windows.Forms.Button();
                    button.Text = label;
                    button.AutoSize = true;
                    button.AccessibleName = label;
                    button.Tag = label;
                    button.Click += delegate(object sender, System.EventArgs e)
                    {
                        System.Windows.Forms.Button source = sender as System.Windows.Forms.Button;
                        if (source == null) return;
                        HandleCalculatorButton(System.Convert.ToString(source.Tag));
                    };
                    pad.Controls.Add(button, c, r);
                }
            }
            return pad;
        }
        private void HandleCalculatorButton(string label)
        {
            if (label == "Igual") { RunCalculator1554(); return; }
            if (label == "Borrar último")
            {
                PlayCalculatorSound1554("calc_borrar");
                if (calculadoraInput.Text.Length > 0) calculadoraInput.Text = calculadoraInput.Text.Substring(0, calculadoraInput.Text.Length - 1);
                calculadoraInput.SelectionStart = calculadoraInput.Text.Length;
                calculadoraInput.Focus();
                return;
            }
            string value = label;
            if (label == "Dividir") value = "/";
            else if (label == "Multiplicar") value = "*";
            else if (label == "Restar") value = "-";
            else if (label == "Sumar") value = "+";
            else if (label == "Coma decimal") value = ",";
            else if (label == "Porcentaje") value = "%";
            else if (label == "Abrir paréntesis") value = "(";
            else if (label == "Cerrar paréntesis") value = ")";
            if (label == "Coma decimal") PlayCalculatorSound1554("calc_decimal");
            else if (label.Length == 1 && char.IsDigit(label[0])) PlayCalculatorSound1554("calc_numero");
            else PlayCalculatorSound1554("calc_operacion");
            int start = calculadoraInput.SelectionStart;
            calculadoraInput.Text = calculadoraInput.Text.Insert(start, value);
            calculadoraInput.SelectionStart = start + value.Length;
            calculadoraInput.Focus();
        }
        private void AddAdvancedDiceControls1532(TableLayoutPanel layout)
        {
            diceCount1532 = AddNumeric(layout, "Cantidad de dados:", 1, 100, 3);
            diceSides1532 = AddLabeledCombo(layout, "Caras de cada dado:", new string[] { "2", "4", "6", "8", "10", "12", "20", "100" });
            diceSides1532.DropDownStyle = ComboBoxStyle.DropDown;
            diceSides1532.Text = "6";
            diceSides1532.AccessibleDescription = "Puedes elegir un valor habitual o escribir otro número de caras entre dos y cien mil.";
            diceModifier1532 = AddNumeric(layout, "Modificador que se suma o resta:", -100000, 100000, 2);

            diceCount1532.ValueChanged += delegate { UpdateDiceNotationFromControls1532(); };
            diceSides1532.TextChanged += delegate { UpdateDiceNotationFromControls1532(); };
            diceModifier1532.ValueChanged += delegate { UpdateDiceNotationFromControls1532(); };
            if (dadosInput != null)
            {
                dadosInput.Leave += delegate { LoadDiceControlsFromNotation1532(); };
                dadosInput.KeyDown += delegate(object sender, KeyEventArgs e)
                {
                    if (e.KeyCode == Keys.Enter) { RollDice(); e.SuppressKeyPress = true; }
                };
            }

            FlowLayoutPanel presets = ButtonRow();
            presets.Controls.Add(NewButton("Un dado de 6", delegate { SetDicePreset1532(1, 6, 0); }));
            presets.Controls.Add(NewButton("Un dado de 20", delegate { SetDicePreset1532(1, 20, 0); }));
            presets.Controls.Add(NewButton("Dos dados de 6", delegate { SetDicePreset1532(2, 6, 0); }));
            presets.Controls.Add(NewButton("Percentil, d100", delegate { SetDicePreset1532(1, 100, 0); }));
            AddFullRow(layout, presets, false);
            UpdateDiceNotationFromControls1532();
        }
        private void SetDicePreset1532(int count, int sides, int modifier)
        {
            diceControlsUpdating1532 = true;
            try
            {
                diceCount1532.Value = count;
                diceSides1532.Text = sides.ToString();
                diceModifier1532.Value = modifier;
            }
            finally { diceControlsUpdating1532 = false; }
            UpdateDiceNotationFromControls1532();
            RollDice();
        }
        private void UpdateDiceNotationFromControls1532()
        {
            if (diceControlsUpdating1532 || dadosInput == null || diceCount1532 == null || diceSides1532 == null || diceModifier1532 == null) return;
            int sides;
            if (!int.TryParse(diceSides1532.Text.Trim(), out sides) || sides < 2 || sides > 100000) return;
            int modifier = (int)diceModifier1532.Value;
            dadosInput.Text = ((int)diceCount1532.Value).ToString() + "d" + sides.ToString() + (modifier > 0 ? "+" + modifier.ToString() : modifier < 0 ? modifier.ToString() : "");
        }
        private void LoadDiceControlsFromNotation1532()
        {
            if (dadosInput == null || diceCount1532 == null || diceSides1532 == null || diceModifier1532 == null) return;
            Match match = Regex.Match(dadosInput.Text.Trim(), @"^(\d+)d(\d+)([+-]\d+)?$", RegexOptions.IgnoreCase);
            if (!match.Success) return;
            int count;
            int sides;
            int modifier = 0;
            if (!int.TryParse(match.Groups[1].Value, out count) || !int.TryParse(match.Groups[2].Value, out sides)) return;
            if (match.Groups[3].Success) int.TryParse(match.Groups[3].Value, out modifier);
            if (count < 1 || count > 100 || sides < 2 || sides > 100000 || modifier < -100000 || modifier > 100000) return;
            diceControlsUpdating1532 = true;
            try
            {
                diceCount1532.Value = count;
                diceSides1532.Text = sides.ToString();
                diceModifier1532.Value = modifier;
            }
            finally { diceControlsUpdating1532 = false; }
        }
        private System.Windows.Forms.NumericUpDown AddNumeric(System.Windows.Forms.TableLayoutPanel layout, string labelText, decimal min, decimal max, decimal value)
        {
            int row = layout.RowCount++;
            layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            System.Windows.Forms.Label label = new System.Windows.Forms.Label();
            label.Text = labelText;
            label.AutoSize = true;
            label.Anchor = System.Windows.Forms.AnchorStyles.Left;
            System.Windows.Forms.NumericUpDown number = new System.Windows.Forms.NumericUpDown();
            number.Minimum = min;
            number.Maximum = max;
            number.Value = value;
            number.Dock = System.Windows.Forms.DockStyle.Top;
            number.AccessibleName = labelText.TrimEnd(':');
            layout.Controls.Add(label, 0, row);
            layout.Controls.Add(number, 1, row);
            return number;
        }
        private void InstallCalculatorRuntimePerformance2930()
        {
            if (sellerPerformanceInstalled2930) return;
            sellerPerformanceInstalled2930 = true;
            FormClosing += delegate
            {
                try { FlushSellerDraftSave2930(); } catch { }
            };
        }
        private void ScheduleSellerDraftSave2930()
        {
            sellerDraftSavePending2930 = true;
            if (sellerDraftSaveTimer2930 == null)
            {
                sellerDraftSaveTimer2930 = new Timer();
                sellerDraftSaveTimer2930.Interval = 220;
                sellerDraftSaveTimer2930.Tick += delegate
                {
                    sellerDraftSaveTimer2930.Stop();
                    FlushSellerDraftSave2930();
                };
            }
            sellerDraftSaveTimer2930.Stop();
            sellerDraftSaveTimer2930.Start();
        }
        private void FlushSellerDraftSave2930()
        {
            if (sellerDraftSaveTimer2930 != null) sellerDraftSaveTimer2930.Stop();
            if (!sellerDraftSavePending2930) return;
            sellerDraftSavePending2930 = false;
            Stopwatch watch2930 = Stopwatch.StartNew();
            SaveSellerDraft2825();
            watch2930.Stop();
            LogCalculatorPerformance2930("Guardar borrador de liquidación", watch2930.ElapsedMilliseconds);
        }
        private void RefreshSellerRowAfterMutation2930(int index2930, bool appended2930)
        {
            Stopwatch watch2930 = Stopwatch.StartNew();
            if (sellerRows2825 == null || index2930 < 0 || index2930 >= sellerLines2825.Count)
            {
                RefreshSellerRows2825(index2930);
                watch2930.Stop();
                LogCalculatorPerformance2930("Refresco completo de liquidación por desajuste", watch2930.ElapsedMilliseconds);
                return;
            }

            int expectedBefore2930 = appended2930 ? sellerLines2825.Count - 1 : sellerLines2825.Count;
            if (sellerRows2825.Items.Count != expectedBefore2930)
            {
                RefreshSellerRows2825(index2930);
                watch2930.Stop();
                LogCalculatorPerformance2930("Refresco completo de liquidación por desajuste", watch2930.ElapsedMilliseconds);
                return;
            }

            bool hadFocus2930 = sellerRows2825.ContainsFocus;
            sellerLoading2825 = true;
            sellerRows2825.BeginUpdate();
            try
            {
                if (appended2930)
                    sellerRows2825.Items.Add(sellerLines2825[index2930]);
                else
                    sellerRows2825.Items[index2930] = sellerLines2825[index2930];
                sellerRows2825.SelectedIndex = index2930;
            }
            finally
            {
                sellerRows2825.EndUpdate();
                sellerLoading2825 = false;
            }
            if (sellerTotals2825 != null) sellerTotals2825.Text = SellerTotalsText2825();
            if (hadFocus2930) sellerRows2825.Focus();
            watch2930.Stop();
            LogCalculatorPerformance2930(appended2930 ? "Añadir fila de liquidación" : "Actualizar fila de liquidación", watch2930.ElapsedMilliseconds);
        }
        private void RefreshSellerAfterDelete2930(int removedIndex2930)
        {
            Stopwatch watch2930 = Stopwatch.StartNew();
            if (sellerRows2825 == null)
            {
                if (sellerTotals2825 != null) sellerTotals2825.Text = SellerTotalsText2825();
                return;
            }
            if (removedIndex2930 < 0 || sellerRows2825.Items.Count != sellerLines2825.Count + 1)
            {
                int fallback2930 = sellerLines2825.Count == 0 ? -1 : Math.Max(0, Math.Min(removedIndex2930, sellerLines2825.Count - 1));
                RefreshSellerRows2825(fallback2930);
                watch2930.Stop();
                LogCalculatorPerformance2930("Refresco completo tras borrar fila por desajuste", watch2930.ElapsedMilliseconds);
                return;
            }

            bool hadFocus2930 = sellerRows2825.ContainsFocus;
            sellerLoading2825 = true;
            sellerRows2825.BeginUpdate();
            try
            {
                sellerRows2825.Items.RemoveAt(removedIndex2930);
                if (sellerRows2825.Items.Count > 0)
                    sellerRows2825.SelectedIndex = Math.Max(0, Math.Min(removedIndex2930, sellerRows2825.Items.Count - 1));
                else
                    sellerRows2825.SelectedIndex = -1;
            }
            finally
            {
                sellerRows2825.EndUpdate();
                sellerLoading2825 = false;
            }
            if (sellerTotals2825 != null) sellerTotals2825.Text = SellerTotalsText2825();
            if (hadFocus2930) sellerRows2825.Focus();
            watch2930.Stop();
            LogCalculatorPerformance2930("Eliminar fila de liquidación", watch2930.ElapsedMilliseconds);
        }
        private void LogCalculatorPerformance2930(string operation2930, long elapsed2930)
        {
            if (elapsed2930 < 80) return;
            try
            {
                string line2930 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | " + operation2930 + " | " + elapsed2930.ToString(CultureInfo.InvariantCulture) + " ms";
                AppendBoundedPerformanceLine2917(Path.Combine(dataDir, "rendimiento_calculadora.txt"), line2930);
            }
            catch { }
        }
        private void RecordSellerLiquidationSyncTombstone2878(SellerLiquidation2826 item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.Id)) return;
            string path = Path.Combine(dataDir, "navaja-liquidacion-tombstones.json");
            JavaScriptSerializer serializer = new JavaScriptSerializer(); serializer.MaxJsonLength = 20 * 1024 * 1024;
            ArrayList entries = new ArrayList();
            try { if (File.Exists(path)) { IList old = serializer.DeserializeObject(File.ReadAllText(path, Encoding.UTF8)) as IList; if (old != null) entries.AddRange(old); } } catch { }
            string now = string.IsNullOrWhiteSpace(item.DeletedAt) ? DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture) : item.DeletedAt;
            Dictionary<string, object> tombstone = new Dictionary<string, object>(); tombstone["id"] = item.Id; tombstone["fecha"] = item.When; tombstone["nota"] = item.Note; tombstone["filas"] = new ArrayList(); tombstone["correcciones"] = new ArrayList(); tombstone["updated_at"] = now; tombstone["deleted_at"] = now; tombstone["device_id"] = "windows-pc";
            for (int i = entries.Count - 1; i >= 0; i--) { Dictionary<string, object> old = entries[i] as Dictionary<string, object>; if (old != null && string.Equals(MailValue292(old, "id"), item.Id, StringComparison.OrdinalIgnoreCase)) entries.RemoveAt(i); }
            entries.Add(tombstone);
            try { Directory.CreateDirectory(dataDir); File.WriteAllText(path, serializer.Serialize(entries), new UTF8Encoding(false)); } catch { }
        }
}}