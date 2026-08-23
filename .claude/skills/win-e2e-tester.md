---
name: win-e2e-tester
description: Human-like End-to-End UI automation testing for Windows WPF/WinForms applications using pywinauto (UIA), visual inspection, mouse/keyboard simulation, and screenshot capture.
---

# Windows Desktop Human-Like E2E UI Automation Skill

This skill allows Claude Agent to perform native End-to-End (E2E) desktop UI testing on Windows applications (such as `.NET 8 WPF` or `WinForms`) in real-time, taking screenshots and interacting with windows, controls, textboxes, and shortcuts like a human tester.

---

## Capabilities & Workflows

### 1. Application Connection / Launch
Attach to a running instance or start the compiled executable:
```python
import sys, io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
import pywinauto
from pywinauto.application import Application

# Connect to running app or launch
try:
    app = Application(backend='uia').connect(path='MrCorrect.exe')
except Exception:
    app = Application(backend='uia').start(r'E:\prg\MrCorrect\Prg_UI\bin\Debug\net8.0-windows7.0\win-x64\MrCorrect.exe')
```

### 2. Inspect Active Windows & UI Controls
Tree inspection using `AutomationId`, `Name`, and `ControlType`:
```python
win = app.top_window()
print(f"Top Window: {win.window_text()} | Handle: {win.handle}")
# Print hierarchy to file for debugging
win.print_control_identifiers(depth=4)
```

### 3. Human Actions Simulation
- **Type Keys / Shortcuts:** `win.type_keys('^3')` (Ctrl+3), `win.type_keys('{F5}')`
- **Click Buttons:** `win.child_window(auto_id="Btn_Close", control_type="Button").click()`
- **Fill Inputs:** `win.child_window(auto_id="TXT_SEARCH_NAME", control_type="Edit").type_keys('آرمان', with_spaces=True)`

### 4. Visual Inspection & Proof
Capture real-time screenshots and read them back using the `Read` tool to visually verify UI appearance:
```python
img = win.capture_as_image()
img.save(r'C:/Users/Administrator/Desktop/ui_test_proof.png')
```

---

## Instructions for Claude Agent
1. When asked to "test like a human" or perform "E2E UI test":
   - Use Python with `pywinauto` (`backend='uia'`).
   - Interact with the app (open forms, type text, press shortcuts, click buttons).
   - Capture a screenshot to `C:/Users/Administrator/Desktop/`.
   - Call the `Read` tool on the screenshot image to visually verify layout, alignment, and responsiveness.
   - Report findings clearly to the user.
