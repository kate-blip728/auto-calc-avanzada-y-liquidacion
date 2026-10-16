# Auto Calc 0.1.11 — favoritos, memorias y operaciones

## Novedades

- Menú Favoritos y memorias: guarda hasta 50 cálculos básicos o avanzados, incluidos modo y entrada. Los primeros nueve tienen atajos Ctrl+Mayúsculas+F1 a F9 dentro de la aplicación. Cargar no calcula automáticamente; revisa la entrada y pulsa Calcular. Incluye renombrado y eliminación.
- Memorias con nombre: hasta 50 valores persistentes, con recuperación en Expresión libre y eliminación. Puedes guardar el resultado básico sugerido o escribir un número. Sustituir un nombre existente requiere confirmación.
- Cálculos encadenados con Ans en Expresión libre: calcula 2+3 y luego Ans*4 para obtener 20. Continuar cálculo (Ans) prepara la entrada. Ans es el último resultado válido de Calcular de esta sesión; un error no lo borra. Se admite notación científica, por ejemplo 1e6. Las operaciones con Ans no muestran resultados antiguos del aviso de cálculos repetidos.
- Porcentajes y compras: Quitar IVA del total (121;21), Qué porcentaje es una parte del total (30;200), Precio por unidad (12;8).
- Estadística: Media ponderada (10;2;20;3). Los valores se dan por parejas valor;peso. Las operaciones nuevas incluyen ejemplo, miniayuda y validación.
- Las copias nuevas incluyen favoritos y memorias. Restaurar copias antiguas que no los incluyen conserva los actuales. Restaurar vacía Ans.

Incluye deshacer y rehacer del borrador, copias de seguridad y cierre antes de actualizar de las versiones anteriores. Versión publicada en GitHub.

## Pruebas

Compilación, verificación del instalador y pruebas del lanzador superadas. Pasan las comprobaciones existentes de cálculo, persistencia, copia/restauración, deshacer/rehacer y espera al cierre para instalar. También pasan pruebas de favoritos básicos y avanzados, atajos, persistencia de memorias, recuperación desde copias, compatibilidad con copias antiguas, reversión al fallar un guardado, Ans con negativos y notación científica, protección del último resultado ante errores y validación de las cuatro operaciones nuevas.

Pendiente de comprobar con NVDA real: navegación por los nuevos menús y diálogos y sus anuncios.

## Instalación

Abre AutoCalc-0.1.11.nvda-addon con NVDA y completa la instalación. Si tienes Auto Calc 0.1.8 o anterior abierta, ciérrala manualmente antes de instalar y reiniciar NVDA. Favoritos y memorias se guardan en favoritos-memorias.json dentro de la carpeta de datos habitual.
