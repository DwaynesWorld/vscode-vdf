"use strict";
import * as vscode from "vscode";
import * as path from "path";
import { execFile } from "child_process";
import { vdfOnDidSaveTextDocument } from "./providers/documentEventHandlers";
import { IndentVdfCommand } from "./providers/vscodeCommands";
import { VdfProxyFactory } from "./languageServer/vdfProxyFactory";
import { VdfDefinitionProvider } from "./providers/definitionProvider";

const VDF_LANGUAGE = "vdf";
const VDF = [
  { scheme: "file", language: VDF_LANGUAGE },
  { scheme: "untitled", language: VDF_LANGUAGE }
];

var vdfProxyFactory = null;

export function activate(context: vscode.ExtensionContext) {
  vscode.window.setStatusBarMessage('"vdfpack" is now active!', 3000);

  // Extension
  vdfProxyFactory = new VdfProxyFactory(context.extensionPath);
  context.subscriptions.push(vdfProxyFactory);

  //Register User commands
  context.subscriptions.push(IndentVdfCommand);

  //Register providers
  context.subscriptions.push(
    vscode.languages.registerDefinitionProvider(
      VDF,
      new VdfDefinitionProvider(vdfProxyFactory)
    )
  );

  //Handle Document Events
  vscode.workspace.onDidSaveTextDocument(
    vdfOnDidSaveTextDocument,
    null,
    context.subscriptions
  );
}

export function deactivate() {}
