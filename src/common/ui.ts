import * as vscode from "vscode";

let ui: UI;

export class UI {
	private indexStatusBarItem: vscode.StatusBarItem;

	constructor() {
		this.indexStatusBarItem = vscode.window.createStatusBarItem(
			vscode.StatusBarAlignment.Right,
			1
		);
		this.indexStatusBarItem.text = "";
		this.indexStatusBarItem.tooltip = "Updating Index...";
		this.indexStatusBarItem.color = "Red";
		this.ShowFlameIcon = true;
	}

	public get IsUpdatingIndex(): boolean {
		return this.indexStatusBarItem.text !== "";
	}

	public set IsUpdatingIndex(val: boolean) {
		this.indexStatusBarItem.text = val ? "$(flame)" : "";
		this.ShowFlameIcon = val;
	}

	private set ShowFlameIcon(show: boolean) {
		if (show && this.IsUpdatingIndex) {
			this.indexStatusBarItem.show();
		} else {
			this.indexStatusBarItem.hide();
		}
	}

	public activeDocumentChanged(): void {
		let activeEditor: vscode.TextEditor = vscode.window.activeTextEditor;
		let isVdf: boolean =
			activeEditor && activeEditor.document.languageId === "vdf";

		this.ShowFlameIcon = isVdf;
	}

	public dispose(): void {
		this.indexStatusBarItem.dispose();
	}
}

export function getUI(): UI {
	if (ui === undefined) {
		ui = new UI();
	}
	return ui;
}
