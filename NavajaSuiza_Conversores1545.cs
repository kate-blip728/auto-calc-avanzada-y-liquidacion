using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace TodoAMano
{
    public partial class MainForm
    {
        private ComboBox unitCategory1545;
        private TextBox unitValue1545;
        private ComboBox unitFrom1545;
        private ComboBox unitTo1545;
        private TextBox unitResult1545;

        private TextBox currencyValue1545;
        private ComboBox currencyFrom1545;
        private ComboBox currencyTo1545;
        private TextBox currencyResult1545;
        private Label currencyStatus1545;
        private bool currencyBusy1545;
        private readonly Dictionary<string, double> currencyRates1545 = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        private string currencyUpdated1545 = "";
        private string currencySource1545 = "";

        private sealed class CurrencyChoice1545
        {
            public string Code;
            public string Name;
            public CurrencyChoice1545(string code, string name) { Code = code; Name = name; }
            public override string ToString() { return Name + " (" + Code + ")"; }
        }

        private void AddConverters1545(TableLayoutPanel layout)
        {
            Label unitHeading = new Label();
            unitHeading.AutoSize = true;
            unitHeading.Text = "CONVERSOR DE UNIDADES";
            unitHeading.AccessibleName = "Conversor de unidades";
            AddFullRow(layout, unitHeading, false);

            unitCategory1545 = AddLabeledCombo(layout, "Categoría de unidades:", new string[]
            {
                "Longitud", "Masa", "Temperatura", "Volumen", "Área", "Velocidad", "Tiempo", "Datos digitales", "Presión", "Ángulo", "Energía", "Potencia", "Frecuencia", "Fuerza", "Densidad"
            });
            unitCategory1545.SelectedIndexChanged += delegate { RefreshUnitChoices1545(); };
            unitValue1545 = AddLabeledText(layout, "Cantidad que se va a convertir:", false);
            unitValue1545.Text = "1";
            AttachCalculatorTextBox1554(unitValue1545, delegate { RunUnitConversion1554(); });
            unitFrom1545 = AddLabeledCombo(layout, "Unidad de origen:", new string[0]);
            unitTo1545 = AddLabeledCombo(layout, "Unidad de destino:", new string[0]);
            RefreshUnitChoices1545();

            FlowLayoutPanel unitButtons = ButtonRow();
            unitButtons.Controls.Add(NewButton("Convertir unidades", delegate { RunUnitConversion1554(); }));
            unitButtons.Controls.Add(NewButton("Intercambiar unidades", delegate { SwapUnitChoices1545(); }));
            unitButtons.Controls.Add(NewButton("Copiar resultado de unidades", delegate { CopyConverterResult1545(unitResult1545); }));
            AddFullRow(layout, unitButtons, false);
            unitResult1545 = AddLabeledText(layout, "Resultado de la conversión de unidades:", true);
            unitResult1545.ReadOnly = true;
            unitResult1545.Height = 72;

            Label currencyHeading = new Label();
            currencyHeading.AutoSize = true;
            currencyHeading.Text = "CONVERSOR DE MONEDAS";
            currencyHeading.AccessibleName = "Conversor de monedas";
            AddFullRow(layout, currencyHeading, false);

            currencyValue1545 = AddLabeledText(layout, "Cantidad de dinero:", false);
            currencyValue1545.Text = "100";
            AttachCalculatorTextBox1554(currencyValue1545, delegate { RunCurrencyConversion1554(false); });
            CurrencyChoice1545[] choices = CurrencyChoices1545();
            string[] labels = new string[choices.Length];
            for (int i = 0; i < choices.Length; i++) labels[i] = choices[i].ToString();
            currencyFrom1545 = AddLabeledCombo(layout, "Moneda de origen:", labels);
            currencyTo1545 = AddLabeledCombo(layout, "Moneda de destino:", labels);
            SetCurrencyChoice1545(currencyFrom1545, "EUR");
            SetCurrencyChoice1545(currencyTo1545, "USD");

            FlowLayoutPanel currencyButtons = ButtonRow();
            currencyButtons.Controls.Add(NewButton("Convertir monedas", delegate { RunCurrencyConversion1554(false); }));
            currencyButtons.Controls.Add(NewButton("Actualizar tipos y convertir", delegate { RunCurrencyConversion1554(true); }));
            currencyButtons.Controls.Add(NewButton("Intercambiar monedas", delegate { SwapCurrencyChoices1545(); }));
            currencyButtons.Controls.Add(NewButton("Copiar resultado de monedas", delegate { CopyConverterResult1545(currencyResult1545); }));
            AddFullRow(layout, currencyButtons, false);

            currencyStatus1545 = new Label();
            currencyStatus1545.AutoSize = true;
            currencyStatus1545.Text = "Los tipos se consultan al convertir y se guarda una copia local para emergencias sin conexión.";
            currencyStatus1545.AccessibleName = "Estado de los tipos de cambio";
            AddFullRow(layout, currencyStatus1545, false);

            currencyResult1545 = AddLabeledText(layout, "Resultado de la conversión de monedas:", true);
            currencyResult1545.ReadOnly = true;
            currencyResult1545.Height = 110;
        }

        private void RefreshUnitChoices1545()
        {
            if (unitCategory1545 == null || unitFrom1545 == null || unitTo1545 == null) return;
            string[] units = UnitsForCategory1545(unitCategory1545.Text);
            unitFrom1545.Items.Clear();
            unitTo1545.Items.Clear();
            unitFrom1545.Items.AddRange(units);
            unitTo1545.Items.AddRange(units);
            if (units.Length > 0)
            {
                unitFrom1545.SelectedIndex = 0;
                unitTo1545.SelectedIndex = units.Length > 1 ? 1 : 0;
            }
        }

        private string[] UnitsForCategory1545(string category)
        {
            if (category == "Longitud") return new string[] { "Micrómetros (µm)", "Milímetros (mm)", "Centímetros (cm)", "Decímetros (dm)", "Metros (m)", "Kilómetros (km)", "Pulgadas (in)", "Pies (ft)", "Yardas (yd)", "Millas (mi)", "Millas náuticas (nmi)" };
            if (category == "Masa") return new string[] { "Miligramos (mg)", "Gramos (g)", "Kilogramos (kg)", "Toneladas métricas (t)", "Onzas (oz)", "Libras (lb)", "Stones (st)" };
            if (category == "Temperatura") return new string[] { "Grados Celsius (°C)", "Grados Fahrenheit (°F)", "Kelvin (K)" };
            if (category == "Volumen") return new string[] { "Mililitros (ml)", "Centilitros (cl)", "Litros (l)", "Metros cúbicos (m³)", "Cucharaditas métricas", "Cucharadas métricas", "Tazas métricas", "Pintas estadounidenses", "Galones estadounidenses", "Pintas imperiales", "Galones imperiales" };
            if (category == "Área") return new string[] { "Milímetros cuadrados (mm²)", "Centímetros cuadrados (cm²)", "Metros cuadrados (m²)", "Kilómetros cuadrados (km²)", "Hectáreas (ha)", "Pies cuadrados (ft²)", "Yardas cuadradas (yd²)", "Acres" };
            if (category == "Velocidad") return new string[] { "Metros por segundo (m/s)", "Kilómetros por hora (km/h)", "Millas por hora (mph)", "Nudos (kn)" };
            if (category == "Tiempo") return new string[] { "Milisegundos", "Segundos", "Minutos", "Horas", "Días", "Semanas" };
            if (category == "Datos digitales") return new string[] { "Bits", "Bytes", "Kilobytes decimales (KB)", "Kibibytes (KiB)", "Megabytes decimales (MB)", "Mebibytes (MiB)", "Gigabytes decimales (GB)", "Gibibytes (GiB)", "Terabytes decimales (TB)", "Tebibytes (TiB)" };
            if (category == "Presión") return new string[] { "Pascales (Pa)", "Kilopascales (kPa)", "Bares (bar)", "Atmósferas (atm)", "Libras por pulgada cuadrada (psi)", "Milímetros de mercurio (mmHg)" };
            if (category == "Ángulo") return new string[] { "Grados (°)", "Radianes (rad)", "Gradianes (gon)", "Vueltas completas" };
            if (category == "Energía") return new string[] { "Julios (J)", "Kilojulios (kJ)", "Calorías (cal)", "Kilocalorías (kcal)", "Vatios-hora (Wh)", "Kilovatios-hora (kWh)" };
            if (category == "Potencia") return new string[] { "Vatios (W)", "Kilovatios (kW)", "Megavatios (MW)", "Caballos de vapor métricos (CV)", "Horsepower mecánico (hp)" };
            if (category == "Frecuencia") return new string[] { "Hercios (Hz)", "Kilohercios (kHz)", "Megahercios (MHz)", "Gigahercios (GHz)" };
            if (category == "Fuerza") return new string[] { "Newtons (N)", "Kilonewtons (kN)", "Kilogramos-fuerza (kgf)", "Libras-fuerza (lbf)" };
            if (category == "Densidad") return new string[] { "Kilogramos por metro cúbico (kg/m³)", "Gramos por centímetro cúbico (g/cm³)", "Gramos por litro (g/l)", "Libras por pie cúbico (lb/ft³)" };
            return new string[0];
        }

        private void ConvertUnits1545()
        {
            try
            {
                double value = ParseFlexibleNumber1545(unitValue1545 == null ? "" : unitValue1545.Text);
                string category = unitCategory1545 == null ? "" : unitCategory1545.Text;
                string from = unitFrom1545 == null ? "" : unitFrom1545.Text;
                string to = unitTo1545 == null ? "" : unitTo1545.Text;
                if (from.Length == 0 || to.Length == 0) throw new Exception("Selecciona las dos unidades.");
                double result;
                if (category == "Temperatura") result = ConvertTemperature1545(value, from, to);
                else result = value * UnitFactor1545(category, from) / UnitFactor1545(category, to);
                string message = FormatNumber1545(value) + " " + UnitShortName1545(from) + " equivalen a " + FormatNumber1545(result) + " " + UnitShortName1545(to) + ".";
                unitResult1545.Text = message;
                PlayCalculatorSound1554("calc_conversion");
                SetStatus("Conversión de unidades lista.");
                AnnounceResults(unitResult1545, message);
            }
            catch (Exception ex)
            {
                unitResult1545.Text = "No se pudo convertir: " + ex.Message;
                PlayNamedSound("error");
                SetStatus("Error en el conversor de unidades.");
                AnnounceResults(unitResult1545, unitResult1545.Text);
            }
        }

        private double UnitFactor1545(string category, string unit)
        {
            if (category == "Longitud")
            {
                if (unit.StartsWith("Micrómetros")) return 0.000001;
                if (unit.StartsWith("Milímetros")) return 0.001;
                if (unit.StartsWith("Centímetros")) return 0.01;
                if (unit.StartsWith("Decímetros")) return 0.1;
                if (unit.StartsWith("Metros (")) return 1;
                if (unit.StartsWith("Kilómetros")) return 1000;
                if (unit.StartsWith("Pulgadas")) return 0.0254;
                if (unit.StartsWith("Pies")) return 0.3048;
                if (unit.StartsWith("Yardas")) return 0.9144;
                if (unit.StartsWith("Millas (")) return 1609.344;
                if (unit.StartsWith("Millas náuticas")) return 1852;
            }
            if (category == "Masa")
            {
                if (unit.StartsWith("Miligramos")) return 0.000001;
                if (unit.StartsWith("Gramos")) return 0.001;
                if (unit.StartsWith("Kilogramos")) return 1;
                if (unit.StartsWith("Toneladas")) return 1000;
                if (unit.StartsWith("Onzas")) return 0.028349523125;
                if (unit.StartsWith("Libras")) return 0.45359237;
                if (unit.StartsWith("Stones")) return 6.35029318;
            }
            if (category == "Volumen")
            {
                if (unit.StartsWith("Mililitros")) return 0.001;
                if (unit.StartsWith("Centilitros")) return 0.01;
                if (unit.StartsWith("Litros")) return 1;
                if (unit.StartsWith("Metros cúbicos")) return 1000;
                if (unit.StartsWith("Cucharaditas")) return 0.005;
                if (unit.StartsWith("Cucharadas")) return 0.015;
                if (unit.StartsWith("Tazas")) return 0.25;
                if (unit.StartsWith("Pintas estadounidenses")) return 0.473176473;
                if (unit.StartsWith("Galones estadounidenses")) return 3.785411784;
                if (unit.StartsWith("Pintas imperiales")) return 0.56826125;
                if (unit.StartsWith("Galones imperiales")) return 4.54609;
            }
            if (category == "Área")
            {
                if (unit.StartsWith("Milímetros cuadrados")) return 0.000001;
                if (unit.StartsWith("Centímetros cuadrados")) return 0.0001;
                if (unit.StartsWith("Metros cuadrados")) return 1;
                if (unit.StartsWith("Kilómetros cuadrados")) return 1000000;
                if (unit.StartsWith("Hectáreas")) return 10000;
                if (unit.StartsWith("Pies cuadrados")) return 0.09290304;
                if (unit.StartsWith("Yardas cuadradas")) return 0.83612736;
                if (unit.StartsWith("Acres")) return 4046.8564224;
            }
            if (category == "Velocidad")
            {
                if (unit.StartsWith("Metros por segundo")) return 1;
                if (unit.StartsWith("Kilómetros por hora")) return 0.277777777777778;
                if (unit.StartsWith("Millas por hora")) return 0.44704;
                if (unit.StartsWith("Nudos")) return 0.514444444444444;
            }
            if (category == "Tiempo")
            {
                if (unit == "Milisegundos") return 0.001;
                if (unit == "Segundos") return 1;
                if (unit == "Minutos") return 60;
                if (unit == "Horas") return 3600;
                if (unit == "Días") return 86400;
                if (unit == "Semanas") return 604800;
            }
            if (category == "Datos digitales")
            {
                if (unit == "Bits") return 0.125;
                if (unit == "Bytes") return 1;
                if (unit.StartsWith("Kilobytes")) return 1000;
                if (unit.StartsWith("Kibibytes")) return 1024;
                if (unit.StartsWith("Megabytes")) return 1000000;
                if (unit.StartsWith("Mebibytes")) return 1048576;
                if (unit.StartsWith("Gigabytes")) return 1000000000;
                if (unit.StartsWith("Gibibytes")) return 1073741824;
                if (unit.StartsWith("Terabytes")) return 1000000000000;
                if (unit.StartsWith("Tebibytes")) return 1099511627776;
            }
            if (category == "Presión")
            {
                if (unit.StartsWith("Pascales")) return 1;
                if (unit.StartsWith("Kilopascales")) return 1000;
                if (unit.StartsWith("Bares")) return 100000;
                if (unit.StartsWith("Atmósferas")) return 101325;
                if (unit.StartsWith("Libras por")) return 6894.757293168;
                if (unit.StartsWith("Milímetros de")) return 133.322387415;
            }
            if (category == "Ángulo")
            {
                if (unit.StartsWith("Grados")) return Math.PI / 180.0;
                if (unit.StartsWith("Radianes")) return 1;
                if (unit.StartsWith("Gradianes")) return Math.PI / 200.0;
                if (unit.StartsWith("Vueltas")) return 2.0 * Math.PI;
            }
            if (category == "Energía")
            {
                if (unit.StartsWith("Julios")) return 1;
                if (unit.StartsWith("Kilojulios")) return 1000;
                if (unit.StartsWith("Calorías")) return 4.184;
                if (unit.StartsWith("Kilocalorías")) return 4184;
                if (unit.StartsWith("Vatios-hora")) return 3600;
                if (unit.StartsWith("Kilovatios-hora")) return 3600000;
            }
            if (category == "Potencia")
            {
                if (unit.StartsWith("Vatios (")) return 1;
                if (unit.StartsWith("Kilovatios")) return 1000;
                if (unit.StartsWith("Megavatios")) return 1000000;
                if (unit.StartsWith("Caballos")) return 735.49875;
                if (unit.StartsWith("Horsepower")) return 745.6998715822702;
            }
            if (category == "Frecuencia")
            {
                if (unit.StartsWith("Hercios")) return 1;
                if (unit.StartsWith("Kilohercios")) return 1000;
                if (unit.StartsWith("Megahercios")) return 1000000;
                if (unit.StartsWith("Gigahercios")) return 1000000000;
            }
            if (category == "Fuerza")
            {
                if (unit.StartsWith("Newtons")) return 1;
                if (unit.StartsWith("Kilonewtons")) return 1000;
                if (unit.StartsWith("Kilogramos-fuerza")) return 9.80665;
                if (unit.StartsWith("Libras-fuerza")) return 4.4482216152605;
            }
            if (category == "Densidad")
            {
                if (unit.StartsWith("Kilogramos por")) return 1;
                if (unit.StartsWith("Gramos por centímetro")) return 1000;
                if (unit.StartsWith("Gramos por litro")) return 1;
                if (unit.StartsWith("Libras por pie")) return 16.01846337396;
            }
            throw new Exception("No se reconoce la unidad seleccionada.");
        }

        private double ConvertTemperature1545(double value, string from, string to)
        {
            double celsius;
            if (from.StartsWith("Grados Celsius")) celsius = value;
            else if (from.StartsWith("Grados Fahrenheit")) celsius = (value - 32.0) * 5.0 / 9.0;
            else if (from.StartsWith("Kelvin")) celsius = value - 273.15;
            else throw new Exception("No se reconoce la temperatura de origen.");
            if (celsius < -273.15) throw new Exception("La temperatura está por debajo del cero absoluto.");

            if (to.StartsWith("Grados Celsius")) return celsius;
            if (to.StartsWith("Grados Fahrenheit")) return celsius * 9.0 / 5.0 + 32.0;
            if (to.StartsWith("Kelvin")) return celsius + 273.15;
            throw new Exception("No se reconoce la temperatura de destino.");
        }

        private void SwapUnitChoices1545()
        {
            if (unitFrom1545 == null || unitTo1545 == null) return;
            int old = unitFrom1545.SelectedIndex;
            unitFrom1545.SelectedIndex = unitTo1545.SelectedIndex;
            unitTo1545.SelectedIndex = old;
            PlayNamedSound("navegar");
            SetStatus("Unidades intercambiadas.");
        }

        private void ConvertCurrencies1545(bool forceRefresh)
        {
            if (currencyBusy1545)
            {
                PlayNamedSound("aviso");
                AnnounceToScreenReader("La consulta de tipos de cambio ya está en curso.");
                return;
            }

            double amount;
            try { amount = ParseFlexibleNumber1545(currencyValue1545 == null ? "" : currencyValue1545.Text); }
            catch (Exception ex)
            {
                currencyResult1545.Text = "No se pudo convertir: " + ex.Message;
                PlayNamedSound("error");
                AnnounceResults(currencyResult1545, currencyResult1545.Text);
                return;
            }

            string fromCode = SelectedCurrencyCode1545(currencyFrom1545);
            string toCode = SelectedCurrencyCode1545(currencyTo1545);
            if (fromCode.Length == 0 || toCode.Length == 0)
            {
                currencyResult1545.Text = "Selecciona las monedas de origen y destino.";
                PlayNamedSound("error");
                AnnounceResults(currencyResult1545, currencyResult1545.Text);
                return;
            }

            currencyBusy1545 = true;
            currencyStatus1545.Text = forceRefresh ? "Actualizando tipos de cambio y convirtiendo..." : "Consultando tipos de cambio...";
            currencyResult1545.Text = "Consultando...";
            PlayNamedSound("internet");
            SetStatus("Consultando tipos de cambio.");

            ThreadPool.QueueUserWorkItem(delegate
            {
                string loadMessage;
                bool loaded = LoadCurrencyRates1545(forceRefresh, out loadMessage);
                BeginInvoke((MethodInvoker)delegate
                {
                    currencyBusy1545 = false;
                    if (!loaded)
                    {
                        currencyResult1545.Text = "No se pudieron obtener tipos de cambio. " + loadMessage;
                        currencyStatus1545.Text = "Sin tipos disponibles.";
                        PlayNamedSound("error");
                        SetStatus("No se pudieron consultar los tipos de cambio.");
                        AnnounceResults(currencyResult1545, currencyResult1545.Text);
                        return;
                    }

                    double fromRate;
                    double toRate;
                    if (!currencyRates1545.TryGetValue(fromCode, out fromRate) || !currencyRates1545.TryGetValue(toCode, out toRate) || fromRate <= 0 || toRate <= 0)
                    {
                        currencyResult1545.Text = "La fuente consultada no ofrece uno de los códigos seleccionados: " + fromCode + " o " + toCode + ".";
                        currencyStatus1545.Text = "Tipos cargados, pero falta una moneda.";
                        PlayNamedSound("aviso");
                        AnnounceResults(currencyResult1545, currencyResult1545.Text);
                        return;
                    }

                    double crossRate = toRate / fromRate;
                    double converted = amount * crossRate;
                    string fromName = CurrencyNameForCode1545(fromCode);
                    string toName = CurrencyNameForCode1545(toCode);
                    string result = FormatNumber1545(amount) + " " + fromName + " (" + fromCode + ") equivalen aproximadamente a " + FormatMoney1545(converted) + " " + toName + " (" + toCode + ").\r\n" +
                        "Tipo aplicado: 1 " + fromCode + " = " + FormatRate1545(crossRate) + " " + toCode + ".\r\n" +
                        "Actualización: " + (currencyUpdated1545.Length > 0 ? currencyUpdated1545 : "no indicada") + ".\r\n" +
                        "Fuente: " + currencySource1545 + ". Valor orientativo; bancos, tarjetas, PayPal y casas de cambio pueden aplicar otro tipo y comisiones.";
                    currencyResult1545.Text = result;
                    currencyStatus1545.Text = "Tipos disponibles. " + loadMessage;
                    PlayCalculatorSound1554("calc_conversion");
                    SetStatus("Resultado monetario listo.");
                    AnnounceResults(currencyResult1545, FormatNumber1545(amount) + " " + fromCode + " equivalen aproximadamente a " + FormatMoney1545(converted) + " " + toCode + ".");
                });
            });
        }

        private bool LoadCurrencyRates1545(bool forceRefresh, out string message)
        {
            string cachePath = Path.Combine(dataDir, "tipos_cambio_cache.json");
            if (!forceRefresh && File.Exists(cachePath))
            {
                try
                {
                    TimeSpan age = DateTime.Now - File.GetLastWriteTime(cachePath);
                    if (age.TotalHours < 12 && ParseCurrencyJson1545(File.ReadAllText(cachePath), "ExchangeRate-API Open Access, copia local reciente"))
                    {
                        message = "Se utilizó la copia local reciente.";
                        return true;
                    }
                }
                catch { }
            }

            try
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
                using (WebClient client = NewWebClient())
                {
                    NavajaWebClient timed = client as NavajaWebClient;
                    if (timed != null) timed.TimeoutMilliseconds = 15000;
                    string response = client.DownloadString("https://open.er-api.com/v6/latest/EUR");
                    if (!ParseCurrencyJson1545(response, "ExchangeRate-API Open Access")) throw new Exception("La respuesta no contenía tipos de cambio válidos.");
                    try { File.WriteAllText(cachePath, response, System.Text.Encoding.UTF8); } catch { }
                    message = "Tipos actualizados por Internet.";
                    return true;
                }
            }
            catch (Exception ex)
            {
                try
                {
                    using (WebClient client = NewWebClient())
                    {
                        NavajaWebClient timed = client as NavajaWebClient;
                        if (timed != null) timed.TimeoutMilliseconds = 15000;
                        string ecbXml = client.DownloadString("https://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml");
                        if (ParseEcbXml1545(ecbXml))
                        {
                            message = "La fuente amplia no respondió; se utilizaron los tipos oficiales del Banco Central Europeo para las monedas disponibles.";
                            return true;
                        }
                    }
                }
                catch { }
                try
                {
                    if (File.Exists(cachePath) && ParseCurrencyJson1545(File.ReadAllText(cachePath), "ExchangeRate-API Open Access, copia local de la última consulta"))
                    {
                        message = "Internet no respondió; se utilizó la última copia local. " + DescribeNetworkException153(ex);
                        return true;
                    }
                }
                catch { }
                message = DescribeNetworkException153(ex);
                return false;
            }
        }

        private bool ParseEcbXml1545(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;
            XmlDocument document = new XmlDocument();
            document.XmlResolver = null;
            document.LoadXml(text);
            XmlNodeList nodes = document.SelectNodes("//*[local-name()='Cube' and @currency and @rate]");
            if (nodes == null || nodes.Count == 0) return false;
            Dictionary<string, double> parsed = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            parsed["EUR"] = 1.0;
            foreach (XmlNode node in nodes)
            {
                if (node.Attributes == null) continue;
                XmlAttribute currency = node.Attributes["currency"];
                XmlAttribute rateText = node.Attributes["rate"];
                double rate;
                if (currency != null && rateText != null && double.TryParse(rateText.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out rate) && rate > 0)
                    parsed[currency.Value] = rate;
            }
            if (parsed.Count < 2) return false;
            lock (currencyRates1545)
            {
                currencyRates1545.Clear();
                foreach (KeyValuePair<string, double> pair in parsed) currencyRates1545[pair.Key] = pair.Value;
            }
            XmlNode dateNode = document.SelectSingleNode("//*[local-name()='Cube' and @time]");
            currencyUpdated1545 = dateNode != null && dateNode.Attributes != null && dateNode.Attributes["time"] != null ? dateNode.Attributes["time"].Value : "";
            currencySource1545 = "Banco Central Europeo";
            return true;
        }

        private bool ParseCurrencyJson1545(string text, string source)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;
            object rootObject = new System.Web.Script.Serialization.JavaScriptSerializer().DeserializeObject(text);
            Dictionary<string, object> root = rootObject as Dictionary<string, object>;
            if (root == null) return false;
            object resultObject;
            if (root.TryGetValue("result", out resultObject) && !string.Equals(Convert.ToString(resultObject), "success", StringComparison.OrdinalIgnoreCase)) return false;
            object ratesObject;
            if (!root.TryGetValue("rates", out ratesObject)) return false;
            Dictionary<string, object> rates = ratesObject as Dictionary<string, object>;
            if (rates == null || rates.Count == 0) return false;

            Dictionary<string, double> parsed = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, object> pair in rates)
            {
                double rate;
                if (double.TryParse(Convert.ToString(pair.Value, CultureInfo.InvariantCulture), NumberStyles.Float, CultureInfo.InvariantCulture, out rate) && rate > 0)
                    parsed[pair.Key] = rate;
            }
            parsed["EUR"] = 1.0;
            if (parsed.Count < 2) return false;

            lock (currencyRates1545)
            {
                currencyRates1545.Clear();
                foreach (KeyValuePair<string, double> pair in parsed) currencyRates1545[pair.Key] = pair.Value;
            }
            object updated;
            currencyUpdated1545 = root.TryGetValue("time_last_update_utc", out updated) ? Convert.ToString(updated) : "";
            currencySource1545 = source;
            return true;
        }

        private CurrencyChoice1545[] CurrencyChoices1545()
        {
            return new CurrencyChoice1545[]
            {
                new CurrencyChoice1545("EUR", "euros"),
                new CurrencyChoice1545("USD", "dólares estadounidenses"),
                new CurrencyChoice1545("GBP", "libras esterlinas"),
                new CurrencyChoice1545("ARS", "pesos argentinos"),
                new CurrencyChoice1545("VES", "bolívares venezolanos"),
                new CurrencyChoice1545("MXN", "pesos mexicanos"),
                new CurrencyChoice1545("COP", "pesos colombianos"),
                new CurrencyChoice1545("CLP", "pesos chilenos"),
                new CurrencyChoice1545("UYU", "pesos uruguayos"),
                new CurrencyChoice1545("PEN", "soles peruanos"),
                new CurrencyChoice1545("BRL", "reales brasileños"),
                new CurrencyChoice1545("DOP", "pesos dominicanos"),
                new CurrencyChoice1545("CUP", "pesos cubanos"),
                new CurrencyChoice1545("CRC", "colones costarricenses"),
                new CurrencyChoice1545("GTQ", "quetzales guatemaltecos"),
                new CurrencyChoice1545("HNL", "lempiras hondureñas"),
                new CurrencyChoice1545("NIO", "córdobas nicaragüenses"),
                new CurrencyChoice1545("PAB", "balboas panameños"),
                new CurrencyChoice1545("PYG", "guaraníes paraguayos"),
                new CurrencyChoice1545("BOB", "bolivianos"),
                new CurrencyChoice1545("CAD", "dólares canadienses"),
                new CurrencyChoice1545("AUD", "dólares australianos"),
                new CurrencyChoice1545("NZD", "dólares neozelandeses"),
                new CurrencyChoice1545("CHF", "francos suizos"),
                new CurrencyChoice1545("JPY", "yenes japoneses"),
                new CurrencyChoice1545("CNY", "yuanes chinos"),
                new CurrencyChoice1545("HKD", "dólares de Hong Kong"),
                new CurrencyChoice1545("INR", "rupias indias"),
                new CurrencyChoice1545("KRW", "wones surcoreanos"),
                new CurrencyChoice1545("TRY", "liras turcas"),
                new CurrencyChoice1545("RUB", "rublos rusos"),
                new CurrencyChoice1545("UAH", "grivnas ucranianas"),
                new CurrencyChoice1545("PLN", "eslotis polacos"),
                new CurrencyChoice1545("CZK", "coronas checas"),
                new CurrencyChoice1545("HUF", "florines húngaros"),
                new CurrencyChoice1545("RON", "leus rumanos"),
                new CurrencyChoice1545("SEK", "coronas suecas"),
                new CurrencyChoice1545("NOK", "coronas noruegas"),
                new CurrencyChoice1545("DKK", "coronas danesas"),
                new CurrencyChoice1545("ISK", "coronas islandesas"),
                new CurrencyChoice1545("ILS", "séqueles israelíes"),
                new CurrencyChoice1545("AED", "dírhams de Emiratos"),
                new CurrencyChoice1545("SAR", "riales saudíes"),
                new CurrencyChoice1545("ZAR", "rands sudafricanos"),
                new CurrencyChoice1545("MAD", "dírhams marroquíes"),
                new CurrencyChoice1545("EGP", "libras egipcias"),
                new CurrencyChoice1545("PHP", "pesos filipinos"),
                new CurrencyChoice1545("THB", "bahts tailandeses"),
                new CurrencyChoice1545("SGD", "dólares de Singapur"),
                new CurrencyChoice1545("IDR", "rupias indonesias"),
                new CurrencyChoice1545("MYR", "ringgits malasios")
            };
        }

        private void SetCurrencyChoice1545(ComboBox combo, string code)
        {
            if (combo == null) return;
            for (int i = 0; i < combo.Items.Count; i++)
            {
                if (ExtractCurrencyCode1545(Convert.ToString(combo.Items[i])) == code) { combo.SelectedIndex = i; return; }
            }
        }

        private string SelectedCurrencyCode1545(ComboBox combo)
        {
            return combo == null ? "" : ExtractCurrencyCode1545(combo.Text);
        }

        private string ExtractCurrencyCode1545(string label)
        {
            if (string.IsNullOrWhiteSpace(label)) return "";
            int open = label.LastIndexOf('(');
            int close = label.LastIndexOf(')');
            if (open >= 0 && close > open) return label.Substring(open + 1, close - open - 1).Trim().ToUpperInvariant();
            return label.Trim().ToUpperInvariant();
        }

        private string CurrencyNameForCode1545(string code)
        {
            CurrencyChoice1545[] choices = CurrencyChoices1545();
            for (int i = 0; i < choices.Length; i++) if (choices[i].Code == code) return choices[i].Name;
            return "unidades monetarias";
        }

        private void SwapCurrencyChoices1545()
        {
            if (currencyFrom1545 == null || currencyTo1545 == null) return;
            int old = currencyFrom1545.SelectedIndex;
            currencyFrom1545.SelectedIndex = currencyTo1545.SelectedIndex;
            currencyTo1545.SelectedIndex = old;
            PlayNamedSound("navegar");
            SetStatus("Monedas intercambiadas.");
        }

        private double ParseFlexibleNumber1545(string text)
        {
            string value = (text ?? "").Trim().Replace(" ", "");
            if (value.Length == 0) throw new Exception("Escribe una cantidad.");
            if (value.Contains(",") && value.Contains("."))
            {
                if (value.LastIndexOf(',') > value.LastIndexOf('.')) value = value.Replace(".", "").Replace(',', '.');
                else value = value.Replace(",", "");
            }
            else value = value.Replace(',', '.');
            double number;
            if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out number)) throw new Exception("No se reconoce la cantidad.");
            return number;
        }

        private string FormatNumber1545(double value)
        {
            return value.ToString("0.############", new CultureInfo("es-ES"));
        }

        private string FormatMoney1545(double value)
        {
            double absolute = Math.Abs(value);
            string format = absolute >= 1000 ? "N2" : absolute >= 1 ? "0.####" : "0.########";
            return value.ToString(format, new CultureInfo("es-ES"));
        }

        private string FormatRate1545(double value)
        {
            return value.ToString("0.########", new CultureInfo("es-ES"));
        }

        private string UnitShortName1545(string unit)
        {
            int open = unit.LastIndexOf('(');
            int close = unit.LastIndexOf(')');
            if (open >= 0 && close > open) return unit.Substring(open + 1, close - open - 1);
            return unit.ToLowerInvariant();
        }

        private void CopyConverterResult1545(TextBox box)
        {
            if (box == null || string.IsNullOrWhiteSpace(box.Text))
            {
                PlayNamedSound("aviso");
                AnnounceToScreenReader("Todavía no hay un resultado que copiar.");
                return;
            }
            Clipboard.SetText(box.Text);
            PlayCalculatorSound1554("calc_copiar");
            SetStatus("Resultado copiado al portapapeles.");
            AnnounceToScreenReader("Resultado copiado al portapapeles.");
        }
    }
}
