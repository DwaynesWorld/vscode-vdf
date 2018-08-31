import * as vscode from "vscode";
import { runVdfIndentation } from "../common/util";
import { VdfProxyFactory } from "../client/vdfProxyFactory";

export const formatVdfCommand = vscode.commands.registerCommand("extension.FormatVDF", () => {
  runVdfIndentation(vscode.window.activeTextEditor.document);
});

export const restartVdfServer = vscode.commands.registerCommand(
  "extension.RestartVdfServer",
  (vdfProxyFactory: VdfProxyFactory) => {
    console.log(vdfProxyFactory);
  }
);
