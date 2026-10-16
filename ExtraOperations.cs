using System;
using System.Globalization;

namespace TodoAMano { public partial class MainForm {
    private static string ExtraOperationExample(string op) {
        if(op=="Quitar IVA del total") return "121;21";
        if(op=="Qué porcentaje es una parte del total") return "30;200";
        if(op=="Precio por unidad") return "12;8";
        if(op=="Media ponderada") return "10;2;20;3";
        return null;
    }
    private static string ExtraOperationHelp(string op) {
        if(op=="Quitar IVA del total") return "Escribe total con IVA;porcentaje. Ejemplo: 121;21 da base 100 e IVA 21. El porcentaje no puede ser negativo. Usa el porcentaje que corresponda a tu operación.";
        if(op=="Qué porcentaje es una parte del total") return "Escribe parte;total. Ejemplo: 30;200 da 15 %. El total debe ser mayor que cero.";
        if(op=="Precio por unidad") return "Escribe precio total;cantidad de unidades. Ejemplo: 12;8 da 1,5 por unidad. Las unidades deben ser mayores que cero.";
        if(op=="Media ponderada") return "Escribe pares valor;peso, separados por punto y coma. Ejemplo: 10;2;20;3 da 16. Los pesos no pueden ser negativos y al menos uno debe ser mayor que cero.";
        return null;
    }
    private string CalculateExtraOperation(string op,string input) {
        if(ExtraOperationExample(op)==null) return null;
        double[] values=ParseNumberList(input,2);
        if(op!="Media ponderada" && values.Length!=2) throw new ArgumentException("Esta operación requiere exactamente dos valores separados por punto y coma.");
        var es=SellerCulture2825;
        if(op=="Quitar IVA del total") {
            if(values[0]<0 || values[1]<0) throw new ArgumentException("El total y el porcentaje deben ser cero o positivos.");
            double amount=values[0]/(1+values[1]/100);
            return "Base sin IVA: "+amount.ToString("0.############",es)+". IVA incluido: "+(values[0]-amount).ToString("0.############",es)+".";
        }
        if(op=="Qué porcentaje es una parte del total") {
            if(values[1]<=0) throw new ArgumentException("El total debe ser mayor que cero.");
            return "Porcentaje: "+FiniteResult(values[0]/values[1]*100).ToString("0.############",es)+" %.";
        }
        if(op=="Precio por unidad") {
            if(values[0]<0 || values[1]<=0) throw new ArgumentException("El precio no puede ser negativo y las unidades deben ser mayores que cero.");
            return "Precio por unidad: "+FiniteResult(values[0]/values[1]).ToString("0.############",es)+".";
        }
        if(values.Length%2!=0) throw new ArgumentException("Cada valor necesita su peso: valor;peso;valor;peso.");
        // Scale weights first to avoid overflow when weights are very large.
        double maxWeight=0;
        for(int i=1;i<values.Length;i+=2) { if(values[i]<0)throw new ArgumentException("Los pesos no pueden ser negativos."); maxWeight=Math.Max(maxWeight,values[i]); }
        if(maxWeight==0) throw new ArgumentException("Al menos un peso debe ser mayor que cero.");
        double total=0;
        for(int i=1;i<values.Length;i+=2) total+=values[i]/maxWeight;
        double mean=0;
        for(int i=0;i<values.Length;i+=2) mean+=values[i]*(values[i+1]/maxWeight/total);
        return "Media ponderada: "+FiniteResult(mean).ToString("0.############",es)+".";
    }
    private static double FiniteResult(double value) {
        if(double.IsNaN(value) || double.IsInfinity(value)) throw new ArithmeticException("El resultado no es un número finito o supera el tamaño admitido.");
        return value;
    }
} }
