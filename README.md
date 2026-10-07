# Auto Calc Avanzada y Liquidación

Complemento para NVDA que abre una calculadora independiente de Navaja Suiza. Versión **0.1.2 de prueba**. No necesitas instalar ni abrir Navaja.

## Instalar y abrir

Descarga [AutoCalc-0.1.2.nvda-addon](AutoCalc-0.1.2.nvda-addon), ábrelo con NVDA y reinicia el lector. Pulsa **NVDA+Alt+C** o usa **NVDA → Herramientas → Auto Calc Avanzada y Liquidación**. Puedes cambiar el atajo en Gestos de entrada.

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

El repositorio incluye todos los fuentes y sonidos. También puedes descargar [el código organizado en carpetas](AutoCalc_codigo_fuente_0.1.2.zip), extraerlo y compilarlo con los mismos scripts.

## Compilar

En Windows con .NET Framework 4.8, ejecuta `powershell -NoProfile -ExecutionPolicy Bypass -File build.ps1`. Ejecuta `test.ps1` para las pruebas. `python test_plugin.py` comprueba el lanzador con simulaciones. `package.py` usa Python 3 para producir el `.nvda-addon` sin bibliotecas adicionales.

## Procedencia

La calculadora, conversores, liquidaciones y sonidos proceden del paquete de Navaja Suiza de 4 de octubre de 2026 facilitado por su usuario. El lanzador de NVDA, la adaptación independiente y las reparaciones están en este repositorio. Se conserva la atribución a Navaja Suiza. Este repositorio público no fija una nueva licencia para los materiales originales.

## Actualizaciones desde GitHub

En la calculadora pulsa Alt y elige **Buscar actualizaciones**. Consulta update.json del repositorio público y compara la versión. Si hay una nueva, muestra sus novedades y pide descargarla. Verifica SHA-256 y la versión del paquete antes de abrir el instalador de NVDA. Cierra la calculadora antes de completar la instalación y reinicia NVDA después. Conserva tus datos y conexiones.

El teclado completo se muestra con la casilla **Mostrar teclado de números y signos** y recuerda tu elección. El menú **Conexiones** permite conectar Google Drive o Dropbox e importar la autorización existente de Navaja.

Para publicar una versión futura: cambia AutoCalcVersion en Updater.cs y version en manifest.ini, compila y empaqueta, sube el .nvda-addon a la raíz y actualiza update.json con version, file, sha256 y notes en el mismo commit. La descarga debe llamarse AutoCalc-VERSION.nvda-addon. SHA-256 comprueba integridad respecto al índice del mismo repositorio; no es una firma independiente. La comprobación es manual y no envía datos ni credenciales de liquidaciones.