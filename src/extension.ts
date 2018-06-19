"use strict";
// The module 'vscode' contains the VS Code extensibility API
// Import the module and reference it with the alias vscode in your code below
import * as vscode from "vscode";
const { execFile } = require("child_process");

function runVdfIndentation(document: vscode.TextDocument) {
	if (document !== null) {
		let vdfSource =
			vscode.extensions.getExtension("hcss.vdfpack").extensionPath +
			"\\indentation\\VDFSource.exe";

		vscode.window.setStatusBarMessage(
			`Indenting: ${document.fileName}`,
			5000
		);
		execFile(vdfSource, [document.fileName]);
	}
}

let indentVDFCommand = vscode.commands.registerCommand(
	"extension.IndentVDF",
	() => {
		runVdfIndentation(vscode.window.activeTextEditor.document);
	}
);

function vdfOnDidSaveTextDocument(document: vscode.TextDocument) {
	if (document.isUntitled || document.languageId !== "vdf") return;
	runVdfIndentation(document);
}

export function activate(context: vscode.ExtensionContext) {
	vscode.window.setStatusBarMessage('"vdfpack" is now active!', 3000);
	context.subscriptions.push(indentVDFCommand);
	vscode.workspace.onDidSaveTextDocument(
		vdfOnDidSaveTextDocument,
		null,
		context.subscriptions
	);
}

export function deactivate() {}
