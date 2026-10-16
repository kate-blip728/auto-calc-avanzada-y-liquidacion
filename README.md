# Auto Calc Avanzada y Liquidación

Complemento para NVDA que abre una calculadora independiente de Navaja Suiza. Versión **0.1.11 de prueba**. No necesitas instalar ni abrir Navaja.

## Instalar y abrir

Descarga [AutoCalc-0.1.11.nvda-addon](AutoCalc-0.1.11.nvda-addon), ábrelo con NVDA y reinicia el lector. Pulsa **NVDA+Alt+C** o usa **NVDA → Herramientas → Auto Calc Avanzada y Liquidación**. Puedes cambiar el atajo en Gestos de entrada.

El complemento carga un pequeño lanzador y abre la calculadora sólo cuando la pides. La calculadora usa .NET Framework de Windows y sus controles estándar accesibles. No lleva las funciones de radio, correo, juegos en red ni otros módulos de Navaja.

## Funciones

- Calculadora básica, teclado de botones, porcentajes, IVA, descuento, aumento, regla de tres, medias, raíces y potencias.
- Operaciones avanzadas originales, memoria y curiosidades del resultado.
- Conversores de unidades, divisas y criptomonedas; las cotizaciones requieren Internet y dependen de sus proveedores.
- Fechas, diferencias entre fechas y dados.
- Liquidaciones: monedas, billetes, blísteres y envases; añadir, restar, corregir, copiar y guardar.
- Historial por fechas, filtros, desglose, totales del período e historial de correcciones.
- Inventario y libros de rascas del módulo móvil.
- Sonidos originales de la calculadora, que puedes desactivar desde el menú.
- Exportación de la calculadora independiente en ZIP, sin liquidaciones ni credenciales personales.

Los datos propios se guardan en `%APPDATA%\AutoCalcAvanzadaLiquidacion`. No se comparte directamente la carpeta de datos de Navaja: la combinación se realiza mediante el archivo de nube.

## Google Drive y Dropbox

Abre **Inventario y nube**, entra en la pestaña de sincronización, conecta el proveedor y pulsa **Sincronizar ahora**. La conexión a una cuenta y la confirmación de envío son estados diferentes. En **Liquidaciones guardadas** encontrarás el estado general y el de cada liquidación. Una corrección posterior queda pendiente hasta el próximo envío confirmado.

Si ya tienes la cuenta conectada en Navaja, pulsa **Importar conexión de Navaja** en el diálogo del proveedor y elige su carpeta. Esto reutiliza localmente la autorización del mismo usuario de Windows; no copia tus liquidaciones ni publica credenciales. Después pulsa **Sincronizar ahora** para comprobarla.

Para una conexión nueva, configura el cliente OAuth: identificador/app key y, para Google, el secreto del cliente de escritorio. No incluye credenciales de Navaja ni de ninguna cuenta. Los tokens y secretos se protegen con Windows para el usuario actual.

**Para compartir con Navaja y el móvil, debes usar la misma aplicación OAuth y la misma cuenta.** Google Drive guarda `navaja-liquidacion-sync.json` en `appDataFolder`, privado para la aplicación; otro proyecto OAuth verá una carpeta diferente. En Dropbox debes usar la misma aplicación y modalidad de acceso (carpeta de aplicación o Dropbox completo).

El cliente Google debe ser de escritorio con permiso `drive.appdata`. Dropbox debe permitir PKCE, acceso sin conexión y los permisos `files.content.read` y `files.content.write`. Dropbox devuelve un código que se pega en el diálogo. Google regresa a una dirección local temporal del equipo.

La sincronización descarga, combina cambios por identificador y fecha y sube el resultado. Incluye las eliminaciones para evitar que reaparezcan registros antiguos. Si dos dispositivos envían exactamente a la vez, todavía puede haber una carrera entre descarga y subida: vuelve a sincronizar después. No se ha probado esa concurrencia contra las cuentas reales.

