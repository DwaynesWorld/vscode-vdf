"use strict";
import * as vscode from "vscode";
import * as path from "path";
import { execFile } from "child_process";

function runVdfIndentation(document: vscode.TextDocument) {
  if (document !== null) {
    const extensionPath = vscode.extensions.getExtension("hcss.vdfpack")
      .extensionPath;
    let vdfSource = path.join(
      extensionPath,
      "resources\\indentation\\VDFSource.exe"
    );
    vscode.window.setStatusBarMessage(`Indenting: ${document.fileName}`, 5000);
    execFile(vdfSource, [document.fileName]);
  }
}

function vdfOnDidSaveTextDocument(document: vscode.TextDocument) {
  if (document.isUntitled || document.languageId !== "vdf") return;
  runVdfIndentation(document);
}

export function activate(context: vscode.ExtensionContext) {
  vscode.window.setStatusBarMessage('"vdfpack" is now active!', 3000);
  context.subscriptions.push(
    vscode.commands.registerCommand("extension.IndentVDF", () => {
      runVdfIndentation(vscode.window.activeTextEditor.document);
    })
  );
  vscode.workspace.onDidSaveTextDocument(
    vdfOnDidSaveTextDocument,
    null,
    context.subscriptions
  );
}

export function deactivate() {}
