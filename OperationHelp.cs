using System;
using System.Windows.Forms;
namespace TodoAMano { public partial class MainForm {
    private TextBox basicOperationHelp;
    private TextBox advancedOperationHelp;
    internal static string BasicOperationHelp(string mode) {
        string help;
        if(mode.StartsWith("Potencia")) help="Eleva una base a un exponente. Escribe base;exponente, separados por punto y coma. Ejemplo: 2;8 calcula 2 elevado a 8 y da 256. En este modo no escribas el signo ^.";
        else if(mode.StartsWith("Raíz")) help="Calcula la raíz cuadrada. Escribe un número no negativo, por ejemplo 81; el resultado es 9.";
        else if(mode.StartsWith("Porcentaje")) help="Calcula un porcentaje de una cantidad. Escribe porcentaje;cantidad. Ejemplo: 15;200 da 30.";
        else if(mode.StartsWith("Descuento")) help="Resta un porcentaje al precio. Escribe precio;porcentaje. Ejemplo: 100;20 da 80.";
        else if(mode.StartsWith("Aumento")) help="Añade un porcentaje a la cantidad. Escribe cantidad;porcentaje. Ejemplo: 100;10 da 110.";
        else if(mode.StartsWith("IVA")) help="Añade el IVA a una base. Escribe base;porcentaje. Ejemplo: 100;21 da 121.";
        else if(mode.StartsWith("Media")) help="Suma los números y divide entre cuántos hay. Sepáralos por punto y coma. Ejemplo: 10;20;30 da 20.";
        else if(mode.StartsWith("Regla")) help="Resuelve A es a B como C es a X: X es B por C dividido entre A. Escribe A;B;C. Ejemplo: 2;10;5 da 25. A no puede ser cero.";
        else help="Escribe una expresión: + para sumar, - para restar, * asterisco para multiplicar y / barra para dividir. Usa paréntesis para agrupar. Ejemplo: (147 / 3) + 2 da 51. Para potencias elige el modo Potencia.";
        return "Operación: "+mode+". "+help+" Puedes usar coma o punto decimal. Pulsa Calcular o Enter en el campo de datos.";
    }
    private void UpdateBasicOperationHelp() {
        if(basicOperationHelp==null || calcTemplate==null)return;
        basicOperationHelp.Text=BasicOperationHelp(calcTemplate.Text);
        if(calcTemplate.Focused)AnnounceToScreenReader(basicOperationHelp.Text);
    }
    private void UpdateAdvancedOperationHelp() {
        if(advancedOperationHelp==null || advancedOperation2824==null || advancedInput2824==null)return;
        advancedOperationHelp.Text="Operación: "+advancedOperation2824.Text+". Ejemplo de datos: "+advancedInput2824.Text+". Los valores se separan con punto y coma; para decimales usa coma o punto. Consulta la ayuda inferior para el orden de los datos. Pulsa Calcular para realizar esta operación.";
        if(advancedOperation2824.Focused)AnnounceToScreenReader(advancedOperationHelp.Text);
    }
}}
