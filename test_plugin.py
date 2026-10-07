import importlib.util
import json
import os
from pathlib import Path
import sys
import tempfile
import types
from unittest.mock import Mock, patch

with tempfile.TemporaryDirectory(dir=Path(__file__).resolve().parent, prefix='test-data-plugin-') as directory:
    assert Path(__file__).resolve().parent in Path(directory).resolve().parents
    os.environ['APPDATA'] = directory
    frame, item, timer = Mock(), Mock(), Mock()
    menu = frame.sysTrayIcon.toolsMenu
    menu.Append.return_value = item
    messages = []
    modules = {
        'globalPluginHandler': types.SimpleNamespace(GlobalPlugin=type('BasePlugin', (), {'terminate': lambda self: None})),
        'globalVars': types.SimpleNamespace(appArgs=types.SimpleNamespace(secure=False)),
        'gui': types.SimpleNamespace(mainFrame=frame),
        'ui': types.SimpleNamespace(message=messages.append),
        'wx': types.SimpleNamespace(ID_ANY=-1, EVT_MENU=1, EVT_TIMER=2, Timer=lambda owner: timer),
        'scriptHandler': types.SimpleNamespace(script=lambda **kwargs: lambda function: function),
        'logHandler': types.SimpleNamespace(log=Mock()),
    }
    with patch.dict(sys.modules, modules):
        pluginPath = Path(__file__).parent / 'addon/globalPlugins/autoCalc.py'
        if not pluginPath.exists():
            pluginPath = Path(__file__).parent / 'autoCalc.py'
        spec = importlib.util.spec_from_file_location('autoCalcPluginTest', pluginPath)
        module = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(module)
        plugin = module.GlobalPlugin()
        assert timer.Start.call_args.args == (150,)
        print('OK: menÃº y temporizador del lanzador')
        with patch.object(module.subprocess, 'Popen') as launch, patch.object(module.subprocess, 'CREATE_NO_WINDOW', 0x08000000, create=True):
            plugin.script_openCalculator(None)
            args = launch.call_args.args[0]
            assert args[0].endswith('AutoCalc.exe') and args[1] == '--data-dir'
            assert args[2] == str(Path(directory) / 'AutoCalcAvanzadaLiquidacion')
        print('OK: lanzamiento con ruta propia de datos, sin shell')
        plugin._speechFile.write_text(json.dumps({'id': 'one', 'text': 'Cambios enviados a Dropbox.'}), encoding='utf-8')
        plugin._pollSpeech(None)
        plugin._pollSpeech(None)
        assert messages == ['Cambios enviados a Dropbox.']
        print('OK: aviso comunicado una vez a NVDA')
        plugin.terminate()
        timer.Stop.assert_called_once()
        menu.Remove.assert_called_once_with(item)
        print('OK: limpieza de menÃº y temporizador')
        modules['globalVars'].appArgs.secure = True
        secure = module.GlobalPlugin()
        with patch.object(module.subprocess, 'Popen') as launch:
            secure.script_openCalculator(None)
            launch.assert_not_called()
        print('OK: no se abre en escritorio seguro')
