import * as vscode from "vscode";
import * as path from "path";
import { execFile } from "child_process";

let globalContext: vscode.ExtensionContext;

export function setGlobalContext(context: vscode.ExtensionContext) {
  globalContext = context;
}

export function getGlobalContext(): vscode.ExtensionContext {
  return globalContext;
}

export function runVdfIndentation(document: vscode.TextDocument) {
  if (document !== null) {
    const extensionPath = getGlobalContext().extensionPath;
    let vdfSource = path.join(extensionPath, "resources\\VDFSource.exe");
    vscode.window.setStatusBarMessage(`Indenting: ${document.fileName}`, 3000);
    execFile(vdfSource, [document.fileName]);
  }
}