Referencias: [datos de aplicación de Google Drive](https://developers.google.com/workspace/drive/api/guides/appdata), [OAuth de Dropbox](https://docs.dropboxapi.com/dropbox-api/docs/oauth).

## Estado de las pruebas

Se ha compilado la calculadora y probado su construcción, cálculos básicos, conversión de temperatura, conservación de metadatos, confirmaciones por versión y proveedor, combinación de registros y eliminaciones, protección de credenciales y persistencia del resultado de sincronización. El lanzador se valida con simulaciones de las interfaces de NVDA.

**Pendiente de validar con un usuario:** instalación y navegación con NVDA real, anuncios y sonidos durante el uso, autorización OAuth y sincronización contra Google Drive/Dropbox. La compatibilidad de manifiesto está declarada para NVDA 2024.1–2026.1; no equivale a una prueba realizada en esas versiones.

## Código fuente

El repositorio incluye todos los fuentes y sonidos. También puedes descargar [el código organizado en carpetas](AutoCalc_codigo_fuente_0.1.11.zip), extraerlo y compilarlo con los mismos scripts.

## Compilar

En Windows con .NET Framework 4.8, ejecuta `powershell -NoProfile -ExecutionPolicy Bypass -File build.ps1`. Ejecuta `test.ps1` para las pruebas. `python test_plugin.py` comprueba el lanzador con simulaciones. `package.py` usa Python 3 para producir el `.nvda-addon` sin bibliotecas adicionales.

## Procedencia

La calculadora, conversores, liquidaciones y sonidos proceden del paquete de Navaja Suiza de 4 de octubre de 2026 facilitado por su usuario. El lanzador de NVDA, la adaptación independiente y las reparaciones están en este repositorio. Se conserva la atribución a Navaja Suiza. Este repositorio público no fija una nueva licencia para los materiales originales.

## Actualizaciones desde GitHub

En la calculadora pulsa Alt y elige **Buscar actualizaciones**. Consulta update.json del repositorio público y compara la versión. Si hay una nueva, muestra sus novedades y pide descargarla. Verifica SHA-256 y la versión del paquete antes de abrir el instalador de NVDA. Cierra la calculadora antes de completar la instalación y reinicia NVDA después. Conserva tus datos y conexiones.

El teclado completo se muestra con la casilla **Mostrar teclado de números y signos** y recuerda tu elección. El menú **Conexiones** permite conectar Google Drive o Dropbox e importar la autorización existente de Navaja.

Para publicar una versión futura: cambia AutoCalcVersion en Updater.cs y version en manifest.ini, compila y empaqueta, sube el .nvda-addon a la raíz y actualiza update.json con version, file, sha256 y notes en el mismo commit. La descarga debe llamarse AutoCalc-VERSION.nvda-addon. SHA-256 comprueba integridad respecto al índice del mismo repositorio; no es una firma independiente. La comprobación es manual y no envía datos ni credenciales de liquidaciones.
El aviso de inicio se envía al lector sin cuadro de edición. Desmarca **Mostrar mensajes de ayuda** en el menú para ocultar las ayudas; se recuerda al reabrir. Resultados y estados de sincronización conservan sus controles.
## Cerrar con Escape
Escape cierra la calculadora y la ventana de inventario y nube. En los formularios de conexión y autorización cancela sin pulsar Conectar ni confirmar los datos pendientes. Los resultados están dentro de la ventana principal. Los selectores de archivo y mensajes de Windows conservan sus teclas habituales. Al cerrar se conservan los guardados automáticos existentes; Escape no deshace una operación ya confirmada. Las versiones publicadas se ofrecen desde Buscar actualizaciones.
## Operaciones ya realizadas

En Calcular, la opción Avisar automáticamente de operaciones ya realizadas está activada por defecto. Al escribir una operación guardada y dejar de teclear durante 0,9 segundos, suena el aviso de historial y NVDA anuncia el resultado sin pulsar Calcular. El resultado también queda disponible para copiar. Se reconoce la misma expresión y el mismo tipo de operación; los espacios alrededor de signos no importan. Usa las últimas 300 operaciones del historial local, también después de cerrar y abrir. Al desactivar la opción se guarda tu preferencia. El menú Sonidos permite silenciar el aviso.


Al cambiar de pestaña se utiliza el anuncio nativo de NVDA; se elimina el aviso adicional que repetía Calculadora y el nombre de la pestaña.


## Miniayuda de operaciones en 0.1.7

Al elegir una operacion se muestra su miniayuda en un cuadro accesible y se anuncia al lector. Explica los signos y el orden de los datos. Potencia: base;exponente; por ejemplo 2;8 da 256. Las operaciones avanzadas muestran su nombre y un ejemplo.

## Apertura más rápida en 0.1.8

Se agrupa el cálculo de distribución de los controles al abrir la ventana. En una prueba local la mediana de construcción pasó de 1640 ms a 99 ms. No mide el reinicio completo de NVDA. Pruebas automatizadas superadas; pendiente de validación con NVDA real. [Detalles de la mejora](CAMBIOS-0.1.8.md).

## Actualización con cierre automático en 0.1.9

Tras descargar y verificar una actualización, la calculadora solicita su cierre normal, ejecuta los guardados y un auxiliar independiente espera a que el proceso termine antes de abrir el instalador de NVDA. Si tarda más de 60 segundos, no instala y guarda un diagnóstico junto al paquete. Si cancelas la instalación de NVDA, puedes volver a abrir la versión actual.

Al pasar desde 0.1.8 u otra versión anterior a 0.1.9, cierra manualmente Auto Calc antes de aceptar la instalación y reiniciar NVDA. El cierre automático se aplica a las actualizaciones iniciadas desde 0.1.9; no modifica el actualizador que ya está ejecutándose en versiones anteriores.

## Deshacer y copias de seguridad en 0.1.10

La pestaña Liquidación vendedor tiene Deshacer cambio (Ctrl+Alt+Z) y Rehacer cambio (Ctrl+Alt+Y). Conserva hasta 50 cambios del borrador durante la sesión: añadir, editar, eliminar, modificar cantidades, crear una nueva liquidación y cargar otra como borrador o para corregir. Las fechas y notas del estado previo se recuperan junto con las filas. No deshace por separado cada letra escrita ni cada cambio de fecha. Al guardar o eliminar un registro histórico se vacían estas acciones; las correcciones guardadas mantienen sus revisiones originales.

El menú Copias de seguridad permite crear, exportar y restaurar. Se hace una copia al abrir y cada 15 minutos si los datos han cambiado; las últimas 20 se conservan en la carpeta copias-seguridad de los datos de Auto Calc. La copia incluye borrador, fecha, nota, liquidaciones con revisiones e historial de cálculos. Excluye inventario, ajustes y conexiones. Los ZIP exportados a otra carpeta no se borran por rotación.

Restaurar requiere confirmación y crea antes una copia del estado actual; rechaza archivos incompatibles antes de cambiar los datos. La restauración sustituye los datos locales y desactiva la sincronización automática. No modifica la nube ni recupera borrados ya sincronizados. Al volver a sincronizar, las reglas existentes de combinación de datos y eliminaciones siguen aplicándose. Revisa los registros antes de hacerlo.

La fecha y la nota del borrador se conservan al cerrar normalmente. Deshacer y rehacer se reinician al cerrar. Se mantiene el cierre seguro para actualizar de 0.1.9.

## Favoritos, memorias y operaciones en 0.1.11

En el menú Favoritos y memorias puedes guardar el cálculo actual de Calcular o Práctica y científica. Guarda el modo, los datos y un nombre. Cargarlo no lo calcula automáticamente: revisa la entrada y pulsa Calcular. Se admiten 50 favoritos; los primeros nueve se abren con Ctrl+Mayúsculas+F1 a F9 mientras la aplicación está abierta. Gestionar favoritos permite renombrarlos y eliminarlos.

Memorias con nombre permite guardar hasta 50 valores, con coma o punto decimal, recuperarlos en Expresión libre y eliminarlos. Inicialmente sugiere el resultado numérico básico actual, que puedes modificar. Guardar con un nombre ya existente pide confirmar su sustitución. Los nombres no distinguen mayúsculas. Se guardan entre sesiones en favoritos-memorias.json.

En Expresión libre, Ans reutiliza el último resultado válido de Calcular durante esta sesión. Por ejemplo, calcula 2+3 y después Ans*4 para obtener 20. El botón Continuar cálculo (Ans) prepara el campo. Un error no borra el último resultado válido. También puedes escribir notación científica, por ejemplo 1e6. No se usan avisos del historial para expresiones con Ans porque su resultado depende del contexto.

Las copias de seguridad nuevas incluyen favoritos y memorias. Restaurar una copia antigua que no los incluya conserva los actuales. Ans es temporal y se vacía al restaurar.

Práctica y científica añade Quitar IVA del total (121;21), Qué porcentaje es una parte del total (30;200) y Precio por unidad (12;8) a Porcentajes y compras. Estadística añade Media ponderada (10;2;20;3). Cada operación explica el orden de entrada en su miniayuda. Se conserva el funcionamiento de las operaciones existentes.
