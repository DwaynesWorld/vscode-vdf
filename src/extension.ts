import * as path from 'path';
import * as vscode from 'vscode';
import { execFile } from 'child_process';
import { formatVdfCommand, restartVdfServer } from './vscodeCommands';
import { getUI, UI } from './common/ui';
import { setGlobalContext } from './common/util';
import { setVdfProxyService, VdfProxyService } from './client/vdfProxyFactory';
import { VdfDefinitionProvider } from './providers/definitionProvider';
import { VdfDocumentSymbolProvider } from './providers/symbolProvider';
import { vdfOnDidSaveTextDocument } from './handlers/documentEventHandlers';

"use strict";

const VDF_LANGUAGE = "vdf";
const VDF = [
  { scheme: "file", language: VDF_LANGUAGE },
  { scheme: "untitled", language: VDF_LANGUAGE }
];

export function activate(context: vscode.ExtensionContext) {
  vscode.window.setStatusBarMessage("VDF Language Server is now active!", 2000);
  
  // Extension Activation
  const vdfProxyService = new VdfProxyService(context.extensionPath);
  setGlobalContext(context);
  setVdfProxyService(vdfProxyService);
  context.subscriptions.push(vdfProxyService);

  //Register User commands
  context.subscriptions.push(formatVdfCommand);
  context.subscriptions.push(restartVdfServer);

  //Register providers
  context.subscriptions.push(vscode.languages.registerDefinitionProvider(VDF, new VdfDefinitionProvider(vdfProxyService)));
  context.subscriptions.push(vscode.languages.registerDocumentSymbolProvider(VDF, new VdfDocumentSymbolProvider(vdfProxyService)));

  //Handle Document Events
  vscode.workspace.onDidSaveTextDocument(vdfOnDidSaveTextDocument, null, context.subscriptions);
}

export function deactivate() {}
