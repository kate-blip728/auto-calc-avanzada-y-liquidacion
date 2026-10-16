# Auto Calc 0.1.8 — mejora local de apertura

Basada en la versión 0.1.7 anunciada por update.json de https://github.com/kate-blip728/auto-calc-avanzada-y-liquidacion (consultado el 16 de octubre de 2026).

Se agrupa el cálculo de distribución de tablas y filas de botones durante la construcción inicial de la ventana. Al finalizar se reanuda de dentro hacia fuera. Los diálogos abiertos después mantienen su distribución normal.

Medición local: seis construcciones por versión, con carpetas de datos nuevas. Primera construcción: 1914 ms antes y 316 ms después. Mediana de las cinco siguientes: 1640 ms antes y 99 ms después (94 % menos). Esto mide construcción de la ventana, no todo el tiempo de reinicio de NVDA ni el comportamiento con historiales grandes.

Comprobaciones: compilación .NET Framework, pruebas existentes de cálculos, persistencia, nube simulada, cierre con Escape y lanzador de NVDA. Pendiente de probar instalación y navegación con NVDA real. No se ha cambiado el lanzador ni el sondeo de voz; la mejora se concentra en la apertura de la calculadora.

Instalación: abrir AutoCalc-0.1.8.nvda-addon con NVDA, aceptar la actualización y reiniciar NVDA. Abrir con NVDA+Alt+C. Usa la misma carpeta de datos y conserva los guardados existentes. Versión publicada en GitHub.
