using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace TodoAMano
{
    public partial class MainForm
    {
        private TabControl calculatorTabs2824;
        private TabPage calculatorBasicPage2824;
        private TabPage calculatorAdvancedPage2824;
        private TabPage calculatorConvertPage2824;
        private TabPage calculatorCryptoPage2824;
        private TabPage calculatorDatesPage2824;
        private TabPage calculatorSellerPage2825;
        private TabPage calculatorSellerHistoryPage2826;
        private TabPage calculatorHistoryPage2824;

        private ComboBox advancedCategory2824;
        private ComboBox advancedOperation2824;
        private TextBox advancedInput2824;
        private TextBox advancedResult2824;
        private double calculatorMemory2824;
        private bool calculatorMemorySet2824;

        private TextBox cryptoValue2824;
        private ComboBox cryptoFrom2824;
        private ComboBox cryptoTo2824;
        private TextBox cryptoResult2824;
        private Label cryptoStatus2824;
        private bool cryptoBusy2824;
        private readonly Dictionary<string, CryptoPrice2824> cryptoPrices2824 = new Dictionary<string, CryptoPrice2824>(StringComparer.OrdinalIgnoreCase);

        private TextBox diceResult2824;
        private TextBox simpleDateResult2824;
        private ComboBox dateOperation2824;
        private TextBox dateAdvancedInput2824;
        private TextBox dateAdvancedResult2824;

        private ListBox calculatorHistoryList2824;
        private TextBox calculatorHistoryDetail2824;
        private readonly List<CalculatorHistoryItem2824> calculatorHistory2824 = new List<CalculatorHistoryItem2824>();
        private string calculatorHistoryPath2824 = "";
        private bool calculatorHistoryLoading2824;
        private string lastBasicHistory2824 = "";
        private string lastUnitHistory2824 = "";
        private string lastCurrencyHistory2824 = "";

        private ComboBox sellerType2825;
        private ComboBox sellerDenomination2825;
        private TextBox sellerQuantity2825;
        private ListBox sellerRows2825;
        private TextBox sellerTotals2825;
        private readonly List<SellerLine2825> sellerLines2825 = new List<SellerLine2825>();
        private string sellerDraftPath2825 = "";
        private bool sellerLoading2825;
        private static readonly CultureInfo SellerCulture2825 = CultureInfo.GetCultureInfo("es-ES");
        private readonly Dictionary<string, SellerDenominationChoice2825[]> sellerDenominationCatalog2825 =
            new Dictionary<string, SellerDenominationChoice2825[]>(StringComparer.Ordinal);
        private string sellerDenominationCacheKey2825 = "";
        private bool sellerDenominationUpdating2825;

        private DateTimePicker sellerRecordDate2826;
        private TextBox sellerRecordNote2826;
        private ListBox sellerSavedList2826;
        private TextBox sellerSavedDetail2826;
        private TextBox sellerSavedSummary2826;
        private DateTimePicker sellerFilterFrom2826;
        private DateTimePicker sellerFilterTo2826;
        private readonly List<SellerLiquidation2826> sellerSaved2826 = new List<SellerLiquidation2826>();
        private readonly List<SellerLiquidation2826> sellerVisibleSaved2826 = new List<SellerLiquidation2826>();
        private string sellerSavedPath2826 = "";
        private bool sellerSavedLoading2826;
        private string sellerCorrectionId2827 = "";
        private TextBox sellerCorrectionStatus2827;

        private sealed class SellerDenominationChoice2825
        {
            public string Label;
            public double Value;
            public int UnitsPerContainer;
            public SellerDenominationChoice2825(string label, double value, int unitsPerContainer)
            {
                Label = label; Value = value; UnitsPerContainer = unitsPerContainer;
            }
            public override string ToString() { return Label; }
        }

        private sealed class SellerLine2825
        {
            public string Type = "";
            public string Denomination = "";
            public int Quantity;
            public double UnitValue;
            public int UnitsPerContainer;
            public double Subtotal { get { return Quantity * UnitValue; } }

            private bool displayCacheValid2825;
            private string displayCacheType2825 = "";
            private string displayCacheDenomination2825 = "";
            private int displayCacheQuantity2825;
            private double displayCacheUnitValue2825;
            private int displayCacheUnitsPerContainer2825;
            private string displayCacheText2825 = "";

            public override string ToString()
            {
                if (displayCacheValid2825 &&
                    displayCacheQuantity2825 == Quantity &&
                    displayCacheUnitValue2825 == UnitValue &&
                    displayCacheUnitsPerContainer2825 == UnitsPerContainer &&
                    string.Equals(displayCacheType2825, Type, StringComparison.Ordinal) &&
                    string.Equals(displayCacheDenomination2825, Denomination, StringComparison.Ordinal))
                    return displayCacheText2825;

                string money = Subtotal.ToString("N2", MainForm.SellerCulture2825) + " €";
                if (Type.StartsWith("Blíster", StringComparison.OrdinalIgnoreCase))
                    displayCacheText2825 = Quantity + (Quantity == 1 ? " blíster" : " blísteres") + " de " + Denomination + " (" + UnitsPerContainer + " monedas cada uno) = " + money;
                else if (Type == "Billete")
                    displayCacheText2825 = Quantity + (Quantity == 1 ? " billete" : " billetes") + " de " + Denomination + " = " + money;
                else
                    displayCacheText2825 = Quantity + (Quantity == 1 ? " moneda" : " monedas") + " de " + Denomination + " = " + money;

                displayCacheType2825 = Type;
                displayCacheDenomination2825 = Denomination;
                displayCacheQuantity2825 = Quantity;
                displayCacheUnitValue2825 = UnitValue;
                displayCacheUnitsPerContainer2825 = UnitsPerContainer;
                displayCacheValid2825 = true;
                return displayCacheText2825;
            }
        }

        private sealed class SellerRevision2827
        {
            public string CorrectedAt = "";
            public string PreviousWhen = "";
            public string PreviousNote = "";
            public string ChangeSummary = "";
            public readonly List<SellerLine2825> PreviousLines = new List<SellerLine2825>();

            public double PreviousTotal
            {
                get
                {
                    double total = 0;
                    foreach (SellerLine2825 line in PreviousLines) total += line.Subtotal;
                    return total;
                }
            }

            public DateTime CorrectedDateValue
            {
                get
                {
                    DateTime parsed;
                    if (DateTime.TryParse(CorrectedAt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out parsed)) return parsed;
                    return DateTime.MinValue;
                }
            }
        }

        private sealed class SellerLiquidation2826
        {
            public string Id = "";
            public string When = "";
            public string Note = "";
            public string UpdatedAt = "";
            public string DeletedAt = "";
            public string DeviceId = "";
            public string CloudState2962 = "";
            public readonly List<SellerLine2825> Lines = new List<SellerLine2825>();
            public readonly List<SellerRevision2827> Revisions = new List<SellerRevision2827>();

            public double Total
            {
                get
                {
                    double total = 0;
                    foreach (SellerLine2825 line in Lines) total += line.Subtotal;
                    return total;
                }
            }

            public DateTime DateValue
            {
                get
                {
                    DateTime parsed;
                    if (DateTime.TryParse(When, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out parsed)) return parsed;
                    if (DateTime.TryParse(When, SellerCulture2825, DateTimeStyles.None, out parsed)) return parsed;
                    return DateTime.MinValue;
                }
            }

            public override string ToString()
            {
                string label = DateValue == DateTime.MinValue ? When : DateValue.ToString("dd/MM/yyyy HH:mm");
                string note = string.IsNullOrWhiteSpace(Note) ? "" : " — " + Note.Trim();
                string corrections = Revisions.Count == 0 ? "" : " — corregida " + Revisions.Count + (Revisions.Count == 1 ? " vez" : " veces");
                return label + note + " — " + Total.ToString("N2", SellerCulture2825) + " € — " + Lines.Count + (Lines.Count == 1 ? " fila" : " filas") + corrections + (CloudState2962.Length == 0 ? "" : " — " + CloudState2962);
            }
        }

        private sealed class CryptoPrice2824
        {
            public double EuroValue;
            public DateTime Updated;
            public string Source = "";
        }

        private sealed class CalculatorHistoryItem2824
        {
            public string When = "";
            public string Kind = "";
            public string Input = "";
            public string Result = "";
            public string ReuseTarget = "";

            public override string ToString()
            {
                string result = Result == null ? "" : Result.Replace("\r", " ").Replace("\n", " ").Trim();
                if (result.Length > 65) result = result.Substring(0, 65) + "…";
                return When + " — " + Kind + " — " + result;
            }
        }

        private sealed class CryptoChoice2824
        {
            public string Code;
            public string Name;
            public bool IsCrypto;
            public CryptoChoice2824(string code, string name, bool isCrypto)
            {
                Code = code;
                Name = name;
                IsCrypto = isCrypto;
            }
            public override string ToString() { return Name + " (" + Code + ")"; }
        }

        private void BuildCalculatorSuite2824(TabPage page)
        {
            calculatorHistoryPath2824 = Path.Combine(dataDir, "calculadora_historial.json");
            sellerDraftPath2825 = Path.Combine(dataDir, "calculadora_vendedor_borrador.json");
            sellerSavedPath2826 = Path.Combine(dataDir, "calculadora_vendedor_liquidaciones.json");
            InstallCalculatorRuntimePerformance2930();
            LoadCalculatorHistory2824();
            LoadSellerDraft2825();
            LoadSellerLiquidations2826();

            calculatorTabs2824 = new TabControl();
            calculatorTabs2824.Dock = DockStyle.Fill;
            calculatorTabs2824.Multiline = true;
            calculatorTabs2824.AccessibleName = "Páginas de Calculadora y utilidades";

            calculatorBasicPage2824 = BuildCalculatorBasicPage2824();
            calculatorAdvancedPage2824 = BuildCalculatorAdvancedPage2824();
            calculatorConvertPage2824 = BuildCalculatorConvertPage2824();
            calculatorCryptoPage2824 = BuildCalculatorCryptoPage2824();
            calculatorDatesPage2824 = BuildCalculatorDatesPage2824();
            calculatorSellerPage2825 = BuildCalculatorSellerPage2825();
            calculatorSellerHistoryPage2826 = BuildCalculatorSellerHistoryPage2826();
            calculatorHistoryPage2824 = BuildCalculatorHistoryPage2824();

            calculatorTabs2824.TabPages.Add(calculatorBasicPage2824);
            calculatorTabs2824.TabPages.Add(calculatorAdvancedPage2824);
            calculatorTabs2824.TabPages.Add(calculatorConvertPage2824);
            calculatorTabs2824.TabPages.Add(calculatorCryptoPage2824);
            calculatorTabs2824.TabPages.Add(calculatorDatesPage2824);
            calculatorTabs2824.TabPages.Add(calculatorSellerPage2825);
            calculatorTabs2824.TabPages.Add(calculatorSellerHistoryPage2826);
            calculatorTabs2824.TabPages.Add(calculatorHistoryPage2824);
            calculatorTabs2824.SelectedIndexChanged += delegate
            {
                if (calculatorTabs2824.SelectedTab != null)
                    AnnounceToScreenReader("Calculadora. " + calculatorTabs2824.SelectedTab.Text + ".");
            };

            page.Controls.Clear();
            page.Controls.Add(calculatorTabs2824);
        }

        private TabPage BuildCalculatorBasicPage2824()
        {
            TabPage page = new TabPage("Calcular");
            TableLayoutPanel layout = BaseLayout();

            TextBox intro = ResultBox();
            intro.Height = 62;
            intro.Text = "La calculadora básica conserva el teclado y las operaciones rápidas. Puedes copiar el resultado, reutilizarlo, guardarlo en una memoria temporal o pedir una curiosidad matemática sobre él.";
            AddResultRow(layout, "Uso rápido:", intro);

            calcTemplate = AddLabeledCombo(layout, "Operación rápida:", new string[] {
                "Expresión libre", "Porcentaje: A por ciento de B", "Descuento: precio y porcentaje",
                "Aumento: cantidad y porcentaje", "IVA: base y porcentaje", "Media de varios números",
                "Regla de tres: A es a B como C es a X", "Raíz cuadrada", "Potencia"
            });
            calcTemplate.SelectedIndexChanged += delegate
            {
                ApplyCalculatorTemplate();
                if (calcTemplate.Focused) PlayCalculatorSound1554("calc_operacion");
            };

            calculadoraInput = AddLabeledText(layout, "Cálculo o datos:", false);
            calculadoraInput.Text = "(25 * 79,90) / 100";
            AttachCalculatorTextBox1554(calculadoraInput, delegate { RunCalculator1554(); });

            TableLayoutPanel keypad = BuildCalculatorKeypad();
            AddFullRow(layout, keypad, false);
            CheckBox showKeypad = new CheckBox { Text = "Mostrar teclado de números y signos", AutoSize = true, Checked = File.Exists(Path.Combine(dataDir, "mostrar_teclado.txt")) };
            keypad.Visible = showKeypad.Checked;
            showKeypad.CheckedChanged += delegate { keypad.Visible = showKeypad.Checked; string preference = Path.Combine(dataDir, "mostrar_teclado.txt"); if (showKeypad.Checked) File.WriteAllText(preference, "1"); else if (File.Exists(preference)) File.Delete(preference); };
            AddFullRow(layout, showKeypad, false);

            FlowLayoutPanel buttons = ButtonRow();
            buttons.Controls.Add(NewButton("Calcular", delegate { RunCalculator1554(); }));
            buttons.Controls.Add(NewButton("Copiar resultado", delegate { CopyBasicCalculatorResult2824(); }));
            buttons.Controls.Add(NewButton("Usar resultado", delegate { UseBasicCalculatorResult2824(); }));
            buttons.Controls.Add(NewButton("Guardar en memoria", delegate { SaveCalculatorMemory2824(); }));
            buttons.Controls.Add(NewButton("Recuperar memoria", delegate { RecallCalculatorMemory2824(); }));
            buttons.Controls.Add(NewButton("Borrar", delegate { ClearCalculator1554(); }));
            AddFullRow(layout, buttons, false);

            calculadoraResultado = AddLabeledText(layout, "Resultado:", false);
            calculadoraResultado.ReadOnly = true;
            calculadoraResultado.TextChanged += delegate { CaptureBasicHistory2824(); };

            FlowLayoutPanel curiosityButtons = ButtonRow();
            curiosityButtons.Controls.Add(NewButton("Curiosidad del resultado", delegate { DescribeResultCuriosity2824(calculadoraResultado == null ? "" : calculadoraResultado.Text); }));
            curiosityButtons.Controls.Add(NewButton("Abrir historial", delegate { SelectCalculatorHistory2824(); }));
            AddFullRow(layout, curiosityButtons, false);

            page.Controls.Add(layout);
            return page;
        }

        private TabPage BuildCalculatorAdvancedPage2824()
        {
            TabPage page = new TabPage("Práctica y científica");
            TableLayoutPanel layout = BaseLayout();

            advancedCategory2824 = AddLabeledCombo(layout, "Categoría:", new string[]
            {
                "Porcentajes y compras", "Estadística", "Científica", "Finanzas", "Bases numéricas"
            });
            advancedCategory2824.SelectedIndexChanged += delegate { RefreshAdvancedOperations2824(); };
            advancedOperation2824 = AddLabeledCombo(layout, "Operación:", new string[0]);
            advancedOperation2824.SelectedIndexChanged += delegate { ApplyAdvancedExample2824(); };
            RefreshAdvancedOperations2824();

            advancedInput2824 = AddLabeledText(layout, "Valores:", false);
            AttachCalculatorTextBox1554(advancedInput2824, delegate { CalculateAdvanced2824(); });
            ApplyAdvancedExample2824();

            FlowLayoutPanel buttons = ButtonRow();
            buttons.Controls.Add(NewButton("Calcular", delegate { CalculateAdvanced2824(); }));
            buttons.Controls.Add(NewButton("Copiar resultado", delegate { CopyTextResult2824(advancedResult2824); }));
            buttons.Controls.Add(NewButton("Usar resultado en calculadora", delegate { UseAdvancedResultInBasic2824(); }));
            buttons.Controls.Add(NewButton("Guardar en memoria", delegate { SaveTextResultToMemory2824(advancedResult2824); }));
            buttons.Controls.Add(NewButton("Borrar campos", delegate { ClearAdvanced2824(); }));
            AddFullRow(layout, buttons, false);

            advancedResult2824 = AddLabeledText(layout, "Resultado:", true);
            advancedResult2824.ReadOnly = true;
            advancedResult2824.Height = 90;

            TextBox help = ResultBox();
            help.Height = 115;
            help.Text = "Los valores se separan con punto y coma. Ejemplos: cambio porcentual 100;125. Propina y reparto 40;10;2. Interés compuesto 1000;5;10;12 significa capital, porcentaje anual, años y capitalizaciones por año. Cuota de préstamo 10000;6;36 significa principal, TAE aproximada y meses. En bases numéricas escribe un solo valor.";
            AddResultRow(layout, "Ayuda:", help);

            page.Controls.Add(layout);
            return page;
        }

        private TabPage BuildCalculatorConvertPage2824()
        {
            TabPage page = new TabPage("Conversiones");
            TableLayoutPanel layout = BaseLayout();
            AddConverters1545(layout);

            if (unitResult1545 != null) unitResult1545.TextChanged += delegate { CaptureUnitHistory2824(); };
            if (currencyResult1545 != null) currencyResult1545.TextChanged += delegate { CaptureCurrencyHistory2824(); };

            TextBox help = ResultBox();
            help.Height = 75;
            help.Text = "Todas las magnitudes usan el mismo patrón: categoría, cantidad, unidad de origen y unidad de destino. Además de las categorías anteriores se incluyen ángulo, energía, potencia, frecuencia, fuerza y densidad.";
            AddResultRow(layout, "Ayuda:", help);
            page.Controls.Add(layout);
            return page;
        }

        private TabPage BuildCalculatorCryptoPage2824()
        {
            TabPage page = new TabPage("Criptomonedas");
            TableLayoutPanel layout = BaseLayout();

            TextBox intro = ResultBox();
            intro.Height = 80;
            intro.Text = "Conversión orientativa entre criptomonedas habituales y monedas fiduciarias. Para cripto se consulta el precio spot público de Coinbase; para EUR, USD y GBP se reutilizan los tipos de cambio de Navaja. Los precios pueden cambiar en segundos y no constituyen una cotización para comprar o vender.";
            AddResultRow(layout, "Información:", intro);

            cryptoValue2824 = AddLabeledText(layout, "Cantidad:", false);
            cryptoValue2824.Text = "1";
            AttachCalculatorTextBox1554(cryptoValue2824, delegate { ConvertCrypto2824(false); });

            CryptoChoice2824[] choices = CryptoChoices2824();
            string[] labels = new string[choices.Length];
            for (int i = 0; i < choices.Length; i++) labels[i] = choices[i].ToString();
            cryptoFrom2824 = AddLabeledCombo(layout, "Activo de origen:", labels);
            cryptoTo2824 = AddLabeledCombo(layout, "Activo de destino:", labels);
            SetComboText2824(cryptoFrom2824, "Bitcoin (BTC)");
            SetComboText2824(cryptoTo2824, "euros (EUR)");

            FlowLayoutPanel buttons = ButtonRow();
            buttons.Controls.Add(NewButton("Convertir", delegate { ConvertCrypto2824(false); }));
            buttons.Controls.Add(NewButton("Actualizar precio y convertir", delegate { ConvertCrypto2824(true); }));
            buttons.Controls.Add(NewButton("Intercambiar", delegate { SwapComboIndexes2824(cryptoFrom2824, cryptoTo2824); }));
            buttons.Controls.Add(NewButton("Copiar resultado", delegate { CopyTextResult2824(cryptoResult2824); }));
            AddFullRow(layout, buttons, false);

            cryptoStatus2824 = new Label();
            cryptoStatus2824.AutoSize = true;
            cryptoStatus2824.Text = "Los precios se consultan al convertir y se reutilizan unos minutos para evitar consultas innecesarias.";
            cryptoStatus2824.AccessibleName = "Estado de la consulta de criptomonedas";
            AddFullRow(layout, cryptoStatus2824, false);

            cryptoResult2824 = AddLabeledText(layout, "Resultado:", true);
            cryptoResult2824.ReadOnly = true;
            cryptoResult2824.Height = 120;

            page.Controls.Add(layout);
            return page;
        }

        private TabPage BuildCalculatorDatesPage2824()
        {
            TabPage page = new TabPage("Fechas y azar");
            TableLayoutPanel layout = BaseLayout();

            Label diceHeading = new Label();
            diceHeading.AutoSize = true;
            diceHeading.Text = "DADOS";
            diceHeading.AccessibleName = "Dados";
            AddFullRow(layout, diceHeading, false);

            dadosInput = AddLabeledText(layout, "Tirada de dados:", false);
            dadosInput.Text = "3d6+2";
            AddAdvancedDiceControls1532(layout);
            FlowLayoutPanel diceButtons = ButtonRow();
            diceButtons.Controls.Add(NewButton("Tirar o tirar de nuevo", delegate { RollDice(); }));
            diceButtons.Controls.Add(NewButton("Copiar resultado", delegate { CopyTextResult2824(diceResult2824); }));
            AddFullRow(layout, diceButtons, false);
            diceResult2824 = AddLabeledText(layout, "Resultado de dados:", true);
            diceResult2824.ReadOnly = true;
            diceResult2824.Height = 72;

            Label simpleDateHeading = new Label();
            simpleDateHeading.AutoSize = true;
            simpleDateHeading.Text = "CONSULTA DE FECHA";
            simpleDateHeading.AccessibleName = "Consulta de fecha";
            AddFullRow(layout, simpleDateHeading, false);

            fechaInput = AddLabeledText(layout, "Fecha, formato día/mes/año:", false);
            fechaInput.Text = DateTime.Today.ToString("dd/MM/yyyy");
            FlowLayoutPanel dateButtons = ButtonRow();
            dateButtons.Controls.Add(NewButton("Consultar día", delegate { CheckDate(); }));
            dateButtons.Controls.Add(NewButton("Copiar resultado", delegate { CopyTextResult2824(simpleDateResult2824); }));
            AddFullRow(layout, dateButtons, false);
            simpleDateResult2824 = AddLabeledText(layout, "Resultado de la fecha:", true);
            simpleDateResult2824.ReadOnly = true;
            simpleDateResult2824.Height = 72;

            Label advancedDateHeading = new Label();
            advancedDateHeading.AutoSize = true;
            advancedDateHeading.Text = "CÁLCULOS CON FECHAS";
            advancedDateHeading.AccessibleName = "Cálculos con fechas";
            AddFullRow(layout, advancedDateHeading, false);

            dateOperation2824 = AddLabeledCombo(layout, "Operación con fechas:", new string[]
            {
                "Edad exacta hasta hoy", "Diferencia entre dos fechas", "Sumar días a una fecha", "Restar días a una fecha", "Días laborables entre dos fechas, sin festivos"
            });
            dateOperation2824.SelectedIndexChanged += delegate { ApplyDateExample2824(); };
            dateAdvancedInput2824 = AddLabeledText(layout, "Fecha o valores:", false);
            AttachCalculatorTextBox1554(dateAdvancedInput2824, delegate { CalculateDate2824(); });
            ApplyDateExample2824();

            FlowLayoutPanel advancedDateButtons = ButtonRow();
            advancedDateButtons.Controls.Add(NewButton("Calcular fecha", delegate { CalculateDate2824(); }));
            advancedDateButtons.Controls.Add(NewButton("Copiar resultado", delegate { CopyTextResult2824(dateAdvancedResult2824); }));
            AddFullRow(layout, advancedDateButtons, false);
            dateAdvancedResult2824 = AddLabeledText(layout, "Resultado de fechas:", true);
            dateAdvancedResult2824.ReadOnly = true;
            dateAdvancedResult2824.Height = 90;

            page.Controls.Add(layout);
            return page;
        }

        private TabPage BuildCalculatorSellerPage2825()
        {
            TabPage page = new TabPage("Liquidación vendedor");
            TableLayoutPanel layout = BaseLayout();

            TextBox intro = ResultBox();
            intro.Height = 82;
            intro.Text = "Añade el efectivo por filas. Puedes usar blíster formato europeo o formato español/antiguo. Europeo: 10 c, 20 c y 50 c llevan 40 monedas. Español/antiguo: 10 c lleva 50; 20 c y 50 c llevan 25. En ambos, 1 € y 2 € llevan 25 monedas. El borrador se guarda automáticamente en datos\\calculadora_vendedor_borrador.json.";
            AddResultRow(layout, "Uso:", intro);

            sellerType2825 = AddLabeledCombo(layout, "Tipo de efectivo:", new string[] { "Blíster formato europeo", "Blíster formato español/antiguo", "Billete", "Moneda suelta" });
            sellerType2825.SelectedIndexChanged += delegate { if (!sellerDenominationUpdating2825) RefreshSellerDenominations2825(); };
            sellerDenomination2825 = AddLabeledCombo(layout, "Denominación:", new string[0]);
            RefreshSellerDenominations2825();

            Label quantityLabel = new Label();
            quantityLabel.AutoSize = true;
            quantityLabel.Text = "Cantidad:";
            sellerQuantity2825 = new TextBox();
            sellerQuantity2825.Text = "1";
            sellerQuantity2825.MaxLength = 6;
            sellerQuantity2825.AccessibleName = "Cantidad de blísteres, billetes o monedas";
            sellerQuantity2825.AccessibleDescription = "Escribe una cantidad entera entre 1 y 100000.";
            sellerQuantity2825.Dock = DockStyle.Top;
            int qr = layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.Controls.Add(quantityLabel, 0, qr);
            layout.Controls.Add(sellerQuantity2825, 1, qr);

            FlowLayoutPanel addButtons = ButtonRow();
            addButtons.Controls.Add(NewButton("Añadir fila", delegate { AddSellerLine2825(); }));
            addButtons.Controls.Add(NewButton("Cargar seleccionada para editar", delegate { LoadSelectedSellerLine2825(); }));
            addButtons.Controls.Add(NewButton("Actualizar seleccionada", delegate { UpdateSelectedSellerLine2825(); }));
            AddFullRow(layout, addButtons, false);

            sellerRows2825 = new ListBox();
            sellerRows2825.Dock = DockStyle.Fill;
            sellerRows2825.Height = 180;
            sellerRows2825.AccessibleName = "Filas de la liquidación de vendedor";
            sellerRows2825.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Delete)
                {
                    e.Handled = true;
                    DeleteSelectedSellerLine2825();
                }
                else if (e.KeyCode == Keys.Add || e.KeyCode == Keys.Oemplus)
                {
                    e.Handled = true;
                    ChangeSelectedSellerQuantity2825(1);
                }
                else if (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.OemMinus)
                {
                    e.Handled = true;
                    ChangeSelectedSellerQuantity2825(-1);
                }
            };
            AddFullRow(layout, sellerRows2825, true);

            FlowLayoutPanel rowButtons = ButtonRow();
            rowButtons.Controls.Add(NewButton("+1", delegate { ChangeSelectedSellerQuantity2825(1); }));
            rowButtons.Controls.Add(NewButton("-1", delegate { ChangeSelectedSellerQuantity2825(-1); }));
            rowButtons.Controls.Add(NewButton("Eliminar fila", delegate { DeleteSelectedSellerLine2825(); }));
            rowButtons.Controls.Add(NewButton("Nueva liquidación", delegate { ClearSellerLiquidation2825(); }));
            AddFullRow(layout, rowButtons, false);

            sellerTotals2825 = AddLabeledText(layout, "Totales:", true);
            sellerTotals2825.ReadOnly = true;
            sellerTotals2825.Height = 110;

            Label recordDateLabel = new Label();
            recordDateLabel.AutoSize = true;
            recordDateLabel.Text = "Fecha y hora de la liquidación:";
            sellerRecordDate2826 = new DateTimePicker();
            sellerRecordDate2826.Format = DateTimePickerFormat.Custom;
            sellerRecordDate2826.CustomFormat = "dd/MM/yyyy HH:mm";
            sellerRecordDate2826.Value = DateTime.Now;
            sellerRecordDate2826.AccessibleName = "Fecha y hora de la liquidación";
            sellerRecordDate2826.Dock = DockStyle.Top;
            int dr = layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.Controls.Add(recordDateLabel, 0, dr);
            layout.Controls.Add(sellerRecordDate2826, 1, dr);

            sellerRecordNote2826 = AddLabeledText(layout, "Nota o nombre opcional:", false);
            sellerRecordNote2826.AccessibleName = "Nota o nombre opcional de la liquidación";

            sellerCorrectionStatus2827 = AddLabeledText(layout, "Estado de corrección:", true);
            sellerCorrectionStatus2827.ReadOnly = true;
            sellerCorrectionStatus2827.Height = 55;
            UpdateSellerCorrectionStatus2827();

            FlowLayoutPanel totalButtons = ButtonRow();
            totalButtons.Controls.Add(NewButton("Copiar total", delegate { CopySellerTotal2825(); }));
            totalButtons.Controls.Add(NewButton("Copiar desglose", delegate { CopySellerBreakdown2825(); }));
            totalButtons.Controls.Add(NewButton("Guardar liquidación por fecha", delegate { SaveSellerLiquidation2826(); }));
            totalButtons.Controls.Add(NewButton("Guardar corrección", delegate { SaveSellerCorrection2827(); }));
            totalButtons.Controls.Add(NewButton("Cancelar corrección", delegate { CancelSellerCorrection2827(true); }));
            totalButtons.Controls.Add(NewButton("Ver liquidaciones guardadas", delegate { SelectSellerHistory2826(); }));
            totalButtons.Controls.Add(NewButton("Inventario, rascas y móvil", delegate { ShowLiquidacionMovil2940(); }));
            totalButtons.Controls.Add(NewButton("Exportar calculadora independiente (ZIP)", delegate { ExportSellerLiquidationStandalone2850(); }));
            AddFullRow(layout, totalButtons, false);

            TextBox help = ResultBox();
            help.Height = 90;
            help.Text = "Ejemplos: 2 blísteres de 1 € = 50 €. Un blíster español/antiguo de 50 c = 12,50 €. Un blíster europeo de 50 c = 20 €. En la lista, Suprimir elimina la fila; + y - aumentan o reducen una unidad. El borrador se conserva al cerrar Navaja. Puedes exportar esta calculadora con sus sonidos en un ZIP sin incluir tus datos personales.";
            AddResultRow(layout, "Ayuda:", help);

            RefreshSellerRows2825(-1);
            page.Controls.Add(layout);
            return page;
        }

        private string SellerDenominationCacheKey2825(string type)
        {
            if (type == "Blíster formato europeo" || type == "Blíster europeo") return "europeo";
            if (type == "Blíster formato español/antiguo") return "español";
            if (type == "Billete") return "billete";
            return "moneda";
        }

        private SellerDenominationChoice2825[] GetSellerDenominations2825(string type)
        {
            string key = SellerDenominationCacheKey2825(type);
            SellerDenominationChoice2825[] choices;
            if (sellerDenominationCatalog2825.TryGetValue(key, out choices)) return choices;

            if (key == "europeo")
            {
                choices = new SellerDenominationChoice2825[] {
                    new SellerDenominationChoice2825("1 céntimo", 0.50, 50),
                    new SellerDenominationChoice2825("2 céntimos", 1.00, 50),
                    new SellerDenominationChoice2825("5 céntimos", 2.50, 50),
                    new SellerDenominationChoice2825("10 céntimos", 4.00, 40),
                    new SellerDenominationChoice2825("20 céntimos", 8.00, 40),
                    new SellerDenominationChoice2825("50 céntimos", 20.00, 40),
                    new SellerDenominationChoice2825("1 €", 25.00, 25),
                    new SellerDenominationChoice2825("2 €", 50.00, 25)
                };
            }
            else if (key == "español")
            {
                choices = new SellerDenominationChoice2825[] {
                    new SellerDenominationChoice2825("1 céntimo", 0.50, 50),
                    new SellerDenominationChoice2825("2 céntimos", 1.00, 50),
                    new SellerDenominationChoice2825("5 céntimos", 2.50, 50),
                    new SellerDenominationChoice2825("10 céntimos", 5.00, 50),
                    new SellerDenominationChoice2825("20 céntimos", 5.00, 25),
                    new SellerDenominationChoice2825("50 céntimos", 12.50, 25),
                    new SellerDenominationChoice2825("1 €", 25.00, 25),
                    new SellerDenominationChoice2825("2 €", 50.00, 25)
                };
            }
            else if (key == "billete")
            {
                choices = new SellerDenominationChoice2825[] {
                    new SellerDenominationChoice2825("5 €", 5.00, 1),
                    new SellerDenominationChoice2825("10 €", 10.00, 1),
                    new SellerDenominationChoice2825("20 €", 20.00, 1),
                    new SellerDenominationChoice2825("50 €", 50.00, 1),
                    new SellerDenominationChoice2825("100 €", 100.00, 1),
                    new SellerDenominationChoice2825("200 €", 200.00, 1),
                    new SellerDenominationChoice2825("500 €", 500.00, 1)
                };
            }
            else
            {
                choices = new SellerDenominationChoice2825[] {
                    new SellerDenominationChoice2825("1 céntimo", 0.01, 1),
                    new SellerDenominationChoice2825("2 céntimos", 0.02, 1),
                    new SellerDenominationChoice2825("5 céntimos", 0.05, 1),
                    new SellerDenominationChoice2825("10 céntimos", 0.10, 1),
                    new SellerDenominationChoice2825("20 céntimos", 0.20, 1),
                    new SellerDenominationChoice2825("50 céntimos", 0.50, 1),
                    new SellerDenominationChoice2825("1 €", 1.00, 1),
                    new SellerDenominationChoice2825("2 €", 2.00, 1)
                };
            }

            sellerDenominationCatalog2825[key] = choices;
            return choices;
        }

        private void RefreshSellerDenominations2825()
        {
            if (sellerType2825 == null || sellerDenomination2825 == null) return;
            string type = sellerType2825.Text;
            string key = SellerDenominationCacheKey2825(type);
            SellerDenominationChoice2825[] choices = GetSellerDenominations2825(type);
            if (key == sellerDenominationCacheKey2825 && sellerDenomination2825.Items.Count == choices.Length) return;

            sellerDenominationUpdating2825 = true;
            sellerDenomination2825.BeginUpdate();
            try
            {
                sellerDenomination2825.Items.Clear();
                sellerDenomination2825.Items.AddRange(choices);
                if (sellerDenomination2825.Items.Count > 0) sellerDenomination2825.SelectedIndex = 0;
                sellerDenominationCacheKey2825 = key;
            }
            finally
            {
                sellerDenomination2825.EndUpdate();
                sellerDenominationUpdating2825 = false;
            }
        }

        private SellerDenominationChoice2825 SelectedSellerDenomination2825()
        {
            return sellerDenomination2825 == null ? null : sellerDenomination2825.SelectedItem as SellerDenominationChoice2825;
        }

        private bool TryGetSellerQuantity2825(out int quantity)
        {
            quantity = 0;
            if (sellerQuantity2825 != null && int.TryParse(sellerQuantity2825.Text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out quantity) && quantity >= 1 && quantity <= 100000)
                return true;
            AnnounceToScreenReader("Escribe una cantidad entera entre 1 y 100000.");
            if (sellerQuantity2825 != null) sellerQuantity2825.Focus();
            return false;
        }

        private void AddSellerLine2825()
        {
            SellerDenominationChoice2825 choice = SelectedSellerDenomination2825();
            if (choice == null || sellerQuantity2825 == null || sellerType2825 == null) { PlayNamedSound("aviso"); return; }
            int quantity;
            if (!TryGetSellerQuantity2825(out quantity)) return;
            SellerLine2825 line = new SellerLine2825();
            line.Type = sellerType2825.Text;
            line.Denomination = choice.Label;
            line.Quantity = quantity;
            line.UnitValue = choice.Value;
            line.UnitsPerContainer = choice.UnitsPerContainer;
            sellerLines2825.Add(line);
            ScheduleSellerDraftSave2930();
            RefreshSellerRowAfterMutation2930(sellerLines2825.Count - 1, true);
            PlayCalculatorSound1554("calc_liquidacion_anadir");
            AnnounceToScreenReader("Añadido. " + line.ToString() + ". Total " + SellerTotal2825().ToString("N2", SellerCulture2825) + " euros.");
        }

        private void LoadSelectedSellerLine2825()
        {
            if (sellerRows2825 == null || sellerRows2825.SelectedIndex < 0) { PlayNamedSound("aviso"); return; }
            SellerLine2825 line = sellerLines2825[sellerRows2825.SelectedIndex];
            string typeToSelect2825 = string.Equals(line.Type, "Blíster europeo", StringComparison.OrdinalIgnoreCase) ? "Blíster formato europeo" : line.Type;
            SelectComboText2825(sellerType2825, typeToSelect2825);
            RefreshSellerDenominations2825();
            SelectComboText2825(sellerDenomination2825, line.Denomination);
            int quantity = line.Quantity;
            if (quantity < 1) quantity = 1;
            if (quantity > 100000) quantity = 100000;
            sellerQuantity2825.Text = quantity.ToString(CultureInfo.InvariantCulture);
            PlayCalculatorSound1554("calc_memoria");
            AnnounceToScreenReader("Fila cargada para editar.");
        }

        private void UpdateSelectedSellerLine2825()
        {
            if (sellerRows2825 == null || sellerRows2825.SelectedIndex < 0) { PlayNamedSound("aviso"); return; }
            SellerDenominationChoice2825 choice = SelectedSellerDenomination2825();
            if (choice == null) return;
            int quantity;
            if (!TryGetSellerQuantity2825(out quantity)) return;
            int index = sellerRows2825.SelectedIndex;
            SellerLine2825 line = sellerLines2825[index];
            line.Type = sellerType2825.Text;
            line.Denomination = choice.Label;
            line.Quantity = quantity;
            line.UnitValue = choice.Value;
            line.UnitsPerContainer = choice.UnitsPerContainer;
            ScheduleSellerDraftSave2930();
            RefreshSellerRowAfterMutation2930(index, false);
            PlayCalculatorSound1554("calc_liquidacion_anadir");
            AnnounceToScreenReader("Fila actualizada. Total " + SellerTotal2825().ToString("N2", SellerCulture2825) + " euros.");
        }

        private void ChangeSelectedSellerQuantity2825(int delta)
        {
            if (sellerRows2825 == null || sellerRows2825.SelectedIndex < 0) { PlayNamedSound("aviso"); return; }
            int index = sellerRows2825.SelectedIndex;
            SellerLine2825 line = sellerLines2825[index];
            int next = line.Quantity + delta;
            if (next <= 0)
            {
                DeleteSelectedSellerLine2825();
                return;
            }
            line.Quantity = next;
            ScheduleSellerDraftSave2930();
            RefreshSellerRowAfterMutation2930(index, false);
            PlayCalculatorSound1554(delta > 0 ? "calc_liquidacion_anadir" : "calc_liquidacion_restar");
            AnnounceToScreenReader((delta > 0 ? "Añadida una unidad. " : "Quitada una unidad. ") + line.ToString() + ". Total " + SellerTotal2825().ToString("N2", SellerCulture2825) + " euros.");
        }

        private void DeleteSelectedSellerLine2825()
        {
            if (sellerRows2825 == null || sellerRows2825.SelectedIndex < 0) { PlayNamedSound("aviso"); return; }
            int index = sellerRows2825.SelectedIndex;
            string deleted = sellerLines2825[index].ToString();
            sellerLines2825.RemoveAt(index);
            ScheduleSellerDraftSave2930();
            if (index >= sellerLines2825.Count) index = sellerLines2825.Count - 1;
            RefreshSellerAfterDelete2930(index);
            PlayCalculatorSound1554("calc_liquidacion_borrar");
            AnnounceToScreenReader("Fila eliminada: " + deleted + ". Total " + SellerTotal2825().ToString("N2", SellerCulture2825) + " euros.");
        }

        private void ClearSellerLiquidation2825()
        {
            if (sellerLines2825.Count == 0) { PlayNamedSound("aviso"); AnnounceToScreenReader("La liquidación ya está vacía."); return; }
            DialogResult answer = MessageBox.Show("¿Crear una liquidación nueva y borrar todas las filas actuales? Puedes guardarla antes como liquidación por fecha.", "Nueva liquidación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (answer != DialogResult.Yes) return;
            sellerLines2825.Clear();
            CancelSellerCorrection2827(false);
            ScheduleSellerDraftSave2930();
            if (sellerRecordDate2826 != null) sellerRecordDate2826.Value = DateTime.Now;
            if (sellerRecordNote2826 != null) sellerRecordNote2826.Clear();
            RefreshSellerRows2825(-1);
            PlayCalculatorSound1554("calc_liquidacion_borrar");
            AnnounceToScreenReader("Nueva liquidación preparada. Total cero euros.");
        }

        private double SellerTotal2825()
        {
            double total = 0;
            foreach (SellerLine2825 line in sellerLines2825) total += line.Subtotal;
            return total;
        }

        private string SellerTotalsText2825()
        {
            return SellerTotalsText2826(sellerLines2825);
        }

        private void RefreshSellerRows2825(int selected)
        {
            if (sellerRows2825 != null)
            {
                sellerLoading2825 = true;
                sellerRows2825.BeginUpdate();
                try
                {
                    sellerRows2825.Items.Clear();
                    sellerRows2825.Items.AddRange(sellerLines2825.ToArray());
                    if (sellerRows2825.Items.Count > 0)
                    {
                        if (selected < 0 || selected >= sellerRows2825.Items.Count) selected = sellerRows2825.Items.Count - 1;
                        sellerRows2825.SelectedIndex = selected;
                    }
                }
                finally
                {
                    sellerRows2825.EndUpdate();
                    sellerLoading2825 = false;
                }
            }
            if (sellerTotals2825 != null) sellerTotals2825.Text = SellerTotalsText2825();
        }

        private string SellerBreakdown2825()
        {
            DateTime when = sellerRecordDate2826 == null ? DateTime.Now : sellerRecordDate2826.Value;
            string note = sellerRecordNote2826 == null ? "" : sellerRecordNote2826.Text;
            return SellerBreakdown2826(when, note, sellerLines2825);
        }

        private string SellerBreakdown2826(DateTime when, string note, IList<SellerLine2825> lines)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Liquidación de vendedor - " + when.ToString("dd/MM/yyyy HH:mm"));
            if (!string.IsNullOrWhiteSpace(note)) sb.AppendLine("Nota: " + note.Trim());
            if (lines == null || lines.Count == 0) sb.AppendLine("Sin filas.");
            else
            {
                for (int i = 0; i < lines.Count; i++) sb.AppendLine((i + 1) + ". " + lines[i].ToString());
            }
            sb.AppendLine();
            sb.Append(SellerTotalsText2826(lines));
            return sb.ToString();
        }

        private string SellerTotalsText2826(IList<SellerLine2825> lines)
        {
            double blisters = 0, bills = 0, loose = 0;
            int blisterCount = 0, coinCount = 0, billCount = 0;
            if (lines != null)
            {
                foreach (SellerLine2825 line in lines)
                {
                    if (line.Type.StartsWith("Blíster", StringComparison.OrdinalIgnoreCase)) { blisters += line.Subtotal; blisterCount += line.Quantity; coinCount += line.Quantity * line.UnitsPerContainer; }
                    else if (line.Type == "Billete") { bills += line.Subtotal; billCount += line.Quantity; }
                    else { loose += line.Subtotal; coinCount += line.Quantity; }
                }
            }
            CultureInfo es = SellerCulture2825;
            return "Blísteres: " + blisterCount + ", " + blisters.ToString("N2", es) + " €\r\n" +
                   "Monedas contabilizadas: " + coinCount + ", incluidas las de los blísteres. Monedas sueltas: " + loose.ToString("N2", es) + " €\r\n" +
                   "Billetes: " + billCount + ", " + bills.ToString("N2", es) + " €\r\n" +
                   "TOTAL: " + (blisters + bills + loose).ToString("N2", es) + " €";
        }

        private void CopySellerTotal2825()
        {
            try
            {
                string text = SellerTotal2825().ToString("N2", SellerCulture2825) + " €";
                Clipboard.SetText(text);
                PlayCalculatorSound1554("calc_liquidacion_total");
                AnnounceToScreenReader("Total copiado: " + text + ".");
            }
            catch { PlayNamedSound("error"); }
        }

        private void CopySellerBreakdown2825()
        {
            try
            {
                Clipboard.SetText(SellerBreakdown2825());
                PlayCalculatorSound1554("calc_copiar");
                AnnounceToScreenReader("Desglose completo de la liquidación copiado.");
            }
            catch { PlayNamedSound("error"); }
        }

        private void SaveSellerToHistory2825()
        {
            if (sellerLines2825.Count == 0) { PlayNamedSound("aviso"); return; }
            string total = "Total: " + SellerTotal2825().ToString("N2", SellerCulture2825) + " €";
            AddCalculatorHistory2824("Liquidación de vendedor", SellerBreakdown2825(), total, "");
            PlayCalculatorSound1554("calc_liquidacion_total");
            AnnounceToScreenReader("Liquidación guardada en el historial. " + total + ".");
        }

        private SellerLine2825 CloneSellerLine2826(SellerLine2825 source)
        {
            SellerLine2825 copy = new SellerLine2825();
            if (source == null) return copy;
            copy.Type = source.Type;
            copy.Denomination = source.Denomination;
            copy.Quantity = source.Quantity;
            copy.UnitValue = source.UnitValue;
            copy.UnitsPerContainer = source.UnitsPerContainer;
            return copy;
        }

        private double SellerTotal2826(IList<SellerLine2825> lines)
        {
            double total = 0;
            if (lines != null) foreach (SellerLine2825 line in lines) total += line.Subtotal;
            return total;
        }

        private bool SellerDraftMatchesSavedLiquidation2930(DateTime when, string note, IList<SellerLine2825> lines)
        {
            string targetWhen = when.ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture);
            string targetNote = (note ?? "").Trim();
            foreach (SellerLiquidation2826 saved in sellerSaved2826)
            {
                if (saved.DateValue == DateTime.MinValue || saved.DateValue.ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture) != targetWhen) continue;
                if (!string.Equals((saved.Note ?? "").Trim(), targetNote, StringComparison.Ordinal)) continue;
                if (saved.Lines.Count != lines.Count) continue;
                bool same = true;
                for (int i = 0; i < lines.Count; i++)
                {
                    SellerLine2825 a = saved.Lines[i];
                    SellerLine2825 b = lines[i];
                    if (!string.Equals(a.Type, b.Type, StringComparison.Ordinal) ||
                        !string.Equals(a.Denomination, b.Denomination, StringComparison.Ordinal) ||
                        a.Quantity != b.Quantity ||
                        a.UnitValue != b.UnitValue ||
                        a.UnitsPerContainer != b.UnitsPerContainer)
                    {
                        same = false;
                        break;
                    }
                }
                if (same) return true;
            }
            return false;
        }

        private void PrepareNewSellerLiquidationAfterSave2826()
        {
            sellerLines2825.Clear();
            sellerCorrectionId2827 = "";
            UpdateSellerCorrectionStatus2827();
            if (sellerRecordDate2826 != null) sellerRecordDate2826.Value = DateTime.Now;
            if (sellerRecordNote2826 != null) sellerRecordNote2826.Clear();
            ScheduleSellerDraftSave2930();
            RefreshSellerRows2825(-1);
            if (sellerRows2825 != null) sellerRows2825.Focus();
        }

        private void SaveSellerLiquidation2826()
        {
            if (!string.IsNullOrWhiteSpace(sellerCorrectionId2827))
            {
                PlayNamedSound("aviso");
                AnnounceToScreenReader("Estás corrigiendo una liquidación guardada. Usa Guardar corrección o Cancelar corrección.");
                return;
            }
            if (sellerLines2825.Count == 0)
            {
                PlayNamedSound("aviso");
                AnnounceToScreenReader("No hay filas para guardar.");
                return;
            }

            DateTime when = sellerRecordDate2826 == null ? DateTime.Now : sellerRecordDate2826.Value;
            string note = sellerRecordNote2826 == null ? "" : sellerRecordNote2826.Text.Trim();
            if (SellerDraftMatchesSavedLiquidation2930(when, note, sellerLines2825))
            {
                string duplicateMessage = "Esta liquidación ya está guardada. Crea una nueva o modifica sus filas antes de volver a guardar.";
                PlayNamedSound("aviso");
                MessageBox.Show(duplicateMessage, "Liquidación ya guardada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                AnnounceToScreenReader(duplicateMessage);
                return;
            }
            if (!CanStartSellerLiquidationSave285())
            {
                PlayNamedSound("aviso");
                AnnounceToScreenReader("La liquidación anterior todavía se está guardando. Espera un instante y vuelve a intentarlo.");
                return;
            }

            SellerLiquidation2826 item = new SellerLiquidation2826();
            item.Id = Guid.NewGuid().ToString("N");
            item.When = when.ToString("o", CultureInfo.InvariantCulture);
            item.Note = note;
            item.UpdatedAt = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
            item.DeviceId = LiquidacionMovilDeviceId2940();
            foreach (SellerLine2825 line in sellerLines2825) item.Lines.Add(CloneSellerLine2826(line));
            sellerSaved2826.Add(item);
            QueueSellerLiquidationsSave285();
            RefreshSellerSavedList2826();
            PlayCalculatorSound1554("calc_historial");
            AnnounceToScreenReader("Liquidación guardada para " + when.ToString("dd/MM/yyyy") + ". Total " + item.Total.ToString("N2", SellerCulture2825) + " euros.");
            PrepareNewSellerLiquidationAfterSave2826();
            AnnounceToScreenReader("Nueva liquidación preparada para introducir la siguiente.");
        }

        private TabPage BuildCalculatorSellerHistoryPage2826()
        {
            TabPage page = new TabPage("Liquidaciones guardadas");
            TableLayoutPanel layout = BaseLayout();

            TextBox intro = ResultBox();
            intro.Height = 70;
            intro.Text = "Historial independiente de liquidaciones. Filtra por fechas, consulta cada desglose, copia totales o vuelve a cargar una liquidación como borrador. Los registros se guardan localmente en datos\\calculadora_vendedor_liquidaciones.json.";
            AddResultRow(layout, "Uso:", intro);
            sellerCloudStatus2962 = AddLabeledText(layout, "Sincronización de liquidaciones:", true);
            sellerCloudStatus2962.ReadOnly = true;
            sellerCloudStatus2962.AccessibleName = "Estado de sincronización de las liquidaciones guardadas";
            FlowLayoutPanel cloudActions = ButtonRow();
            cloudActions.Controls.Add(NewButton("Sincronizar ahora", delegate { QueueLiquidacionMovilSync2940(true); }));
            cloudActions.Controls.Add(NewButton("Configurar Google Drive o Dropbox", delegate { ShowLiquidacionMovil2940(); }));
            AddFullRow(layout, cloudActions, false);
            RefreshSellerCloudStatus2962();

            sellerFilterFrom2826 = NewSellerDatePicker2826("Fecha inicial del filtro");
            sellerFilterTo2826 = NewSellerDatePicker2826("Fecha final del filtro");
            DateTime today = DateTime.Today;
            sellerFilterFrom2826.Value = new DateTime(today.Year, today.Month, 1);
            sellerFilterTo2826.Value = today;
            AddSellerDateRow2826(layout, "Desde:", sellerFilterFrom2826);
            AddSellerDateRow2826(layout, "Hasta:", sellerFilterTo2826);

            FlowLayoutPanel filterButtons = ButtonRow();
            filterButtons.Controls.Add(NewButton("Aplicar filtro", delegate { RefreshSellerSavedList2826(); }));
            filterButtons.Controls.Add(NewButton("Hoy", delegate { SetSellerSavedFilterToday2826(); }));
            filterButtons.Controls.Add(NewButton("Este mes", delegate { SetSellerSavedFilterMonth2826(); }));
            filterButtons.Controls.Add(NewButton("Todo", delegate { SetSellerSavedFilterAll2826(); }));
            AddFullRow(layout, filterButtons, false);

            sellerSavedSummary2826 = AddLabeledText(layout, "Resumen del período:", true);
            sellerSavedSummary2826.ReadOnly = true;
            sellerSavedSummary2826.Height = 75;

            sellerSavedList2826 = new ListBox();
            sellerSavedList2826.Dock = DockStyle.Fill;
            sellerSavedList2826.Height = 190;
            sellerSavedList2826.AccessibleName = "Liquidaciones guardadas por fecha";
            sellerSavedList2826.SelectedIndexChanged += delegate { RefreshSellerSavedDetail2826(); };
            sellerSavedList2826.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Delete)
                {
                    e.Handled = true;
                    DeleteSellerSaved2826();
                }
                else if (e.Control && e.KeyCode == Keys.C)
                {
                    e.Handled = true;
                    CopySellerSavedBreakdown2826();
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    e.Handled = true;
                    LoadSellerSavedAsDraft2826();
                }
            };
            AddFullRow(layout, sellerSavedList2826, true);

            sellerSavedDetail2826 = AddLabeledText(layout, "Detalle:", true);
            sellerSavedDetail2826.ReadOnly = true;
            sellerSavedDetail2826.Height = 145;

            FlowLayoutPanel actions = ButtonRow();
            actions.Controls.Add(NewButton("Copiar total", delegate { CopySellerSavedTotal2826(); }));
            actions.Controls.Add(NewButton("Copiar desglose", delegate { CopySellerSavedBreakdown2826(); }));
            actions.Controls.Add(NewButton("Copiar resumen del período", delegate { CopySellerSavedPeriodSummary2826(); }));
            actions.Controls.Add(NewButton("Cargar como borrador", delegate { LoadSellerSavedAsDraft2826(); }));
            actions.Controls.Add(NewButton("Corregir seleccionada", delegate { StartSellerCorrection2827(); }));
            actions.Controls.Add(NewButton("Historial de correcciones", delegate { ShowSellerCorrections2827(); }));
            actions.Controls.Add(NewButton("Eliminar", delegate { DeleteSellerSaved2826(); }));
            AddFullRow(layout, actions, false);

            page.Controls.Add(layout);
            RefreshSellerSavedList2826();
            return page;
        }

        private DateTimePicker NewSellerDatePicker2826(string accessibleName)
        {
            DateTimePicker picker = new DateTimePicker();
            picker.Format = DateTimePickerFormat.Custom;
            picker.CustomFormat = "dd/MM/yyyy";
            picker.AccessibleName = accessibleName;
            picker.Dock = DockStyle.Top;
            return picker;
        }

        private void AddSellerDateRow2826(TableLayoutPanel layout, string labelText, DateTimePicker picker)
        {
            Label label = new Label();
            label.AutoSize = true;
            label.Text = labelText;
            int row = layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.Controls.Add(label, 0, row);
            layout.Controls.Add(picker, 1, row);
        }

        private void SelectSellerHistory2826()
        {
            if (calculatorTabs2824 == null || calculatorSellerHistoryPage2826 == null) return;
            calculatorTabs2824.SelectedTab = calculatorSellerHistoryPage2826;
            RefreshSellerSavedList2826();
            if (sellerSavedList2826 != null && sellerSavedList2826.Items.Count > 0) sellerSavedList2826.Focus();
        }

        private void SetSellerSavedFilterToday2826()
        {
            if (sellerFilterFrom2826 == null || sellerFilterTo2826 == null) return;
            sellerFilterFrom2826.Value = DateTime.Today;
            sellerFilterTo2826.Value = DateTime.Today;
            RefreshSellerSavedList2826();
        }

        private void SetSellerSavedFilterMonth2826()
        {
            if (sellerFilterFrom2826 == null || sellerFilterTo2826 == null) return;
            DateTime today = DateTime.Today;
            sellerFilterFrom2826.Value = new DateTime(today.Year, today.Month, 1);
            sellerFilterTo2826.Value = today;
            RefreshSellerSavedList2826();
        }

        private void SetSellerSavedFilterAll2826()
        {
            if (sellerFilterFrom2826 == null || sellerFilterTo2826 == null) return;
            DateTime min = DateTime.Today;
            DateTime max = DateTime.Today;
            bool found = false;
            foreach (SellerLiquidation2826 item in sellerSaved2826)
            {
                DateTime d = item.DateValue;
                if (d == DateTime.MinValue) continue;
                if (!found) { min = max = d.Date; found = true; }
                else { if (d.Date < min) min = d.Date; if (d.Date > max) max = d.Date; }
            }
            sellerFilterFrom2826.Value = min;
            sellerFilterTo2826.Value = max;
            RefreshSellerSavedList2826();
        }

        private void RefreshSellerSavedList2826()
        {
            if (sellerSavedList2826 == null) return;
            string selectedId2962 = SelectedSellerSaved2826() == null ? "" : SelectedSellerSaved2826().Id;
            sellerSavedLoading2826 = true;
            try
            {
                DateTime from = sellerFilterFrom2826 == null ? DateTime.MinValue : sellerFilterFrom2826.Value.Date;
                DateTime to = sellerFilterTo2826 == null ? DateTime.MaxValue : sellerFilterTo2826.Value.Date.AddDays(1).AddTicks(-1);
                if (from > to)
                {
                    DateTime swap = from; from = to.Date; to = swap.Date.AddDays(1).AddTicks(-1);
                }

                sellerVisibleSaved2826.Clear();
                foreach (SellerLiquidation2826 item in sellerSaved2826)
                {
                    item.CloudState2962 = SellerRecordCloudStatus2962(item);
                    DateTime d = item.DateValue;
                    if (string.IsNullOrWhiteSpace(item.DeletedAt) && (d == DateTime.MinValue || (d >= from && d <= to))) sellerVisibleSaved2826.Add(item);
                }
                sellerVisibleSaved2826.Sort(delegate(SellerLiquidation2826 a, SellerLiquidation2826 b) { return b.DateValue.CompareTo(a.DateValue); });

                sellerSavedList2826.BeginUpdate();
                try
                {
                    sellerSavedList2826.Items.Clear();
                    sellerSavedList2826.Items.AddRange(sellerVisibleSaved2826.ToArray());
                    if (sellerSavedList2826.Items.Count > 0) { sellerSavedList2826.SelectedIndex = 0; for (int i2962 = 0; i2962 < sellerVisibleSaved2826.Count; i2962++) if (sellerVisibleSaved2826[i2962].Id == selectedId2962) { sellerSavedList2826.SelectedIndex = i2962; break; } }
                    else if (sellerSavedDetail2826 != null) sellerSavedDetail2826.Text = "No hay liquidaciones en este período.";
                }
                finally { sellerSavedList2826.EndUpdate(); }
                RefreshSellerSavedSummary2826();
            }
            finally { sellerSavedLoading2826 = false; }
            RefreshSellerSavedDetail2826();
            RefreshSellerCloudStatus2962();
        }

        private SellerLiquidation2826 SelectedSellerSaved2826()
        {
            return sellerSavedList2826 == null ? null : sellerSavedList2826.SelectedItem as SellerLiquidation2826;
        }

        private void RefreshSellerSavedDetail2826()
        {
            if (sellerSavedLoading2826 || sellerSavedDetail2826 == null) return;
            SellerLiquidation2826 item = SelectedSellerSaved2826();
            if (item == null) { sellerSavedDetail2826.Text = ""; return; }
            StringBuilder detail = new StringBuilder(SellerBreakdown2826(item.DateValue, item.Note, item.Lines));
            detail.AppendLine(); detail.AppendLine("Nube: " + SellerRecordCloudStatus2962(item));
            if (item.Revisions.Count > 0)
            {
                SellerRevision2827 last = item.Revisions[item.Revisions.Count - 1];
                string when = last.CorrectedDateValue == DateTime.MinValue ? last.CorrectedAt : last.CorrectedDateValue.ToString("dd/MM/yyyy HH:mm");
                detail.AppendLine();
                detail.Append("Corregida ").Append(item.Revisions.Count).Append(item.Revisions.Count == 1 ? " vez" : " veces").Append(". Última corrección: ").Append(when).Append(".");
            }
            sellerSavedDetail2826.Text = detail.ToString();
        }

        private void RefreshSellerSavedSummary2826()
        {
            if (sellerSavedSummary2826 == null) return;
            CultureInfo es = SellerCulture2825;
            if (sellerVisibleSaved2826.Count == 0)
            {
                sellerSavedSummary2826.Text = "0 liquidaciones. Total: 0,00 €.";
                return;
            }
            double total = 0, max = double.MinValue;
            SellerLiquidation2826 maxItem = null;
            foreach (SellerLiquidation2826 item in sellerVisibleSaved2826)
            {
                double value = item.Total;
                total += value;
                if (value > max) { max = value; maxItem = item; }
            }
            double average = total / sellerVisibleSaved2826.Count;
            string maxDate = maxItem == null || maxItem.DateValue == DateTime.MinValue ? "" : maxItem.DateValue.ToString("dd/MM/yyyy");
            sellerSavedSummary2826.Text = sellerVisibleSaved2826.Count + (sellerVisibleSaved2826.Count == 1 ? " liquidación" : " liquidaciones") +
                ". Total: " + total.ToString("N2", es) + " €. Media: " + average.ToString("N2", es) + " €. Mayor: " + max.ToString("N2", es) + " €" +
                (string.IsNullOrEmpty(maxDate) ? "." : " el " + maxDate + ".");
        }

        private void CopySellerSavedTotal2826()
        {
            SellerLiquidation2826 item = SelectedSellerSaved2826();
            if (item == null) { PlayNamedSound("aviso"); return; }
            try
            {
                string text = item.Total.ToString("N2", SellerCulture2825) + " €";
                Clipboard.SetText(text);
                PlayCalculatorSound1554("calc_liquidacion_total");
                AnnounceToScreenReader("Total copiado: " + text + ".");
            }
            catch { PlayNamedSound("error"); }
        }

        private void CopySellerSavedBreakdown2826()
        {
            SellerLiquidation2826 item = SelectedSellerSaved2826();
            if (item == null) { PlayNamedSound("aviso"); return; }
            try
            {
                Clipboard.SetText(SellerBreakdown2826(item.DateValue, item.Note, item.Lines));
                PlayCalculatorSound1554("calc_copiar");
                AnnounceToScreenReader("Desglose de la liquidación copiado.");
            }
            catch { PlayNamedSound("error"); }
        }

        private void CopySellerSavedPeriodSummary2826()
        {
            if (sellerSavedSummary2826 == null) return;
            try
            {
                StringBuilder sb = new StringBuilder();
                DateTime from = sellerFilterFrom2826 == null ? DateTime.Today : sellerFilterFrom2826.Value.Date;
                DateTime to = sellerFilterTo2826 == null ? DateTime.Today : sellerFilterTo2826.Value.Date;
                sb.AppendLine("Resumen de liquidaciones del " + from.ToString("dd/MM/yyyy") + " al " + to.ToString("dd/MM/yyyy"));
                sb.AppendLine(sellerSavedSummary2826.Text);
                foreach (SellerLiquidation2826 item in sellerVisibleSaved2826) sb.AppendLine(item.ToString());
                Clipboard.SetText(sb.ToString().TrimEnd());
                PlayCalculatorSound1554("calc_copiar");
                AnnounceToScreenReader("Resumen del período copiado.");
            }
            catch { PlayNamedSound("error"); }
        }

        private void LoadSellerSavedAsDraft2826()
        {
            SellerLiquidation2826 item = SelectedSellerSaved2826();
            if (item == null) { PlayNamedSound("aviso"); return; }
            if (sellerLines2825.Count > 0)
            {
                DialogResult answer = MessageBox.Show("Esto sustituirá el borrador actual por la liquidación guardada seleccionada. La liquidación histórica no se borrará. ¿Continuar?", "Cargar liquidación", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (answer != DialogResult.Yes) return;
            }
            CancelSellerCorrection2827(false);
            sellerLines2825.Clear();
            foreach (SellerLine2825 line in item.Lines) sellerLines2825.Add(CloneSellerLine2826(line));
            ScheduleSellerDraftSave2930();
            if (sellerRecordDate2826 != null && item.DateValue != DateTime.MinValue) sellerRecordDate2826.Value = item.DateValue;
            if (sellerRecordNote2826 != null) sellerRecordNote2826.Text = item.Note;
            RefreshSellerRows2825(-1);
            if (calculatorTabs2824 != null && calculatorSellerPage2825 != null) calculatorTabs2824.SelectedTab = calculatorSellerPage2825;
            PlayCalculatorSound1554("calc_memoria");
            AnnounceToScreenReader("Liquidación cargada como borrador. Total " + item.Total.ToString("N2", SellerCulture2825) + " euros.");
        }

        private void UpdateSellerCorrectionStatus2827()
        {
            if (sellerCorrectionStatus2827 == null) return;
            if (string.IsNullOrWhiteSpace(sellerCorrectionId2827))
            {
                sellerCorrectionStatus2827.Text = "No estás corrigiendo una liquidación guardada.";
                return;
            }
            SellerLiquidation2826 item = FindSellerLiquidation2827(sellerCorrectionId2827);
            if (item == null)
            {
                sellerCorrectionId2827 = "";
                sellerCorrectionStatus2827.Text = "No estás corrigiendo una liquidación guardada.";
                return;
            }
            string date = item.DateValue == DateTime.MinValue ? item.When : item.DateValue.ToString("dd/MM/yyyy HH:mm");
            sellerCorrectionStatus2827.Text = "Corrigiendo la liquidación de " + date + ". Total guardado antes de esta corrección: " + item.Total.ToString("N2", SellerCulture2825) + " €. Al guardar se conservará una revisión de los datos anteriores.";
        }

        private SellerLiquidation2826 FindSellerLiquidation2827(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            foreach (SellerLiquidation2826 item in sellerSaved2826)
                if (string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase)) return item;
            return null;
        }

        private void StartSellerCorrection2827()
        {
            SellerLiquidation2826 item = SelectedSellerSaved2826();
            if (item == null) { PlayNamedSound("aviso"); return; }
            if (sellerLines2825.Count > 0)
            {
                DialogResult answer = MessageBox.Show("Esto sustituirá el borrador actual por una copia de la liquidación seleccionada para corregirla. El registro histórico seguirá intacto hasta que pulses Guardar corrección. ¿Continuar?", "Corregir liquidación", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (answer != DialogResult.Yes) return;
            }
            sellerLines2825.Clear();
            foreach (SellerLine2825 line in item.Lines) sellerLines2825.Add(CloneSellerLine2826(line));
            ScheduleSellerDraftSave2930();
            if (sellerRecordDate2826 != null && item.DateValue != DateTime.MinValue) sellerRecordDate2826.Value = item.DateValue;
            if (sellerRecordNote2826 != null) sellerRecordNote2826.Text = item.Note;
            sellerCorrectionId2827 = item.Id;
            UpdateSellerCorrectionStatus2827();
            RefreshSellerRows2825(-1);
            if (calculatorTabs2824 != null && calculatorSellerPage2825 != null) calculatorTabs2824.SelectedTab = calculatorSellerPage2825;
            PlayCalculatorSound1554("calc_memoria");
            AnnounceToScreenReader("Modo corrección. Modifica las filas necesarias y pulsa Guardar corrección. El total anterior es " + item.Total.ToString("N2", SellerCulture2825) + " euros.");
        }

        private void CancelSellerCorrection2827(bool announce)
        {
            if (string.IsNullOrWhiteSpace(sellerCorrectionId2827))
            {
                if (announce) { PlayNamedSound("aviso"); AnnounceToScreenReader("No hay ninguna corrección activa."); }
                return;
            }
            sellerCorrectionId2827 = "";
            UpdateSellerCorrectionStatus2827();
            if (announce)
            {
                PlayCalculatorSound1554("calc_memoria");
                AnnounceToScreenReader("Corrección cancelada. Las filas actuales permanecen como borrador y no se ha modificado el historial.");
            }
        }

        private bool SellerLinesEqual2827(IList<SellerLine2825> a, IList<SellerLine2825> b)
        {
            if (a == null || b == null || a.Count != b.Count) return false;
            Dictionary<string, int> da = SellerQuantityMap2827(a);
            Dictionary<string, int> db = SellerQuantityMap2827(b);
            if (da.Count != db.Count) return false;
            foreach (KeyValuePair<string, int> pair in da)
            {
                int value;
                if (!db.TryGetValue(pair.Key, out value) || value != pair.Value) return false;
            }
            return true;
        }

        private string SellerLineKey2827(SellerLine2825 line)
        {
            return (line.Type ?? "") + "\u001f" + (line.Denomination ?? "") + "\u001f" + line.UnitValue.ToString("R", CultureInfo.InvariantCulture) + "\u001f" + line.UnitsPerContainer.ToString(CultureInfo.InvariantCulture);
        }

        private Dictionary<string, int> SellerQuantityMap2827(IList<SellerLine2825> lines)
        {
            Dictionary<string, int> map = new Dictionary<string, int>(StringComparer.Ordinal);
            if (lines == null) return map;
            foreach (SellerLine2825 line in lines)
            {
                string key = SellerLineKey2827(line);
                int current;
                map.TryGetValue(key, out current);
                map[key] = current + line.Quantity;
            }
            return map;
        }

        private SellerLine2825 SellerLineFromKey2827(string key, int quantity)
        {
            string[] parts = (key ?? "").Split(new char[] { '\u001f' });
            SellerLine2825 line = new SellerLine2825();
            if (parts.Length > 0) line.Type = parts[0];
            if (parts.Length > 1) line.Denomination = parts[1];
            double value;
            int units;
            if (parts.Length > 2 && double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out value)) line.UnitValue = value;
            if (parts.Length > 3 && int.TryParse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out units)) line.UnitsPerContainer = units;
            line.Quantity = quantity;
            return line;
        }

        private double SellerTotalFromLines2827(IList<SellerLine2825> lines)
        {
            double total = 0;
            if (lines != null) foreach (SellerLine2825 line in lines) total += line.Subtotal;
            return total;
        }

        private string BuildSellerCorrectionSummary2827(SellerLiquidation2826 oldItem, DateTime newWhen, string newNote, IList<SellerLine2825> newLines)
        {
            CultureInfo es = SellerCulture2825;
            StringBuilder sb = new StringBuilder();
            DateTime oldWhen = oldItem.DateValue;
            if (oldWhen != DateTime.MinValue && oldWhen.ToString("yyyyMMddHHmm") != newWhen.ToString("yyyyMMddHHmm"))
                sb.AppendLine("Fecha/hora: " + oldWhen.ToString("dd/MM/yyyy HH:mm") + " → " + newWhen.ToString("dd/MM/yyyy HH:mm") + ".");
            string oldNote = (oldItem.Note ?? "").Trim();
            string normalizedNewNote = (newNote ?? "").Trim();
            if (!string.Equals(oldNote, normalizedNewNote, StringComparison.Ordinal))
                sb.AppendLine("Nota: " + (oldNote.Length == 0 ? "(vacía)" : oldNote) + " → " + (normalizedNewNote.Length == 0 ? "(vacía)" : normalizedNewNote) + ".");

            Dictionary<string, int> before = SellerQuantityMap2827(oldItem.Lines);
            Dictionary<string, int> after = SellerQuantityMap2827(newLines);
            HashSet<string> keys = new HashSet<string>(before.Keys, StringComparer.Ordinal);
            foreach (string key in after.Keys) keys.Add(key);
            List<string> sorted = new List<string>(keys);
            sorted.Sort(StringComparer.CurrentCultureIgnoreCase);
            foreach (string key in sorted)
            {
                int oldQty = 0, newQty = 0;
                before.TryGetValue(key, out oldQty);
                after.TryGetValue(key, out newQty);
                if (oldQty == newQty) continue;
                SellerLine2825 sample = SellerLineFromKey2827(key, 1);
                double delta = (newQty - oldQty) * sample.UnitValue;
                string label = sample.Type + " " + sample.Denomination;
                sb.AppendLine(label + ": " + oldQty + " → " + newQty + " (" + (delta >= 0 ? "+" : "") + delta.ToString("N2", es) + " €).");
            }
            double oldTotal = oldItem.Total;
            double newTotal = SellerTotalFromLines2827(newLines);
            double totalDelta = newTotal - oldTotal;
            sb.Append("Total: " + oldTotal.ToString("N2", es) + " € → " + newTotal.ToString("N2", es) + " € (" + (totalDelta >= 0 ? "+" : "") + totalDelta.ToString("N2", es) + " €).");
            return sb.ToString();
        }

        private void SaveSellerCorrection2827()
        {
            SellerLiquidation2826 item = FindSellerLiquidation2827(sellerCorrectionId2827);
            if (item == null)
            {
                PlayNamedSound("aviso");
                AnnounceToScreenReader("No hay ninguna liquidación cargada en modo corrección.");
                return;
            }
            if (sellerLines2825.Count == 0)
            {
                PlayNamedSound("aviso");
                AnnounceToScreenReader("La corrección no puede guardarse sin filas. Si quieres eliminar toda la liquidación, hazlo desde Liquidaciones guardadas.");
                return;
            }
            DateTime newWhen = sellerRecordDate2826 == null ? item.DateValue : sellerRecordDate2826.Value;
            if (newWhen == DateTime.MinValue) newWhen = DateTime.Now;
            string newNote = sellerRecordNote2826 == null ? item.Note : sellerRecordNote2826.Text.Trim();
            bool sameDate = item.DateValue != DateTime.MinValue && item.DateValue.ToString("yyyyMMddHHmm") == newWhen.ToString("yyyyMMddHHmm");
            bool sameNote = string.Equals((item.Note ?? "").Trim(), (newNote ?? "").Trim(), StringComparison.Ordinal);
            bool sameLines = SellerLinesEqual2827(item.Lines, sellerLines2825);
            if (sameDate && sameNote && sameLines)
            {
                PlayNamedSound("aviso");
                AnnounceToScreenReader("No hay cambios que guardar en esta liquidación.");
                return;
            }

            string summary = BuildSellerCorrectionSummary2827(item, newWhen, newNote, sellerLines2825);
            DialogResult answer = MessageBox.Show("Se guardará la corrección y se conservará una copia de los datos anteriores.\r\n\r\n" + summary + "\r\n\r\n¿Guardar corrección?", "Guardar corrección", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (answer != DialogResult.Yes) return;

            SellerRevision2827 revision = new SellerRevision2827();
            revision.CorrectedAt = DateTime.Now.ToString("o", CultureInfo.InvariantCulture);
            revision.PreviousWhen = item.When;
            revision.PreviousNote = item.Note;
            revision.ChangeSummary = summary;
            foreach (SellerLine2825 line in item.Lines) revision.PreviousLines.Add(CloneSellerLine2826(line));
            item.Revisions.Add(revision);

            item.When = newWhen.ToString("o", CultureInfo.InvariantCulture);
            item.Note = newNote;
            item.UpdatedAt = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
            item.DeviceId = LiquidacionMovilDeviceId2940();
            item.Lines.Clear();
            foreach (SellerLine2825 line in sellerLines2825) item.Lines.Add(CloneSellerLine2826(line));
            QueueSellerLiquidationsSave285();
            RefreshSellerSavedList2826();
            sellerCorrectionId2827 = "";
            UpdateSellerCorrectionStatus2827();
            PlayCalculatorSound1554("calc_historial");
            AnnounceToScreenReader("Corrección guardada. Nuevo total " + item.Total.ToString("N2", SellerCulture2825) + " euros. La versión anterior se conserva en el historial de correcciones.");
        }

        private void ShowSellerCorrections2827()
        {
            SellerLiquidation2826 item = SelectedSellerSaved2826();
            if (item == null) { PlayNamedSound("aviso"); return; }
            if (item.Revisions.Count == 0)
            {
                if (sellerSavedDetail2826 != null) sellerSavedDetail2826.Text = "Esta liquidación no tiene correcciones registradas.";
                AnnounceToScreenReader("Esta liquidación no tiene correcciones registradas.");
                return;
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Historial de correcciones de la liquidación");
            sb.AppendLine("Estado actual: " + item.Total.ToString("N2", SellerCulture2825) + " €.");
            sb.AppendLine();
            for (int i = item.Revisions.Count - 1; i >= 0; i--)
            {
                SellerRevision2827 revision = item.Revisions[i];
                DateTime corrected = revision.CorrectedDateValue;
                sb.AppendLine("Corrección " + (i + 1) + " — " + (corrected == DateTime.MinValue ? revision.CorrectedAt : corrected.ToString("dd/MM/yyyy HH:mm")));
                sb.AppendLine(revision.ChangeSummary);
                sb.AppendLine("Total que había antes: " + revision.PreviousTotal.ToString("N2", SellerCulture2825) + " €.");
                sb.AppendLine();
            }
            if (sellerSavedDetail2826 != null)
            {
                sellerSavedDetail2826.Text = sb.ToString().TrimEnd();
                sellerSavedDetail2826.Focus();
            }
            PlayCalculatorSound1554("calc_memoria");
            AnnounceToScreenReader("Historial de correcciones. " + item.Revisions.Count + (item.Revisions.Count == 1 ? " corrección." : " correcciones."));
        }

        private void DeleteSellerSaved2826()
        {
            SellerLiquidation2826 item = SelectedSellerSaved2826();
            if (item == null) { PlayNamedSound("aviso"); return; }
            string label = item.ToString();
            DialogResult answer = MessageBox.Show("¿Eliminar esta liquidación guardada?\r\n\r\n" + label + "\r\n\r\nEl borrador actual no se modificará.", "Eliminar liquidación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (answer != DialogResult.Yes) return;
            RecordSellerLiquidationSyncTombstone2878(item);
            sellerSaved2826.Remove(item);
            if (string.Equals(sellerCorrectionId2827, item.Id, StringComparison.OrdinalIgnoreCase)) CancelSellerCorrection2827(false);
            QueueSellerLiquidationsSave285();
            RefreshSellerSavedList2826();
            PlayCalculatorSound1554("calc_liquidacion_borrar");
            AnnounceToScreenReader("Liquidación guardada eliminada.");
        }

        private Dictionary<string, object> SellerLineToDictionary2826(SellerLine2825 line)
        {
            Dictionary<string, object> item = new Dictionary<string, object>();
            item["tipo"] = line.Type;
            item["denominacion"] = line.Denomination;
            item["cantidad"] = line.Quantity;
            item["valor_unitario"] = line.UnitValue;
            item["unidades_envase"] = line.UnitsPerContainer;
            return item;
        }

        private SellerLine2825 SellerLineFromDictionary2826(Dictionary<string, object> item)
        {
            if (item == null) return null;
            SellerLine2825 line = new SellerLine2825();
            object value;
            if (item.TryGetValue("tipo", out value)) line.Type = Convert.ToString(value);
            if (item.TryGetValue("denominacion", out value)) line.Denomination = Convert.ToString(value);
            if (item.TryGetValue("cantidad", out value)) line.Quantity = Convert.ToInt32(value, CultureInfo.InvariantCulture);
            if (item.TryGetValue("valor_unitario", out value)) line.UnitValue = Convert.ToDouble(value, CultureInfo.InvariantCulture);
            if (item.TryGetValue("unidades_envase", out value)) line.UnitsPerContainer = Convert.ToInt32(value, CultureInfo.InvariantCulture);
            return line.Quantity > 0 && line.UnitValue >= 0 ? line : null;
        }

        private void LoadSellerLiquidations2826()
        {
            sellerSaved2826.Clear();
            try
            {
                if (string.IsNullOrWhiteSpace(sellerSavedPath2826) || !File.Exists(sellerSavedPath2826)) return;
                object raw = LoadCalculatorJsonObject285(sellerSavedPath2826, "las liquidaciones guardadas");
                IList list = raw as IList;
                if (list == null) return;
                foreach (object entry in list)
                {
                    Dictionary<string, object> data = entry as Dictionary<string, object>;
                    if (data == null) continue;
                    SellerLiquidation2826 saved = new SellerLiquidation2826();
                    object value;
                    if (data.TryGetValue("id", out value)) saved.Id = Convert.ToString(value);
                    if (string.IsNullOrWhiteSpace(saved.Id)) saved.Id = Guid.NewGuid().ToString("N");
                    if (data.TryGetValue("fecha", out value)) saved.When = Convert.ToString(value);
                    if (data.TryGetValue("nota", out value)) saved.Note = Convert.ToString(value);
                    if (data.TryGetValue("updated_at", out value)) saved.UpdatedAt = Convert.ToString(value);
                    if (data.TryGetValue("deleted_at", out value)) saved.DeletedAt = Convert.ToString(value);
                    if (data.TryGetValue("device_id", out value)) saved.DeviceId = Convert.ToString(value);
                    if (data.TryGetValue("filas", out value))
                    {
                        IList rows = value as IList;
                        if (rows != null)
                        {
                            foreach (object row in rows)
                            {
                                SellerLine2825 line = SellerLineFromDictionary2826(row as Dictionary<string, object>);
                                if (line != null) saved.Lines.Add(line);
                            }
                        }
                    }
                    if (data.TryGetValue("correcciones", out value))
                    {
                        IList revisions = value as IList;
                        if (revisions != null)
                        {
                            foreach (object revisionEntry in revisions)
                            {
                                Dictionary<string, object> revisionData = revisionEntry as Dictionary<string, object>;
                                if (revisionData == null) continue;
                                SellerRevision2827 revision = new SellerRevision2827();
                                object revisionValue;
                                if (revisionData.TryGetValue("fecha_correccion", out revisionValue)) revision.CorrectedAt = Convert.ToString(revisionValue);
                                if (revisionData.TryGetValue("fecha_anterior", out revisionValue)) revision.PreviousWhen = Convert.ToString(revisionValue);
                                if (revisionData.TryGetValue("nota_anterior", out revisionValue)) revision.PreviousNote = Convert.ToString(revisionValue);
                                if (revisionData.TryGetValue("resumen", out revisionValue)) revision.ChangeSummary = Convert.ToString(revisionValue);
                                if (revisionData.TryGetValue("filas_anteriores", out revisionValue))
                                {
                                    IList previousRows = revisionValue as IList;
                                    if (previousRows != null)
                                    {
                                        foreach (object previousRow in previousRows)
                                        {
                                            SellerLine2825 line = SellerLineFromDictionary2826(previousRow as Dictionary<string, object>);
                                            if (line != null) revision.PreviousLines.Add(line);
                                        }
                                    }
                                }
                                revision.ChangeSummary = revision.ChangeSummary ?? "";
                                saved.Revisions.Add(revision);
                            }
                        }
                    }
                    if (saved.Lines.Count > 0 && string.IsNullOrWhiteSpace(saved.DeletedAt)) sellerSaved2826.Add(saved);
                }
            }
            catch { }
        }

        private void SaveSellerLiquidations2826()
        {
            if (string.IsNullOrWhiteSpace(sellerSavedPath2826)) return;
            try
            {
                ArrayList list = new ArrayList();
                foreach (SellerLiquidation2826 saved in sellerSaved2826)
                {
                    Dictionary<string, object> data = new Dictionary<string, object>();
                    data["id"] = saved.Id;
                    data["fecha"] = saved.When;
                    data["nota"] = saved.Note;
                    data["updated_at"] = string.IsNullOrWhiteSpace(saved.UpdatedAt) ? saved.When : saved.UpdatedAt;
                    data["deleted_at"] = string.IsNullOrWhiteSpace(saved.DeletedAt) ? null : (object)saved.DeletedAt;
                    data["device_id"] = saved.DeviceId ?? "";
                    ArrayList rows = new ArrayList();
                    foreach (SellerLine2825 line in saved.Lines) rows.Add(SellerLineToDictionary2826(line));
                    data["filas"] = rows;
                    ArrayList revisions = new ArrayList();
                    foreach (SellerRevision2827 revision in saved.Revisions)
                    {
                        Dictionary<string, object> revisionData = new Dictionary<string, object>();
                        revisionData["fecha_correccion"] = revision.CorrectedAt;
                        revisionData["fecha_anterior"] = revision.PreviousWhen;
                        revisionData["nota_anterior"] = revision.PreviousNote;
                        revisionData["resumen"] = revision.ChangeSummary;
                        ArrayList previousRows = new ArrayList();
                        foreach (SellerLine2825 line in revision.PreviousLines) previousRows.Add(SellerLineToDictionary2826(line));
                        revisionData["filas_anteriores"] = previousRows;
                        revisions.Add(revisionData);
                    }
                    if (revisions.Count > 0) data["correcciones"] = revisions;
                    list.Add(data);
                }
                Directory.CreateDirectory(dataDir);
                File.WriteAllText(sellerSavedPath2826, new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(list), new UTF8Encoding(false));
            }
            catch { }
        }

        private void LoadSellerDraft2825()
        {
            sellerLoading2825 = true;
            try
            {
                sellerLines2825.Clear();
                if (string.IsNullOrWhiteSpace(sellerDraftPath2825) || !File.Exists(sellerDraftPath2825)) return;
                object raw = LoadCalculatorJsonObject285(sellerDraftPath2825, "el borrador de la liquidación");
                IList list = raw as IList;
                if (list == null) return;
                foreach (object entry in list)
                {
                    Dictionary<string, object> item = entry as Dictionary<string, object>;
                    if (item == null) continue;
                    SellerLine2825 line = new SellerLine2825();
                    object value;
                    if (item.TryGetValue("tipo", out value)) line.Type = Convert.ToString(value);
                    if (item.TryGetValue("denominacion", out value)) line.Denomination = Convert.ToString(value);
                    if (item.TryGetValue("cantidad", out value)) line.Quantity = Convert.ToInt32(value, CultureInfo.InvariantCulture);
                    if (item.TryGetValue("valor_unitario", out value)) line.UnitValue = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                    if (item.TryGetValue("unidades_envase", out value)) line.UnitsPerContainer = Convert.ToInt32(value, CultureInfo.InvariantCulture);
                    if (line.Quantity > 0 && line.UnitValue >= 0) sellerLines2825.Add(line);
                }
            }
            catch { sellerLines2825.Clear(); }
            finally { sellerLoading2825 = false; }
        }

        private void SaveSellerDraft2825()
        {
            if (sellerLoading2825 || string.IsNullOrWhiteSpace(sellerDraftPath2825)) return;
            try
            {
                ArrayList list = new ArrayList();
                foreach (SellerLine2825 line in sellerLines2825)
                {
                    Dictionary<string, object> item = new Dictionary<string, object>();
                    item["tipo"] = line.Type;
                    item["denominacion"] = line.Denomination;
                    item["cantidad"] = line.Quantity;
                    item["valor_unitario"] = line.UnitValue;
                    item["unidades_envase"] = line.UnitsPerContainer;
                    list.Add(item);
                }
                Directory.CreateDirectory(dataDir);
                File.WriteAllText(sellerDraftPath2825, new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(list), new UTF8Encoding(false));
            }
            catch { }
        }

        private void SelectComboText2825(ComboBox combo, string text)
        {
            if (combo == null) return;
            for (int i = 0; i < combo.Items.Count; i++)
            {
                if (string.Equals(Convert.ToString(combo.Items[i]), text, StringComparison.OrdinalIgnoreCase))
                {
                    combo.SelectedIndex = i;
                    return;
                }
            }
        }

        private TabPage BuildCalculatorHistoryPage2824()
        {
            TabPage page = new TabPage("Historial");
            TableLayoutPanel layout = BaseLayout();

            calculatorHistoryList2824 = new ListBox();
            calculatorHistoryList2824.Dock = DockStyle.Fill;
            calculatorHistoryList2824.Height = 180;
            calculatorHistoryList2824.AccessibleName = "Historial de operaciones";
            calculatorHistoryList2824.SelectedIndexChanged += delegate { RefreshHistoryDetail2824(); };
            calculatorHistoryList2824.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Delete)
                {
                    e.Handled = true;
                    DeleteSelectedHistory2824();
                }
                else if (e.Control && e.KeyCode == Keys.C)
                {
                    e.Handled = true;
                    CopySelectedHistoryResult2824();
                }
            };
            AddFullRow(layout, calculatorHistoryList2824, true);

            calculatorHistoryDetail2824 = AddLabeledText(layout, "Detalle:", true);
            calculatorHistoryDetail2824.ReadOnly = true;
            calculatorHistoryDetail2824.Height = 120;

            FlowLayoutPanel buttons = ButtonRow();
            buttons.Controls.Add(NewButton("Copiar resultado", delegate { CopySelectedHistoryResult2824(); }));
            buttons.Controls.Add(NewButton("Copiar operación completa", delegate { CopySelectedHistoryFull2824(); }));
            buttons.Controls.Add(NewButton("Reutilizar valores", delegate { ReuseSelectedHistory2824(); }));
            buttons.Controls.Add(NewButton("Eliminar seleccionado", delegate { DeleteSelectedHistory2824(); }));
            buttons.Controls.Add(NewButton("Vaciar historial", delegate { ClearHistory2824(); }));
            AddFullRow(layout, buttons, false);

            TextBox help = ResultBox();
            help.Height = 70;
            help.Text = "El historial se guarda localmente en datos\\calculadora_historial.json y conserva hasta 300 operaciones. Suprimir elimina la entrada seleccionada y Control+C copia su resultado. Los errores y los estados temporales de consulta no se guardan.";
            AddResultRow(layout, "Ayuda:", help);

            RefreshHistoryList2824(-1);
            page.Controls.Add(layout);
            return page;
        }

        private void RefreshAdvancedOperations2824()
        {
            if (advancedCategory2824 == null || advancedOperation2824 == null) return;
            string category = advancedCategory2824.Text;
            string[] operations;
            if (category == "Porcentajes y compras")
                operations = new string[] { "Cambio porcentual", "Margen sobre precio de venta", "Descuento encadenado", "Cambio a devolver", "Propina y reparto" };
            else if (category == "Estadística")
                operations = new string[] { "Suma", "Media", "Mediana", "Mínimo", "Máximo", "Rango", "Desviación estándar poblacional" };
            else if (category == "Científica")
                operations = new string[] { "Raíz cúbica", "Logaritmo decimal", "Logaritmo natural", "Seno en grados", "Coseno en grados", "Tangente en grados", "Factorial", "Combinaciones n sobre r", "Permutaciones de n tomando r" };
            else if (category == "Finanzas")
                operations = new string[] { "Interés simple", "Interés compuesto", "Cuota mensual de préstamo", "Repartir una cuenta" };
            else
                operations = new string[] { "Decimal a binario", "Decimal a hexadecimal", "Decimal a octal", "Binario a decimal", "Hexadecimal a decimal", "Octal a decimal" };

            advancedOperation2824.Items.Clear();
            advancedOperation2824.Items.AddRange(operations);
            if (advancedOperation2824.Items.Count > 0) advancedOperation2824.SelectedIndex = 0;
            ApplyAdvancedExample2824();
        }

        private void ApplyAdvancedExample2824()
        {
            if (advancedOperation2824 == null || advancedInput2824 == null) return;
            string op = advancedOperation2824.Text;
            if (op == "Cambio porcentual") advancedInput2824.Text = "100;125";
            else if (op == "Margen sobre precio de venta") advancedInput2824.Text = "60;100";
            else if (op == "Descuento encadenado") advancedInput2824.Text = "100;20;10";
            else if (op == "Cambio a devolver") advancedInput2824.Text = "17,50;20";
            else if (op == "Propina y reparto") advancedInput2824.Text = "40;10;2";
            else if (op == "Suma" || op == "Media" || op == "Mediana" || op == "Mínimo" || op == "Máximo" || op == "Rango" || op.StartsWith("Desviación")) advancedInput2824.Text = "10;20;30;40";
            else if (op == "Raíz cúbica") advancedInput2824.Text = "27";
            else if (op.StartsWith("Logaritmo")) advancedInput2824.Text = "100";
            else if (op.StartsWith("Seno") || op.StartsWith("Coseno") || op.StartsWith("Tangente")) advancedInput2824.Text = "45";
            else if (op == "Factorial") advancedInput2824.Text = "5";
            else if (op.StartsWith("Combinaciones") || op.StartsWith("Permutaciones")) advancedInput2824.Text = "10;3";
            else if (op == "Interés simple") advancedInput2824.Text = "1000;5;2";
            else if (op == "Interés compuesto") advancedInput2824.Text = "1000;5;10;12";
            else if (op == "Cuota mensual de préstamo") advancedInput2824.Text = "10000;6;36";
            else if (op == "Repartir una cuenta") advancedInput2824.Text = "75;3";
            else if (op.StartsWith("Decimal a")) advancedInput2824.Text = "255";
            else if (op == "Binario a decimal") advancedInput2824.Text = "11111111";
            else if (op == "Hexadecimal a decimal") advancedInput2824.Text = "FF";
            else if (op == "Octal a decimal") advancedInput2824.Text = "377";
        }

        private void CalculateAdvanced2824()
        {
            PlayCalculatorSound1554("calc_igual");
            if (advancedOperation2824 == null || advancedInput2824 == null || advancedResult2824 == null) return;
            string op = advancedOperation2824.Text;
            string input = advancedInput2824.Text.Trim();
            try
            {
                string result = CalculateAdvancedText2824(op, input);
                advancedResult2824.Text = result;
                ScheduleCalculatorSound1554("calc_resultado", 125);
                AddCalculatorHistory2824("Avanzada: " + op, input, result, "advanced");
                SetStatus("Cálculo avanzado listo.");
                AnnounceResults(advancedResult2824, result);
            }
            catch (Exception ex)
            {
                advancedResult2824.Text = "No se pudo calcular: " + ex.Message;
                PlayNamedSound("error");
                SetStatus("Entrada no válida.");
                AnnounceResults(advancedResult2824, advancedResult2824.Text);
            }
        }

        private string CalculateAdvancedText2824(string op, string input)
        {
            CultureInfo es = SellerCulture2825;
            double[] values;
            if (op == "Cambio porcentual")
            {
                values = ParseNumberList(input, 2);
                if (values[0] == 0) throw new Exception("El valor inicial no puede ser cero.");
                double change = (values[1] - values[0]) / Math.Abs(values[0]) * 100.0;
                return "Cambio: " + change.ToString("0.############", es) + " %.";
            }
            if (op == "Margen sobre precio de venta")
            {
                values = ParseNumberList(input, 2);
                if (values[1] == 0) throw new Exception("El precio de venta no puede ser cero.");
                double margin = (values[1] - values[0]) / values[1] * 100.0;
                return "Margen: " + margin.ToString("0.############", es) + " %. Beneficio unitario: " + (values[1] - values[0]).ToString("0.############", es) + ".";
            }
            if (op == "Descuento encadenado")
            {
                values = ParseNumberList(input, 2);
                double result = values[0];
                for (int i = 1; i < values.Length; i++) result *= 1.0 - values[i] / 100.0;
                double effective = values[0] == 0 ? 0 : (1.0 - result / values[0]) * 100.0;
                return "Precio final: " + result.ToString("0.############", es) + ". Descuento efectivo: " + effective.ToString("0.############", es) + " %.";
            }
            if (op == "Cambio a devolver")
            {
                values = ParseNumberList(input, 2);
                double change = values[1] - values[0];
                if (change < 0) return "Faltan " + Math.Abs(change).ToString("0.############", es) + ".";
                return "Cambio a devolver: " + change.ToString("0.############", es) + ".";
            }
            if (op == "Propina y reparto")
            {
                values = ParseNumberList(input, 2);
                double people = values.Length >= 3 ? values[2] : 1;
                if (people <= 0) throw new Exception("El número de personas debe ser mayor que cero.");
                double tip = values[0] * values[1] / 100.0;
                double total = values[0] + tip;
                return "Propina: " + tip.ToString("0.############", es) + ". Total: " + total.ToString("0.############", es) + ". Por persona: " + (total / people).ToString("0.############", es) + ".";
            }
            if (op == "Suma" || op == "Media" || op == "Mediana" || op == "Mínimo" || op == "Máximo" || op == "Rango" || op.StartsWith("Desviación"))
            {
                values = ParseNumberList(input, 1);
                double sum = 0;
                double min = values[0];
                double max = values[0];
                for (int i = 0; i < values.Length; i++)
                {
                    sum += values[i];
                    if (values[i] < min) min = values[i];
                    if (values[i] > max) max = values[i];
                }
                if (op == "Suma") return "Suma: " + sum.ToString("0.############", es) + ".";
                if (op == "Media") return "Media: " + (sum / values.Length).ToString("0.############", es) + ".";
                if (op == "Mínimo") return "Mínimo: " + min.ToString("0.############", es) + ".";
                if (op == "Máximo") return "Máximo: " + max.ToString("0.############", es) + ".";
                if (op == "Rango") return "Rango: " + (max - min).ToString("0.############", es) + ".";
                if (op == "Mediana")
                {
                    double[] copy = (double[])values.Clone();
                    Array.Sort(copy);
                    double median = copy.Length % 2 == 1 ? copy[copy.Length / 2] : (copy[copy.Length / 2 - 1] + copy[copy.Length / 2]) / 2.0;
                    return "Mediana: " + median.ToString("0.############", es) + ".";
                }
                double mean = sum / values.Length;
                double squared = 0;
                for (int i = 0; i < values.Length; i++) squared += (values[i] - mean) * (values[i] - mean);
                return "Desviación estándar poblacional: " + Math.Sqrt(squared / values.Length).ToString("0.############", es) + ".";
            }
            if (op == "Raíz cúbica")
            {
                values = ParseNumberList(input, 1);
                double x = values[0];
                double root = x < 0 ? -Math.Pow(-x, 1.0 / 3.0) : Math.Pow(x, 1.0 / 3.0);
                return "Raíz cúbica: " + root.ToString("0.############", es) + ".";
            }
            if (op == "Logaritmo decimal" || op == "Logaritmo natural")
            {
                values = ParseNumberList(input, 1);
                if (values[0] <= 0) throw new Exception("El valor debe ser mayor que cero.");
                double result = op == "Logaritmo decimal" ? Math.Log10(values[0]) : Math.Log(values[0]);
                return "Resultado: " + result.ToString("0.############", es) + ".";
            }
            if (op == "Seno en grados" || op == "Coseno en grados" || op == "Tangente en grados")
            {
                values = ParseNumberList(input, 1);
                double radians = values[0] * Math.PI / 180.0;
                double result = op.StartsWith("Seno") ? Math.Sin(radians) : (op.StartsWith("Coseno") ? Math.Cos(radians) : Math.Tan(radians));
                return "Resultado: " + result.ToString("0.############", es) + ".";
            }
            if (op == "Factorial")
            {
                values = ParseNumberList(input, 1);
                int n = RequireInteger2824(values[0], 0, 170, "factorial");
                double result = 1;
                for (int i = 2; i <= n; i++) result *= i;
                return n.ToString(CultureInfo.InvariantCulture) + "! = " + result.ToString("0.############E+0", es) + ".";
            }
            if (op.StartsWith("Combinaciones") || op.StartsWith("Permutaciones"))
            {
                values = ParseNumberList(input, 2);
                int n = RequireInteger2824(values[0], 0, 170, "n");
                int r = RequireInteger2824(values[1], 0, n, "r");
                double result = op.StartsWith("Combinaciones") ? Combination2824(n, r) : Permutation2824(n, r);
                return "Resultado: " + result.ToString("0.############E+0", es) + ".";
            }
            if (op == "Interés simple")
            {
                values = ParseNumberList(input, 3);
                double interest = values[0] * values[1] / 100.0 * values[2];
                return "Intereses: " + interest.ToString("0.############", es) + ". Total: " + (values[0] + interest).ToString("0.############", es) + ".";
            }
            if (op == "Interés compuesto")
            {
                values = ParseNumberList(input, 3);
                double n = values.Length >= 4 ? values[3] : 12;
                if (n <= 0) throw new Exception("Las capitalizaciones por año deben ser mayores que cero.");
                double total = values[0] * Math.Pow(1.0 + values[1] / 100.0 / n, n * values[2]);
                return "Capital final aproximado: " + total.ToString("0.############", es) + ". Intereses: " + (total - values[0]).ToString("0.############", es) + ".";
            }
            if (op == "Cuota mensual de préstamo")
            {
                values = ParseNumberList(input, 3);
                int months = RequireInteger2824(values[2], 1, 1200, "meses");
                double monthlyRate = values[1] / 100.0 / 12.0;
                double payment = monthlyRate == 0 ? values[0] / months : values[0] * monthlyRate / (1.0 - Math.Pow(1.0 + monthlyRate, -months));
                return "Cuota mensual aproximada: " + payment.ToString("0.############", es) + ". Total de cuotas: " + (payment * months).ToString("0.############", es) + ". No incluye comisiones ni seguros.";
            }
            if (op == "Repartir una cuenta")
            {
                values = ParseNumberList(input, 2);
                if (values[1] <= 0) throw new Exception("El número de personas debe ser mayor que cero.");
                return "Por persona: " + (values[0] / values[1]).ToString("0.############", es) + ".";
            }

            return ConvertNumberBase2824(op, input);
        }

        private int RequireInteger2824(double value, int min, int max, string name)
        {
            if (Math.Abs(value - Math.Round(value)) > 0.000000001) throw new Exception(name + " debe ser un número entero.");
            int result = Convert.ToInt32(Math.Round(value));
            if (result < min || result > max) throw new Exception(name + " debe estar entre " + min + " y " + max + ".");
            return result;
        }

        private double Combination2824(int n, int r)
        {
            r = Math.Min(r, n - r);
            double result = 1;
            for (int i = 1; i <= r; i++) result = result * (n - r + i) / i;
            return result;
        }

        private double Permutation2824(int n, int r)
        {
            double result = 1;
            for (int i = 0; i < r; i++) result *= (n - i);
            return result;
        }

        private string ConvertNumberBase2824(string op, string input)
        {
            string text = (input ?? "").Trim();
            if (text.Length == 0) throw new Exception("Escribe un valor.");
            long value;
            if (op.StartsWith("Decimal a"))
            {
                if (!long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value)) throw new Exception("Escribe un entero decimal válido.");
                if (op == "Decimal a binario") return "Binario: " + Convert.ToString(value, 2) + ".";
                if (op == "Decimal a hexadecimal") return "Hexadecimal: " + value.ToString("X", CultureInfo.InvariantCulture) + ".";
                return "Octal: " + Convert.ToString(value, 8) + ".";
            }
            int fromBase = op == "Binario a decimal" ? 2 : (op == "Hexadecimal a decimal" ? 16 : 8);
            try { value = Convert.ToInt64(text, fromBase); }
            catch { throw new Exception("El valor no es válido para base " + fromBase + "."); }
            return "Decimal: " + value.ToString(CultureInfo.InvariantCulture) + ".";
        }

        private void CopyBasicCalculatorResult2824()
        {
            CopyTextResult2824(calculadoraResultado);
        }

        private void CopyTextResult2824(TextBox box)
        {
            if (box == null || string.IsNullOrWhiteSpace(box.Text))
            {
                PlayNamedSound("aviso");
                AnnounceToScreenReader("No hay resultado para copiar.");
                return;
            }
            try
            {
                Clipboard.SetText(box.Text);
                PlayCalculatorSound1554("calc_copiar");
                SetStatus("Resultado copiado.");
                AnnounceToScreenReader("Resultado copiado.");
            }
            catch (Exception ex)
            {
                PlayNamedSound("error");
                AnnounceToScreenReader("No se pudo copiar el resultado. " + ex.Message);
            }
        }

        private void UseBasicCalculatorResult2824()
        {
            if (calculadoraInput == null || calculadoraResultado == null) return;
            double value;
            if (!TryExtractFirstNumber2824(calculadoraResultado.Text, out value))
            {
                PlayNamedSound("aviso");
                AnnounceToScreenReader("El resultado no contiene un número reutilizable.");
                return;
            }
            calculadoraInput.Text = value.ToString("0.############", SellerCulture2825);
            calculadoraInput.Focus();
            calculadoraInput.SelectAll();
            PlayCalculatorSound1554("calc_memoria");
            AnnounceToScreenReader("Resultado colocado como nueva entrada.");
        }

        private void UseAdvancedResultInBasic2824()
        {
            if (advancedResult2824 == null) return;
            double value;
            if (!TryExtractFirstNumber2824(advancedResult2824.Text, out value))
            {
                PlayNamedSound("aviso");
                AnnounceToScreenReader("No se encontró un número reutilizable en el resultado.");
                return;
            }
            if (calculadoraInput != null) calculadoraInput.Text = value.ToString("0.############", SellerCulture2825);
            if (calculatorTabs2824 != null && calculatorBasicPage2824 != null) calculatorTabs2824.SelectedTab = calculatorBasicPage2824;
            if (calculadoraInput != null) { calculadoraInput.Focus(); calculadoraInput.SelectAll(); }
            PlayCalculatorSound1554("calc_memoria");
            AnnounceToScreenReader("Resultado enviado a la calculadora básica.");
        }

        private void SaveCalculatorMemory2824()
        {
            SaveTextResultToMemory2824(calculadoraResultado);
        }

        private void SaveTextResultToMemory2824(TextBox box)
        {
            if (box == null) return;
            double value;
            if (!TryExtractFirstNumber2824(box.Text, out value))
            {
                PlayNamedSound("aviso");
                AnnounceToScreenReader("No se encontró un número para guardar en memoria.");
                return;
            }
            calculatorMemory2824 = value;
            calculatorMemorySet2824 = true;
            PlayCalculatorSound1554("calc_memoria");
            AnnounceToScreenReader("Guardado en memoria: " + value.ToString("0.############", SellerCulture2825) + ".");
        }

        private void RecallCalculatorMemory2824()
        {
            if (!calculatorMemorySet2824)
            {
                PlayNamedSound("aviso");
                AnnounceToScreenReader("La memoria está vacía.");
                return;
            }
            if (calculadoraInput != null)
            {
                calculadoraInput.Text = calculatorMemory2824.ToString("0.############", SellerCulture2825);
                calculadoraInput.Focus();
                calculadoraInput.SelectAll();
            }
            PlayCalculatorSound1554("calc_memoria");
            AnnounceToScreenReader("Memoria recuperada.");
        }

        private void ClearAdvanced2824()
        {
            PlayCalculatorSound1554("calc_borrar");
            if (advancedInput2824 != null) advancedInput2824.Clear();
            if (advancedResult2824 != null) advancedResult2824.Clear();
            if (advancedInput2824 != null) advancedInput2824.Focus();
        }

        private bool TryExtractFirstNumber2824(string text, out double value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(text)) return false;
            System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(text, @"[-+]?\d+(?:[\.,]\d+)?(?:[eE][-+]?\d+)?");
            if (!match.Success) return false;
            return double.TryParse(match.Value.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        private void DescribeResultCuriosity2824(string text)
        {
            double value;
            if (!TryExtractFirstNumber2824(text, out value))
            {
                PlayNamedSound("aviso");
                AnnounceToScreenReader("Primero calcula un resultado numérico.");
                return;
            }
            StringBuilder sb = new StringBuilder();
            sb.Append("Curiosidad: ");
            if (value == 0) sb.Append("es cero");
            else sb.Append(value > 0 ? "es positivo" : "es negativo");
            double rounded = Math.Round(value);
            if (Math.Abs(value - rounded) < 0.000000001 && Math.Abs(rounded) <= 2000000000)
            {
                long n = Convert.ToInt64(rounded);
                sb.Append(", entero");
                sb.Append(n % 2 == 0 ? " y par" : " e impar");
                long abs = Math.Abs(n);
                if (abs >= 2 && IsPrime2824(abs)) sb.Append(", además es primo");
                long root = Convert.ToInt64(Math.Floor(Math.Sqrt(abs)));
                if (root * root == abs) sb.Append(", y su valor absoluto es un cuadrado perfecto");
                if (abs > 0)
                {
                    long digits = abs;
                    int sum = 0;
                    while (digits > 0) { sum += Convert.ToInt32(digits % 10); digits /= 10; }
                    sb.Append(". La suma de sus cifras es " + sum);
                }
            }
            sb.Append(".");
            PlayCalculatorSound1554("calc_historial");
            AnnounceToScreenReader(sb.ToString());
            SetStatus(sb.ToString());
        }

        private bool IsPrime2824(long n)
        {
            if (n < 2) return false;
            if (n % 2 == 0) return n == 2;
            for (long i = 3; i * i <= n; i += 2) if (n % i == 0) return false;
            return true;
        }

        private CryptoChoice2824[] CryptoChoices2824()
        {
            return new CryptoChoice2824[]
            {
                new CryptoChoice2824("EUR", "euros", false),
                new CryptoChoice2824("USD", "dólares estadounidenses", false),
                new CryptoChoice2824("GBP", "libras esterlinas", false),
                new CryptoChoice2824("BTC", "Bitcoin", true),
                new CryptoChoice2824("ETH", "Ether", true),
                new CryptoChoice2824("SOL", "Solana", true),
                new CryptoChoice2824("XRP", "XRP", true),
                new CryptoChoice2824("ADA", "Cardano", true),
                new CryptoChoice2824("DOGE", "Dogecoin", true),
                new CryptoChoice2824("LTC", "Litecoin", true),
                new CryptoChoice2824("BCH", "Bitcoin Cash", true),
                new CryptoChoice2824("LINK", "Chainlink", true),
                new CryptoChoice2824("XLM", "Stellar", true),
                new CryptoChoice2824("AVAX", "Avalanche", true)
            };
        }

        private void SetComboText2824(ComboBox combo, string text)
        {
            if (combo == null) return;
            for (int i = 0; i < combo.Items.Count; i++)
            {
                if (string.Equals(Convert.ToString(combo.Items[i]), text, StringComparison.OrdinalIgnoreCase))
                {
                    combo.SelectedIndex = i;
                    return;
                }
            }
        }

        private void SwapComboIndexes2824(ComboBox a, ComboBox b)
        {
            if (a == null || b == null) return;
            int index = a.SelectedIndex;
            a.SelectedIndex = b.SelectedIndex;
            b.SelectedIndex = index;
            PlayNamedSound("navegar");
            AnnounceToScreenReader("Origen y destino intercambiados.");
        }

        private string CryptoCodeFromCombo2824(ComboBox combo)
        {
            if (combo == null) return "";
            string text = combo.Text;
            int open = text.LastIndexOf('(');
            int close = text.LastIndexOf(')');
            if (open >= 0 && close > open) return text.Substring(open + 1, close - open - 1).Trim().ToUpperInvariant();
            return "";
        }

        private bool IsCryptoCode2824(string code)
        {
            CryptoChoice2824[] choices = CryptoChoices2824();
            for (int i = 0; i < choices.Length; i++) if (choices[i].IsCrypto && string.Equals(choices[i].Code, code, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        private void ConvertCrypto2824(bool forceRefresh)
        {
            if (cryptoBusy2824)
            {
                PlayNamedSound("aviso");
                AnnounceToScreenReader("Ya hay una consulta de criptomonedas en curso.");
                return;
            }
            double amount;
            try { amount = ParseFlexibleNumber1545(cryptoValue2824 == null ? "" : cryptoValue2824.Text); }
            catch (Exception ex)
            {
                if (cryptoResult2824 != null) cryptoResult2824.Text = "No se pudo convertir: " + ex.Message;
                PlayNamedSound("error");
                return;
            }
            string from = CryptoCodeFromCombo2824(cryptoFrom2824);
            string to = CryptoCodeFromCombo2824(cryptoTo2824);
            if (from.Length == 0 || to.Length == 0)
            {
                PlayNamedSound("error");
                AnnounceToScreenReader("Selecciona origen y destino.");
                return;
            }
            if (!IsCryptoCode2824(from) && !IsCryptoCode2824(to))
            {
                PlayNamedSound("aviso");
                AnnounceToScreenReader("Para convertir únicamente monedas tradicionales usa la pestaña Conversiones.");
                return;
            }

            cryptoBusy2824 = true;
            if (cryptoStatus2824 != null) cryptoStatus2824.Text = forceRefresh ? "Actualizando precios…" : "Consultando precios…";
            if (cryptoResult2824 != null) cryptoResult2824.Text = "Consultando…";
            PlayNamedSound("internet");

            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    string sourceFrom;
                    string sourceTo;
                    DateTime updatedFrom;
                    DateTime updatedTo;
                    double fromEuro = AssetEuroValue2824(from, forceRefresh, out sourceFrom, out updatedFrom);
                    double toEuro = AssetEuroValue2824(to, forceRefresh, out sourceTo, out updatedTo);
                    if (fromEuro <= 0 || toEuro <= 0) throw new Exception("No se obtuvo un precio válido.");
                    double converted = amount * fromEuro / toEuro;
                    DateTime updated = updatedFrom < updatedTo ? updatedFrom : updatedTo;
                    string result = FormatNumber1545(amount) + " " + from + " equivalen aproximadamente a " + FormatCryptoNumber2824(converted) + " " + to + ".\r\n" +
                        "Tipo orientativo: 1 " + from + " = " + FormatCryptoNumber2824(fromEuro / toEuro) + " " + to + ".\r\n" +
                        "Datos: " + sourceFrom + (sourceTo == sourceFrom ? "" : " + " + sourceTo) + ". Actualización aproximada: " + updated.ToString("dd/MM/yyyy HH:mm") + ".\r\n" +
                        "Los precios de cripto pueden variar en segundos; el resultado no incluye comisiones ni constituye una oferta de compra o venta.";
                    BeginInvoke((MethodInvoker)delegate
                    {
                        cryptoBusy2824 = false;
                        cryptoResult2824.Text = result;
                        cryptoStatus2824.Text = "Conversión lista.";
                        PlayCalculatorSound1554("calc_conversion");
                        AddCalculatorHistory2824("Cripto " + from + " → " + to, cryptoValue2824.Text + ";" + from + ";" + to, result, "crypto");
                        SetStatus("Conversión de criptomonedas lista.");
                        AnnounceResults(cryptoResult2824, FormatNumber1545(amount) + " " + from + " equivalen aproximadamente a " + FormatCryptoNumber2824(converted) + " " + to + ".");
                    });
                }
                catch (Exception ex)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        cryptoBusy2824 = false;
                        cryptoResult2824.Text = "No se pudo consultar la conversión: " + DescribeNetworkException153(ex);
                        cryptoStatus2824.Text = "Consulta no disponible.";
                        PlayNamedSound("error");
                        AnnounceResults(cryptoResult2824, cryptoResult2824.Text);
                    });
                }
            });
        }

        private double AssetEuroValue2824(string code, bool forceRefresh, out string source, out DateTime updated)
        {
            if (string.Equals(code, "EUR", StringComparison.OrdinalIgnoreCase))
            {
                source = "euro como moneda base";
                updated = DateTime.Now;
                return 1.0;
            }
            if (!IsCryptoCode2824(code))
            {
                string message;
                if (!LoadCurrencyRates1545(forceRefresh, out message)) throw new Exception("No se pudo obtener el tipo de " + code + ". " + message);
                double rate;
                if (!currencyRates1545.TryGetValue(code, out rate) || rate <= 0) throw new Exception("No hay tipo de cambio para " + code + ".");
                source = currencySource1545.Length > 0 ? currencySource1545 : "tipos de cambio de Navaja";
                updated = DateTime.Now;
                return 1.0 / rate;
            }

            CryptoPrice2824 cached;
            lock (cryptoPrices2824)
            {
                if (!forceRefresh && cryptoPrices2824.TryGetValue(code, out cached) && DateTime.Now - cached.Updated < TimeSpan.FromMinutes(5))
                {
                    source = cached.Source + ", caché reciente";
                    updated = cached.Updated;
                    return cached.EuroValue;
                }
            }

            LoadCryptoDiskCache2824();
            lock (cryptoPrices2824)
            {
                if (!forceRefresh && cryptoPrices2824.TryGetValue(code, out cached) && DateTime.Now - cached.Updated < TimeSpan.FromMinutes(5))
                {
                    source = cached.Source + ", copia local reciente";
                    updated = cached.Updated;
                    return cached.EuroValue;
                }
            }

            try
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
                using (WebClient client = NewWebClient())
                {
                    NavajaWebClient timed = client as NavajaWebClient;
                    if (timed != null) timed.TimeoutMilliseconds = 15000;
                    string response = client.DownloadString("https://api.coinbase.com/v2/prices/" + Uri.EscapeDataString(code) + "-EUR/spot");
                    object rootObject = new System.Web.Script.Serialization.JavaScriptSerializer().DeserializeObject(response);
                    Dictionary<string, object> root = rootObject as Dictionary<string, object>;
                    if (root == null || !root.ContainsKey("data")) throw new Exception("Respuesta inesperada de Coinbase.");
                    Dictionary<string, object> data = root["data"] as Dictionary<string, object>;
                    if (data == null || !data.ContainsKey("amount")) throw new Exception("Coinbase no devolvió un precio para " + code + ".");
                    double value;
                    if (!double.TryParse(Convert.ToString(data["amount"], CultureInfo.InvariantCulture), NumberStyles.Float, CultureInfo.InvariantCulture, out value) || value <= 0)
                        throw new Exception("El precio recibido no es válido.");
                    CryptoPrice2824 item = new CryptoPrice2824();
                    item.EuroValue = value;
                    item.Updated = DateTime.Now;
                    item.Source = "Coinbase spot";
                    lock (cryptoPrices2824) cryptoPrices2824[code] = item;
                    SaveCryptoDiskCache2824();
                    source = item.Source;
                    updated = item.Updated;
                    return item.EuroValue;
                }
            }
            catch (Exception networkEx)
            {
                lock (cryptoPrices2824)
                {
                    if (cryptoPrices2824.TryGetValue(code, out cached) && cached.EuroValue > 0)
                    {
                        source = cached.Source + ", última copia local; Internet no respondió";
                        updated = cached.Updated;
                        return cached.EuroValue;
                    }
                }
                throw networkEx;
            }
        }

        private string CryptoCachePath2824()
        {
            return Path.Combine(dataDir, "cripto_cache.json");
        }

        private void LoadCryptoDiskCache2824()
        {
            string path = CryptoCachePath2824();
            if (!File.Exists(path)) return;
            try
            {
                object rootObject = new System.Web.Script.Serialization.JavaScriptSerializer().DeserializeObject(File.ReadAllText(path));
                Dictionary<string, object> root = rootObject as Dictionary<string, object>;
                if (root == null) return;
                lock (cryptoPrices2824)
                {
                    foreach (KeyValuePair<string, object> pair in root)
                    {
                        Dictionary<string, object> item = pair.Value as Dictionary<string, object>;
                        if (item == null) continue;
                        double value;
                        DateTime when;
                        if (!item.ContainsKey("eur") || !double.TryParse(Convert.ToString(item["eur"], CultureInfo.InvariantCulture), NumberStyles.Float, CultureInfo.InvariantCulture, out value) || value <= 0) continue;
                        if (!item.ContainsKey("updated") || !DateTime.TryParse(Convert.ToString(item["updated"]), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out when)) continue;
                        CryptoPrice2824 price = new CryptoPrice2824();
                        price.EuroValue = value;
                        price.Updated = when.ToLocalTime();
                        price.Source = "Coinbase spot";
                        cryptoPrices2824[pair.Key] = price;
                    }
                }
            }
            catch { }
        }

        private void SaveCryptoDiskCache2824()
        {
            try
            {
                Dictionary<string, object> root = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                lock (cryptoPrices2824)
                {
                    foreach (KeyValuePair<string, CryptoPrice2824> pair in cryptoPrices2824)
                    {
                        Dictionary<string, object> item = new Dictionary<string, object>();
                        item["eur"] = pair.Value.EuroValue;
                        item["updated"] = pair.Value.Updated.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
                        root[pair.Key] = item;
                    }
                }
                Directory.CreateDirectory(dataDir);
                File.WriteAllText(CryptoCachePath2824(), new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(root), new UTF8Encoding(false));
            }
            catch { }
        }

        private string FormatCryptoNumber2824(double value)
        {
            if (Math.Abs(value) >= 1000) return value.ToString("N4", SellerCulture2825);
            if (Math.Abs(value) >= 1) return value.ToString("0.########", SellerCulture2825);
            return value.ToString("0.############", SellerCulture2825);
        }

        private void ApplyDateExample2824()
        {
            if (dateOperation2824 == null || dateAdvancedInput2824 == null) return;
            string op = dateOperation2824.Text;
            if (op == "Edad exacta hasta hoy") dateAdvancedInput2824.Text = "01/01/2000";
            else if (op == "Diferencia entre dos fechas" || op.StartsWith("Días laborables")) dateAdvancedInput2824.Text = "01/01/2026;10/08/2026";
            else dateAdvancedInput2824.Text = "10/08/2026;30";
        }

        private void CalculateDate2824()
        {
            PlayCalculatorSound1554("calc_igual");
            if (dateOperation2824 == null || dateAdvancedInput2824 == null || dateAdvancedResult2824 == null) return;
            string op = dateOperation2824.Text;
            string input = dateAdvancedInput2824.Text.Trim();
            try
            {
                string[] parts = input.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                string result;
                if (op == "Edad exacta hasta hoy")
                {
                    if (parts.Length < 1) throw new Exception("Escribe la fecha de nacimiento.");
                    DateTime birth = ParseDate2824(parts[0]);
                    DateTime today = DateTime.Today;
                    if (birth > today) throw new Exception("La fecha de nacimiento está en el futuro.");
                    int years = today.Year - birth.Year;
                    DateTime anniversary = birth.AddYears(years);
                    if (anniversary > today) { years--; anniversary = birth.AddYears(years); }
                    int months = 0;
                    DateTime cursor = anniversary;
                    while (cursor.AddMonths(1) <= today) { cursor = cursor.AddMonths(1); months++; }
                    int days = (today - cursor).Days;
                    result = "Edad: " + years + " años, " + months + " meses y " + days + " días.";
                }
                else if (op == "Diferencia entre dos fechas")
                {
                    if (parts.Length < 2) throw new Exception("Escribe dos fechas separadas por punto y coma.");
                    DateTime a = ParseDate2824(parts[0]);
                    DateTime b = ParseDate2824(parts[1]);
                    int days = Convert.ToInt32(Math.Abs((b - a).TotalDays));
                    result = "Diferencia: " + days + " días, aproximadamente " + (days / 7.0).ToString("0.##", SellerCulture2825) + " semanas.";
                }
                else if (op == "Sumar días a una fecha" || op == "Restar días a una fecha")
                {
                    if (parts.Length < 2) throw new Exception("Escribe fecha y número de días separados por punto y coma.");
                    DateTime date = ParseDate2824(parts[0]);
                    int days;
                    if (!int.TryParse(parts[1].Trim(), out days)) throw new Exception("El número de días debe ser entero.");
                    DateTime final = op.StartsWith("Sumar") ? date.AddDays(days) : date.AddDays(-days);
                    result = "Fecha resultante: " + final.ToString("dddd, d 'de' MMMM 'de' yyyy", SellerCulture2825) + ".";
                }
                else
                {
                    if (parts.Length < 2) throw new Exception("Escribe dos fechas separadas por punto y coma.");
                    DateTime a = ParseDate2824(parts[0]).Date;
                    DateTime b = ParseDate2824(parts[1]).Date;
                    if (b < a) { DateTime swap = a; a = b; b = swap; }
                    int workdays = 0;
                    DateTime cursor = a;
                    while (cursor <= b)
                    {
                        if (cursor.DayOfWeek != DayOfWeek.Saturday && cursor.DayOfWeek != DayOfWeek.Sunday) workdays++;
                        cursor = cursor.AddDays(1);
                    }
                    result = "Días laborables de lunes a viernes, contando ambos extremos: " + workdays + ". No se descuentan festivos.";
                }
                dateAdvancedResult2824.Text = result;
                ScheduleCalculatorSound1554("calc_resultado", 125);
                AddCalculatorHistory2824("Fecha: " + op, input, result, "dates");
                AnnounceResults(dateAdvancedResult2824, result);
            }
            catch (Exception ex)
            {
                dateAdvancedResult2824.Text = "No se pudo calcular: " + ex.Message;
                PlayNamedSound("error");
                AnnounceResults(dateAdvancedResult2824, dateAdvancedResult2824.Text);
            }
        }

        private DateTime ParseDate2824(string text)
        {
            DateTime value;
            string[] formats = new string[] { "d/M/yyyy", "dd/MM/yyyy", "d-M-yyyy", "dd-MM-yyyy", "yyyy-MM-dd" };
            if (DateTime.TryParseExact(text.Trim(), formats, SellerCulture2825, DateTimeStyles.None, out value)) return value.Date;
            if (DateTime.TryParse(text.Trim(), SellerCulture2825, DateTimeStyles.None, out value)) return value.Date;
            throw new Exception("No se reconoce la fecha: " + text.Trim() + ".");
        }

        private void CaptureBasicHistory2824()
        {
            if (calculatorHistoryLoading2824 || calculadoraResultado == null || calculadoraInput == null) return;
            string result = calculadoraResultado.Text.Trim();
            if (!IsHistoryResultValid2824(result)) return;
            string key = calculadoraInput.Text + "\n" + result;
            if (key == lastBasicHistory2824) return;
            lastBasicHistory2824 = key;
            string kind = calcTemplate == null ? "Calculadora" : "Calculadora: " + calcTemplate.Text;
            AddCalculatorHistory2824(kind, calculadoraInput.Text, result, "basic");
        }

        private void CaptureUnitHistory2824()
        {
            if (calculatorHistoryLoading2824 || unitResult1545 == null || unitValue1545 == null) return;
            string result = unitResult1545.Text.Trim();
            if (!IsHistoryResultValid2824(result)) return;
            string input = (unitCategory1545 == null ? "" : unitCategory1545.Text) + ": " + unitValue1545.Text + " " + (unitFrom1545 == null ? "" : unitFrom1545.Text) + " → " + (unitTo1545 == null ? "" : unitTo1545.Text);
            string key = input + "\n" + result;
            if (key == lastUnitHistory2824) return;
            lastUnitHistory2824 = key;
            AddCalculatorHistory2824("Conversión de unidades", input, result, "units");
        }

        private void CaptureCurrencyHistory2824()
        {
            if (calculatorHistoryLoading2824 || currencyResult1545 == null || currencyValue1545 == null) return;
            string result = currencyResult1545.Text.Trim();
            if (!IsHistoryResultValid2824(result)) return;
            string input = currencyValue1545.Text + " " + SelectedCurrencyCode1545(currencyFrom1545) + " → " + SelectedCurrencyCode1545(currencyTo1545);
            string key = input + "\n" + result;
            if (key == lastCurrencyHistory2824) return;
            lastCurrencyHistory2824 = key;
            AddCalculatorHistory2824("Conversión de monedas", input, result, "currency");
        }

        private bool IsHistoryResultValid2824(string result)
        {
            if (string.IsNullOrWhiteSpace(result)) return false;
            string lower = result.ToLowerInvariant();
            if (lower.StartsWith("consultando") || lower.StartsWith("no se pudo") || lower.StartsWith("selecciona") || lower.StartsWith("formato no válido")) return false;
            return true;
        }

        private void AddCalculatorHistory2824(string kind, string input, string result, string target)
        {
            if (calculatorHistoryLoading2824 || !IsHistoryResultValid2824(result)) return;
            CalculatorHistoryItem2824 item = new CalculatorHistoryItem2824();
            item.When = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            item.Kind = kind ?? "Operación";
            item.Input = input ?? "";
            item.Result = result ?? "";
            item.ReuseTarget = target ?? "";
            calculatorHistory2824.Insert(0, item);
            while (calculatorHistory2824.Count > 300) calculatorHistory2824.RemoveAt(calculatorHistory2824.Count - 1);
            ScheduleCalculatorHistorySave285();
            RefreshHistoryList2824(0);
            PlayCalculatorSound1554("calc_historial");
        }

        private void LoadCalculatorHistory2824()
        {
            calculatorHistoryLoading2824 = true;
            try
            {
                calculatorHistory2824.Clear();
                if (!File.Exists(calculatorHistoryPath2824)) return;
                object raw = LoadCalculatorJsonObject285(calculatorHistoryPath2824, "el historial de la calculadora");
                IList list = raw as IList;
                if (list == null) return;
                foreach (object entry in list)
                {
                    Dictionary<string, object> item = entry as Dictionary<string, object>;
                    if (item == null) continue;
                    CalculatorHistoryItem2824 h = new CalculatorHistoryItem2824();
                    object value;
                    if (item.TryGetValue("fecha", out value)) h.When = Convert.ToString(value);
                    if (item.TryGetValue("tipo", out value)) h.Kind = Convert.ToString(value);
                    if (item.TryGetValue("entrada", out value)) h.Input = Convert.ToString(value);
                    if (item.TryGetValue("resultado", out value)) h.Result = Convert.ToString(value);
                    if (item.TryGetValue("destino", out value)) h.ReuseTarget = Convert.ToString(value);
                    calculatorHistory2824.Add(h);
                    if (calculatorHistory2824.Count >= 300) break;
                }
            }
            catch { }
            finally { calculatorHistoryLoading2824 = false; }
        }

        private void SaveCalculatorHistory2824()
        {
            try
            {
                ArrayList list = new ArrayList();
                foreach (CalculatorHistoryItem2824 h in calculatorHistory2824)
                {
                    Dictionary<string, object> item = new Dictionary<string, object>();
                    item["fecha"] = h.When;
                    item["tipo"] = h.Kind;
                    item["entrada"] = h.Input;
                    item["resultado"] = h.Result;
                    item["destino"] = h.ReuseTarget;
                    list.Add(item);
                }
                Directory.CreateDirectory(dataDir);
                File.WriteAllText(calculatorHistoryPath2824, new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(list), new UTF8Encoding(false));
            }
            catch { }
        }

        private void RefreshHistoryList2824(int selected)
        {
            if (calculatorHistoryList2824 == null) return;
            calculatorHistoryLoading2824 = true;
            try
            {
                calculatorHistoryList2824.Items.Clear();
                foreach (CalculatorHistoryItem2824 item in calculatorHistory2824) calculatorHistoryList2824.Items.Add(item);
                if (calculatorHistoryList2824.Items.Count > 0)
                {
                    if (selected < 0 || selected >= calculatorHistoryList2824.Items.Count) selected = 0;
                    calculatorHistoryList2824.SelectedIndex = selected;
                }
                else if (calculatorHistoryDetail2824 != null) calculatorHistoryDetail2824.Text = "El historial está vacío.";
            }
            finally { calculatorHistoryLoading2824 = false; }
            RefreshHistoryDetail2824();
        }

        private CalculatorHistoryItem2824 SelectedHistory2824()
        {
            if (calculatorHistoryList2824 == null) return null;
            return calculatorHistoryList2824.SelectedItem as CalculatorHistoryItem2824;
        }

        private void RefreshHistoryDetail2824()
        {
            if (calculatorHistoryDetail2824 == null) return;
            CalculatorHistoryItem2824 item = SelectedHistory2824();
            if (item == null) { calculatorHistoryDetail2824.Text = "El historial está vacío."; return; }
            calculatorHistoryDetail2824.Text = "Fecha: " + item.When + "\r\nTipo: " + item.Kind + "\r\nEntrada: " + item.Input + "\r\nResultado: " + item.Result;
        }

        private void CopySelectedHistoryResult2824()
        {
            CalculatorHistoryItem2824 item = SelectedHistory2824();
            if (item == null) { PlayNamedSound("aviso"); AnnounceToScreenReader("No hay una operación seleccionada."); return; }
            try
            {
                Clipboard.SetText(item.Result ?? "");
                PlayCalculatorSound1554("calc_copiar");
                AnnounceToScreenReader("Resultado del historial copiado.");
            }
            catch { PlayNamedSound("error"); }
        }

        private void CopySelectedHistoryFull2824()
        {
            CalculatorHistoryItem2824 item = SelectedHistory2824();
            if (item == null) { PlayNamedSound("aviso"); return; }
            try
            {
                Clipboard.SetText("Fecha: " + item.When + Environment.NewLine + "Tipo: " + item.Kind + Environment.NewLine + "Entrada: " + item.Input + Environment.NewLine + "Resultado: " + item.Result);
                PlayCalculatorSound1554("calc_copiar");
                AnnounceToScreenReader("Operación completa copiada.");
            }
            catch { PlayNamedSound("error"); }
        }

        private void ReuseSelectedHistory2824()
        {
            CalculatorHistoryItem2824 item = SelectedHistory2824();
            if (item == null) { PlayNamedSound("aviso"); return; }
            if (item.ReuseTarget == "advanced" && advancedInput2824 != null)
            {
                advancedInput2824.Text = item.Input;
                calculatorTabs2824.SelectedTab = calculatorAdvancedPage2824;
                advancedInput2824.Focus();
                advancedInput2824.SelectAll();
            }
            else if (item.ReuseTarget == "dates" && dateAdvancedInput2824 != null)
            {
                dateAdvancedInput2824.Text = item.Input;
                calculatorTabs2824.SelectedTab = calculatorDatesPage2824;
                dateAdvancedInput2824.Focus();
                dateAdvancedInput2824.SelectAll();
            }
            else if (item.ReuseTarget == "crypto" && cryptoValue2824 != null)
            {
                string[] parts = (item.Input ?? "").Split(';');
                cryptoValue2824.Text = parts.Length > 0 ? parts[0] : item.Input;
                if (parts.Length > 1) SelectAssetCode2824(cryptoFrom2824, parts[1]);
                if (parts.Length > 2) SelectAssetCode2824(cryptoTo2824, parts[2]);
                calculatorTabs2824.SelectedTab = calculatorCryptoPage2824;
                cryptoValue2824.Focus();
                cryptoValue2824.SelectAll();
            }
            else if (item.ReuseTarget == "units" && unitValue1545 != null)
            {
                unitValue1545.Text = ExtractLeadingValue2824(item.Input);
                calculatorTabs2824.SelectedTab = calculatorConvertPage2824;
                unitValue1545.Focus();
                unitValue1545.SelectAll();
            }
            else if (item.ReuseTarget == "currency" && currencyValue1545 != null)
            {
                currencyValue1545.Text = ExtractLeadingValue2824(item.Input);
                calculatorTabs2824.SelectedTab = calculatorConvertPage2824;
                currencyValue1545.Focus();
                currencyValue1545.SelectAll();
            }
            else if (item.ReuseTarget == "dice" && dadosInput != null)
            {
                dadosInput.Text = item.Input;
                calculatorTabs2824.SelectedTab = calculatorDatesPage2824;
                LoadDiceControlsFromNotation1532();
                dadosInput.Focus();
                dadosInput.SelectAll();
            }
            else if (item.ReuseTarget == "simpledate" && fechaInput != null)
            {
                fechaInput.Text = item.Input;
                calculatorTabs2824.SelectedTab = calculatorDatesPage2824;
                fechaInput.Focus();
                fechaInput.SelectAll();
            }
            else if (item.ReuseTarget == "basic" && calculadoraInput != null)
            {
                calculadoraInput.Text = item.Input;
                calculatorTabs2824.SelectedTab = calculatorBasicPage2824;
                calculadoraInput.Focus();
                calculadoraInput.SelectAll();
            }
            else
            {
                PlayNamedSound("aviso");
                AnnounceToScreenReader("Esta operación se puede consultar y copiar, pero sus controles no se pueden restaurar automáticamente.");
                return;
            }
            PlayCalculatorSound1554("calc_memoria");
            AnnounceToScreenReader("Valores recuperados del historial.");
        }

        private void SelectAssetCode2824(ComboBox combo, string code)
        {
            if (combo == null || string.IsNullOrWhiteSpace(code)) return;
            string suffix = "(" + code.Trim().ToUpperInvariant() + ")";
            for (int i = 0; i < combo.Items.Count; i++)
            {
                if (Convert.ToString(combo.Items[i]).EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    combo.SelectedIndex = i;
                    return;
                }
            }
        }

        private string ExtractLeadingValue2824(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "";
            System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(input, @"[-+]?\d+(?:[\.,]\d+)?");
            return match.Success ? match.Value : input;
        }

        private void DeleteSelectedHistory2824()
        {
            if (calculatorHistoryList2824 == null || calculatorHistoryList2824.SelectedIndex < 0) { PlayNamedSound("aviso"); return; }
            int index = calculatorHistoryList2824.SelectedIndex;
            calculatorHistory2824.RemoveAt(index);
            ScheduleCalculatorHistorySave285();
            PlayCalculatorSound1554("calc_eliminar_resultado");
            if (index >= calculatorHistory2824.Count) index = calculatorHistory2824.Count - 1;
            RefreshHistoryList2824(index);
            AnnounceToScreenReader("Operación eliminada del historial.");
        }

        private void ClearHistory2824()
        {
            if (calculatorHistory2824.Count == 0) { PlayNamedSound("aviso"); AnnounceToScreenReader("El historial ya está vacío."); return; }
            DialogResult answer = MessageBox.Show("¿Vaciar todo el historial de la calculadora? Esta acción no se puede deshacer.", "Vaciar historial", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (answer != DialogResult.Yes) return;
            calculatorHistory2824.Clear();
            ScheduleCalculatorHistorySave285();
            PlayCalculatorSound1554("calc_historial_limpiar");
            RefreshHistoryList2824(-1);
            AnnounceToScreenReader("Historial de la calculadora vacío.");
        }

        private void SelectCalculatorHistory2824()
        {
            if (calculatorTabs2824 != null && calculatorHistoryPage2824 != null) calculatorTabs2824.SelectedTab = calculatorHistoryPage2824;
            if (calculatorHistoryList2824 != null) calculatorHistoryList2824.Focus();
        }
    }
}

