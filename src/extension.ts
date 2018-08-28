"use strict";
import * as vscode from "vscode";
import * as path from "path";
import { execFile } from "child_process";
import { UI, getUI } from "./common/ui";
import { vdfOnDidSaveTextDocument } from "./providers/documentEventHandlers";
import { IndentVdfCommand } from "./providers/vscodeCommands";
import { VdfProxyFactory } from "./client/vdfProxyFactory";
import { VdfDefinitionProvider } from "./providers/definitionProvider";
import { setGlobalContext } from "./common/util";

const VDF_LANGUAGE = "vdf";
const VDF = [
	{ scheme: "file", language: VDF_LANGUAGE },
	{ scheme: "untitled", language: VDF_LANGUAGE }
];

let vdfProxyFactory: VdfProxyFactory;
let ui: UI;

export function activate(context: vscode.ExtensionContext) {
	setGlobalContext(context);

	vscode.window.setStatusBarMessage(
		"VDF Language Server is now active!",
		2000
	);

	// Create UI
	ui = getUI();

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
