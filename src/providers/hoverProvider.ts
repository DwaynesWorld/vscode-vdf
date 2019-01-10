import * as vscode from "vscode";
import { CommandType, ICommand, IHoverResult } from "../client/proxy";
import { VdfProxyService } from "../client/vdfProxyService";

export class VdfHoverProvider implements vscode.HoverProvider {
	constructor(private vdfProxyService: VdfProxyService) {}

	provideHover(
		document: vscode.TextDocument,
		position: vscode.Position,
		token: vscode.CancellationToken
	): vscode.ProviderResult<vscode.Hover> {
		// Comment line
		if (document.lineAt(position.line).text.match(/^\s*\/\//)) {
			return Promise.resolve(null);
		}

		// silly!
		if (position.character <= 0) {
			return Promise.resolve(null);
		}

		const fileName = document.fileName;
		const range = document.getWordRangeAtPosition(position);
		const possibleWord = document.getText(range);
		const command = CommandType.Hover;
		const lineIndex = position.line;
		const columnIndex = range.isEmpty
			? position.character
			: range.end.character;

		const cmd: ICommand<IHoverResult> = {
			command,
			possibleWord,
			fileName,
			columnIndex,
			lineIndex
		};

		return this.vdfProxyService
			.getVdfProxyHandler<IHoverResult>(document.uri)
			.sendCommand(cmd, token)
			.then(result => {
				console.log(result);

				if (result) {
					var item: vscode.Hover = new vscode.Hover(
						new vscode.MarkdownString(result.main)
					);

					item.contents.push({ language: "vdf", value: result.contents });

					if (result.hoverMetadata && result.hoverMetadata.trim() != "")
						item.contents.push(new vscode.MarkdownString(result.hoverMetadata));

					return Promise.resolve(item);
				}

				return Promise.reject(null);
			});
	}
}
