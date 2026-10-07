import json
import os
from pathlib import Path
import subprocess

import globalPluginHandler
import globalVars
import gui
import ui
import wx
from scriptHandler import script
from logHandler import log


class GlobalPlugin(globalPluginHandler.GlobalPlugin):
    scriptCategory = "Auto Calc Avanzada y Liquidación"

    def __init__(self):
        super().__init__()
        self._menuItem = None
        self._timer = None
        self._process = None
        self._lastSpeech = None
        self._speechStamp = None
        if globalVars.appArgs.secure:
            return
        self._dataDir = Path(os.environ["APPDATA"]) / "AutoCalcAvanzadaLiquidacion"
        self._dataDir.mkdir(parents=True, exist_ok=True)
        self._speechFile = self._dataDir / "nvda_speech.json"
        try:
            self._lastSpeech = json.loads(self._speechFile.read_text(encoding="utf-8"))["id"]
        except (OSError, ValueError, KeyError):
            pass
        self._menu = gui.mainFrame.sysTrayIcon.toolsMenu
        self._menuItem = self._menu.Append(wx.ID_ANY, "Auto Calc Avanzada y Liquidación…")
        gui.mainFrame.sysTrayIcon.Bind(wx.EVT_MENU, self._open, self._menuItem)
        self._timer = wx.Timer(gui.mainFrame)
        gui.mainFrame.Bind(wx.EVT_TIMER, self._pollSpeech, self._timer)
        self._timer.Start(150)

    def _open(self, event=None):
        if globalVars.appArgs.secure:
            return
        executable = Path(__file__).resolve().parent / "autoCalc" / "AutoCalc.exe"
        try:
            self._process = subprocess.Popen(
                [str(executable), "--data-dir", str(self._dataDir)],
                cwd=str(executable.parent),
                creationflags=subprocess.CREATE_NO_WINDOW,
            )
        except OSError:
            log.exception("No se pudo abrir Auto Calc")
            ui.message("No se pudo abrir Auto Calc. Reinstala el complemento.")

    @script(
        description="Abrir Auto Calc Avanzada y Liquidación",
        gesture="kb:NVDA+alt+c",
    )
    def script_openCalculator(self, gesture):
        self._open()

    def _pollSpeech(self, event):
        try:
            stamp = self._speechFile.stat().st_mtime_ns
            if stamp == self._speechStamp:
                return
            message = json.loads(self._speechFile.read_text(encoding="utf-8"))
            self._speechStamp = stamp
            if message.get("id") != self._lastSpeech:
                self._lastSpeech = message.get("id")
                if message.get("text"):
                    ui.message(message["text"])
        except (OSError, ValueError):
            pass

    def terminate(self):
        if self._timer:
            self._timer.Stop()
            gui.mainFrame.Unbind(wx.EVT_TIMER, handler=self._pollSpeech, source=self._timer)
        if self._menuItem:
            gui.mainFrame.sysTrayIcon.Unbind(wx.EVT_MENU, handler=self._open, source=self._menuItem)
            self._menu.Remove(self._menuItem)
        super().terminate()
