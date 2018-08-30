import * as vscode from "vscode";
import { runVdfIndentation } from "../common/util";

export const IndentVdfCommand = vscode.commands.registerCommand("extension.IndentVDF", () => {
  runVdfIndentation(vscode.window.activeTextEditor.document);
});
