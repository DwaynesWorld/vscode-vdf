import * as vscode from "vscode";
import * as path from "path";
import { execFile } from "child_process";

export function runVdfIndentation(document: vscode.TextDocument) {
  if (document !== null) {
    const extensionPath = vscode.extensions.getExtension("hcss.vdfpack")
      .extensionPath;
    let vdfSource = path.join(extensionPath, "resources\\VDFSource.exe");
    vscode.window.setStatusBarMessage(`Indenting: ${document.fileName}`, 3000);
    execFile(vdfSource, [document.fileName]);
  }
}
