import * as path from 'path';
import * as vscode from 'vscode';
import { execFile } from 'child_process';
import { formatVdfCommand, restartVdfServer } from './vscodeCommands';
import { getUI, UI } from './common/ui';
import { setGlobalContext } from './common/util';
import { setVdfProxyService, VdfProxyService } from './client/vdfProxyService';
import { VdfDefinitionProvider } from './providers/definitionProvider';
import { VdfDocumentSymbolProvider } from './providers/symbolProvider';
import { VdfHoverProvider } from './providers/hoverProvider';
import { vdfOnDidSaveTextDocument } from './handlers/documentEventHandlers';

"use strict";

const VDF_LANGUAGE = "vdf";
const VDF = [
  { scheme: "file", language: VDF_LANGUAGE },
  { scheme: "untitled", language: VDF_LANGUAGE }
];

let diagnosticCollection: vscode.DiagnosticCollection;

export function activate(context: vscode.ExtensionContext) {
  vscode.window.setStatusBarMessage("VDF Language Server is now active!", 2000);
  
  // Extension Activation
  const vdfProxyService = new VdfProxyService(context.extensionPath);
  setGlobalContext(context);
  setVdfProxyService(vdfProxyService);
  context.subscriptions.push(vdfProxyService);
  diagnosticCollection = vscode.languages.createDiagnosticCollection("vdf");

  //Register User commands
  context.subscriptions.push(formatVdfCommand);
  context.subscriptions.push(restartVdfServer);

  //Register providers
  context.subscriptions.push(vscode.languages.registerDefinitionProvider(VDF, new VdfDefinitionProvider(vdfProxyService)));
  context.subscriptions.push(vscode.languages.registerHoverProvider(VDF, new VdfHoverProvider(vdfProxyService)));
  context.subscriptions.push(vscode.languages.registerDocumentSymbolProvider(VDF, new VdfDocumentSymbolProvider(vdfProxyService)));

  //Handle Document Events
  //vscode.workspace.onDidSaveTextDocument(vdfOnDidSaveTextDocument, null, context.subscriptions);
  vscode.workspace.onDidSaveTextDocument(onChange, null, context.subscriptions);
}

function onChange(document: vscode.TextDocument) {
    console.log("changed");
    let diagnostics = [];
    let range = new vscode.Range(0,0,0,10);
    diagnostics.push(new vscode.Diagnostic(range, "Test", vscode.DiagnosticSeverity.Error))
    diagnosticCollection.clear();
    diagnosticCollection.set(document.uri, diagnostics);
    // let uri = document.uri;
    // check(uri.fsPath, goConfig).then(errors => {
    //   diagnosticCollection.clear();
    //   let diagnosticMap: Map<string, vscode.Diagnostic[]> = new Map();
    //   errors.forEach(error => {
    //     let canonicalFile = vscode.Uri.file(error.file).toString();
    //     let range = new vscode.Range(error.line - 1, error.startColumn, error.line - 1, error.endColumn);
    //     let diagnostics = diagnosticMap.get(canonicalFile);
    //     if (!diagnostics) { diagnostics = []; }
    //     diagnostics.push(new vscode.Diagnostic(range, error.msg, error.severity));
    //     diagnosticMap.set(canonicalFile, diagnostics);
    //   });
    //   diagnosticMap.forEach((diags, file) => {
    //     diagnosticCollection.set(vscode.Uri.parse(file), diags);
    //   });
    }


export function deactivate() {}
