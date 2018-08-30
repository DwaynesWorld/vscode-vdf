import * as vscode from "vscode";
import { VdfProxyFactory } from "../client/vdfProxyFactory";
import { IDefinitionResult, ICommand, CommandType } from "../client/proxy";

export class VdfDefinitionProvider implements vscode.DefinitionProvider {
	constructor(private vdfProxyFactory: VdfProxyFactory) {}

	provideDefinition(
		document: vscode.TextDocument,
		position: vscode.Position,
		token: vscode.CancellationToken
	): Thenable<vscode.Definition> {
		if (document.lineAt(position.line).text.match(/^\s*\/\//)) {
			return Promise.resolve(null);
		}

		if (position.character <= 0) {
			return Promise.resolve(null);
		}

		const fileName = document.fileName;
		const range = document.getWordRangeAtPosition(position);
		const possibleWord = document.getText(range);
		const command = CommandType.Definitions;
		const lineIndex = position.line;
		const columnIndex = range.isEmpty ? position.character : range.end.character;

		const cmd: ICommand<IDefinitionResult> = {
			command,
			possibleWord,
			fileName,
			columnIndex,
			lineIndex
		};

		// TODO: Fix this, causes crash
		// if (document.isDirty) {
		//   cmd.source = document.getText();
		// }

		return this.vdfProxyFactory
			.getVdfProxyHandler<IDefinitionResult>(document.uri)
			.sendCommand(cmd, token)
			.then(result => {
				if (result) {
					const locations = result.definitions.map(def => {
						const uri = vscode.Uri.file(def.filePath);
						return new vscode.Location(
							uri,
							new vscode.Range(
								def.range.startLine,
								def.range.startColumn,
								def.range.endLine,
								def.range.endColumn
							)
						);
					});

					if (locations.length > 0) return Promise.resolve(locations);
				}

				return Promise.reject(null);
			});
	}
}
